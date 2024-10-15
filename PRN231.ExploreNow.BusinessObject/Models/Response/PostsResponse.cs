namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class PostsResponse
	{
		public Guid PostsId { get; set; }
		public string Content { get; set; }
		public int Rating { get; set; }
		public string Status { get; set; }
		public UserReponse User { get; set; }
		public List<CommentsResponse> Comments { get; set; } = new List<CommentsResponse>();
		public List<PhotoResponse> Photos { get; set; } = new List<PhotoResponse>();
	}
}
