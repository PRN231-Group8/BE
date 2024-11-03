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
	[Route("api/tour-timestamps")]
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
		[ProducesResponseType(typeof(BaseResponse<List<TourTimeStampResponse>>), 200)]
		public async Task<IActionResult> GetAllTourTimeStamps(
		   [FromQuery(Name = "page-number")] int page = 1,
		   [FromQuery(Name = "page-size")] int pageSize = 10,
		   [FromQuery(Name = "sort-time")] TimeSpan? sortByTime = null,
		   [FromQuery(Name = "search-term")] string? searchTerm = null)
		{
			try
			{
				var zeroBasedPage = page - 1;
				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues();
				List<TourTimeStampResponse> items;
				int totalCount;

				// If data is found in cache, filter and return it
				if (cacheData.Count > 0)
				{
					var filteredData = cacheData.Values.AsQueryable();

					if (!string.IsNullOrEmpty(searchTerm))
					{
						filteredData = filteredData.Where(t => t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
					}

					totalCount = filteredData.Count();

					if (sortByTime.HasValue)
					{
						filteredData = filteredData.OrderBy(t => Math.Abs((t.PreferredTimeSlot.StartTime - sortByTime.Value).TotalMinutes));
					}
					else
					{
						filteredData = filteredData.OrderBy(t => t.PreferredTimeSlot.StartTime);
					}
					items = filteredData
						.Skip(zeroBasedPage * pageSize)
						.Take(pageSize)
						.ToList();
				}
				else
				{
					// If not in cache, query from TourTimeStampService
					var (serviceItems, serviceTotalCount) = await _tourTimeStampService.GetAllTourTimeStampAsync(page, pageSize, sortByTime, searchTerm);
					items = serviceItems;
					totalCount = serviceTotalCount;

					// Save the result to cache for future requests
					await Save(items).ConfigureAwait(false);
				}

				return Ok(new BaseResponse<TourTimeStampResponse>
				{
					IsSucceed = true,
					Results = items,
					TotalElements = totalCount,
					Message = items.Count > 0 ? "Tour timestamps retrieved successfully." : "No tour timestamps found.",
					Size = pageSize,
					Number = zeroBasedPage,
					TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
					NumberOfElements = items.Count,
					First = zeroBasedPage == 0,
					Last = zeroBasedPage >= (int)Math.Ceiling(totalCount / (double)pageSize) - 1,
					Empty = !items.Any(),
					Sort = new BaseResponse<TourTimeStampResponse>.SortInfo
					{
						Empty = false,
						Sorted = true,
						Unsorted = false
					}
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<List<object>>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving tour timestamps: {ex.Message}"
				});
			}
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(BaseResponse<List<TourTimeStampResponse>>), 200)]
		public async Task<IActionResult> GetTourTimeStampById(Guid id)
		{
			try
			{
				// Attempt to retrieve data from Redis cache
				var cacheData = GetKeyValues();

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
				if (tourTimeStamp == null)
				{
					return NotFound(new BaseResponse<TourTimeStampResponse>
					{
						IsSucceed = false,
						Message = $"TourTimestamp with ID {id} not found or has been deleted."
					});
				}

				// Save the result to cache for future requests
				await Save(new List<TourTimeStampResponse> { tourTimeStamp }).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = tourTimeStamp,
					Message = "Tour timestamp retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving the tour timestamp: {ex.Message}"
				});
			}
		}

		[HttpPost("{durationMinutes}")]
		[ProducesResponseType(typeof(BaseResponse<TourTimeStampResponse>), 201)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> CreateBatchTourTimeStamps([FromBody] List<TourTimeStampRequest> requests, int durationMinutes)
		{
			try
			{
				foreach (var request in requests)
				{
					// Validate the incoming request
					ValidationResult validationResult = await _validator.ValidateAsync(request);
					if (!validationResult.IsValid)
					{
						return BadRequest(new BaseResponse<object>
						{
							IsSucceed = false,
							Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
						});
					}
				}

				var results = await _tourTimeStampService.CreateBatchTourTimeStampsAsync(requests);

				var cacheData = GetKeyValues();
				foreach (var result in results)
				{
					cacheData[result.Id] = result;
				}
				await Save(cacheData.Values, durationMinutes).ConfigureAwait(false);

				return CreatedAtAction(nameof(GetAllTourTimeStamps), new BaseResponse<TourTimeStampResponse>
				{
					IsSucceed = true,
					Result = null,
					Message = "Tour timestamps created successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while creating the tour timestamps: {ex.Message}"
				});
			}
		}

		[HttpPut("{id}")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> UpdateTourTimeStamp(Guid id, [FromBody] TourTimeStampRequest request)
		{
			try
			{
				// Validate the incoming request
				ValidationResult validationResult = await _validator.ValidateAsync(request);
				if (!validationResult.IsValid)
				{
					return BadRequest(new BaseResponse<object>
					{
						IsSucceed = false,
						Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
					});
				}

				var updatedTourTimeStamp = await _tourTimeStampService.UpdateTourTimeStampAsync(id, request);

				if (updatedTourTimeStamp == null)
				{
					return NotFound(new BaseResponse<TourTimeStampResponse>
					{
						IsSucceed = false,
						Message = $"TourTimestamp with ID {id} not found or has been deleted."
					});
				}

				// Update the cache with the new post data
				var cacheData = GetKeyValues();
				cacheData[id] = updatedTourTimeStamp;
				await Save(cacheData.Values).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = null,
					Message = "Tour timestamp updated successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while updating the tour timestamp: {ex.Message}"
				});
			}
		}

		[HttpDelete("{id}")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> DeleteTourTimeStamp(Guid id)
		{
			try
			{
				var result = await _tourTimeStampService.DeleteAsync(id);

				// Remove the post from the list cache
				var cacheData = GetKeyValues();
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
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<bool>
				{
					IsSucceed = false,
					Message = $"An error occurred while deleting the tour timestamp: {ex.Message}"
				});
			}
		}

		private Task<bool> Save(IEnumerable<TourTimeStampResponse> tourTimeStamps, double expireAfterSeconds = 30)
		{
			// Set expiration time for the cache (default is 30 seconds)
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);

			// Save data to Redis cache
			return _cacheService.AddOrUpdateAsync(nameof(TourTimeStampResponse), tourTimeStamps, expirationTime);
		}

		private Dictionary<Guid, TourTimeStampResponse> GetKeyValues()
		{
			// Attempt to retrieve data from Redis cache
			var data = _cacheService.Get<IEnumerable<TourTimeStampResponse>>(nameof(TourTimeStampResponse));

			// Convert data to Dictionary or return empty Dictionary if no data
			return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, TourTimeStampResponse>();
		}
	}
}
