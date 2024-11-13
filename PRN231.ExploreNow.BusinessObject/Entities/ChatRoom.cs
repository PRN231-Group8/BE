using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities
{
    public class ChatRoom : BaseEntity
    {
        public bool IsActive { get; set; } = true;
        public ChatRoomStatus Status { get; set; }
        public string? Subject { get; set; } // Issue for the chat
        public DateTime? LastMessageTime { get; set; }
        public int UnreadMessageCount { get; set; } = 0;
        public DateTime? FirstResponseTime { get; set; }
        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }
        public string? ModeratorId { get; set; }
        public ApplicationUser? Moderator { get; set; }
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
