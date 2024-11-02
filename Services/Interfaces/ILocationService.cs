using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces;

public interface ILocationService
{
	Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus,
		string? searchTerm);
	Task<LocationResponse> GetByIdAsync(Guid id);
	Task<LocationResponse> CreateAsync(LocationCreateRequest locationsRequest, List<IFormFile> files);
	Task<LocationResponse> UpdateAsync(Guid id, LocationsRequest locationsRequest, List<IFormFile> files);
	Task<bool> DeleteAsync(Guid id);
}