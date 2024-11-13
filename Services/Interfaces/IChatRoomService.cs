using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface IChatRoomService
    {
        Task<ChatRoomResponse> GetChatRoomDetailsAsync(Guid roomId);
        Task<bool> AcceptChatRoomAsync(Guid roomId);
        Task<bool> EndChatRoomAsync(Guid roomId);
        Task<ChatRoomResponse> CreateChatRoomAsync(CreateChatRoomRequest request);
        Task<ChatRoomResponse> GetActiveChatForCustomerAsync(string customerId);
        Task<IEnumerable<ChatRoomResponse>> GetPendingChatsAsync();
        Task<IEnumerable<ChatRoomResponse>> GetActiveChatsAsync();
        Task<ChatRoomResponse> GetCurrentRoomAsync();
    }
}
