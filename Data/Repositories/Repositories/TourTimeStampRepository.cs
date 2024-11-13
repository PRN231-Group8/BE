using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
    public class TourTimeStampRepository : BaseRepository<TourTimestamp>, ITourTimeStampRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public TourTimeStampRepository(ApplicationDbContext context, IMapper mapper) : base(context)
		{
			_context = context;
			_mapper = mapper;
		}

        public async Task<(List<TourTimeStampResponse> Items, int TotalCount)> GetAllTourTimestampsAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm)
        {
            var query = GetQueryable(p => !p.IsDeleted && !p.Tour.IsDeleted && !p.Location.IsDeleted)
                       .Include(p => p.Tour)
                       .Include(tt => tt.Location)
                           .ThenInclude(l => l.Photos.Where(p => !p.IsDeleted))
                       .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(tt =>
                    tt.Title.Contains(searchTerm) ||
                    tt.Description.Contains(searchTerm));
            }
            query = query.OrderBy(tt => tt.PreferredTimeSlot.StartTime);

            if (sortByTime.HasValue)
            {
                query = query.OrderBy(tt => Math.Abs((tt.PreferredTimeSlot.StartTime - sortByTime.Value).TotalMinutes));
            }

            var totalCount = await query.CountAsync();

            // Apply paging after sorting
            var tourTimestamps = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedResults = _mapper.Map<List<TourTimeStampResponse>>(tourTimestamps);
            return (mappedResults, totalCount);
        }
    }
}
