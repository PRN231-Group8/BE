using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface IPostsService
    {
        Task<List<PostsResponse>> GetAllPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm);
        Task<List<PostsResponse>> GetAllPendingPostsAsync(int page, int pageSize, string? searchTerm);
        Task<List<PostsResponse>> GetUserPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm);
        Task<PostsResponse> GetPostsByIdAsync(Guid postsId);
        Task<PostsResponse> UpdatePostsAsync(Guid postsId, PostsRequest postsRequest);
        Task<bool> DeletePostAsync(Guid postsId);
        Task<PostsResponse> CreatePost(CreatePostRequest createPostRequest);
        Task<Payment> HasValidPaymentAsync(string userId, Guid tourTripId);
    }
}
