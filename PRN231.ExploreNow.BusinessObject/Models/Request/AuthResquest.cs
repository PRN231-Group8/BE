namespace PRN231.ExploreNow.BusinessObject.Models.Request;

public class AuthResquest
{
    public bool IsSucceed { get; set; }
    public string? Token { get; set; }
    public string? Role { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
}