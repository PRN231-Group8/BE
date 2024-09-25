using ExploreNow.Domain.Models.Response.Auth;

namespace ExploreNow.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SeedRolesAsync();
}