using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories
{
    public class TourRepository : BaseRepository<Tour>, ITourRepository
    {
        private readonly BaseRepository<Tour> _baseRepository;

        public TourRepository(DbContext dbContext) : base(dbContext)
        {
            _baseRepository = new BaseRepository<Tour>(dbContext);
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
