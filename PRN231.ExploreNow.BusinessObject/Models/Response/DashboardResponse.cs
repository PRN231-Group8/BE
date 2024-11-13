using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
    public class DashboardResponse
    {
    }
    public class MoodUsageResult
    {
        public string MoodTag { get; set; }
        public int Count { get; set; }
    }

    public class DailyEarningsResult
    {
        public DateTime Date { get; set; }
        public decimal TotalEarnings { get; set; }
    }

    public class MonthlyEarningsResult
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalEarnings { get; set; }
    }
}
