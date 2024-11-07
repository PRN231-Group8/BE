using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.Validations.Mood;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
	[Route("api/moods")]
	[ApiController]
	public class MoodController : ControllerBase
	{
		private readonly IMoodService _moodService;
		private readonly MoodValidation _validation;
		private readonly ICacheService _cacheService;

		public MoodController(IMoodService moodService, MoodValidation validation, ICacheService cacheService)
		{
			_moodService = moodService;
			_validation = validation;
			_cacheService = cacheService;
		}

		[HttpGet]
		[ProducesResponseType(typeof(BaseResponse<List<MoodResponse>>), 200)]
		public async Task<IActionResult> GetAllMood(
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10,
			[FromQuery(Name = "search-term")] string? searchTerm = null)
		{
			try
			{
				var cacheData = GetKeyValues();
				IQueryable<MoodResponse> filteredData;

				if (cacheData.Count > 0)
				{
					filteredData = cacheData.Values.AsQueryable();

					if (!string.IsNullOrEmpty(searchTerm))
					{
						filteredData = filteredData.Where(m =>
							m.MoodTag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
					}

					var totalElements = filteredData.Count();

					var result = filteredData
						.Skip((page - 1) * pageSize)
						.Take(pageSize)
						.ToList();

					return Ok(new BaseResponse<MoodResponse>(
						result,
						totalElements,
						page,
						pageSize,
						result.Any() ? "Moods retrieved from cache successfully." : "No moods found."));
				}
				else
				{
					var (result, totalElements) = await _moodService.GetAllAsync(page, pageSize, searchTerm);
					await Save(result, totalElements);

					return Ok(new BaseResponse<MoodResponse>(
						result,
						totalElements,
						page,
						pageSize,
						result.Any() ? "Moods retrieved successfully." : "No moods found."));
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
		[ProducesResponseType(typeof(BaseResponse<MoodResponse>), 200)]
		public async Task<IActionResult> GetById(Guid id)
		{
			try
			{
				var cacheData = GetKeyValues();
				if (cacheData.TryGetValue(id, out var cachedMood))
				{
					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Result = cachedMood,
						Message = "Mood retrieved from cache successfully."
					});
				}

				var mood = await _moodService.GetById(id);
				if (mood == null)
				{
					return NotFound(new BaseResponse<MoodResponse>
					{
						IsSucceed = false,
						Message = $"Mood with ID {id} not found or has been deleted."
					});
				}

				await Save(new List<MoodResponse> { mood }).ConfigureAwait(false);

				return Ok(new BaseResponse<object>
				{
					IsSucceed = true,
					Result = mood,
					Message = "Mood retrieved successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while retrieving the mood: {ex.Message}"
				});
			}
		}

		[HttpPost]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<MoodResponse>), 201)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> AddMood([FromBody] MoodRequest request)
		{
			try
			{
				var validationResult = _validation.Validate(request);
				if (!validationResult.IsValid)
				{
					return BadRequest(new BaseResponse<object>
					{
						IsSucceed = false,
						Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
					});
				}

				// Add mood
				var result = await _moodService.Add(request);

				// Update the cache
				var cacheData = GetKeyValues();
				cacheData[result.Id] = result;
				await Save(cacheData.Values).ConfigureAwait(false);

				return CreatedAtAction(nameof(GetAllMood), new BaseResponse<MoodResponse>
				{
					IsSucceed = true,
					Result = null,
					Message = "Mood created successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode((int) HttpStatusCode.InternalServerError, new BaseResponse<object>
				{
					IsSucceed = false,
					Message = $"An error occurred while creating the mood: {ex.Message}"
				});
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 400)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> UpdateMood(Guid id, MoodRequest mood)
		{
			try
			{
				ValidationResult validationResult = _validation.Validate(mood);
				if (validationResult.IsValid)
				{
					await _moodService.Update(mood, id);
					return Ok(new BaseResponse<object>
					{
						IsSucceed = true,
						Message = "Updated Successfully"
					});
				}
				var error = validationResult.Errors.Select(e => (object)new
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
				return BadRequest(new BaseResponse<object>
				{
					IsSucceed = false,
					Message = ex.Message
				});
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "ADMIN")]
		[ProducesResponseType(typeof(BaseResponse<object>), 200)]
		[ProducesResponseType(typeof(BaseResponse<object>), 404)]
		[ProducesResponseType(typeof(BaseResponse<object>), 500)]
		public async Task<IActionResult> DeleteMood(Guid id)
		{
			try
			{
				await _moodService.Delete(id);

				var cacheData = GetKeyValues();
				cacheData.Remove(id);
				await Save(cacheData.Values);

				return Ok(new BaseResponse<bool>
				{
					IsSucceed = true,
					Result = true,
					Message = "Mood deleted successfully."
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
		private Task<bool> Save(IEnumerable<MoodResponse> posts, double expireAfterSeconds = 3)
		{
			// Set expiration time for the cache (default is 3 seconds)
			var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
			return _cacheService.AddOrUpdateAsync(nameof(Moods), posts, expirationTime);
		}

		private Dictionary<Guid, MoodResponse> GetKeyValues()
		{
			// Attempt to retrieve data from Redis cache
			var data = _cacheService.Get<IEnumerable<MoodResponse>>(nameof(Moods));

			// Convert data to Dictionary or return empty Dictionary if no data
			return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, MoodResponse>();
		}
		#endregion
	}
}
