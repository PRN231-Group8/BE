using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers
{
    [ApiController]
    [Route("location")]
    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly IValidator<LocationsRequest> _validator; 

        public LocationController(ILocationService locationService, IValidator<LocationsRequest> validator)
        {
            _locationService = locationService;
            _validator = validator;
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
            var validationResult = await _validator.ValidateAsync(locationsRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var data = await _locationService.CreateAsync(locationsRequest);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location created successfully"
            };
            return Ok(baseResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LocationsRequest locationsRequest)
        {
            var validationResult = await _validator.ValidateAsync(locationsRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
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
                Message = "Location delete successfully"
            };
            return Ok(baseResponse);
        }
    }
}
