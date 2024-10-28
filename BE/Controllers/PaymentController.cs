using FluentValidation;
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

		public PaymentController(IVNPayService vnPayService, IValidator<PaymentRequest> validator)
		{
			_vnPayService = vnPayService;
			_validator = validator;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetTourHistory(
			[FromQuery(Name = "page-number")] int page = 1,
			[FromQuery(Name = "page-size")] int pageSize = 10,
			[FromQuery(Name = "filter-status")] PaymentTransactionStatus? status = null,
			[FromQuery(Name = "search-term")] string? searchTerm = null)
		{
			try
			{
				var tourHistory = await _vnPayService.GetUserTourHistory(
					page,
					pageSize,
					status,
					searchTerm);

				if (tourHistory == null || !tourHistory.Any())
				{
					return NotFound(new BaseResponse<List<TourPackageHistoryResponse>>
					{
						IsSucceed = false,
						Message = "No tour history found for the current user."
					});
				}

				return Ok(new BaseResponse<List<TourPackageHistoryResponse>>
				{
					IsSucceed = true,
					Result = tourHistory,
					Message = "Tour history retrieved successfully."
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

		[HttpGet("{id}")]
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
						Message = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
					});
				}

				var paymentUrl = await _vnPayService.CreatePaymentForTourTrip(request);
				return CreatedAtAction(nameof(GetTourPackageDetails), new { id = request.TourTripId }, new BaseResponse<string>
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
	}
}
