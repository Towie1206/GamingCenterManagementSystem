using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using GamingCenterManagementSystem.DAL;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem.Tests;

[Collection("GamingCenterSequential")]
public sealed class TournamentSqlTests : IClassFixture<TournamentSqlTests.SqlDatabase>
{
    private readonly SqlDatabase sqlDatabase;

    public TournamentSqlTests(SqlDatabase sqlDatabase) => this.sqlDatabase = sqlDatabase;

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void AutoAdvanceRound_SqlPersistsPairsAndGeneratedIds(int winnerCount)
    {
        using var tournament = sqlDatabase.CreateTournament();
        for (int index = 1; index <= winnerCount; index++)
            tournament.FinishedMatch($"Winner {index}", $"Loser {index}");

        var result = tournament.Database.AutoAdvanceRound(tournament.Id);

        Assert.Equal(winnerCount / 2, result.CreatedMatches.Count);
        Assert.Equal("Ongoing", tournament.Status);
        Assert.Equal(1, result.CompletedRound);
        Assert.Equal(2, result.NextRound);
        Assert.False(result.IsTournamentFinished);
        var persisted = tournament.Database.GetMatchesByTournament(tournament.Id)
            .Where(match => match.RoundNumber == 2).ToList();
        Assert.Equal(result.CreatedMatches.Select(match => match.MatchId), persisted.Select(match => match.MatchId));
        for (int index = 0; index < persisted.Count; index++)
        {
            var match = persisted[index];
            Assert.True(match.MatchId > 0);
            Assert.Equal($"Winner {index * 2 + 1}", match.Team1Name);
            Assert.Equal($"Winner {index * 2 + 2}", match.Team2Name);
            Assert.Equal("Pending", match.Status);
            Assert.Equal("0 - 0", match.Score);
            // Check SQL NULL directly: the existing reader converts DBNull to an empty string.
            Assert.Equal(1, tournament.Count("RoundNumber = 2 AND MatchId = " + match.MatchId +
                " AND WinnerTeamName IS NULL AND AssignedStationIds IS NULL"));
        }
        Assert.Throws<InvalidOperationException>(() => tournament.Database.AutoAdvanceRound(tournament.Id));
        Assert.Equal(winnerCount / 2, tournament.Count("RoundNumber = 2"));
    }

    [Fact]
    public void AutoAdvanceRound_SqlPersistsByeAndFinishesAfterFinalWithoutExtraRound()
    {
        using var tournament = sqlDatabase.CreateTournament("DoubleElimination");
        tournament.FinishedMatch("Alpha", "Loser A");
        tournament.FinishedMatch("Bravo", "Loser B");
        tournament.FinishedMatch("Charlie", "Loser C");

        var secondRound = tournament.Database.AutoAdvanceRound(tournament.Id);

        Assert.Equal("Charlie", secondRound.ByeTeamName);
        Assert.Equal(2, secondRound.CreatedMatches.Count);
        Assert.Equal(1, tournament.Count("RoundNumber = 2 AND Team1Name = N'Charlie'" +
            " AND Team2Name = N'BYE (Miễn đấu)' AND WinnerTeamName = N'Charlie'" +
            " AND Score = N'1-0' AND Status = N'Finished' AND AssignedStationIds IS NULL"));
        var semifinal = Assert.Single(secondRound.CreatedMatches, match => match.Status == "Pending");
        tournament.Database.UpdateMatchScore(semifinal.MatchId, 2, 0, "Alpha");

        var thirdRound = tournament.Database.AutoAdvanceRound(tournament.Id);
        var final = Assert.Single(thirdRound.CreatedMatches);
        Assert.Equal(3, final.RoundNumber);
        Assert.Equal("Alpha", final.Team1Name);
        Assert.Equal("Charlie", final.Team2Name);
        tournament.Database.UpdateMatchScore(final.MatchId, 2, 1, "Alpha");
        int beforeFinalAdvance = tournament.Count("1 = 1");

        var finished = tournament.Database.AutoAdvanceRound(tournament.Id);

        Assert.True(finished.IsTournamentFinished);
        Assert.Equal("Alpha", finished.ChampionTeamName);
        Assert.Equal(3, finished.CompletedRound);
        Assert.Equal(3, finished.NextRound);
        Assert.Empty(finished.CreatedMatches);
        Assert.Equal("Finished", tournament.Status);
        Assert.Equal(beforeFinalAdvance, tournament.Count("1 = 1"));
        Assert.Equal(0, tournament.Count("RoundNumber > 3"));
        Assert.Throws<InvalidOperationException>(() => tournament.Database.AutoAdvanceRound(tournament.Id));
    }

