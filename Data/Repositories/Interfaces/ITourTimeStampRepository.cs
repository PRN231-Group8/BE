using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface ITourTimeStampRepository : IBaseRepository<TourTimestamp>
	{
		Task<List<TourTimeStampResponse>> GetAllTourTimestampsAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm);
	}
}
