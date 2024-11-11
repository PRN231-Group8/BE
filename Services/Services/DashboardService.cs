using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response; // Assuming this is where the result models are
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Task<decimal> GetTotalCompletedAmountAsync() =>
            _unitOfWork.DashboardRepository.GetTotalCompletedAmountAsync();
        public Task<int> GetSuccessfulPaymentsCountAsync() =>
            _unitOfWork.DashboardRepository.GetSuccessfulPaymentsCountAsync();

        public Task<int> GetApprovedPostsCountAsync() =>
            _unitOfWork.DashboardRepository.GetApprovedPostsCountAsync();

        public Task<int> GetPendingPostsCountAsync() =>
            _unitOfWork.DashboardRepository.GetPendingPostsCountAsync();

        public Task<int> GetRejectedPostsCountAsync() =>
            _unitOfWork.DashboardRepository.GetRejectedPostsCountAsync();

        public Task<int> GetActiveCustomersCountAsync() =>
            _unitOfWork.DashboardRepository.GetActiveCustomersCountAsync();

        public Task<int> GetTotalPassengersAsync() =>
            _unitOfWork.DashboardRepository.GetTotalPassengersAsync();

        public Task<int> GetActiveTransportationsCountAsync() =>
            _unitOfWork.DashboardRepository.GetActiveTransportationsCountAsync();

        public Task<List<MoodUsageResult>> GetMoodUsageCountAsync() =>
            _unitOfWork.DashboardRepository.GetMoodUsageCountAsync();

        public Task<List<DailyEarningsResult>> GetEarningsByDayAsync() =>
            _unitOfWork.DashboardRepository.GetEarningsByDayAsync();

        public Task<List<MonthlyEarningsResult>> GetEarningsByMonthAsync() =>
            _unitOfWork.DashboardRepository.GetEarningsByMonthAsync();

        public Task<List<OrderHistoryResponse>> GetOrderHistoryAsync() =>
            _unitOfWork.DashboardRepository.GetOrderHistoryAsync();
    }
}
