using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.Validations.Mood;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/mood")]
    [ApiController]
    public class MoodController : ControllerBase
    {
        private readonly IMoodService _moodService;
        private readonly MoodValidation _validation;
        public MoodController(IMoodService moodService, MoodValidation validation)
        {
            _moodService = moodService;
            _validation = validation;
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

                var moods = await _moodService.GetAllAsync(page, pageSize, searchTerms);
                return Ok(new BaseResponse<Moods>
                {
                    IsSucceed = true,
                    Results = moods,
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
                var mood = await _moodService.GetById(id);
                return Ok(new BaseResponse<Moods>
                {
                    IsSucceed = true,
                    Result = mood,
                    Message = "Successfuly"
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
                if (validationResult.IsValid)
                {
                    await _moodService.Add(mood);
                    return Ok(new BaseResponse<object>
                    {
                        IsSucceed = true,
                        Message = "Created successfully"
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
    }
}
