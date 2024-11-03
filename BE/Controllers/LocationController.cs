using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
	[ApiController]
	[Route("api/locations")]
	public class LocationController : ControllerBase
	{
		private readonly ILocationService _locationService;
		private readonly IValidator<LocationsRequest> _locationValidator;
		private readonly IValidator<LocationCreateRequest> _locationCreateValidator;
		private readonly ICacheService _cacheService;

		public LocationController(ILocationService locationService, IValidator<LocationsRequest> locationValidator,
			IValidator<LocationCreateRequest> locationCreateValidator, ICacheService cacheService)
		{
			_locationService = locationService;
			_locationValidator = locationValidator;
			_locationCreateValidator = locationCreateValidator;
			_cacheService = cacheService;
		}

		[HttpGet]
		[ProducesResponseType(typeof(BaseResponse<List<LocationResponse>>), 200)]
		public async Task<IActionResult> GetAllLocations(
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10,
			[FromQuery(Name = "sort-by-status")] WeatherStatus? sortByStatus = null,
			[FromQuery(Name = "search-term")] string? searchTerm = null)
		{
			try
			{
				var cache = GetKeyValues();
				if (cache.Count > 0)
				{
					var filteredData = cache.Values.AsQueryable();

					if (!string.IsNullOrWhiteSpace(searchTerm))
					{
						filteredData = filteredData.Where(l =>
							l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
							l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
						);
					}

					var totalElements = filteredData.Count();
					var result = ApplySortingAndPagination(filteredData, sortByStatus, page, pageSize);

					return Ok(new BaseResponse<LocationResponse>(
						result.ToList(),
						totalElements,
						page,
						pageSize,
						result.Any() ? "Locations retrieved from cache successfully." : "No locations found."));
				}

				var (serviceItems, serviceTotalCount) = await _locationService.GetAllLocationsAsync(
					page,
					pageSize,
					sortByStatus,
					searchTerm);

				// Save to cache
				await Save(serviceItems);

				return Ok(new BaseResponse<LocationResponse>(
					serviceItems,
					serviceTotalCount,
					page,
					pageSize,
					serviceItems.Any() ? "Locations retrieved successfully." : "No locations found."));
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
		[ProducesResponseType(typeof(BaseResponse<LocationResponse>), 200)]
		public async Task<IActionResult> GetById(Guid id)
		{
			try
			{
				var cacheData = GetKeyValues();
				if (cacheData.TryGetValue(id, out var cachedLocation))
				{
					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Result = cachedLocation,
						Message = "Location retrieved from cache successfully."
					});
				}

				var location = await _locationService.GetByIdAsync(id);
				if (location == null)
				{
					return NotFound(new BaseResponse<LocationResponse>
					{
						IsSucceed = false,
						Message = $"Location with ID {id} not found or has been deleted."
					});
				}

				// Save to cache
				await Save(new List<LocationResponse> { location }).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = location,
					Message = "Location retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving the location: {ex.Message}"
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(BaseResponse<LocationResponse>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> Create([FromForm] LocationCreateRequest locationsRequest, [FromForm] List<IFormFile> files)
		{
			ValidationResult validationResult = await _locationCreateValidator.ValidateAsync(locationsRequest);
			if (!validationResult.IsValid)
			{
				return BadRequest(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
				});
			}

			var data = await _locationService.CreateAsync(locationsRequest, files);
			var baseResponse = new BaseResponse<object>
			{
				IsSucceed = true,
				Result = null,
				Message = "Location created successfully"
			};
			return CreatedAtAction(nameof(GetById), new { id = data.Id }, baseResponse);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "ADMIN")]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> Update(Guid id,
			[FromForm] LocationsRequest locationsRequest,
			[FromForm] List<IFormFile> files = null)
		{
			try
			{
				foreach (var key in Request.Form.Keys)
				{
					if (key == "Photos")
					{
						var photosJson = Request.Form[key].ToString();
						if (!string.IsNullOrEmpty(photosJson))
						{
							try
							{
								if (!photosJson.TrimStart().StartsWith("["))
								{
									var photo = JsonConvert.DeserializeObject<PhotoRequest>(photosJson);
									if (photo != null)
									{
										locationsRequest.Photos = new List<PhotoRequest> { photo };
									}
								}
								else
								{
									var photos = JsonConvert.DeserializeObject<List<PhotoRequest>>(photosJson);
									locationsRequest.Photos = photos ?? new List<PhotoRequest>();
								}
							}
							catch (JsonException jsonEx)
							{
								locationsRequest.Photos = new List<PhotoRequest>();
							}
						}
					}
				}

				var data = await _locationService.UpdateAsync(id, locationsRequest, files);
				if (data != null)
				{
					var cacheData = GetKeyValues();
					if (cacheData.ContainsKey(id))
					{
						cacheData[id] = data;
						await Save(cacheData.Values);
					}

					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Result = data,
						Message = "Location updated successfully"
					});
				}

				return NotFound(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"Location with ID {id} not found."
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

		[HttpDelete("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> Delete(Guid id)
		{
			try
			{
				var result = await _locationService.DeleteAsync(id);
				if (result)
				{
					// Remove from cache if exists
					var cacheData = GetKeyValues();
					if (cacheData.ContainsKey(id))
					{
						cacheData.Remove(id);
						await Save(cacheData.Values);
					}

					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Message = "Location deleted successfully"
					});
				}

				return NotFound(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"Location with ID {id} not found."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"Error deleting location: {ex.Message}"
				});
			}
		}

		#region Helper method
		private Task<bool> Save(IEnumerable<LocationResponse> locations, double expireAfterSeconds = 30)
		{
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
			return _cacheService.AddOrUpdateAsync(nameof(LocationResponse), locations, expirationTime);
		}

		private Dictionary<Guid, LocationResponse> GetKeyValues()
		{
			var data = _cacheService.Get<IEnumerable<LocationResponse>>(nameof(LocationResponse));
			return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, LocationResponse>();
		}

		private List<LocationResponse> ApplySortingAndPagination(
			IQueryable<LocationResponse> query,
			WeatherStatus? sortByStatus,
			int page,
			int pageSize)
		{
			// Apply sorting
			if (sortByStatus.HasValue)
			{
				query = query.OrderBy(l => l.Status == sortByStatus.Value.ToString());
			}

			// Apply pagination
			return query.Skip((page - 1) * pageSize)
					   .Take(pageSize)
					   .ToList();
		}
		#endregion
	}
}