    [Fact]
    public void AutoAdvanceRound_SqlConstraintFailureRollsBackAllPairsWithoutChangingOfflineState()
    {
        using var tournament = sqlDatabase.CreateTournament();
        tournament.FinishedMatch("Alpha", "Loser A");
        tournament.FinishedMatch("Bravo", "Loser B");
        tournament.FinishedMatch("Reject winner", "Loser C");
        tournament.FinishedMatch("Delta", "Loser D");
        string[] fallbackBefore = SnapshotOfflineState();

        // The scratch schema rejects the second pair, after the first INSERT succeeded.
        var error = Assert.Throws<SqlException>(() => tournament.Database.AutoAdvanceRound(tournament.Id));

        Assert.Equal(547, error.Number);
        Assert.Equal(0, tournament.Count("RoundNumber = 2"));
        Assert.Equal(4, tournament.Count("1 = 1"));
        Assert.Equal("Upcoming", tournament.Status);
        Assert.Equal(fallbackBefore, SnapshotOfflineState());
    }

    [Fact]
    public async Task AutoAdvanceRound_ConcurrentSqlCallsCreateOnlyOneNextRound()
    {
        using var tournament = sqlDatabase.CreateTournament();
        tournament.FinishedMatch("Alpha", "Loser A");
        tournament.FinishedMatch("Bravo", "Loser B");
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<(AdvanceRoundResult? Result, Exception? Error)> Advance()
        {
            await start.Task;
            try { return (tournament.Database.AutoAdvanceRound(tournament.Id), null); }
            catch (Exception ex) { return (null, ex); }
        }
        var first = Task.Run(Advance);
        var second = Task.Run(Advance);

        start.SetResult();
        var outcomes = await Task.WhenAll(first, second);

        Assert.Single(outcomes, outcome => outcome.Result != null);
        Assert.IsType<InvalidOperationException>(Assert.Single(outcomes, outcome => outcome.Error != null).Error);
        Assert.Equal(1, tournament.Count("RoundNumber = 2"));
        Assert.Equal(3, tournament.Count("1 = 1"));
    }

    private static string[] SnapshotOfflineState()
    {
        var offline = new DBTournament(() => throw new IOException("SQL integration offline-state probe"));
        return offline.GetAll().OrderBy(tournament => tournament.TournamentId).SelectMany(tournament =>
            new[] { $"T|{tournament.TournamentId}|{tournament.Status}" }.Concat(
                offline.GetMatchesByTournament(tournament.TournamentId).OrderBy(match => match.MatchId)
                    .Select(match => $"M|{match.MatchId}|{match.RoundNumber}|{match.Team1Name}|{match.Team2Name}|{match.WinnerTeamName}|{match.Score}|{match.AssignedStationIds}|{match.Status}")))
            .ToArray();
    }

    public sealed class SqlDatabase : IDisposable
    {
        private readonly string databaseName = "__TournamentSqlTests_" + Guid.NewGuid().ToString("N");
        private readonly string masterConnectionString;
        private readonly string connectionString;
        private bool created;

