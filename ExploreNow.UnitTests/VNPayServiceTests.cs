using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Security.Cryptography;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Services;
using PRN231.ExploreNow.UnitTests.OtherObjects;
using System.Security.Claims;
using System.Text;
using Xunit;
using System.Net;
using PRN231.ExploreNow.BusinessObject.OtherObjects;

namespace PRN231.ExploreNow.UnitTests
{
	public class VNPayServiceTests
	{
		private readonly Mock<IUnitOfWork> _mockUnitOfWork;
		private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
		private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
		private readonly Mock<IMapper> _mockMapper;
		private readonly Mock<IConfiguration> _mockConfiguration;
		private readonly VNPayService _vnPayService;

		public VNPayServiceTests()
		{
			_mockUnitOfWork = new Mock<IUnitOfWork>();
			_mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
			_mockUserManager = MockUserManager();
			_mockMapper = new Mock<IMapper>();
			_mockConfiguration = new Mock<IConfiguration>();

			_vnPayService = new VNPayService(
				_mockUnitOfWork.Object,
				_mockHttpContextAccessor.Object,
				_mockUserManager.Object,
				_mockMapper.Object,
				_mockConfiguration.Object
			);

			SetupConfiguration();
		}

		private void SetupConfiguration()
		{
			_mockConfiguration.Setup(c => c["TimeZoneId"]).Returns("SE Asia Standard Time");
			_mockConfiguration.Setup(c => c["VnPay:Version"]).Returns("2.1.0");
			_mockConfiguration.Setup(c => c["VnPay:Command"]).Returns("pay");
			_mockConfiguration.Setup(c => c["VnPay:TmnCode"]).Returns("1R0NMGOT");
			_mockConfiguration.Setup(c => c["VnPay:HashSecret"]).Returns("NFT99RL4QBFPPSO49D0NTZI1Z15YDT1Q");
			_mockConfiguration.Setup(c => c["VnPay:CurrCode"]).Returns("VND");
			_mockConfiguration.Setup(c => c["VnPay:BaseUrl"]).Returns("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
			_mockConfiguration.Setup(c => c["VnPay:Locale"]).Returns("vn");
			_mockConfiguration.Setup(c => c["VnPay:PaymentBackReturnUrl"]).Returns("https://localhost:7130/api/payments/callback");
		}

		[Fact]
		public async Task CreatePaymentForTourTrip_ValidRequest_ReturnsPaymentUrl()
		{
			// Arrange
			var user = new ApplicationUser { Id = "testUserId", UserName = "testUser" };
			var tourTrip = new TourTrip
			{
				Id = Guid.NewGuid(),
				Price = 1000000,
				Tour = new Tour
				{
					Transportations = new List<Transportation>
					{
						new Transportation { Price = 500000 }
					}
				}
			};

			SetupMockUser(user);
			SetupMockTourTrip(tourTrip);
			SetupMockRepositories();

			var paymentRequest = new PaymentRequest { TourTripId = tourTrip.Id };

			// Act
			var result = await _vnPayService.CreatePaymentForTourTrip(paymentRequest);

			// Assert
			Assert.NotNull(result);
			Assert.Contains("sandbox.vnpayment.vn", result);
			Assert.Contains("vnp_Amount=1500000", result);
		}

		[Fact]
		public async Task ProcessPaymentCallback_SuccessfulPayment_ReturnsSuccessResponse()
		{
			// Arrange
			var tourTrip = new TourTrip
			{
				Id = Guid.NewGuid(),
				BookedSeats = 5,
				TotalSeats = 10,
				TripStatus = TripStatus.OPEN,
				Tour = new Tour
				{
					Id = Guid.NewGuid(),
					Status = TourStatus.ACTIVE
				}
			};

			var payment = new Payment
			{
				Id = Guid.NewGuid(),
				PaymentTransactionId = "TEST123",
				Amount = 1500000,
				Status = PaymentStatus.PENDING,
				TourTripId = tourTrip.Id,
				TourTrip = tourTrip
			};

			var transaction = new Transaction
			{
				Id = Guid.NewGuid(),
				PaymentId = payment.Id,
				Status = PaymentTransactionStatus.PENDING
			};

			SetupMockPaymentAndTransaction(payment, transaction);
			SetupMockTourTrip(tourTrip);

			// Create sorted dictionary for proper hash calculation
			var vnp_Params = new SortedList<string, string>(new VnPayCompare())
			{
				{ "vnp_Amount", "150000000" }, // Amount in VNPay format (x100)
				{ "vnp_BankCode", "NCB" },
				{ "vnp_BankTranNo", "VNP13875753" },
				{ "vnp_CardType", "ATM" },
				{ "vnp_OrderInfo", $"Payment for tour trip - {payment.Id}" },
				{ "vnp_PayDate", "20240328183520" },
				{ "vnp_ResponseCode", "00" },
				{ "vnp_TmnCode", "1R0NMGOT" },
				{ "vnp_TransactionNo", "13875753" },
				{ "vnp_TransactionStatus", "00" },
				{ "vnp_TxnRef", "TEST123" },
				{ "vnp_Version", "2.1.0" }
			};

			// Calculate VNPay hash using the same method as VNPayLibrary
			var signData = string.Join("&", vnp_Params
				.Where(kv => !string.IsNullOrEmpty(kv.Value))
				.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

			var secretKey = _mockConfiguration.Object["VnPay:HashSecret"];
			var hash = new StringBuilder();
			var keyBytes = Encoding.UTF8.GetBytes(secretKey);
			var inputBytes = Encoding.UTF8.GetBytes(signData);
			using (var hmac = new HMACSHA512(keyBytes))
			{
				var hashValue = hmac.ComputeHash(inputBytes);
				foreach (var theByte in hashValue)
				{
					hash.Append(theByte.ToString("x2"));
				}
			}
			var vnpSecureHash = hash.ToString().ToUpper();

			// Add secure hash to parameters
			vnp_Params.Add("vnp_SecureHash", vnpSecureHash);

			// Convert to query collection
			var queryCollection = new QueryCollection(new Dictionary<string, StringValues>(
				vnp_Params.ToDictionary(
					k => k.Key,
					v => new StringValues(v.Value)
				)
			));

			// Act
			var result = await _vnPayService.ProcessPaymentCallback(queryCollection);

			// Assert
			Assert.True(result.Success);
			Assert.Equal("Payment successful", result.Message);
			Assert.Equal(payment.Id.ToString(), result.PaymentId);
			Assert.Equal("TEST123", result.OrderId);
			Assert.Equal(1500000, result.Amount);
			Assert.Equal("13875753", result.TransactionId);

			// Verify repository updates
			_mockUnitOfWork.Verify(u => u.GetRepository<IPaymentRepository>().UpdateAsync(
				It.Is<Payment>(p => p.Status == PaymentStatus.COMPLETED)),
				Times.Once);

			_mockUnitOfWork.Verify(u => u.GetRepository<ITransactionRepository>().UpdateAsync(
				It.Is<Transaction>(t => t.Status == PaymentTransactionStatus.SUCCESSFUL)),
				Times.Once);

			_mockUnitOfWork.Verify(u => u.GetRepository<ITourTripRepository>().UpdateAsync(
				It.Is<TourTrip>(t => t.BookedSeats == 6)), // Original 5 + 1
				Times.Once);
		}

		[Fact]
		public async Task ProcessPaymentCallback_FailedPayment_ReturnsFailureResponse()
		{
			// Arrange
			var paymentId = Guid.NewGuid();
			var payment = new Payment
			{
				Id = paymentId,
				PaymentTransactionId = "TEST123",
				Amount = 1500000,
				Status = PaymentStatus.PENDING,
				TourTrip = new TourTrip
				{
					Tour = new Tour { Status = TourStatus.ACTIVE }
				}
			};

			var transaction = new Transaction
			{
				PaymentId = payment.Id,
				Status = PaymentTransactionStatus.PENDING
			};

			SetupMockPaymentAndTransaction(payment, transaction);

			// Create sorted dictionary for proper hash calculation
			var queryParams = new SortedList<string, string>
			{
				{ "vnp_Amount", "150000000" },
				{ "vnp_BankCode", "NCB" },
				{ "vnp_BankTranNo", "VNP13875753" },
				{ "vnp_CardType", "ATM" },
				{ "vnp_OrderInfo", $"Payment for tour trip - {payment.Id}" },
				{ "vnp_PayDate", "20240328183520" },
				{ "vnp_ResponseCode", "24" },  // Error code
				{ "vnp_TmnCode", "TestTmnCode" },
				{ "vnp_TransactionNo", "13875753" },
				{ "vnp_TransactionStatus", "24" },
				{ "vnp_TxnRef", "TEST123" }
			};

			// Calculate VNPay hash
			var inputData = new StringBuilder();
			foreach (var (key, value) in queryParams)
			{
				if (string.IsNullOrEmpty(value)) continue;
				inputData.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
			}

			var hashSecret = _mockConfiguration.Object["VnPay:HashSecret"];
			inputData.Remove(inputData.Length - 1, 1); // Remove last &
			var signData = hashSecret + inputData.ToString();
			var vnpSecureHash = GetVnPaySecureHash(signData);

			// Convert to query collection
			var queryDictionary = new Dictionary<string, StringValues>();
			foreach (var param in queryParams)
			{
				queryDictionary.Add(param.Key, new StringValues(param.Value));
			}
			queryDictionary.Add("vnp_SecureHash", new StringValues(vnpSecureHash));

			var mockQuery = new Mock<IQueryCollection>();
			mockQuery.Setup(x => x.Keys).Returns(queryDictionary.Keys);
			mockQuery.Setup(x => x.GetEnumerator()).Returns(queryDictionary.GetEnumerator());
			foreach (var (key, value) in queryDictionary)
			{
				mockQuery.Setup(x => x[key]).Returns(value);
			}

			// Act
			var result = await _vnPayService.ProcessPaymentCallback(mockQuery.Object);

			// Assert
			Assert.False(result.Success);
			Assert.Equal("Lỗi không xác định", result.Message);
			Assert.Equal(paymentId.ToString(), result.PaymentId);
		}

		private string GetVnPaySecureHash(string inputHash)
		{
			using var sha256 = SHA256.Create();
			var byteArray = Encoding.UTF8.GetBytes(inputHash);
			var hash = sha256.ComputeHash(byteArray);
			var sb = new StringBuilder();
			foreach (byte b in hash)
			{
				sb.Append(b.ToString("x2"));
			}
			return sb.ToString().ToUpper();
		}

		#region Helper Methods
		private Mock<UserManager<ApplicationUser>> MockUserManager()
		{
			var store = new Mock<IUserStore<ApplicationUser>>();
			var mockUserManager = new Mock<UserManager<ApplicationUser>>(
				store.Object,
				null, null, null, null, null, null, null, null
			);
			return mockUserManager;
		}

		private void SetupMockUser(ApplicationUser user)
		{
			var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id)
			}));

