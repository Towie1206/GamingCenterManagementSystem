using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem;

public sealed class DBTournament
{
    private const decimal MaximumPrizePool = 9999999999999999.99m;
    private readonly Func<SqlConnection> connectionFactory;

    public DBTournament() : this(Database.GetConnection)
    {
    }

    public DBTournament(Func<SqlConnection> connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        this.connectionFactory = connectionFactory;
    }

    public DataTable GetTournaments() => ReadTable("""
        SELECT TournamentId, Title, GameTitle, FormatType, PrizePool, Status, StartDate
        FROM dbo.Tournament ORDER BY StartDate DESC, TournamentId DESC;
        """);

    public DataTable GetMatches(int tournamentId) => ReadTable("""
        SELECT MatchId, TournamentId, RoundNumber, Team1Name, Team2Name,
               WinnerTeamName, Score, AssignedStationIds, Status
        FROM dbo.TournamentMatch WHERE TournamentId = @TournamentId
        ORDER BY RoundNumber, MatchId;
        """, new SqlParameter("@TournamentId", SqlDbType.Int) { Value = tournamentId });

    public DataTable GetStations() => ReadTable("""
        SELECT s.StationId, s.StationName, s.Status,
               CAST(CASE WHEN s.Status = N'InUse' OR EXISTS (
                   SELECT 1 FROM dbo.GamingSession AS gs
                   WHERE gs.StationId = s.StationId
                     AND (gs.Status = N'Active' OR gs.EndTime IS NULL)
               ) THEN 1 ELSE 0 END AS bit) AS IsOccupied
        FROM dbo.ComputerStation AS s ORDER BY s.StationId;
        """);

    /// <summary>Checks the required tournament fields before opening a database connection.</summary>
    public static void ValidateTournament(string title, decimal prizePool, DateTime startDate)
    {
        ValidateText(title, 150, "Tên giải đấu");
        if (prizePool <= 0 || prizePool > MaximumPrizePool)
            throw new ArgumentException("Tổng giải thưởng phải lớn hơn 0 và không vượt quá 9.999.999.999.999.999,99.", nameof(prizePool));
        if (decimal.Round(prizePool, 2) != prizePool)
            throw new ArgumentException("Tổng giải thưởng chỉ được có tối đa 2 chữ số thập phân.", nameof(prizePool));
        if (startDate.Date < DateTime.Today)
            throw new ArgumentException("Ngày bắt đầu phải từ ngày hiện tại trở đi.", nameof(startDate));
    }

