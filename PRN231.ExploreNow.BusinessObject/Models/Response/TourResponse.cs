using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal TotalPrice { get; set; }
		public TourStatus Status { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public ICollection<TransportationResponse> Transportations { get; set; }
		public ICollection<TourTimeStampResponse> TourTimestamps { get; set; }
		public ICollection<LocationInTourResponse> LocationInTours { get; set; }
		public ICollection<TourMoodResponse> TourMoods { get; set; }
		public ICollection<TourTripDetailsResponse> TourTrips { get; set; }
	}
}
