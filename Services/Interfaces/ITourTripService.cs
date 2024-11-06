using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface ITourTripService
	{
		Task<(List<TourTripResponse> Items, int TotalCount)> GetAllTourTripAsync(int page, int pageSize, bool? sortByPrice, string? searchTerm);
		Task<TourTripResponse> GetTourTripByIdAsync(Guid tourTripId);
		Task<TourDetailsResponse> GetTourTripsByTourIdAsync(Guid tourId);
		Task<List<TourTripResponse>> CreateBatchTourTrips(List<TourTripRequest> tourTripRequests);
		Task<TourTripResponse> UpdateTourTrip(Guid tourTripId, TourTripRequest updateRequest);
		Task<bool> DeleteTourTrip(Guid id);
		Task<bool> ValidateTotalSeatsTransportation(Guid tourId, int newTotalSeats);
	}
}
