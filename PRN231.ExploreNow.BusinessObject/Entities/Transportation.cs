using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Transportation : BaseEntity
{
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public TransportationType Type { get; set; }
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; }
}