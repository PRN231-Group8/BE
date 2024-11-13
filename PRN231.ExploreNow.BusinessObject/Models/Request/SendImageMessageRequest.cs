using Microsoft.AspNetCore.Http;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
    public class SendImageMessageRequest
    {
        public Guid ChatRoomId { get; set; }
        public string Image { get; set; }
    }
}
