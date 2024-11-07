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
	[Route("api/tour-trips")]
	public class TourTripController : ControllerBase
	{
		private readonly ITourTripService _tourTripService;
		private readonly IValidator<TourTripRequest> _validator;
		private readonly ICacheService _cacheService;

		public TourTripController(ITourTripService tourTripService, IValidator<TourTripRequest> validator, ICacheService cacheService)
		{
			_tourTripService = tourTripService;
			_validator = validator;
			_cacheService = cacheService;
		}

		[HttpGet]
		[ProducesResponseType(typeof(BaseResponse<List<TourTripResponse>>), 200)]
		public async Task<IActionResult> GetTourTrips(
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10,
			[FromQuery(Name = "sort-by-price")] bool? sortByPrice = null,
			[FromQuery(Name = "search-term")] string? searchTerm = null)
		{
			try
			{
				var cacheData = GetKeyValues();
				IQueryable<TourTripResponse> filteredData;
				if (cacheData.Count > 0)
				{
					filteredData = cacheData.Values.AsQueryable();

					// Apply search filter for status
					if (!string.IsNullOrEmpty(searchTerm))
						filteredData = filteredData.Where(t =>
							t.TripStatus.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

					var totalElements = filteredData.Count();

					var result = ApplySortingAndPagination(filteredData, sortByPrice, page, pageSize);

					return Ok(new BaseResponse<TourTripResponse>(
						result,
						totalElements,
						page,
						pageSize,
						result.Any() ? "Tour trips retrieved from cache successfully." : "No tour trips found."));
				}
				else
				{
					var (serviceItems, serviceTotalCount) = await _tourTripService.GetAllTourTripAsync(
						page, pageSize, sortByPrice, searchTerm);

					await Save(serviceItems);
					return Ok(new BaseResponse<TourTripResponse>(
						serviceItems,
						serviceTotalCount,
						page,
						pageSize,
						serviceItems.Any() ? "Tour trips retrieved from cache successfully." : "No tour trips found."));
				}
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"Error: {ex.Message}"
				});
			}
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(BaseResponse<TourTripResponse>), 200)]
		public async Task<IActionResult> GetTourTripById(Guid id)
		{
			try
			{
				var cacheData = GetKeyValues();

				if (cacheData.TryGetValue(id, out var cachedTourTrip))
				{
					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Result = cachedTourTrip,
						Message = "Tour trip retrieved from cache successfully."
					});
				}

				var tourTrip = await _tourTripService.GetTourTripByIdAsync(id);
				if (tourTrip == null)
				{
					return NotFound(new BaseResponse<TourTripResponse>
					{
						IsSucceed = false,
						Message = $"Tour trip with ID {id} not found or has been deleted."
					});
				}

				await Save(new List<TourTripResponse> { tourTrip }).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = tourTrip,
					Message = "Tour trip retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving the tour trip: {ex.Message}"
				});
			}
		}

		[HttpGet("{id}/tour")]
		[ProducesResponseType(typeof(BaseResponse<TourDetailsResponse>), 200)]
		public async Task<IActionResult> GetTourTripByTourId(Guid id)
		{
			try
			{
				var result = await _tourTripService.GetTourTripsByTourIdAsync(id);

				if (result == null)
				{
					return NotFound(new BaseResponse<TourDetailsResponse>
					{
						IsSucceed = false,
						Message = $"Tour with ID {id} not found or has been deleted."
					});
				}

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = result,
					Message = "Tour and trips retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving the tour trip: {ex.Message}"
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<TourTripResponse>), 201)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> CreateBatchTourTrips([FromBody] List<TourTripRequest> requests)
		{
			try
			{
				foreach (var request in requests)
				{
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

				var results = await _tourTripService.CreateBatchTourTrips(requests);

				// Update the cache
				var cacheData = GetKeyValues();
				foreach (var result in results)
				{
					cacheData[result.TourTripId] = result;
				}
				await Save(cacheData.Values).ConfigureAwait(false);

				return CreatedAtAction(nameof(GetTourTrips), new BaseResponse<TourTripResponse>
				{
					IsSucceed = true,
					Result = null,
					Message = "Tour trips created successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while creating the tour trips: {ex.Message}"
				});
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> UpdateTourTrip(Guid id, [FromBody] TourTripRequest request)
		{
			try
			{
				ValidationResult validationResult = await _validator.ValidateAsync(request);
				if (!validationResult.IsValid)
				{
					return BadRequest(new BaseResponse<object>
					{
						IsSucceed = false,
						Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
					});
				}

				var updatedTourTrip = await _tourTripService.UpdateTourTrip(id, request);

				if (updatedTourTrip == null)
				{
					return NotFound(new BaseResponse<TourTripResponse>
					{
						IsSucceed = false,
						Message = $"Tour trip with ID {id} not found or has been deleted."
					});
				}

				var cacheData = GetKeyValues();
				cacheData[id] = updatedTourTrip;
				await Save(cacheData.Values).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = null,
					Message = "Tour trip updated successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while updating the tour trip: {ex.Message}"
				});
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> DeleteTourTrip(Guid id)
		{
			try
			{
				var result = await _tourTripService.DeleteTourTrip(id);

				if (result)
				{
					var cacheData = GetKeyValues();
					cacheData.Remove(id);
					await Save(cacheData.Values);

					return Ok(new BaseResponse<bool>
					{
						IsSucceed = true,
						Result = true,
						Message = "Tour trip deleted successfully."
					});
				}

				return NotFound(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"Tour trip with ID {id} not found."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"Error: {ex.Message}"
				});
			}
		}

		#region Helper method
		private Task<bool> Save(IEnumerable<TourTripResponse> tourTrips, double expireAfterSeconds = 3)
		{
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
			return _cacheService.AddOrUpdateAsync(nameof(TourTripResponse), tourTrips, expirationTime);
		}

		private Dictionary<Guid, TourTripResponse> GetKeyValues()
		{
			var data = _cacheService.Get<IEnumerable<TourTripResponse>>(nameof(TourTripResponse));
			return data?.ToDictionary(key => key.TourTripId, val => val) ?? new Dictionary<Guid, TourTripResponse>();
		}

		private List<TourTripResponse> ApplySortingAndPagination(IQueryable<TourTripResponse> query, bool? sortByPrice, int page, int pageSize)
		{
			var currentDate = DateTime.Now.Date;

			// Apply sorting with date priority and price if specified
			if (sortByPrice.HasValue)
			{
				query = sortByPrice.Value
					? query.OrderBy(t => t.Price)
								.ThenBy(t => t.TripDate < currentDate)
								.ThenBy(t => Math.Abs((t.TripDate - currentDate).TotalDays))
								.ThenByDescending(t => t.CreatedDate)
					: query.OrderByDescending(t => t.Price)
								.ThenBy(t => t.TripDate < currentDate)
								.ThenBy(t => Math.Abs((t.TripDate - currentDate).TotalDays))
								.ThenByDescending(t => t.CreatedDate);
			}
			else
			{
				query = query.OrderBy(t => t.TripDate < currentDate)
										 .ThenBy(t => Math.Abs((t.TripDate - currentDate).TotalDays))
										 .ThenByDescending(t => t.CreatedDate);
			}

			// Apply pagination
			return query.Skip((page - 1) * pageSize)
			  .Take(pageSize)
			  .ToList();
		}
		#endregion
	}
}
