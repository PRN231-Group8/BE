using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using System.Collections.Generic;
namespace PRN231.ExploreNow.Repositories.Repositories.Interface;

public interface ILocationRepository : IBaseRepository<Location>
{
    Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm);
    Task<LocationResponse> GetByIdAsync(Guid id);
    Task<LocationResponse> CreateAsync(Location location);
    Task<LocationResponse> UpdateAsync(Location location);
    
}