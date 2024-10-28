namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Photo : BaseEntity
{
	public string Url { get; set; }
	public string Alt { get; set; }
	public Guid LocationId { get; set; }
	public Location Location { get; set; }
}