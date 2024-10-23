using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class TourTrip : BaseEntity
{
	public Guid TourId { get; set; }
	public Tour Tour { get; set; }
	public DateTime TripDate { get; set; }
	public decimal Price { get; set; }
	public int TotalSeats { get; set; }
	public int BookedSeats { get; set; }
	public TripStatus TripStatus { get; set; } // Được ví như là BookingStatus
	public ICollection<Payment> Payments { get; set; }
}