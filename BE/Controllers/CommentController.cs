﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Security.Claims;

namespace PRN231.ExploreNow.API.Controllers
{
	[Route("api/comments")]
	[ApiController]
	public class CommentController : ControllerBase
	{
		private readonly ICommentService _commentService;

		public CommentController(ICommentService commentService)
		{
			_commentService = commentService;
		}

		[HttpPost]
		[Authorize]
		[ProducesResponseType(typeof(BaseResponse<CommentResponse>), 201)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> AddComment([FromBody] CommentRequest model)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userId == null)
			{
				return Unauthorized(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = "User not authorized"
				});
			}

			try
			{
				var result = await _commentService.AddCommentAsync(userId, model);
				return Ok(new BaseResponse<CommentResponse>
				{
					IsSucceed = true,
					Result = null,
					Message = "Comment added successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = "Error adding comment",
					Result = ex.Message
				});
			}
		}

		[HttpGet("{id}/post")]
		[ProducesResponseType(typeof(BaseResponse<List<CommentResponse>>), 200)]
		public async Task<IActionResult> GetCommentsByPostId(Guid id)
		{
			try
			{
				var comments = await _commentService.GetCommentsByPostIdAsync(id);
				return Ok(new BaseResponse<List<CommentResponse>>
				{
					IsSucceed = true,
					Result = comments,
					Message = "Comments retrieved successfully"
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = "Error retrieving comments",
					Result = ex.Message
				});
			}
		}
	}
}
