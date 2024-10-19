using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.OtherObjects;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
	public class VNPayService : IVNPayService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;

		public VNPayService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
			UserManager<ApplicationUser> userManager,
			IMapper mapper, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
			_configuration = configuration;
		}

		public async Task<string> CreatePaymentForTourTrip(VNPayRequest request)
		{
			var user = await GetAuthenticatedUserAsync();
			var tourTrip = await _unitOfWork.GetRepository<ITourTripRepository>().GetQueryable()
						  .Include(tt => tt.Tour)
						  .ThenInclude(t => t.Transportations)
						  .FirstOrDefaultAsync(tt => tt.Id == request.TourTripId);

			var amount = CalculateTourTripTotalPrice(tourTrip);

			var vnPayRequest = _mapper.Map<VNPayRequest>(tourTrip);

			vnPayRequest.Amount = amount;
			vnPayRequest.Description = $"Thanh toán cho tour trip {tourTrip.Id}";

			var paymentUrl = CreatePaymentUrl(_httpContextAccessor.HttpContext, vnPayRequest);

			// Create a pending payment record
			var payment = new Payment
			{
				Amount = (decimal)amount,
				Code = GenerateUniqueCode(),
				TourTripId = tourTrip.Id,
				Status = PaymentStatus.PENDING,
				CreatedDate = DateTime.Now,
				UserId = user.Id,
				CreatedBy = user.UserName,
				LastUpdatedBy = user.UserName,
				LastUpdatedDate = DateTime.Now,
				PaymentMethod = "VnPay",
				PaymentTransactionId = Guid.NewGuid().ToString()
			};

			await _unitOfWork.GetRepository<IPaymentRepository>().AddAsync(payment);
			await _unitOfWork.SaveChangesAsync();
			return paymentUrl;
		}

		public async Task<VNPayResponse> ProcessPaymentCallback(IQueryCollection query)
		{
			var vnPayResponse = PaymentExecute(query);

			var payment = await _unitOfWork.GetRepository<IPaymentRepository>().GetQueryable()
						 .FirstOrDefaultAsync(p => p.TourTripId == vnPayResponse.TourTripId);

			payment.Status = vnPayResponse.Success ? PaymentStatus.COMPLETED : PaymentStatus.FAILED;
			payment.PaymentTransactionId = vnPayResponse.TransactionId.ToString();
			await _unitOfWork.GetRepository<IPaymentRepository>().UpdateAsync(payment);

			if (vnPayResponse.Success)
			{
				var tourTrip = await _unitOfWork.GetRepository<ITourTripRepository>().GetById(vnPayResponse.TourTripId);
				tourTrip.BookedSeats++;
				if (tourTrip.BookedSeats == tourTrip.TotalSeats)
				{
					tourTrip.TripStatus = TripStatus.FULLYBOOKED;
				}
				await _unitOfWork.GetRepository<ITourTripRepository>().UpdateAsync(tourTrip);
			}

			return new VNPayResponse
			{
				Success = vnPayResponse.Success,
				TransactionId = vnPayResponse.TransactionId
			};
		}

		private async Task<ApplicationUser> GetAuthenticatedUserAsync()
		{
			var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
			if (user == null)
			{
				throw new UnauthorizedAccessException("User not authenticated");
			}
			return user;
		}

		// Generate random Code
		private static string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

		private static double CalculateTourTripTotalPrice(TourTrip tourTrip)
		{
			double totalPrice = (double)tourTrip.Tour.TotalPrice;

			// Thêm chi phí vận chuyển
			if (tourTrip.Tour.Transportations != null)
			{
				totalPrice += tourTrip.Tour.Transportations.Sum(t => (double)t.Price);
			}

			// Áp dụng phụ phí cuối tuần nếu có
			if (tourTrip.TripDate.DayOfWeek == DayOfWeek.Saturday || tourTrip.TripDate.DayOfWeek == DayOfWeek.Sunday)
			{
				totalPrice *= 1.1;
			}

			// Áp dụng giảm giá nếu ít người đặt
			if (tourTrip.BookedSeats < tourTrip.TotalSeats * 0.5)
			{
				totalPrice *= 0.9;
			}

			// Làm tròn đến hàng nghìn đồng
			return Math.Round(totalPrice / 1000, 0) * 1000;
		}

		private string CreatePaymentUrl(HttpContext context, VNPayRequest model)
		{
			var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
			var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
			var tick = DateTime.Now.Ticks.ToString();
			var vnpay = new VNPayLibrary();

			vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
			vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
			vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
			vnpay.AddRequestData("vnp_Amount", ((int)(model.Amount * 100)).ToString());
			vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
			vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
			vnpay.AddRequestData("vnp_IpAddr", vnpay.GetIpAddress(context));
			vnpay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
			vnpay.AddRequestData("vnp_OrderInfo", $"{model.FullName} {model.Description} {model.Amount}");
			vnpay.AddRequestData("vnp_OrderType", "other");
			vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:PaymentBackReturnUrl"]);
			vnpay.AddRequestData("vnp_TxnRef", tick);

			var paymentUrl = vnpay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);
			return paymentUrl;
		}

		public VNPayResponse PaymentExecute(IQueryCollection collections)
		{
			var pay = new VNPayLibrary();
			var response = pay.GetFullResponseData(collections, _configuration["VnPay:HashSecret"]);

			return response;
		}

		private Guid ParseGuid(string input)
		{
			if (Guid.TryParse(input, out Guid result))
			{
				return result;
			}
			return Guid.Empty;
		}
	}
}
