using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class TourTripRepository : BaseRepository<TourTrip>, ITourTripRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public TourTripRepository(ApplicationDbContext context, IMapper mapper) : base(context)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<(List<TourTripResponse> Items, int TotalCount)> GetAllTourTripAsync(int page, int pageSize, bool? sortByPrice, string? searchTerm)
		{
			var currentDate = DateTime.Now.Date;
			var query = GetQueryable(tt => !tt.IsDeleted)
						.AsSplitQuery()
						.Include(tt => tt.Tour)
						.AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(tt =>
					tt.TripStatus.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
			}

			var totalCount = await query.CountAsync();
			var tourTrips = await query.ToListAsync();

			// Apply sorting and pagination
			var sortedAndPaginatedTrips = ApplySortingAndPaginationToEntity(tourTrips, sortByPrice, page, pageSize);

			var mappedResults = _mapper.Map<List<TourTripResponse>>(sortedAndPaginatedTrips);
			return (mappedResults, totalCount);
		}

		private List<TourTrip> ApplySortingAndPaginationToEntity(List<TourTrip> tourTrips, bool? sortByPrice, int page, int pageSize)
		{
			var currentDate = DateTime.Now.Date;

			// Apply sorting
			if (sortByPrice.HasValue)
			{
				tourTrips = sortByPrice.Value
					? tourTrips.OrderBy(tt => tt.Price)
							  .ThenBy(tt => tt.TripDate < currentDate)
							  .ThenBy(tt => Math.Abs((tt.TripDate - currentDate).TotalDays))
							  .ThenByDescending(tt => tt.CreatedDate)
							  .ToList()
					: tourTrips.OrderByDescending(tt => tt.Price)
							  .ThenBy(tt => tt.TripDate < currentDate)
							  .ThenBy(tt => Math.Abs((tt.TripDate - currentDate).TotalDays))
							  .ThenByDescending(tt => tt.CreatedDate)
							  .ToList();
			}
			else
			{
				tourTrips = tourTrips.OrderBy(tt => tt.TripDate < currentDate)
									.ThenBy(tt => Math.Abs((tt.TripDate - currentDate).TotalDays))
									.ThenByDescending(tt => tt.CreatedDate)
									.ThenBy(tt => tt.Payments.Any(p => p.Status == PaymentStatus.PENDING))
									.ToList();
			}

			// Apply pagination
			return tourTrips.Skip((page - 1) * pageSize)
						   .Take(pageSize)
						   .ToList();
		}
	}
}
