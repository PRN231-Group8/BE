using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Booking : BaseEntity
{
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public Guid TourId { get; set; }
    public Tour Tour { get; set; }
    public ICollection<Transportation> Transportations { get; set; }
}