using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface ITourRepository : IBaseRepository<Tour>
	{
		Task<List<Tour>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm);
		Task<List<Tour>> GetTourBookingHistoryAsync(string userId, int page, int pageSize, PaymentTransactionStatus? filterTransactionStatus, string? searchTerm = null);
	}
}
