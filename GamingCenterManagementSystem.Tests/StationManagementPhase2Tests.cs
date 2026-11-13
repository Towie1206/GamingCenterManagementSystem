using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    [Collection("GamingCenterSequential")]
    public class StationManagementPhase2Tests
    {
        private static readonly Regex Ipv4Regex = new(
            @"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$",
            RegexOptions.Compiled);

        [Theory]
        [InlineData("192.168.1.10", true)]
        [InlineData("10.0.0.1", true)]
        [InlineData("172.16.254.1", true)]
        [InlineData("255.255.255.255", true)]
        [InlineData("0.0.0.0", true)]
        [InlineData("192.168.1.256", false)]
        [InlineData("999.999.999.999", false)]
        [InlineData("192.168.1", false)]
        [InlineData("abc.def.ghi.jkl", false)]
        [InlineData("", false)]
        public void Ipv4Validation_ValidatesCorrectly(string ip, bool expectedValid)
        {
            bool isValid = Ipv4Regex.IsMatch(ip.Trim());
            Assert.Equal(expectedValid, isValid);
        }

        [Fact]
        public void StationZone_PriceValidation_RejectsNegativeOrZero()
        {
            var dbStation = new DBStation();

            var negativeZone = new StationZone
            {
                ZoneName = "Invalid Zone",
                BasePricePerHour = -1000m
            };

            bool added = dbStation.AddZone(negativeZone, out string error);
            Assert.False(added);
            Assert.Contains("Đơn giá", error);
        }

        [Fact]
        public void StationZone_NameValidation_RejectsEmptyName()
        {
            var dbStation = new DBStation();

            var emptyZone = new StationZone
            {
                ZoneName = "   ",
                BasePricePerHour = 10000m
            };

            bool added = dbStation.AddZone(emptyZone, out string error);
            Assert.False(added);
            Assert.Contains("Tên khu vực", error);
        }

        [Fact]
        public void TransferStation_FailsWhenSourceEqualsTarget()
        {
            var dbStation = new DBStation();
            bool result = dbStation.TransferStation(1, 1, out string error);

            Assert.False(result);
            Assert.Contains("trùng nhau", error);
        }

        [Fact]
        public void DeleteZone_FailsForNonExistentZone()
        {
            var dbStation = new DBStation();
            bool result = dbStation.DeleteZone(999999, out string error);

            Assert.False(result);
        }

        [Fact]
        public void StationZone_DefaultValues_AreCorrect()
        {
            var zone = new StationZone
            {
                ZoneId = 10,
                ZoneName = "Arena VIP",
                BasePricePerHour = 25000m,
                Description = "RTX 4090, 360Hz"
            };

            Assert.Equal(10, zone.ZoneId);
            Assert.Equal("Arena VIP", zone.ZoneName);
            Assert.Equal(25000m, zone.BasePricePerHour);
            Assert.Equal("RTX 4090, 360Hz", zone.Description);
        }
    }
}
