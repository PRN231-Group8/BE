namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class UserProfileRequestModel
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public DateTime? Dob { get; set; }
		public string? Gender { get; set; }
		public string? AvatarPath { get; set; }
	}
}
