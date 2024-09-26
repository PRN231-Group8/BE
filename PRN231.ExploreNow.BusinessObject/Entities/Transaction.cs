namespace PRN231.ExploreNow.BusinessObject.Entities;

public class Transaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}