using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface IPostsRepository : IBaseRepository<Posts>
	{
		Task<List<PostsResponse>> GetAllPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm);
	}
}
