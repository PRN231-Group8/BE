using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.Validations.Tour;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using PRN231.ExploreNow.BusinessObject.OtherObjects;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
	[Route("api/tours")]
	[ApiController]
	public class TourController : ControllerBase
	{
		private readonly ITourService _tourService;
		private readonly TourValidation _tourValidation;
		private readonly ICacheService _cacheService;

		public TourController(ITourService tourService, TourValidation tourValidation, ICacheService cacheService)
		{
			_tourService = tourService;
			_tourValidation = tourValidation;
			_cacheService = cacheService;
		}

		[HttpGet]
		[ProducesResponseType(typeof(BaseResponse<List<TourResponse>>), 200)]
		public async Task<IActionResult> GetTours(
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10,
			[FromQuery(Name = "sort-by-status")] TourStatus? sortByStatus = null,
			[FromQuery(Name = "search-term")] string? searchTerm = null)
		{
			try
			{
				// Check cache
				var cache = GetKeyValues();
				if (cache.Count > 0)
				{
					var filteredData = cache.Values.AsQueryable();

					// Apply search filter
					if (!string.IsNullOrWhiteSpace(searchTerm))
					{
						filteredData = filteredData.Where(t => t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
						t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
						t.TourMoods.Any(tm => tm != null &&
							tm.MoodTag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
						t.LocationInTours.Any(lit => lit != null &&
							lit.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
					);
					}

					var totalElements = filteredData.Count();
					var result = ApplySortingAndPagination(filteredData, sortByStatus, page, pageSize);

					return Ok(new BaseResponse<TourResponse>(
						result.ToList(),
						totalElements,
						page,
						pageSize,
						result.Any() ? "Tours retrieved from cache successfully." : "No tours found."));
				}

				// Get from service if not in cache
				var (serviceItems, serviceTotalCount) = await _tourService.GetAllToursAsync(page, pageSize, sortByStatus, searchTerm);

				// Save to cache
				await Save(serviceItems);

				return Ok(new BaseResponse<TourResponse>(
					serviceItems,
					serviceTotalCount,
					page,
					pageSize,
					serviceItems.Any() ? "Tours retrieved successfully." : "No tours found."));
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
		[ProducesResponseType(typeof(BaseResponse<TourResponse>), 200)]
		public async Task<IActionResult> GetById(Guid id)
		{
			try
			{
				var cache = GetKeyValues();
				var tour = cache.TryGetValue(id, out var cacheTour);
				if (cacheTour == null)
				{
					var result = await _tourService.GetById(id);
					if (result == null)
					{
						return NotFound(new BaseResponse<object> { IsSucceed = false, Message = $"Not found tour with Id {id}" });
					}
					return Ok(new BaseResponse<object> { IsSucceed = true, Result = result, Message = "Success" });
				}
				return Ok(new BaseResponse<object> { IsSucceed = true, Result = cacheTour, Message = "Success" });
			}
			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
			}
		}

		[HttpPost]
		[Authorize(Roles = StaticUserRoles.ADMIN)]
		[ProducesResponseType(typeof(BaseResponse<TourResponse>), 201)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> AddTour([FromBody] TourRequestModel model)
		{
			try
			{
				ValidationResult ValidateResult = await _tourValidation.ValidateAsync(model);
				var cacheData = GetKeyValues();
				if (ValidateResult.IsValid)
				{
					var tour = await _tourService.Add(model);
					cacheData[tour.Id] = tour;
					await Save(cacheData.Values).ConfigureAwait(false);
					return Ok(new BaseResponse<object> { IsSucceed = true, Message = "Created successfully" });
				}
				var errors = ValidateResult.Errors.Select(e => (object)new
				{
					e.PropertyName,
					e.ErrorMessage
				}).ToList();
				return BadRequest(new BaseResponse<object>
				{
					IsSucceed = false,
					Results = errors
				});
			}
			catch (CreateException ce)
			{
				return BadRequest(new BaseResponse<object> { IsSucceed = false, Message = ce.Message });
			}
			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = StaticUserRoles.ADMIN)]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> Update([FromBody] TourRequestModel model, Guid id)
		{
			try
			{
				ValidationResult ValidateResult = await _tourValidation.ValidateAsync(model);
				var cache = GetKeyValues();
				if (ValidateResult.IsValid)
				{
					var tour = await _tourService.UpdateAsync(model, id);
					cache[id] = tour;
					await Save(cache.Values).ConfigureAwait(false);
					return Ok(new BaseResponse<object> { IsSucceed = true, Message = "Succesfully" });
				}

				var error = ValidateResult.Errors.Select(e => (object)new
				{
					e.PropertyName,
					e.ErrorMessage
				}).ToList();

				return BadRequest(new BaseResponse<object>
				{
					IsSucceed = false,
					Results = error
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = StaticUserRoles.ADMIN)]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> Delete(Guid id)
		{
			try
			{
				var cache = GetKeyValues();
				if (await _tourService.GetById(id) != null)
				{
					await _tourService.Delete(id);
					cache.Remove(id);
					return Ok(new BaseResponse<Tour>
					{
						IsSucceed = true,
						Message = "Delete successfully"
					});
				}
				return NotFound(new BaseResponse<Tour>
				{
					IsSucceed = false,
					Message = $"Not found tour with id = {id}"
				});
			}

			catch (Exception ex)
			{
				return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
			}
		}

		private Task<bool> Save(IEnumerable<TourResponse> tour, double expireAfterSeconds = 3)
		{
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
			return _cacheService.AddOrUpdateAsync(nameof(TourResponse), tour, expirationTime); // khoi tao key hoac luu value trong key trong cache 3 giay
		}

		private Dictionary<Guid, TourResponse> GetKeyValues()
		{
			var data = _cacheService.Get<IEnumerable<TourResponse>>(nameof(TourResponse)); // dat ten key
			return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, TourResponse>();
		}

		private IQueryable<TourResponse> ApplySortingAndPagination(IQueryable<TourResponse> query, TourStatus? sortByStatus, int page, int pageSize)
		{
			if (sortByStatus.HasValue)
			{
				query = query.OrderBy(t => t.Status == sortByStatus.Value);
			}

			return query
				.Skip((page - 1) * pageSize)
				.Take(pageSize);
		}
	}
}
