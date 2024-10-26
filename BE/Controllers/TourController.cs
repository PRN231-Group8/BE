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
using PRN231.ExploreNow.BusinessObject.OtherObjects;
using Newtonsoft.Json.Linq;


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
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10, TourStatus? sortByStatus = null, List<string>? searchTerm = null)
        {
            try
            {
                var cache = GetKeyValues();
                var result = cache.Values;
                if (result.Count == 0)
                {
                    var tour = await _tourService.GetToursAsync(page, pageSize, sortByStatus, searchTerm);
                    return Ok(new BaseResponse<TourResponse> { IsSucceed = true, Results = tour.ToList(), Message = "Success" });
                }
                return Ok(new BaseResponse<TourResponse> { IsSucceed = true, Results = result.ToList(), Message = "Success" });
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

        private Task<bool> Save(IEnumerable<TourResponse> tour, double expireAfterSeconds = 30)
        {
            var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
            return _cacheService.AddOrUpdateAsync(nameof(TourResponse), tour, expirationTime); // khoi tao key hoac luu value trong key trong cache 30 giay
        }

        private Dictionary<Guid, TourResponse> GetKeyValues()
        {
            var data = _cacheService.Get<IEnumerable<TourResponse>>(nameof(TourResponse)); // dat ten key
            return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, TourResponse>();
        }
    }
}
