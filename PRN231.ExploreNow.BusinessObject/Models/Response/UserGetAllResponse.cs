namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class UserGetAllResponse
	{
		public string Id { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Email { get; set; }
		public DateTime? Dob { get; set; }
		public string? Gender { get; set; }
		public string? AvatarPath { get; set; }
		public bool IsActive { get; set; }
	}
}
