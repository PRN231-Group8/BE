using Google.Apis.Auth;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;

namespace PRN231.ExploreNow.Services.Interfaces;

public interface IAuthService
{
	Task<AuthResquest> SeedRolesAsync();
	Task<AuthResquest> LoginAsync(LoginResponse loginResponse);
	Task<AuthResquest> RegisterAsync(RegisterResponse registerResponse);
	Task<AuthResquest> MakeAdminAsync(UpdatePermissionResponse updatePermissionResponse);
	Task<AuthResquest> MakeModeratorAsync(UpdatePermissionResponse updatePermissionResponse);
	Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthRequest externalAuth);
	Task<ExternalAuthResponse> HandleExternalLogin(ExternalAuthRequest externalAuth);
}