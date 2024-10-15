using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Posts : BaseEntity
{
	public string Content { get; set; }
	public int Rating { get; set; }
	public PostsStatus Status { get; set; } = PostsStatus.Pending;
	public string UserId { get; set; }
	public ApplicationUser User { get; set; }

	public ICollection<Comments> Comments { get; set; } = new List<Comments>();
	public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}