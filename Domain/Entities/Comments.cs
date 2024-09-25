namespace ExploreNow.Domain.Entities;

public class Comments : BaseEntity
{
    public string Content { get; set; }
    public Guid PostId { get; set; }
    public Posts Post { get; set; }
}