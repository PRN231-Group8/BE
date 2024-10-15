using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
	[ApiController]
	[Route("api/posts")]
	[Authorize(Roles = "MODERATOR")]
	public class PostsController : ControllerBase
	{
		private readonly IPostsService _postsService;
		private readonly IValidator<PostsRequest> _postsValidator;
		private readonly ICacheService _cacheService;
		public PostsController(IPostsService postsService, IValidator<PostsRequest> postsValidator, ICacheService cacheService)
		{
			_postsService = postsService;
			_postsValidator = postsValidator;
			_cacheService = cacheService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllPosts(
			[FromQuery] PostsStatus? sortPostsStatus,
			[FromQuery] string? searchTerm,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			try
			{
				// Create a unique cache key based on query parameters
				var cacheKey = $"Posts_{sortPostsStatus}_{searchTerm}_{page}_{pageSize}";

				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues(cacheKey);

				// If data is found in cache, return it immediately
				if (cacheData.Any()) return Ok(new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = true,
					Result = cacheData.Values.ToList(),
					Message = "Posts retrieved from cache successfully."
				});

				// If no data in cache, query from PostsService
				var posts = await _postsService.GetAllPostsAsync(page, pageSize, sortPostsStatus, searchTerm);

				// Save the result to cache for future requests
				await Save(posts, cacheKey).ConfigureAwait(false);

				return Ok(new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = true,
					Result = posts,
					Message = "Posts retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving posts: {ex.Message}"
				});
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetPostsById(Guid id)
		{
			try
			{
				// Create a cache key for the specific post
				var cacheKey = $"Post_{id}";

				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues(cacheKey);

				// If the post is found in cache, return it immediately
				if (cacheData.TryGetValue(id, out var cachedPost))
				{
					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Result = cachedPost,
						Message = "Post retrieved from cache successfully."
					});
				}

				// If not in cache, query from PostsService
				var post = await _postsService.GetPostsByIdAsync(id);

				// Save the result to cache for future requests
				await Save(new List<PostsResponse> { post }, cacheKey).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = post,
					Message = "Post retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Result = null,
					Message = ex.Message
				});
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdatePosts(Guid id, [FromBody] PostsRequest postsRequest)
		{
			try
			{
				// Validate the incoming request
				ValidationResult validationResult = await _postsValidator.ValidateAsync(postsRequest);
				if (!validationResult.IsValid)
				{
					var validationErrors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
					return Ok(new BaseResponse<object>
					{
						IsSucceed = false,
						Message = $"Validation failed: {validationErrors}"
					});
				}

				// Update the post in the database
				var updatedPost = await _postsService.UpdatePostsAsync(id, postsRequest);

				// Update the cache with the new post data
				var cacheData = GetKeyValues("Posts");
				cacheData[id] = updatedPost;
				await Save(cacheData.Values).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = updatedPost,
					Message = "Posts updated successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while updating the posts: {ex.Message}"
				});
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePosts(Guid id)
		{
			try
			{
				var result = await _postsService.DeletePostAsync(id);

				// Remove the post from the list cache
				var cacheData = GetKeyValues("Posts");
				cacheData.Remove(id);
				await Save(cacheData.Values).ConfigureAwait(false);

				return Ok(new BaseResponse<bool>
				{
					IsSucceed = true,
					Result = true,
					Message = "Post deleted successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<bool>
				{
					IsSucceed = false,
					Message = $"An error occurred while deleting the post: {ex.Message}"
				});
			}
		}

		private Task<bool> Save(IEnumerable<PostsResponse> posts, string cacheKey = "Posts", double expireAfterSeconds = 300)
		{
			// Set expiration time for the cache (default is 5 minutes)
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);

			// Save data to Redis cache
			return _cacheService.AddOrUpdateAsync(cacheKey, posts, expirationTime);
		}

		private Dictionary<Guid, PostsResponse> GetKeyValues(string cacheKey = "Posts")
		{
			// Attempt to retrieve data from Redis cache
			var data = _cacheService.Get<IEnumerable<PostsResponse>>(cacheKey);

			// Convert data to Dictionary or return empty Dictionary if no data
			return data?.ToDictionary(key => key.PostsId, val => val) ?? new Dictionary<Guid, PostsResponse>();
		}
	}
}
