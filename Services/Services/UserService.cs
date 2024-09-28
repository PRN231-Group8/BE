using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<bool> VerifyEmailTokenAsync(string email, string token)
        {
            var user = await _userRepo.GetUserByEmailAsync(email);

            if (user != null && user.VerifyToken == token && !user.isActived)
            {
                user.isActived = true;
                user.VerifyToken = null;
                user.VerifyTokenExpires = DateTime.MinValue;
                await _userRepo.Update(user);
                return true;
            }
            return false;
        }
    }
}
