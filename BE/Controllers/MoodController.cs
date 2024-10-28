using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.Services.Services;
using PRN231.ExploreNow.Validations.Mood;

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
        public async Task<IActionResult> GetAllMood(int page = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                List<string>? searchTerms = null;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerms = searchTerm.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                var cache = GetKeyValues();
                List<Moods> result;
                if (cache.Count > 0)
                {
                    var filteredData = cache.Values.AsQueryable();

                    // Search by content or rating
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        filteredData = filteredData.Where(p => p.MoodTag.ToString().Contains(searchTerm));
                    }

                    result = filteredData
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {

                    result = await _moodService.GetAllAsync(page, pageSize, searchTerms);
                    await Save(result).ConfigureAwait(false);
                }
                return Ok(new BaseResponse<Moods>
                {
                    IsSucceed = true,
                    Results = result,
                    Message = "Successfully"
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {

                var cache = GetKeyValues();
                if (cache.TryGetValue(id, out var result))
                {
                    return Ok(new BaseResponse<Moods>
                    {
                        IsSucceed = true,
                        Result = result,
                        Message = "Get mood in cache successfully"
                    });
                }
                var mood = await _moodService.GetById(id);
                if (mood == null)
                {
                    return NotFound(new BaseResponse<object>
                    {
                        IsSucceed = false,
                        Message = $"Not found mood with {id}"
                    });
                }
                return Ok(new BaseResponse<Moods>
                {
                    IsSucceed = true,
                    Result = mood,
                    Message = "Get mood successful"
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

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddMood(MoodRequest mood)
        {
            try
            {
                ValidationResult validationResult = _validation.Validate(mood);
                var cache = GetKeyValues();
                if (validationResult.IsValid)
                {
                    return Ok(new BaseResponse<object>
                    {
                        IsSucceed = true,
                        Message = "Created successfully"
                    });
                }
                var error = validationResult.Errors.Select(e => (object) new
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

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
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
                var error = validationResult.Errors.Select(e => (object) new
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
        public async Task<IActionResult> DeleteMood(Guid id)
        {
            try
            {
                await _moodService.Delete(id);
                return Ok(new BaseResponse<object>
                {
                    IsSucceed = true,
                    Message = "Delete successfully"
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
        private Task<bool> Save(IEnumerable<Moods> posts, double expireAfterSeconds = 30)
        {
            // Set expiration time for the cache (default is 30 seconds)
            var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
            return _cacheService.AddOrUpdateAsync(nameof(Moods), posts, expirationTime);
        }

        private Dictionary<Guid, Moods> GetKeyValues()
        {
            // Attempt to retrieve data from Redis cache
            var data = _cacheService.Get<IEnumerable<Moods>>(nameof(Moods));

            // Convert data to Dictionary or return empty Dictionary if no data
            return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, Moods>();
        }
    }
}
