using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class TransactionResponse
	{
		public Guid Id { get; set; }
		public string UserId { get; set; }
		public PaymentTransactionStatus Status { get; set; }
		public decimal Amount { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
