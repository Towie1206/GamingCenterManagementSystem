using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    public class DBTournament : IRepository<Tournament>
    {
        public const decimal MaximumPrizePool = 9999999999999999.99m;

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

        public List<Tournament> GetAll()
        {
            List<Tournament> list = new List<Tournament>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
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
                using (SqlConnection conn = Database.GetConnection())
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

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Tournament (Title, GameTitle, FormatType, PrizePool, Status, StartDate)
                                   VALUES (@Title, @GameTitle, @FormatType, @PrizePool, @Status, @StartDate)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", entity.Title.Trim());
                        cmd.Parameters.AddWithValue("@GameTitle", entity.GameTitle.Trim());
                        cmd.Parameters.AddWithValue("@FormatType", (entity.FormatType ?? "SingleElimination").Trim());
                        cmd.Parameters.AddWithValue("@PrizePool", entity.PrizePool);
                        cmd.Parameters.AddWithValue("@Status", (entity.Status ?? "Upcoming").Trim());
                        cmd.Parameters.AddWithValue("@StartDate", entity.StartDate);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                entity.TournamentId = _fallbackTournaments.Count > 0 ? _fallbackTournaments[^1].TournamentId + 1 : 1;
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

            try
            {
                using (SqlConnection conn = Database.GetConnection())
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
                        cmd.Parameters.AddWithValue("@PrizePool", entity.PrizePool);
                        cmd.Parameters.AddWithValue("@Status", (entity.Status ?? "Upcoming").Trim());
                        cmd.Parameters.AddWithValue("@StartDate", entity.StartDate);
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
                using (SqlConnection conn = Database.GetConnection())
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
                using (SqlConnection conn = Database.GetConnection())
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

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO TournamentMatch (TournamentId, RoundNumber, Team1Name, Team2Name, WinnerTeamName, Score, AssignedStationIds, Status)
                                   VALUES (@TournamentId, @RoundNumber, @Team1Name, @Team2Name, @WinnerTeamName, @Score, @AssignedStationIds, @Status)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TournamentId", match.TournamentId);
                        cmd.Parameters.AddWithValue("@RoundNumber", match.RoundNumber);
                        cmd.Parameters.AddWithValue("@Team1Name", match.Team1Name.Trim());
                        cmd.Parameters.AddWithValue("@Team2Name", match.Team2Name.Trim());
                        cmd.Parameters.AddWithValue("@WinnerTeamName", (object?)match.WinnerTeamName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Score", (object?)match.Score ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedStationIds", (object?)match.AssignedStationIds ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", match.Status);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                match.MatchId = _fallbackMatches.Count > 0 ? _fallbackMatches[^1].MatchId + 1 : 1;
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
            }

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE TournamentMatch 
                                   SET RoundNumber = @RoundNumber, Team1Name = @Team1Name, Team2Name = @Team2Name,
                                       WinnerTeamName = @WinnerTeamName, Score = @Score, AssignedStationIds = @AssignedStationIds, Status = @Status
                                   WHERE MatchId = @MatchId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoundNumber", match.RoundNumber);
                        cmd.Parameters.AddWithValue("@Team1Name", match.Team1Name.Trim());
                        cmd.Parameters.AddWithValue("@Team2Name", match.Team2Name.Trim());
                        cmd.Parameters.AddWithValue("@WinnerTeamName", (object?)match.WinnerTeamName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Score", (object?)match.Score ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedStationIds", (object?)match.AssignedStationIds ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", match.Status);
                        cmd.Parameters.AddWithValue("@MatchId", match.MatchId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
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
                using (SqlConnection conn = Database.GetConnection())
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
                using (SqlConnection conn = Database.GetConnection())
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

        /// <summary>Tries to parse score text in formats like '2-1', '2 - 1', '2:1'.</summary>
        public static bool TryParseScore(string? scoreText, out int score1, out int score2)
        {
            score1 = 0;
            score2 = 0;
            if (string.IsNullOrWhiteSpace(scoreText)) return false;

            string[] parts = scoreText.Split(new[] { '-', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out score1) && int.TryParse(parts[1].Trim(), out score2))
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

        /// <summary>Updates match score and sets status to Finished.</summary>
        public void UpdateMatchScore(int matchId, int team1Score, int team2Score, string winnerTeamName)
        {
            TournamentMatch? existing = null;
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT MatchId, TournamentId, RoundNumber, Team1Name, Team2Name, WinnerTeamName, Score, AssignedStationIds, Status FROM TournamentMatch WHERE MatchId = @MatchId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MatchId", matchId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existing = new TournamentMatch
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
                                };
                            }
                        }
                    }
                }
            }
            catch
            {
                existing = _fallbackMatches.Find(m => m.MatchId == matchId);
            }

            if (existing == null)
                throw new InvalidOperationException("Không tìm thấy trận đấu đã chọn.");

            ValidateMatchScore(existing.Team1Name, existing.Team2Name, team1Score, team2Score, winnerTeamName);
            existing.Score = FormatScore(team1Score, team2Score);
            string winner = team1Score > team2Score ? existing.Team1Name : existing.Team2Name;
            existing.WinnerTeamName = winner;
            existing.Status = "Finished";
            UpdateMatch(existing);
        }

        /// <summary>Checks and reserves station IDs for tournament.</summary>
        public void LockTournament(int tournamentId, int matchId, IReadOnlyCollection<int> stationIds)
        {
            ArgumentNullException.ThrowIfNull(stationIds);
            int[] ids = stationIds.OrderBy(id => id).ToArray();
            if (ids.Length != 5 || ids.Distinct().Count() != 5 || ids.Any(id => id <= 0))
                throw new ArgumentException("Vui lòng chọn đúng 5 máy thi đấu khác nhau.", nameof(stationIds));

            LockTournamentStations(ids);
        }
    }
}
