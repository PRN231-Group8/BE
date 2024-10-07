using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public LocationService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public async Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm)
        {
            return await _unitOfWork.LocationRepository.GetAllLocationsAsync(page, pageSize, sortByStatus, searchTerm);
        }

        public async Task<LocationResponse> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.LocationRepository.GetByIdAsync(id);
        }

        public async Task<LocationResponse> CreateAsync(LocationsRequest locationsRequest)
        {
            var location = MapToLocation(locationsRequest);
            return await _unitOfWork.LocationRepository.CreateAsync(location);
        }

        public async Task<LocationResponse> UpdateAsync(Guid id, LocationsRequest locationsRequest)
        {
            var location = MapToLocation(locationsRequest);
            location.Id = id;
            return await _unitOfWork.LocationRepository.UpdateAsync(location);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _unitOfWork.LocationRepository.DeleteAsync(id);
        }

        private Location MapToLocation(LocationsRequest locationsRequest)
        {
            var currentUser = _userManager.GetUserAsync(_contextAccessor.HttpContext.User).Result;
            var currentUserName = currentUser?.UserName ?? "Admin";   
            return new Location
            {
                Code = GenerateUniqueCode(),
                CreatedBy = currentUserName,
                CreatedDate = DateTime.Now,
                LastUpdatedBy = currentUserName,
                Name = locationsRequest.Name,
                Description = locationsRequest.Description,
                Address = locationsRequest.Address,
                Status = locationsRequest.Status,
                Temperature = locationsRequest.Temperature,
                Photos = locationsRequest.Photos.Select(p => new Photo
                {
                    Url = p.Url,
                    Alt = p.Alt,
                    Code = GenerateUniqueCode(),
                    CreatedBy = currentUserName,
                    CreatedDate = DateTime.Now,
                    LastUpdatedBy = currentUserName
                }).ToList()
            };
        }

        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }
}
