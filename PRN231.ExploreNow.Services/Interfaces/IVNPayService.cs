using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface IVNPayService
	{
		Task<TourPackageDetailsResponse> GetTourPackageDetails(Guid tourId);
		Task<string> CreateEmbeddedPaymentForTourTrip(PaymentRequest request);
		Task<VNPayResponse> ProcessPaymentCallback(IQueryCollection query);
		Task<(List<TourPackageHistoryResponse> Items, int TotalCount)> GetUserTourHistory(
			int page,
			int pageSize,
			PaymentTransactionStatus? filterByPaymentStatus = null,
			string? searchTerm = null);
	}
}
