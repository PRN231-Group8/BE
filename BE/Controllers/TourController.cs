using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;
using PRN231.ExploreNow.BusinessObject;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;


        public TourController(ITourService tourService)
        {
            _tourService = tourService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if (_tourService.GetAll() == null)
                {
                    return NotFound(new BaseResponse<Tour> { IsSucceed = false, Results = null, Message = "No Tour !!" });
                }
                var result = (IList<Tour>)_tourService.GetAll();
                return Ok(new BaseResponse<Tour> { IsSucceed = true, Results = result.ToList(), Message = "No Tour !!" });
            }
            catch (Exception ex)
            {
                throw;
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
                throw;
            }
        }

        [HttpPost]
        /*public async Task<IActionResult> AddTour(TourRequestModel model)
        {
            if( model != null)
            {

                await _tourService.Add(model);
            }
        }*/
    }
}
