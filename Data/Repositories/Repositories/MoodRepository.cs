using Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
    public class MoodRepository : BaseRepository<Moods>, IMoodRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        public MoodRepository(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager) : base(dbContext)
        {
            _context = dbContext;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public async Task<MoodResponse> CreateAsync(Moods mood)
        {
            Add(mood);
            await _context.SaveChangesAsync();
            return MapToResponse(mood);
        }

        public async Task<List<Moods>> GetAllAsync(int page, int pageSize, string? searchTerm)
        {
            var query = GetQueryable()
                        .Where(m => !m.IsDeleted);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m => m.MoodTag.Contains(searchTerm));
            }
            var moods = await query.Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();
            return moods.ToList();
        }

        public async Task<MoodResponse> GetByIdAsync(Guid id)
        {
            var mood = await GetQueryable(m => m.Id == id && !m.IsDeleted)
                                .Include(m => m.TourMoods)
                                .FirstOrDefaultAsync();
            return mood == null ? null : MapToResponse(mood);
        }

        public async Task<MoodResponse> UpdateAsync(Moods mood)
        {
            var existMood = await GetQueryable(m => m.Id == mood.Id && m.IsDeleted == false)
                .Include(m => m.TourMoods)
                .FirstOrDefaultAsync();
            if(existMood == null)
            {
                return null;
            }
            UpdateMoodsPropertise(existMood, mood);
            Update(existMood);
            await _context.SaveChangesAsync();
            return MapToResponse(existMood);
        }

        Task IMoodRepository.DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        private void UpdateMoodsPropertise(Moods mood , Moods newMood)
        {
            var currUser = _userManager.GetUserAsync(_contextAccessor.HttpContext.User).Result;
            var currUserName = currUser?.UserName ?? "Admin";
            mood.MoodTag = newMood.MoodTag;
            if (mood.TourMoods.Any() || mood.TourMoods != null) 
            {
                foreach (var tours in mood.TourMoods)
                {
                    tours.IsDeleted = true;
                }
                mood.TourMoods = newMood.TourMoods.Select(p => new TourMood
                {
                    TourId = p.TourId,
                    MoodId = p.MoodId,
                    Code = p.Code ?? GenerateUniqueCode(),
                    CreatedBy = currUserName,
                    CreatedDate = DateTime.Now,
                    LastUpdatedBy = currUserName,
                    IsDeleted = false
                }).ToList();
            }

        }

        private MoodResponse MapToResponse (Moods mood)
        {
            return new MoodResponse
            {
                Id = mood.Id,
                MoodTag = mood.MoodTag,
                TourMoods = mood.TourMoods
                .Where(m => !m.IsDeleted)
                .Select(m => new TourMood
                {
                    Id = m.Id,
                    Tour = m.Tour,
                }).ToList()
            };
        }

        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }
}
