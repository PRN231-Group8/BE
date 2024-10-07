using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories
{
    public class TourRepository : BaseRepository<Tour>, ITourRepository
    {
        private ApplicationDbContext _dbContext;

        public TourRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm)
        {
            var query = GetQueryable()
                        .Include(t => t.TourMoods)
                        .Where(l => !l.IsDeleted);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.Title.Contains(searchTerm) || l.Description.Contains(searchTerm));
            }
            if (sortByStatus.HasValue)
            {
                query = query.OrderBy(l => l.Status == sortByStatus.Value);
            }
            var tours = await query.Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();
            return tours.ToList();
        }
    }
}
