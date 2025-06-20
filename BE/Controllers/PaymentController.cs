﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Services.Interfaces;
using System.Net;

namespace PRN231.ExploreNow.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IValidator<PaymentRequest> _validator;
        private readonly ICacheService _cacheService;

        public PaymentController(IVNPayService vnPayService, IValidator<PaymentRequest> validator,
            ICacheService cacheService)
        {
            _vnPayService = vnPayService;
            _validator = validator;
            _cacheService = cacheService;
        }

        // [HttpGet("history")]
        // [Authorize]
        // public async Task<IActionResult> GetTourHistory(
        //     [FromQuery(Name = "page-number")] int page = 1,
        //     [FromQuery(Name = "page-size")] int pageSize = 10,
        //     [FromQuery(Name = "filter-status")] PaymentTransactionStatus? status = null,
        //     [FromQuery(Name = "search-term")] string? searchTerm = null)
        // {
        //     try
        //     {
        //         var zeroBasedPage = page - 1;
        //         var cacheData = GetKeyValues();
        //         List<TourPackageHistoryResponse> items;
        //         int totalCount;
        //
        //         // If data is found in cache, filter and return it
        //         if (cacheData.Count > 0)
        //         {
        //             var filteredData = cacheData.Values.AsQueryable();
        //             if (!string.IsNullOrEmpty(searchTerm) && decimal.TryParse(searchTerm, out decimal searchPrice))
        //             {
        //                 filteredData = filteredData.Where(t => t.TotalPrice == searchPrice);
        //             }
        //
        //             if (status.HasValue)
        //             {
        //                 filteredData = filteredData.Where(t =>
        //                     t.Transactions.Any(tr => tr.Status == status)
        //                 );
        //             }
        //
        //             totalCount = filteredData.Count();
        //             items = filteredData
        //                 .Skip(zeroBasedPage * pageSize)
        //                 .Take(pageSize)
        //                 .ToList();
        //         }
        //         else
        //         {
        //             // If not in cache, query from service
        //             var (serviceItems, serviceTotalCount) = await _vnPayService.GetUserTourHistory(
        //                 page, pageSize, status, searchTerm);
        //             items = serviceItems;
        //             totalCount = serviceTotalCount;
        //
        //             // Save the result to cache for future requests
        //             await Save(items).ConfigureAwait(false);
        //         }
        //
        //         return Ok(new BaseResponse<TourPackageHistoryResponse>
        //         {
        //             IsSucceed = true,
        //             Results = items,
        //             TotalElements = totalCount,
        //             Message = items?.Any() == true
        //                 ? "Tour history retrieved successfully."
        //                 : "No tour history found for the current user.",
        //             Size = pageSize,
        //             Number = zeroBasedPage,
        //             TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        //             NumberOfElements = items.Count,
        //             First = zeroBasedPage == 0,
        //             Last = zeroBasedPage >= (int)Math.Ceiling(totalCount / (double)pageSize) - 1,
        //             Empty = !items.Any(),
        //             Sort = new BaseResponse<TourPackageHistoryResponse>.SortInfo
        //             {
        //                 Empty = false,
        //                 Sorted = true,
        //                 Unsorted = false
        //             }
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<TourPackageDetailsResponse>
        //         {
        //             IsSucceed = false,
        //             Message = $"An error occurred while retrieving tour package details: {ex.Message}"
        //         });
        //     }
        // }

        [HttpGet("{id}/tour")]
        [Authorize]
        public async Task<IActionResult> GetTourPackageDetails(Guid id)
        {
            try
            {
                var tourPackageDetails = await _vnPayService.GetTourPackageDetails(id);
                if (tourPackageDetails == null)
                {
                    return NotFound(new BaseResponse<TourPackageDetailsResponse>
                    {
                        IsSucceed = false,
                        Message = "Tour package details not found."
                    });
                }

                return Ok(new BaseResponse<TourPackageDetailsResponse>
                {
                    IsSucceed = true,
                    Result = tourPackageDetails,
                    Message = "Tour package details retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<TourPackageDetailsResponse>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving tour package details: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                ValidationResult validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        IsSucceed = false,
                        Message = string.Join(", ",
                            validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
                    });
                }

                var paymentUrl = await _vnPayService.CreateEmbeddedPaymentForTourTrip(request);
                return CreatedAtAction(nameof(GetTourPackageDetails), new { id = request.TourTripId },
                    new BaseResponse<string>
                    {
                        IsSucceed = true,
                        Result = paymentUrl,
                        Message = "Payment created successfully."
                    });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<string>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while creating the payment: {ex.Message}"
                });
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var response = await _vnPayService.ProcessPaymentCallback(Request.Query);
                if (response == null)
                {
                    return NotFound(new BaseResponse<VNPayResponse>
                    {
                        IsSucceed = false,
                        Message = "No pending payment found for the given transaction reference."
                    });
                }

                if (response.Success)
                {
                    return Ok(new BaseResponse<VNPayResponse>
                    {
                        IsSucceed = true,
                        Result = response,
                        Message = "Payment processed successfully."
                    });
                }
                else
                {
                    return BadRequest(new BaseResponse<VNPayResponse>
                    {
                        IsSucceed = false,
                        Result = response,
                        Message = "Payment processing failed."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<VNPayResponse>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while processing the payment callback: {ex.Message}"
                });
            }
        }

        [HttpGet("booking-history")]
        [Authorize]
        public async Task<IActionResult> GetBookingHistory(
            [FromQuery(Name = "page-number")] int page = 1,
            [FromQuery(Name = "page-size")] int pageSize = 10,
            [FromQuery(Name = "filter-status")] PaymentTransactionStatus? status = null,
            [FromQuery(Name = "search-term")] string? searchTerm = null)
        {
            try
            {
                // Step 1: Check if the data is already cached
                var cachedData = GetKeyValues();
                List<BookingHistoryResponse> transactionDetails;
                int totalCount;

                // Step 2: If data is cached, apply the necessary filters and pagination
                if (cachedData.Count > 0)
                {
                    var filteredData = cachedData.Values.AsQueryable();

                    // Apply search filter if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        filteredData =
                            filteredData.Where(t =>
                                t.Id.ToString().Contains(searchTerm)); // Modify as per your search logic
                    }

                    // Apply status filter if provided
                    if (status.HasValue)
                    {
                        filteredData = filteredData.Where(t => t.TransactionStatus == status);
                    }

                    // Get total count and apply pagination
                    totalCount = filteredData.Count();
                    transactionDetails = filteredData
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    // Step 3: If no data in cache, call the service to get data
                    var (serviceItems, serviceTotalCount) = await _vnPayService.GetSuccessfulTransactionsWithTourInfo(
                        page, pageSize, status, searchTerm);

                    transactionDetails = serviceItems;
                    totalCount = serviceTotalCount;

                    // Step 4: Save the data to Redis cache for future use
                    if (transactionDetails.Any())
                    {
                        await Save(transactionDetails); // Cache data for future requests
                    }
                }

                // Step 5: Return the response
                return Ok(new BaseResponse<BookingHistoryResponse>
                {
                    IsSucceed = true,
                    Results = transactionDetails,
                    TotalElements = totalCount,
                    Message = transactionDetails.Any()
                        ? "Transaction details retrieved successfully."
                        : "No successful transactions found.",
                    Size = pageSize,
                    Number = page - 1,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    NumberOfElements = transactionDetails.Count,
                    First = page == 1,
                    Last = page >= (int)Math.Ceiling(totalCount / (double)pageSize),
                    Empty = !transactionDetails.Any(),
                    Sort = new BaseResponse<BookingHistoryResponse>.SortInfo
                    {
                        Empty = false,
                        Sorted = true,
                        Unsorted = false
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponse<BookingHistoryResponse>
                {
                    IsSucceed = false,
                    Message = $"An error occurred while retrieving transaction details: {ex.Message}"
                });
            }
        }


        #region Helper method

        private Task<bool> Save(IEnumerable<BookingHistoryResponse> bookingHistory, double expireAfterSeconds = 3)
        {
            // Set expiration time for the cache (default is 3 minutes)
            var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds);
            // Save data to Redis cache
            return _cacheService.AddOrUpdateAsync(nameof(BookingHistoryResponse), bookingHistory, expirationTime);
        }

        private Dictionary<Guid, BookingHistoryResponse> GetKeyValues()
        {
            // Attempt to retrieve data from Redis cache
            var data = _cacheService.Get<IEnumerable<BookingHistoryResponse>>(nameof(BookingHistoryResponse));
            // Convert data to Dictionary or return empty Dictionary if no data
            return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<Guid, BookingHistoryResponse>();
        }

        #endregion
    }
}