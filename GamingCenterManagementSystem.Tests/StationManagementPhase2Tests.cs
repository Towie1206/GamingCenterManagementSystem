using System;
using System.Text.RegularExpressions;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class StationManagementPhase2Tests
    {
        [Fact]
        public void StationZone_UpdatePrice_RejectsNegativeOrZero()
        {
            var dbStation = new DBStation();
            var invalidZone = new StationZone { ZoneId = 1, ZoneName = "VIP", BasePricePerHour = 0m };
            bool updated = dbStation.UpdateZone(invalidZone, out string error);
            Assert.False(updated);
            Assert.Contains("Đơn giá", error);
        }

        [Fact]
        public void ComputerStation_InvalidIpLength_Rejected()
        {
            var ipv4Regex = new Regex(@"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$");
            const string malformedIp = "192.168.1.999";
            bool isValid = ipv4Regex.IsMatch(malformedIp);
            Assert.False(isValid);
        }

        [Fact]
        public void StationZone_Description_AllowsNullOrEmpty()
        {
            var zone = new StationZone { ZoneId = 5, ZoneName = "Khu Thường 2", BasePricePerHour = 10000m, Description = null };
            Assert.Null(zone.Description);
            Assert.Equal("Khu Thường 2", zone.ZoneName);
        }

        [Fact]
        public void StationNetworkHealth_EvaluateStatus_ClassifiesCorrectly()
        {
            Assert.Equal(NetworkHealthStatus.Good, StationNetworkHealth.EvaluateStatus(15, 0));
            Assert.Equal(NetworkHealthStatus.Warning, StationNetworkHealth.EvaluateStatus(45, 0));
            Assert.Equal(NetworkHealthStatus.Critical, StationNetworkHealth.EvaluateStatus(95, 0));
            Assert.Equal(NetworkHealthStatus.Offline, StationNetworkHealth.EvaluateStatus(-1, 100));
        }

        [Fact]
        public void StationNetworkHealth_GetStatusBadge_ReturnsDescriptiveText()
        {
            var health = new StationNetworkHealth { Status = NetworkHealthStatus.Good };
            Assert.Contains("Ổn định", health.GetStatusBadge());
        }

        [Fact]
        public void DBStation_CheckAllStationsNetworkHealth_ReturnsValidList()
        {
            var db = new DBStation();
            var list = db.CheckAllStationsNetworkHealth();
            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }
    }

}