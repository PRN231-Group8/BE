using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public ChatMessageService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            var user = await GetAuthenticatedUserAsync();
            return await CreateMessageAsync(request.ChatRoomId, user.Id, request.Content);
        }

        public async Task<ChatMessageResponse> SendImageMessageAsync(SendImageMessageRequest request)
        {
            var user = await GetAuthenticatedUserAsync();

            // Convert base64 to stream
            byte[] imageBytes = Convert.FromBase64String(request.Image);
            using var stream = new MemoryStream(imageBytes);

            // Create a FormFile from stream
            var file = new FormFile(
                baseStream: stream,
                baseStreamOffset: 0,
                length: imageBytes.Length,
                name: "image",
                fileName: $"image_{DateTime.UtcNow.Ticks}.jpg"
            );

            var imageUrl = await _userService.SaveImage(file);
            if (string.IsNullOrEmpty(imageUrl))
                throw new InvalidOperationException("Failed to upload image");

            return await CreateImageMessageAsync(request.ChatRoomId, user.Id, imageUrl);
        }

        public async Task<int> GetUnreadMessageCountAsync()
        {
            var user = await GetAuthenticatedUserAsync();
            return await _unitOfWork.GetRepository<IChatMessageRepository>()
                .GetQueryable()
                .CountAsync(m => m.ReceiverId == user.Id && !m.IsRead);
        }

        public async Task MarkAsReadAsync(Guid chatRoomId)
        {
            var user = await GetAuthenticatedUserAsync();
            await MarkMessagesAsReadAsync(chatRoomId, user.Id);
        }

        public async Task<IEnumerable<ChatMessageResponse>> GetChatMessagesAsync(Guid chatRoomId)
        {
            var user = await GetAuthenticatedUserAsync();

            // Verify access
            var hasAccess = await _unitOfWork.GetRepository<IChatRoomRepository>()
                .GetQueryable()
                .AnyAsync(c => c.Id == chatRoomId &&
                    (c.CustomerId == user.Id || c.ModeratorId == user.Id));

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("User does not have access to this chat room");
            }

            var messages = await _unitOfWork.GetRepository<IChatMessageRepository>()
                .GetQueryable()
                .Include(m => m.Sender)
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatMessageResponse>>(messages);
        }

        #region Helper method
        private async Task<ChatMessageResponse> CreateMessageAsync(Guid chatRoomId, string senderId, string content)
        {
            var user = await GetAuthenticatedUserAsync();
            var chatRoom = await _unitOfWork.GetRepository<IChatRoomRepository>().GetById(chatRoomId);
            if (chatRoom == null || chatRoom.Status != ChatRoomStatus.ACTIVE)
                throw new InvalidOperationException("Chat room is not active");

            if (chatRoom.CustomerId != user.Id && chatRoom.ModeratorId != user.Id)
                throw new UnauthorizedAccessException("User does not have access to this chat room");
            var message = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                ReceiverId = senderId == chatRoom.CustomerId ? chatRoom.ModeratorId : chatRoom.CustomerId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                CreatedBy = user.UserName,
                LastUpdatedBy = user.UserName,
                CreatedDate = DateTime.UtcNow,
                Code = GenerateMessageCode()
            };

            await _unitOfWork.GetRepository<IChatMessageRepository>().AddAsync(message);

            chatRoom.LastMessageTime = DateTime.UtcNow;
            chatRoom.UnreadMessageCount++;
            await _unitOfWork.GetRepository<IChatRoomRepository>().UpdateAsync(chatRoom);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ChatMessageResponse>(message);
        }

        private async Task<ChatMessageResponse> CreateImageMessageAsync(Guid chatRoomId, string senderId, string imageUrl)
        {
            var user = await GetAuthenticatedUserAsync();
            var chatRoom = await _unitOfWork.GetRepository<IChatRoomRepository>().GetById(chatRoomId);
            if (chatRoom == null || chatRoom.Status != ChatRoomStatus.ACTIVE)
                throw new InvalidOperationException("Chat room is not active");

            var message = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                ReceiverId = senderId == chatRoom.CustomerId ? chatRoom.ModeratorId : chatRoom.CustomerId,
                Content = "[Image]",
                ImageUrl = imageUrl,
                Timestamp = DateTime.UtcNow,
                CreatedBy = user.UserName,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedBy = user.UserName,
                Code = GenerateMessageCode()
            };

            await _unitOfWork.GetRepository<IChatMessageRepository>().AddAsync(message);
            await UpdateChatRoomLastMessage(chatRoom);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ChatMessageResponse>(message);
        }

        private async Task MarkMessagesAsReadAsync(Guid chatRoomId, string userId)
        {
            var user = await GetAuthenticatedUserAsync();
            var chatRoom = await _unitOfWork.GetRepository<IChatRoomRepository>().GetById(chatRoomId);
            if (chatRoom == null)
                throw new Exception("Chat room not found");

            var unreadMessages = await _unitOfWork.GetRepository<IChatMessageRepository>()
                .GetQueryable()
                .Where(m => m.ChatRoomId == chatRoomId &&
                           m.ReceiverId == userId &&
                           !m.IsRead &&
                           !m.IsDeleted)
                .ToListAsync();

            if (!unreadMessages.Any()) return;

            var currentTime = DateTime.UtcNow;
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.LastUpdatedBy = user.UserName;
                message.LastUpdatedDate = currentTime;
            }

            _unitOfWork.GetRepository<IChatMessageRepository>().UpdateRange(unreadMessages);
            chatRoom.UnreadMessageCount = 0;
            await _unitOfWork.GetRepository<IChatRoomRepository>().UpdateAsync(chatRoom);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateChatRoomLastMessage(ChatRoom chatRoom)
        {
            chatRoom.LastMessageTime = DateTime.UtcNow;
            chatRoom.UnreadMessageCount++;
            await _unitOfWork.GetRepository<IChatRoomRepository>().UpdateAsync(chatRoom);
        }

        private async Task<ApplicationUser> GetAuthenticatedUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }

        private string GenerateMessageCode()
        {
            return $"MSG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";
        }
        #endregion
    }
}
