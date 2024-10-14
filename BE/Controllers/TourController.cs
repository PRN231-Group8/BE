using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.Validations.Tour;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/tour")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private ITourService _tourService;
        private readonly TourValidation _tourValidation;

        public TourController(ITourService tourService, TourValidation tourValidation)
        {
            _tourService = tourService;
            _tourValidation = tourValidation;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, BookingStatus? sortByStatus = null, string? searchTerm = null)
        {
            try
            {
                var result = await _tourService.GetToursAsync(page, pageSize, sortByStatus, searchTerm);
                return Ok(new BaseResponse<Tour> { IsSucceed = true, Results = result.ToList(), Message = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _tourService.GetById(id);
                if (result == null)
                {
                    return NotFound(new BaseResponse<object> { IsSucceed = false, Message = $"Not found tour with Id {id}" });
                }
                return Ok(new BaseResponse<object> { IsSucceed = true, Result = result, Message = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
            }
        }

        [HttpPost]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> AddTour([FromBody] TourRequestModel model)
        {
            try
            {
                ValidationResult ValidateResult = await _tourValidation.ValidateAsync(model);
                if (ValidateResult.IsValid)
                {
                    await _tourService.Add(model);
                    return Ok(new BaseResponse<object> { IsSucceed = true, Result = model, Message = "Created successfully" });
                }
                return BadRequest(new BaseResponse<Tour>
                {
                    IsSucceed = true,
                    Message = ValidateResult.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> Update([FromBody] TourRequestModel model, Guid id)
        {
            try
            {
                ValidationResult ValidateResult = await _tourValidation.ValidateAsync(model);
                var error = ValidateResult.ToString();
                if (ValidateResult.IsValid)
                {
                    var tour = await _tourService.UpdateAsync(model, id);
                    return Ok(new BaseResponse<object> { IsSucceed = true, Result = tour, Message = "Succesfully" });
                }

                return BadRequest(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Message = error
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id != null)
                {
                    if (await _tourService.GetById(id) != null)
                    {
                        await _tourService.Delete(id);
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
                return BadRequest(new BaseResponse<Tour>
                {
                    IsSucceed = false,
                    Message = "Please input correct"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<object> { IsSucceed = false, Result = ex.Message, Message = "There is something wrong" });
            }
        }
    }
}
