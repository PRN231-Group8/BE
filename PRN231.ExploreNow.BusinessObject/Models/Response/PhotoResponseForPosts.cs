namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class PhotoResponseForPosts
	{
		public Guid Id { get; set; }
		public string Url { get; set; }
		public string Alt { get; set; }
		public Guid? PostId { get; set; }
	}
}
