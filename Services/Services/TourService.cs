using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System.Security.Claims;

namespace PRN231.ExploreNow.Services.Services
{
    public class TourService : ITourService
    {
        private IUnitOfWork _iUnitOfWork;
        private IHttpContextAccessor _iContextAccessor;

        public TourService(IUnitOfWork iUnitOfWork, IHttpContextAccessor IContextAccessor)
        {
            _iUnitOfWork = iUnitOfWork;
            _iContextAccessor = IContextAccessor;
        }

        public async Task Add(TourRequestModel tour)
        {
            var _tour = MapToTour(tour);
            await _iUnitOfWork.TourRepository.CreateAsync(_tour);
        }

        public async Task Delete(Guid id)
        {
            await _iUnitOfWork.GetRepositoryByEntity<Tour>().DeleteAsync(id);
        }

        public async Task<IList<Tour>> GetAll()
        {
            return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetAll();
        }

        public async Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm)
        {
            return await _iUnitOfWork.TourRepository.GetToursAsync(page, pageSize, sortByStatus, searchTerm);   
        }

        public async Task<Tour> Update(TourRequestModel tour, Guid id)
        {
            var _tour = MapToTour(tour);
            _tour.Id = id;
            _iUnitOfWork.GetRepositoryByEntity<Tour>().Update(_tour);
            await _iUnitOfWork.SaveChangesAsync();
            return _tour;
        }

        public async Task<TourResponse> UpdateAsync(TourRequestModel tour, Guid id)
        {
            var _tour = MapToTour(tour);
            _tour.Id = id;
            return await _iUnitOfWork.TourRepository.UpdateAsync(_tour);
        }

        async Task<TourResponse> ITourService.GetById(Guid id)
        {
            return await _iUnitOfWork.TourRepository.GetByIdAsync(id);
        }

        private Tour MapToTour(TourRequestModel tour)
        {
            var currentUserId = _iContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return new Tour
            {
                Code = tour.Code,
                CreatedBy = /*currentUserId*/"Admin",
                CreatedDate = DateTime.Now,
                StartDate = tour.StartDate,
                EndDate = tour.EndDate,
                LastUpdatedBy = /*currentUserId*/"Admin",
                LastUpdatedDate = DateTime.Now,
                IsDeleted = false,
                TotalPrice = tour.TotalPrice,
                Status = tour.Status,
                UserId = "87c47c39-15ae-4208-8a78-53cf7dc6c480",
                Title = tour.Title,
                Description = tour.Description,
            };
        }
    }
}
