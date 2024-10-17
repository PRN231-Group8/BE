using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface ITourTimeStampService
	{
		Task<List<TourTimeStampResponse>> GetAllTourTimeStampAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm);
		Task<TourTimeStampResponse> GetTourTimeStampByIdAsync(Guid tourTimeStampId);
		Task<List<TourTimeStampResponse>> CreateBatchTourTimeStampsAsync(List<TourTimeStampRequest> tourTimeStampRequests);
		Task<TourTimeStampResponse> UpdateTourTimeStampAsync(Guid tourTimeStampId, TourTimeStampRequest tourTimeStampRequest);
		Task<bool> DeleteAsync(Guid tourTimeStampId);
	}
}
