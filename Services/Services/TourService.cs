using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
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
            var id = _iContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Tour _tour = new Tour
            {
                Code = tour.Code,
                CreatedBy = id,
                CreatedDate = DateTime.Now,
                StartDate = tour.StartDate,
                EndDate = tour.EndDate,
                LastUpdatedBy = id,
                LastUpdatedDate = DateTime.Now,
                IsDeleted = false,
                TotalPrice = tour.TotalPrice,
                Status = tour.Status,
                UserId = id,
                Title = tour.Title,
                Description = tour.Description,
            };

            _iUnitOfWork.GetRepositoryByEntity<Tour>().Add(_tour);

            _iUnitOfWork.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            Tour tour = await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetById(id);

            _iUnitOfWork.GetRepositoryByEntity<Tour>().Delete(tour);
            _iUnitOfWork.SaveChangesAsync();
        }

        public async Task<IList<Tour>> GetAll()
        {
            return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetAll();
        }

        public async Task<Tour> GetById(Guid id)
        {
            return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetById(id);
        }

        public async Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm)
        {
            var tourRepo = _iUnitOfWork.GetRepository<ITourRepository>();

            return await tourRepo.GetToursAsync(page, pageSize, sortByStatus, searchTerm);
        }

        public async Task<Tour> Update(TourRequestModel tour, Guid id)
        {
            var _tour = MapToTour(tour);
            _tour.Id = id;
            _iUnitOfWork.GetRepositoryByEntity<Tour>().Update(_tour);
            await _iUnitOfWork.SaveChangesAsync();
            return _tour;
        }

        private Tour MapToTour(TourRequestModel tour)
        {
            var currentUserId = _iContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return new Tour
            {
                Code = tour.Code,
                StartDate = tour.StartDate,
                EndDate = tour.EndDate,
                LastUpdatedBy = currentUserId,
                LastUpdatedDate = DateTime.Now,
                IsDeleted = false,
                TotalPrice = tour.TotalPrice,
                Status = tour.Status,
                UserId = currentUserId,
                Title = tour.Title,
                Description = tour.Description,
            };
        }
    }
}
