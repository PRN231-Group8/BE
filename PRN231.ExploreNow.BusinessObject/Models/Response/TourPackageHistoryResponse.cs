namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TourPackageHistoryResponse
	{
		public Guid Id { get; set; }
		public decimal TotalPrice { get; set; }
		public DateTime EndDate { get; set; }
		public List<TransactionResponse> Transactions { get; set; }
	}
}
