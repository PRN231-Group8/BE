using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interface;

namespace PRN231.ExploreNow.Repositories.Repositories
{
	public class LocationRepository : BaseRepository<Location>, ILocationRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public LocationRepository(ApplicationDbContext context, IMapper mapper) : base(context)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<(List<LocationResponse> Items, int TotalCount)> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm)
		{
			var query = GetQueryable()
				.Include(l => l.Photos)
				.Where(l => !l.IsDeleted);

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(l => l.Name.Contains(searchTerm) ||
										 l.Description.Contains(searchTerm));
			}

			if (sortByStatus.HasValue)
			{
				query = query.OrderBy(l => l.Status == sortByStatus.Value);
			}

			var totalCount = await query.CountAsync();

			var locations = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var locationResponses = _mapper.Map<List<LocationResponse>>(locations);

			return (locationResponses, totalCount);
		}
	}
}