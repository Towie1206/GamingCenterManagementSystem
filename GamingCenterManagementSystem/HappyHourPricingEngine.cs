using System;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public enum StationTier
    {
        Standard,
        Master,
        Maximum
    }

    public class HappyHourQuote
    {
        public StationTier Tier { get; set; }
        public string ZoneDisplayName { get; set; } = string.Empty;
        public decimal StandardPricePerHour { get; set; }
        public decimal AppliedPricePerHour { get; set; }
        public bool IsHappyHourActive { get; set; }
        public TimeSpan RemainingHappyHour { get; set; }
        public string BadgeText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bộ tính cước Happy Hours tự động (23:00 đêm đến 11:00 trưa hôm sau, giảm 40% theo chính sách Master Game).
    /// </summary>
    public class HappyHourPricingEngine
    {
        public const decimal StandardBasePrice = 10000m;
        public const decimal MasterBasePrice = 13000m;
        public const decimal MaximumBasePrice = 15000m;

        public const decimal StandardHappyHourPrice = 6000m;
        public const decimal MasterHappyHourPrice = 8000m;
        public const decimal MaximumHappyHourPrice = 9000m;

        /// <summary>
        /// Kiểm tra thời điểm truyền vào có thuộc khung giờ Happy Hours (23:00 - 11:00) hay không.
        /// Từ 11:01 trở đi đến 22:59 sẽ trả về giá gốc.
        /// </summary>
        public static bool IsHappyHour(DateTime timestamp)
        {
            var time = timestamp.TimeOfDay;
            var startNight = new TimeSpan(23, 0, 0);
            var endMorning = new TimeSpan(11, 0, 59); // Hết phút 11:00, sang 11:01 là hết Happy Hours

            return time >= startNight || time <= endMorning;
        }

        /// <summary>
        /// Tính thời gian còn lại của khung giờ Happy Hours tại thời điểm kiểm tra.
        /// </summary>
        public static TimeSpan GetRemainingHappyHour(DateTime timestamp)
        {
            if (!IsHappyHour(timestamp))
            {
                return TimeSpan.Zero;
            }

            DateTime endWindow = timestamp.Hour >= 23
                ? timestamp.Date.AddDays(1).AddHours(11)
                : timestamp.Date.AddHours(11);

            return endWindow > timestamp ? endWindow - timestamp : TimeSpan.Zero;
        }

        /// <summary>
        /// Xác định hạng dàn máy dựa theo tên khu vực hoặc đơn giá gốc.
        /// </summary>
        public static StationTier ResolveTier(string? zoneName, decimal basePrice = 0m)
        {
            string normalized = (zoneName ?? string.Empty).Trim();

            if (normalized.Contains("Maximum", StringComparison.OrdinalIgnoreCase) || basePrice >= 15000m)
                return StationTier.Maximum;

            if (normalized.Contains("Master", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("VIP", StringComparison.OrdinalIgnoreCase) ||
                (basePrice >= 12000m && basePrice < 15000m))
                return StationTier.Master;

            return StationTier.Standard;
        }

        /// <summary>
        /// Lấy đơn giá áp dụng theo hạng dàn máy và thời điểm hiện tại.
        /// </summary>
        public static decimal GetHourlyRate(StationTier tier, DateTime timestamp)
        {
            bool active = IsHappyHour(timestamp);

            return tier switch
            {
                StationTier.Standard => active ? StandardHappyHourPrice : StandardBasePrice,
                StationTier.Master => active ? MasterHappyHourPrice : MasterBasePrice,
                StationTier.Maximum => active ? MaximumHappyHourPrice : MaximumBasePrice,
                _ => active ? StandardHappyHourPrice : StandardBasePrice
            };
        }

        /// <summary>
        /// Tính toán chi tiết bảng giá và thông tin hiển thị badge Happy Hours cho khu vực máy trạm.
        /// </summary>
        public HappyHourQuote EvaluateZoneQuote(StationZone zone, DateTime? now = null)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));

            DateTime checkTime = now ?? DateTime.Now;
            StationTier tier = ResolveTier(zone.ZoneName, zone.BasePricePerHour);
            bool active = IsHappyHour(checkTime);

            decimal standardPrice = tier switch
            {
                StationTier.Standard => StandardBasePrice,
                StationTier.Master => MasterBasePrice,
                StationTier.Maximum => MaximumBasePrice,
                _ => zone.BasePricePerHour > 0 ? zone.BasePricePerHour : StandardBasePrice
            };

            decimal appliedPrice = GetHourlyRate(tier, checkTime);
            TimeSpan remaining = GetRemainingHappyHour(checkTime);

            string badge = active
                ? $"⚡ Happy Hours (-40%) | Còn lại: {(int)remaining.TotalHours:D2}h {remaining.Minutes:D2}p"
                : "☀️ Giờ tiêu chuẩn (Happy Hours: 23:00 - 11:00)";

            return new HappyHourQuote
            {
                Tier = tier,
                ZoneDisplayName = zone.ZoneName,
                StandardPricePerHour = standardPrice,
                AppliedPricePerHour = appliedPrice,
                IsHappyHourActive = active,
                RemainingHappyHour = remaining,
                BadgeText = badge
            };
        }
    }
}