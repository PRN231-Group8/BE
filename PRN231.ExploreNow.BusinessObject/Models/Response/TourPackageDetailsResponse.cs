using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourPackageDetailsResponse
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public decimal TotalPrice { get; set; }
		public TourStatus Status { get; set; }
		public DateTime EndDate { get; set; }
		public List<TourTripDetailsResponse> TourTrips { get; set; }
		public List<TourTimeStampResponse> TourTimestamps { get; set; }
		public List<TransportationResponse> Transportations { get; set; }
		public List<MoodResponse> Moods { get; set; }
	}
}
