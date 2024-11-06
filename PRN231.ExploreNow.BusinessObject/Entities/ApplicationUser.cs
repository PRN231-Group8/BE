using Microsoft.AspNetCore.Identity;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class ApplicationUser : IdentityUser<string>
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public DateTime? Dob { get; set; }
	public string? DeviceId { get; set; }
	public string? Gender { get; set; }
	public string? Address { get; set; }
	public DateTime CreatedDate { get; set; }
	public string? CreatedBy { get; set; }
	public string? LastUpdatedBy { get; set; }
	public DateTime? LastUpdatedDate { get; set; }
	public string? AvatarPath { get; set; }
	public string? VerifyToken { get; set; }
	public DateTime? VerifyTokenExpires { get; set; }
	public bool isActived { get; set; } = false;
	public ICollection<Tour> Tours { get; set; } = new List<Tour>();
	public ICollection<Posts> Posts { get; set; }
	public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
	public ICollection<Payment> Payments { get; set; }
	public ICollection<Comments> Comments { get; set; } = new List<Comments>();
}