using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response; // Assuming this is where the result models are
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services
{
    public interface IDashboardService
    {
        Task<decimal> GetTotalCompletedAmountAsync();
        Task<int> GetSuccessfulPaymentsCountAsync();
        Task<int> GetApprovedPostsCountAsync();
        Task<int> GetPendingPostsCountAsync();
        Task<int> GetRejectedPostsCountAsync();
        Task<int> GetActiveCustomersCountAsync();
        Task<int> GetTotalPassengersAsync();
        Task<int> GetActiveTransportationsCountAsync();
        Task<List<MoodUsageResult>> GetMoodUsageCountAsync();
        Task<List<DailyEarningsResult>> GetEarningsByDayAsync();
        Task<List<MonthlyEarningsResult>> GetEarningsByMonthAsync();
        Task<List<OrderHistoryResponse>> GetOrderHistoryAsync();
    }
}
