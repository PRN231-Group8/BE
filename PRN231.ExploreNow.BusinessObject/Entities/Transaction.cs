using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Transaction : BaseEntity
{
	public decimal Amount { get; set; }
	public string UserId { get; set; }
	public ApplicationUser User { get; set; }
	public Guid PaymentId { get; set; }
	public Payment Payment { get; set; }
	public PaymentTransactionStatus Status { get; set; }
}