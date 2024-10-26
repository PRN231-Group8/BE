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
    }
}
