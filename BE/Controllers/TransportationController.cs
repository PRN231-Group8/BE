using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.Models.Response.CacheResponse;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers;

[Route("api/transportations")]
[ApiController]
public class TransportationController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ITransportationService _transportationService;
    private readonly IValidator<TransportationRequestModel> _validator;

    public TransportationController(ITransportationService transportationService, ICacheService cacheService,
        IValidator<TransportationRequestModel> validator)
    {
        _transportationService = transportationService;
        _cacheService = cacheService;
        _validator = validator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<TransportationResponse>), 200)]
    public async Task<IActionResult> GetTransportationById(Guid id)
    {
        try
        {
            var cachedData = GetKeyValues();
            if (cachedData.TryGetValue(id, out var cachedTransportation))
                return Ok(new BaseResponse<TransportationResponse>
                {
                    IsSucceed = true,
                    Result = cachedTransportation,
                    Message = "Transportation retrieved from cache successfully."
                });

            var transportation = await _transportationService.GetTransportationById(id);
            if (transportation == null)
                return NotFound(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Message = $"Transportation with ID {id} not found."
                });

            await Save(new List<TransportationResponse> { transportation });

            return Ok(new BaseResponse<TransportationResponse>
            {
                IsSucceed = true,
                Result = transportation,
                Message = "Transportation retrieved successfully."
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
            {
                IsSucceed = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<List<TransportationResponse>>), 200)]
    public async Task<IActionResult> GetTransportations(
        [FromQuery(Name = "page-number")] int page = 1,
        [FromQuery(Name = "page-size")] int pageSize = 10,
        [FromQuery(Name = "sort-by")] string? sortBy = "price",
        [FromQuery(Name = "sort-order")] string? sortOrder = "asc",
        [FromQuery(Name = "search-term")] string? searchTerm = null)
    {
        try
        {
            var cacheData = GetCachedTransportations();
            IQueryable<TransportationResponse> filteredData;

            if (cacheData.ResponseList.Count > 0)
            {
                filteredData = cacheData.ResponseList.AsQueryable();

                // Apply search filter if searchTerm is provided
                if (!string.IsNullOrEmpty(searchTerm))
                    filteredData = filteredData.Where(t =>
                        t.Type.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Capacity.ToString().Contains(searchTerm));

                // Apply sorting if sortBy is provided
                if (!string.IsNullOrWhiteSpace(sortBy)) filteredData = ApplySorting(filteredData, sortBy, sortOrder);

                // Apply pagination
                var result = filteredData
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new BaseResponse<TransportationResponse>(result, cacheData.TotalElements, page, pageSize, result.Any() ? "Transportations retrieved in cache successfully." : "No transportations found."));
            }
            else
            {
                var (result, totalElements) = await _transportationService.GetTransportations(page, pageSize, sortBy, searchTerm);
                await Save(result, totalElements);
                return Ok(new BaseResponse<TransportationResponse>(result, totalElements, page, pageSize, result.Any() ? "Transportations retrieved successfully." : "No transportations found."));
            }
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
            {
                IsSucceed = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = StaticUserRoles.ADMIN)]
    [ProducesResponseType(typeof(BaseResponse<TransportationResponse>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    [ProducesResponseType(typeof(BaseResponse<object>), 500)]
    public async Task<IActionResult> AddTransportation([FromBody] TransportationRequestModel transportation)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(transportation);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList();
                return BadRequest(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Result = errors,
                    Message = "Validation failed."
                });
            }

            await _transportationService.AddTransportation(transportation);
            return Ok(new BaseResponse<object>
            {
                IsSucceed = true,
                Message = "Transportation added successfully."
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
            {
                IsSucceed = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = StaticUserRoles.ADMIN)]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    [ProducesResponseType(typeof(BaseResponse<object>), 500)]
    public async Task<IActionResult> UpdateTransportation(Guid id, [FromBody] TransportationRequestModel transportation)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(transportation);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList();
                return BadRequest(new BaseResponse<object>
                {
                    IsSucceed = false,
                    Result = errors,
                    Message = "Validation failed."
                });
            }

            var result = await _transportationService.UpdateTransportation(id, transportation);
            if (result)
            {
                var cacheData = GetKeyValues();
                if (cacheData.ContainsKey(id))
                {
                    cacheData[id] = await _transportationService.GetTransportationById(id);
                    await Save(cacheData.Values);
                }

                return Ok(new BaseResponse<object>
                {
                    IsSucceed = true,
                    Message = "Transportation updated successfully."
                });
            }

            return NotFound(new BaseResponse<object>
            {
                IsSucceed = false,
                Message = $"Transportation with ID {id} not found."
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

    [HttpDelete("{id}")]
    [Authorize(Roles = StaticUserRoles.ADMIN)]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    [ProducesResponseType(typeof(BaseResponse<object>), 500)]
    public async Task<IActionResult> DeleteTransportation(Guid id)
    {
        try
        {
            var result = await _transportationService.DeleteTransportation(id);

            if (result)
            {
                var cacheData = GetKeyValues();
                cacheData.Remove(id);
                await Save(cacheData.Values);

                return Ok(new BaseResponse<bool>
                {
                    IsSucceed = true,
                    Result = true,
                    Message = "Transportation deleted successfully."
                });
            }

            return NotFound(new BaseResponse<object>
            {
                IsSucceed = false,
                Message = $"Transportation with ID {id} not found."
            });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<object>
            {
                IsSucceed = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    private Task<bool> Save(IEnumerable<TransportationResponse> transportations, double expireAfterSeconds = 30)
    {
        var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
        return _cacheService.AddOrUpdateAsync(nameof(TransportationResponse), transportations, expirationTime);
    }

    private async Task<bool> Save(List<TransportationResponse> transportations, int totalElements, double expireAfterSeconds = 30)
    {
        var cacheData = new BaseCacheResponse<TransportationResponse>
        {
            ResponseList = transportations,
            TotalElements = totalElements
        };

        var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
        return await _cacheService.AddOrUpdateAsync("BaseCacheResponse", cacheData, expirationTime);
    }

    private BaseCacheResponse<TransportationResponse> GetCachedTransportations()
    {
        return _cacheService.Get<BaseCacheResponse<TransportationResponse>>("BaseCacheResponse") ?? new BaseCacheResponse<TransportationResponse>();
    }


    private Dictionary<Guid, TransportationResponse> GetKeyValues()
    {
        var data = _cacheService.Get<IEnumerable<TransportationResponse>>(nameof(TransportationResponse));
        return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, TransportationResponse>();
    }

    // Helper method to apply sorting dynamically
    private IQueryable<TransportationResponse> ApplySorting(
        IQueryable<TransportationResponse> source,
        string sortBy,
        string sortOrder)
    {
        if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
            return sortBy switch
            {
                "price" => source.OrderByDescending(t => t.Price),
                "capacity" => source.OrderByDescending(t => t.Capacity),
                "type" => source.OrderByDescending(t => t.Type),
                _ => source
            };
        return sortBy switch
        {
            "price" => source.OrderBy(t => t.Price),
            "capacity" => source.OrderBy(t => t.Capacity),
            "type" => source.OrderBy(t => t.Type),
            _ => source
        };
    }
}