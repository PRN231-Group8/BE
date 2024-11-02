using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface IMoodRepository : IBaseRepository<Moods>
	{
		Task<(List<Moods> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? searchTerms);
	}
}
