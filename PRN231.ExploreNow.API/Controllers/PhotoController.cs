using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers
{
	[Route("api/photos")]
	[ApiController]
	public class PhotoController : ControllerBase
	{
		private readonly IPhotoService _photoService;

		public PhotoController(IPhotoService photoService)
		{
			_photoService = photoService;
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetPhotoById(Guid id)
		{
			try
			{
				var result = await _photoService.GetPhotoByIdAsync(id);
				var response = new BaseResponse<PhotoResponse>
				{
					IsSucceed = true,
					Result = result,
					Message = "Photo retrieved successfully"
				};

				return Ok(response);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new BaseResponse<PhotoResponse>
				{
					IsSucceed = false,
					Message = ex.Message
				});
			}
		}

		[HttpPut]
		[Authorize]
		public async Task<IActionResult> UpdatePhoto([FromForm] UpdatePhotoRequest updatePhotoRequest)
		{
			// Validate file presence and size
			if (updatePhotoRequest.File == null || updatePhotoRequest.File.Length == 0)
			{
				return BadRequest(new BaseResponse<PhotoResponse>
				{
					IsSucceed = false,
					Message = "Invalid image file."
				});
			}

			try
			{
				// Call the service with the properties from updatePhotoRequest
				var result = await _photoService.UpdatePhotoAsync(updatePhotoRequest.PhotoId, updatePhotoRequest.PostId, updatePhotoRequest.File);

				var response = new BaseResponse<PhotoResponse>
				{
					IsSucceed = true,
					Result = result,
					Message = "Photo updated successfully"
				};

				return Ok(response);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new BaseResponse<PhotoResponse>
				{
					IsSucceed = false,
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<PhotoResponse>
				{
					IsSucceed = false,
					Message = ex.Message
				});
			}
		}
	}
}
