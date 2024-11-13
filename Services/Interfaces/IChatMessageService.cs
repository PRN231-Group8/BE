using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface IChatMessageService
    {
        Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<ChatMessageResponse> SendImageMessageAsync(SendImageMessageRequest request);
        Task<IEnumerable<ChatMessageResponse>> GetChatMessagesAsync(Guid chatRoomId);
        Task MarkAsReadAsync(Guid chatRoomId);
        Task<int> GetUnreadMessageCountAsync();
    }
}