        public SqlDatabase()
        {
            using var template = GamingCenterManagementSystem.Database.GetConnection();
            var builder = new SqlConnectionStringBuilder(template.ConnectionString)
            {
                InitialCatalog = "master", ConnectTimeout = 30, Pooling = false
            };
            masterConnectionString = builder.ConnectionString;
            builder.InitialCatalog = databaseName;
            connectionString = builder.ConnectionString;
            try
            {
                using (var master = new SqlConnection(masterConnectionString))
                {
                    master.Open();
                    using var command = new SqlCommand($"CREATE DATABASE [{databaseName}];", master);
                    command.ExecuteNonQuery();
                    created = true;
                }
                using var connection = OpenConnection();
                using var schema = new SqlCommand("""
                    CREATE TABLE dbo.Tournament (
                        TournamentId INT IDENTITY(1, 1) PRIMARY KEY,
                        Title NVARCHAR(150) NOT NULL,
                        GameTitle NVARCHAR(100) NOT NULL,
                        FormatType NVARCHAR(50) NOT NULL,
                        PrizePool DECIMAL(18, 2) NOT NULL,
                        Status NVARCHAR(30) NOT NULL,
                        StartDate DATETIME NOT NULL
                    );
                    CREATE TABLE dbo.TournamentMatch (
                        MatchId INT IDENTITY(1, 1) PRIMARY KEY,
                        TournamentId INT NOT NULL REFERENCES dbo.Tournament(TournamentId),
                        RoundNumber INT NOT NULL,
                        Team1Name NVARCHAR(100) NOT NULL,
                        Team2Name NVARCHAR(100) NOT NULL,
                        WinnerTeamName NVARCHAR(100) NULL,
                        Score NVARCHAR(20) NULL,
                        AssignedStationIds NVARCHAR(100) NULL,
                        Status NVARCHAR(20) NOT NULL,
                        CONSTRAINT CK_TournamentSqlTests_RejectPair
                            CHECK (RoundNumber <> 2 OR Team1Name <> N'Reject winner')
                    );
                    """, connection);
                schema.ExecuteNonQuery();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public TournamentScope CreateTournament(string format = "SingleElimination")
        {
            using var connection = OpenConnection();
            using var command = new SqlCommand("""
                INSERT INTO dbo.Tournament (Title, GameTitle, FormatType, PrizePool, Status, StartDate)
                OUTPUT INSERTED.TournamentId
                VALUES (N'SQL integration test', N'Test game', @Format, 100, N'Upcoming', GETDATE());
                """, connection);
            command.Parameters.Add("@Format", SqlDbType.NVarChar, 50).Value = format;
            int id = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            return new TournamentScope(this, id, new DBTournament(() => new SqlConnection(connectionString)));
        }

        public void Dispose()
        {
            if (!created) return;
            // This fixture may only remove the database whose exact generated name it created.
            if (!Regex.IsMatch(databaseName, @"^__TournamentSqlTests_[a-f0-9]{32}$", RegexOptions.CultureInvariant))
                throw new InvalidOperationException("Unexpected SQL test database name.");
            using var master = new SqlConnection(masterConnectionString);
            master.Open();
            using var command = new SqlCommand($"""
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
                """, master);
            command.ExecuteNonQuery();
            created = false;
        }
    }

    public sealed class TournamentScope : IDisposable
    {
        private readonly SqlDatabase sqlDatabase;
        public int Id { get; }
        public DBTournament Database { get; }

        public TournamentScope(SqlDatabase sqlDatabase, int id, DBTournament database)
        {
            this.sqlDatabase = sqlDatabase;
            Id = id;
            Database = database;
        }

        public string Status
        {
            get
            {
                using var connection = sqlDatabase.OpenConnection();
                using var command = new SqlCommand("SELECT Status FROM dbo.Tournament WHERE TournamentId = @Id;", connection);
                command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                return Convert.ToString(command.ExecuteScalar(), CultureInfo.InvariantCulture)!;
            }
        }

        public void FinishedMatch(string winner, string loser)
        {
            using var connection = sqlDatabase.OpenConnection();
            using var command = new SqlCommand("""
                INSERT INTO dbo.TournamentMatch
                    (TournamentId, RoundNumber, Team1Name, Team2Name, WinnerTeamName, Score, Status)
                VALUES (@Id, 1, @Winner, @Loser, @Winner, N'2-0', N'Finished');
                """, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
            command.Parameters.Add("@Winner", SqlDbType.NVarChar, 100).Value = winner;
            command.Parameters.Add("@Loser", SqlDbType.NVarChar, 100).Value = loser;
            command.ExecuteNonQuery();
        }

        public int Count(string predicate)
        {
            using var connection = sqlDatabase.OpenConnection();
            using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.TournamentMatch WHERE TournamentId = @Id AND " + predicate, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
            return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            using var connection = sqlDatabase.OpenConnection();
            using var command = new SqlCommand("""
                DELETE FROM dbo.TournamentMatch WHERE TournamentId = @Id;
                DELETE FROM dbo.Tournament WHERE TournamentId = @Id;
                """, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
            command.ExecuteNonQuery();
        }
    }
}
