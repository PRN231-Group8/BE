using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<LocationsRequest> _locationRequestValidator;

        public LocationService(IUnitOfWork unitOfWork, IValidator<LocationsRequest> locationRequestValidator)
        {
            _unitOfWork = unitOfWork;
            _locationRequestValidator = locationRequestValidator;
        }

        public async Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm)
        {
            return await _unitOfWork.LocationRepository.GetAllLocationsAsync(page, pageSize, sortByStatus, searchTerm);
        }

        public async Task<Location> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.LocationRepository.GetById(id);
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
            return new Location
            {
                Code = GenerateUniqueCode(),
                CreatedBy = "admin",
                CreatedDate = DateTime.Now,
                LastUpdatedBy = "admin",
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
                    CreatedBy = "admin",
                    CreatedDate = DateTime.Now,
                    LastUpdatedBy = "admin"
                }).ToList()
            };
        }

        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }
}
