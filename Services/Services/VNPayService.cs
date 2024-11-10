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

        public async Task<(List<BookingHistoryResponse> Items, int TotalCount)> GetSuccessfulTransactionsWithTourInfo(
            int page,
            int pageSize,
            PaymentTransactionStatus? filterByStatus = null,
            string? searchTerm = null)
        {
            var user = await GetAuthenticatedUserAsync();
            var zeroBasedPage = page - 1;

            var query = _unitOfWork.GetRepository<ITransactionRepository>()
                .GetQueryable()
                .Where(t => t.Status == PaymentTransactionStatus.SUCCESSFUL &&
                            t.UserId == user.Id);
            
            if (filterByStatus.HasValue)
            {
                query = query.Where(t => t.Status == filterByStatus.Value);
            }
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t =>
                    t.Amount.ToString().Contains(searchTerm) ||
                    t.Payment.PaymentMethod.Contains(searchTerm) ||
                    t.Payment.TourTrip.Tour.Title.Contains(searchTerm)
                );
            }
            
            var totalCount = await query.CountAsync();
            
            var transactions = await query
                .Skip(zeroBasedPage * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var transactionDetails = new List<BookingHistoryResponse>();
            
            foreach (var transaction in transactions)
            {
                var payment = await _unitOfWork.GetRepository<IPaymentRepository>()
                    .GetById(transaction.PaymentId);
                var tourTrip = await _unitOfWork.GetRepository<ITourTripRepository>()
                    .GetById(payment.TourTripId);
                var tour = await _unitOfWork.GetRepository<ITourRepository>()
                    .GetById(tourTrip.TourId);

                var tourResponse = _mapper.Map<TourResponse>(tour);

                var transactionDetail = new BookingHistoryResponse
                {
                    TransactionAmount = transaction.Amount,
                    TransactionStatus = transaction.Status,
                    PaymentMethod = payment.PaymentMethod,
                    NumberOfPassengers = payment.NumberOfPassengers,
                    TourTripDate = tourTrip.TripDate,
                    Tour = tourResponse,
                };

                transactionDetails.Add(transactionDetail);
            }

            return (transactionDetails, totalCount);
        }


        public async Task<(List<TourPackageHistoryResponse> Items, int TotalCount)> GetUserTourHistory(
            int page,
            int pageSize,
            PaymentTransactionStatus? filterByPaymentStatus = null,
            string? searchTerm = null)
        {
            var user = await GetAuthenticatedUserAsync();

            var (tours, totalCount) = await _unitOfWork.GetRepository<ITourRepository>()
                .GetTourBookingHistoryAsync(
                    user.Id,
                    page,
                    pageSize,
                    filterByPaymentStatus,
                    searchTerm);

            var mappedResults = _mapper.Map<List<TourPackageHistoryResponse>>(tours);
            return (mappedResults, totalCount);
        }

        public async Task<TourPackageDetailsResponse> GetTourPackageDetails(Guid tourId)
        {
            var tour = await _unitOfWork.GetRepository<ITourRepository>().GetQueryable()
                .AsSplitQuery()
                .Where(t => t.Id == tourId)
                .Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
                .Include(t => t.TourTimestamps.Where(ts => !ts.IsDeleted && !ts.Location.IsDeleted))
                .ThenInclude(ts => ts.Location)
                .ThenInclude(l => l.Photos.Where(p => !p.IsDeleted))
                .Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
                .Include(t => t.TourMoods.Where(tm => !tm.IsDeleted && !tm.Mood.IsDeleted))
                .ThenInclude(tm => tm.Mood)
                .SingleOrDefaultAsync();

            var tourPackageDetails = _mapper.Map<TourPackageDetailsResponse>(tour);

            tourPackageDetails.TourTrips = _mapper.Map<List<TourTripDetailsResponse>>(tour.TourTrips);
            tourPackageDetails.TourTimestamps = _mapper.Map<List<TourTimeStampResponse>>(tour.TourTimestamps);
            tourPackageDetails.Transportations = _mapper.Map<List<TransportationResponse>>(tour.Transportations);
            tourPackageDetails.Moods =
                _mapper.Map<List<MoodResponseWithoutTours>>(tour.TourMoods.Select(tm => tm.Mood));

            return tourPackageDetails;
        }

        public async Task<string> CreateEmbeddedPaymentForTourTrip(PaymentRequest request)
        {
            var user = await GetAuthenticatedUserAsync();
            var tourTrip = await _unitOfWork.GetRepository<ITourTripRepository>().GetQueryable()
                .Where(tt => tt.Id == request.TourTripId
                             && !tt.IsDeleted
                             && !tt.Tour.IsDeleted)
                .Include(tt => tt.Tour)
                .ThenInclude(t => t.Transportations.Where(tr => !tr.IsDeleted))
                .SingleOrDefaultAsync();

            // Validate remaining seats
            int remainingSeats = tourTrip.TotalSeats - tourTrip.BookedSeats;
            if (remainingSeats < request.NumberOfPassengers)
            {
                throw new InvalidOperationException(
                    $"Not enough seats available. Only {remainingSeats} seats remaining.");
            }

            var amount = CalculateTourTripTotalPrice(tourTrip, request.NumberOfPassengers);
            var paymentTransactionId = GenerateUniqueCode();

            // Create and save payment record
            var payment = await CreatePaymentRecord(user, tourTrip, amount, request.NumberOfPassengers,
                paymentTransactionId);

            // Create payment URL
            return CreatePaymentUrl(_httpContextAccessor.HttpContext,
                new VNPayRequest
                {
                    Amount = amount,
                    Description = $"Payment for Tour Trip {tourTrip.Id}",
                    FullName = user.UserName,
                    OrderId = paymentTransactionId
                }
            );
        }

        private async Task<Payment> CreatePaymentRecord(ApplicationUser user, TourTrip tourTrip, long amount,
            int numberOfPassengers, string paymentTransactionId)
        {
            var payment = new Payment
            {
                Amount = amount,
                Code = GenerateUniqueCode(),
                TourTripId = tourTrip.Id,
                Status = PaymentStatus.PENDING,
                CreatedDate = DateTime.Now,
                UserId = user.Id,
                CreatedBy = user.UserName,
                LastUpdatedBy = user.UserName,
                LastUpdatedDate = DateTime.Now,
                NumberOfPassengers = numberOfPassengers,
                PaymentMethod = "VnPay",
                PaymentTransactionId = paymentTransactionId
            };

            await _unitOfWork.GetRepository<IPaymentRepository>().AddAsync(payment);

            var transaction = new Transaction
            {
                Amount = payment.Amount,
                UserId = payment.UserId,
                PaymentId = payment.Id,
                Status = PaymentTransactionStatus.PENDING,
                CreatedDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now,
                CreatedBy = user.UserName,
                LastUpdatedBy = user.UserName,
                Code = GenerateUniqueCode()
            };

            await _unitOfWork.GetRepository<ITransactionRepository>().AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return payment;
        }

        private string CreatePaymentUrl(HttpContext context, VNPayRequest model)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var vnpay = new VNPayLibrary();

            vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", vnpay.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", $"{model.FullName} {model.Description} {model.Amount}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", model.OrderId);

            var paymentUrl =
                vnpay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);
            return paymentUrl;
        }

        public async Task<VNPayResponse> ProcessPaymentCallback(IQueryCollection query)
        {
            var paymentResponse = PaymentExecute(query);
            var vnpTxnRef = query["vnp_TxnRef"].ToString();

            var payment = await _unitOfWork.GetRepository<IPaymentRepository>().GetQueryable()
                .Where(p => p.PaymentTransactionId == vnpTxnRef && p.Status == PaymentStatus.PENDING
                                                                && !p.IsDeleted
                                                                && !p.TourTrip.IsDeleted
                                                                && !p.TourTrip.Tour.IsDeleted)
                .Include(p => p.TourTrip)
                .ThenInclude(tt => tt.Tour)
                .SingleOrDefaultAsync();

            if (payment == null)
            {
                return new VNPayResponse
                {
                    Success = false,
                    Message = "No pending payment found"
                };
            }

            var tourTrip = payment.TourTrip;
            var tour = tourTrip.Tour;

            var transaction = await _unitOfWork.GetRepository<ITransactionRepository>().GetQueryable()
                .Where(t => t.PaymentId == payment.Id && !t.IsDeleted)
                .SingleOrDefaultAsync();

            if (tour.Status != TourStatus.ACTIVE)
            {
                await UpdateFailedPayment(payment, transaction);
                return CreateFailedPaymentResponse(payment, paymentResponse,
                    "Tour is not active. Payment cannot be processed.");
            }

            if (tourTrip.TripStatus == TripStatus.FULLYBOOKED)
            {
                await UpdateFailedPayment(payment, transaction);
                return CreateFailedPaymentResponse(payment, paymentResponse,
                    "Tour trip is fully booked. Payment cannot be processed.");
            }

            if (paymentResponse.VnPayResponseCode == "00")
            {
                await UpdateSuccessfulPayment(payment, transaction, tourTrip);
                return CreateSuccessfulPaymentResponse(payment, paymentResponse);
            }
            else
            {
                await UpdateFailedPayment(payment, transaction);
                return CreateFailedPaymentResponse(payment, paymentResponse,
                    GetMessageFromResponseCode(paymentResponse.VnPayResponseCode));
            }
        }

        public VNPayResponse PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VNPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnPayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var orderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var amount = Convert.ToDouble(vnpay.GetResponseData("vnp_Amount")) / 100;

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VNPayResponse
                {
                    Success = false
                };
            }

            return new VNPayResponse
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId,
                PaymentId = vnPayTranId,
                TransactionId = vnPayTranId,
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode,
                Amount = amount
            };
        }

        #region Helper method

        private string GetMessageFromResponseCode(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "01" => "Giao dịch chưa hoàn tất",
                "02" => "Giao dịch bị lỗi",
                "04" => "Giao dịch đã được thực hiện nhưng chưa thành công",
                "05" => "VNPay đang xử lý giao dịch này",
                "06" => "VNPay đã gửi yêu cầu hoàn tiền",
                "07" => "Giao dịch bị nghi ngờ gian lận",
                "09" => "Giao dịch hoàn trả bị từ chối",
                "10" => "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Đã hết hạn chờ thanh toán",
                "12" => "Thẻ/Tài khoản của khách hàng bị khóa",
                "13" => "Khách nhập sai mật khẩu xác thực giao dịch",
                "24" => "Khách hàng hủy giao dịch",
                "51" => "Tài khoản của quý khách không đủ số dư để thực hiện giao dịch",
                "65" => "Tài khoản của quý khách đã vượt quá hạn mức giao dịch trong ngày",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "KH nhập sai mật khẩu thanh toán quá số lần quy định",
                "99" => "Lỗi khác",
                _ => "Lỗi không xác định"
            };
        }

        private async Task UpdateSuccessfulPayment(Payment payment, Transaction transaction, TourTrip tourTrip)
        {
            payment.Status = PaymentStatus.COMPLETED;
            payment.LastUpdatedDate = DateTime.UtcNow;
            transaction.Status = PaymentTransactionStatus.SUCCESSFUL;
            transaction.LastUpdatedDate = DateTime.UtcNow;

            // Update the number of booked seats based on the number of people in payment
            tourTrip.BookedSeats += payment.NumberOfPassengers;
            if (tourTrip.BookedSeats == tourTrip.TotalSeats)
            {
                tourTrip.TripStatus = TripStatus.FULLYBOOKED;
            }

            await _unitOfWork.GetRepository<ITourTripRepository>().UpdateAsync(tourTrip);
            await _unitOfWork.GetRepository<IPaymentRepository>().UpdateAsync(payment);
            await _unitOfWork.GetRepository<ITransactionRepository>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateFailedPayment(Payment payment, Transaction transaction)
        {
            payment.Status = PaymentStatus.FAILED;
            payment.LastUpdatedDate = DateTime.UtcNow;
            transaction.Status = PaymentTransactionStatus.FAILED;
            transaction.LastUpdatedDate = DateTime.UtcNow;

            await _unitOfWork.GetRepository<IPaymentRepository>().UpdateAsync(payment);
            await _unitOfWork.GetRepository<ITransactionRepository>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        private VNPayResponse CreateSuccessfulPaymentResponse(Payment payment, VNPayResponse paymentResponse)
        {
            return new VNPayResponse
            {
                Success = true,
                Message = "Payment successful",
                PaymentId = payment.Id.ToString(),
                OrderId = payment.PaymentTransactionId,
                Amount = (double)payment.Amount,
                TransactionId = paymentResponse.TransactionId
            };
        }

        private VNPayResponse CreateFailedPaymentResponse(Payment payment, VNPayResponse paymentResponse,
            string message)
        {
            return new VNPayResponse
            {
                Success = false,
                Message = message,
                PaymentId = payment.Id.ToString(),
                OrderId = payment.PaymentTransactionId,
                Amount = (double)payment.Amount,
                TransactionId = paymentResponse.TransactionId
            };
        }

        private static long CalculateTourTripTotalPrice(TourTrip tourTrip, int numberOfPassengers)
        {
            // Calculate price per person
            decimal tourPrice = tourTrip.Price * numberOfPassengers;
            decimal transportationPrice = tourTrip.Tour.Transportations.Sum(t => t.Price);

            // Calculate totalprice
            decimal totalPrice = tourPrice + transportationPrice;
            return (long)Math.Round(totalPrice / 1000, 0) * 1000;
        }

        // Generate random Code
        private static string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        private async Task<ApplicationUser> GetAuthenticatedUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return user;
        }

        #endregion
    }
}