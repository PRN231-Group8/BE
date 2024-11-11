using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<decimal> GetTotalCompletedAmountAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.COMPLETED)
                .SumAsync(p => p.Amount);
        }
        public async Task<int> GetSuccessfulPaymentsCountAsync() =>
            await _context.Payments.CountAsync(p => p.Status == PaymentStatus.COMPLETED);

        public async Task<int> GetApprovedPostsCountAsync() =>
            await _context.Posts.CountAsync(p => p.Status == PostsStatus.Approved);

        public async Task<int> GetPendingPostsCountAsync() =>
            await _context.Posts.CountAsync(p => p.Status == PostsStatus.Pending);

        public async Task<int> GetRejectedPostsCountAsync() =>
            await _context.Posts.CountAsync(p => p.Status == PostsStatus.Rejected);
        public async Task<int> GetActiveCustomersCountAsync()
        {
            var usersInCustomerRole = await _userManager.GetUsersInRoleAsync("CUSTOMER");
            return usersInCustomerRole.Count(user => user.isActived);
        }

        public async Task<int> GetTotalPassengersAsync() => await _context.Payments.SumAsync(p => p.NumberOfPassengers);

        public async Task<int> GetActiveTransportationsCountAsync() =>
            await _context.Transportations.CountAsync(t => !t.IsDeleted);

        public async Task<List<MoodUsageResult>> GetMoodUsageCountAsync() =>
    await _context.TourMoods
        .GroupBy(tm => tm.Mood.MoodTag)
        .Select(g => new MoodUsageResult { MoodTag = g.Key, Count = g.Count() })
        .ToListAsync();

        public async Task<List<DailyEarningsResult>> GetEarningsByDayAsync() =>
            await _context.Transactions
                .Where(t => t.Status == PaymentTransactionStatus.SUCCESSFUL)
                .GroupBy(t => t.CreatedDate.Date)
                .Select(g => new DailyEarningsResult { Date = g.Key, TotalEarnings = g.Sum(t => t.Amount) })
                .ToListAsync();

        public async Task<List<MonthlyEarningsResult>> GetEarningsByMonthAsync() =>
            await _context.Transactions
                .Where(t => t.Status == PaymentTransactionStatus.SUCCESSFUL)
                .GroupBy(t => new { t.CreatedDate.Year, t.CreatedDate.Month })
                .Select(g => new MonthlyEarningsResult { Year = g.Key.Year, Month = g.Key.Month, TotalEarnings = g.Sum(t => t.Amount) })
                .ToListAsync();

        public async Task<List<OrderHistoryResponse>> GetOrderHistoryAsync()
        {
            return await _context.Transactions
                .Select(t => new OrderHistoryResponse
                {
                    Amount = t.Amount,
                    Status = t.Status.ToString(),
                    CreatedBy = t.CreatedBy,
                    CreatedDate = t.CreatedDate
                })
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

    }

}
