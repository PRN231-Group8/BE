using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;

namespace PRN231.ExploreNow.BusinessObject.Contracts.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; 
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor with ApplicationDbContext as a parameter
        public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Method to get user by email
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            var users = await _context.Users
                .Where(u => u.Email == email)
                .ToListAsync();
            if (users.Count > 1)
            {
                throw new InvalidOperationException("Sequence contains more than one element.");
            }
            return users.SingleOrDefault();
        }

        // Method to update user
        public async Task Update(ApplicationUser applicationUser)
        {
            _context.Update(applicationUser);
            await _context.SaveChangesAsync();
        }
    }
}
