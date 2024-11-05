namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class PaymentRequest
	{
		public Guid TourTripId { get; set; }
		public int NumberOfPassengers { get; set; }
	}
}
