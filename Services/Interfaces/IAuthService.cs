using Google.Apis.Auth;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;

namespace PRN231.ExploreNow.Services.Interfaces;

public interface IAuthService
{
	Task<AuthResponse> SeedRolesAsync();
	Task<AuthResponse> LoginAsync(LoginResponse loginResponse);
	Task<AuthResponse> RegisterAsync(RegisterResponse registerResponse);
	Task<AuthResponse> MakeAdminAsync(UpdatePermissionResponse updatePermissionResponse);
	Task<AuthResponse> MakeModeratorAsync(UpdatePermissionResponse updatePermissionResponse);
	Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthRequest externalAuth);
	Task<ExternalAuthResponse> HandleExternalLogin(ExternalAuthRequest externalAuth);
}