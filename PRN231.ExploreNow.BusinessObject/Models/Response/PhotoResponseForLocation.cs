namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class PhotoResponseForLocation
	{
		public Guid Id { get; set; }
		public string Url { get; set; }
		public string Alt { get; set; }
		public Guid? LocationId { get; set; }
	}
}
