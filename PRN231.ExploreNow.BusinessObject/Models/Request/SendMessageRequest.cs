namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
    public class SendMessageRequest
    {
        public Guid ChatRoomId { get; set; }
        public string Content { get; set; }
    }
}