    /// <summary>Creates a tournament only after validating the title, prize pool and start date.</summary>
    public int CreateTournament(string title, string gameTitle, decimal prizePool,
        DateTime startDate, string formatType = "SingleElimination")
    {
        ValidateTournament(title, prizePool, startDate);
        ValidateText(gameTitle, 100, "Tên trò chơi");
        ValidateText(formatType, 50, "Thể thức");

        using var connection = connectionFactory();
        connection.Open();
        using var command = new SqlCommand("""
            INSERT INTO dbo.Tournament (Title, GameTitle, FormatType, PrizePool, StartDate, Status)
            OUTPUT INSERTED.TournamentId
            VALUES (@Title, @GameTitle, @FormatType, @PrizePool, @StartDate, N'Upcoming');
            """, connection);
        AddText(command, "@Title", 150, title.Trim());
        AddText(command, "@GameTitle", 100, gameTitle.Trim());
        AddText(command, "@FormatType", 50, formatType.Trim());
        var prizeParameter = command.Parameters.Add("@PrizePool", SqlDbType.Decimal);
        prizeParameter.Precision = 18;
        prizeParameter.Scale = 2;
        prizeParameter.Value = prizePool;
        command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = startDate.Date;
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    /// <summary>Adds a pending match with two distinct teams to an unfinished tournament.</summary>
    public int CreateMatch(int tournamentId, string team1Name, string team2Name, int roundNumber)
    {
        ValidateText(team1Name, 100, "Tên đội 1");
        ValidateText(team2Name, 100, "Tên đội 2");
        if (string.Equals(team1Name.Trim(), team2Name.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Hai đội thi đấu phải có tên khác nhau.");
        if (roundNumber <= 0)
            throw new ArgumentException("Vòng đấu phải lớn hơn 0.", nameof(roundNumber));

        using var connection = connectionFactory();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        RequireTournament(connection, transaction, tournamentId);
        using var command = new SqlCommand("""
            INSERT INTO dbo.TournamentMatch
                (TournamentId, RoundNumber, Team1Name, Team2Name, Status)
            OUTPUT INSERTED.MatchId
            VALUES (@TournamentId, @RoundNumber, @Team1Name, @Team2Name, N'Pending');
            """, connection, transaction);
        command.Parameters.Add("@TournamentId", SqlDbType.Int).Value = tournamentId;
        command.Parameters.Add("@RoundNumber", SqlDbType.Int).Value = roundNumber;
        AddText(command, "@Team1Name", 100, team1Name.Trim());
        AddText(command, "@Team2Name", 100, team2Name.Trim());
        int matchId = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        transaction.Commit();
        return matchId;
    }

    /// <summary>Rejects invalid scores, ties, and winners inconsistent with the participating teams or score.</summary>
    public static void ValidateMatchScore(string team1Name, string team2Name,
        int team1Score, int team2Score, string winnerTeamName)
    {
        ValidateText(team1Name, 100, "Tên đội 1");
        ValidateText(team2Name, 100, "Tên đội 2");
        ValidateText(winnerTeamName, 100, "Đội thắng");
        if (string.Equals(team1Name.Trim(), team2Name.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Hai đội thi đấu phải có tên khác nhau.");
        if (team1Score < 0 || team2Score < 0)
            throw new ArgumentException("Tỉ số không được âm.");
        if (team1Score == team2Score)
            throw new ArgumentException("Trận đấu phải có đội thắng; tỉ số không được hòa.");
        if (FormatScore(team1Score, team2Score).Length > 20)
            throw new ArgumentException("Tỉ số vượt quá độ dài lưu trữ cho phép (20 ký tự).");

        string expectedWinner = team1Score > team2Score ? team1Name : team2Name;
        if (!string.Equals(winnerTeamName.Trim(), expectedWinner.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Đội thắng phải là đội tham gia trận đấu và có tỉ số cao hơn.", nameof(winnerTeamName));
    }

    /// <summary>Saves a validated score and winner, and marks the selected match Finished atomically.</summary>
    public void UpdateMatchScore(int matchId, int team1Score, int team2Score, string winnerTeamName)
    {
        using var connection = connectionFactory();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // Read the owner first so all mutations take locks in tournament -> match order.
        using var ownerCommand = new SqlCommand(
            "SELECT TournamentId FROM dbo.TournamentMatch WHERE MatchId = @MatchId;", connection, transaction);
        ownerCommand.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
        object? tournamentId = ownerCommand.ExecuteScalar();
        if (tournamentId is null)
            throw new InvalidOperationException("Không tìm thấy trận đấu đã chọn. Vui lòng tải lại danh sách.");
        RequireTournament(connection, transaction, Convert.ToInt32(tournamentId, CultureInfo.InvariantCulture));

        string team1Name;
        string team2Name;
        using (var matchCommand = new SqlCommand("""
            SELECT Team1Name, Team2Name, Status FROM dbo.TournamentMatch WITH (UPDLOCK, HOLDLOCK)
            WHERE MatchId = @MatchId;
            """, connection, transaction))
        {
            matchCommand.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
            using var reader = matchCommand.ExecuteReader();
            if (!reader.Read())
                throw new InvalidOperationException("Không tìm thấy trận đấu đã chọn. Vui lòng tải lại danh sách.");
            team1Name = reader.GetString(0);
            team2Name = reader.GetString(1);
            if (reader.GetString(2) == "Finished")
                throw new InvalidOperationException("Trận đấu đã kết thúc và đã có kết quả. Vui lòng tải lại danh sách.");
        }
        ValidateMatchScore(team1Name, team2Name, team1Score, team2Score, winnerTeamName);
        string winner = team1Score > team2Score ? team1Name : team2Name;
        using var updateCommand = new SqlCommand("""
            UPDATE dbo.TournamentMatch
            SET Score = @Score, WinnerTeamName = @WinnerTeamName, Status = N'Finished'
            WHERE MatchId = @MatchId;
            """, connection, transaction);
        AddText(updateCommand, "@Score", 20, FormatScore(team1Score, team2Score));
        AddText(updateCommand, "@WinnerTeamName", 100, winner);
        updateCommand.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
        updateCommand.ExecuteNonQuery();
        transaction.Commit();
    }

    /// <summary>Checks and reserves exactly five unoccupied stations for a pending match in one transaction.</summary>
    public void LockTournament(int tournamentId, int matchId, IReadOnlyCollection<int> stationIds)
    {
        ArgumentNullException.ThrowIfNull(stationIds);
        int[] ids = stationIds.OrderBy(id => id).ToArray();
        if (ids.Length != 5 || ids.Distinct().Count() != 5 || ids.Any(id => id <= 0))
            throw new ArgumentException("Vui lòng chọn đúng 5 máy thi đấu khác nhau.", nameof(stationIds));

        using var connection = connectionFactory();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        RequireTournament(connection, transaction, tournamentId);

        using (var matchCommand = new SqlCommand("""
            SELECT Status, AssignedStationIds FROM dbo.TournamentMatch WITH (UPDLOCK, HOLDLOCK)
            WHERE MatchId = @MatchId AND TournamentId = @TournamentId;
            """, connection, transaction))
        {
            matchCommand.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
            matchCommand.Parameters.Add("@TournamentId", SqlDbType.Int).Value = tournamentId;
            using var reader = matchCommand.ExecuteReader();
            if (!reader.Read())
                throw new InvalidOperationException("Trận đấu không thuộc giải đấu đã chọn hoặc không còn tồn tại.");
            if (reader.GetString(0) != "Pending")
                throw new InvalidOperationException("Chỉ có thể khóa máy cho trận đấu đang chờ (Pending).");
            if (!reader.IsDBNull(1) && !string.IsNullOrWhiteSpace(reader.GetString(1)))
                throw new InvalidOperationException("Trận đấu đã được phân máy. Không thể khóa máy lần nữa.");
        }

        string parameterNames = string.Join(", ", ids.Select((_, index) => $"@Station{index}"));
        // HOLDLOCK also protects the session ranges against inserts until the reservation commits.
        using (var stationCommand = new SqlCommand($"""
            SELECT s.StationId, s.StationName, s.Status,
                   CAST(CASE WHEN s.Status = N'InUse' OR EXISTS (
                       SELECT 1 FROM dbo.GamingSession AS gs WITH (UPDLOCK, HOLDLOCK)
                       WHERE gs.StationId = s.StationId
                         AND (gs.Status = N'Active' OR gs.EndTime IS NULL)
                   ) THEN 1 ELSE 0 END AS bit) AS IsOccupied
            FROM dbo.ComputerStation AS s WITH (UPDLOCK, HOLDLOCK)
            WHERE s.StationId IN ({parameterNames}) ORDER BY s.StationId;
            """, connection, transaction))
        {
            AddStationIds(stationCommand, ids);
            var occupied = new List<string>();
            var unavailable = new List<string>();
            int count = 0;
            using var reader = stationCommand.ExecuteReader();
            while (reader.Read())
            {
                count++;
                string stationName = reader.GetString(1);
                if (reader.GetBoolean(3))
                    occupied.Add(stationName);
                else if (reader.GetString(2) != "Available")
                    unavailable.Add(stationName);
            }
            if (occupied.Count > 0)
                throw new InvalidOperationException($"Máy {string.Join(", ", occupied)} đang có khách. Vui lòng chuyển khách sang máy khác trước khi khóa giải đấu.");
            if (count != 5)
                throw new InvalidOperationException("Có máy thi đấu không còn tồn tại. Vui lòng tải lại danh sách và chọn đủ 5 máy.");
            if (unavailable.Count > 0)
                throw new InvalidOperationException($"Máy {string.Join(", ", unavailable)} không sẵn sàng (đang bảo trì hoặc đã khóa). Vui lòng chọn máy khác.");
        }

        using (var lockCommand = new SqlCommand($"""
            UPDATE dbo.ComputerStation SET Status = N'TournamentLocked'
            WHERE StationId IN ({parameterNames});
            """, connection, transaction))
        {
            AddStationIds(lockCommand, ids);
            if (lockCommand.ExecuteNonQuery() != 5)
                throw new InvalidOperationException("Không thể khóa đủ 5 máy. Chưa có thay đổi nào được lưu.");
        }
        using (var assignCommand = new SqlCommand("""
            UPDATE dbo.TournamentMatch SET AssignedStationIds = @StationIds WHERE MatchId = @MatchId;
            UPDATE dbo.Tournament SET Status = N'Ongoing' WHERE TournamentId = @TournamentId;
            """, connection, transaction))
        {
            AddText(assignCommand, "@StationIds", 100, string.Join(",", ids));
            assignCommand.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
            assignCommand.Parameters.Add("@TournamentId", SqlDbType.Int).Value = tournamentId;
            assignCommand.ExecuteNonQuery();
        }
        transaction.Commit();
    }

    private static void RequireTournament(SqlConnection connection, SqlTransaction transaction, int tournamentId)
    {
        using var command = new SqlCommand("""
            SELECT Status FROM dbo.Tournament WITH (UPDLOCK, HOLDLOCK) WHERE TournamentId = @TournamentId;
            """, connection, transaction);
        command.Parameters.Add("@TournamentId", SqlDbType.Int).Value = tournamentId;
        object? status = command.ExecuteScalar();
        if (status is null)
            throw new InvalidOperationException("Không tìm thấy giải đấu đã chọn. Vui lòng tải lại danh sách.");
        if (string.Equals(Convert.ToString(status, CultureInfo.InvariantCulture), "Finished", StringComparison.Ordinal))
            throw new InvalidOperationException("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.");
    }

    private DataTable ReadTable(string sql, params SqlParameter[] parameters)
    {
        using var connection = connectionFactory();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private static void ValidateText(string value, int maximumLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} không được để trống.");
        if (value.Trim().Length > maximumLength)
            throw new ArgumentException($"{fieldName} không được dài quá {maximumLength} ký tự.");
    }

    private static void AddText(SqlCommand command, string name, int size, string value) =>
        command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value;

    private static void AddStationIds(SqlCommand command, int[] ids)
    {
        for (int index = 0; index < ids.Length; index++)
            command.Parameters.Add($"@Station{index}", SqlDbType.Int).Value = ids[index];
    }

    private static string FormatScore(int team1Score, int team2Score) =>
        string.Create(CultureInfo.InvariantCulture, $"{team1Score}-{team2Score}");
}
