namespace ExploreNow.Domain.Entities;

public class LocationRequest : BaseEntity
{
    public Guid TourId { get; set; }
    public Tour Tour { get; set; }
    public Guid LocationId { get; set; }
    public Location Location { get; set; }
}