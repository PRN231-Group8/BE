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
    }
}
