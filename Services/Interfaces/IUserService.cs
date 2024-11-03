using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface IUserService
	{
		Task<bool> VerifyEmailTokenAsync(string email, string token);
		Task<string> SaveImage(IFormFile image);
		Task<UserProfileResponseModel> UpdateUserProfile(string id, UserProfileRequestModel profile);
		Task<UserProfileResponseModel> GetUserByEmailAsync(string email);
		Task<List<UserResponse>> GetAllUsersAsync();
	}
}
