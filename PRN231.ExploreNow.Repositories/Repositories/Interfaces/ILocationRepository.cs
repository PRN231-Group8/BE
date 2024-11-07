using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces;

public interface ILocationRepository : IBaseRepository<Location>
{
    Task<(List<LocationResponse> Items, int TotalCount)> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm);
}