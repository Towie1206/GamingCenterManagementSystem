using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using GamingCenterManagementSystem;
using Microsoft.Data.SqlClient;

internal static class Program
{
    private static int passed;
    private static int failed;

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--probe-db", StringComparer.Ordinal))
            return ProbeDatabase();

        Run("Tournament accepts today at midnight and a positive prize", () =>
            DBTournament.ValidateTournament("LAN Party", 0.01m, DateTime.Today));
        Run("Tournament accepts future dates and maximum SQL decimal prize", () =>
            DBTournament.ValidateTournament("LAN Party", 9999999999999999.99m, DateTime.Today.AddDays(1)));
        Run("Tournament rejects empty and whitespace names", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("", 10m, DateTime.Today));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament(" \t\r\n", 10m, DateTime.Today));
        });
        Run("Tournament rejects names exceeding database capacity", () =>
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament(new string('A', 151), 10m, DateTime.Today)));
        Run("Tournament rejects zero and negative prizes", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("LAN Party", 0m, DateTime.Today));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("LAN Party", -1m, DateTime.Today));
        });
        Run("Tournament rejects prizes that would round to zero or overflow SQL decimal", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("LAN Party", 0.001m, DateTime.Today));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("LAN Party", 10.123m, DateTime.Today));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("LAN Party", 10000000000000000m, DateTime.Today));
        });
        Run("Tournament rejects yesterday even at the end of the day", () =>
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateTournament("LAN Party", 10m, DateTime.Today.AddTicks(-1))));

        Run("Score accepts either participating team as the winning team", () =>
        {
            DBTournament.ValidateMatchScore("Alpha", "Beta", 2, 1, "Alpha");
            DBTournament.ValidateMatchScore("Alpha", "Beta", 0, 3, "Beta");
        });
        Run("Score accepts normalized participant names", () =>
            DBTournament.ValidateMatchScore(" Alpha ", "Beta", 2, 1, " alpha "));
        Run("Score rejects negative values", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", -1, 0, "Beta"));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", 0, -1, "Alpha"));
        });
        Run("Score rejects ties and a selected loser", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", 1, 1, "Alpha"));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", 2, 1, "Beta"));
        });
        Run("Score rejects missing or nonparticipating winners", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", 2, 1, ""));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", 2, 1, "Gamma"));
        });
        Run("Score rejects identical teams and oversized serialized results", () =>
        {
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", " alpha ", 2, 1, "Alpha"));
            ExpectThrows<ArgumentException>(() => DBTournament.ValidateMatchScore("Alpha", "Beta", int.MaxValue, int.MaxValue - 1, "Alpha"));
        });
        Run("Score accepts the storage length boundary", () =>
            DBTournament.ValidateMatchScore("Alpha", "Beta", int.MaxValue, 999999999, "Alpha"));

        Run("Station reservation rejects counts other than five before connecting", () =>
        {
            var database = new DBTournament();
            ExpectThrows<ArgumentException>(() => database.LockTournament(1, 1, []));
            ExpectThrows<ArgumentException>(() => database.LockTournament(1, 1, [1, 2, 3, 4]));
            ExpectThrows<ArgumentException>(() => database.LockTournament(1, 1, [1, 2, 3, 4, 5, 6]));
        });
        Run("Station reservation rejects duplicate, nonpositive and null IDs before connecting", () =>
        {
            var database = new DBTournament();
            ExpectThrows<ArgumentException>(() => database.LockTournament(1, 1, [1, 2, 3, 4, 4]));
            ExpectThrows<ArgumentException>(() => database.LockTournament(1, 1, [0, 1, 2, 3, 4]));
            ExpectThrows<ArgumentException>(() => database.LockTournament(1, 1, [-1, 1, 2, 3, 4]));
            ExpectThrows<ArgumentNullException>(() => database.LockTournament(1, 1, null!));
        });

        ApplicationConfiguration.Initialize();
        Run("Tournament form constructs and lays out without opening a database connection", () =>
        {
            using var form = new FormTournament();
            form.CreateControl();
            form.PerformLayout();
            // Exercise the real Load layout hook without showing the form (Shown loads SQL data).
            typeof(Form).GetMethod("OnLoad", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, [EventArgs.Empty]);
            form.PerformLayout();
            Assert(!form.Visible, "Smoke check must not show the form or trigger database loading.");
            Assert(Descendants(form).OfType<DataGridView>().Count() == 2, "Expected tournament and match grids.");
            Assert(Descendants(form).OfType<TabControl>().Single().TabPages.Count == 2, "Expected match and station tabs.");
            Assert(Descendants(form).OfType<CheckedListBox>().Count() == 1, "Expected station selector.");
            Assert(Descendants(form).OfType<Button>().Any(button => button.Text == "Lưu kết quả"), "Missing score action.");
            Assert(Descendants(form).OfType<Button>().Any(button => button.Text == "Khóa 5 máy cho giải đấu"), "Missing reservation action.");
            var split = Descendants(form).OfType<SplitContainer>().Single();
            Assert(split.Panel1.Width >= 300 && split.Panel2.Width >= 600, "Layout leaves insufficient room for the tournament list or match editor.");
        });
        Run("Home form exposes the tournament management action", () =>
        {
            using var form = new Form1();
            Assert(Descendants(form).OfType<Button>().Any(button => button.Text == "Quản lý giải đấu"), "Missing tournament entry point.");
        });
        Run("Switching match rows refreshes the score editor and protects finished results", () =>
        {
            using var form = new FormTournament();
            form.BindingContext = new BindingContext();
            PrivateField<bool>(form, "loading", true);
            var tournaments = new DataTable();
            tournaments.Columns.Add("TournamentId", typeof(int));
            tournaments.Columns.Add("Title");
            tournaments.Columns.Add("Status");
            tournaments.Rows.Add(1, "LAN", "Ongoing");
            var tournamentGrid = PrivateField<DataGridView>(form, "tournamentsGrid");
            tournamentGrid.DataSource = tournaments;
            tournamentGrid.CurrentCell = tournamentGrid.Rows[0].Cells[0];
            var matches = new DataTable();
            matches.Columns.Add("MatchId", typeof(int));
            matches.Columns.Add("TournamentId", typeof(int));
            matches.Columns.Add("RoundNumber", typeof(int));
            foreach (string column in new[] { "Team1Name", "Team2Name", "Score", "WinnerTeamName", "Status", "AssignedStationIds" })
                matches.Columns.Add(column);
            matches.Rows.Add(1, 1, 1, "Alpha", "Beta", DBNull.Value, DBNull.Value, "Pending", DBNull.Value);
            matches.Rows.Add(2, 1, 1, "Gamma", "Delta", "1-2", "Delta", "Finished", DBNull.Value);
            matches.Rows.Add(3, 1, 1, "Epsilon", "Zeta", DBNull.Value, DBNull.Value, "Pending", "1,2,3,4,5");
            var matchGrid = PrivateField<DataGridView>(form, "matchesGrid");
            matchGrid.DataSource = matches;
            matchGrid.CurrentCell = matchGrid.Rows[0].Cells[0];
            PrivateField<bool>(form, "loading", false);
            matchGrid.CurrentCell = matchGrid.Rows[1].Cells[0];
            var winner = PrivateField<ComboBox>(form, "winnerInput");
            Assert(winner.Items.Cast<string>().SequenceEqual(new[] { "Gamma", "Delta" }), "Editor kept the previous row's teams.");
            Assert(winner.SelectedItem as string == "Delta", "Editor did not select the finished row's winner.");
            Assert(PrivateField<NumericUpDown>(form, "team1ScoreInput").Value == 1
                && PrivateField<NumericUpDown>(form, "team2ScoreInput").Value == 2, "Editor kept the previous score.");
            Assert(!PrivateField<Button>(form, "saveScoreButton").Enabled, "Finished result remained editable.");
            Assert(!PrivateField<Button>(form, "lockButton").Enabled, "Finished match allowed station reservation.");
            matchGrid.CurrentCell = matchGrid.Rows[2].Cells[0];
            Assert(!PrivateField<Button>(form, "lockButton").Enabled, "Already assigned match allowed a repeat reservation.");
            matchGrid.CurrentCell = matchGrid.Rows[0].Cells[0];
            Assert(PrivateField<Button>(form, "saveScoreButton").Enabled && PrivateField<Button>(form, "lockButton").Enabled,
                "Pending match did not enable result and reservation actions.");
            PrivateField<bool>(form, "loading", true);
        });

        bool integration = args.Contains("--integration", StringComparer.Ordinal);
        if (integration) RunIntegrationChecks();
        Console.WriteLine($"{passed} checks passed; {failed} failed. Database integration {(integration ? "was requested" : "was not run")}.");
        return failed == 0 ? 0 : 1;
    }

    private static void RunIntegrationChecks()
    {
        string databaseName = "__TournamentChecks_" + Guid.NewGuid().ToString("N");
        // The only database this harness creates or drops has this exact generated name.
        Assert(Regex.IsMatch(databaseName, @"^__TournamentChecks_[a-f0-9]{32}$"), "Invalid scratch database name.");
        using var template = Database.GetConnection();
        var masterBuilder = new SqlConnectionStringBuilder(template.ConnectionString)
        {
            InitialCatalog = "master", ConnectTimeout = 5, Pooling = false
        };
        var scratchBuilder = new SqlConnectionStringBuilder(masterBuilder.ConnectionString) { InitialCatalog = databaseName };
        string masterConnectionString = masterBuilder.ConnectionString;
        string scratchConnectionString = scratchBuilder.ConnectionString;
        bool created = false;
        try
        {
            string schema = File.ReadAllText(FindDatabaseScript()).Replace("GamingCenterDB", databaseName, StringComparison.Ordinal);
            ExecuteSql(masterConnectionString, $"CREATE DATABASE [{databaseName}];");
            created = true;
            foreach (string batch in Regex.Split(schema, @"^[ \t]*GO[ \t]*(?:--[^\r\n]*)?\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(batch)) ExecuteSql(scratchConnectionString, batch);
            }
            var database = new DBTournament(() => new SqlConnection(scratchConnectionString));
            int tournamentId = database.CreateTournament("  Integration LAN  ", "Test Game", 1000000m, DateTime.Today);
            int scoreMatchId = database.CreateMatch(tournamentId, "Alpha", "Beta", 1);
            int lockMatchId = database.CreateMatch(tournamentId, "Gamma", "Delta", 1);

            Run("SQL tournament creation persists normalized valid data", () =>
                Assert(Count(scratchConnectionString, $"SELECT COUNT(*) FROM dbo.Tournament WHERE TournamentId = {tournamentId} AND Title = N'Integration LAN' AND PrizePool = 1000000 AND CAST(StartDate AS date) = CAST(GETDATE() AS date) AND Status = N'Upcoming';") == 1,
                    "Created tournament data did not match the request."));
            Run("SQL invalid tournament creation writes no row", () =>
            {
                int before = Count(scratchConnectionString, "SELECT COUNT(*) FROM dbo.Tournament;");
                ExpectThrows<ArgumentException>(() => database.CreateTournament(" ", "Test", 1m, DateTime.Today));
                ExpectThrows<ArgumentException>(() => database.CreateTournament("Invalid prize", "Test", 0m, DateTime.Today));
                ExpectThrows<ArgumentException>(() => database.CreateTournament("Past date", "Test", 1m, DateTime.Today.AddDays(-1)));
                Assert(Count(scratchConnectionString, "SELECT COUNT(*) FROM dbo.Tournament;") == before, "Validation inserted a tournament.");
            });
            Run("SQL invalid winner leaves the match pending with no score", () =>
            {
                ExpectThrows<ArgumentException>(() => database.UpdateMatchScore(scoreMatchId, 2, 1, "Beta"));
                Assert(Count(scratchConnectionString, $"SELECT COUNT(*) FROM dbo.TournamentMatch WHERE MatchId = {scoreMatchId} AND Status = N'Pending' AND Score IS NULL AND WinnerTeamName IS NULL;") == 1,
                    "Rejected score partially changed the match.");
            });
            Run("SQL valid result saves score, canonical winner and Finished together", () =>
            {
                database.UpdateMatchScore(scoreMatchId, 2, 1, "alpha");
                Assert(Count(scratchConnectionString, $"SELECT COUNT(*) FROM dbo.TournamentMatch WHERE MatchId = {scoreMatchId} AND Score = N'2-1' AND WinnerTeamName = N'Alpha' AND Status = N'Finished';") == 1,
                    "Final match result was incomplete or inconsistent.");
            });
            Run("SQL finished match rejects score changes", () =>
            {
                ExpectThrows<InvalidOperationException>(() => database.UpdateMatchScore(scoreMatchId, 0, 2, "Beta"));
                Assert(Count(scratchConnectionString, $"SELECT COUNT(*) FROM dbo.TournamentMatch WHERE MatchId = {scoreMatchId} AND Score = N'2-1' AND WinnerTeamName = N'Alpha' AND Status = N'Finished';") == 1,
                    "Finished result was overwritten.");
            });
            Run("SQL reservation reports busy PCs including an active session on an Available PC", () =>
            {
                ExecuteSql(scratchConnectionString, """
                    UPDATE dbo.ComputerStation SET Status = N'InUse' WHERE StationId = 1;
                    INSERT INTO dbo.GamingSession (StationId, TariffType, Status) VALUES (2, N'Hourly', N'Active');
                    """);
                InvalidOperationException error = ExpectThrows<InvalidOperationException>(() => database.LockTournament(tournamentId, lockMatchId, [1, 2, 3, 4, 5]));
                Assert(error.Message.Contains("PC-01", StringComparison.Ordinal) && error.Message.Contains("PC-02", StringComparison.Ordinal)
                    && error.Message.Contains("chuyển khách", StringComparison.Ordinal), "Busy warning did not name both machines and request moving customers.");
                AssertNoReservation(scratchConnectionString, tournamentId, lockMatchId);
                using var stations = database.GetStations();
                Assert(Convert.ToBoolean(stations.Select("StationId = 2").Single()["IsOccupied"]), "Station list missed an active session on an Available machine.");
            });
            Run("SQL reservation treats an unclosed session as busy even when marked Completed", () =>
            {
                ExecuteSql(scratchConnectionString, """
                    UPDATE dbo.ComputerStation SET Status = N'Available';
                    DELETE FROM dbo.GamingSession;
                    INSERT INTO dbo.GamingSession (StationId, TariffType, Status) VALUES (3, N'Hourly', N'Completed');
                    """);
                InvalidOperationException error = ExpectThrows<InvalidOperationException>(() => database.LockTournament(tournamentId, lockMatchId, [1, 2, 3, 4, 5]));
                Assert(error.Message.Contains("PC-03", StringComparison.Ordinal), "Unclosed session was not reported.");
                AssertNoReservation(scratchConnectionString, tournamentId, lockMatchId);
            });
            Run("SQL maintenance rejection leaves all reservation data unchanged", () =>
            {
                ExecuteSql(scratchConnectionString, "DELETE FROM dbo.GamingSession; UPDATE dbo.ComputerStation SET Status = N'Maintenance' WHERE StationId = 3;");
                ExpectThrows<InvalidOperationException>(() => database.LockTournament(tournamentId, lockMatchId, [1, 2, 3, 4, 5]));
                AssertNoReservation(scratchConnectionString, tournamentId, lockMatchId);
                Assert(Count(scratchConnectionString, "SELECT COUNT(*) FROM dbo.ComputerStation WHERE StationId = 3 AND Status = N'Maintenance';") == 1,
                    "Maintenance status was overwritten.");
            });
            Run("SQL missing station rejection leaves all reservation data unchanged", () =>
            {
                ExecuteSql(scratchConnectionString, "UPDATE dbo.ComputerStation SET Status = N'Available';");
                ExpectThrows<InvalidOperationException>(() => database.LockTournament(tournamentId, lockMatchId, [1, 2, 3, 4, int.MaxValue]));
                AssertNoReservation(scratchConnectionString, tournamentId, lockMatchId);
            });
            Run("SQL successful reservation locks exactly five and persists the match assignment", () =>
            {
                database.LockTournament(tournamentId, lockMatchId, [5, 3, 1, 4, 2]);
                Assert(Count(scratchConnectionString, "SELECT COUNT(*) FROM dbo.ComputerStation WHERE Status = N'TournamentLocked';") == 5,
                    "Reservation did not lock exactly five stations.");
                Assert(Count(scratchConnectionString, "SELECT COUNT(*) FROM dbo.ComputerStation WHERE StationId IN (1, 2, 3, 4, 5) AND Status = N'TournamentLocked';") == 5,
                    "Reservation locked the wrong stations.");
                Assert(Count(scratchConnectionString, $"SELECT COUNT(*) FROM dbo.TournamentMatch WHERE MatchId = {lockMatchId} AND AssignedStationIds = N'1,2,3,4,5';") == 1,
                    "Station assignment was not persisted.");
                Assert(Count(scratchConnectionString, $"SELECT COUNT(*) FROM dbo.Tournament WHERE TournamentId = {tournamentId} AND Status = N'Ongoing';") == 1,
                    "Tournament did not move to Ongoing.");
            });
            Run("SQL second reservation cannot overwrite an existing match assignment", () =>
            {
                ExpectThrows<InvalidOperationException>(() => database.LockTournament(tournamentId, lockMatchId, [6, 7, 8, 9, 10]));
                Assert(Count(scratchConnectionString, "SELECT COUNT(*) FROM dbo.ComputerStation WHERE StationId IN (6, 7, 8, 9, 10) AND Status = N'Available';") == 5,
                    "Rejected repeat reservation changed other machines.");
            });
        }
        catch (Exception ex)
        {
            failed++;
            Console.Error.WriteLine($"FAIL SQL integration setup: {ex.GetBaseException().Message}");
        }
        finally
        {
            if (created)
            {
                try
                {
                    ExecuteSql(masterConnectionString, $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];");
                    Console.WriteLine("Scratch database removed.");
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.Error.WriteLine($"FAIL scratch database cleanup ({databaseName}): {ex.GetBaseException().Message}");
                }
            }
        }
    }

    private static void AssertNoReservation(string connectionString, int tournamentId, int matchId)
    {
        Assert(Count(connectionString, "SELECT COUNT(*) FROM dbo.ComputerStation WHERE Status = N'TournamentLocked';") == 0,
            "Rejected reservation partially locked stations.");
        Assert(Count(connectionString, $"SELECT COUNT(*) FROM dbo.TournamentMatch WHERE MatchId = {matchId} AND AssignedStationIds IS NULL AND Status = N'Pending';") == 1,
            "Rejected reservation changed the match assignment.");
        Assert(Count(connectionString, $"SELECT COUNT(*) FROM dbo.Tournament WHERE TournamentId = {tournamentId} AND Status = N'Upcoming';") == 1,
            "Rejected reservation changed the tournament status.");
    }

    private static string FindDatabaseScript()
    {
        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            for (DirectoryInfo? directory = new(start); directory != null; directory = directory.Parent)
            {
                string candidate = Path.Combine(directory.FullName, "Database.sql");
                if (File.Exists(candidate)) return candidate;
            }
        }
        throw new FileNotFoundException("Could not locate the repository Database.sql script.");
    }

    private static int Count(string connectionString, string sql)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void ExecuteSql(string connectionString, string sql)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        command.ExecuteNonQuery();
    }

    private static int ProbeDatabase()
    {
        try
        {
            using var connection = Database.GetConnection();
            var builder = new SqlConnectionStringBuilder(connection.ConnectionString) { ConnectTimeout = 5 };
            connection.ConnectionString = builder.ConnectionString;
            connection.Open();
            using var command = new SqlCommand("SELECT DB_NAME();", connection);
            Console.WriteLine($"Database connection succeeded: {command.ExecuteScalar()}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Database probe failed ({ex.GetType().Name}): {ex.Message}");
            return 2;
        }
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control descendant in Descendants(child))
                yield return descendant;
        }
    }

    private static T PrivateField<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;

    private static void PrivateField<T>(object instance, string name, T value) =>
        instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(instance, value);

    private static void Run(string name, Action check)
    {
        try
        {
            check();
            passed++;
            Console.WriteLine($"PASS {name}");
        }
        catch (Exception ex)
        {
            failed++;
            Console.Error.WriteLine($"FAIL {name}: {ex.GetBaseException().Message}");
        }
    }

    private static TException ExpectThrows<TException>(Action action) where TException : Exception
    {
        try { action(); }
        catch (TException ex) { return ex; }
        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
