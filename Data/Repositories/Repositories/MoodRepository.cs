using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
    public class MoodRepository : BaseRepository<Moods>, IMoodRepository
	{
		private readonly ApplicationDbContext _context;

		public MoodRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_context = dbContext;
		}

		public async Task<(List<Moods> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? searchTerms)
		{
			var query = GetQueryable()
						.Where(m => !m.IsDeleted)
						.AsEnumerable();

			if (searchTerms != null && searchTerms.Any())
			{
				query = query.Where(m => searchTerms.Any(term => m.MoodTag.Contains(term)));
			}

			// Tính tổng số lượng record thỏa điều kiện
			var totalCount = query.Count();

			var moods = query.Skip((page - 1) * pageSize)
							 .Take(pageSize)
							 .ToList();

			return (moods, totalCount);
		}
	}
}
