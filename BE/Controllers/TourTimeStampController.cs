using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
	[ApiController]
	[Route("api/tourtimestamps")]
	public class TourTimeStampController : ControllerBase
	{
		private readonly ITourTimeStampService _tourTimeStampService;
		private readonly IValidator<TourTimeStampRequest> _validator;

		public TourTimeStampController(ITourTimeStampService tourTimeStampService, IValidator<TourTimeStampRequest> validator)
		{
			_tourTimeStampService = tourTimeStampService;
			_validator = validator;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllTourTimeStamps(
		   [FromQuery] int page = 1,
		   [FromQuery] int pageSize = 10,
		   [FromQuery] TimeSpan? sortByTime = null,
		   [FromQuery] string? searchTerm = null)
		{
			try
			{
				var result = await _tourTimeStampService.GetAllTourTimeStampAsync(page, pageSize, sortByTime, searchTerm);
				return Ok(new BaseResponse<TourTimeStampResponse>
				{
					IsSucceed = true,
					Results = result,
					Message = "Tour timestamps retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<List<object>>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving tour timestamps: {ex.Message}"
				});
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetTourTimeStampById(Guid id)
		{
			try
			{
				var tourTimeStamp = await _tourTimeStampService.GetTourTimeStampByIdAsync(id);
				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = tourTimeStamp,
					Message = "Tour timestamp retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving the tour timestamp: {ex.Message}"
				});
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateMultipleTourTimeStamps([FromBody] List<TourTimeStampRequest> requests)
		{
			try
			{
				foreach (var request in requests)
				{
					ValidationResult validationResult = await _validator.ValidateAsync(request);
					if (!validationResult.IsValid)
					{
						return Ok(new BaseResponse<object>
						{
							IsSucceed = false,
							Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
						});
					}
				}

				var results = await _tourTimeStampService.CreateMultipleTourTimeStampsAsync(requests);
				return CreatedAtAction(nameof(GetAllTourTimeStamps), new BaseResponse<TourTimeStampResponse>
				{
					IsSucceed = true,
					Results = results,
					Message = "Tour timestamps created successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while creating the tour timestamps: {ex.Message}"
				});
			}
		}


		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateTourTimeStamp(Guid id, [FromBody] TourTimeStampRequest request)
		{
			try
			{
				ValidationResult validationResult = await _validator.ValidateAsync(request);
				if (!validationResult.IsValid)
				{
					return Ok(new BaseResponse<object>
					{
						IsSucceed = false,
						Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
					});
				}

				var updatedTourTimeStamp = await _tourTimeStampService.UpdateTourTimeStampAsync(id, request);
				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = updatedTourTimeStamp,
					Message = "Tour timestamp updated successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while updating the tour timestamp: {ex.Message}"
				});
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTourTimeStamp(Guid id)
		{
			try
			{
				var result = await _tourTimeStampService.DeleteAsync(id);
				return Ok(new BaseResponse<bool>
				{
					IsSucceed = true,
					Result = true,
					Message = "Tour timestamp deleted successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<bool>
				{
					IsSucceed = false,
					Message = $"An error occurred while deleting the tour timestamp: {ex.Message}"
				});
			}
		}
	}
}
