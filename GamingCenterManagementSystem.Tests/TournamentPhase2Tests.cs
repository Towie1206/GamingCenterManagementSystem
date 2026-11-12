using System.IO;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem.Tests;

[Collection("GamingCenterSequential")]
public class TournamentPhase2Tests
{
    private const string ByeOpponent = "BYE (Miễn đấu)";

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void AutoAdvanceRound_PairsWinnersInMatchOrderAndDoesNotRepeatPendingRound(int winnerCount)
    {
        using var fixture = new OfflineTournament();
        for (int index = 0; index < winnerCount; index++)
            fixture.FinishedMatch($"Winner {index + 1}", $"Loser {index + 1}");

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.Equal(fixture.TournamentId, result.TournamentId);
        Assert.Equal(1, result.CompletedRound);
        Assert.Equal(2, result.NextRound);
        Assert.False(result.IsTournamentFinished);
        Assert.Null(result.ChampionTeamName);
        Assert.Null(result.ByeTeamName);
        Assert.Equal(winnerCount / 2, result.CreatedMatches.Count);
        for (int index = 0; index < result.CreatedMatches.Count; index++)
        {
            var match = result.CreatedMatches[index];
            Assert.Equal($"Winner {index * 2 + 1}", match.Team1Name);
            Assert.Equal($"Winner {index * 2 + 2}", match.Team2Name);
            AssertPendingMatch(match, fixture.TournamentId, 2);
        }
        Assert.Equal(result.CreatedMatches.Count,
            result.CreatedMatches.Select(match => match.MatchId).Distinct().Count());
        Assert.Equal("Ongoing", fixture.Tournament.Status);

        int savedCount = fixture.Matches.Count;
        Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));
        Assert.Equal(savedCount, fixture.Matches.Count);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void AutoAdvanceRound_OddWinnersReceiveOneCompletedByeAndAllAdvanceExactlyOnce(int winnerCount)
    {
        using var fixture = new OfflineTournament();
        string[] winners = Enumerable.Range(1, winnerCount).Select(index => $"Winner {index}").ToArray();
        foreach (string winner in winners)
            fixture.FinishedMatch(winner, $"Opponent of {winner}");

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.Equal(winners[^1], result.ByeTeamName);
        Assert.Equal((winnerCount + 1) / 2, result.CreatedMatches.Count);
        var bye = Assert.Single(result.CreatedMatches, match => match.Team2Name == ByeOpponent);
        Assert.Equal(winners[^1], bye.Team1Name);
        Assert.Equal(bye.Team1Name, bye.WinnerTeamName);
        Assert.Equal("Finished", bye.Status);
        Assert.Equal("1-0", bye.Score);
        Assert.Equal(2, bye.RoundNumber);
        Assert.Null(bye.AssignedStationIds);
        Assert.True(DBTournament.TryParseScore(bye.Score, out int firstScore, out int secondScore));
        DBTournament.ValidateMatchScore(bye.Team1Name, bye.Team2Name, firstScore, secondScore, bye.WinnerTeamName!);

        var pending = result.CreatedMatches.Where(match => match.Team2Name != ByeOpponent).ToList();
        Assert.All(pending, match => AssertPendingMatch(match, fixture.TournamentId, 2));
        string[] advancingTeams = pending.SelectMany(match => new[] { match.Team1Name, match.Team2Name })
            .Append(bye.Team1Name).ToArray();
        Assert.Equal(winners, advancingTeams);
    }

    [Fact]
    public void AutoAdvanceRound_PrefersWinnerWhoHasNotReceivedByeIgnoringNameCase()
    {
        using var fixture = new OfflineTournament();
        fixture.FinishedMatch("CHARLIE", ByeOpponent, round: 1);
        fixture.FinishedMatch("alpha", ByeOpponent, round: 1);
        fixture.FinishedMatch("Alpha", "Loser A", round: 3);
        fixture.FinishedMatch("Bravo", "Loser B", round: 3);
        fixture.FinishedMatch("Charlie", "Loser C", round: 3);

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.Equal(3, result.CompletedRound);
        Assert.Equal(4, result.NextRound);
        Assert.Equal("Bravo", result.ByeTeamName);
        var pending = Assert.Single(result.CreatedMatches, match => match.Status == "Pending");
        Assert.Equal("Alpha", pending.Team1Name);
        Assert.Equal("Charlie", pending.Team2Name);
    }

    [Fact]
    public void AutoAdvanceRound_WhenAllWinnersHadByesUsesLastWinner()
    {
        using var fixture = new OfflineTournament();
        foreach (string team in new[] { "Alpha", "Bravo", "Charlie" })
        {
            fixture.FinishedMatch(team, ByeOpponent, round: 1);
            fixture.FinishedMatch(team, $"Loser {team}", round: 2);
        }

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.Equal("Charlie", result.ByeTeamName);
        Assert.Equal(3, result.NextRound);
    }

    [Fact]
    public void AutoAdvanceRound_UsesHighestRoundAndIgnoresUnfinishedEarlierRound()
    {
        using var fixture = new OfflineTournament();
        fixture.PendingMatch("Historical A", "Historical B", round: 1);
        fixture.FinishedMatch("Alpha", "Bravo", round: 4);
        fixture.FinishedMatch("Charlie", "Delta", round: 4);

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.Equal(4, result.CompletedRound);
        Assert.Equal(5, result.NextRound);
        var match = Assert.Single(result.CreatedMatches);
        Assert.Equal("Alpha", match.Team1Name);
        Assert.Equal("Charlie", match.Team2Name);
    }

    [Fact]
    public void AutoAdvanceRound_FinalMarksChampionWithoutCreatingAnotherRound()
    {
        using var fixture = new OfflineTournament();
        fixture.FinishedMatch("Alpha", "Bravo", round: 3);
        int originalCount = fixture.Matches.Count;

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.True(result.IsTournamentFinished);
        Assert.Equal("Alpha", result.ChampionTeamName);
        Assert.Equal(3, result.CompletedRound);
        Assert.Equal(3, result.NextRound);
        Assert.Empty(result.CreatedMatches);
        Assert.Null(result.ByeTeamName);
        Assert.Equal("Finished", fixture.Tournament.Status);
        Assert.Equal(originalCount, fixture.Matches.Count);
        Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));
        Assert.Equal(originalCount, fixture.Matches.Count);
    }

    [Fact]
    public void AutoAdvanceRound_ThreeWinnersCompleteByeSemifinalAndFinalLifecycle()
    {
        using var fixture = new OfflineTournament();
        fixture.FinishedMatch("Alpha", "Bravo");
        fixture.FinishedMatch("Charlie", "Delta");
        fixture.FinishedMatch("Echo", "Foxtrot");

        var semifinal = fixture.Database.AutoAdvanceRound(fixture.TournamentId);
        var actualSemifinal = Assert.Single(semifinal.CreatedMatches, match => match.Status == "Pending");
        fixture.Database.UpdateMatchScore(actualSemifinal.MatchId, 2, 0, actualSemifinal.Team1Name);

        var final = fixture.Database.AutoAdvanceRound(fixture.TournamentId);
        Assert.Null(final.ByeTeamName);
        var finalMatch = Assert.Single(final.CreatedMatches);
        Assert.Equal("Alpha", finalMatch.Team1Name);
        Assert.Equal("Echo", finalMatch.Team2Name);
        fixture.Database.UpdateMatchScore(finalMatch.MatchId, 1, 2, "Echo");

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);
        Assert.True(result.IsTournamentFinished);
        Assert.Equal("Echo", result.ChampionTeamName);
        Assert.Equal("Finished", fixture.Tournament.Status);
        Assert.Equal(6, fixture.Matches.Count);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Playing")]
    public void AutoAdvanceRound_RejectsUnfinishedRoundWithoutChangingState(string status)
    {
        using var fixture = new OfflineTournament();
        fixture.FinishedMatch("Alpha", "Bravo");
        fixture.PendingMatch("Charlie", "Delta").Status = status;

        var error = Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));

        Assert.Contains("Vòng 1", error.Message);
        Assert.Contains("1 trận", error.Message);
        Assert.Equal(2, fixture.Matches.Count);
        Assert.Equal("Upcoming", fixture.Tournament.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AutoAdvanceRound_RejectsFinishedMatchWithMissingWinner(string? missingWinner)
    {
        using var fixture = new OfflineTournament();
        var match = fixture.FinishedMatch("Alpha", "Bravo");
        // Simulate an incomplete legacy record; new results normally validate the winner.
        match.WinnerTeamName = missingWinner;

        var error = Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));

        Assert.Contains($"#{match.MatchId}", error.Message);
        Assert.Single(fixture.Matches);
        Assert.Equal("Upcoming", fixture.Tournament.Status);
    }

    [Fact]
    public void AutoAdvanceRound_RejectsDuplicateWinnersAfterTrimmingIgnoringCase()
    {
        using var fixture = new OfflineTournament();
        fixture.FinishedMatch("Alpha", "Bravo");
        var duplicate = fixture.FinishedMatch("Charlie", "Delta");
        duplicate.WinnerTeamName = " alpha ";

        Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));

        Assert.Equal(2, fixture.Matches.Count);
        Assert.Equal("Upcoming", fixture.Tournament.Status);
    }

    [Fact]
    public void AutoAdvanceRound_RejectsTournamentWithNoMatches()
    {
        using var fixture = new OfflineTournament();

        var error = Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));

        Assert.Contains("Vòng 1", error.Message);
        Assert.Empty(fixture.Matches);
        Assert.Equal("Upcoming", fixture.Tournament.Status);
    }

    [Theory]
    [InlineData("Finished", "SingleElimination")]
    [InlineData("Ongoing", "RoundRobin")]
    public void AutoAdvanceRound_RejectsFinishedTournamentOrRoundRobin(string status, string format)
    {
        using var fixture = new OfflineTournament();
        fixture.FinishedMatch("Alpha", "Bravo");
        fixture.Tournament.Status = status;
        fixture.Tournament.FormatType = format;

        Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(fixture.TournamentId));

        Assert.Single(fixture.Matches);
        Assert.Equal(status, fixture.Tournament.Status);
    }

    [Fact]
    public void AutoAdvanceRound_DoubleEliminationSupportsAdvancingWinners()
    {
        using var fixture = new OfflineTournament();
        fixture.Tournament.FormatType = "DoubleElimination";
        fixture.FinishedMatch("Alpha", "Bravo");
        fixture.FinishedMatch("Charlie", "Delta");

        var result = fixture.Database.AutoAdvanceRound(fixture.TournamentId);

        Assert.Single(result.CreatedMatches);
        Assert.Equal(2, result.NextRound);
    }

    [Fact]
    public void AutoAdvanceRound_RejectsMissingTournament()
    {
        using var fixture = new OfflineTournament();

        Assert.Throws<InvalidOperationException>(() => fixture.Database.AutoAdvanceRound(int.MaxValue));

        Assert.Empty(fixture.Matches);
        Assert.Equal("Upcoming", fixture.Tournament.Status);
    }

    [Fact]
    public void AutoAdvanceRound_DoesNotTreatInvalidConnectionConfigurationAsOffline()
    {
        var expected = new InvalidOperationException("Invalid connection configuration.");
        var database = new DBTournament(() => throw expected);

        var actual = Assert.Throws<InvalidOperationException>(() => database.AutoAdvanceRound(1));

        Assert.Same(expected, actual);
    }

    [Fact]
    public void FormTournament_SwitchingToTournamentWithoutMatchesClearsPreviousMatchAndEditor()
    {
        using var populated = new OfflineTournament();
        using var empty = new OfflineTournament();
        var previousMatch = populated.FinishedMatch("Alpha", "Bravo", round: 3);

        WithOfflineForm(form =>
        {
            SetField(form, "_dbTournament", populated.Database);
            Invoke(form, "LoadTournaments", populated.TournamentId);
            Assert.Equal(previousMatch.MatchId, Field<TournamentMatch>(form, "_selectedMatch")!.MatchId);

            Invoke(form, "LoadTournaments", empty.TournamentId);

            Assert.Null(Field<TournamentMatch>(form, "_selectedMatch"));
            Assert.Empty(Field<List<TournamentMatch>>(form, "_matchList")!);
            Assert.Equal(0, Field<DataGridView>(form, "dgvTranDau")!.Rows.Count);
            Assert.Equal("1", Field<TextBox>(form, "txtRoundNumber")!.Text);
            Assert.Empty(Field<TextBox>(form, "txtTeam1")!.Text);
            Assert.Empty(Field<TextBox>(form, "txtTeam2")!.Text);
            Assert.Equal("0 - 0", Field<TextBox>(form, "txtScore")!.Text);
            var winner = Field<ComboBox>(form, "cboWinnerTeam")!;
            Assert.Single(winner.Items);
            Assert.Equal("-- Chưa có --", winner.SelectedItem);
            Assert.Equal("Pending", Field<ComboBox>(form, "cboMatchStatus")!.SelectedItem);
        });
    }

    [Fact]
    public void FormTournament_ReloadPreservesSelectedTournamentAndItsMatches()
    {
        using var first = new OfflineTournament();
        using var selected = new OfflineTournament();
        first.PendingMatch("First A", "First B");
        var selectedMatch = selected.PendingMatch("Selected A", "Selected B");

        WithOfflineForm(form =>
        {
            SetField(form, "_dbTournament", selected.Database);
            Invoke(form, "LoadTournaments", selected.TournamentId);
            Invoke(form, "LoadTournaments", selected.TournamentId);

            Assert.Equal(selected.TournamentId, Field<Tournament>(form, "_selectedTournament")!.TournamentId);
            Assert.Equal(selectedMatch.MatchId, Field<TournamentMatch>(form, "_selectedMatch")!.MatchId);
            var grid = Field<DataGridView>(form, "dgvGiaiDau")!;
            Assert.Equal(selected.TournamentId, Convert.ToInt32(grid.CurrentRow!.Cells["MaGiai"].Value));
        });
    }

    [Fact]
    public void FormTournament_StationCounterIncludesCurrentClickAndCountsOnlyReadyStations()
    {
        WithOfflineForm(form =>
        {
            var stations = Field<CheckedListBox>(form, "clbDanMay")!;
            stations.Items.Clear();
            for (int index = 0; index < 5; index++)
                stations.Items.Add(StationItem(index + 1, $"Ready {index + 1}", "Available"));
            stations.Items.Add(StationItem(6, "Occupied", "Available", occupied: true));
            stations.Items.Add(StationItem(7, "Maintenance", "Maintenance"));
            var counter = Field<Label>(form, "lblSoMayDaChon")!;

            for (int index = 0; index < 5; index++)
            {
                stations.SetItemChecked(index, true);
                Assert.Equal($"Đã chọn: {index + 1}/5 máy (Sẵn sàng khóa: {index + 1}/5)", counter.Text);
            }
            Assert.Equal(Color.Green, counter.ForeColor);

            stations.SetItemChecked(4, false);
            Assert.Equal("Đã chọn: 4/5 máy (Sẵn sàng khóa: 4/5)", counter.Text);
            stations.SetItemChecked(5, true);
            Assert.Equal("Đã chọn: 5/5 máy (Sẵn sàng khóa: 4/5)", counter.Text);
            Assert.NotEqual(Color.Green, counter.ForeColor);
            stations.SetItemChecked(6, true);
            Assert.Equal("Đã chọn: 6/5 máy (Sẵn sàng khóa: 4/5)", counter.Text);
        });
    }

    [Theory]
    [InlineData("6, 7, 8, 9, 10")]
    [InlineData("PC-06, pc-07, PC-08, PC-09, PC-10")]
    public void FormTournament_UnlockAssignmentsUseFreshStatusesAndSupportIdsAndNames(string assignments)
    {
        WithOfflineForm(form =>
        {
            var database = new DBStation();
            var stations = database.GetAll().Where(station => station.StationId >= 6).ToList();
            var originalStatuses = stations.ToDictionary(station => station.StationId, station => station.Status);
            try
            {
                foreach (var station in stations) station.Status = "TournamentLocked";
                Invoke(form, "LoadStationsChecklist");
                // Change stations after the checklist loaded to reproduce stale UI state.
                stations.Single(station => station.StationId == 7).Status = "InUse";
                stations.Single(station => station.StationId == 8).Status = "Maintenance";
                stations.Single(station => station.StationId == 10).Status = "Available";
                SetField(form, "_selectedTournament", new Tournament { TournamentId = 42 });
                SetField(form, "_selectedMatch", new TournamentMatch
                {
                    TournamentId = 42,
                    AssignedStationIds = assignments
                });

                var lockedIds = (int[])Invoke(form, "GetLockedStationIds")!;

                Assert.Equal(new[] { 6, 9 }, lockedIds);
                Assert.Equal("InUse", stations.Single(station => station.StationId == 7).Status);
                Assert.Equal("Maintenance", stations.Single(station => station.StationId == 8).Status);
            }
            finally
            {
                foreach (var station in stations) station.Status = originalStatuses[station.StationId];
            }
        });
    }

    [Fact]
    public void FormTournament_UnlockCheckedStationsTakePrecedenceOverMatchAssignments()
    {
        WithOfflineForm(form =>
        {
            var database = new DBStation();
            var stations = database.GetAll().Where(station => station.StationId >= 6).ToList();
            var originalStatuses = stations.ToDictionary(station => station.StationId, station => station.Status);
            try
            {
                foreach (var station in stations) station.Status = "TournamentLocked";
                stations.Single(station => station.StationId == 8).Status = "InUse";
                Invoke(form, "LoadStationsChecklist");
                SetField(form, "_selectedTournament", new Tournament { TournamentId = 42 });
                SetField(form, "_selectedMatch", new TournamentMatch
                {
                    TournamentId = 42,
                    AssignedStationIds = "6,7,8,9,10"
                });
                var checklist = Field<CheckedListBox>(form, "clbDanMay")!;
                foreach (int id in new[] { 6, 8 })
                {
                    int itemIndex = Enumerable.Range(0, checklist.Items.Count).Single(index =>
                        (int)checklist.Items[index].GetType().GetProperty("StationId")!.GetValue(checklist.Items[index])! == id);
                    checklist.SetItemChecked(itemIndex, true);
                }

                Assert.Equal(new[] { 6 }, (int[])Invoke(form, "GetLockedStationIds")!);

                int readyIndex = Enumerable.Range(0, checklist.Items.Count).Single(index =>
                    (int)checklist.Items[index].GetType().GetProperty("StationId")!.GetValue(checklist.Items[index])! == 6);
                checklist.SetItemChecked(readyIndex, false);
                Assert.Empty((int[])Invoke(form, "GetLockedStationIds")!);
            }
            finally
            {
                foreach (var station in stations) station.Status = originalStatuses[station.StationId];
            }
        });
    }

    [Fact]
    public void FormTournament_AdvanceButtonAndStationControlsFitAtDefaultAndMinimumSizes()
    {
        WithOfflineForm(form =>
        {
            void AssertControlsFit()
            {
                var panel = Field<Panel>(form, "pnlTranDauControl")!;
                var advance = Field<Button>(form, "btnTaoVongSau")!;
                Assert.True(panel.ClientRectangle.Contains(advance.Bounds));
                var machines = Field<GroupBox>(form, "grpDieuPhoiMay")!;
                var checklist = Field<CheckedListBox>(form, "clbDanMay")!;
                var counter = Field<Label>(form, "lblSoMayDaChon")!;
                var lockButton = Field<Button>(form, "btnKhoaMayGiaiDau")!;
                var unlockButton = Field<Button>(form, "btnMoKhoaMay")!;
                Assert.True(machines.ClientRectangle.Contains(checklist.Bounds));
                Assert.True(machines.ClientRectangle.Contains(counter.Bounds));
                Assert.True(machines.ClientRectangle.Contains(lockButton.Bounds));
                Assert.True(machines.ClientRectangle.Contains(unlockButton.Bounds));
                Assert.True(checklist.Bottom <= counter.Top);
                Assert.True(counter.Bottom <= lockButton.Top);
                Assert.True(lockButton.Bottom <= unlockButton.Top);
            }

            AssertControlsFit();
            string? previewPath = Environment.GetEnvironmentVariable("GCMS_TOURNAMENT_PREVIEW_PATH");
            if (!string.IsNullOrWhiteSpace(previewPath)) SavePreview(form, previewPath);
            form.Size = form.MinimumSize;
            Application.DoEvents();
            AssertControlsFit();
            if (!string.IsNullOrWhiteSpace(previewPath))
                SavePreview(form, Path.Combine(Path.GetDirectoryName(previewPath)!,
                    $"{Path.GetFileNameWithoutExtension(previewPath)}-minimum.png"));
        });
    }

    private static void SavePreview(FormTournament form, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }

    private static object StationItem(int id, string name, string status, bool occupied = false)
    {
        var type = typeof(FormTournament).GetNestedType("StationCheckItem", BindingFlags.NonPublic)!;
        var item = Activator.CreateInstance(type, nonPublic: true)!;
        type.GetProperty("StationId")!.SetValue(item, id);
        type.GetProperty("StationName")!.SetValue(item, name);
        type.GetProperty("Status")!.SetValue(item, status);
        type.GetProperty("IsOccupied")!.SetValue(item, occupied);
        return item;
    }

    private static T? Field<T>(FormTournament form, string name) where T : class =>
        typeof(FormTournament).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form) as T;

    private static void SetField(FormTournament form, string name, object value) =>
        typeof(FormTournament).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, value);

    private static object? Invoke(FormTournament form, string name, params object?[] arguments) =>
        typeof(FormTournament).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, arguments);

    private static void WithOfflineForm(Action<FormTournament> action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var connectionString = typeof(Database).GetField("connectionString", BindingFlags.Static | BindingFlags.NonPublic)!;
            object? originalConnectionString = connectionString.GetValue(null);
            ThreadExceptionEventHandler threadExceptionHandler = (_, args) => failure ??= args.Exception;
            try
            {
                Application.ThreadException += threadExceptionHandler;
                // Force all reads to fallback immediately; never connect to the user's SQL database.
                connectionString.SetValue(null, string.Empty);
                using var form = new FormTournament
                {
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                form.Show();
                Application.DoEvents();
                action(form);
                form.Close();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Application.ThreadException -= threadExceptionHandler;
                connectionString.SetValue(null, originalConnectionString);
            }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Tournament UI check did not finish within 30 seconds.");
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static void AssertPendingMatch(TournamentMatch match, int tournamentId, int round)
    {
        Assert.True(match.MatchId > 0);
        Assert.Equal(tournamentId, match.TournamentId);
        Assert.Equal(round, match.RoundNumber);
        Assert.Equal("Pending", match.Status);
        Assert.Equal("0 - 0", match.Score);
        Assert.Null(match.WinnerTeamName);
        Assert.Null(match.AssignedStationIds);
    }

    private sealed class OfflineTournament : IDisposable
    {
        public DBTournament Database { get; } = new(() => throw new IOException("Offline test connection."));
        public int TournamentId { get; }
        public Tournament Tournament => Database.GetById(TournamentId)!;
        public List<TournamentMatch> Matches => Database.GetMatchesByTournament(TournamentId);

        public OfflineTournament()
        {
            TournamentId = Database.CreateTournament($"Round advance test {Guid.NewGuid():N}", "Test Game", 1000m, DateTime.Today);
        }

        public TournamentMatch PendingMatch(string firstTeam, string secondTeam, int round = 1)
        {
            var match = new TournamentMatch
            {
                TournamentId = TournamentId,
                RoundNumber = round,
                Team1Name = firstTeam,
                Team2Name = secondTeam,
                Status = "Pending",
                Score = "0 - 0"
            };
            Assert.True(Database.AddMatch(match));
            return match;
        }

        public TournamentMatch FinishedMatch(string winner, string loser, int round = 1)
        {
            var match = PendingMatch(winner, loser, round);
            Database.UpdateMatchScore(match.MatchId, 2, 0, winner);
            return match;
        }

        public void Dispose() => Database.Delete(TournamentId);
    }
}