			var httpContext = new DefaultHttpContext();
			httpContext.User = claimsPrincipal;

			_mockHttpContextAccessor.Setup(x => x.HttpContext)
				.Returns(httpContext);

			_mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(user);
		}

		private void SetupMockTourTrip(TourTrip tourTrip)
		{
			var mockTourTripRepo = new Mock<ITourTripRepository>();
			var data = new List<TourTrip> { tourTrip };
			var queryable = data.AsQueryable();
			var mockSet = new Mock<DbSet<TourTrip>>();

			// Setup for sync operations
			mockSet.As<IQueryable<TourTrip>>()
				.Setup(m => m.Provider)
				.Returns(new TestAsyncQueryProvider<TourTrip>(queryable.Provider));
			mockSet.As<IQueryable<TourTrip>>()
				.Setup(m => m.Expression)
				.Returns(queryable.Expression);
			mockSet.As<IQueryable<TourTrip>>()
				.Setup(m => m.ElementType)
				.Returns(queryable.ElementType);
			mockSet.As<IQueryable<TourTrip>>()
				.Setup(m => m.GetEnumerator())
				.Returns(() => queryable.GetEnumerator());

			// Setup for async operations
			mockSet.As<IAsyncEnumerable<TourTrip>>()
				.Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
				.Returns(new TestAsyncEnumerator<TourTrip>(data.GetEnumerator()));

			mockTourTripRepo.Setup(r => r.GetQueryable(It.IsAny<CancellationToken>()))
				.Returns(mockSet.Object);

			_mockUnitOfWork.Setup(u => u.GetRepository<ITourTripRepository>())
				.Returns(mockTourTripRepo.Object);
		}

		private void SetupMockPaymentAndTransaction(Payment payment, Transaction transaction)
		{
			// Setup Payment Repository
			var mockPaymentRepo = new Mock<IPaymentRepository>();
			var payments = new List<Payment> { payment };
			var paymentsQueryable = payments.AsQueryable();
			var mockPaymentSet = new Mock<DbSet<Payment>>();

			mockPaymentSet.As<IQueryable<Payment>>()
				.Setup(m => m.Provider)
				.Returns(new TestAsyncQueryProvider<Payment>(paymentsQueryable.Provider));
			mockPaymentSet.As<IQueryable<Payment>>()
				.Setup(m => m.Expression)
				.Returns(paymentsQueryable.Expression);
			mockPaymentSet.As<IQueryable<Payment>>()
				.Setup(m => m.ElementType)
				.Returns(paymentsQueryable.ElementType);
			mockPaymentSet.As<IQueryable<Payment>>()
				.Setup(m => m.GetEnumerator())
				.Returns(() => paymentsQueryable.GetEnumerator());

			mockPaymentSet.As<IAsyncEnumerable<Payment>>()
				.Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
				.Returns(new TestAsyncEnumerator<Payment>(payments.GetEnumerator()));

			mockPaymentRepo.Setup(r => r.GetQueryable(It.IsAny<CancellationToken>()))
				.Returns(mockPaymentSet.Object);

			// Setup Transaction Repository
			var mockTransactionRepo = new Mock<ITransactionRepository>();
			var transactions = new List<Transaction> { transaction };
			var transactionsQueryable = transactions.AsQueryable();
			var mockTransactionSet = new Mock<DbSet<Transaction>>();

			mockTransactionSet.As<IQueryable<Transaction>>()
				.Setup(m => m.Provider)
				.Returns(new TestAsyncQueryProvider<Transaction>(transactionsQueryable.Provider));
			mockTransactionSet.As<IQueryable<Transaction>>()
				.Setup(m => m.Expression)
				.Returns(transactionsQueryable.Expression);
			mockTransactionSet.As<IQueryable<Transaction>>()
				.Setup(m => m.ElementType)
				.Returns(transactionsQueryable.ElementType);
			mockTransactionSet.As<IQueryable<Transaction>>()
				.Setup(m => m.GetEnumerator())
				.Returns(() => transactionsQueryable.GetEnumerator());

			mockTransactionSet.As<IAsyncEnumerable<Transaction>>()
				.Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
				.Returns(new TestAsyncEnumerator<Transaction>(transactions.GetEnumerator()));

			mockTransactionRepo.Setup(r => r.GetQueryable(It.IsAny<CancellationToken>()))
				.Returns(mockTransactionSet.Object);

			_mockUnitOfWork.Setup(u => u.GetRepository<IPaymentRepository>())
				.Returns(mockPaymentRepo.Object);
			_mockUnitOfWork.Setup(u => u.GetRepository<ITransactionRepository>())
				.Returns(mockTransactionRepo.Object);
		}

		private void SetupMockRepositories()
		{
			var mockPaymentRepo = new Mock<IPaymentRepository>();
			mockPaymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>()))
				.Returns(Task.CompletedTask);

			var mockTransactionRepo = new Mock<ITransactionRepository>();
			mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
				.Returns(Task.CompletedTask);

			_mockUnitOfWork.Setup(u => u.GetRepository<IPaymentRepository>())
				.Returns(mockPaymentRepo.Object);
			_mockUnitOfWork.Setup(u => u.GetRepository<ITransactionRepository>())
				.Returns(mockTransactionRepo.Object);
			_mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			// Setup success response for SaveChanges
			_mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);
		}
		#endregion
	}
}
