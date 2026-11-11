using System;
using System.Collections.Generic;
using System.Linq;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class BillingRevenueTests
    {
        [Fact]
        public void DateFilter_IncludesEntireLastDay_ExcludesActive()
        {
            DateTime day = new DateTime(2026, 11, 1);
            var sessions = new List<GamingSession>
            {
                new() { SessionId = 1, StationId = 1, EndTime = day, Status = "Completed", TotalCost = 10000 },
                new() { SessionId = 2, StationId = 1, EndTime = day.AddDays(1).AddTicks(-1), Status = "Completed", TotalCost = 20000 },
                new() { SessionId = 3, StationId = 2, EndTime = day.AddDays(1), Status = "Completed", TotalCost = 30000 },
                new() { SessionId = 4, StationId = 2, EndTime = day.AddHours(12), Status = "Active", TotalCost = 50000 },
                new() { SessionId = 5, StationId = 2, EndTime = null, Status = "Completed", TotalCost = 60000 }
            };

            var result = DBBilling.FilterCompletedRevenueSessions(sessions, day, day);

            Assert.Equal(new[] { 1, 2 }, result.Select(s => s.SessionId).ToArray());
            Assert.Equal(30000m, result.Sum(s => s.TotalCost));
        }

        [Fact]
        public void GroupByStation_RanksHighestRevenueFirst()
        {
            DateTime end = new DateTime(2026, 11, 1);
            var sessions = new[]
            {
                new GamingSession { StationId = 1, EndTime = end, Status = "Completed", TotalCost = 10000 },
                new GamingSession { StationId = 2, EndTime = end, Status = "Completed", TotalCost = 25000 },
                new GamingSession { StationId = 1, EndTime = end, Status = "Completed", TotalCost = 30000 },
                new GamingSession { StationId = 3, EndTime = null, Status = "Active", TotalCost = 999999 }
            };
            var revenue = DBBilling.GroupRevenueByStation(sessions);

            Assert.Equal(1, revenue[0].StationId);
            Assert.Equal(40000m, revenue[0].Revenue);
            Assert.Equal(2, revenue[0].SessionCount);
            Assert.Equal(2, revenue[1].StationId);
            Assert.Equal(2, revenue.Count);
        }

        [Fact]
        public void TextReport_ContainsOnlyFilteredCompletedSessions()
        {
            DateTime day = new DateTime(2026, 11, 1);
            var rows = new[]
            {
                new GamingSession { SessionId = 101, StationId = 7, StartTime = day, EndTime = day.AddHours(2), Status = "Completed", PlayCost = 10000, ServiceCost = 5000, TotalCost = 15000 },
                new GamingSession { SessionId = 102, StationId = 8, StartTime = day, EndTime = null, Status = "Active", TotalCost = 99000 }
            };
            string report = DBBilling.BuildRevenueShiftReport(rows, day, day);

            Assert.Contains("BÁO CÁO DOANH THU", report);
            Assert.Contains("PC-07", report);
            Assert.DoesNotContain("PC-08", report);
            Assert.Contains("TỔNG DOANH THU: 15.000 VNĐ", report);
        }

        [Fact]
        public void DateFilter_RejectsInvertedDateRange()
        {
            Assert.Throws<ArgumentException>(() =>
                DBBilling.FilterCompletedRevenueSessions(
                    Array.Empty<GamingSession>(), new DateTime(2026, 11, 2), new DateTime(2026, 11, 1)));
        }
    }
}
