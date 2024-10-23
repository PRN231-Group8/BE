using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.OtherObjects;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System.Security.Claims;

namespace PRN231.ExploreNow.Services.Services
{
    public class TourService : ITourService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _iUnitOfWork;
        private readonly IHttpContextAccessor _iContextAccessor;

        public TourService(IUnitOfWork iUnitOfWork, IHttpContextAccessor IContextAccessor, IMapper mapper)
        {
            _iUnitOfWork = iUnitOfWork;
            _iContextAccessor = IContextAccessor;
            _mapper = mapper;
        }

        public async Task Add(TourRequestModel tour)
        {
            var _tour = await MapToTourAsync(tour);
            string error = null;
            if (_tour.Transportations.Count == 0)
            {
                error = "Transportation not found ";
            }
            if (_tour.TourMoods.Count == 0)
            {
                error += " TourMoods not found ";
            }
            if (_tour.LocationInTours.Count == 0)
            {
                error += " Location in Tour not found";
                throw new CreateException(error);
            }
            await _iUnitOfWork.GetRepositoryByEntity<Tour>().AddAsync(_tour);
            await _iUnitOfWork.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await _iUnitOfWork.GetRepositoryByEntity<Tour>().DeleteAsync(id);
        }

        public async Task<TourResponse> GetById(Guid id)
        {
            var tour = await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetQueryable()
                .Where(t => t.Id == id && !t.IsDeleted)
                .Include(t => t.TourTrips)
                .Include(t => t.TourMoods)
                .Include(t => t.TourTimestamps)
                .Include(t => t.Transportations)
                .Include(t => t.LocationInTours)
                .FirstOrDefaultAsync();
            return _mapper.Map<TourResponse>(tour);
        }

        public async Task<List<TourResponse>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm)
        {
            return await _iUnitOfWork.GetRepository<ITourRepository>().GetToursAsync(page, pageSize, sortByStatus, searchTerm);
        }

        public async Task UpdateAsync(TourRequestModel tour, Guid id)
        {
            var _tour = await MapToTourAsync(tour);
            _tour.Id = id;
            await _iUnitOfWork.GetRepositoryByEntity<Tour>().UpdateAsync(_tour);
            await _iUnitOfWork.SaveChangesAsync();
        }

        private Tour MapToTour(TourRequestModel tour, List<Transportation> transportations, List<LocationInTour> locations, List<TourMood> tourMoods)
        {
            var currentUser = _iContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currUserName = _iContextAccessor.HttpContext?.User.Identity.Name;

            return new Tour
            {
                Code = GenerateUniqueCode(),
                CreatedBy = currUserName,
                CreatedDate = DateTime.Now,
                StartDate = tour.StartDate,
                EndDate = tour.EndDate,
                LastUpdatedBy = currUserName,
                LastUpdatedDate = DateTime.Now,
                IsDeleted = false,
                TotalPrice = tour.TotalPrice,
                Status = tour.Status,
                UserId = currentUser,
                Title = tour.Title,
                Description = tour.Description,
                Transportations = transportations,
                LocationInTours = locations,
                TourMoods = tourMoods,
            };
        }


        public async Task<Tour> MapToTourAsync(TourRequestModel tour)
        {
            var transportations = (await _iUnitOfWork.GetRepositoryByEntity<Transportation>().GetByIds(tour.Transports)).ToList();
            var locations = (await _iUnitOfWork.GetRepositoryByEntity<LocationInTour>().GetByIds(tour.LocationInTours)).ToList();
            var tourMoods = (await _iUnitOfWork.GetRepositoryByEntity<TourMood>().GetByIds(tour.TourMoods)).ToList();
            return MapToTour(tour, transportations, locations, tourMoods);
        }
        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }
}
