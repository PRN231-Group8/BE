using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
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

		public async Task<List<Tour>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm)
		{
			var query = GetQueryable()
						.Where(t => !t.IsDeleted);
			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(t => t.Title.Contains(searchTerm) || t.Description.Contains(searchTerm));
			}
			if (sortByStatus.HasValue)
			{
				query = query.OrderBy(t => t.Status == sortByStatus.Value);
			}
			var tours = await query.Skip((page - 1) * pageSize)
									   .Take(pageSize)
									   .ToListAsync();
			return tours.ToList();
		}
	}
}
