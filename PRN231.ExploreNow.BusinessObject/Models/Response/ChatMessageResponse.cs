namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
    public class ChatMessageResponse
    {
        public Guid ChatRoomId { get; set; }
        public Guid Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }
}
