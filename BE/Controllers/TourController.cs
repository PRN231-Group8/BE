using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.BusinessObject;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using Microsoft.VisualBasic;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly IUserService _userService;

        public TourController(ITourService tourService, IUserService userService)
        {
            _tourService = tourService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm)
        {
            try
            {
                if (_tourService.GetAll() == null)
                {
                    return NotFound(new BaseResponse<Tour> { IsSucceed = false, Results = null, Message = "No Tour !!" });
                }
                var result = (IList<Tour>)_tourService.GetToursAsync(page, pageSize, sortByStatus, searchTerm);
                return Ok(new BaseResponse<Tour> { IsSucceed = true, Results = result.ToList(), Message = "No Tour !!" });
            }
            catch (Exception ex)
            {
                throw new Exception(new BaseResponse<Tour> { Message = ex.Message, IsSucceed = false }.ToString());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id != null)
                {
                    if ((await _tourService.GetById(id)) == null)
                    {
                        return NotFound(new BaseResponse<Tour> { IsSucceed = false, Results = null, Message = $"Not found tour with id = {id}" });
                    }
                    var result = await _tourService.GetById(id);
                    return Ok(new BaseResponse<Tour> { IsSucceed = true, Result = result, Message = "Success" });
                }
                return BadRequest(new BaseResponse<Tour> { IsSucceed = false, Message = "Please input id correct" });
            }
            catch (Exception ex)
            {
                throw new Exception(new BaseResponse<Tour> { Message = ex.Message, IsSucceed = false }.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTour([FromBody] TourRequestModel model)
        {
            try
            {
                Tour tour = new Tour
                {
                    Id = model.Id,
                    Code = model.Code,
                    CreatedBy = model.CreatedBy,
                    CreatedDate = DateTime.Now,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    LastUpdatedBy = model.LastUpdatedBy,
                    LastUpdatedDate = DateTime.Now,
                    IsDeleted = false,
                    TotalPrice = model.TotalPrice,
                    Status = model.Status,
                    UserId = model.UserId,
                    User = model.User,
                    Title = model.Title,
                    Description = model.Description,
                };
                await _tourService.Add(tour);
                return Ok(new BaseResponse<Tour> { IsSucceed = true, Result = tour, Message = "Created successfully" }); ;
            }
            catch (Exception ex)
            {
                throw new Exception(new BaseResponse<Tour> { Message = ex.Message, IsSucceed = false }.ToString());
            }
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] TourRequestModel model)
        {
            try
            {
                if (model != null)
                {
                    if (await _tourService.GetById(model.Id) != null)
                    {
                        Tour tour = new Tour
                        {
                            Id = model.Id,
                            Code = model.Code,
                            CreatedBy = model.CreatedBy,
                            CreatedDate = DateTime.Now,
                            StartDate = model.StartDate,
                            EndDate = model.EndDate,
                            LastUpdatedBy = model.LastUpdatedBy,
                            LastUpdatedDate = DateTime.Now,
                            IsDeleted = false,
                            TotalPrice = model.TotalPrice,
                            Status = model.Status,
                            UserId = model.UserId,
                            User = model.User,
                            Title = model.Title,
                            Description = model.Description,
                        };
                        await _tourService.Update(tour);
                        return Ok(new BaseResponse<Tour> { IsSucceed = true, Result = tour, Message = "Update successfully" });
                    }
                    return NotFound(new BaseResponse<Tour> { IsSucceed = false, Message = $"Not found tour with id = {model.Id}" });
                }
                return BadRequest(new BaseResponse<Tour> { IsSucceed = false, Message = "Please input correct" });
            }
            catch (Exception ex)
            {
                throw new Exception(new BaseResponse<Tour> { Message = ex.Message, IsSucceed = false }.ToString());
            }
        }
        [HttpDelete]
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
                throw new Exception(new BaseResponse<Tour> { Message = ex.Message, IsSucceed = false }.ToString());
            }
        }
    }
}
