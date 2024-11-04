using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

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
					Result = result,
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
	}
}
