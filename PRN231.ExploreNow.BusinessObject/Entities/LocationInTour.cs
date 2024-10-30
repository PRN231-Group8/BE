namespace PRN231.ExploreNow.BusinessObject.Entities;

public class LocationInTour : BaseEntity
{
	public Guid TourId { get; set; }
	public Tour Tour { get; set; }
	public Guid LocationId { get; set; }
	public Location Location { get; set; }
}