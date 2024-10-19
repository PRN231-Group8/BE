using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface IVNPayService
	{
		Task<string> CreatePaymentForTourTrip(VNPayRequest request);
		Task<VNPayResponse> ProcessPaymentCallback(IQueryCollection query);
	}
}
