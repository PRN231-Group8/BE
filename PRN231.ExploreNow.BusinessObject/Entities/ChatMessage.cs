namespace PRN231.ExploreNow.BusinessObject.Entities
{
    public class ChatMessage : BaseEntity
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
        public Guid ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; }
    }
}
