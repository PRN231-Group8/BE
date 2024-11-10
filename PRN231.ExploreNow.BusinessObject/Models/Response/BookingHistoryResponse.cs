using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Models.Response;

public class BookingHistoryResponse
{
    public decimal TransactionAmount { get; set; }
    public PaymentTransactionStatus TransactionStatus { get; set; }
    public string PaymentMethod { get; set; }
    public int NumberOfPassengers { get; set; }
    public DateTime TourTripDate { get; set; }
    public TourResponse Tour { get; set; }
}