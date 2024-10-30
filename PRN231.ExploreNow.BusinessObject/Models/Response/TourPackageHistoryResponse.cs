namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourPackageHistoryResponse
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public decimal TotalPrice { get; set; }
		public string Status { get; set; }
		public DateTime EndDate { get; set; }
		public List<TourTripDetailsResponse> TourTrips { get; set; }
		public List<TourTimeStampResponse> TourTimestamps { get; set; }
		public List<TransportationResponse> Transportations { get; set; }
		public List<MoodResponseWithoutTours> Moods { get; set; }
		public List<TransactionResponse> Transactions { get; set; }
	}
}
