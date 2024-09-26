using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;

namespace PRN231.ExploreNow.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SeedRolesAsync();
}