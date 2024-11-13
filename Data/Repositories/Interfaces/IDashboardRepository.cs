using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
    public interface IDashboardRepository : IBaseRepository
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
