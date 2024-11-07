namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class CommentResponse
	{
		public Guid Id { get; set; }
		public string Content { get; set; }
		public Guid PostId { get; set; }
		public DateTime CreatedDate { get; set; }
		public UserPostResponse User { get; set; }
	}
}
