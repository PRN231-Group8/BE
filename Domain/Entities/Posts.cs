namespace ExploreNow.Domain.Entities;

public class Posts : BaseEntity
{
    public string Content { get; set; }
    public int Rating { get; set; }
}