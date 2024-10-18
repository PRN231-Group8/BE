using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Services
{
    public class MoodService : IMoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public MoodService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task Add(MoodRequest moods)
        {
            var mood = MapToMoods(moods);
            await _unitOfWork.GetRepositoryByEntity<Moods>().AddAsync(mood);    
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await _unitOfWork.GetRepositoryByEntity<Moods>().DeleteAsync(id);
        }

        public async Task<List<Moods>> GetAllAsync(int page, int pageSize, List<string>? searchTerm)
        {
            return await _unitOfWork.GetRepository<IMoodRepository>().GetAllAsync(page, pageSize, searchTerm);
        }

        public async Task<Moods> GetById(Guid id)
        {
            var mood = await _unitOfWork.GetRepositoryByEntity<Moods>().GetById(id);
            return mood;
        }

        public async Task Update(MoodRequest moods, Guid id)
        {
            var mood = MapToMoods(moods);
            mood.Id = id;
            await _unitOfWork.GetRepositoryByEntity<Moods>().UpdateAsync(mood);
            await _unitOfWork.SaveChangesAsync();
        }

        private Moods MapToMoods(MoodRequest mood) 
        {
            var currentUser = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currUserName = _contextAccessor.HttpContext?.User.Identity.Name;
            return new Moods
            {
                Code = GenerateUniqueCode(),
                MoodTag = mood.MoodTag,
                CreatedBy = currUserName,
                CreatedDate = DateTime.Now,
                LastUpdatedBy = currUserName,
                LastUpdatedDate = DateTime.Now,
                IsDeleted = false,
            };
        }
        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }
}
