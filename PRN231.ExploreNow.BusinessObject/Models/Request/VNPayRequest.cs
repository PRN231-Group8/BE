namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class VNPayRequest
	{
		public Guid TourTripId { get; set; }
		public string FullName { get; set; }
		public string Description { get; set; }
		public double Amount { get; set; }
	}
}
