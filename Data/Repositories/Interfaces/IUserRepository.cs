using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces
{
	public interface IUserRepository : IBaseRepository
	{
		Task<ApplicationUser> GetUserByEmailAsync(string email);
		Task Update(ApplicationUser applicationUser);
		Task<UserProfileResponseModel> UpdateProfileAsync(ApplicationUser applicationUser);
		Task<ApplicationUser> GetUsersClaimIdentity();
	}
}
