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


    public sealed class DailyRevenueSummary
    {
        public DateTime Date { get; set; }
        public int SessionCount { get; set; }
        public decimal PlayRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal Revenue { get; set; }
    }

    public sealed class TariffRevenueSummary
    {
        public string TariffType { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public decimal PlayRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal Revenue { get; set; }
    }

    public sealed class SessionServiceDetail
    {
        public int SessionServiceId { get; set; }
        public int SessionId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderedAt { get; set; }
        public string? Note { get; set; }
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

        private static readonly object _fallbackLock = new();

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


        // Shared offline fallback used when SQL Server is unavailable.
        private static readonly List<ServiceItem> _fallbackServiceItems = new()
        {
            new() { ServiceId = 1, ServiceName = "Sting Dâu", Category = "Đồ uống", UnitPrice = 15000m, StockQuantity = 50, IsActive = true },
            new() { ServiceId = 2, ServiceName = "Red Bull", Category = "Đồ uống", UnitPrice = 20000m, StockQuantity = 40, IsActive = true },
            new() { ServiceId = 3, ServiceName = "Coca-Cola Lon", Category = "Đồ uống", UnitPrice = 15000m, StockQuantity = 60, IsActive = true },
            new() { ServiceId = 4, ServiceName = "Mì Tôm Trứng Xúc Xích", Category = "Đồ ăn", UnitPrice = 25000m, StockQuantity = 30, IsActive = true },
            new() { ServiceId = 5, ServiceName = "Cơm Gà Xối Mỡ", Category = "Đồ ăn", UnitPrice = 45000m, StockQuantity = 20, IsActive = true },
            new() { ServiceId = 6, ServiceName = "Bánh Mì Trứng Ốp La", Category = "Đồ ăn", UnitPrice = 20000m, StockQuantity = 25, IsActive = true }
        };

        private static readonly List<SessionService> _fallbackSessionServices = new()
        {
            new() { SessionServiceId = 1, SessionId = 1, ServiceId = 1, Quantity = 1,
                UnitPrice = 15000m, TotalPrice = 15000m, Note = "Dịch vụ mẫu phiên 1", OrderedAt = DateTime.Today.AddHours(9) }
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
                lock (_fallbackLock)
                {
                    return new List<TariffPlan>(_fallbackPlans);
                }
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
                lock (_fallbackLock)
                {
                    plan.PlanId = _fallbackPlans.Count > 0 ? _fallbackPlans[^1].PlanId + 1 : 1;
                    _fallbackPlans.Add(plan);
                    return true;
                }
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
                lock (_fallbackLock)
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
        }


        public bool DeletePlan(int planId)
        {
            if (planId <= 0)
                throw new ArgumentOutOfRangeException(nameof(planId), "Mã gói cước không hợp lệ.");

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var cmd = new SqlCommand("DELETE FROM dbo.TariffPlan WHERE PlanId = @PlanId", conn);
                cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = planId;
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    return _fallbackPlans.RemoveAll(p => p.PlanId == planId) > 0;
                }
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
                lock (_fallbackLock)
                {
                    return new List<GamingSession>(_fallbackSessions);
                }
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


        public static List<DailyRevenueSummary> GroupRevenueByDay(IEnumerable<GamingSession> completedSessions)
        {
            ArgumentNullException.ThrowIfNull(completedSessions);
            return completedSessions
                .Where(s => s.EndTime.HasValue && string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .GroupBy(s => s.EndTime!.Value.Date)
                .Select(g => new DailyRevenueSummary
                {
                    Date = g.Key,
                    SessionCount = g.Count(),
                    PlayRevenue = g.Sum(s => s.PlayCost),
                    ServiceRevenue = g.Sum(s => s.ServiceCost),
                    Revenue = g.Sum(s => s.TotalCost)
                })
                .OrderByDescending(x => x.Date)
                .ToList();
        }

        public static List<TariffRevenueSummary> GroupRevenueByTariffType(IEnumerable<GamingSession> completedSessions)
        {
            ArgumentNullException.ThrowIfNull(completedSessions);
            return completedSessions
                .Where(s => s.EndTime.HasValue && string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .GroupBy(s => string.IsNullOrWhiteSpace(s.TariffType) ? "Hourly" : s.TariffType.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Select(g => new TariffRevenueSummary
                {
                    TariffType = g.Key,
                    SessionCount = g.Count(),
                    PlayRevenue = g.Sum(s => s.PlayCost),
                    ServiceRevenue = g.Sum(s => s.ServiceCost),
                    Revenue = g.Sum(s => s.TotalCost)
                })
                .OrderByDescending(x => x.Revenue)
                .ThenBy(x => x.TariffType)
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


            report.AppendLine();
            report.AppendLine("THỐNG KÊ DOANH THU THEO NGÀY");
            report.AppendLine("Ngày\tSố phiên\tTiền giờ\tTiền dịch vụ\tTổng doanh thu (VNĐ)");
            foreach (DailyRevenueSummary item in GroupRevenueByDay(sessions))
                report.AppendLine($"{item.Date:dd/MM/yyyy}\t{item.SessionCount}\t{item.PlayRevenue.ToString("N0", vn)}\t{item.ServiceRevenue.ToString("N0", vn)}\t{item.Revenue.ToString("N0", vn)}");

            report.AppendLine();
            report.AppendLine("THỐNG KÊ DOANH THU THEO GÓI CƯỚC");
            report.AppendLine("Gói cước\tSố phiên\tTiền giờ\tTiền dịch vụ\tTổng doanh thu (VNĐ)");
            foreach (TariffRevenueSummary item in GroupRevenueByTariffType(sessions))
                report.AppendLine($"{item.TariffType}\t{item.SessionCount}\t{item.PlayRevenue.ToString("N0", vn)}\t{item.ServiceRevenue.ToString("N0", vn)}\t{item.Revenue.ToString("N0", vn)}");

            return report.ToString();
        }

        public static string NormalizeTariffType(string? tariffType)
        {
            if (string.IsNullOrWhiteSpace(tariffType))
                return "Hourly";

            string trimmed = tariffType.Trim();
            if (trimmed.Equals("Hourly", StringComparison.OrdinalIgnoreCase))
                return "Hourly";
            if (trimmed.Equals("NightCombo", StringComparison.OrdinalIgnoreCase))
                return "NightCombo";
            if (trimmed.Equals("Tournament", StringComparison.OrdinalIgnoreCase))
                return "Tournament";
            if (trimmed.Equals("MemberTier", StringComparison.OrdinalIgnoreCase))
                return "MemberTier";

            throw new ArgumentException("Loại gói cước không hợp lệ (Hourly, NightCombo, Tournament, MemberTier).", nameof(tariffType));
        }

        public static TariffCalculatorBase CreateTariffCalculator(string? tariffType, string? memberTier = null)
        {
            if (string.IsNullOrWhiteSpace(tariffType))
                return new HourlyTariff();

            string trimmed = tariffType.Trim();
            if (trimmed.Equals("NightCombo", StringComparison.OrdinalIgnoreCase))
                return new NightComboTariff();
            if (trimmed.Equals("Tournament", StringComparison.OrdinalIgnoreCase))
                return new TournamentTariff();
            if (trimmed.Equals("MemberTier", StringComparison.OrdinalIgnoreCase))
                return new MemberTierTariff(string.IsNullOrWhiteSpace(memberTier) ? "Standard" : memberTier);

            return new HourlyTariff();
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


        private static void ValidateServiceItem(ServiceItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            item.ServiceName = (item.ServiceName ?? string.Empty).Trim();
            item.Category = string.IsNullOrWhiteSpace(item.Category) ? "Đồ uống" : item.Category.Trim();
            if (item.ServiceName.Length == 0 || item.ServiceName.Length > 100)
                throw new ArgumentException("Tên món phải có từ 1 đến 100 ký tự.");
            if (item.Category.Length > 50)
                throw new ArgumentException("Phân loại không được vượt quá 50 ký tự.");
            if (item.UnitPrice < 0m || item.StockQuantity < 0)
                throw new ArgumentException("Giá hoặc số lượng tồn kho không được âm.");
        }

        private static ServiceItem CopyServiceItem(ServiceItem item) => new()
        {
            ServiceId = item.ServiceId,
            ServiceName = item.ServiceName,
            Category = item.Category,
            UnitPrice = item.UnitPrice,
            StockQuantity = item.StockQuantity,
            IsActive = item.IsActive
        };

        public List<ServiceItem> GetAllServiceItems(bool activeOnly = false)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                string sql = @"SELECT ServiceId, ServiceName, Category, UnitPrice, StockQuantity, IsActive
                               FROM dbo.ServiceItem" + (activeOnly ? " WHERE IsActive = 1" : "") + " ORDER BY Category, ServiceName";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                var items = new List<ServiceItem>();
                while (reader.Read())
                {
                    items.Add(new ServiceItem
                    {
                        ServiceId = reader.GetInt32(0),
                        ServiceName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Category = reader.IsDBNull(2) ? "Đồ uống" : reader.GetString(2),
                        UnitPrice = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                        StockQuantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        IsActive = !reader.IsDBNull(5) && reader.GetBoolean(5)
                    });
                }
                return items;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    return _fallbackServiceItems.Where(s => !activeOnly || s.IsActive)
                        .OrderBy(s => s.Category).ThenBy(s => s.ServiceName)
                        .Select(CopyServiceItem).ToList();
                }
            }
        }

        public bool AddServiceItem(ServiceItem item)
        {
            ValidateServiceItem(item);
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string existsSql = @"SELECT COUNT(1) FROM dbo.ServiceItem
                    WHERE LOWER(LTRIM(RTRIM(ServiceName))) = LOWER(LTRIM(RTRIM(@ServiceName)))";
                using (var exists = new SqlCommand(existsSql, conn))
                {
                    exists.Parameters.Add("@ServiceName", SqlDbType.NVarChar, 100).Value = item.ServiceName;
                    if (Convert.ToInt32(exists.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
                        throw new InvalidOperationException("Đã có món ăn hoặc đồ uống trùng tên.");
                }

                const string sql = @"INSERT INTO dbo.ServiceItem
                    (ServiceName, Category, UnitPrice, StockQuantity, IsActive)
                    OUTPUT INSERTED.ServiceId
                    VALUES (@ServiceName, @Category, @UnitPrice, @StockQuantity, @IsActive)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@ServiceName", SqlDbType.NVarChar, 100).Value = item.ServiceName;
                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = item.Category;
                cmd.Parameters.Add(CreateDecimalParameter("@UnitPrice", item.UnitPrice));
                cmd.Parameters.Add("@StockQuantity", SqlDbType.Int).Value = item.StockQuantity;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                item.ServiceId = Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
                return item.ServiceId > 0;
            }
            catch (SqlException ex) when (opened && (ex.Number == 2627 || ex.Number == 2601))
            {
                throw new InvalidOperationException("Đã có món ăn hoặc đồ uống trùng tên.", ex);
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    if (_fallbackServiceItems.Any(s => s.ServiceName.Trim().Equals(item.ServiceName, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("Đã có món ăn hoặc đồ uống trùng tên.");
                    item.ServiceId = _fallbackServiceItems.Count == 0 ? 1 : _fallbackServiceItems.Max(s => s.ServiceId) + 1;
                    _fallbackServiceItems.Add(CopyServiceItem(item));
                    return true;
                }
            }
        }

        public bool UpdateServiceItem(ServiceItem item)
        {
            ValidateServiceItem(item);
            if (item.ServiceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(item.ServiceId));
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string existsSql = @"SELECT COUNT(1) FROM dbo.ServiceItem
                    WHERE ServiceId <> @ServiceId AND LOWER(LTRIM(RTRIM(ServiceName))) = LOWER(LTRIM(RTRIM(@ServiceName)))";
                using (var exists = new SqlCommand(existsSql, conn))
                {
                    exists.Parameters.Add("@ServiceId", SqlDbType.Int).Value = item.ServiceId;
                    exists.Parameters.Add("@ServiceName", SqlDbType.NVarChar, 100).Value = item.ServiceName;
                    if (Convert.ToInt32(exists.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
                        throw new InvalidOperationException("Tên món đã được dùng cho một món khác.");
                }
                const string sql = @"UPDATE dbo.ServiceItem SET ServiceName=@ServiceName, Category=@Category,
                    UnitPrice=@UnitPrice, StockQuantity=@StockQuantity, IsActive=@IsActive WHERE ServiceId=@ServiceId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = item.ServiceId;
                cmd.Parameters.Add("@ServiceName", SqlDbType.NVarChar, 100).Value = item.ServiceName;
                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = item.Category;
                cmd.Parameters.Add(CreateDecimalParameter("@UnitPrice", item.UnitPrice));
                cmd.Parameters.Add("@StockQuantity", SqlDbType.Int).Value = item.StockQuantity;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (SqlException ex) when (opened && (ex.Number == 2627 || ex.Number == 2601))
            {
                throw new InvalidOperationException("Tên món đã được dùng cho một món khác.", ex);
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    if (_fallbackServiceItems.Any(s => s.ServiceId != item.ServiceId && s.ServiceName.Trim().Equals(item.ServiceName, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("Tên món đã được dùng cho một món khác.");
                    int index = _fallbackServiceItems.FindIndex(s => s.ServiceId == item.ServiceId);
                    if (index < 0) return false;
                    _fallbackServiceItems[index] = CopyServiceItem(item);
                    return true;
                }
            }
        }

        public bool ToggleServiceItemActive(int serviceId)
        {
            if (serviceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(serviceId));
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"UPDATE dbo.ServiceItem SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                                     OUTPUT INSERTED.IsActive WHERE ServiceId = @ServiceId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;
                return cmd.ExecuteScalar() is not null;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    ServiceItem? item = _fallbackServiceItems.FirstOrDefault(x => x.ServiceId == serviceId);
                    if (item is null) return false;
                    item.IsActive = !item.IsActive;
                    return true;
                }
            }
        }

        public bool RestockServiceItem(int serviceId, int addedQuantity)
        {
            if (serviceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(serviceId));
            if (addedQuantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(addedQuantity), "Số lượng nhập thêm phải lớn hơn 0.");

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"UPDATE dbo.ServiceItem
                                     SET StockQuantity = StockQuantity + @AddedQuantity
                                     WHERE ServiceId = @ServiceId AND StockQuantity <= 2147483647 - @AddedQuantity";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;
                cmd.Parameters.Add("@AddedQuantity", SqlDbType.Int).Value = addedQuantity;
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    ServiceItem? item = _fallbackServiceItems.FirstOrDefault(s => s.ServiceId == serviceId);
                    if (item == null) return false;
                    item.StockQuantity = checked(item.StockQuantity + addedQuantity);
                    return true;
                }
            }
        }

        private static SessionServiceDetail MapSessionService(SqlDataReader reader)
        {
            return new SessionServiceDetail
            {
                SessionServiceId = reader.GetInt32(0),
                SessionId = reader.GetInt32(1),
                ServiceId = reader.GetInt32(2),
                ServiceName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Category = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Quantity = reader.GetInt32(5),
                UnitPrice = reader.GetDecimal(6),
                TotalPrice = reader.GetDecimal(7),
                OrderedAt = reader.GetDateTime(8),
                Note = reader.IsDBNull(9) ? null : reader.GetString(9)
            };
        }

        public List<SessionServiceDetail> GetSessionServices(int sessionId)
        {
            if (sessionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = @"SELECT ss.SessionServiceId, ss.SessionId, ss.ServiceId, si.ServiceName,
                       si.Category, ss.Quantity, ss.UnitPrice, ss.TotalPrice, ss.OrderedAt, ss.Note
                    FROM dbo.SessionService ss INNER JOIN dbo.ServiceItem si ON ss.ServiceId = si.ServiceId
                    WHERE ss.SessionId = @SessionId ORDER BY ss.OrderedAt DESC, ss.SessionServiceId DESC";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                using var reader = cmd.ExecuteReader();
                var items = new List<SessionServiceDetail>();
                while (reader.Read()) items.Add(MapSessionService(reader));
                return items;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    return _fallbackSessionServices.Where(ss => ss.SessionId == sessionId)
                        .OrderByDescending(ss => ss.OrderedAt).ThenByDescending(ss => ss.SessionServiceId)
                        .Select(ss =>
                        {
                            ServiceItem? item = _fallbackServiceItems.FirstOrDefault(x => x.ServiceId == ss.ServiceId);
                            return new SessionServiceDetail
                            {
                                SessionServiceId = ss.SessionServiceId,
                                SessionId = ss.SessionId,
                                ServiceId = ss.ServiceId,
                                ServiceName = item?.ServiceName ?? string.Empty,
                                Category = item?.Category ?? string.Empty,
                                Quantity = ss.Quantity,
                                UnitPrice = ss.UnitPrice,
                                TotalPrice = ss.TotalPrice,
                                OrderedAt = ss.OrderedAt,
                                Note = ss.Note
                            };
                        }).ToList();
                }
            }
        }

        public bool OrderServiceForSession(int sessionId, int serviceId, int quantity, string? note = null)
        {
            if (sessionId <= 0) throw new ArgumentOutOfRangeException(nameof(sessionId));
            if (serviceId <= 0) throw new ArgumentOutOfRangeException(nameof(serviceId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            string? cleanNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (cleanNote is { Length: > 255 })
                throw new ArgumentException("Ghi chú không được vượt quá 255 ký tự.", nameof(note));

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                const string sessionSql = @"SELECT Status FROM dbo.GamingSession WITH (UPDLOCK, HOLDLOCK)
                                            WHERE SessionId = @SessionId";
                using (var cmd = new SqlCommand(sessionSql, conn, trans))
                {
                    cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                    string? status = cmd.ExecuteScalar() as string;
                    if (!string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Phiên chơi không tồn tại hoặc đã thanh toán, không thể gọi thêm món.");
                }

                decimal unitPrice;
                using (var cmd = new SqlCommand(
                    @"SELECT ServiceName, UnitPrice, StockQuantity, IsActive FROM dbo.ServiceItem WITH (UPDLOCK, HOLDLOCK)
                      WHERE ServiceId = @ServiceId", conn, trans))
                {
                    cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                        throw new InvalidOperationException("Không tìm thấy món cần gọi.");
                    unitPrice = reader.GetDecimal(1);
                    var item = new ServiceItem
                    {
                        ServiceId = serviceId,
                        ServiceName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        UnitPrice = unitPrice,
                        StockQuantity = reader.GetInt32(2),
                        IsActive = reader.GetBoolean(3)
                    };
                    if (!item.IsActive)
                        throw new InvalidOperationException("Món đang tạm ngừng bán.");
                    if (!item.IsAvailableForOrder(quantity))
                        throw new InvalidOperationException($"{item.ServiceName}: Tồn kho chỉ còn {item.StockQuantity}, không đủ số lượng yêu cầu.");
                }

                const string reduceSql = @"UPDATE dbo.ServiceItem SET StockQuantity = StockQuantity - @Quantity
                                           WHERE ServiceId = @ServiceId AND IsActive = 1 AND StockQuantity >= @Quantity";
                using (var cmd = new SqlCommand(reduceSql, conn, trans))
                {
                    cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                    if (cmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("Không đủ tồn kho để gọi món.");
                }

                var order = new SessionService
                {
                    SessionId = sessionId,
                    ServiceId = serviceId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    OrderedAt = DateTime.Now,
                    Note = cleanNote
                };
                order.TotalPrice = order.CalculateLineTotal();

                const string insertSql = @"INSERT INTO dbo.SessionService
                        (SessionId, ServiceId, Quantity, UnitPrice, TotalPrice, OrderedAt, Note)
                        OUTPUT INSERTED.SessionServiceId
                        VALUES (@SessionId, @ServiceId, @Quantity, @UnitPrice, @TotalPrice, @OrderedAt, @Note)";
                using (var cmd = new SqlCommand(insertSql, conn, trans))
                {
                    cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                    cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                    cmd.Parameters.Add(CreateDecimalParameter("@UnitPrice", unitPrice));
                    cmd.Parameters.Add(CreateDecimalParameter("@TotalPrice", order.TotalPrice));
                    cmd.Parameters.Add("@OrderedAt", SqlDbType.DateTime).Value = order.OrderedAt;
                    cmd.Parameters.Add("@Note", SqlDbType.NVarChar, 255).Value = (object?)cleanNote ?? DBNull.Value;
                    object? orderId = cmd.ExecuteScalar();
                    if (orderId == null || orderId == DBNull.Value)
                        throw new InvalidOperationException("Không lưu được món đã gọi.");
                }

                const string updateSessionSql = @"UPDATE dbo.GamingSession
                    SET ServiceCost = ISNULL((SELECT SUM(TotalPrice) FROM dbo.SessionService WHERE SessionId = @SessionId), 0),
                        TotalCost = PlayCost + ISNULL((SELECT SUM(TotalPrice) FROM dbo.SessionService WHERE SessionId = @SessionId), 0)
                    WHERE SessionId = @SessionId";
                using (var cmd = new SqlCommand(updateSessionSql, conn, trans))
                {
                    cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                    if (cmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("Không cập nhật được tiền dịch vụ của phiên chơi.");
                }

                trans.Commit();
                return true;
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    GamingSession? session = _fallbackSessions.FirstOrDefault(s => s.SessionId == sessionId);
                    if (session == null || !string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Phiên chơi không tồn tại hoặc đã thanh toán, không thể gọi thêm món.");
                    ServiceItem? item = _fallbackServiceItems.FirstOrDefault(s => s.ServiceId == serviceId);
                    if (item == null) throw new InvalidOperationException("Không tìm thấy món cần gọi.");
                    if (!item.IsActive) throw new InvalidOperationException("Món đang tạm ngừng bán.");
                    if (!item.IsAvailableForOrder(quantity))
                        throw new InvalidOperationException($"{item.ServiceName}: Tồn kho chỉ còn {item.StockQuantity}, không đủ số lượng yêu cầu.");
                    var order = new SessionService
                    {
                        SessionServiceId = _fallbackSessionServices.Count == 0 ? 1 : _fallbackSessionServices.Max(s => s.SessionServiceId) + 1,
                        SessionId = sessionId, ServiceId = serviceId, Quantity = quantity,
                        UnitPrice = item.UnitPrice, OrderedAt = DateTime.Now, Note = cleanNote
                    };
                    order.TotalPrice = order.CalculateLineTotal();
                    decimal totalService = _fallbackSessionServices.Where(s => s.SessionId == sessionId).Sum(s => s.TotalPrice) + order.TotalPrice;
                    item.StockQuantity -= quantity;
                    _fallbackSessionServices.Add(order);
                    session.ServiceCost = totalService;
                    session.TotalCost = session.PlayCost + totalService;
                    return true;
                }
            }
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
                lock (_fallbackLock)
                {
                    session.SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions.Max(s => s.SessionId) + 1 : 1;
                    _fallbackSessions.Insert(0, session);
                    return true;
                }
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
                lock (_fallbackLock)
                {
                    return _fallbackSessions.RemoveAll(s => s.SessionId == sessionId) > 0;
                }
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
                const string sql = @"IF OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL
                                         SELECT TOP 1
                                             gs.SessionId, gs.StationId, gs.UserId, gs.StartTime, gs.EndTime, gs.TariffType, gs.PlayCost,
                                             CASE
                                                 WHEN EXISTS (SELECT 1 FROM dbo.SessionService ss WHERE ss.SessionId = gs.SessionId)
                                                 THEN ISNULL((SELECT SUM(ss.TotalPrice) FROM dbo.SessionService ss WHERE ss.SessionId = gs.SessionId), 0)
                                                 ELSE gs.ServiceCost
                                             END AS ServiceCost,
                                             gs.TotalCost, gs.Status
                                         FROM dbo.GamingSession gs
                                         WHERE gs.StationId = @StationId AND gs.Status = N'Active'
                                         ORDER BY gs.SessionId DESC;
                                     ELSE
                                         SELECT TOP 1 SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status
                                         FROM dbo.GamingSession
                                         WHERE StationId = @StationId AND Status = N'Active'
                                         ORDER BY SessionId DESC;";
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
                lock (_fallbackLock)
                {
                    return _fallbackSessions.Find(s => s.StationId == stationId && s.Status == "Active");
                }
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
                const string sql = @"IF OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL
                                         SELECT
                                             gs.SessionId, gs.StationId, gs.UserId, gs.StartTime, gs.EndTime, gs.TariffType, gs.PlayCost,
                                             CASE
                                                 WHEN EXISTS (SELECT 1 FROM dbo.SessionService ss WHERE ss.SessionId = gs.SessionId)
                                                 THEN ISNULL((SELECT SUM(ss.TotalPrice) FROM dbo.SessionService ss WHERE ss.SessionId = gs.SessionId), 0)
                                                 ELSE gs.ServiceCost
                                             END AS ServiceCost,
                                             gs.TotalCost, gs.Status
                                         FROM dbo.GamingSession gs
                                         WHERE gs.Status = N'Active'
                                         ORDER BY gs.SessionId DESC;
                                     ELSE
                                         SELECT SessionId, StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status
                                         FROM dbo.GamingSession
                                         WHERE Status = N'Active'
                                         ORDER BY SessionId DESC;";
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
                lock (_fallbackLock)
                {
                    return _fallbackSessions.FindAll(s => s.Status == "Active");
                }
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
                lock (_fallbackLock)
                {
                    session.SessionId = _fallbackSessions.Count > 0 ? _fallbackSessions.Max(s => s.SessionId) + 1 : 1;
                    _fallbackSessions.Insert(0, session);
                    return true;
                }
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
                lock (_fallbackLock)
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

        public GamingSession OpenStationSession(
            int stationId,
            int? userId,
            string tariffType,
            DateTime? startTime = null,
            decimal minimumRequiredBalance = 1m)
        {
            if (stationId <= 0)
                throw new ArgumentException("Mã máy trạm không hợp lệ.", nameof(stationId));

            string normalizedTariff = NormalizeTariffType(tariffType);
            if (string.Equals(normalizedTariff, "MemberTier", StringComparison.OrdinalIgnoreCase) && !userId.HasValue)
                throw new InvalidOperationException("Gói cước Ưu đãi hội viên (MemberTier) chỉ áp dụng khi có tài khoản hội viên.");

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
                lock (_fallbackLock)
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
                lock (_fallbackLock)
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
