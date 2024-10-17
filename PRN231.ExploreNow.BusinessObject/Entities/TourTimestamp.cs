using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class TourTimestamp : BaseEntity
{
	public string Title { get; set; }
	public string Description { get; set; }
	public Guid TourId { get; set; }
	public TimeSlot PreferredTimeSlot { get; set; }
	public Tour Tour { get; set; }
	public Guid LocationId { get; set; }
	public Location Location { get; set; }
}