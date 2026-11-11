using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    public sealed class StationRevenueSummary
    {
        public int StationId { get; set; }
        public int SessionCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public sealed class SessionCloseResult
    {
        public int SessionId { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string TariffType { get; set; } = "Hourly";
        public string TariffDisplayName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal PlayCost { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal WalletDeducted { get; set; }
        public decimal CashDue { get; set; }
        public decimal? RemainingBalance { get; set; }
    }

    public class DBBilling
    {
        private readonly Func<SqlConnection> _connectionFactory;

        public DBBilling() : this(Database.GetConnection)
        {
        }

        public DBBilling(Func<SqlConnection> connectionFactory)
        {
            ArgumentNullException.ThrowIfNull(connectionFactory);
            _connectionFactory = connectionFactory;
        }

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
            bool opened = false;
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    opened = true;
                    const string sql = "SELECT PlanId, PlanName, TariffType, FixedPrice, DiscountRate, StartHour, EndHour FROM TariffPlan ORDER BY PlanId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TariffPlan
                            {
                                PlanId = reader.GetInt32(0),
                                PlanName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TariffType = reader.IsDBNull(2) ? "Hourly" : reader.GetString(2),
                                FixedPrice = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                                DiscountRate = reader.IsDBNull(4) ? 0d : reader.GetDouble(4),
                                StartHour = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                EndHour = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                            });
                        }
                    }
                }
                return list;
            }
            catch when (!opened)
            {
                return new List<TariffPlan>(_fallbackPlans);
            }
        }

        public bool AddPlan(TariffPlan plan)
        {
            ArgumentNullException.ThrowIfNull(plan);
            bool opened = false;
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    opened = true;
                    const string sql = @"INSERT INTO TariffPlan (PlanName, TariffType, FixedPrice, DiscountRate, StartHour, EndHour)
                                         VALUES (@PlanName, @TariffType, @FixedPrice, @DiscountRate, @StartHour, @EndHour)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@PlanName", SqlDbType.NVarChar, 100).Value = plan.PlanName;
                        cmd.Parameters.Add("@TariffType", SqlDbType.NVarChar, 50).Value = plan.TariffType;
                        cmd.Parameters.Add(new SqlParameter("@FixedPrice", SqlDbType.Decimal)
                        {
                            Precision = 18,
                            Scale = 2,
                            Value = (object?)plan.FixedPrice ?? DBNull.Value
                        });
                        cmd.Parameters.Add("@DiscountRate", SqlDbType.Float).Value = plan.DiscountRate;
                        cmd.Parameters.Add("@StartHour", SqlDbType.Int).Value = (object?)plan.StartHour ?? DBNull.Value;
                        cmd.Parameters.Add("@EndHour", SqlDbType.Int).Value = (object?)plan.EndHour ?? DBNull.Value;

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch when (!opened)
            {
                plan.PlanId = _fallbackPlans.Count > 0 ? _fallbackPlans[^1].PlanId + 1 : 1;
                _fallbackPlans.Add(plan);
                return true;
            }
        }

        public bool UpdatePlan(TariffPlan plan)
        {
            ArgumentNullException.ThrowIfNull(plan);
            bool opened = false;
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    opened = true;
                    const string sql = @"UPDATE TariffPlan
                                         SET PlanName = @PlanName, TariffType = @TariffType, FixedPrice = @FixedPrice,
                                             DiscountRate = @DiscountRate, StartHour = @StartHour, EndHour = @EndHour
                                         WHERE PlanId = @PlanId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@PlanName", SqlDbType.NVarChar, 100).Value = plan.PlanName;
                        cmd.Parameters.Add("@TariffType", SqlDbType.NVarChar, 50).Value = plan.TariffType;
                        cmd.Parameters.Add(new SqlParameter("@FixedPrice", SqlDbType.Decimal)
                        {
                            Precision = 18,
                            Scale = 2,
                            Value = (object?)plan.FixedPrice ?? DBNull.Value
                        });
                        cmd.Parameters.Add("@DiscountRate", SqlDbType.Float).Value = plan.DiscountRate;
                        cmd.Parameters.Add("@StartHour", SqlDbType.Int).Value = (object?)plan.StartHour ?? DBNull.Value;
                        cmd.Parameters.Add("@EndHour", SqlDbType.Int).Value = (object?)plan.EndHour ?? DBNull.Value;
                        cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = plan.PlanId;

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch when (!opened)
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
            bool opened = false;
            try
            {
                using (SqlConnection conn = _connectionFactory())
                {
                    conn.Open();
                    opened = true;
                    const string sql = @"SELECT SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status
                                         FROM GamingSession ORDER BY SessionId DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(MapSession(reader));
                        }
                    }
                }
                return list;
            }
            catch when (!opened)
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


        // Khôi - Logic 1: LINQ filters completed play sessions by their END date.
        // The last day is inclusive even when EndTime contains hours/minutes/seconds.
        public static List<GamingSession> FilterCompletedRevenueSessions(
            IEnumerable<GamingSession> sessions, DateTime fromDate, DateTime toDate,
            string? tariffType = null)
        {
            if (sessions == null) throw new ArgumentNullException(nameof(sessions));
            if (fromDate.Date > toDate.Date)
                throw new ArgumentException("Từ ngày không được sau Đến ngày.");

            DateTime from = fromDate.Date;
            DateTime toExclusive = toDate.Date.AddDays(1);

            return sessions.Where(s =>
                    s.EndTime.HasValue
                    && s.EndTime.Value >= from
                    && s.EndTime.Value < toExclusive
                    && string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrWhiteSpace(tariffType)
                        || tariffType == "Tất cả"
                        || string.Equals(s.TariffType, tariffType, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(s => s.EndTime)
                .ThenBy(s => s.SessionId)
                .ToList();
        }

        // Khôi - Logic 2: GroupBy(StationId) and rank stations by revenue.
        public static List<StationRevenueSummary> GroupRevenueByStation(
            IEnumerable<GamingSession> completedSessions)
        {
            if (completedSessions == null)
                throw new ArgumentNullException(nameof(completedSessions));

            return completedSessions
                .Where(s => s.EndTime.HasValue &&
                            string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .GroupBy(s => s.StationId)
                .Select(group => new StationRevenueSummary
                {
                    StationId = group.Key,
                    SessionCount = group.Count(),
                    Revenue = group.Sum(s => s.TotalCost)
                })
                .OrderByDescending(item => item.Revenue)
                .ThenBy(item => item.StationId)
                .ToList();
        }

        // Khôi - Logic 3: Build the TXT report from the SAME filtered rows as the grid.
        // The Form handles SaveFileDialog and File.WriteAllText.
        public static string BuildRevenueShiftReport(
            IEnumerable<GamingSession> filteredSessions, DateTime fromDate, DateTime toDate)
        {
            if (filteredSessions == null)
                throw new ArgumentNullException(nameof(filteredSessions));

            List<GamingSession> sessions = FilterCompletedRevenueSessions(
                filteredSessions, fromDate, toDate);
            List<StationRevenueSummary> byStation = GroupRevenueByStation(sessions);
            var vn = CultureInfo.GetCultureInfo("vi-VN");
            var report = new StringBuilder();

            report.AppendLine("BÁO CÁO DOANH THU CA TRỰC - CYBER GAME");
            report.AppendLine($"Từ ngày: {fromDate:dd/MM/yyyy} | Đến ngày: {toDate:dd/MM/yyyy}");
            report.AppendLine($"Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            report.AppendLine(new string('-', 100));
            report.AppendLine("Mã phiên\tMáy\tHội viên\tBắt đầu\tKết thúc\tGói cước\tTiền giờ\tDịch vụ\tThành tiền (VNĐ)");

            foreach (GamingSession s in sessions)
            {
                report.AppendLine(string.Join("\t", new[]
                {
                    s.SessionId.ToString(),
                    $"PC-{s.StationId:D2}",
                    s.UserId?.ToString() ?? "Khách vãng lai",
                    s.StartTime.ToString("dd/MM/yyyy HH:mm", vn),
                    s.EndTime!.Value.ToString("dd/MM/yyyy HH:mm", vn),
                    s.TariffType,
                    s.PlayCost.ToString("N0", vn),
                    s.ServiceCost.ToString("N0", vn),
                    s.TotalCost.ToString("N0", vn)
                }));
            }

            report.AppendLine(new string('-', 100));
            report.AppendLine($"Tổng số phiên hoàn thành: {sessions.Count}");
            report.AppendLine($"Tổng tiền giờ: {sessions.Sum(s => s.PlayCost).ToString("N0", vn)} VNĐ");
            report.AppendLine($"Tổng tiền dịch vụ: {sessions.Sum(s => s.ServiceCost).ToString("N0", vn)} VNĐ");
            report.AppendLine($"TỔNG DOANH THU: {sessions.Sum(s => s.TotalCost).ToString("N0", vn)} VNĐ");
            report.AppendLine();
            report.AppendLine("THỐNG KÊ DOANH THU THEO MÁY (CAO NHẤT TRƯỚC)");
            report.AppendLine("Máy\tSố phiên\tDoanh thu (VNĐ)");
            foreach (StationRevenueSummary item in byStation)
                report.AppendLine($"PC-{item.StationId:D2}\t{item.SessionCount}\t{item.Revenue.ToString("N0", vn)}");

            if (byStation.Count > 0)
                report.AppendLine($"MÁY DOANH THU CAO NHẤT: PC-{byStation[0].StationId:D2} - {byStation[0].Revenue.ToString("N0", vn)} VNĐ");
            else
                report.AppendLine("Không có phiên chơi hoàn thành trong khoảng ngày đã chọn.");

            return report.ToString();
        }

        public static TariffCalculatorBase CreateTariffCalculator(string? tariffType, string? memberTier = null)
        {
            return tariffType switch
            {
                "NightCombo" => new NightComboTariff(),
                "Tournament" => new TournamentTariff(),
                "MemberTier" => new MemberTierTariff(string.IsNullOrWhiteSpace(memberTier) ? "Standard" : memberTier),
                _ => new HourlyTariff()
            };
        }

        private static SqlParameter CreateDecimalParameter(string name, decimal value)
        {
            return new SqlParameter(name, SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = value
            };
        }

        private static GamingSession MapSession(SqlDataReader reader)
        {
            return new GamingSession
            {
                SessionId = reader.GetInt32(0),
                StationId = reader.GetInt32(1),
                UserId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                StartTime = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3),
                EndTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                TariffType = reader.IsDBNull(5) ? "Hourly" : reader.GetString(5),
                PlayCost = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                ServiceCost = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                TotalCost = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                Status = reader.IsDBNull(9) ? "Active" : reader.GetString(9)
            };
        }

        public bool AddSession(GamingSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"INSERT INTO dbo.GamingSession (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status)
                                     OUTPUT INSERTED.SessionId
                                     VALUES (@StationId, @UserId, @StartTime, @EndTime, @TariffType, @PlayCost, @ServiceCost, @TotalCost, @Status)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StationId", SqlDbType.Int).Value = session.StationId;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)session.UserId ?? DBNull.Value;
                cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = session.StartTime;
                cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = (object?)session.EndTime ?? DBNull.Value;
                cmd.Parameters.Add("@TariffType", SqlDbType.NVarChar, 50).Value = session.TariffType;
                cmd.Parameters.Add(CreateDecimalParameter("@PlayCost", session.PlayCost));
                cmd.Parameters.Add(CreateDecimalParameter("@ServiceCost", session.ServiceCost));
                cmd.Parameters.Add(CreateDecimalParameter("@TotalCost", session.TotalCost));
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = session.Status;

                object? insertedId = cmd.ExecuteScalar();
                if (insertedId != null && insertedId != DBNull.Value)
                {
                    session.SessionId = Convert.ToInt32(insertedId, CultureInfo.InvariantCulture);
                    return true;
                }
                return false;
            }
            catch when (!opened)
            {
                session.SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions.Max(s => s.SessionId) + 1 : 1;
                _fallbackSessions.Insert(0, session);
                return true;
            }
        }

        public bool DeleteSession(int sessionId)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                const string cleanupSql = @"IF OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL
                                                DELETE FROM dbo.SessionService WHERE SessionId = @SessionId;
                                            IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                                DELETE FROM dbo.WalletTransaction WHERE SessionId = @SessionId;
                                            DELETE FROM dbo.GamingSession WHERE SessionId = @SessionId;";
                using var cmd = new SqlCommand(cleanupSql, conn, trans);
                cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                int affected = cmd.ExecuteNonQuery();
                trans.Commit();
                return affected > 0;
            }
            catch when (!opened)
            {
                return _fallbackSessions.RemoveAll(s => s.SessionId == sessionId) > 0;
            }
        }

        public GamingSession? GetActiveSession(int stationId)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"SELECT TOP 1 SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status
                                     FROM dbo.GamingSession
                                     WHERE StationId = @StationId AND Status = N'Active'
                                     ORDER BY SessionId DESC";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapSession(reader);
                }
                return null;
            }
            catch when (!opened)
            {
                return _fallbackSessions.Find(s => s.StationId == stationId && s.Status == "Active");
            }
        }

        public List<GamingSession> GetActiveSessions()
        {
            var list = new List<GamingSession>();
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"SELECT SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status
                                     FROM dbo.GamingSession
                                     WHERE Status = N'Active'
                                     ORDER BY SessionId DESC";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapSession(reader));
                }
                return list;
            }
            catch when (!opened)
            {
                return _fallbackSessions.FindAll(s => s.Status == "Active");
            }
        }

        public bool StartSession(GamingSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            session.Status = "Active";
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"INSERT INTO dbo.GamingSession (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status)
                                     OUTPUT INSERTED.SessionId
                                     VALUES (@StationId, @UserId, @StartTime, NULL, @TariffType, 0, 0, 0, N'Active')";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StationId", SqlDbType.Int).Value = session.StationId;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)session.UserId ?? DBNull.Value;
                cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = session.StartTime;
                cmd.Parameters.Add("@TariffType", SqlDbType.NVarChar, 50).Value = session.TariffType;

                object? newId = cmd.ExecuteScalar();
                if (newId != null && newId != DBNull.Value)
                {
                    session.SessionId = Convert.ToInt32(newId, CultureInfo.InvariantCulture);
                    return true;
                }
                return false;
            }
            catch when (!opened)
            {
                session.SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions.Max(s => s.SessionId) + 1 : 1;
                _fallbackSessions.Insert(0, session);
                return true;
            }
        }

        public bool CompleteSession(int sessionId, DateTime endTime, decimal playCost, decimal serviceCost, decimal totalCost)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"UPDATE dbo.GamingSession
                                     SET EndTime = @EndTime, PlayCost = @PlayCost, ServiceCost = @ServiceCost, TotalCost = @TotalCost, Status = N'Completed'
                                     WHERE SessionId = @SessionId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
                cmd.Parameters.Add(CreateDecimalParameter("@PlayCost", playCost));
                cmd.Parameters.Add(CreateDecimalParameter("@ServiceCost", serviceCost));
                cmd.Parameters.Add(CreateDecimalParameter("@TotalCost", totalCost));
                cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch when (!opened)
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

        public GamingSession OpenStationSession(
            int stationId,
            int? userId,
            string tariffType,
            DateTime? startTime = null,
            decimal minimumRequiredBalance = 1m)
        {
            if (stationId <= 0)
                throw new ArgumentException("Mã máy trạm không hợp lệ.", nameof(stationId));

            string normalizedTariff = string.IsNullOrWhiteSpace(tariffType) ? "Hourly" : tariffType.Trim();
            DateTime effectiveStart = startTime ?? DateTime.Now;
            decimal requiredFloor = minimumRequiredBalance > 0m ? minimumRequiredBalance : 1m;

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                string stationName;
                string stationStatus;
                const string stationSql = @"SELECT StationName, Status
                                            FROM dbo.ComputerStation WITH (UPDLOCK, HOLDLOCK)
                                            WHERE StationId = @StationId";
                using (var stationCmd = new SqlCommand(stationSql, conn, trans))
                {
                    stationCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    using var reader = stationCmd.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Không tìm thấy máy trạm được chọn.");

                    stationName = reader.IsDBNull(0) ? $"PC-{stationId:D2}" : reader.GetString(0);
                    stationStatus = reader.IsDBNull(1) ? "Available" : reader.GetString(1);
                }

                int activeCount;
                const string activeSql = @"SELECT COUNT(1)
                                           FROM dbo.GamingSession WITH (UPDLOCK, HOLDLOCK)
                                           WHERE StationId = @StationId AND Status = N'Active'";
                using (var activeCmd = new SqlCommand(activeSql, conn, trans))
                {
                    activeCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    object? countObj = activeCmd.ExecuteScalar();
                    activeCount = countObj != null && countObj != DBNull.Value
                        ? Convert.ToInt32(countObj, CultureInfo.InvariantCulture)
                        : 0;
                }

                if (string.Equals(stationStatus, "InUse", StringComparison.OrdinalIgnoreCase) || activeCount > 0)
                    throw new StationInUseException(stationId, stationName);

                if (!string.Equals(stationStatus, "Available", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Máy {stationName} hiện đang ở trạng thái '{stationStatus}', không thể mở.");

                if (userId.HasValue)
                {
                    bool hasIsActive;
                    using (var colCmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH('dbo.AppUser', 'IsActive') IS NOT NULL THEN 1 ELSE 0 END", conn, trans))
                    {
                        object? colObj = colCmd.ExecuteScalar();
                        hasIsActive = colObj != null && colObj != DBNull.Value && Convert.ToInt32(colObj, CultureInfo.InvariantCulture) == 1;
                    }

                    string userSql = hasIsActive
                        ? @"SELECT Username, Balance, IsActive FROM dbo.AppUser WITH (UPDLOCK, HOLDLOCK) WHERE UserId = @UserId"
                        : @"SELECT Username, Balance FROM dbo.AppUser WITH (UPDLOCK, HOLDLOCK) WHERE UserId = @UserId";

                    using var userCmd = new SqlCommand(userSql, conn, trans);
                    userCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
                    using var userReader = userCmd.ExecuteReader();
                    if (!userReader.Read())
                        throw new InvalidOperationException("Không tìm thấy tài khoản hội viên.");

                    string username = userReader.IsDBNull(0) ? string.Empty : userReader.GetString(0);
                    decimal balance = userReader.IsDBNull(1) ? 0m : userReader.GetDecimal(1);
                    bool isActive = !hasIsActive || userReader.IsDBNull(2) || userReader.GetBoolean(2);

                    if (!isActive)
                        throw new InvalidOperationException($"Tài khoản '{username}' hiện đang bị khóa, không thể mở máy.");

                    if (balance <= 0m || balance < requiredFloor)
                        throw new InsufficientBalanceException(balance, requiredFloor);
                }

                int sessionId;
                const string insertSessionSql = @"INSERT INTO dbo.GamingSession
                                                      (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status)
                                                  OUTPUT INSERTED.SessionId
                                                  VALUES
                                                      (@StationId, @UserId, @StartTime, NULL, @TariffType, 0, 0, 0, N'Active')";
                using (var insertCmd = new SqlCommand(insertSessionSql, conn, trans))
                {
                    insertCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    insertCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)userId ?? DBNull.Value;
                    insertCmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = effectiveStart;
                    insertCmd.Parameters.Add("@TariffType", SqlDbType.NVarChar, 50).Value = normalizedTariff;

                    object? newSessionId = insertCmd.ExecuteScalar();
                    if (newSessionId == null || newSessionId == DBNull.Value)
                        throw new InvalidOperationException("Không thể khởi tạo phiên chơi mới.");

                    sessionId = Convert.ToInt32(newSessionId, CultureInfo.InvariantCulture);
                }

                const string updateStationSql = @"UPDATE dbo.ComputerStation
                                                  SET Status = N'InUse'
                                                  WHERE StationId = @StationId";
                using (var updateStationCmd = new SqlCommand(updateStationSql, conn, trans))
                {
                    updateStationCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    if (updateStationCmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("Không thể cập nhật trạng thái máy trạm.");
                }

                trans.Commit();

                return new GamingSession
                {
                    SessionId = sessionId,
                    StationId = stationId,
                    UserId = userId,
                    StartTime = effectiveStart,
                    EndTime = null,
                    TariffType = normalizedTariff,
                    PlayCost = 0m,
                    ServiceCost = 0m,
                    TotalCost = 0m,
                    Status = "Active"
                };
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException or StationInUseException or InsufficientBalanceException))
            {
                if (_fallbackSessions.Any(s => s.StationId == stationId && s.Status == "Active"))
                    throw new StationInUseException(stationId, $"PC-{stationId:D2}");

                var newSession = new GamingSession
                {
                    SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions.Max(s => s.SessionId) + 1 : 1,
                    StationId = stationId,
                    UserId = userId,
                    StartTime = effectiveStart,
                    EndTime = null,
                    TariffType = normalizedTariff,
                    PlayCost = 0m,
                    ServiceCost = 0m,
                    TotalCost = 0m,
                    Status = "Active"
                };
                _fallbackSessions.Insert(0, newSession);
                return newSession;
            }
        }

        public SessionCloseResult CloseStationSession(
            int stationId,
            decimal basePricePerHour,
            DateTime? endTime = null,
            bool allowCashSupplement = false,
            int? fallbackUserId = null,
            string? fallbackTariffType = null,
            DateTime? fallbackStartTime = null,
            int? processedByUserId = null)
        {
            if (stationId <= 0)
                throw new ArgumentException("Mã máy trạm không hợp lệ.", nameof(stationId));
            if (basePricePerHour < 0m)
                throw new ArgumentOutOfRangeException(nameof(basePricePerHour), "Đơn giá giờ không được âm.");

            DateTime effectiveEnd = endTime ?? DateTime.Now;
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                string stationName;
                string stationStatus;
                const string stationSql = @"SELECT StationName, Status
                                            FROM dbo.ComputerStation WITH (UPDLOCK, HOLDLOCK)
                                            WHERE StationId = @StationId";
                using (var stationCmd = new SqlCommand(stationSql, conn, trans))
                {
                    stationCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    using var reader = stationCmd.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Không tìm thấy máy trạm được chọn.");

                    stationName = reader.IsDBNull(0) ? $"PC-{stationId:D2}" : reader.GetString(0);
                    stationStatus = reader.IsDBNull(1) ? "Available" : reader.GetString(1);
                }

                int sessionId = 0;
                int? userId = fallbackUserId;
                DateTime startTime = fallbackStartTime ?? effectiveEnd.AddHours(-1.5);
                string tariffType = string.IsNullOrWhiteSpace(fallbackTariffType) ? "Hourly" : fallbackTariffType.Trim();
                decimal existingServiceCost = 0m;

                const string activeSql = @"SELECT TOP 1 SessionId, UserId, StartTime, TariffType, ServiceCost
                                           FROM dbo.GamingSession WITH (UPDLOCK, HOLDLOCK)
                                           WHERE StationId = @StationId AND Status = N'Active'
                                           ORDER BY SessionId DESC";
                using (var activeCmd = new SqlCommand(activeSql, conn, trans))
                {
                    activeCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    using var reader = activeCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        sessionId = reader.GetInt32(0);
                        userId = reader.IsDBNull(1) ? null : reader.GetInt32(1);
                        startTime = reader.IsDBNull(2) ? startTime : reader.GetDateTime(2);
                        tariffType = reader.IsDBNull(3) ? "Hourly" : reader.GetString(3);
                        existingServiceCost = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                    }
                    else if (!string.Equals(stationStatus, "InUse", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Máy {stationName} hiện không có phiên chơi đang hoạt động.");
                    }
                }

                decimal serviceCost = existingServiceCost;
                if (sessionId > 0)
                {
                    const string serviceSql = @"IF OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL
                                                    SELECT COUNT(1), ISNULL(SUM(TotalPrice), 0)
                                                    FROM dbo.SessionService
                                                    WHERE SessionId = @SessionId
                                                ELSE
                                                    SELECT 0, CAST(0 AS DECIMAL(18,2));";
                    using var svcCmd = new SqlCommand(serviceSql, conn, trans);
                    svcCmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                    using var svcReader = svcCmd.ExecuteReader();
                    if (svcReader.Read())
                    {
                        int itemCount = svcReader.IsDBNull(0) ? 0 : svcReader.GetInt32(0);
                        decimal itemSum = svcReader.IsDBNull(1) ? 0m : svcReader.GetDecimal(1);
                        if (itemCount > 0)
                        {
                            serviceCost = itemSum;
                        }
                    }
                }

                string? username = null;
                decimal balanceBefore = 0m;
                string memberTier = "Standard";

                if (userId.HasValue)
                {
                    const string userSql = @"SELECT Username, Balance, MemberTier
                                             FROM dbo.AppUser WITH (UPDLOCK, HOLDLOCK)
                                             WHERE UserId = @UserId";
                    using var userCmd = new SqlCommand(userSql, conn, trans);
                    userCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
                    using var userReader = userCmd.ExecuteReader();
                    if (!userReader.Read())
                        throw new InvalidOperationException("Không tìm thấy tài khoản hội viên của phiên chơi.");

                    username = userReader.IsDBNull(0) ? string.Empty : userReader.GetString(0);
                    balanceBefore = userReader.IsDBNull(1) ? 0m : userReader.GetDecimal(1);
                    memberTier = userReader.IsDBNull(2) ? "Standard" : userReader.GetString(2);
                }

                TariffCalculatorBase calculator = CreateTariffCalculator(tariffType, memberTier);
                decimal playCost = calculator.CalculateCost(startTime, effectiveEnd, basePricePerHour);
                decimal totalCost = playCost + serviceCost;

                decimal walletDeducted = 0m;
                decimal cashDue = totalCost;
                decimal? balanceAfter = null;

                if (userId.HasValue)
                {
                    if (balanceBefore < totalCost)
                    {
                        if (!allowCashSupplement)
                            throw new InsufficientBalanceException(balanceBefore, totalCost);

                        walletDeducted = balanceBefore;
                        cashDue = totalCost - walletDeducted;
                        balanceAfter = 0m;
                    }
                    else
                    {
                        walletDeducted = totalCost;
                        cashDue = 0m;
                        balanceAfter = balanceBefore - walletDeducted;
                    }

                    if (walletDeducted > 0m)
                    {
                        const string deductSql = @"UPDATE dbo.AppUser
                                                   SET Balance = Balance - @Amount
                                                   WHERE UserId = @UserId AND Balance >= @Amount";
                        using var deductCmd = new SqlCommand(deductSql, conn, trans);
                        deductCmd.Parameters.Add(CreateDecimalParameter("@Amount", walletDeducted));
                        deductCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
                        if (deductCmd.ExecuteNonQuery() != 1)
                            throw new InsufficientBalanceException(balanceBefore, totalCost);
                    }
                }

                if (sessionId > 0)
                {
                    const string completeSql = @"UPDATE dbo.GamingSession
                                                 SET EndTime = @EndTime,
                                                     PlayCost = @PlayCost,
                                                     ServiceCost = @ServiceCost,
                                                     TotalCost = @TotalCost,
                                                     Status = N'Completed'
                                                 WHERE SessionId = @SessionId AND Status = N'Active'";
                    using var completeCmd = new SqlCommand(completeSql, conn, trans);
                    completeCmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = effectiveEnd;
                    completeCmd.Parameters.Add(CreateDecimalParameter("@PlayCost", playCost));
                    completeCmd.Parameters.Add(CreateDecimalParameter("@ServiceCost", serviceCost));
                    completeCmd.Parameters.Add(CreateDecimalParameter("@TotalCost", totalCost));
                    completeCmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                    if (completeCmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("Phiên chơi đã được đóng trước đó.");
                }
                else
                {
                    const string insertCompletedSql = @"INSERT INTO dbo.GamingSession
                                                            (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status)
                                                        OUTPUT INSERTED.SessionId
                                                        VALUES
                                                            (@StationId, @UserId, @StartTime, @EndTime, @TariffType, @PlayCost, @ServiceCost, @TotalCost, N'Completed')";
                    using var insertCmd = new SqlCommand(insertCompletedSql, conn, trans);
                    insertCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    insertCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)userId ?? DBNull.Value;
                    insertCmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = startTime;
                    insertCmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = effectiveEnd;
                    insertCmd.Parameters.Add("@TariffType", SqlDbType.NVarChar, 50).Value = tariffType;
                    insertCmd.Parameters.Add(CreateDecimalParameter("@PlayCost", playCost));
                    insertCmd.Parameters.Add(CreateDecimalParameter("@ServiceCost", serviceCost));
                    insertCmd.Parameters.Add(CreateDecimalParameter("@TotalCost", totalCost));

                    object? newSessionId = insertCmd.ExecuteScalar();
                    sessionId = newSessionId != null && newSessionId != DBNull.Value
                        ? Convert.ToInt32(newSessionId, CultureInfo.InvariantCulture)
                        : 0;
                }

                if (userId.HasValue && walletDeducted > 0m && balanceAfter.HasValue)
                {
                    string paymentMethod = cashDue > 0m ? "Ví hội viên + Tiền mặt" : "Ví hội viên";
                    string description = cashDue > 0m
                        ? $"Thanh toán phiên {stationName} (#{sessionId}) - trừ ví {walletDeducted:N0}đ, thu thêm tiền mặt {cashDue:N0}đ"
                        : $"Thanh toán phiên {stationName} (#{sessionId}) qua ví hội viên";

                    const string walletTxSql = @"IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                                 BEGIN
                                                     DECLARE @SafeCreatedBy INT = CASE
                                                         WHEN @CreatedByUserId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.AppUser WHERE UserId = @CreatedByUserId)
                                                         THEN @CreatedByUserId
                                                         ELSE NULL
                                                     END;
                                                     INSERT INTO dbo.WalletTransaction
                                                         (UserId, SessionId, TransactionType, Amount, BalanceBefore, BalanceAfter, PaymentMethod, Description, CreatedByUserId)
                                                     VALUES
                                                         (@UserId, @SessionId, N'SessionPayment', @Amount, @BalanceBefore, @BalanceAfter, @PaymentMethod, @Description, @SafeCreatedBy);
                                                 END";
                    using var txCmd = new SqlCommand(walletTxSql, conn, trans);
                    txCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
                    txCmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                    txCmd.Parameters.Add(CreateDecimalParameter("@Amount", walletDeducted));
                    txCmd.Parameters.Add(CreateDecimalParameter("@BalanceBefore", balanceBefore));
                    txCmd.Parameters.Add(CreateDecimalParameter("@BalanceAfter", balanceAfter.Value));
                    txCmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = paymentMethod;
                    txCmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = description;
                    txCmd.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = (object?)processedByUserId ?? DBNull.Value;
                    txCmd.ExecuteNonQuery();
                }

                const string resetStationSql = @"UPDATE dbo.ComputerStation
                                                 SET Status = N'Available'
                                                 WHERE StationId = @StationId";
                using (var resetCmd = new SqlCommand(resetStationSql, conn, trans))
                {
                    resetCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    resetCmd.ExecuteNonQuery();
                }

                trans.Commit();

                return new SessionCloseResult
                {
                    SessionId = sessionId,
                    StationId = stationId,
                    StationName = stationName,
                    UserId = userId,
                    Username = username,
                    TariffType = tariffType,
                    TariffDisplayName = calculator.TariffName,
                    StartTime = startTime,
                    EndTime = effectiveEnd,
                    PlayCost = playCost,
                    ServiceCost = serviceCost,
                    TotalCost = totalCost,
                    WalletDeducted = walletDeducted,
                    CashDue = cashDue,
                    RemainingBalance = balanceAfter
                };
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException or StationInUseException or InsufficientBalanceException))
            {
                var active = _fallbackSessions.Find(s => s.StationId == stationId && s.Status == "Active");
                DateTime startTime = active?.StartTime ?? fallbackStartTime ?? effectiveEnd.AddHours(-1.5);
                int? userId = active?.UserId ?? fallbackUserId;
                string tariffType = active?.TariffType ?? (string.IsNullOrWhiteSpace(fallbackTariffType) ? "Hourly" : fallbackTariffType);
                decimal serviceCost = active?.ServiceCost ?? 0m;

                TariffCalculatorBase calculator = CreateTariffCalculator(tariffType, "Standard");
                decimal playCost = calculator.CalculateCost(startTime, effectiveEnd, basePricePerHour);
                decimal totalCost = playCost + serviceCost;

                if (active != null)
                {
                    active.EndTime = effectiveEnd;
                    active.PlayCost = playCost;
                    active.ServiceCost = serviceCost;
                    active.TotalCost = totalCost;
                    active.Status = "Completed";
                }

                return new SessionCloseResult
                {
                    SessionId = active?.SessionId ?? 0,
                    StationId = stationId,
                    StationName = $"PC-{stationId:D2}",
                    UserId = userId,
                    TariffType = tariffType,
                    TariffDisplayName = calculator.TariffName,
                    StartTime = startTime,
                    EndTime = effectiveEnd,
                    PlayCost = playCost,
                    ServiceCost = serviceCost,
                    TotalCost = totalCost,
                    WalletDeducted = 0m,
                    CashDue = totalCost,
                    RemainingBalance = null
                };
            }
        }

        public string ToggleStationMaintenance(int stationId)
        {
            if (stationId <= 0)
                throw new ArgumentException("Mã máy trạm không hợp lệ.", nameof(stationId));

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                string stationName;
                string currentStatus;
                const string selectSql = @"SELECT StationName, Status
                                           FROM dbo.ComputerStation WITH (UPDLOCK, HOLDLOCK)
                                           WHERE StationId = @StationId";
                using (var cmd = new SqlCommand(selectSql, conn, trans))
                {
                    cmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Không tìm thấy máy trạm được chọn.");

                    stationName = reader.IsDBNull(0) ? $"PC-{stationId:D2}" : reader.GetString(0);
                    currentStatus = reader.IsDBNull(1) ? "Available" : reader.GetString(1);
                }

                int activeCount;
                const string activeSql = @"SELECT COUNT(1)
                                           FROM dbo.GamingSession WITH (UPDLOCK, HOLDLOCK)
                                           WHERE StationId = @StationId AND Status = N'Active'";
                using (var activeCmd = new SqlCommand(activeSql, conn, trans))
                {
                    activeCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    object? countObj = activeCmd.ExecuteScalar();
                    activeCount = countObj != null && countObj != DBNull.Value
                        ? Convert.ToInt32(countObj, CultureInfo.InvariantCulture)
                        : 0;
                }

                if (string.Equals(currentStatus, "InUse", StringComparison.OrdinalIgnoreCase) || activeCount > 0)
                    throw new StationInUseException(stationId, stationName);

                if (string.Equals(currentStatus, "TournamentLocked", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Máy {stationName} đang khóa cho giải đấu LAN, không thể chuyển bảo trì.");

                string targetStatus = string.Equals(currentStatus, "Maintenance", StringComparison.OrdinalIgnoreCase)
                    ? "Available"
                    : "Maintenance";

                const string updateSql = @"UPDATE dbo.ComputerStation
                                           SET Status = @Status
                                           WHERE StationId = @StationId";
                using (var updateCmd = new SqlCommand(updateSql, conn, trans))
                {
                    updateCmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = targetStatus;
                    updateCmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
                    updateCmd.ExecuteNonQuery();
                }

                trans.Commit();
                return targetStatus;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException or StationInUseException))
            {
                return "Maintenance";
            }
        }
    }
}
