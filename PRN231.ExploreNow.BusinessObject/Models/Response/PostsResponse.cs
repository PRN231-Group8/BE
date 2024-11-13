namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
    public class PostsResponse
    {
        public Guid PostsId { get; set; }
        public bool IsRecommended { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public UserPostResponse User { get; set; }
        public Guid TourTripId { get; set; }
        public List<CommentResponse> Comments { get; set; } = new List<CommentResponse>();
        public List<PhotoResponseForPosts> Photos { get; set; } = new List<PhotoResponseForPosts>();
    }
}
