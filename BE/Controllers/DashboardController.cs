using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services;

namespace PRN231.ExploreNow.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = new
            {
                GetTotalCompletedAmount = await _dashboardService.GetTotalCompletedAmountAsync(),
                SuccessfulPaymentsCount = await _dashboardService.GetSuccessfulPaymentsCountAsync(),
                ApprovedPostsCount = await _dashboardService.GetApprovedPostsCountAsync(),
                PendingPostsCount = await _dashboardService.GetPendingPostsCountAsync(),
                RejectedPostsCount = await _dashboardService.GetRejectedPostsCountAsync(),
                ActiveUsersCount = await _dashboardService.GetActiveCustomersCountAsync(),
                TotalPassengers = await _dashboardService.GetTotalPassengersAsync(),
                ActiveTransportationsCount = await _dashboardService.GetActiveTransportationsCountAsync(),
                MoodUsage = await _dashboardService.GetMoodUsageCountAsync(),
                EarningsByDay = await _dashboardService.GetEarningsByDayAsync(),
                EarningsByMonth = await _dashboardService.GetEarningsByMonthAsync(),
                OrderHistory = await _dashboardService.GetOrderHistoryAsync()
            };

            return Ok(result);
        }
    }
}
