using Microsoft.AspNetCore.Identity;

namespace PRN231.ExploreNow.BusinessObject.Entities;

public class ApplicationUser : IdentityUser<string>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;
    public string? AvatarPath { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Posts> Posts { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}