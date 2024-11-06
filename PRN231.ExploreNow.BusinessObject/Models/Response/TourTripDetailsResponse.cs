using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourTripDetailsResponse
	{
		public Guid TourTripId { get; set; }
		public DateTime TripDate { get; set; }
		public TripStatus TripStatus { get; set; }
		public decimal Price { get; set; }
		public int BookedSeats { get; set; }
		public int TotalSeats { get; set; }
	}
}
