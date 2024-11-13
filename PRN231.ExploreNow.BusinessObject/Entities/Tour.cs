using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Tour : BaseEntity
{
    public decimal TotalPrice { get; set; }
    public TourStatus Status { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    //public bool? isCustomization { get; set; }
    public ICollection<Transportation> Transportations { get; set; }
    public ICollection<TourTimestamp> TourTimestamps { get; set; }
    public ICollection<LocationInTour> LocationInTours { get; set; }
    public ICollection<TourMood> TourMoods { get; set; }
    public ICollection<TourTrip> TourTrips { get; set; }
}