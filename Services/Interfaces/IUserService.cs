using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface IUserService
	{
		Task<bool> VerifyEmailTokenAsync(string email, string token);
		Task<string> SaveImage(IFormFile image);
		Task<UserProfileResponseModel> UpdateUserProfile(string id, UserProfileRequestModel profile);
		Task<UserProfileResponseModel> GetUserByEmailAsync(string email);
	}
}
