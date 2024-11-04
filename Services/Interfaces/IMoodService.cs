using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface IMoodService
	{
		Task<(List<MoodResponse> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? searchTerm);
		Task<MoodResponse> GetById(Guid id);
		Task<MoodResponse> Add(MoodRequest moods);
		Task Update(MoodRequest moods, Guid id);
		Task Delete(Guid id);
	}
}
