namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourTripResponse
	{
		public Guid TourTripId { get; set; }
		public Guid TourId { get; set; }
		public DateTime TripDate { get; set; }
		public decimal Price { get; set; }
		public int TotalSeats { get; set; }
		public int BookedSeats { get; set; }
		public string TripStatus { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}
