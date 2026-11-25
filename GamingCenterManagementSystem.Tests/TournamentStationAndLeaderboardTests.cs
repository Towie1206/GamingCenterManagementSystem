using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem.Tests;

public class TournamentStationAndLeaderboardTests
{
    #region TournamentMachineLockEngine & TournamentStationManager Tests

    [Fact]
    public void Engine_DefaultInitialization_Configures30MasterGameStationsAcross3Zones()
    {
        var engine = new TournamentMachineLockEngine();
        var stations = engine.GetAllStations();

        Assert.Equal(30, stations.Count);
        Assert.Equal(10, stations.Count(s => s.ZoneName == "Standard" && s.StandardHourlyRate == 10000m));
        Assert.Equal(10, stations.Count(s => s.ZoneName == "Maximum" && s.StandardHourlyRate == 15000m));
        Assert.Equal(10, stations.Count(s => s.ZoneName == "Master" && s.StandardHourlyRate == 13000m));
        Assert.All(stations, s =>
        {
            Assert.False(s.TournamentMode);
            Assert.Equal("Available", s.Status);
        });
    }

    [Fact]
    public void ParseStationRange_ValidMaximumRange_ReturnsPC11ThroughPC20()
    {
        var range = TournamentMachineLockEngine.ParseStationRange("pc-11", "PC-20");

        Assert.Equal(10, range.Count);
        Assert.Equal("PC-11", range[0]);
        Assert.Equal("PC-20", range[^1]);
    }

