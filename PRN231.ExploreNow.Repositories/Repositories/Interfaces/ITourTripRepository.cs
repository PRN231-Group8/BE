using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface ITourTripRepository : IBaseRepository<TourTrip>
	{
		Task<(List<TourTripResponse> Items, int TotalCount)> GetAllTourTripAsync(int page, int pageSize, bool? sortByPrice, string? searchTerm);
	}
}
