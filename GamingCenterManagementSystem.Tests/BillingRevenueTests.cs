using System;
using System.Collections.Generic;
using System.Linq;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    [Collection("GamingCenterSequential")]
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
        [Fact]
        public void GroupRevenueByDay_UsesEndDate_AndIgnoresActiveSessions()
        {
            var day1 = new DateTime(2026, 11, 10);
            var day2 = new DateTime(2026, 11, 11);
            GamingSession[] sessions =
            {
                new() { SessionId = 1, EndTime = day1.AddHours(23), Status = "Completed", PlayCost = 10000, ServiceCost = 5000, TotalCost = 15000 },
                new() { SessionId = 2, EndTime = day1.AddHours(2), Status = "Completed", PlayCost = 12000, ServiceCost = 8000, TotalCost = 20000 },
                new() { SessionId = 3, EndTime = day2, Status = "Completed", PlayCost = 25000, ServiceCost = 0, TotalCost = 25000 },
                new() { SessionId = 4, EndTime = day2, Status = "Active", PlayCost = 99999, ServiceCost = 0, TotalCost = 99999 }
            };

            var result = DBBilling.GroupRevenueByDay(sessions);

            Assert.Equal(2, result.Count);
            Assert.Equal(day2, result[0].Date);
            Assert.Equal(day1, result[1].Date);
            Assert.Equal(2, result[1].SessionCount);
            Assert.Equal(22000m, result[1].PlayRevenue);
            Assert.Equal(13000m, result[1].ServiceRevenue);
            Assert.Equal(35000m, result[1].Revenue);
        }

        [Fact]
        public void GroupRevenueByTariffType_IsCaseInsensitive_AndSortsRevenue()
        {
            var day = new DateTime(2026, 11, 11);
            GamingSession[] sessions =
            {
                new() { EndTime = day, Status = "Completed", TariffType = "Hourly", PlayCost = 10000, TotalCost = 10000 },
                new() { EndTime = day, Status = "Completed", TariffType = "hourly", PlayCost = 20000, TotalCost = 20000 },
                new() { EndTime = day, Status = "Completed", TariffType = "NightCombo", PlayCost = 45000, TotalCost = 45000 },
                new() { EndTime = day, Status = "Active", TariffType = "MemberTier", TotalCost = 99999 }
            };

            var result = DBBilling.GroupRevenueByTariffType(sessions);

            Assert.Equal(2, result.Count);
            Assert.Equal("NightCombo", result[0].TariffType);
            Assert.Equal(45000m, result[0].Revenue);
            Assert.Equal(2, result[1].SessionCount);
            Assert.Equal(30000m, result[1].Revenue);
        }

        [Fact]
        public void RevenueTxt_KeepsOriginalSections_AndAddsDailyAndTariffSummaries()
        {
            var day = new DateTime(2026, 11, 11);
            var sessions = new[]
            {
                new GamingSession { SessionId = 12, StationId = 7,
                    StartTime = day.AddHours(9), EndTime = day.AddHours(10),
                    Status = "Completed", TariffType = "Hourly",
                    PlayCost = 10000m, ServiceCost = 5000m, TotalCost = 15000m }
            };

            string report = DBBilling.BuildRevenueShiftReport(sessions, day, day);

            Assert.Contains("THỐNG KÊ DOANH THU THEO MÁY", report);
            Assert.Contains("THỐNG KÊ DOANH THU THEO NGÀY", report);
            Assert.Contains("THỐNG KÊ DOANH THU THEO GÓI CƯỚC", report);
            Assert.Contains("TỔNG DOANH THU: 15.000 VNĐ", report);
            Assert.Contains("PC-07", report);
            Assert.Contains("Hourly", report);
        }

        [Fact]
        public void Receipt_PrintsItemsVatAndTotalInDong()
        {
            var items = new List<ReceiptItem>
            {
                new() { ItemName = "Mì xào bò", Quantity = 2, UnitPrice = 30000m },
                new() { ItemName = "Sting dâu", Quantity = 1, UnitPrice = 15000m }
            };

            var lines = ThermalReceiptFormatter.FormatReceipt("Cyber X", "HD001", "Máy 05", "staff01", items)
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            Assert.Contains("CYBER X", lines);
            Assert.Contains(lines, line => line.StartsWith("Mì xào bò") && line.EndsWith("60.000"));
            Assert.Contains("Cộng tiền hàng: 75.000 đ", lines);
            Assert.Contains("Thuế GTGT (8%): 6.000 đ", lines);
            Assert.Contains("TỔNG CỘNG: 81.000 đ", lines);
            Assert.All(lines, line => Assert.True(line.Length <= 40));
        }

        [Fact]
        public void Receipt_FractionalPricesMatchPrintedTotals()
        {
            var items = new List<ReceiptItem>
            {
                new() { ItemName = "A", Quantity = 2, UnitPrice = 1000.25m },
                new() { ItemName = "B", Quantity = 1, UnitPrice = 1000.50m }
            };

            var lines = ThermalReceiptFormatter.FormatReceipt("Cyber X", "HD003", "Máy 01", "staff01", items)
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            Assert.Contains(lines, line => line.StartsWith("A ") && line.Contains("1.000,25") && line.EndsWith("2.000,5"));
            Assert.Contains(lines, line => line.StartsWith("B ") && line.EndsWith("1.000,5"));
            Assert.Contains("Cộng tiền hàng: 3.001 đ", lines);
            Assert.Contains("Thuế GTGT (8%): 240 đ", lines);
            Assert.Contains("TỔNG CỘNG: 3.241 đ", lines);
        }

        [Fact]
        public void Receipt_EmptyOrderPrintsZeroForEveryTotal()
        {
            var lines = ThermalReceiptFormatter.FormatReceipt("Cyber X", "HD002", "Mang về", "staff01",
                new List<ReceiptItem>(), 10m).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            Assert.Contains("Cộng tiền hàng: 0 đ", lines);
            Assert.Contains("Thuế GTGT (10%): 0 đ", lines);
            Assert.Contains("TỔNG CỘNG: 0 đ", lines);
        }

        [Fact]
        public void LowStock_IncludesItemsAtOrBelowThreshold()
        {
            var items = new[]
            {
                new ServiceItem { StockQuantity = 4 },
                new ServiceItem { StockQuantity = 5 },
                new ServiceItem { StockQuantity = 6 }
            };

            Assert.Equal(new[] { items[0], items[1] }, DBBilling.FilterLowStockItems(items).ToArray());
            Assert.Equal(new[] { items[0] }, DBBilling.FilterLowStockItems(items, 4).ToArray());
        }

    }
}
