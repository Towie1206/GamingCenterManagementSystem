using System;
using System.Linq;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class ServiceComboAndHappyHourRevenueTests
    {
        [Theory]
        [InlineData(22, 59, false)]
        [InlineData(23, 0, true)]
        [InlineData(0, 0, true)]
        [InlineData(10, 59, true)]
        [InlineData(11, 0, false)]
        public void ShiftBoundaries(int hour, int minute, bool expected)
        {
            Assert.Equal(expected, HappyHourRevenueAnalysis.IsHappyHour(new DateTime(2026, 11, 23, hour, minute, 0)));
        }

        [Theory]
        [InlineData(40000, 25000, 20000)]
        [InlineData(50000, 0, 0)]
        [InlineData(40000, 1, 2)]
        public void ComboAllocationConservesPrice(decimal price, decimal food, decimal drink)
        {
            var result = ServiceComboManager.AllocatePrice(price, food, drink);
            Assert.Equal(price, result.Food + result.Drink);
            Assert.InRange(result.Food, 0, price);
            Assert.Equal(result.Food, Math.Round(result.Food, 2));
        }

        [Fact]
        public void OnlyCompletedSessionsAreCountedAtSettlementTime()
        {
            var day = new DateTime(2026, 11, 23);
            var sessions = new[]
            {
                new GamingSession { StartTime = day.AddHours(10), EndTime = day.AddHours(12), Status = "Completed", PlayCost = 10, ServiceCost = 20, TotalCost = 30 },
                new GamingSession { EndTime = day.AddHours(23), Status = "Completed", TotalCost = 40 },
                new GamingSession { EndTime = day.AddHours(23), Status = "Active", TotalCost = 100 },
                new GamingSession { EndTime = day.AddDays(-1), Status = "Completed", TotalCost = 200 }
            };
            var rows = HappyHourRevenueAnalysis.Summarize(sessions, day, day);
            Assert.Equal(2, rows.Sum(r => r.Sessions));
            Assert.Equal(70m, rows.Sum(r => r.Revenue));
            Assert.Equal(30m, rows.Single(r => r.Period == "Giờ tiêu chuẩn").Revenue);
        }

        [Fact]
        public void DepositsKeepCashAndBonusSeparate()
        {
            var day = new DateTime(2026, 11, 23);
            var rows = new[]
            {
                new DepositRevenue(day.AddHours(23), 500000m, 100000m),
                new DepositRevenue(day.AddHours(12), 1000000m, 500000m)
            };
            Assert.Equal((500000m, 100000m), HappyHourRevenueAnalysis.SumDeposits(rows, day, day, true));
            Assert.Equal((1500000m, 600000m), HappyHourRevenueAnalysis.SumDeposits(rows, day, day, null));
        }

        [Fact]
        public void ComboChecksIngredientsAndRejectsInvalidInputsBeforeConnecting()
        {
            Assert.True(ServiceComboManager.MatchesIngredient(0, false, "Sting Dâu"));
            Assert.False(ServiceComboManager.MatchesIngredient(1, true, "Sting Dâu"));
            var manager = new ServiceComboManager(() => throw new Exception("Unexpected connection"));
            Assert.Throws<ArgumentException>(() => manager.Order(0, 1, 2, 2, 1));
            Assert.Throws<ArgumentException>(() => manager.Order(0, 1, 2, 3, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ServiceComboManager.AllocatePrice(-1, 1, 2));
        }
    }
}
