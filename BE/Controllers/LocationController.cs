using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IValidator<LocationsRequest> _locationValidator;
        private readonly IValidator<LocationCreateRequest> _locationCreateValidator;
        public LocationController(ILocationService locationService, IValidator<LocationsRequest> locationValidator, IValidator<LocationCreateRequest> locationCreateValidator)
        {
            _locationService = locationService;
            _locationValidator = locationValidator;
            _locationCreateValidator = locationCreateValidator;
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
    
            if (data == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Result = null,
                    Message = "Location not found"
                });
            }

            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location retrieved successfully"
            };
    
            return Ok(baseResponse);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] LocationCreateRequest locationsRequest, [FromForm] List<IFormFile> files)
        {
            ValidationResult validationResult = await _locationCreateValidator.ValidateAsync(locationsRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
                });
            }

            var data = await _locationService.CreateAsync(locationsRequest, files);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location created successfully"
            };
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, baseResponse);
        }
        
        //[Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] LocationsRequest locationsRequest, [FromForm] string photos = null, [FromForm] List<IFormFile> files = null)
        {
            // Kiểm tra nếu photos có giá trị và không rỗng trước khi xử lý
            if (!string.IsNullOrEmpty(photos))
            {
                // Kiểm tra nếu `photos` là một JSON array hợp lệ trước khi Deserialize
                if (photos.Trim().StartsWith("[") && photos.Trim().EndsWith("]"))
                {
                    try
                    {
                        locationsRequest.Photos = JsonConvert.DeserializeObject<List<PhotoRequest>>(photos);
                    }
                    catch (JsonSerializationException)
                    {
                        // Xử lý lỗi nếu JSON không hợp lệ
                        return BadRequest(new { message = "Invalid photos JSON format. Please provide a valid JSON array." });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Photos should be a JSON array." });
                }
            }
            // Nếu `photos` không có hoặc rỗng, thì không làm gì, giữ nguyên `locationsRequest.Photos`
    
            // Xử lý phần cập nhật location
            var data = await _locationService.UpdateAsync(id, locationsRequest, files);
            var baseResponse = new BaseResponse<object>
            {
                IsSucceed = true,
                Result = data,
                Message = "Location updated successfully"
            };

            return Ok(baseResponse);
        }


        [Authorize(Roles = "ADMIN")]
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
