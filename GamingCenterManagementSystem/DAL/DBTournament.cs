using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    public class DBTournament : IRepository<Tournament>
    {
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
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Tournament (Title, GameTitle, FormatType, PrizePool, Status, StartDate)
                                   VALUES (@Title, @GameTitle, @FormatType, @PrizePool, @Status, @StartDate)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", entity.Title);
                        cmd.Parameters.AddWithValue("@GameTitle", entity.GameTitle);
                        cmd.Parameters.AddWithValue("@FormatType", entity.FormatType);
                        cmd.Parameters.AddWithValue("@PrizePool", entity.PrizePool);
                        cmd.Parameters.AddWithValue("@Status", entity.Status);
                        cmd.Parameters.AddWithValue("@StartDate", entity.StartDate);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                entity.TournamentId = _fallbackTournaments.Count > 0 ? _fallbackTournaments[^1].TournamentId + 1 : 1;
                _fallbackTournaments.Add(entity);
                return true;
            }
        }

        public bool Update(Tournament entity)
        {
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
                        cmd.Parameters.AddWithValue("@Title", entity.Title);
                        cmd.Parameters.AddWithValue("@GameTitle", entity.GameTitle);
                        cmd.Parameters.AddWithValue("@FormatType", entity.FormatType);
                        cmd.Parameters.AddWithValue("@PrizePool", entity.PrizePool);
                        cmd.Parameters.AddWithValue("@Status", entity.Status);
                        cmd.Parameters.AddWithValue("@StartDate", entity.StartDate);
                        cmd.Parameters.AddWithValue("@TournamentId", entity.TournamentId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var idx = _fallbackTournaments.FindIndex(t => t.TournamentId == entity.TournamentId);
                if (idx >= 0)
                {
                    _fallbackTournaments[idx].Title = entity.Title;
                    _fallbackTournaments[idx].GameTitle = entity.GameTitle;
                    _fallbackTournaments[idx].FormatType = entity.FormatType;
                    _fallbackTournaments[idx].PrizePool = entity.PrizePool;
                    _fallbackTournaments[idx].Status = entity.Status;
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
                        cmd.Parameters.AddWithValue("@Team1Name", match.Team1Name);
                        cmd.Parameters.AddWithValue("@Team2Name", match.Team2Name);
                        cmd.Parameters.AddWithValue("@WinnerTeamName", (object?)match.WinnerTeamName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Score", (object?)match.Score ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedStationIds", (object?)match.AssignedStationIds ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", match.Status);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                match.MatchId = _fallbackMatches.Count > 0 ? _fallbackMatches[^1].MatchId + 1 : 1;
                _fallbackMatches.Add(match);
                return true;
            }
        }

        public bool UpdateMatch(TournamentMatch match)
        {
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
                        cmd.Parameters.AddWithValue("@Team1Name", match.Team1Name);
                        cmd.Parameters.AddWithValue("@Team2Name", match.Team2Name);
                        cmd.Parameters.AddWithValue("@WinnerTeamName", (object?)match.WinnerTeamName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Score", (object?)match.Score ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignedStationIds", (object?)match.AssignedStationIds ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", match.Status);
                        cmd.Parameters.AddWithValue("@MatchId", match.MatchId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var idx = _fallbackMatches.FindIndex(m => m.MatchId == match.MatchId);
                if (idx >= 0)
                {
                    _fallbackMatches[idx].RoundNumber = match.RoundNumber;
                    _fallbackMatches[idx].Team1Name = match.Team1Name;
                    _fallbackMatches[idx].Team2Name = match.Team2Name;
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
    }
}
