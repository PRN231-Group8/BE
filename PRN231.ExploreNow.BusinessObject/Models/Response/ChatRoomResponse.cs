using PRN231.ExploreNow.BusinessObject.Enums;
namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
    public class ChatRoomResponse
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public ChatRoomStatus Status { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadMessageCount { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ModeratorId { get; set; }
        public string ModeratorName { get; set; }
        public List<ChatMessageResponse> Messages { get; set; }
    }
}
