using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;

namespace PRN231.ExploreNow.BusinessObject.Contracts.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task Update(ApplicationUser applicationUser)
        {
            _context.Users.Update(applicationUser);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProfileResponseModel> UpdateProfileAsync(ApplicationUser applicationUser)
        {
            var existUser = _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;
            if (existUser == null)
            {
                return null;
            }
            UpdateProfileProperty(existUser, applicationUser);
            await Update(existUser);
            await _context.SaveChangesAsync();
            return MapToResponse(existUser);
        }

        private void UpdateProfileProperty(ApplicationUser existUser, ApplicationUser newUser)
        {
            existUser.Gender = newUser.Gender;
            existUser.FirstName = newUser.FirstName;
            existUser.LastName = newUser.LastName;
            existUser.Dob = newUser.Dob;
            existUser.AvatarPath = newUser.AvatarPath;
        }

        private UserProfileResponseModel MapToResponse(ApplicationUser applicationUser)
        {
            return new UserProfileResponseModel
            {
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                Dob = applicationUser.Dob,
                Gender = applicationUser.Gender,
                AvatarPath = applicationUser.AvatarPath
            };
        }
    }
}
