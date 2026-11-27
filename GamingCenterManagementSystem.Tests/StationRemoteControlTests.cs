using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    [Collection("GamingCenterSequential")]
    public class StationRemoteControlTests : IDisposable
    {
        public StationRemoteControlTests()
        {
            StationRemoteControlService.ResetForTesting();
        }

        public void Dispose()
        {
            StationRemoteControlService.ResetForTesting();
        }

        [Fact]
        public void StationRemoteControlService_SendForceLock_And_Unlock_TracksLockStateProperly()
        {
            // Arrange
            int stationId = 5;
            string stationName = "PC-05";
            string reason = "Vi phạm quy định: Hút thuốc lá trong phòng lạnh";
            string operatorName = "Admin";

            bool eventFired = false;
            bool lastLockState = false;
            string? lastReason = null;

            Action<int, bool, string?> handler = (stId, isLocked, r) =>
            {
                if (stId == stationId)
                {
                    eventFired = true;
                    lastLockState = isLocked;
                    lastReason = r;
                }
            };

            StationRemoteControlService.OnStationLockStateChanged += handler;

            try
            {
                // Act - Force Lock
                var lockCmd = StationRemoteControlService.SendForceLock(stationId, stationName, reason, operatorName);

                // Assert Lock
                Assert.NotNull(lockCmd);
                Assert.Equal(RemoteCommandType.ForceLock, lockCmd.CommandType);
                Assert.Equal(stationId, lockCmd.TargetStationId);
                Assert.Equal(reason, lockCmd.Payload);
                Assert.True(StationRemoteControlService.IsStationLocked(stationId, out string? actualReason));
                Assert.Equal(reason, actualReason);
                Assert.True(eventFired);
                Assert.True(lastLockState);
                Assert.Equal(reason, lastReason);

                // Act - Unlock
                eventFired = false;
                var unlockCmd = StationRemoteControlService.SendUnlock(stationId, stationName, operatorName);

                // Assert Unlock
                Assert.NotNull(unlockCmd);
                Assert.Equal(RemoteCommandType.Unlock, unlockCmd.CommandType);
                Assert.False(StationRemoteControlService.IsStationLocked(stationId, out _));
                Assert.True(eventFired);
                Assert.False(lastLockState);
            }
            finally
            {
                StationRemoteControlService.OnStationLockStateChanged -= handler;
            }
        }

        [Fact]
        public void StationRemoteControlService_SendTransferStation_CreatesCommandAndHistory()
        {
            int sourceId = 2;
            string sourceName = "PC-02";
            int targetId = 16;
            string targetName = "PC-16";
            string operatorName = "Cashier01";

            var cmd = StationRemoteControlService.SendTransferStation(sourceId, sourceName, targetId, targetName, operatorName);

            Assert.NotNull(cmd);
            Assert.Equal(RemoteCommandType.TransferStation, cmd.CommandType);
            Assert.Equal(sourceId, cmd.TargetStationId);
            Assert.Equal(targetId.ToString(), cmd.Payload);
            Assert.Equal(targetName, cmd.AdditionalInfo);

            var history = StationRemoteControlService.GetCommandHistory(sourceId);
            Assert.Contains(history, c => c.CommandType == RemoteCommandType.TransferStation && c.TargetStationId == sourceId);
        }

        [Fact]
        public void StationRemoteControlService_SendAlertPopup_And_Broadcast_DispatchesEvents()
        {
            int targetId = 7;
            string targetName = "PC-07";
            string msg = "Yêu cầu máy 07 giữ trật tự!";

            RemoteCommand? receivedCmd = null;
            Action<RemoteCommand> handler = c => receivedCmd = c;
            StationRemoteControlService.OnCommandDispatched += handler;

            try
            {
                // Single station alert
                StationRemoteControlService.SendAlertPopup(targetId, targetName, msg, "CẢNH BÁO", "Admin", false);
                Assert.NotNull(receivedCmd);
                Assert.Equal(RemoteCommandType.AlertPopup, receivedCmd!.CommandType);
                Assert.Equal(targetId, receivedCmd.TargetStationId);
                Assert.Equal(msg, receivedCmd.Payload);

                // Broadcast alert
                receivedCmd = null;
                StationRemoteControlService.SendAlertPopup(0, "Toàn bộ phòng máy", "Bảo trì sau 15 phút", "THÔNG BÁO CHUNG", "Admin", true);
                Assert.NotNull(receivedCmd);
                Assert.Equal(RemoteCommandType.BroadcastPopup, receivedCmd!.CommandType);
                Assert.Equal(0, receivedCmd.TargetStationId);
            }
            finally
            {
                StationRemoteControlService.OnCommandDispatched -= handler;
            }
        }

        [Fact]
        public void StationRemoteControlService_SendKillApp_And_RunningApps_ManagesList()
        {
            int stationId = 10;
            var apps = new List<RunningAppInfo>
            {
                new("Valorant", "Valorant", 2400, true),
                new("Spotify", "Spotify Music", 320, false)
            };

            StationRemoteControlService.ReportRunningApps(stationId, apps);
            var retrieved = StationRemoteControlService.GetRunningApps(stationId);

            Assert.Equal(2, retrieved.Count);
            Assert.Equal("Valorant", retrieved[0].ProcessName);

            var cmd = StationRemoteControlService.SendKillApp(stationId, "PC-10", "Valorant", "Admin");
            Assert.NotNull(cmd);
            Assert.Equal(RemoteCommandType.KillRunningApp, cmd.CommandType);
            Assert.Equal("Valorant", cmd.Payload);
        }

        [Fact]
        public void StationRemoteControlService_ScreenshotSimulation_And_Report()
        {
            int stationId = 12;
            using var simulated = StationRemoteControlService.GetLatestScreenshot(stationId);
            Assert.NotNull(simulated);
            Assert.Equal(640, simulated!.Width);
            Assert.Equal(360, simulated.Height);

            using var customBmp = new Bitmap(200, 100);
            StationRemoteControlService.ReportScreenshot(stationId, customBmp);

            using var retrieved = StationRemoteControlService.GetLatestScreenshot(stationId);
            Assert.NotNull(retrieved);
            Assert.Equal(200, retrieved!.Width);
            Assert.Equal(100, retrieved.Height);
        }

        [Fact]
        public void FormClientDashboard_ReceivesForceLock_DisablesUnlockBox_And_PreventsClientUnlock()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.SetUser(new AppUser
                    {
                        UserId = 2,
                        Username = "gamer01",
                        FullName = "Nguyễn Văn A",
                        Role = "Member",
                        Balance = 50_000_000m,
                        MemberTier = "Standard",
                        IsActive = true
                    });

                    using var dashboard = new FormClientDashboard(new DBStation(), new DBBilling(), new DBUser(), 1)
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    dashboard.SetPromptHandler((msg, cap, btn, icon) => DialogResult.OK);
                    dashboard.Show();
                    Application.DoEvents();

                    Assert.False(dashboard.IsScreenLocked);

                    int stId = dashboard.CurrentStation?.StationId ?? 1;
                    string stName = dashboard.CurrentStation?.StationName ?? "PC-01";

                    // Server dispatches force lock to station
                    StationRemoteControlService.SendForceLock(stId, stName, "Hút thuốc lá", "Admin");
                    Application.DoEvents();

                    // Client must now be locked
                    Assert.True(dashboard.IsScreenLocked);

                    // Client attempts to unlock with correct password -> must fail because of server lock!
                    bool unlockResult = dashboard.TryUnlockStation("123456", out string unlockMsg);
                    Assert.False(unlockResult);
                    Assert.Contains("khóa cưỡng chế", unlockMsg, StringComparison.OrdinalIgnoreCase);

                    // Server dispatches unlock
                    StationRemoteControlService.SendUnlock(stId, stName, "Admin");
                    Application.DoEvents();

                    // Client must now be unlocked
                    Assert.False(dashboard.IsScreenLocked);

                    dashboard.Close();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            bool completed = staThread.Join(TimeSpan.FromSeconds(10));

            Assert.True(completed, "STA Thread timed out");
            Assert.Null(threadException);
        }

        [Fact]
        public void FormClientDashboard_ReceivesKillRunningApp_StopsGame()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.SetUser(new AppUser
                    {
                        UserId = 2,
                        Username = "gamer01",
                        FullName = "Nguyễn Văn A",
                        Role = "Member",
                        Balance = 50_000_000m,
                        MemberTier = "Standard",
                        IsActive = true
                    });

                    using var dashboard = new FormClientDashboard(new DBStation(), new DBBilling(), new DBUser(), 1)
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    dashboard.SetPromptHandler((msg, cap, btn, icon) => DialogResult.OK);
                    dashboard.Show();
                    Application.DoEvents();

                    // Launch game
                    if (dashboard.AvailableGames.Count > 0)
                    {
                        dashboard.LaunchGame(dashboard.AvailableGames[0].GameId);
                        Assert.True(dashboard.IsGameRunning);

                        int stId = dashboard.CurrentStation?.StationId ?? 1;
                        string stName = dashboard.CurrentStation?.StationName ?? "PC-01";

                        // Server dispatches KillRunningApp
                        StationRemoteControlService.SendKillApp(stId, stName, dashboard.AvailableGames[0].Name, "Admin");
                        Application.DoEvents();

                        // Game must be closed
                        Assert.False(dashboard.IsGameRunning);
                    }

                    dashboard.Close();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            bool completed = staThread.Join(TimeSpan.FromSeconds(10));

            Assert.True(completed, "STA Thread timed out");
            Assert.Null(threadException);
        }

        [Fact]
        public void FormStationRemoteControl_InitializesProperlyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    var station = new ComputerStation { StationId = 3, StationName = "PC-03", Status = "InUse", ZoneId = 1 };
                    var allStations = new List<ComputerStation>
                    {
                        station,
                        new() { StationId = 4, StationName = "PC-04", Status = "Available", ZoneId = 1 }
                    };
                    var zones = new List<StationZone>
                    {
                        new() { ZoneId = 1, ZoneName = "Dàn Standard", BasePricePerHour = 10000m }
                    };

                    using var form = new FormStationRemoteControl(station, allStations, zones, new DBBilling(), new DBStation())
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    Assert.NotNull(form);
                    Assert.Contains("PC-03", form.Text);

                    // Test force lock from dialog
                    StationRemoteControlService.SendForceLock(3, "PC-03", "Test lock", "Admin");
                    Application.DoEvents();
                    Assert.True(StationRemoteControlService.IsStationLocked(3, out _));

                    // Test unlock from dialog
                    StationRemoteControlService.SendUnlock(3, "PC-03", "Admin");
                    Application.DoEvents();
                    Assert.False(StationRemoteControlService.IsStationLocked(3, out _));

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

            Assert.True(completed, "STA Thread timed out");
            Assert.Null(threadException);
        }

        [Fact]
        public void Form1_DieuKhienMayButton_Exists_And_RoleAuthorization_Works()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new Form1
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    Assert.NotNull(form.btnDieuKhienMay);
                    Assert.True(form.btnDieuKhienMay.Visible);

                    // Member role test
                    CurrentUserSession.SetUser(new AppUser
                    {
                        UserId = 99,
                        Username = "member99",
                        FullName = "Test Member",
                        Role = "Member",
                        Balance = 50000m
                    });

                    form.ApplyRolePermissions();
                    Assert.False(form.btnDieuKhienMay.Enabled);

                    // Admin role test
                    CurrentUserSession.SetUser(new AppUser
                    {
                        UserId = 1,
                        Username = "admin",
                        FullName = "Administrator",
                        Role = "Admin"
                    });

                    form.ApplyRolePermissions();
                    Assert.True(form.btnDieuKhienMay.Enabled);

                    CurrentUserSession.Clear();
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

            Assert.True(completed, "STA Thread timed out");
            Assert.Null(threadException);
        }
    }
}
