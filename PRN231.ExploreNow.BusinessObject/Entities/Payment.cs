using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Payment : BaseEntity
{
	public decimal Amount { get; set; }
	public Guid TourTripId { get; set; }
	public TourTrip TourTrip { get; set; }
	public string UserId { get; set; }
	public ApplicationUser User { get; set; }
	public PaymentStatus Status { get; set; }
	public string PaymentMethod { get; set; }
	public string PaymentTransactionId { get; set; }
	public Transaction Transaction { get; set; }
}