    [Fact]
    public void ParseStationRange_ReversedOrInvalidFormat_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TournamentMachineLockEngine.ParseStationRange("PC-20", "PC-11"));
        Assert.Throws<ArgumentException>(() => TournamentMachineLockEngine.ParseStationRange("MAY-01", "PC-10"));
        Assert.Throws<ArgumentException>(() => TournamentMachineLockEngine.ParseStationRange("", "PC-10"));
    }

    [Fact]
    public void ActivateTournamentCluster_MaximumRange_SetsTournamentModeTrueAndLocks10Stations()
    {
        var manager = new TournamentStationManager();
        var notifications = new List<string>();
        manager.Engine.ClusterStatusChanged += msg => notifications.Add(msg);

        var reservation = manager.ActivateTournamentCluster(
            "PC-11",
            "PC-20",
            "Trận Tứ kết 1",
            tournamentId: 101,
            tournamentTitle: "Master Game Valorant Cup",
            adminName: "KienAdmin");

        Assert.True(reservation.IsActive);
        Assert.Equal("PC-11", reservation.StartStationName);
        Assert.Equal("PC-20", reservation.EndStationName);
        Assert.Equal(10, reservation.StationNames.Count);
        Assert.Single(notifications);

        for (int i = 11; i <= 20; i++)
        {
            var st = manager.Engine.GetStation($"PC-{i:D2}");
            Assert.NotNull(st);
            Assert.True(st!.TournamentMode);
            Assert.Equal("TournamentLocked", st.Status);
            Assert.Equal("Trận Tứ kết 1", st.MatchLabel);
            Assert.Equal(101, st.TournamentId);
        }

        // Các dàn còn lại không bị ảnh hưởng
        Assert.False(manager.IsStationInTournamentMode("PC-10"));
        Assert.False(manager.IsStationInTournamentMode("PC-21"));
    }

    [Fact]
    public void ActivateZoneCluster_MasterZone_LocksPC21ThroughPC30()
    {
        var manager = new TournamentStationManager();

        var reservation = manager.ActivateZoneCluster("Master", "Trận Bán kết 2");

        Assert.Equal("PC-21", reservation.StartStationName);
        Assert.Equal("PC-30", reservation.EndStationName);
        Assert.Equal(10, manager.GetLockedStations().Count);
        Assert.All(reservation.StationNames, name => Assert.True(manager.IsStationInTournamentMode(name)));
    }

    [Fact]
    public void ActivateCluster_WhenOneStationInRangeIsInUse_ThrowsAndLeavesRangeUnchanged()
    {
        var manager = new TournamentStationManager();
        manager.Engine.SetStationStatus("PC-15", "InUse");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            manager.ActivateTournamentCluster("PC-11", "PC-20", "Trận Tứ kết 1"));

        Assert.Contains("PC-15", ex.Message);
        Assert.Empty(manager.GetLockedStations());
        Assert.False(manager.IsStationInTournamentMode("PC-11"));
        Assert.Equal("Available", manager.Engine.GetStation("PC-11")!.Status);
    }

    [Fact]
    public void ActivateCluster_WhenOverlappingExistingCluster_ThrowsInvalidOperationException()
    {
        var manager = new TournamentStationManager();
        manager.ActivateTournamentCluster("PC-11", "PC-15", "Trận Tứ kết 1");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            manager.ActivateTournamentCluster("PC-14", "PC-20", "Trận Tứ kết 2"));

        Assert.Contains("PC-14", ex.Message);
        Assert.Equal(5, manager.GetLockedStations().Count);
    }

    [Fact]
    public void StationAccess_TournamentModeBlocksGuestAndRegularMember_AllowsTournamentParticipant()
    {
        var manager = new TournamentStationManager();
        manager.ActivateTournamentCluster("PC-11", "PC-20", "Trận Tứ kết 1");

        bool guestCanOpen = manager.CanGuestOrRegularAccountOpen("PC-12", out string guestReason);
        Assert.False(guestCanOpen);
        Assert.Contains("Đang thi đấu giải - Trận Tứ kết 1", guestReason);

        var memberAccess = manager.CheckAccountAccess("PC-12", accountRole: "Member", isTournamentParticipant: false);
        Assert.False(memberAccess.IsAllowed);
        Assert.True(memberAccess.IsBlockedByTournamentMode);

        Assert.Throws<InvalidOperationException>(() =>
            manager.EnsureStationAccessForAccount("PC-12", accountRole: "Guest", isTournamentParticipant: false));

        var participantAccess = manager.CheckAccountAccess("PC-12", accountRole: "Member", isTournamentParticipant: true);
        Assert.True(participantAccess.IsAllowed);
        Assert.False(participantAccess.IsBlockedByTournamentMode);
    }

    [Fact]
    public void GetStationVisualInfo_InTournamentMode_ReturnsSpecialPurpleGoldStyleAndMatchBadge()
    {
        var manager = new TournamentStationManager();
        manager.ActivateTournamentCluster("PC-11", "PC-20", "Trận Tứ kết 1");

        var visual = manager.GetStationVisualInfo("PC-11");

        Assert.True(visual.TournamentMode);
        Assert.Equal("🏆", visual.Icon);
        Assert.Equal("Đang thi đấu giải - Trận Tứ kết 1", visual.BadgeText);
        Assert.Contains("Đang thi đấu giải - Trận Tứ kết 1", visual.ButtonDisplayText);
        Assert.Equal(TournamentMachineLockEngine.TournamentBackColor, visual.BackColor);
        Assert.Equal(TournamentMachineLockEngine.TournamentBorderColor, visual.BorderColor);
        Assert.Equal(TournamentMachineLockEngine.TournamentAccentGold, visual.AccentColor);
        Assert.False(visual.AllowRegularGuestOpen);

        RunOnSTA(() =>
        {
            using var btn = new Button();
            manager.ApplyVisualStyleToButton(btn, "PC-11");
            Assert.Equal(TournamentMachineLockEngine.TournamentBackColor, btn.BackColor);
            Assert.Equal(TournamentMachineLockEngine.TournamentBorderColor, btn.FlatAppearance.BorderColor);
            Assert.Equal(3, btn.FlatAppearance.BorderSize);
            Assert.Contains("Trận Tứ kết 1", btn.Text);
        });
    }

    [Fact]
    public void DeactivateTournamentCluster_AndReleaseRange_RestoresAvailableAndAllowsRegularOpen()
    {
        var manager = new TournamentStationManager();
        var cluster = manager.ActivateTournamentCluster("PC-11", "PC-20", "Trận Tứ kết 1");

        bool released = manager.DeactivateTournamentCluster(cluster.ClusterId);
        Assert.True(released);
        Assert.Empty(manager.GetActiveReservations());
        Assert.Empty(manager.GetLockedStations());
        Assert.True(manager.CanGuestOrRegularAccountOpen("PC-11", out _));

        // Thử giải phóng theo dải
        manager.ActivateTournamentCluster("PC-21", "PC-30", "Trận Chung kết");
        int count = manager.ReleaseStationRange("PC-21", "PC-25");
        Assert.Equal(5, count);
        Assert.False(manager.IsStationInTournamentMode("PC-21"));
        Assert.True(manager.IsStationInTournamentMode("PC-26"));

        int remainingReleased = manager.Engine.ReleaseAllClusters();
        Assert.Equal(5, remainingReleased);
        Assert.Empty(manager.GetLockedStations());
    }

    [Fact]
    public void SyncToComputerStations_PropagatesTournamentLockedStatus()
    {
        var manager = new TournamentStationManager();
        manager.ActivateTournamentCluster("PC-11", "PC-12", "Trận Khai Mạc");

        var stations = new List<ComputerStation>
        {
            new() { StationId = 10, StationName = "PC-10", Status = "Available" },
            new() { StationId = 11, StationName = "PC-11", Status = "Available" },
            new() { StationId = 12, StationName = "PC-12", Status = "Available" }
        };

        manager.Engine.SyncToComputerStations(stations);

        Assert.Equal("Available", stations[0].Status);
        Assert.Equal("TournamentLocked", stations[1].Status);
        Assert.Equal("TournamentLocked", stations[2].Status);
    }

    #endregion

    #region TournamentLeaderboardService & FormTournamentLeaderboard Tests

    [Fact]
    public void LeaderboardService_DefaultSeed_UsesMasterGame165HoangCongChatAndNoLanLabel()
    {
        var service = new TournamentLeaderboardService(loadSampleData: true);
        var entries = service.GetAllEntries();

        Assert.True(entries.Count >= 3);
        Assert.All(entries, e =>
        {
            Assert.Contains("165 Hoàng Công Chất", e.Venue);
            Assert.DoesNotContain("LAN", e.TournamentTitle, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void LeaderboardService_AddAndRank_OrdersByMvpScoreThenPrizeAmount()
    {
        var service = new TournamentLeaderboardService(loadSampleData: false);

        service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "Player B",
            TeamName = "Team Beta",
            TournamentTitle = "Master Game Cup",
            GameTitle = "Valorant",
            MvpScore = 900,
            PrizeAmount = 2000000m
        });
        service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "Player A",
            TeamName = "Team Alpha",
            TournamentTitle = "Master Game Cup",
            GameTitle = "Valorant",
            MvpScore = 950,
            PrizeAmount = 5000000m
        });
        service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "Player C",
            TeamName = "Team Gamma",
            TournamentTitle = "Master Game Cup",
            GameTitle = "Valorant",
            MvpScore = 900,
            PrizeAmount = 3000000m
        });
        service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "Player D",
            TeamName = "Team Delta",
            TournamentTitle = "Master Game Cup",
            GameTitle = "Valorant",
            MvpScore = 800,
            PrizeAmount = 1000000m
        });

        var podium = service.GetTopPodium(3);

        Assert.Equal(3, podium.Count);
        Assert.Equal(1, podium[0].Rank);
        Assert.Equal("Player A", podium[0].Entry.PlayerName);
        Assert.Contains("Top 1", podium[0].MedalLabel);

        Assert.Equal(2, podium[1].Rank);
        Assert.Equal("Player C", podium[1].Entry.PlayerName); // Bằng điểm 900 nhưng thưởng cao hơn
        Assert.Contains("Top 2", podium[1].MedalLabel);

        Assert.Equal(3, podium[2].Rank);
        Assert.Equal("Player B", podium[2].Entry.PlayerName);
        Assert.Contains("Top 3", podium[2].MedalLabel);
    }

    [Fact]
    public void LeaderboardService_InvalidEntries_ThrowArgumentException()
    {
        var service = new TournamentLeaderboardService(loadSampleData: false);

        Assert.Throws<ArgumentException>(() => service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "",
            TeamName = "Team A",
            TournamentTitle = "Cup",
            GameTitle = "Valorant",
            MvpScore = 500,
            PrizeAmount = 1000000m
        }));

        Assert.Throws<ArgumentException>(() => service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "Player A",
            TeamName = "Team A",
            TournamentTitle = "Cup",
            GameTitle = "Valorant",
            MvpScore = 0,
            PrizeAmount = 1000000m
        }));

        Assert.Throws<ArgumentException>(() => service.AddEntry(new TournamentMvpEntry
        {
            PlayerName = "Player A",
            TeamName = "Team A",
            TournamentTitle = "Cup",
            GameTitle = "Valorant",
            MvpScore = 500,
            PrizeAmount = -50000m
        }));
    }

    [Fact]
    public void LeaderboardService_ExportTextAndCsv_ContainsTop3WithPrizeAndHonorPoints()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var service = new TournamentLeaderboardService(loadSampleData: true);

            string txt = service.BuildHallOfFameExportText(topCount: 3, generatedAt: new DateTime(2026, 11, 25, 10, 0, 0));
            Assert.Contains("BẢNG VINH DANH TOP MVP", txt);
            Assert.Contains("165 Hoàng Công Chất", txt);
            Assert.Contains("Top 1", txt);
            Assert.Contains("Top 2", txt);
            Assert.Contains("Top 3", txt);
            Assert.Contains("5,000,000 VNĐ", txt);
            Assert.Contains("985 điểm", txt);

            string csv = service.BuildCsvExportText(topCount: 3);
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(4, lines.Length); // 1 header + Top 3
            Assert.StartsWith("Hang,TuyenThuMVP,DoiTuyen", lines[0]);
            Assert.Contains("Top 1", lines[1]);
            Assert.Contains("5000000", lines[1]);

            string tempDir = Path.Combine(Path.GetTempPath(), $"gcms_mvp_test_{Guid.NewGuid():N}");
            string txtPath = Path.Combine(tempDir, "top3.txt");
            string csvPath = Path.Combine(tempDir, "top3.csv");
            try
            {
                service.ExportLeaderboardToFile(txtPath, topCount: 3);
                service.ExportLeaderboardToFile(csvPath, topCount: 3);

                Assert.True(File.Exists(txtPath));
                Assert.True(File.Exists(csvPath));
                Assert.Contains("Nguyễn Minh Quân", File.ReadAllText(txtPath));
                Assert.Contains("Nguyễn Minh Quân", File.ReadAllText(csvPath));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void FormTournamentLeaderboard_NullService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FormTournamentLeaderboard(null!));
    }

    [Fact]
    public void FormTournamentLeaderboard_RendersAddsMvpAndExportsTop3_OnSTA()
    {
        RunOnSTA(() =>
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var service = new TournamentLeaderboardService(loadSampleData: true);
            using var form = new FormTournamentLeaderboard(service)
            {
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(0, 0),
                Opacity = 0
            };

            string? lastPrompt = null;
            form.SetPromptHandler((msg, _, _, _) =>
            {
                lastPrompt = msg;
                return DialogResult.OK;
            });

            form.Show();
            Application.DoEvents();

            var dgv = FindControlByName<DataGridView>(form, "dgvLeaderboard");
            Assert.NotNull(dgv);
            Assert.Equal(5, dgv!.Rows.Count);

            // Ghi nhận một tuyển thủ MVP mới đạt điểm cao nhất (Top 1)
            FindControlByName<TextBox>(form, "txtPlayerName")!.Text = "Đỗ Trọng Kiên (KienBlack)";
            FindControlByName<TextBox>(form, "txtTeamName")!.Text = "Master Black";
            FindControlByName<TextBox>(form, "txtTournamentTitle")!.Text = "Master Game Grand Final 2026";
            FindControlByName<ComboBox>(form, "cboGameTitle")!.Text = "Valorant";
            FindControlByName<NumericUpDown>(form, "numMvpScore")!.Value = 1200;
            FindControlByName<NumericUpDown>(form, "numPrizeAmount")!.Value = 8000000;
            FindControlByName<TextBox>(form, "txtAchievementNote")!.Text = "Vô địch bất bại tại dàn Maximum";

            var btnAdd = FindControlByName<Button>(form, "btnAddMvp")!;
            btnAdd.PerformClick();
            Application.DoEvents();

            Assert.Equal(6, dgv.Rows.Count);
            Assert.Contains("thành công", lastPrompt ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            string exportText = form.BuildCurrentExportText(topCount: 3);
            Assert.Contains("Đỗ Trọng Kiên (KienBlack)", exportText);
            Assert.Contains("8,000,000 VNĐ", exportText);
            Assert.Contains("1,200 điểm", exportText);

            string tempFile = Path.Combine(Path.GetTempPath(), $"gcms_form_mvp_{Guid.NewGuid():N}.txt");
            try
            {
                form.ExportTopLeaderboard(tempFile, topCount: 3);
                Assert.True(File.Exists(tempFile));
                Assert.Contains("Đỗ Trọng Kiên (KienBlack)", File.ReadAllText(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            form.Close();
        });
    }

    #endregion

    private static T? FindControlByName<T>(Control root, string name) where T : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is T typed && string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                return typed;

            var nested = FindControlByName<T>(child, name);
            if (nested != null)
                return nested;
        }
        return null;
    }

    private static void RunOnSTA(Action action)
    {
        Exception? failure = null;
        CultureInfo callerCulture = CultureInfo.CurrentCulture;
        CultureInfo callerUiCulture = CultureInfo.CurrentUICulture;

        var thread = new Thread(() =>
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
            ThreadExceptionEventHandler handler = (_, args) => failure ??= args.Exception;
            try
            {
                CultureInfo.CurrentCulture = callerCulture;
                CultureInfo.CurrentUICulture = callerUiCulture;
                Application.ThreadException += handler;
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                Application.ThreadException -= handler;
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        });

        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "STA UI test did not complete within 30 seconds.");
        if (failure != null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
