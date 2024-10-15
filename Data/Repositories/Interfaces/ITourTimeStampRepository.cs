using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface ITourTimeStampRepository : IBaseRepository<TourTimestamp>
	{
		Task<List<TourTimeStampResponse>> GetAllTourTimestampsAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm);
		Task<TourTimeStampResponse> GetByIdAsync(Guid id);
		Task<List<TourTimeStampResponse>> CreateMultipleAsync(List<TourTimestamp> tourTimestamps);
		Task<TourTimeStampResponse> UpdateAsync(TourTimestamp tourTimestamp, TourTimeStampRequest tourTimeStampRequest);
	}
}
