using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

		public PaymentController(IVNPayService vnPayService)
		{
			_vnPayService = vnPayService;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreatePayment([FromBody] VNPayRequest request)
		{
			try
			{
				var paymentUrl = await _vnPayService.CreatePaymentForTourTrip(request);

				return Ok(new BaseResponse<string>
				{
					IsSucceed = true,
					Result = paymentUrl,
					Message = "Payment URL created successfully."
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
		[Authorize]
		public async Task<IActionResult> PaymentCallback()
		{
			try
			{
				var response = await _vnPayService.ProcessPaymentCallback(Request.Query);

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
