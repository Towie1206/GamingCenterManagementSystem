using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
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
            const string malformedIp = "192.168.1.999";
            bool isValid = Ipv4Regex.IsMatch(malformedIp);
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

        [Fact]
        public void FormStation_ToolTips_ConfiguredProperlyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormStation
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    var toolTip = typeof(FormStation)
                        .GetField("_stationToolTip", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.GetValue(form) as ToolTip;
                    Assert.NotNull(toolTip);

                    var btnChuyenMay = typeof(FormStation).GetField("btnChuyenMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnBaoTri = typeof(FormStation).GetField("btnBaoTri", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnMoMay = typeof(FormStation).GetField("btnMoMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnThemZone = typeof(FormStation).GetField("btnThemZone", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnThemMay = typeof(FormStation).GetField("btnThemMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;

                    Assert.NotNull(btnChuyenMay);
                    Assert.NotNull(btnBaoTri);
                    Assert.NotNull(btnMoMay);
                    Assert.NotNull(btnThemZone);
                    Assert.NotNull(btnThemMay);

                    Assert.False(string.IsNullOrEmpty(toolTip!.GetToolTip(btnChuyenMay!)));
                    Assert.False(string.IsNullOrEmpty(toolTip.GetToolTip(btnBaoTri!)));
                    Assert.False(string.IsNullOrEmpty(toolTip.GetToolTip(btnMoMay!)));
                    Assert.False(string.IsNullOrEmpty(toolTip.GetToolTip(btnThemZone!)));
                    Assert.False(string.IsNullOrEmpty(toolTip.GetToolTip(btnThemMay!)));

                    form.Close();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            bool completed = staThread.Join(TimeSpan.FromSeconds(10));

            Assert.True(completed, "STA thread timed out");
            Assert.Null(threadException);
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

        [Fact]
        public void StationNetworkHealth_EvaluateStatus_BoundaryValues()
        {
            // Ranh giới Good vs Warning (29ms vs 30ms)
            Assert.Equal(NetworkHealthStatus.Good, StationNetworkHealth.EvaluateStatus(29, 0.0));
            Assert.Equal(NetworkHealthStatus.Warning, StationNetworkHealth.EvaluateStatus(30, 0.0));

            // Ranh giới Warning vs Critical (80ms vs 81ms)
            Assert.Equal(NetworkHealthStatus.Warning, StationNetworkHealth.EvaluateStatus(80, 0.0));
            Assert.Equal(NetworkHealthStatus.Critical, StationNetworkHealth.EvaluateStatus(81, 0.0));

            // Ranh giới packet loss (0% vs 0.1%, 5.0% vs 5.1%)
            Assert.Equal(NetworkHealthStatus.Warning, StationNetworkHealth.EvaluateStatus(20, 0.1));
            Assert.Equal(NetworkHealthStatus.Warning, StationNetworkHealth.EvaluateStatus(20, 5.0));
            Assert.Equal(NetworkHealthStatus.Critical, StationNetworkHealth.EvaluateStatus(20, 5.1));

            // Offline khi >= 100% hoặc latency < 0
            Assert.Equal(NetworkHealthStatus.Offline, StationNetworkHealth.EvaluateStatus(20, 100.0));
            Assert.Equal(NetworkHealthStatus.Offline, StationNetworkHealth.EvaluateStatus(-1, 0.0));
        }

        [Fact]
        public void StationNetworkHealth_GetStatusBadge_AllStatusesCovered()
        {
            var hGood = new StationNetworkHealth { Status = NetworkHealthStatus.Good };
            var hWarn = new StationNetworkHealth { Status = NetworkHealthStatus.Warning };
            var hCrit = new StationNetworkHealth { Status = NetworkHealthStatus.Critical };
            var hOff = new StationNetworkHealth { Status = NetworkHealthStatus.Offline };

            Assert.Contains("Ổn định", hGood.GetStatusBadge());
            Assert.Contains("Chậm", hWarn.GetStatusBadge());
            Assert.Contains("Lag cao", hCrit.GetStatusBadge());
            Assert.Contains("Mất mạng", hOff.GetStatusBadge());
        }

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

        [Fact]
        public void FormStationNetworkScanDialog_InitializesAndBindsScanResultsOnSTA()
        {
            RunOnSTA(() =>
            {
                var db = new DBStation();
                using var dlg = new FormStationNetworkScanDialog(db);
                dlg.Show();
                Application.DoEvents();

                var dgv = dlg.Controls.OfType<DataGridView>().FirstOrDefault();
                Assert.NotNull(dgv);
                Assert.True(dgv.Rows.Count > 0);
            });
        }

        [Fact]
        public void FormStationNetworkScanDialog_NullDb_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FormStationNetworkScanDialog(null!));
        }
    }
}
