﻿using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
	public interface ITourRepository : IBaseRepository<Tour>
	{
		Task<(List<TourResponse> Items, int TotalCount)> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm);
		Task<(List<Tour> Items, int TotalCount)> GetTourBookingHistoryAsync(string userId, int page, int pageSize, PaymentTransactionStatus? filterTransactionStatus, string? searchTerm = null);
	}
}
