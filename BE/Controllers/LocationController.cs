using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers
{
    [ApiController]
    [Route("api/location")]
    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly IValidator<LocationsRequest> _locationValidator;
        private readonly IValidator<PhotoRequest> _photoValidator;
        public LocationController(ILocationService locationService, IValidator<LocationsRequest> locationValidator, IValidator<PhotoRequest> photoValidator)
        {
            _locationService = locationService;
            _locationValidator = locationValidator;
            _photoValidator = photoValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocations(int page = 1, int pageSize = 10, WeatherStatus? sortByStatus = null, string? searchTerm = null)
        {
            var data = await _locationService.GetAllLocationsAsync(page, pageSize, sortByStatus, searchTerm);
            var baseResponse = new BaseResponse<LocationResponse>
            {
                IsSucceed = true,
                Results = data,
                Message = "Locations retrieved successfully"
            };
            return Ok(baseResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _locationService.GetByIdAsync(id);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location retrieved successfully"
            };
            return Ok(baseResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LocationsRequest locationsRequest)
        {
            ValidationResult validationResult = await _locationValidator.ValidateAsync(locationsRequest);
            if (!validationResult.IsValid)
            {
                return Ok(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Message = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToString()
                });
            }
            var data = await _locationService.CreateAsync(locationsRequest);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location created successfully"
            };
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, baseResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LocationsRequest locationsRequest)
        {
            ValidationResult validationResult = await _locationValidator.ValidateAsync(locationsRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Message = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToString()
                });
            }
            
            var data = await _locationService.UpdateAsync(id, locationsRequest);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location updated successfully"
            };
            return Ok(baseResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _locationService.DeleteAsync(id);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location deleted successfully"
            };
            return Ok(baseResponse);
        }
    }
}
