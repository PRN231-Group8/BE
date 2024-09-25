namespace ExploreNow.Domain.Entities;

public class Moods : BaseEntity
{
    public string MoodTag { get; set; }
    public Guid TourId { get; set; }
    public Tour Tour { get; set; }
}