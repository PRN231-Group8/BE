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

        public async Task<TourResponse> CreateAsync(Tour tour)
        {
            Add(tour);
            await _dbContext.SaveChangesAsync();
            return MapToDto(tour);
        }

        public async Task<TourResponse> GetByIdAsync(Guid id)
        {
            var tour = await GetQueryable(t => t.Id == id && !t.IsDeleted)
                                .FirstOrDefaultAsync();
            return tour == null ? null : MapToDto(tour);
        }

        public async Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm)
        {
            var query = GetQueryable()
                        .Where(t => !t.IsDeleted);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.Title.Contains(searchTerm) || t.Description.Contains(searchTerm));
            }
            if (sortByStatus.HasValue)
            {
                query = query.OrderBy( t => t.Status == sortByStatus.Value);
            }
            var tours = await query.Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();
            return tours.ToList();
        }

        async Task<TourResponse> ITourRepository.UpdateTourAsync(Tour tour)
        {
            var _tour = await GetQueryable(t => t.Id == tour.Id && !t.IsDeleted)
                .FirstOrDefaultAsync();
            if (_tour == null)
            {
                return null;
            }
            UpdateTourProperties(_tour,tour);
            Update(_tour);
            await _dbContext.SaveChangesAsync();
            return MapToDto(_tour);
        }

        private void UpdateTourProperties(Tour oldTour, Tour newTour)
        {
            oldTour.Code = newTour.Code;
            oldTour.StartDate = newTour.StartDate;
            oldTour.EndDate = newTour.EndDate;
            oldTour.TotalPrice = newTour.TotalPrice;
            oldTour.Status = newTour.Status;
            oldTour.Title = newTour.Title;
            oldTour.Description = newTour.Description;
        }

        private TourResponse MapToDto(Tour tour)
        {
            return new TourResponse
            {
                Id = tour.Id,
                Code = tour.Code,
                StartDate = tour.StartDate,
                EndDate = tour.EndDate,
                TotalPrice = tour.TotalPrice,
                Status = tour.Status,
                Title = tour.Title,
                Description = tour.Description
            };
        }
    }
}
