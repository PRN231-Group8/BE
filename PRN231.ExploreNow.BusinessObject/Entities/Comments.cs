namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Comments : BaseEntity
{
	public string Content { get; set; }
	public Guid? PostId { get; set; }
	public Posts? Post { get; set; }
	public string? UserId { get; set; }
	public ApplicationUser? User { get; set; }
}