using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class TourTripRequest
	{
		public Guid TourId { get; set; }
		public DateTime TripDate { get; set; }
		public decimal Price { get; set; }
		public int TotalSeats { get; set; }
		public int BookedSeats { get; set; }
		public TripStatus TripStatus { get; set; }
	}
}
