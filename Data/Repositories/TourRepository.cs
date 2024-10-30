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
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        public TourRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<TourResponse>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, List<string>? searchTerm)
        {
            var query = GetQueryable()
                .Where(t => !t.IsDeleted)
                .Include(t => t.TourTimestamps)
                .Include(t => t.TourMoods)
                    .ThenInclude(tm => tm.Mood) // Đảm bảo bao gồm quan hệ Mood
                .Include(t => t.LocationInTours)
                    .ThenInclude(lit => lit.Location)
                .Include(t => t.Transportations)
                .Include(t => t.TourTrips)
                .AsQueryable();

            var allTours = await query.ToListAsync();

            IEnumerable<Tour> filteredTours = allTours;

            if (searchTerm != null && searchTerm.Any())
            {
                filteredTours = allTours.Where(t =>
                    t.TourMoods.Any(tm => tm.Mood != null && searchTerm.Contains(tm.Mood.MoodTag)) || // Kiểm tra null
                    t.LocationInTours.Any(lit => searchTerm.Contains(lit.Location.Name)) ||
                    searchTerm.Any(keyword => t.Description.Contains(keyword))
                );
            }

            if (sortByStatus.HasValue)
            {
                filteredTours = filteredTours.OrderBy(t => t.Status == sortByStatus.Value);
            }

            var tours = filteredTours.Select(t => new TourResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                TourMoods = t.TourMoods
                    .Where(tm => tm.Mood != null) // Lọc những TourMood có Mood không null
                    .Select(tm => new TourMoodResponse
                    {
                        MoodTag = tm.Mood.MoodTag,
                        IconName = tm.Mood.IconName
                    }).ToList(),
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            return tours;
        }

		public async Task<List<Tour>> GetTourBookingHistoryAsync(
			string userId,
			int page,
			int pageSize,
			PaymentTransactionStatus? filterTransactionStatus,
			string? searchTerm = null)
		{
			// Start with tours that have bookings for this user
			var query = GetQueryable()
				.Where(t => !t.IsDeleted)
				.Where(t => t.TourTrips.Any(tt => tt.Payments.Any(p => p.UserId == userId)))
				.Include(t => t.TourTrips.Where(tt => tt.Payments.Any(p => p.UserId == userId)))
					.ThenInclude(tt => tt.Payments.Where(p => p.UserId == userId))
						.ThenInclude(p => p.Transaction)
				.Include(t => t.TourTimestamps)
					.ThenInclude(ts => ts.Location)
						.ThenInclude(l => l.Photos)
				.Include(t => t.Transportations)
				.Include(t => t.TourMoods)
					.ThenInclude(tm => tm.Mood)
				.AsSplitQuery();

			// Apply filters
			if (!string.IsNullOrEmpty(searchTerm))
			{
				searchTerm = searchTerm.ToLower();
				query = query.Where(t =>
					t.Title.ToLower().Contains(searchTerm) ||
					t.Description.ToLower().Contains(searchTerm) ||
					t.TourTimestamps.Any(tt => tt.Location.Name.ToLower().Contains(searchTerm))
				);
			}

			if (filterTransactionStatus.HasValue)
			{
				query = query.Where(t =>
					t.TourTrips.Any(tt =>
						tt.Payments.Any(p =>
							p.UserId == userId &&
							p.Transaction.Status == filterTransactionStatus.Value
						)
					)
				);
			}

			var tours = await query
					.OrderByDescending(t => t.TourTrips
						.SelectMany(tt => tt.Payments)
						.Where(p => p.UserId == userId)
						.Max(p => p.CreatedDate))
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();

			return tours;
		}
	}
}
