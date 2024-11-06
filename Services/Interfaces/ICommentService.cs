using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface ICommentService
	{
		Task<CommentResponse> AddCommentAsync(string userId, CommentRequest model);
		Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid id);
	}
}
