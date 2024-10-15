using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class TourTimeStampRequest
	{
		public string? Title { get; set; }
		public string? Description { get; set; }
		public TimeSlot PreferredTimeSlot { get; set; }
		public Guid TourId { get; set; }
	}
}
