using AutoMapper;
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
		private readonly IMapper _mapper;

		public TourRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext)
		{
			_dbContext = dbContext;
			_mapper = mapper;
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
