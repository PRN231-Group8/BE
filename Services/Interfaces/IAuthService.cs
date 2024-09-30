using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;
using PRN231.ExploreNow.Services.Services;

namespace PRN231.ExploreNow.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SeedRolesAsync();
    Task<AuthResponse> LoginAsync(LoginResponse loginResponse);
    Task<AuthResponse> RegisterAsync(RegisterResponse registerResponse);
    Task<AuthResponse> MakeAdminAsync(UpdatePermissionResponse updatePermissionResponse);
    Task<AuthResponse> MakeModeratorAsync(UpdatePermissionResponse updatePermissionResponse);
}