namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Moods : BaseEntity
{
    public string MoodTag { get; set; }
    public ICollection<TourMood> TourMoods { get; set; }
}