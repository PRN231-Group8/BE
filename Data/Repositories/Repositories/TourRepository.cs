using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
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

        public async Task<(List<TourResponse> Items, int TotalCount)> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm)
        {
            var query = GetQueryable()
                .AsSplitQuery()
                .Where(t => !t.IsDeleted)
                .Include(t => t.TourTimestamps.Where(tt => !tt.IsDeleted))
                .Include(t => t.TourMoods.Where(tm => !tm.IsDeleted && !tm.Mood.IsDeleted))
                    .ThenInclude(tm => tm.Mood)
                .Include(t => t.LocationInTours.Where(lit => !lit.IsDeleted && !lit.Location.IsDeleted))
                    .ThenInclude(lit => lit.Location)
                    .ThenInclude(l => l.Photos.Where(p => !p.IsDeleted))
                .Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
                .Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
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

            var totalCount = await query.CountAsync();

            if (sortByStatus.HasValue)
            {
                filteredTours = filteredTours.OrderBy(t => t.Status == sortByStatus.Value);
            }

            var pagedTours = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedTours = _mapper.Map<List<TourResponse>>(pagedTours);

            return (mappedTours, totalCount);
        }

        public async Task<(List<Tour> Items, int TotalCount)> GetTourBookingHistoryAsync(
            string userId,
            int page,
            int pageSize,
            PaymentTransactionStatus? filterTransactionStatus,
            string? searchTerm = null)
        {
            var query = GetQueryable()
                .Where(t => !t.IsDeleted)
                .Where(t => t.TourTrips.Any(tt => !tt.IsDeleted && tt.Payments.Any(p => !p.IsDeleted && p.UserId == userId)))
                .Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
                    .ThenInclude(tt => tt.Payments.Where(p => !p.IsDeleted && p.UserId == userId))
                    .ThenInclude(p => p.Transaction)
                .AsSplitQuery();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm) && decimal.TryParse(searchTerm, out decimal searchPrice))
            {
                query = query.Where(t => t.TotalPrice == searchPrice);
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

            var totalCount = await query.CountAsync();

            var tours = await query
                    .OrderByDescending(t => t.TourTrips
                        .SelectMany(tt => tt.Payments)
                        .Where(p => p.UserId == userId)
                        .Max(p => p.CreatedDate))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return (tours, totalCount);
        }
    }
}
