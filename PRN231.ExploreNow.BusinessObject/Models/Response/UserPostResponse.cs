namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class UserPostResponse
	{
		public Guid UserId { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? AvatarPath { get; set; }
		public DateTime CreatedDate { get; set; }
		public string? DeviceId { get; set; }
	}
}
