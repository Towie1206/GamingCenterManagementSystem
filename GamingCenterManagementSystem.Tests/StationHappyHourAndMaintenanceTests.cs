using System;
using System.Threading;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class StationHappyHourAndMaintenanceTests
    {
        private static void RunOnSTA(Action action)
        {
            Exception? captured = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (captured != null)
            {
                throw new Exception("STA thread test failed", captured);
            }
        }

        [Theory]
        [InlineData(23, 0, true)]
        [InlineData(2, 30, true)]
        [InlineData(11, 0, true)]
        [InlineData(11, 1, false)]
        [InlineData(15, 0, false)]
        [InlineData(22, 59, false)]
        public void HappyHourPricingEngine_IsHappyHour_EvaluatesTimeWindowAccurately(int hour, int minute, bool expectedActive)
        {
            var timestamp = new DateTime(2026, 11, 23, hour, minute, 0);
            Assert.Equal(expectedActive, HappyHourPricingEngine.IsHappyHour(timestamp));
        }

        [Fact]
        public void HappyHourPricingEngine_AppliesCorrectDiscountedRatesDuringHappyHours()
        {
            var nightTime = new DateTime(2026, 11, 23, 23, 30, 0);

            Assert.Equal(6000m, HappyHourPricingEngine.GetHourlyRate(StationTier.Standard, nightTime));
            Assert.Equal(8000m, HappyHourPricingEngine.GetHourlyRate(StationTier.Master, nightTime));
            Assert.Equal(9000m, HappyHourPricingEngine.GetHourlyRate(StationTier.Maximum, nightTime));
        }

        [Fact]
        public void HappyHourPricingEngine_RevertsToStandardRatesAt1101()
        {
            var afterHappyHour = new DateTime(2026, 11, 24, 11, 1, 0);

            Assert.Equal(10000m, HappyHourPricingEngine.GetHourlyRate(StationTier.Standard, afterHappyHour));
            Assert.Equal(13000m, HappyHourPricingEngine.GetHourlyRate(StationTier.Master, afterHappyHour));
            Assert.Equal(15000m, HappyHourPricingEngine.GetHourlyRate(StationTier.Maximum, afterHappyHour));
        }

        [Fact]
        public void HappyHourPricingEngine_EvaluateZoneQuote_ReturnsBadgeAndRemainingTime()
        {
            var engine = new HappyHourPricingEngine();
            var masterZone = new StationZone { ZoneId = 2, ZoneName = "Dàn Master", BasePricePerHour = 13000m };
            var checkTime = new DateTime(2026, 11, 24, 9, 30, 0);

            var quote = engine.EvaluateZoneQuote(masterZone, checkTime);

            Assert.True(quote.IsHappyHourActive);
            Assert.Equal(8000m, quote.AppliedPricePerHour);
            Assert.Equal(TimeSpan.FromMinutes(90), quote.RemainingHappyHour);
            Assert.Contains("Happy Hours (-40%)", quote.BadgeText);
        }

        [Fact]
        public void FormStationMaintenance_SettingMaintenance_LocksStationStatusOnSTA()
        {
            RunOnSTA(() =>
            {
                var db = new DBStation();
                using var form = new FormStationMaintenance(db);

                bool result = form.ApplyGearMaintenanceStatus(
                    stationId: 1,
                    mouse: "Razer DeathAdder V3",
                    keyboard: "DrunkDeer A75 Rapid Trigger",
                    headset: "HyperX Cloud II",
                    conditionStatus: "Đang bảo trì",
                    notes: "Hỏng click trái chuột",
                    out string error);

                Assert.True(result, error);
                Assert.Equal("Đang bảo trì", form.GearLogs[1].ConditionStatus);

                // Trả lại trạng thái Available để tránh ảnh hưởng dữ liệu fallback cho test khác
                form.ApplyGearMaintenanceStatus(1, "Logitech G102", "Dareu EK87", "Somic G941", "Bình thường", "", out _);
            });
        }
    }
}