using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem.DAL
{
    public class DBBilling
    {
        private static readonly List<TariffPlan> _fallbackPlans = new()
        {
            new TariffPlan { PlanId = 1, PlanName = "Cước Theo Giờ Thường", TariffType = "Hourly", FixedPrice = null, DiscountRate = 0, StartHour = null, EndHour = null },
            new TariffPlan { PlanId = 2, PlanName = "Gói Combo Đêm Trọn Gói", TariffType = "NightCombo", FixedPrice = 45000, DiscountRate = 0, StartHour = 22, EndHour = 6 },
            new TariffPlan { PlanId = 3, PlanName = "Gói Ưu Đãi Giải Đấu LAN", TariffType = "Tournament", FixedPrice = null, DiscountRate = 0.5, StartHour = null, EndHour = null },
            new TariffPlan { PlanId = 4, PlanName = "Ưu Đãi Hội Viên Vàng", TariffType = "MemberTier", FixedPrice = null, DiscountRate = 0.15, StartHour = null, EndHour = null }
        };

        private static readonly List<GamingSession> _fallbackSessions = new()
        {
            new GamingSession { SessionId = 1, StationId = 1, UserId = 2, StartTime = DateTime.Today.AddHours(8), EndTime = DateTime.Today.AddHours(11), TariffType = "Hourly", PlayCost = 24000, ServiceCost = 15000, TotalCost = 39000, Status = "Completed" },
            new GamingSession { SessionId = 2, StationId = 4, UserId = 3, StartTime = DateTime.Today.AddHours(9), EndTime = DateTime.Today.AddHours(12), TariffType = "Hourly", PlayCost = 36000, ServiceCost = 30000, TotalCost = 66000, Status = "Completed" },
            new GamingSession { SessionId = 3, StationId = 6, UserId = 4, StartTime = DateTime.Today.AddHours(22).AddDays(-1), EndTime = DateTime.Today.AddHours(6), TariffType = "NightCombo", PlayCost = 45000, ServiceCost = 25000, TotalCost = 70000, Status = "Completed" },
            new GamingSession { SessionId = 4, StationId = 2, UserId = 2, StartTime = DateTime.Now.AddHours(-1), EndTime = null, TariffType = "Hourly", PlayCost = 8000, ServiceCost = 10000, TotalCost = 18000, Status = "Active" }
        };

        public List<TariffPlan> GetAllPlans()
        {
            List<TariffPlan> list = new List<TariffPlan>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT PlanId, PlanName, TariffType, FixedPrice, DiscountRate, StartHour, EndHour FROM TariffPlan ORDER BY PlanId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TariffPlan
                            {
                                PlanId = Convert.ToInt32(reader["PlanId"]),
                                PlanName = reader["PlanName"].ToString() ?? "",
                                TariffType = reader["TariffType"].ToString() ?? "Hourly",
                                FixedPrice = reader["FixedPrice"] == DBNull.Value ? null : Convert.ToDecimal(reader["FixedPrice"]),
                                DiscountRate = Convert.ToDouble(reader["DiscountRate"]),
                                StartHour = reader["StartHour"] == DBNull.Value ? null : Convert.ToInt32(reader["StartHour"]),
                                EndHour = reader["EndHour"] == DBNull.Value ? null : Convert.ToInt32(reader["EndHour"])
                            });
                        }
                    }
                }
                return list;
            }
            catch
            {
                return new List<TariffPlan>(_fallbackPlans);
            }
        }

        public bool AddPlan(TariffPlan plan)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO TariffPlan (PlanName, TariffType, FixedPrice, DiscountRate, StartHour, EndHour)
                                   VALUES (@PlanName, @TariffType, @FixedPrice, @DiscountRate, @StartHour, @EndHour)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PlanName", plan.PlanName);
                        cmd.Parameters.AddWithValue("@TariffType", plan.TariffType);
                        cmd.Parameters.AddWithValue("@FixedPrice", (object?)plan.FixedPrice ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DiscountRate", plan.DiscountRate);
                        cmd.Parameters.AddWithValue("@StartHour", (object?)plan.StartHour ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndHour", (object?)plan.EndHour ?? DBNull.Value);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                plan.PlanId = _fallbackPlans.Count > 0 ? _fallbackPlans[^1].PlanId + 1 : 1;
                _fallbackPlans.Add(plan);
                return true;
            }
        }

        public bool UpdatePlan(TariffPlan plan)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE TariffPlan 
                                   SET PlanName = @PlanName, TariffType = @TariffType, FixedPrice = @FixedPrice,
                                       DiscountRate = @DiscountRate, StartHour = @StartHour, EndHour = @EndHour
                                   WHERE PlanId = @PlanId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PlanName", plan.PlanName);
                        cmd.Parameters.AddWithValue("@TariffType", plan.TariffType);
                        cmd.Parameters.AddWithValue("@FixedPrice", (object?)plan.FixedPrice ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DiscountRate", plan.DiscountRate);
                        cmd.Parameters.AddWithValue("@StartHour", (object?)plan.StartHour ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndHour", (object?)plan.EndHour ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PlanId", plan.PlanId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var idx = _fallbackPlans.FindIndex(p => p.PlanId == plan.PlanId);
                if (idx >= 0)
                {
                    _fallbackPlans[idx].PlanName = plan.PlanName;
                    _fallbackPlans[idx].TariffType = plan.TariffType;
                    _fallbackPlans[idx].FixedPrice = plan.FixedPrice;
                    _fallbackPlans[idx].DiscountRate = plan.DiscountRate;
                    _fallbackPlans[idx].StartHour = plan.StartHour;
                    _fallbackPlans[idx].EndHour = plan.EndHour;
                    return true;
                }
                return false;
            }
        }

        public List<GamingSession> GetAllSessions()
        {
            List<GamingSession> list = new List<GamingSession>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status 
                                   FROM GamingSession ORDER BY SessionId DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new GamingSession
                            {
                                SessionId = Convert.ToInt32(reader["SessionId"]),
                                StationId = Convert.ToInt32(reader["StationId"]),
                                UserId = reader["UserId"] == DBNull.Value ? null : Convert.ToInt32(reader["UserId"]),
                                StartTime = Convert.ToDateTime(reader["StartTime"]),
                                EndTime = reader["EndTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["EndTime"]),
                                TariffType = reader["TariffType"].ToString() ?? "Hourly",
                                PlayCost = Convert.ToDecimal(reader["PlayCost"]),
                                ServiceCost = Convert.ToDecimal(reader["ServiceCost"]),
                                TotalCost = Convert.ToDecimal(reader["TotalCost"]),
                                Status = reader["Status"].ToString() ?? "Active"
                            });
                        }
                    }
                }
                return list;
            }
            catch
            {
                return new List<GamingSession>(_fallbackSessions);
            }
        }

        public List<GamingSession> GetSessionsByFilter(DateTime fromDate, DateTime toDate, string? tariffType, string? status)
        {
            var all = GetAllSessions();
            var filtered = all.FindAll(s => s.StartTime >= fromDate.Date && s.StartTime <= toDate.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrEmpty(tariffType) && tariffType != "Tất cả")
            {
                filtered = filtered.FindAll(s => s.TariffType.Equals(tariffType, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                filtered = filtered.FindAll(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            return filtered;
        }

        public bool AddSession(GamingSession session)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO GamingSession (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status)
                                   VALUES (@StationId, @UserId, @StartTime, @EndTime, @TariffType, @PlayCost, @ServiceCost, @TotalCost, @Status)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@StationId", session.StationId);
                        cmd.Parameters.AddWithValue("@UserId", (object?)session.UserId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartTime", session.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", (object?)session.EndTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TariffType", session.TariffType);
                        cmd.Parameters.AddWithValue("@PlayCost", session.PlayCost);
                        cmd.Parameters.AddWithValue("@ServiceCost", session.ServiceCost);
                        cmd.Parameters.AddWithValue("@TotalCost", session.TotalCost);
                        cmd.Parameters.AddWithValue("@Status", session.Status);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                session.SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions[0].SessionId + 1 : 1;
                _fallbackSessions.Insert(0, session);
                return true;
            }
        }

        public GamingSession? GetActiveSession(int stationId)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT TOP 1 SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status 
                                   FROM GamingSession 
                                   WHERE StationId = @StationId AND Status = 'Active' 
                                   ORDER BY SessionId DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@StationId", stationId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new GamingSession
                                {
                                    SessionId = Convert.ToInt32(reader["SessionId"]),
                                    StationId = Convert.ToInt32(reader["StationId"]),
                                    UserId = reader["UserId"] == DBNull.Value ? null : Convert.ToInt32(reader["UserId"]),
                                    StartTime = Convert.ToDateTime(reader["StartTime"]),
                                    EndTime = reader["EndTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["EndTime"]),
                                    TariffType = reader["TariffType"].ToString() ?? "Hourly",
                                    PlayCost = Convert.ToDecimal(reader["PlayCost"]),
                                    ServiceCost = Convert.ToDecimal(reader["ServiceCost"]),
                                    TotalCost = Convert.ToDecimal(reader["TotalCost"]),
                                    Status = reader["Status"].ToString() ?? "Active"
                                };
                            }
                        }
                    }
                }
            }
            catch
            {
                return _fallbackSessions.Find(s => s.StationId == stationId && s.Status == "Active");
            }
            return null;
        }

        public List<GamingSession> GetActiveSessions()
        {
            List<GamingSession> list = new();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status 
                                   FROM GamingSession 
                                   WHERE Status = 'Active' 
                                   ORDER BY SessionId DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new GamingSession
                            {
                                SessionId = Convert.ToInt32(reader["SessionId"]),
                                StationId = Convert.ToInt32(reader["StationId"]),
                                UserId = reader["UserId"] == DBNull.Value ? null : Convert.ToInt32(reader["UserId"]),
                                StartTime = Convert.ToDateTime(reader["StartTime"]),
                                EndTime = reader["EndTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["EndTime"]),
                                TariffType = reader["TariffType"].ToString() ?? "Hourly",
                                PlayCost = Convert.ToDecimal(reader["PlayCost"]),
                                ServiceCost = Convert.ToDecimal(reader["ServiceCost"]),
                                TotalCost = Convert.ToDecimal(reader["TotalCost"]),
                                Status = reader["Status"].ToString() ?? "Active"
                            });
                        }
                    }
                }
                return list;
            }
            catch
            {
                return _fallbackSessions.FindAll(s => s.Status == "Active");
            }
        }

        public bool StartSession(GamingSession session)
        {
            session.Status = "Active";
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO GamingSession (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status)
                                   VALUES (@StationId, @UserId, @StartTime, NULL, @TariffType, 0, 0, 0, 'Active');
                                   SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@StationId", session.StationId);
                        cmd.Parameters.AddWithValue("@UserId", (object?)session.UserId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartTime", session.StartTime);
                        cmd.Parameters.AddWithValue("@TariffType", session.TariffType);

                        var newId = cmd.ExecuteScalar();
                        if (newId != null && int.TryParse(newId.ToString(), out int id))
                        {
                            session.SessionId = id;
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch
            {
                session.SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions[0].SessionId + 1 : 1;
                _fallbackSessions.Insert(0, session);
                return true;
            }
        }

        public bool CompleteSession(int sessionId, DateTime endTime, decimal playCost, decimal serviceCost, decimal totalCost)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE GamingSession 
                                   SET EndTime = @EndTime, PlayCost = @PlayCost, ServiceCost = @ServiceCost, TotalCost = @TotalCost, Status = 'Completed'
                                   WHERE SessionId = @SessionId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        cmd.Parameters.AddWithValue("@PlayCost", playCost);
                        cmd.Parameters.AddWithValue("@ServiceCost", serviceCost);
                        cmd.Parameters.AddWithValue("@TotalCost", totalCost);
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var session = _fallbackSessions.Find(s => s.SessionId == sessionId);
                if (session != null)
                {
                    session.EndTime = endTime;
                    session.PlayCost = playCost;
                    session.ServiceCost = serviceCost;
                    session.TotalCost = totalCost;
                    session.Status = "Completed";
                    return true;
                }
                return false;
            }
        }
    }
}
