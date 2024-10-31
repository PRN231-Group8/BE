using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TransportationResponse
	{
		public Guid Id { get; set; }
		public TransportationType Type { get; set; }
		public decimal Price { get; set; }
		public int Capacity { get; set; }
        public Guid TourId { get; set; }
    }
}
