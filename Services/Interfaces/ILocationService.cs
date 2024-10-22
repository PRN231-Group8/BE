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
	Task<LocationResponse> CreateAsync(LocationsRequest locationsRequest);
	Task<LocationResponse> UpdateAsync(Guid id, LocationsRequest locationsRequest);
	Task<bool> DeleteAsync(Guid id);
}