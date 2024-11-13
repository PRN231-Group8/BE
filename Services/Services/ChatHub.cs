using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Collections.Concurrent;

namespace PRN231.ExploreNow.Services.Services
{
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        private readonly IChatRoomService _chatRoomService;
        private readonly IChatMessageService _chatMessageService;

        public ChatHub(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IChatRoomService chatRoomService,
            IChatMessageService chatMessageService)
        {
            _userManager = userManager;
            _chatRoomService = chatRoomService;
            _httpContextAccessor = httpContextAccessor;
            _chatMessageService = chatMessageService;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user != null)
            {
                UserConnections.AddOrUpdate(user.Id, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);

                if (await _userManager.IsInRoleAsync(user, StaticUserRoles.MODERATOR))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Moderators");
                    var pendingChats = await _chatRoomService.GetPendingChatsAsync();
                    await Clients.Caller.SendAsync("PendingChats", pendingChats);
                }
                else
                {
                    // Check for active customer chat
                    var activeChat = await _chatRoomService.GetActiveChatForCustomerAsync(user.Id);
                    if (activeChat != null)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, activeChat.Id.ToString());
                        await Clients.Caller.SendAsync("ReconnectToChat", activeChat);
                    }
                }

                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            var user = await GetAuthenticatedUserAsync();
            if (user != null)
            {
                UserConnections.TryRemove(user.Id, out _);

                if (await _userManager.IsInRoleAsync(user, StaticUserRoles.MODERATOR))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Moderators");
                }
                else
                {
                    var activeChat = await _chatRoomService.GetActiveChatForCustomerAsync(user.Id);
                    if (activeChat != null)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, activeChat.Id.ToString());
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task InitiateChat(CreateChatRoomRequest request)
        {
            try
            {
                var user = await GetAuthenticatedUserAsync();

                // Check if user is a moderator - moderators shouldn't initiate chats
                if (await _userManager.IsInRoleAsync(user, StaticUserRoles.MODERATOR))
                {
                    await Clients.Caller.SendAsync("Error",
                        "Moderators cannot initiate chats. Please wait for customer requests.");
                    return;
                }

                // Only check for active chats if the user is a customer
                var activeChat = await _chatRoomService.GetActiveChatForCustomerAsync(user.Id);
                if (activeChat != null)
                {
                    if (activeChat.Status == ChatRoomStatus.WAITING)
                    {
                        await Clients.Caller.SendAsync("Error",
                            "You already have a chat waiting for staff. Please wait.");
                    }
                    else if (activeChat.Status == ChatRoomStatus.ACTIVE)
                    {
                        await Clients.Caller.SendAsync("Error",
                            "You already have an active chat with our staff.");
                    }
                    return;
                }

                var chatRoom = await _chatRoomService.CreateChatRoomAsync(request);

                await Clients.Group("Moderators").SendAsync("NewChatRequest", new
                {
                    ChatRoomId = chatRoom.Id,
                    Subject = chatRoom.Subject,
                    CustomerName = user.UserName,
                    Timestamp = DateTime.UtcNow
                });

                await Clients.Caller.SendAsync("ChatInitiated", new
                {
                    chatRoom.Id,
                    chatRoom.Status,
                    Message = "Please wait while we connect you with our staff..."
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task AcceptChat(Guid chatRoomId)
        {
            try
            {
                var user = await GetAuthenticatedUserAsync();

                // Verify staff role
                if (!await _userManager.IsInRoleAsync(user, StaticUserRoles.MODERATOR))
                {
                    throw new UnauthorizedAccessException("Only staff can accept chats");
                }

                var success = await _chatRoomService.AcceptChatRoomAsync(chatRoomId);
                if (success)
                {
                    var chatRoom = await _chatRoomService.GetChatRoomDetailsAsync(chatRoomId);
                    if (UserConnections.TryGetValue(chatRoom.CustomerId, out string customerConnection))
                    {
                        await Clients.Client(customerConnection).SendAsync("ModeratorJoined", new
                        {
                            RoomId = chatRoom.Id,
                            ModeratorName = user.UserName,
                            Status = ChatRoomStatus.ACTIVE,
                            Message = $"Staff {user.UserName} has joined the chat"
                        });
                    }
                    await Clients.Caller.SendAsync("ChatAccepted", new
                    {
                        chatRoom.Id,
                        chatRoom.CustomerId,
                        chatRoom.ModeratorId,
                        Status = ChatRoomStatus.ACTIVE,
                        CustomerName = chatRoom.CustomerName,
                        Subject = chatRoom.Subject
                    });

                    // Update pending chats for all moderators
                    var pendingChats = await _chatRoomService.GetPendingChatsAsync();
                    await Clients.Group("Moderators").SendAsync("PendingChats", pendingChats);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task SendMessage(SendMessageRequest request)
        {
            try
            {
                var chatRoom = await _chatRoomService.GetChatRoomDetailsAsync(request.ChatRoomId);
                if (chatRoom == null || chatRoom.Status != ChatRoomStatus.ACTIVE)
                    throw new InvalidOperationException("Chat room is not active");

                var message = await _chatMessageService.SendMessageAsync(request);
                var sender = await _userManager.FindByIdAsync(message.SenderId);

                var response = new ChatMessageResponse
                {
                    ChatRoomId = request.ChatRoomId,
                    Content = request.Content,
                    SenderId = message.SenderId,
                    SenderName = sender.UserName,
                    Timestamp = message.Timestamp,
                    IsRead = false,
                    ImageUrl = message.ImageUrl
                };

                var receiverId = GetReceiverId(chatRoom, message.SenderId);

                if (UserConnections.TryGetValue(receiverId, out var receiverConnection))
                {
                    await Clients.Client(receiverConnection).SendAsync("ReceiveMessage", response);
                }

                await Clients.Caller.SendAsync("ReceiveMessage", response);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task SendImageMessage(SendImageMessageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Image))
                {
                    throw new ArgumentNullException(nameof(request), "Request or image is null");
                }

                // Convert base64 to bytes
                var base64Data = request.Image;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }
                var bytes = Convert.FromBase64String(base64Data);

                // Create temp file
                var fileName = $"image_{DateTime.UtcNow.Ticks}.jpg";
                var file = new FormFile(
                    new MemoryStream(bytes),
                    0,
                    bytes.Length,
                    "image",
                    fileName
                );

                // Create message request
                var imageRequest = new SendImageMessageRequest
                {
                    ChatRoomId = request.ChatRoomId,
                    Image = base64Data
                };

                var message = await _chatMessageService.SendImageMessageAsync(imageRequest);
                var chatRoom = await _chatRoomService.GetChatRoomDetailsAsync(request.ChatRoomId);
                if (chatRoom == null)
                {
                    throw new InvalidOperationException("Chat room not found");
                }

                // Send to receiver
                var receiverId = GetReceiverId(chatRoom, message.SenderId);
                if (UserConnections.TryGetValue(receiverId, out string receiverConnection))
                {
                    await Clients.Client(receiverConnection).SendAsync("ReceiveMessage", new
                    {
                        ChatRoomId = request.ChatRoomId,
                        Message = "[Image]",
                        ImageUrl = message.ImageUrl,
                        SenderName = Context.User.Identity?.Name,
                        Timestamp = message.Timestamp
                    });
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Failed to send image message");
                throw;
            }
        }

        public async Task MarkAsRead(Guid chatRoomId)
        {
            try
            {
                await _chatMessageService.MarkAsReadAsync(chatRoomId);
                var chatRoom = await _chatRoomService.GetChatRoomDetailsAsync(chatRoomId);

                if (UserConnections.TryGetValue(chatRoom.ModeratorId, out string senderConnection))
                {
                    await Clients.Client(senderConnection).SendAsync("MessagesRead", new
                    {
                        ChatRoomId = chatRoomId,
                        ReadAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Cannot mark message as read");
            }
        }

        public async Task EndChat(Guid chatRoomId)
        {
            try
            {
                var chatRoom = await _chatRoomService.GetChatRoomDetailsAsync(chatRoomId);
                var user = await GetAuthenticatedUserAsync();
                if (chatRoom.CustomerId != user.Id && chatRoom.ModeratorId != user.Id)
                {
                    throw new UnauthorizedAccessException("User is not part of this chat");
                }

                var success = await _chatRoomService.EndChatRoomAsync(chatRoomId);
                if (success)
                {
                    await NotifyParticipants(chatRoom, "ChatEnded", new
                    {
                        ChatRoomId = chatRoomId,
                        EndedBy = Context.User.Identity.Name,
                        Timestamp = DateTime.UtcNow,
                        Message = "Chat has ended"
                    });
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        #region Helper methods
        private string GetReceiverId(ChatRoomResponse chatRoom, string senderId)
        {
            return senderId == chatRoom.CustomerId ? chatRoom.ModeratorId : chatRoom.CustomerId;
        }

        private async Task NotifyParticipants(ChatRoomResponse chatRoom, string eventName, object data)
        {
            var participants = new[] { chatRoom.CustomerId, chatRoom.ModeratorId };
            foreach (var participantId in participants.Where(p => p != null))
            {
                if (UserConnections.TryGetValue(participantId, out string connection))
                {
                    await Clients.Client(connection).SendAsync(eventName, data);
                }
            }
        }

        private async Task<ApplicationUser> GetAuthenticatedUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return user;
        }
        #endregion
    }
}
