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
		[Authorize(Roles = "CUSTOMER,MODERATOR,ADMIN")]
		public async Task<IActionResult> GetAllPosts(
			[FromQuery(Name = "filter-status")] PostsStatus? postsStatus,
			[FromQuery(Name = "search-term")] string? searchTerm,
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10)
		{
			try
			{
				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues();
				List<PostsResponse> result;

				// If data is found in cache, filter and return it
				if (cacheData.Count > 0)
				{
					var filteredData = cacheData.Values.AsQueryable();

					// Search by content or rating
					if (!string.IsNullOrEmpty(searchTerm))
					{
						filteredData = filteredData.Where(p => p.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
															   p.Rating.ToString().Contains(searchTerm));
					}

					// Filter by post status
					if (postsStatus.HasValue)
					{
						filteredData = filteredData.Where(p => p.Status == postsStatus.Value.ToString());
					}

					result = filteredData
						.Skip((page - 1) * pageSize)
						.Take(pageSize)
						.ToList();
				}
				else
				{
					// If not in cache, query from PostsService
					result = await _postsService.GetAllPostsAsync(page, pageSize, postsStatus, searchTerm);

					// Save the result to cache for future requests
					await Save(result).ConfigureAwait(false);
				}

				return Ok(new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = true,
					Result = result,
					Message = result.Count > 0 ? "Posts retrieved successfully." : "No posts found."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving posts: {ex.Message}"
				});
			}
		}

		[HttpGet("pending")]
		[Authorize(Roles = "MODERATOR,ADMIN")]
		public async Task<IActionResult> GetAllPendingPosts(
			[FromQuery(Name = "search-term")] string? searchTerm,
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10)
		{
			try
			{
				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues();
				List<PostsResponse> result;

				// If data is found in cache, filter and return it
				if (cacheData.Count > 0)
				{
					var filteredData = cacheData.Values
						.Where(p => p.Status.ToString() == PostsStatus.Pending.ToString())
						.AsQueryable();

					// Search by content
					if (!string.IsNullOrEmpty(searchTerm))
					{
						filteredData = filteredData.Where(p => p.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
					}

					result = filteredData
						.Skip((page - 1) * pageSize)
						.Take(pageSize)
						.ToList();
				}
				else
				{
					// If not in cache, query from PostsService
					result = await _postsService.GetAllPendingPostsAsync(page, pageSize, searchTerm);

					// Save the result to cache for future requests
					await Save(result).ConfigureAwait(false);
				}

				return Ok(new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = true,
					Result = result,
					Message = result.Count > 0 ? "Pending posts retrieved successfully." : "No pending posts found."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving pending posts: {ex.Message}"
				});
			}
		}

		[HttpGet("history")]
		[Authorize(Roles = "CUSTOMER,MODERATOR,ADMIN")]
		public async Task<IActionResult> GetUserPosts(
			[FromQuery(Name = "filter-status")] PostsStatus? postsStatus,
			[FromQuery(Name = "search-term")] string? searchTerm,
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10)
		{
			try
			{
				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues();
				List<PostsResponse> result;

				// If data is found in cache, filter and return it
				if (cacheData.Count > 0)
				{
					var filteredData = cacheData.Values.AsQueryable();

					// Search by content
					if (!string.IsNullOrEmpty(searchTerm))
					{
						filteredData = filteredData.Where(p => p.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
					}

					// Filter by post status
					if (postsStatus.HasValue)
					{
						filteredData = filteredData.Where(p => p.Status == postsStatus.Value.ToString());
					}

					result = filteredData
						.Skip((page - 1) * pageSize)
						.Take(pageSize)
						.ToList();
				}
				else
				{
					// If not in cache, query from PostsService
					result = await _postsService.GetUserPostsAsync(page, pageSize, postsStatus, searchTerm);

					// Save the result to cache for future requests
					await Save(result).ConfigureAwait(false);
				}

				return Ok(new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = true,
					Result = result,
					Message = result.Count > 0 ? "User posts retrieved successfully." : "No posts found for this user."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<List<PostsResponse>>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving user posts: {ex.Message}"
				});
			}
		}

		[HttpGet("{id}")]
		[Authorize(Roles = "CUSTOMER,MODERATOR,ADMIN")]
		public async Task<IActionResult> GetPostsById(Guid id)
		{
			try
			{
				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues();

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
				if (post == null)
				{
					return NotFound(new BaseResponse<object>
					{
						IsSucceed = false,
						Result = null,
						Message = $"Post with Id {id} not found."
					});
				}

				// Save the result to cache for future requests
				await Save(new List<PostsResponse> { post }).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = post,
					Message = "Post retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Result = null,
					Message = ex.Message
				});
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "MODERATOR,ADMIN")]
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
				var cacheData = GetKeyValues();
				cacheData[id] = updatedPost;
				await Save(cacheData.Values).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = null,
					Message = "Posts updated successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while updating the posts: {ex.Message}"
				});
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "CUSTOMER,MODERATOR")]
		public async Task<IActionResult> DeletePosts(Guid id)
		{
			try
			{
				var result = await _postsService.DeletePostAsync(id);

				// Remove the post from the list cache
				var cacheData = GetKeyValues();
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
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<bool>
				{
					IsSucceed = false,
					Message = $"An error occurred while deleting the post: {ex.Message}"
				});
			}
		}

		private Task<bool> Save(IEnumerable<PostsResponse> posts, double expireAfterSeconds = 30)
		{
			// Set expiration time for the cache (default is 30 seconds)
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);

			// Save data to Redis cache
			return _cacheService.AddOrUpdateAsync(nameof(PostsResponse), posts, expirationTime);
		}

		private Dictionary<Guid, PostsResponse> GetKeyValues()
		{
			// Attempt to retrieve data from Redis cache
			var data = _cacheService.Get<IEnumerable<PostsResponse>>(nameof(PostsResponse));

			// Convert data to Dictionary or return empty Dictionary if no data
			return data?.ToDictionary(key => key.PostsId, val => val) ?? new Dictionary<Guid, PostsResponse>();
		}

		[HttpPost]
		[Authorize(Roles = "CUSTOMER,MODERATOR")]
		public async Task<IActionResult> CreatePost([FromForm] CreatePostRequest createPostRequest)
		{
			try
			{
				// Validate the number of uploaded images
				if (createPostRequest.Photos.Count > 5)
				{
					return BadRequest(new BaseResponse<object>
					{
						IsSucceed = false,
						Message = "You can upload up to 5 images only."
					});
				}

				// Validate file sizes
				foreach (var file in createPostRequest.Photos)
				{
					if (file.Length > 3 * 1024 * 1024) // 3MB limit
					{
						return BadRequest(new BaseResponse<object>
						{
							IsSucceed = false,
							Message = $"File {file.FileName} exceeds the 3MB size limit."
						});
					}
				}

				// Call the service to create the post with images
				var result = await _postsService.CreatePost(createPostRequest);

				return Ok(new BaseResponse<PostsResponse>
				{
					IsSucceed = true,
					Result = result,
					Message = "Post created successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while creating the post: {ex.InnerException?.Message ?? ex.Message}"
				});
			}
		}
	}
}
