using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class TransportationRequestModel
	{
		public TransportationType Type { get; set; }
		public decimal Price { get; set; }
		public int Capacity { get; set; }
		public Guid TourId { get; set; }
	}
}
