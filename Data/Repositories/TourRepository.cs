using AutoMapper;
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
		private readonly ApplicationDbContext _dbContext;
		private IMapper _mapper;
        public TourRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<TourResponse>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm)
		{
			var query = GetQueryable()
						.Where(t => !t.IsDeleted)
						.Include(t => t.TourTimestamps)
						.Include(t => t.TourMoods)
						.Include(t => t.LocationInTours)
						.Include(t => t.Transportations)
						.Include(t => t.TourTrips)
						.AsQueryable();
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
			

            return _mapper.Map<List<TourResponse>>(tours) ;
		}
	}
}
