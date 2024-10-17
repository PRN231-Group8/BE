using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourTimeStampResponse
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public TimeSlot PreferredTimeSlot { get; set; }
		public Guid TourId { get; set; }
		public Guid LocationId { get; set; }
		public LocationResponse Location { get; set; }
	}
}
