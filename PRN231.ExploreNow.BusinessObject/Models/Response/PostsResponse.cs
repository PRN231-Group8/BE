namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class PostsResponse
	{
		public Guid PostsId { get; set; }
		public string Content { get; set; }
		public int Rating { get; set; }
		public string Status { get; set; }
		public DateTime CreateDate { get; set; }
		public UserPostResponse User { get; set; }
		public List<CommentResponse> Comments { get; set; } = new List<CommentResponse>();
		public List<PhotoResponse> Photos { get; set; } = new List<PhotoResponse>();
	}
}
