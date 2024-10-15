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

		public PostsController(IPostsService postsService, IValidator<PostsRequest> postsValidator)
		{
			_postsService = postsService;
			_postsValidator = postsValidator;
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
				var posts = await _postsService.GetAllPostsAsync(page, pageSize, sortPostsStatus, searchTerm);
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
				var query = await _postsService.GetPostsByIdAsync(id);
				var result = new BaseResponse<object>
				{
					IsSucceed = true,
					Result = query,
					Message = "Posts retrieved successfully."
				};
				return Ok(result);
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

				var updatedPost = await _postsService.UpdatePostsAsync(id, postsRequest);
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
	}
}
