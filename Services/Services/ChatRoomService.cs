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
    public class ChatRoomService : IChatRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatRoomService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ChatRoomResponse> CreateChatRoomAsync(CreateChatRoomRequest request)
        {
            var user = await GetAuthenticatedUserAsync();
            await ValidateNoDuplicateActiveRoom(user.Id);

            var chatRoom = new ChatRoom
            {
                CustomerId = user.Id,
                Subject = request.Subject,
                Status = ChatRoomStatus.WAITING,
                IsActive = true,
                CreatedBy = user.UserName,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedBy = user.UserName,
                Code = GenerateChatCode()
            };

            await _unitOfWork.GetRepository<IChatRoomRepository>().AddAsync(chatRoom);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ChatRoomResponse>(chatRoom);
        }

        public async Task<bool> EndChatRoomAsync(Guid roomId)
        {
            var user = await GetAuthenticatedUserAsync();
            var chatRoom = await _unitOfWork.GetRepository<IChatRoomRepository>().GetById(roomId);

            if (chatRoom == null || chatRoom.Status == ChatRoomStatus.CLOSED)
                return false;

            await ValidateChatRoomAccess(chatRoom, user.Id);
            await UpdateChatRoomStatus(chatRoom, ChatRoomStatus.CLOSED, user.Id);

            return true;
        }

        public async Task<bool> AcceptChatRoomAsync(Guid roomId)
        {
            var user = await GetAuthenticatedUserAsync();
            await ValidateModeratorRole(user);

            var chatRoom = await _unitOfWork.GetRepository<IChatRoomRepository>().GetById(roomId);
            if (chatRoom == null || chatRoom.Status != ChatRoomStatus.WAITING)
                return false;

            await UpdateChatRoomStatus(chatRoom, ChatRoomStatus.ACTIVE, user.UserName, user.Id);

            return true;
        }

        public async Task<ChatRoomResponse> GetChatRoomDetailsAsync(Guid roomId)
        {
            var user = await GetAuthenticatedUserAsync();
            var chatRoom = await GetDetailedChatRoomAsync(roomId, user.Id);
            return _mapper.Map<ChatRoomResponse>(chatRoom);
        }

        public async Task<ChatRoomResponse> GetActiveChatForCustomerAsync(string customerId)
        {
            var activeChat = await _unitOfWork.GetRepository<IChatRoomRepository>()
                .GetQueryable()
                .Include(c => c.Customer)
                .Include(c => c.Moderator)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId
                                       && c.IsActive
                                       && (c.Status == ChatRoomStatus.WAITING
                                       || c.Status == ChatRoomStatus.ACTIVE));

            if (activeChat == null) return null;

            return _mapper.Map<ChatRoomResponse>(activeChat);
        }

        public async Task<IEnumerable<ChatRoomResponse>> GetPendingChatsAsync()
        {
            var pendingChats = await _unitOfWork.GetRepository<IChatRoomRepository>()
                .GetQueryable()
                .Include(c => c.Customer)
                .Where(c => c.Status == ChatRoomStatus.WAITING && c.IsActive)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatRoomResponse>>(pendingChats);
        }

        public async Task<IEnumerable<ChatRoomResponse>> GetActiveChatsAsync()
        {
            var user = await GetAuthenticatedUserAsync();

            var activeChats = await _unitOfWork.GetRepository<IChatRoomRepository>()
                .GetQueryable()
                .Include(c => c.Customer)
                .Include(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(1))
                .Where(c => c.Status == ChatRoomStatus.ACTIVE
                           && c.IsActive
                           && c.ModeratorId == user.Id)
                .OrderByDescending(c => c.LastMessageTime ?? c.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatRoomResponse>>(activeChats);
        }

        public async Task<ChatRoomResponse> GetCurrentRoomAsync()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Kiểm tra role
            if (await _userManager.IsInRoleAsync(user, StaticUserRoles.MODERATOR))
            {
                throw new InvalidOperationException("Moderators cannot have current chats");
            }

            var currentRoom = await GetActiveChatForCustomerAsync(user.Id);
            return currentRoom;
        }

        #region Helper methods
        private async Task<ChatRoom> GetDetailedChatRoomAsync(Guid roomId, string userId)
        {
            return await _unitOfWork.GetRepository<IChatRoomRepository>()
                .GetQueryable()
                .Include(c => c.Customer)
                .Include(c => c.Moderator)
                .Include(c => c.Messages.OrderByDescending(m => m.Timestamp))
                .FirstOrDefaultAsync(c => c.Id == roomId &&
                    (c.CustomerId == userId || c.ModeratorId == userId));
        }

        private async Task ValidateNoDuplicateActiveRoom(string userId)
        {
            var existingActiveRoom = await _unitOfWork.GetRepository<IChatRoomRepository>()
                .GetQueryable()
                .AnyAsync(c => c.CustomerId == userId &&
                              c.IsActive &&
                              c.Status != ChatRoomStatus.CLOSED);

            if (existingActiveRoom)
                throw new InvalidOperationException("User already has an active chat room");
        }

        private async Task ValidateChatRoomAccess(ChatRoom chatRoom, string userId)
        {
            if (chatRoom.CustomerId != userId && chatRoom.ModeratorId != userId)
                throw new UnauthorizedAccessException("User does not have access to this chat room");
        }

        private async Task ValidateModeratorRole(ApplicationUser user)
        {
            if (!await _userManager.IsInRoleAsync(user, StaticUserRoles.MODERATOR))
                throw new UnauthorizedAccessException("Only moderators can accept chat rooms");
        }

        private async Task UpdateChatRoomStatus(
            ChatRoom chatRoom,
            ChatRoomStatus status,
            string updatedBy,
            string moderatorId = null)
        {
            var user = await GetAuthenticatedUserAsync();

            chatRoom.Status = status;
            chatRoom.IsActive = status != ChatRoomStatus.CLOSED;
            chatRoom.LastUpdatedBy = user.UserName;
            chatRoom.LastUpdatedDate = DateTime.UtcNow;

            if (status == ChatRoomStatus.CLOSED)
                chatRoom.EndDate = DateTime.UtcNow;
            else if (status == ChatRoomStatus.ACTIVE)
            {
                chatRoom.ModeratorId = moderatorId;
                chatRoom.FirstResponseTime = DateTime.UtcNow;
            }

            await _unitOfWork.GetRepository<IChatRoomRepository>().UpdateAsync(chatRoom);
            await _unitOfWork.SaveChangesAsync();
        }

        private string GenerateChatCode()
        {
            return $"CHAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";
        }

        private async Task<ApplicationUser> GetAuthenticatedUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }
        #endregion
    }
}
