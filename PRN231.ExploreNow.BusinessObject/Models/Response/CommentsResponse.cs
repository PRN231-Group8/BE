using PRN231.ExploreNow.BusinessObject.Entities;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class CommentsResponse
	{
		public Guid CommentsId { get; set; }
		public string Content { get; set; }
		public UserResponse User { get; set; }
	}
}
