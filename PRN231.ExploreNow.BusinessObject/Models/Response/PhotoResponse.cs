namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class PhotoResponse
	{
		public Guid Id { get; set; }
		public string Url { get; set; }
		public string Alt { get; set; }
		public Guid? PostId { get; set; }
		public Guid? LocationId { get; set; }
	}
}
