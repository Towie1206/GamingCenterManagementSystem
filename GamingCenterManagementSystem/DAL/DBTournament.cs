using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    public class DBTournament : IRepository<Tournament>
    {
        public const decimal MaximumPrizePool = 9999999999999999.99m;

        private readonly Func<SqlConnection> _connectionFactory;

        private static readonly List<Tournament> _fallbackTournaments = new()
        {
            new Tournament { TournamentId = 1, Title = "EAUT Valorant Championship 2026", GameTitle = "Valorant", FormatType = "SingleElimination", PrizePool = 10000000, Status = "Ongoing", StartDate = DateTime.Now.AddDays(-1) },
            new Tournament { TournamentId = 2, Title = "Đông Á League of Legends LAN Cup", GameTitle = "League of Legends", FormatType = "DoubleElimination", PrizePool = 15000000, Status = "Upcoming", StartDate = DateTime.Now.AddDays(3) }
        };

        private static readonly List<TournamentMatch> _fallbackMatches = new()
        {
            new TournamentMatch { MatchId = 1, TournamentId = 1, RoundNumber = 1, Team1Name = "Dragon Slayer", Team2Name = "Cyber Hunters", WinnerTeamName = "Dragon Slayer", Score = "2 - 1", AssignedStationIds = "PC-06,PC-07,PC-08,PC-09,PC-10", Status = "Finished" },
            new TournamentMatch { MatchId = 2, TournamentId = 1, RoundNumber = 2, Team1Name = "Dragon Slayer", Team2Name = "Phoenix Gaming", WinnerTeamName = null, Score = "0 - 0", AssignedStationIds = "PC-06,PC-07,PC-08,PC-09,PC-10", Status = "Playing" }
        };

        public DBTournament() : this(Database.GetConnection)
        {
        }

        public DBTournament(Func<SqlConnection> connectionFactory)
        {
            ArgumentNullException.ThrowIfNull(connectionFactory);
            _connectionFactory = connectionFactory;
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

        public List<Tournament> GetAll()
        {
            List<Tournament> list = new List<Tournament>();
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string sql = "SELECT TournamentId, Title, GameTitle, FormatType, PrizePool, Status, StartDate FROM Tournament ORDER BY TournamentId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Tournament
                            {
                                TournamentId = Convert.ToInt32(reader["TournamentId"]),
                                Title = reader["Title"].ToString() ?? "",
                                GameTitle = reader["GameTitle"].ToString() ?? "",
                                FormatType = reader["FormatType"].ToString() ?? "SingleElimination",
                                PrizePool = Convert.ToDecimal(reader["PrizePool"]),
                                Status = reader["Status"].ToString() ?? "Upcoming",
                                StartDate = Convert.ToDateTime(reader["StartDate"])
                            });
                        }
                    }
                }
                return list;
            }
            catch
            {
                return new List<Tournament>(_fallbackTournaments);
            }
        }

        public Tournament? GetById(int id)
        {
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string sql = "SELECT TournamentId, Title, GameTitle, FormatType, PrizePool, Status, StartDate FROM Tournament WHERE TournamentId = @TournamentId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TournamentId", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Tournament
                                {
                                    TournamentId = Convert.ToInt32(reader["TournamentId"]),
                                    Title = reader["Title"].ToString() ?? "",
                                    GameTitle = reader["GameTitle"].ToString() ?? "",
                                    FormatType = reader["FormatType"].ToString() ?? "SingleElimination",
                                    PrizePool = Convert.ToDecimal(reader["PrizePool"]),
                                    Status = reader["Status"].ToString() ?? "Upcoming",
                                    StartDate = Convert.ToDateTime(reader["StartDate"])
                                };
                            }
                        }
                    }
                }
            }
            catch
            {
                return _fallbackTournaments.Find(t => t.TournamentId == id);
            }
            return null;
        }

        public bool Add(Tournament entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ValidateTournament(entity.Title, entity.PrizePool, entity.StartDate);
            ValidateText(entity.GameTitle, 100, "Tên trò chơi");
            if (!string.IsNullOrWhiteSpace(entity.FormatType))
                ValidateText(entity.FormatType, 50, "Thể thức");

            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Tournament (Title, GameTitle, FormatType, PrizePool, Status, StartDate)
                                   OUTPUT INSERTED.TournamentId
                                   VALUES (@Title, @GameTitle, @FormatType, @PrizePool, @Status, @StartDate)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", entity.Title.Trim());
                        cmd.Parameters.AddWithValue("@GameTitle", entity.GameTitle.Trim());
                        cmd.Parameters.AddWithValue("@FormatType", (entity.FormatType ?? "SingleElimination").Trim());
                        var prizeParam = cmd.Parameters.Add("@PrizePool", SqlDbType.Decimal);
                        prizeParam.Precision = 18;
                        prizeParam.Scale = 2;
                        prizeParam.Value = entity.PrizePool;
                        cmd.Parameters.AddWithValue("@Status", (entity.Status ?? "Upcoming").Trim());
                        cmd.Parameters.AddWithValue("@StartDate", entity.StartDate.Date);

                        object? result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            entity.TournamentId = Convert.ToInt32(result, CultureInfo.InvariantCulture);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                entity.TournamentId = _fallbackTournaments.Count > 0 ? _fallbackTournaments.Max(t => t.TournamentId) + 1 : 1;
                _fallbackTournaments.Add(entity);
                return true;
            }
        }

        public bool Update(Tournament entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (entity.StartDate.Date < DateTime.Today && (entity.Status == "Ongoing" || entity.Status == "Finished"))
            {
                ValidateText(entity.Title, 150, "Tên giải đấu");
                if (entity.PrizePool <= 0 || entity.PrizePool > MaximumPrizePool)
                    throw new ArgumentException("Tổng giải thưởng phải lớn hơn 0 và không vượt quá 9.999.999.999.999.999,99.", nameof(entity.PrizePool));
                if (decimal.Round(entity.PrizePool, 2) != entity.PrizePool)
                    throw new ArgumentException("Tổng giải thưởng chỉ được có tối đa 2 chữ số thập phân.", nameof(entity.PrizePool));
            }
            else
            {
                ValidateTournament(entity.Title, entity.PrizePool, entity.StartDate);
            }
            ValidateText(entity.GameTitle, 100, "Tên trò chơi");
            if (!string.IsNullOrWhiteSpace(entity.FormatType))
                ValidateText(entity.FormatType, 50, "Thể thức");

            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string sql = @"UPDATE Tournament 
                                   SET Title = @Title, GameTitle = @GameTitle, FormatType = @FormatType, 
                                       PrizePool = @PrizePool, Status = @Status, StartDate = @StartDate
                                   WHERE TournamentId = @TournamentId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", entity.Title.Trim());
                        cmd.Parameters.AddWithValue("@GameTitle", entity.GameTitle.Trim());
                        cmd.Parameters.AddWithValue("@FormatType", (entity.FormatType ?? "SingleElimination").Trim());
                        var prizeParam = cmd.Parameters.Add("@PrizePool", SqlDbType.Decimal);
                        prizeParam.Precision = 18;
                        prizeParam.Scale = 2;
                        prizeParam.Value = entity.PrizePool;
                        cmd.Parameters.AddWithValue("@Status", (entity.Status ?? "Upcoming").Trim());
                        cmd.Parameters.AddWithValue("@StartDate", entity.StartDate.Date);
                        cmd.Parameters.AddWithValue("@TournamentId", entity.TournamentId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                var idx = _fallbackTournaments.FindIndex(t => t.TournamentId == entity.TournamentId);
                if (idx >= 0)
                {
                    _fallbackTournaments[idx].Title = entity.Title.Trim();
                    _fallbackTournaments[idx].GameTitle = entity.GameTitle.Trim();
                    _fallbackTournaments[idx].FormatType = (entity.FormatType ?? "SingleElimination").Trim();
                    _fallbackTournaments[idx].PrizePool = entity.PrizePool;
                    _fallbackTournaments[idx].Status = (entity.Status ?? "Upcoming").Trim();
                    _fallbackTournaments[idx].StartDate = entity.StartDate;
                    return true;
                }
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string sql = "DELETE FROM TournamentMatch WHERE TournamentId = @TournamentId; DELETE FROM Tournament WHERE TournamentId = @TournamentId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TournamentId", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                _fallbackMatches.RemoveAll(m => m.TournamentId == id);
                return _fallbackTournaments.RemoveAll(t => t.TournamentId == id) > 0;
            }
        }

        public List<TournamentMatch> GetMatchesByTournament(int tournamentId)
        {
            List<TournamentMatch> list = new List<TournamentMatch>();
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string sql = "SELECT MatchId, TournamentId, RoundNumber, Team1Name, Team2Name, WinnerTeamName, Score, AssignedStationIds, Status FROM TournamentMatch WHERE TournamentId = @TournamentId ORDER BY RoundNumber, MatchId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TournamentId", tournamentId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new TournamentMatch
                                {
                                    MatchId = Convert.ToInt32(reader["MatchId"]),
                                    TournamentId = Convert.ToInt32(reader["TournamentId"]),
                                    RoundNumber = Convert.ToInt32(reader["RoundNumber"]),
                                    Team1Name = reader["Team1Name"].ToString() ?? "",
                                    Team2Name = reader["Team2Name"].ToString() ?? "",
                                    WinnerTeamName = reader["WinnerTeamName"]?.ToString(),
                                    Score = reader["Score"]?.ToString(),
                                    AssignedStationIds = reader["AssignedStationIds"]?.ToString(),
                                    Status = reader["Status"].ToString() ?? "Pending"
                                });
                            }
                        }
                    }
                }
                return list;
            }
            catch
            {
                return _fallbackMatches.FindAll(m => m.TournamentId == tournamentId);
            }
        }

        public bool AddMatch(TournamentMatch match)
        {
            ArgumentNullException.ThrowIfNull(match);
            ValidateMatch(match.Team1Name, match.Team2Name, match.RoundNumber);

            if (match.Status == "Finished" || !string.IsNullOrWhiteSpace(match.WinnerTeamName))
            {
                if (!TryParseScore(match.Score, out int s1, out int s2))
                    throw new ArgumentException("Tỉ số không hợp lệ. Vui lòng nhập theo định dạng 'Đội1 - Đội2' (ví dụ: 2-1).");
                ValidateMatchScore(match.Team1Name, match.Team2Name, s1, s2, match.WinnerTeamName ?? "");
                match.Score = FormatScore(s1, s2);
                match.WinnerTeamName = (s1 > s2 ? match.Team1Name : match.Team2Name).Trim();
                match.Status = "Finished";
            }

            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    using var trans = conn.BeginTransaction();
                    RequireTournament(conn, trans, match.TournamentId);

                    string sql = @"INSERT INTO TournamentMatch (TournamentId, RoundNumber, Team1Name, Team2Name, WinnerTeamName, Score, AssignedStationIds, Status)
                                   OUTPUT INSERTED.MatchId
                                   VALUES (@TournamentId, @RoundNumber, @Team1Name, @Team2Name, @WinnerTeamName, @Score, @AssignedStationIds, @Status)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@TournamentId", match.TournamentId);
                        cmd.Parameters.AddWithValue("@RoundNumber", match.RoundNumber);
                        cmd.Parameters.AddWithValue("@Team1Name", match.Team1Name.Trim());
                        cmd.Parameters.AddWithValue("@Team2Name", match.Team2Name.Trim());
                        cmd.Parameters.AddWithValue("@WinnerTeamName", (object?)match.WinnerTeamName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Score", (object?)match.Score ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedStationIds", (object?)match.AssignedStationIds ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", match.Status);

                        object? result = cmd.ExecuteScalar();
                        trans.Commit();
                        if (result != null && result != DBNull.Value)
                        {
                            match.MatchId = Convert.ToInt32(result, CultureInfo.InvariantCulture);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
            {
                var tourney = _fallbackTournaments.Find(t => t.TournamentId == match.TournamentId);
                if (tourney != null && tourney.Status == "Finished")
                    throw new InvalidOperationException("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.");

                match.MatchId = _fallbackMatches.Count > 0 ? _fallbackMatches.Max(m => m.MatchId) + 1 : 1;
                _fallbackMatches.Add(match);
                return true;
            }
        }

        public bool UpdateMatch(TournamentMatch match)
        {
            ArgumentNullException.ThrowIfNull(match);
            ValidateMatch(match.Team1Name, match.Team2Name, match.RoundNumber);

            if (match.Status == "Finished" || !string.IsNullOrWhiteSpace(match.WinnerTeamName))
            {
                if (!TryParseScore(match.Score, out int s1, out int s2))
                    throw new ArgumentException("Tỉ số không hợp lệ. Vui lòng nhập theo định dạng 'Đội1 - Đội2' (ví dụ: 2-1).");
                ValidateMatchScore(match.Team1Name, match.Team2Name, s1, s2, match.WinnerTeamName ?? "");
                match.Score = FormatScore(s1, s2);
                match.WinnerTeamName = (s1 > s2 ? match.Team1Name : match.Team2Name).Trim();
                match.Status = "Finished";
            }
            else if (!string.IsNullOrWhiteSpace(match.Score))
            {
                if (TryParseScore(match.Score, out int s1, out int s2))
                {
                    if (s1 < 0 || s2 < 0)
                        throw new ArgumentException("Tỉ số không được âm.");
                    if (FormatScore(s1, s2).Length > 20)
                        throw new ArgumentException("Tỉ số vượt quá độ dài lưu trữ cho phép (20 ký tự).");
                    match.Score = FormatScore(s1, s2);
                }
            }

            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    using var trans = conn.BeginTransaction();
                    RequireTournament(conn, trans, match.TournamentId);

                    string sql = @"UPDATE TournamentMatch 
                                   SET RoundNumber = @RoundNumber, Team1Name = @Team1Name, Team2Name = @Team2Name,
                                       WinnerTeamName = @WinnerTeamName, Score = @Score, AssignedStationIds = @AssignedStationIds, Status = @Status
                                   WHERE MatchId = @MatchId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@RoundNumber", match.RoundNumber);
                        cmd.Parameters.AddWithValue("@Team1Name", match.Team1Name.Trim());
                        cmd.Parameters.AddWithValue("@Team2Name", match.Team2Name.Trim());
                        cmd.Parameters.AddWithValue("@WinnerTeamName", (object?)match.WinnerTeamName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Score", (object?)match.Score ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedStationIds", (object?)match.AssignedStationIds ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", match.Status);
                        cmd.Parameters.AddWithValue("@MatchId", match.MatchId);

                        int rows = cmd.ExecuteNonQuery();
                        trans.Commit();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
            {
                var tourney = _fallbackTournaments.Find(t => t.TournamentId == match.TournamentId);
                if (tourney != null && tourney.Status == "Finished")
                    throw new InvalidOperationException("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.");

                var idx = _fallbackMatches.FindIndex(m => m.MatchId == match.MatchId);
                if (idx >= 0)
                {
                    _fallbackMatches[idx].RoundNumber = match.RoundNumber;
                    _fallbackMatches[idx].Team1Name = match.Team1Name.Trim();
                    _fallbackMatches[idx].Team2Name = match.Team2Name.Trim();
                    _fallbackMatches[idx].WinnerTeamName = match.WinnerTeamName;
                    _fallbackMatches[idx].Score = match.Score;
                    _fallbackMatches[idx].AssignedStationIds = match.AssignedStationIds;
                    _fallbackMatches[idx].Status = match.Status;
                    return true;
                }
                return false;
            }
        }

        public bool LockTournamentStations(int[] stationIds)
        {
            if (stationIds == null || stationIds.Length == 0) return false;

            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string inClause = string.Join(",", stationIds.Select(id => (int)id));
                    string sql = $"UPDATE ComputerStation SET Status = 'TournamentLocked' WHERE StationId IN ({inClause})";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                DBStation.UpdateMultipleStatusesFallback(stationIds, "TournamentLocked");
                return true;
            }
        }

        public bool UnlockTournamentStations(int[] stationIds)
        {
            if (stationIds == null || stationIds.Length == 0) return false;

            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    string inClause = string.Join(",", stationIds.Select(id => (int)id));
                    string sql = $"UPDATE ComputerStation SET Status = 'Available' WHERE StationId IN ({inClause})";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                DBStation.UpdateMultipleStatusesFallback(stationIds, "Available");
                return true;
            }
        }

        /// <summary>Checks the required tournament fields before creating or updating a tournament.</summary>
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

        /// <summary>Validates team names and round number for a match.</summary>
        public static void ValidateMatch(string team1Name, string team2Name, int roundNumber)
        {
            ValidateText(team1Name, 100, "Tên đội 1");
            ValidateText(team2Name, 100, "Tên đội 2");
            if (string.Equals(team1Name.Trim(), team2Name.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Hai đội thi đấu phải có tên khác nhau.");
            if (roundNumber <= 0)
                throw new ArgumentException("Vòng đấu phải lớn hơn 0.", nameof(roundNumber));
        }

        /// <summary>Validates that text is not null, whitespace, or exceeding maximum length.</summary>
        public static void ValidateText(string value, int maximumLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{fieldName} không được để trống.");
            if (value.Trim().Length > maximumLength)
                throw new ArgumentException($"{fieldName} không được dài quá {maximumLength} ký tự.");
        }

        /// <summary>Formats match score as team1Score-team2Score.</summary>
        public static string FormatScore(int team1Score, int team2Score) =>
            $"{team1Score}-{team2Score}";

        /// <summary>Tries to parse score text in formats like '2-1', '2 - 1', '2:1'. Accurately preserves negative values.</summary>
        public static bool TryParseScore(string? scoreText, out int score1, out int score2)
        {
            score1 = 0;
            score2 = 0;
            if (string.IsNullOrWhiteSpace(scoreText)) return false;

            var match = Regex.Match(
                scoreText.Trim(),
                @"^\s*([+-]?\d+)\s*[-:]\s*([+-]?\d+)\s*$"
            );

            if (match.Success &&
                int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out score1) &&
                int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out score2))
            {
                return true;
            }
            return false;
        }

        /// <summary>Creates a tournament after validating the fields.</summary>
        public int CreateTournament(string title, string gameTitle, decimal prizePool,
            DateTime startDate, string formatType = "SingleElimination")
        {
            var t = new Tournament
            {
                Title = title,
                GameTitle = gameTitle,
                FormatType = formatType,
                PrizePool = prizePool,
                StartDate = startDate,
                Status = "Upcoming"
            };
            Add(t);
            return t.TournamentId;
        }

        /// <summary>Creates a match after validating the teams and round.</summary>
        public int CreateMatch(int tournamentId, string team1Name, string team2Name, int roundNumber)
        {
            var m = new TournamentMatch
            {
                TournamentId = tournamentId,
                RoundNumber = roundNumber,
                Team1Name = team1Name,
                Team2Name = team2Name,
                Status = "Pending",
                Score = "0 - 0"
            };
            AddMatch(m);
            return m.MatchId;
        }

        /// <summary>Saves a validated score and winner, and marks the selected match Finished atomically.</summary>
        public void UpdateMatchScore(int matchId, int team1Score, int team2Score, string winnerTeamName)
        {
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                using var trans = conn.BeginTransaction();

                using var ownerCmd = new SqlCommand("SELECT TournamentId FROM dbo.TournamentMatch WHERE MatchId = @MatchId;", conn, trans);
                ownerCmd.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
                object? tournamentIdObj = ownerCmd.ExecuteScalar();
                if (tournamentIdObj is null)
                    throw new InvalidOperationException("Không tìm thấy trận đấu đã chọn. Vui lòng tải lại danh sách.");
                int tournamentId = Convert.ToInt32(tournamentIdObj, CultureInfo.InvariantCulture);
                RequireTournament(conn, trans, tournamentId);

                string team1Name;
                string team2Name;
                using (var matchCmd = new SqlCommand("""
                    SELECT Team1Name, Team2Name, Status FROM dbo.TournamentMatch WITH (UPDLOCK, HOLDLOCK)
                    WHERE MatchId = @MatchId;
                    """, conn, trans))
                {
                    matchCmd.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
                    using var reader = matchCmd.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Không tìm thấy trận đấu đã chọn. Vui lòng tải lại danh sách.");
                    team1Name = reader.GetString(0);
                    team2Name = reader.GetString(1);
                    if (reader.GetString(2) == "Finished")
                        throw new InvalidOperationException("Trận đấu đã kết thúc và đã có kết quả. Vui lòng tải lại danh sách.");
                }

                ValidateMatchScore(team1Name, team2Name, team1Score, team2Score, winnerTeamName);
                string winner = team1Score > team2Score ? team1Name : team2Name;
                using var updateCmd = new SqlCommand("""
                    UPDATE dbo.TournamentMatch
                    SET Score = @Score, WinnerTeamName = @WinnerTeamName, Status = N'Finished'
                    WHERE MatchId = @MatchId;
                    """, conn, trans);
                updateCmd.Parameters.Add("@Score", SqlDbType.NVarChar, 20).Value = FormatScore(team1Score, team2Score);
                updateCmd.Parameters.Add("@WinnerTeamName", SqlDbType.NVarChar, 100).Value = winner.Trim();
                updateCmd.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
                updateCmd.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
            {
                var existing = _fallbackMatches.Find(m => m.MatchId == matchId);
                if (existing == null)
                    throw new InvalidOperationException("Không tìm thấy trận đấu đã chọn. Vui lòng tải lại danh sách.");

                var tourney = _fallbackTournaments.Find(t => t.TournamentId == existing.TournamentId);
                if (tourney != null && tourney.Status == "Finished")
                    throw new InvalidOperationException("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.");

                if (existing.Status == "Finished")
                    throw new InvalidOperationException("Trận đấu đã kết thúc và đã có kết quả. Vui lòng tải lại danh sách.");

                ValidateMatchScore(existing.Team1Name, existing.Team2Name, team1Score, team2Score, winnerTeamName);
                existing.Score = FormatScore(team1Score, team2Score);
                string winner = team1Score > team2Score ? existing.Team1Name : existing.Team2Name;
                existing.WinnerTeamName = winner.Trim();
                existing.Status = "Finished";
            }
        }

        /// <summary>Checks and reserves exactly five unoccupied stations for a pending match in one transaction.</summary>
        public void LockTournament(int tournamentId, int matchId, IReadOnlyCollection<int> stationIds)
        {
            ArgumentNullException.ThrowIfNull(stationIds);
            int[] ids = stationIds.OrderBy(id => id).ToArray();
            if (ids.Length != 5 || ids.Distinct().Count() != 5 || ids.Any(id => id <= 0))
                throw new ArgumentException("Vui lòng chọn đúng 5 máy thi đấu khác nhau.", nameof(stationIds));

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                using var trans = conn.BeginTransaction();
                RequireTournament(conn, trans, tournamentId);

                using (var matchCmd = new SqlCommand("""
                    SELECT Status, AssignedStationIds FROM dbo.TournamentMatch WITH (UPDLOCK, HOLDLOCK)
                    WHERE MatchId = @MatchId AND TournamentId = @TournamentId;
                    """, conn, trans))
                {
                    matchCmd.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
                    matchCmd.Parameters.Add("@TournamentId", SqlDbType.Int).Value = tournamentId;
                    using var reader = matchCmd.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Trận đấu không thuộc giải đấu đã chọn hoặc không còn tồn tại.");
                    if (reader.GetString(0) == "Finished")
                        throw new InvalidOperationException("Trận đấu đã kết thúc, không thể gán máy.");
                    if (!reader.IsDBNull(1) && !string.IsNullOrWhiteSpace(reader.GetString(1)))
                        throw new InvalidOperationException("Trận đấu đã được xếp cụm máy thi đấu từ trước.");
                }

                string parameterNames = string.Join(", ", Enumerable.Range(0, ids.Length).Select(i => $"@Station{i}"));
                using (var stationCmd = new SqlCommand($"""
                    SELECT s.StationId, s.StationName, s.Status,
                           CAST(CASE WHEN s.Status = N'InUse' OR EXISTS (
                               SELECT 1 FROM dbo.GamingSession AS gs
                               WHERE gs.StationId = s.StationId
                                 AND (gs.Status = N'Active' OR gs.EndTime IS NULL)
                           ) THEN 1 ELSE 0 END AS bit) AS IsOccupied
                    FROM dbo.ComputerStation AS s WITH (UPDLOCK, HOLDLOCK)
                    WHERE s.StationId IN ({parameterNames}) ORDER BY s.StationId;
                    """, conn, trans))
                {
                    for (int i = 0; i < ids.Length; i++)
                        stationCmd.Parameters.Add($"@Station{i}", SqlDbType.Int).Value = ids[i];

                    var occupied = new List<string>();
                    var unavailable = new List<string>();
                    int count = 0;
                    using var reader = stationCmd.ExecuteReader();
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

                using (var lockCmd = new SqlCommand($"""
                    UPDATE dbo.ComputerStation SET Status = N'TournamentLocked'
                    WHERE StationId IN ({parameterNames});
                    """, conn, trans))
                {
                    for (int i = 0; i < ids.Length; i++)
                        lockCmd.Parameters.Add($"@Station{i}", SqlDbType.Int).Value = ids[i];

                    if (lockCmd.ExecuteNonQuery() != 5)
                        throw new InvalidOperationException("Không thể khóa đủ 5 máy. Chưa có thay đổi nào được lưu.");
                }

                using (var assignCmd = new SqlCommand("""
                    UPDATE dbo.TournamentMatch SET AssignedStationIds = @StationIds WHERE MatchId = @MatchId;
                    UPDATE dbo.Tournament SET Status = N'Ongoing' WHERE TournamentId = @TournamentId;
                    """, conn, trans))
                {
                    assignCmd.Parameters.Add("@StationIds", SqlDbType.NVarChar, 100).Value = string.Join(",", ids);
                    assignCmd.Parameters.Add("@MatchId", SqlDbType.Int).Value = matchId;
                    assignCmd.Parameters.Add("@TournamentId", SqlDbType.Int).Value = tournamentId;
                    assignCmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
            {
                var tourney = _fallbackTournaments.Find(t => t.TournamentId == tournamentId);
                if (tourney is null)
                    throw new InvalidOperationException("Không tìm thấy giải đấu đã chọn. Vui lòng tải lại danh sách.");
                if (tourney.Status == "Finished")
                    throw new InvalidOperationException("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.");

                var match = _fallbackMatches.Find(m => m.MatchId == matchId && m.TournamentId == tournamentId);
                if (match is null)
                    throw new InvalidOperationException("Trận đấu không thuộc giải đấu đã chọn hoặc không còn tồn tại.");
                if (match.Status == "Finished")
                    throw new InvalidOperationException("Trận đấu đã kết thúc, không thể gán máy.");
                if (!string.IsNullOrWhiteSpace(match.AssignedStationIds))
                    throw new InvalidOperationException("Trận đấu đã được xếp cụm máy thi đấu từ trước.");

                DBStation.UpdateMultipleStatusesFallback(ids, "TournamentLocked");
                match.AssignedStationIds = string.Join(",", ids);
                tourney.Status = "Ongoing";
            }
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
            try
            {
                using var conn = _connectionFactory();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddRange(parameters);
                using var adapter = new SqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
            catch
            {
                var table = new DataTable();
                return table;
            }
        }
    }
}
