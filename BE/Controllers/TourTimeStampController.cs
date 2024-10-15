using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
	[ApiController]
	[Route("api/tourtimestamps")]
	[Authorize(Roles = "ADMIN")]
	public class TourTimeStampController : ControllerBase
	{
		private readonly ITourTimeStampService _tourTimeStampService;
		private readonly IValidator<TourTimeStampRequest> _validator;
		private readonly ICacheService _cacheService;

		public TourTimeStampController(ITourTimeStampService tourTimeStampService, IValidator<TourTimeStampRequest> validator, ICacheService cacheService)
		{
			_tourTimeStampService = tourTimeStampService;
			_validator = validator;
			_cacheService = cacheService;
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
				// Create a unique cache key based on query parameters
				var cacheKey = $"TourTimeStamps_{page}_{pageSize}_{sortByTime}_{searchTerm}";

				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues(cacheKey);

				// If data is found in cache, return it immediately
				if (cacheData.Any()) return Ok(new BaseResponse<List<TourTimeStampResponse>>
				{
					IsSucceed = true,
					Result = cacheData.Values.ToList(),
					Message = "Tour timestamps retrieved from cache successfully."
				});

				// If not in cache, query from TourTimeStampService
				var tourTimeStamps = await _tourTimeStampService.GetAllTourTimeStampAsync(page, pageSize, sortByTime, searchTerm);

				// Save the result to cache for future requests
				await Save(tourTimeStamps, cacheKey).ConfigureAwait(false);

				return Ok(new BaseResponse<TourTimeStampResponse>
				{
					IsSucceed = true,
					Results = tourTimeStamps,
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
				// Create a cache key for the specific post
				var cacheKey = $"TourTimeStamps_{id}";

				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues(cacheKey);

				// If the post is found in cache, return it immediately
				if (cacheData.TryGetValue(id, out var cachedTourtimestamp))
				{
					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Result = cachedTourtimestamp,
						Message = "Tour timestamp retrieved from cache successfully."
					});
				}

				// If not in cache, query from TourTimeStampService
				var tourTimeStamp = await _tourTimeStampService.GetTourTimeStampByIdAsync(id);

				// Save the result to cache for future requests
				await Save(new List<TourTimeStampResponse> { tourTimeStamp }, cacheKey).ConfigureAwait(false);

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
					// Validate the incoming request
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
				// Validate the incoming request
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

				// Update the cache with the new post data
				var cacheData = GetKeyValues("TourTimeStamps");
				cacheData[id] = updatedTourTimeStamp;
				await Save(cacheData.Values).ConfigureAwait(false);

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

				// Remove the post from the list cache
				var cacheData = GetKeyValues("TourTimeStamps");
				cacheData.Remove(id);
				await Save(cacheData.Values).ConfigureAwait(false);

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

		private Task<bool> Save(IEnumerable<TourTimeStampResponse> tourTimeStamps, string cacheKey = "TourTimeStamps", double expireAfterSeconds = 300)
		{
			// Set expiration time for the cache (default is 5 minutes)
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);

			// Save data to Redis cache
			return _cacheService.AddOrUpdateAsync(cacheKey, tourTimeStamps, expirationTime);
		}

		private Dictionary<Guid, TourTimeStampResponse> GetKeyValues(string cacheKey = "TourTimeStamps")
		{
			// Attempt to retrieve data from Redis cache
			var data = _cacheService.Get<IEnumerable<TourTimeStampResponse>>(cacheKey);

			// Convert data to Dictionary or return empty Dictionary if no data
			return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, TourTimeStampResponse>();
		}
	}
}
