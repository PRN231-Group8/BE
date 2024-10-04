using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> VerifyEmailTokenAsync(string email, string token)
        {
            var userRepo = _unitOfWork.GetRepository<IUserRepository>();
            var user = await userRepo.GetUserByEmailAsync(email);

            if (user == null || user.VerifyToken != token || user.isActived)
            {
                return false;
            }

            user.isActived = true;
            user.VerifyToken = null;
            user.VerifyTokenExpires = DateTime.MinValue;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

    }
}
