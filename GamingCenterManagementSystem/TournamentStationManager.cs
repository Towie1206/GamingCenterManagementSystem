using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Trạng thái vận hành và thông tin chế độ thi đấu giải (TournamentMode) của từng máy trạm.
    /// </summary>
    public sealed class TournamentStationState
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string ZoneName { get; set; } = "Standard";
        public decimal StandardHourlyRate { get; set; } = 10000m;
        public string Status { get; set; } = "Available"; // Available, InUse, Maintenance, TournamentLocked
        public bool TournamentMode { get; set; }
        public int? TournamentId { get; set; }
        public string? TournamentTitle { get; set; }
        public string? MatchLabel { get; set; }
        public string? ClusterId { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockedByAdmin { get; set; }

        public TournamentStationState Clone() => new()
        {
            StationId = StationId,
            StationName = StationName,
            ZoneName = ZoneName,
            StandardHourlyRate = StandardHourlyRate,
            Status = Status,
            TournamentMode = TournamentMode,
            TournamentId = TournamentId,
            TournamentTitle = TournamentTitle,
            MatchLabel = MatchLabel,
            ClusterId = ClusterId,
            LockedAt = LockedAt,
            LockedByAdmin = LockedByAdmin
        };
    }

    /// <summary>
    /// Thông tin một đợt kích hoạt cụm máy thi đấu giải (dải máy được giữ chỗ).
    /// </summary>
    public sealed class TournamentClusterReservation
    {
        public string ClusterId { get; init; } = string.Empty;
        public int TournamentId { get; init; }
        public string TournamentTitle { get; init; } = string.Empty;
        public string MatchLabel { get; init; } = string.Empty;
        public string StartStationName { get; init; } = string.Empty;
        public string EndStationName { get; init; } = string.Empty;
        public IReadOnlyList<string> StationNames { get; init; } = Array.Empty<string>();
        public IReadOnlyList<int> StationIds { get; init; } = Array.Empty<int>();
        public DateTime ActivatedAt { get; init; }
        public string ActivatedBy { get; init; } = "Admin";
        public bool IsActive { get; internal set; } = true;
        public DateTime? ReleasedAt { get; internal set; }
    }

    /// <summary>
    /// Dữ liệu hiển thị màu sắc, icon và nhãn trạng thái trên sơ đồ phòng máy (Form1).
    /// </summary>
    public sealed class TournamentStationVisualInfo
    {
        public string StationName { get; init; } = string.Empty;
        public string ZoneName { get; init; } = string.Empty;
        public bool TournamentMode { get; init; }
        public string Icon { get; init; } = "🟢";
        public string BadgeText { get; init; } = "[SẴN SÀNG]";
        public string ButtonDisplayText { get; init; } = string.Empty;
        public Color BackColor { get; init; }
        public Color BorderColor { get; init; }
        public Color AccentColor { get; init; }
        public Color ForeColor { get; init; }
        public bool AllowRegularGuestOpen { get; init; }
        public string BlockReason { get; init; } = string.Empty;
    }

    /// <summary>
    /// Kết quả kiểm tra quyền mở máy của khách vãng lai / tài khoản thường khi máy có thể đang ở chế độ giải đấu.
    /// </summary>
    public sealed class TournamentStationAccessResult
    {
        public bool IsAllowed { get; init; }
        public bool IsBlockedByTournamentMode { get; init; }
        public string Reason { get; init; } = string.Empty;
    }

    /// <summary>
    /// Bộ máy điều phối và khóa cụm máy thi đấu giải (Tournament Mode) cho 30 máy trạm Master Game.
    /// </summary>
    public class TournamentMachineLockEngine
    {
        public const string VenueName = "Master Game — 165 Hoàng Công Chất, Phường Phú Diễn, Quận Bắc Từ Liêm, Hà Nội";

        // Bảng màu tím / vàng nổi bật dành riêng cho máy ở chế độ thi đấu giải trên sơ đồ phòng máy
        public static readonly Color TournamentBackColor = Color.FromArgb(243, 232, 255);      // Tím lavender nhạt
        public static readonly Color TournamentBorderColor = Color.FromArgb(124, 58, 237);     // Viền tím đậm
        public static readonly Color TournamentAccentGold = Color.FromArgb(217, 119, 6);       // Điểm nhấn vàng gold
        public static readonly Color TournamentForeColor = Color.FromArgb(76, 29, 149);        // Chữ tím đậm dễ đọc

        private static readonly Regex StationCodeRegex = new(@"^PC-(\d{1,3})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Dictionary<string, TournamentStationState> _stations = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TournamentClusterReservation> _clusters = new(StringComparer.OrdinalIgnoreCase);
        private int _clusterSequence;

        /// <summary>
        /// Sự kiện thông báo khi có cụm máy thi đấu được kích hoạt hoặc giải phóng.
        /// </summary>
        public event Action<string>? ClusterStatusChanged;

        /// <summary>
        /// Khởi tạo bộ máy với mặc định 30 máy trạm thực tế tại Master Game:
        /// Standard (PC-01..PC-10: 10K), Maximum (PC-11..PC-20: 15K), Master (PC-21..PC-30: 13K).
        /// </summary>
        public TournamentMachineLockEngine() : this(CreateMasterGameDefaultStations())
        {
        }

        public TournamentMachineLockEngine(IEnumerable<TournamentStationState> stations)
        {
            ArgumentNullException.ThrowIfNull(stations);
            foreach (var station in stations)
            {
                if (station is null || string.IsNullOrWhiteSpace(station.StationName))
                    throw new ArgumentException("Danh sách máy trạm chứa thông tin không hợp lệ.", nameof(stations));

                string normalizedName = NormalizeStationName(station.StationName);
                var copy = station.Clone();
                copy.StationName = normalizedName;
                if (!_stations.TryAdd(normalizedName, copy))
                    throw new ArgumentException($"Trùng tên máy trạm '{normalizedName}'.", nameof(stations));
            }

            if (_stations.Count == 0)
                throw new ArgumentException("Danh sách máy trạm không được để trống.", nameof(stations));
        }

        public TournamentMachineLockEngine(IEnumerable<ComputerStation> stations, IEnumerable<StationZone>? zones = null)
        {
            ArgumentNullException.ThrowIfNull(stations);
            var zoneMap = (zones ?? Array.Empty<StationZone>()).ToDictionary(z => z.ZoneId);

            foreach (var station in stations)
            {
                if (station is null || string.IsNullOrWhiteSpace(station.StationName))
                    continue;

                string normalizedName = NormalizeStationName(station.StationName);
                zoneMap.TryGetValue(station.ZoneId, out var zone);
                bool isTournamentLocked = string.Equals(station.Status, "TournamentLocked", StringComparison.OrdinalIgnoreCase);

                _stations[normalizedName] = new TournamentStationState
                {
                    StationId = station.StationId,
                    StationName = normalizedName,
                    ZoneName = zone?.ZoneName ?? ResolveDefaultZoneByStationNumber(station.StationId),
                    StandardHourlyRate = zone?.BasePricePerHour ?? ResolveDefaultRateByStationNumber(station.StationId),
                    Status = string.IsNullOrWhiteSpace(station.Status) ? "Available" : station.Status,
                    TournamentMode = isTournamentLocked,
                    MatchLabel = isTournamentLocked ? "Trận thi đấu giải" : null
                };
            }

            if (_stations.Count == 0)
            {
                foreach (var fallback in CreateMasterGameDefaultStations())
                    _stations[fallback.StationName] = fallback;
            }
        }

        public static List<TournamentStationState> CreateMasterGameDefaultStations()
        {
            var list = new List<TournamentStationState>(30);
            for (int i = 1; i <= 30; i++)
            {
                list.Add(new TournamentStationState
                {
                    StationId = i,
                    StationName = $"PC-{i:D2}",
                    ZoneName = ResolveDefaultZoneByStationNumber(i),
                    StandardHourlyRate = ResolveDefaultRateByStationNumber(i),
                    Status = "Available",
                    TournamentMode = false
                });
            }
            return list;
        }

        public static string ResolveDefaultZoneByStationNumber(int stationNumber) => stationNumber switch
        {
            >= 1 and <= 10 => "Standard",
            >= 11 and <= 20 => "Maximum",
            >= 21 and <= 30 => "Master",
            _ => "Standard"
        };

        public static decimal ResolveDefaultRateByStationNumber(int stationNumber) => stationNumber switch
        {
            >= 1 and <= 10 => 10000m,
            >= 11 and <= 20 => 15000m,
            >= 21 and <= 30 => 13000m,
            _ => 10000m
        };

        public static (string StartStation, string EndStation) GetZoneStationRange(string zoneName)
        {
            if (string.IsNullOrWhiteSpace(zoneName))
                throw new ArgumentException("Tên dàn máy không được để trống.", nameof(zoneName));

            return zoneName.Trim().ToUpperInvariant() switch
            {
                "STANDARD" or "DÀN STANDARD" => ("PC-01", "PC-10"),
                "MAXIMUM" or "DÀN MAXIMUM" => ("PC-11", "PC-20"),
                "MASTER" or "DÀN MASTER" => ("PC-21", "PC-30"),
                _ => throw new ArgumentException($"Không tìm thấy dàn máy '{zoneName}'. Hỗ trợ: Standard (PC-01..PC-10), Maximum (PC-11..PC-20), Master (PC-21..PC-30).", nameof(zoneName))
            };
        }

        /// <summary>
        /// Phân tích dải máy trạm theo định dạng 'PC-11' đến 'PC-20' thành danh sách tên máy đầy đủ.
        /// </summary>
        public static List<string> ParseStationRange(string startStationName, string endStationName)
        {
            int startNum = ParseStationNumber(startStationName, nameof(startStationName));
            int endNum = ParseStationNumber(endStationName, nameof(endStationName));

            if (startNum > endNum)
                throw new ArgumentException($"Dải máy không hợp lệ: máy bắt đầu ({NormalizeStationName(startStationName)}) phải nhỏ hơn hoặc bằng máy kết thúc ({NormalizeStationName(endStationName)}).");

            var range = new List<string>(endNum - startNum + 1);
            for (int num = startNum; num <= endNum; num++)
            {
                range.Add($"PC-{num:D2}");
            }
            return range;
        }

        public static string NormalizeStationName(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName))
                throw new ArgumentException("Tên máy trạm không được để trống.", nameof(stationName));

            string trimmed = stationName.Trim();
            var match = StationCodeRegex.Match(trimmed);
            if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int num) && num > 0)
            {
                return $"PC-{num:D2}";
            }

            return trimmed.ToUpperInvariant();
        }

        private static int ParseStationNumber(string stationName, string paramName)
        {
            if (string.IsNullOrWhiteSpace(stationName))
                throw new ArgumentException("Tên máy trạm không được để trống.", paramName);

            var match = StationCodeRegex.Match(stationName.Trim());
            if (!match.Success ||
                !int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number) ||
                number <= 0)
            {
                throw new ArgumentException($"Tên máy '{stationName}' không đúng định dạng PC-XX (ví dụ: PC-11).", paramName);
            }

            return number;
        }

        public IReadOnlyList<TournamentStationState> GetAllStations() =>
            _stations.Values.OrderBy(s => s.StationId).ThenBy(s => s.StationName, StringComparer.OrdinalIgnoreCase)
                .Select(s => s.Clone()).ToList();

        public TournamentStationState? GetStation(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName))
                return null;

            string key = NormalizeStationName(stationName);
            return _stations.TryGetValue(key, out var state) ? state.Clone() : null;
        }

        public TournamentStationState? GetStation(int stationId)
        {
            var found = _stations.Values.FirstOrDefault(s => s.StationId == stationId);
            return found?.Clone();
        }

        public IReadOnlyList<TournamentStationState> GetTournamentLockedStations() =>
            _stations.Values
                .Where(s => s.TournamentMode)
                .OrderBy(s => s.StationId)
                .Select(s => s.Clone())
                .ToList();

        public IReadOnlyList<TournamentClusterReservation> GetActiveClusters() =>
            _clusters.Values.Where(c => c.IsActive).OrderBy(c => c.ActivatedAt).ToList();

        /// <summary>
        /// Cập nhật trạng thái vận hành thường của một máy (Available / InUse / Maintenance).
        /// Không cho phép ghi đè máy đang khóa giải đấu khi chưa giải phóng cụm máy.
        /// </summary>
        public void SetStationStatus(string stationName, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Trạng thái máy không được để trống.", nameof(status));

            string key = NormalizeStationName(stationName);
            if (!_stations.TryGetValue(key, out var station))
                throw new InvalidOperationException($"Không tìm thấy máy trạm '{key}'.");

            if (station.TournamentMode && !string.Equals(status, "TournamentLocked", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Máy {key} đang trong chế độ thi đấu giải ({station.MatchLabel}). Vui lòng giải phóng cụm máy giải đấu trước.");

            station.Status = status.Trim();
        }

        /// <summary>
        /// Kích hoạt cụm máy thi đấu theo dải máy (ví dụ: PC-11 đến PC-20 hoặc PC-21 đến PC-30).
        /// Toàn bộ máy trong dải sẽ chuyển TournamentMode = true và Status = TournamentLocked nguyên tử (atomic).
        /// </summary>
        public TournamentClusterReservation ActivateCluster(
            string startStationName,
            string endStationName,
            string matchLabel,
            int tournamentId = 0,
            string? tournamentTitle = null,
            string? adminName = null)
        {
            var range = ParseStationRange(startStationName, endStationName);
            return ActivateCluster(range, matchLabel, tournamentId, tournamentTitle, adminName);
        }

        /// <summary>
        /// Kích hoạt cụm máy thi đấu theo tên dàn máy thực tế tại Master Game (Standard, Maximum, Master).
        /// </summary>
        public TournamentClusterReservation ActivateClusterByZone(
            string zoneName,
            string matchLabel,
            int tournamentId = 0,
            string? tournamentTitle = null,
            string? adminName = null)
        {
            var (start, end) = GetZoneStationRange(zoneName);
            return ActivateCluster(start, end, matchLabel, tournamentId, tournamentTitle, adminName);
        }

        /// <summary>
        /// Kích hoạt cụm máy thi đấu cho một danh sách máy cụ thể.
        /// Nếu bất kỳ máy nào đang có khách (InUse), đang bảo trì (Maintenance) hoặc đã khóa giải, toàn bộ thao tác sẽ bị từ chối.
        /// </summary>
        public TournamentClusterReservation ActivateCluster(
            IEnumerable<string> stationNames,
            string matchLabel,
            int tournamentId = 0,
            string? tournamentTitle = null,
            string? adminName = null)
        {
            ArgumentNullException.ThrowIfNull(stationNames);

            if (string.IsNullOrWhiteSpace(matchLabel))
                throw new ArgumentException("Nhãn trận đấu (ví dụ: 'Trận Tứ kết 1') không được để trống.", nameof(matchLabel));

            string cleanMatchLabel = matchLabel.Trim();
            if (cleanMatchLabel.Length > 120)
                throw new ArgumentException("Nhãn trận đấu không được vượt quá 120 ký tự.", nameof(matchLabel));

            var requestedNames = stationNames
                .Select(NormalizeStationName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requestedNames.Count == 0)
                throw new ArgumentException("Vui lòng chọn ít nhất một máy để kích hoạt cụm máy thi đấu.", nameof(stationNames));

            var missing = new List<string>();
            var occupied = new List<string>();
            var maintenance = new List<string>();
            var alreadyTournament = new List<string>();
            var targets = new List<TournamentStationState>(requestedNames.Count);

            foreach (string name in requestedNames)
            {
                if (!_stations.TryGetValue(name, out var station))
                {
                    missing.Add(name);
                    continue;
                }

                if (station.TournamentMode || string.Equals(station.Status, "TournamentLocked", StringComparison.OrdinalIgnoreCase))
                {
                    alreadyTournament.Add(name);
                    continue;
                }

                if (string.Equals(station.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                {
                    occupied.Add(name);
                    continue;
                }

                if (!string.Equals(station.Status, "Available", StringComparison.OrdinalIgnoreCase))
                {
                    maintenance.Add($"{name} ({station.Status})");
                    continue;
                }

                targets.Add(station);
            }

            if (missing.Count > 0)
                throw new InvalidOperationException($"Không tìm thấy máy trạm: {string.Join(", ", missing)}.");

            if (occupied.Count > 0)
                throw new InvalidOperationException($"Không thể kích hoạt cụm máy thi đấu vì máy đang có khách: {string.Join(", ", occupied)}.");

            if (alreadyTournament.Count > 0)
                throw new InvalidOperationException($"Các máy sau đã được gán cho giải đấu khác: {string.Join(", ", alreadyTournament)}.");

            if (maintenance.Count > 0)
                throw new InvalidOperationException($"Các máy sau chưa sẵn sàng để thi đấu: {string.Join(", ", maintenance)}.");

            targets = targets.OrderBy(t => t.StationId).ThenBy(t => t.StationName, StringComparer.OrdinalIgnoreCase).ToList();

            string clusterId = $"CL-{++_clusterSequence:D4}";
            DateTime now = DateTime.Now;
            string cleanTitle = string.IsNullOrWhiteSpace(tournamentTitle)
                ? "Giải đấu cộng đồng Master Game"
                : tournamentTitle.Trim();
            string cleanAdmin = string.IsNullOrWhiteSpace(adminName) ? "Admin" : adminName.Trim();

            foreach (var station in targets)
            {
                station.TournamentMode = true;
                station.Status = "TournamentLocked";
                station.TournamentId = tournamentId > 0 ? tournamentId : null;
                station.TournamentTitle = cleanTitle;
                station.MatchLabel = cleanMatchLabel;
                station.ClusterId = clusterId;
                station.LockedAt = now;
                station.LockedByAdmin = cleanAdmin;
            }

            var reservation = new TournamentClusterReservation
            {
                ClusterId = clusterId,
                TournamentId = tournamentId,
                TournamentTitle = cleanTitle,
                MatchLabel = cleanMatchLabel,
                StartStationName = targets[0].StationName,
                EndStationName = targets[^1].StationName,
                StationNames = targets.Select(t => t.StationName).ToList(),
                StationIds = targets.Select(t => t.StationId).ToList(),
                ActivatedAt = now,
                ActivatedBy = cleanAdmin,
                IsActive = true
            };

            _clusters[clusterId] = reservation;
            ClusterStatusChanged?.Invoke($"Đã kích hoạt cụm máy thi đấu {reservation.StartStationName} - {reservation.EndStationName} ({cleanMatchLabel}).");
            return reservation;
        }

        /// <summary>
        /// Hủy chế độ thi đấu giải của một cụm máy theo mã cụm (ClusterId) và trả máy về trạng thái Available.
        /// </summary>
        public bool DeactivateCluster(string clusterId)
        {
            if (string.IsNullOrWhiteSpace(clusterId) || !_clusters.TryGetValue(clusterId.Trim(), out var cluster) || !cluster.IsActive)
                return false;

            foreach (string stationName in cluster.StationNames)
            {
                if (_stations.TryGetValue(stationName, out var station) &&
                    string.Equals(station.ClusterId, cluster.ClusterId, StringComparison.OrdinalIgnoreCase))
                {
                    ResetStationToAvailable(station);
                }
            }

            cluster.IsActive = false;
            cluster.ReleasedAt = DateTime.Now;
            ClusterStatusChanged?.Invoke($"Đã giải phóng cụm máy thi đấu {cluster.StartStationName} - {cluster.EndStationName}.");
            return true;
        }

        /// <summary>
        /// Giải phóng các máy đang ở TournamentMode nằm trong dải từ startStationName đến endStationName.
        /// </summary>
        public int DeactivateRange(string startStationName, string endStationName)
        {
            var range = ParseStationRange(startStationName, endStationName);
            int releasedCount = 0;

            foreach (string name in range)
            {
                if (_stations.TryGetValue(name, out var station) && station.TournamentMode)
                {
                    ResetStationToAvailable(station);
                    releasedCount++;
                }
            }

            RefreshClusterActivityFlags();
            if (releasedCount > 0)
            {
                ClusterStatusChanged?.Invoke($"Đã giải phóng {releasedCount} máy thi đấu trong dải {range[0]} - {range[^1]}.");
            }
            return releasedCount;
        }

        /// <summary>
        /// Giải phóng toàn bộ các máy đang khóa thi đấu giải về trạng thái Available.
        /// </summary>
        public int ReleaseAllClusters()
        {
            int releasedCount = 0;
            foreach (var station in _stations.Values)
            {
                if (station.TournamentMode)
                {
                    ResetStationToAvailable(station);
                    releasedCount++;
                }
            }

            DateTime now = DateTime.Now;
            foreach (var cluster in _clusters.Values.Where(c => c.IsActive))
            {
                cluster.IsActive = false;
                cluster.ReleasedAt = now;
            }

            if (releasedCount > 0)
            {
                ClusterStatusChanged?.Invoke($"Đã giải phóng toàn bộ {releasedCount} máy thi đấu giải.");
            }
            return releasedCount;
        }

        /// <summary>
        /// Kiểm tra quyền mở máy: khách vãng lai hoặc tài khoản hội viên thông thường không thể mở máy đang bật TournamentMode.
        /// </summary>
        public TournamentStationAccessResult EvaluateStationAccess(
            string stationName,
            string? accountRole = null,
            bool isTournamentParticipant = false)
        {
            if (string.IsNullOrWhiteSpace(stationName))
            {
                return new TournamentStationAccessResult
                {
                    IsAllowed = false,
                    IsBlockedByTournamentMode = false,
                    Reason = "Tên máy trạm không hợp lệ."
                };
            }

            string key = NormalizeStationName(stationName);
            if (!_stations.TryGetValue(key, out var station))
            {
                return new TournamentStationAccessResult
                {
                    IsAllowed = false,
                    IsBlockedByTournamentMode = false,
                    Reason = $"Không tìm thấy máy trạm {key}."
                };
            }

            if (station.TournamentMode || string.Equals(station.Status, "TournamentLocked", StringComparison.OrdinalIgnoreCase))
            {
                string matchInfo = string.IsNullOrWhiteSpace(station.MatchLabel)
                    ? "Đang thi đấu giải"
                    : $"Đang thi đấu giải - {station.MatchLabel}";

                if (isTournamentParticipant)
                {
                    return new TournamentStationAccessResult
                    {
                        IsAllowed = true,
                        IsBlockedByTournamentMode = false,
                        Reason = $"Tuyển thủ được phép mở máy {key} ({matchInfo})."
                    };
                }

                string accountLabel = string.IsNullOrWhiteSpace(accountRole) ||
                                      string.Equals(accountRole, "Guest", StringComparison.OrdinalIgnoreCase)
                    ? "Khách vãng lai"
                    : $"Tài khoản thông thường ({accountRole})";

                return new TournamentStationAccessResult
                {
                    IsAllowed = false,
                    IsBlockedByTournamentMode = true,
                    Reason = $"{accountLabel} không thể mở máy {key}: {matchInfo}."
                };
            }

            if (string.Equals(station.Status, "Maintenance", StringComparison.OrdinalIgnoreCase))
            {
                return new TournamentStationAccessResult
                {
                    IsAllowed = false,
                    IsBlockedByTournamentMode = false,
                    Reason = $"Máy {key} đang bảo trì, không thể mở phiên chơi."
                };
            }

            if (string.Equals(station.Status, "InUse", StringComparison.OrdinalIgnoreCase))
            {
                return new TournamentStationAccessResult
                {
                    IsAllowed = false,
                    IsBlockedByTournamentMode = false,
                    Reason = $"Máy {key} hiện đang có khách sử dụng."
                };
            }

            return new TournamentStationAccessResult
            {
                IsAllowed = true,
                IsBlockedByTournamentMode = false,
                Reason = $"Máy {key} đang sẵn sàng mở phiên chơi."
            };
        }

        public bool CanRegularUserOpenStation(string stationName, out string reason)
        {
            var result = EvaluateStationAccess(stationName, accountRole: "Guest", isTournamentParticipant: false);
            reason = result.Reason;
            return result.IsAllowed;
        }

        public void EnsureCanOpenStation(string stationName, string? accountRole = null, bool isTournamentParticipant = false)
        {
            var result = EvaluateStationAccess(stationName, accountRole, isTournamentParticipant);
            if (!result.IsAllowed)
                throw new InvalidOperationException(result.Reason);
        }

        /// <summary>
        /// Cung cấp thông tin hiển thị (màu tím/vàng, icon cúp, nhãn 'Đang thi đấu giải - ...') cho sơ đồ phòng máy (Form1).
        /// </summary>
        public TournamentStationVisualInfo GetStationVisualInfo(string stationName)
        {
            string key = NormalizeStationName(stationName);
            if (!_stations.TryGetValue(key, out var station))
                throw new InvalidOperationException($"Không tìm thấy máy trạm '{key}'.");

            if (station.TournamentMode || string.Equals(station.Status, "TournamentLocked", StringComparison.OrdinalIgnoreCase))
            {
                string matchSuffix = string.IsNullOrWhiteSpace(station.MatchLabel)
                    ? "Giải đấu Master Game"
                    : station.MatchLabel;
                string badge = $"Đang thi đấu giải - {matchSuffix}";

                return new TournamentStationVisualInfo
                {
                    StationName = station.StationName,
                    ZoneName = station.ZoneName,
                    TournamentMode = true,
                    Icon = "🏆",
                    BadgeText = badge,
                    ButtonDisplayText = $"🏆 {station.StationName}\n({station.ZoneName})\n{badge}",
                    BackColor = TournamentBackColor,
                    BorderColor = TournamentBorderColor,
                    AccentColor = TournamentAccentGold,
                    ForeColor = TournamentForeColor,
                    AllowRegularGuestOpen = false,
                    BlockReason = $"Khách vãng lai không thể mở máy {station.StationName}: {badge}."
                };
            }

            return station.Status switch
            {
                "InUse" => new TournamentStationVisualInfo
                {
                    StationName = station.StationName,
                    ZoneName = station.ZoneName,
                    TournamentMode = false,
                    Icon = "🖥️",
                    BadgeText = "[ĐANG CHƠI]",
                    ButtonDisplayText = $"🖥️ {station.StationName}\n({station.ZoneName})\n\n[ĐANG CHƠI]",
                    BackColor = Color.FromArgb(255, 178, 102),
                    BorderColor = Color.FromArgb(230, 81, 0),
                    AccentColor = Color.FromArgb(230, 81, 0),
                    ForeColor = Color.Black,
                    AllowRegularGuestOpen = false,
                    BlockReason = $"Máy {station.StationName} đang có khách sử dụng."
                },
                "Maintenance" => new TournamentStationVisualInfo
                {
                    StationName = station.StationName,
                    ZoneName = station.ZoneName,
                    TournamentMode = false,
                    Icon = "🛠️",
                    BadgeText = "[BẢO TRÌ]",
                    ButtonDisplayText = $"🛠️ {station.StationName}\n({station.ZoneName})\n\n[BẢO TRÌ]",
                    BackColor = Color.FromArgb(255, 153, 153),
                    BorderColor = Color.FromArgb(178, 34, 34),
                    AccentColor = Color.FromArgb(178, 34, 34),
                    ForeColor = Color.Black,
                    AllowRegularGuestOpen = false,
                    BlockReason = $"Máy {station.StationName} đang trong chế độ bảo trì."
                },
                _ => new TournamentStationVisualInfo
                {
                    StationName = station.StationName,
                    ZoneName = station.ZoneName,
                    TournamentMode = false,
                    Icon = "🟢",
                    BadgeText = "[SẴN SÀNG]",
                    ButtonDisplayText = $"🟢 {station.StationName}\n({station.ZoneName})\n\n[SẴN SÀNG]",
                    BackColor = Color.FromArgb(178, 236, 178),
                    BorderColor = Color.FromArgb(34, 139, 34),
                    AccentColor = Color.FromArgb(34, 139, 34),
                    ForeColor = Color.Black,
                    AllowRegularGuestOpen = true,
                    BlockReason = string.Empty
                }
            };
        }

        /// <summary>
        /// Áp dụng trực tiếp giao diện màu sắc, viền và nhãn thi đấu giải lên nút máy trạm của Form1.
        /// </summary>
        public static void ApplyVisualStyleToButton(Button stationButton, TournamentStationVisualInfo visualInfo)
        {
            ArgumentNullException.ThrowIfNull(stationButton);
            ArgumentNullException.ThrowIfNull(visualInfo);

            stationButton.FlatStyle = FlatStyle.Flat;
            stationButton.BackColor = visualInfo.BackColor;
            stationButton.ForeColor = visualInfo.ForeColor;
            stationButton.FlatAppearance.BorderSize = visualInfo.TournamentMode ? 3 : 2;
            stationButton.FlatAppearance.BorderColor = visualInfo.BorderColor;
            stationButton.Text = visualInfo.ButtonDisplayText;
        }

        /// <summary>
        /// Đồng bộ trạng thái TournamentLocked / Available từ Engine sang danh sách đối tượng ComputerStation.
        /// </summary>
        public void SyncToComputerStations(IEnumerable<ComputerStation> stations)
        {
            ArgumentNullException.ThrowIfNull(stations);
            foreach (var station in stations)
            {
                if (station is null || string.IsNullOrWhiteSpace(station.StationName))
                    continue;

                string key = NormalizeStationName(station.StationName);
                if (_stations.TryGetValue(key, out var state))
                {
                    station.Status = state.TournamentMode ? "TournamentLocked" : state.Status;
                }
            }
        }

        private static void ResetStationToAvailable(TournamentStationState station)
        {
            station.TournamentMode = false;
            station.Status = "Available";
            station.TournamentId = null;
            station.TournamentTitle = null;
            station.MatchLabel = null;
            station.ClusterId = null;
            station.LockedAt = null;
            station.LockedByAdmin = null;
        }

        private void RefreshClusterActivityFlags()
        {
            foreach (var cluster in _clusters.Values.Where(c => c.IsActive))
            {
                bool stillHasLocked = cluster.StationNames.Any(name =>
                    _stations.TryGetValue(name, out var s) &&
                    s.TournamentMode &&
                    string.Equals(s.ClusterId, cluster.ClusterId, StringComparison.OrdinalIgnoreCase));

                if (!stillHasLocked)
                {
                    cluster.IsActive = false;
                    cluster.ReleasedAt = DateTime.Now;
                }
            }
        }
    }

    /// <summary>
    /// Lớp quản lý tổng hợp chế độ cụm máy thi đấu giải đấu cho Master Game,
    /// kết nối TournamentMachineLockEngine với DAL (DBTournament) và hỗ trợ hiển thị trên Form1.
    /// </summary>
    public class TournamentStationManager
    {
        private readonly DBTournament? _dbTournament;

        public static TournamentStationManager Shared { get; } = new();

        public TournamentMachineLockEngine Engine { get; }

        public TournamentStationManager(TournamentMachineLockEngine? engine = null, DBTournament? dbTournament = null)
        {
            Engine = engine ?? new TournamentMachineLockEngine();
            _dbTournament = dbTournament;
        }

        /// <summary>
        /// Kích hoạt cụm máy thi đấu theo dải máy (ví dụ: PC-11 đến PC-20) cho một trận đấu giải.
        /// </summary>
        public TournamentClusterReservation ActivateTournamentCluster(
            string startStationName,
            string endStationName,
            string matchLabel,
            int tournamentId = 0,
            string? tournamentTitle = null,
            string? adminName = null,
            bool syncDatabase = false)
        {
            var reservation = Engine.ActivateCluster(
                startStationName,
                endStationName,
                matchLabel,
                tournamentId,
                tournamentTitle,
                adminName);

            if (syncDatabase && _dbTournament != null && reservation.StationIds.Count > 0)
            {
                _dbTournament.LockTournamentStations(reservation.StationIds.ToArray());
            }

            return reservation;
        }

        /// <summary>
        /// Kích hoạt toàn bộ một dàn máy thi đấu (Standard, Maximum hoặc Master).
        /// </summary>
        public TournamentClusterReservation ActivateZoneCluster(
            string zoneName,
            string matchLabel,
            int tournamentId = 0,
            string? tournamentTitle = null,
            string? adminName = null,
            bool syncDatabase = false)
        {
            var reservation = Engine.ActivateClusterByZone(
                zoneName,
                matchLabel,
                tournamentId,
                tournamentTitle,
                adminName);

            if (syncDatabase && _dbTournament != null && reservation.StationIds.Count > 0)
            {
                _dbTournament.LockTournamentStations(reservation.StationIds.ToArray());
            }

            return reservation;
        }

        /// <summary>
        /// Tắt chế độ thi đấu giải của một cụm máy theo ClusterId và trả máy về Available.
        /// </summary>
        public bool DeactivateTournamentCluster(string clusterId, bool syncDatabase = false)
        {
            var activeCluster = Engine.GetActiveClusters()
                .FirstOrDefault(c => string.Equals(c.ClusterId, clusterId, StringComparison.OrdinalIgnoreCase));

            bool released = Engine.DeactivateCluster(clusterId);
            if (released && syncDatabase && _dbTournament != null && activeCluster != null && activeCluster.StationIds.Count > 0)
            {
                _dbTournament.UnlockTournamentStations(activeCluster.StationIds.ToArray());
            }

            return released;
        }

        /// <summary>
        /// Giải phóng dải máy đang khóa giải đấu về trạng thái sẵn sàng cho khách thường.
        /// </summary>
        public int ReleaseStationRange(string startStationName, string endStationName, bool syncDatabase = false)
        {
            var names = TournamentMachineLockEngine.ParseStationRange(startStationName, endStationName);
            var idsToUnlock = names
                .Select(n => Engine.GetStation(n))
                .Where(s => s != null && s.TournamentMode)
                .Select(s => s!.StationId)
                .ToArray();

            int released = Engine.DeactivateRange(startStationName, endStationName);
            if (released > 0 && syncDatabase && _dbTournament != null && idsToUnlock.Length > 0)
            {
                _dbTournament.UnlockTournamentStations(idsToUnlock);
            }

            return released;
        }

        public bool IsStationInTournamentMode(string stationName) =>
            Engine.GetStation(stationName)?.TournamentMode ?? false;

        public bool CanGuestOrRegularAccountOpen(string stationName, out string reason) =>
            Engine.CanRegularUserOpenStation(stationName, out reason);

        public TournamentStationAccessResult CheckAccountAccess(
            string stationName,
            string? accountRole = null,
            bool isTournamentParticipant = false) =>
            Engine.EvaluateStationAccess(stationName, accountRole, isTournamentParticipant);

        public void EnsureStationAccessForAccount(
            string stationName,
            string? accountRole = null,
            bool isTournamentParticipant = false) =>
            Engine.EnsureCanOpenStation(stationName, accountRole, isTournamentParticipant);

        public TournamentStationVisualInfo GetStationVisualInfo(string stationName) =>
            Engine.GetStationVisualInfo(stationName);

        public void ApplyVisualStyleToButton(Button stationButton, string stationName) =>
            TournamentMachineLockEngine.ApplyVisualStyleToButton(stationButton, Engine.GetStationVisualInfo(stationName));

        public IReadOnlyList<TournamentClusterReservation> GetActiveReservations() =>
            Engine.GetActiveClusters();

        public IReadOnlyList<TournamentStationState> GetLockedStations() =>
            Engine.GetTournamentLockedStations();
    }
}
