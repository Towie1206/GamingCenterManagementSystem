using System;
using System.Globalization;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Kết quả tính khuyến mãi cho một lần nạp ví.
    /// CashAmount     = tiền mặt thu ngân nhận (GiaTriNap).
    /// BonusAmount    = tiền quán tặng thêm (TienKhuyenMai).
    /// TotalCredited  = tổng tiền thực tế vào ví hội viên.
    /// </summary>
    public readonly record struct DepositPromotionResult(
        decimal CashAmount,
        decimal BonusPercent,
        decimal BonusAmount,
        decimal TotalCredited)
    {
        public bool HasBonus => BonusAmount > 0;
    }

    /// <summary>
    /// Chính sách thưởng nạp ví của Master Game:
    ///   &lt; 500.000đ            : 0%
    ///   500.000đ  - 999.999đ    : +20%
    ///   1.000.000đ - 1.999.999đ : +50%
    ///   &gt;= 2.000.000đ          : +100%
    /// </summary>
    public static class DepositPromotionEngine
    {
        public const decimal Tier20Minimum = 500_000m;
        public const decimal Tier50Minimum = 1_000_000m;
        public const decimal Tier100Minimum = 2_000_000m;

        /// <summary>Tên hình thức ghi vào giao dịch ví cho phần tiền khuyến mãi.</summary>
        public const string BonusPaymentMethod = "Khuyến mãi nạp ví";

        // Sắp xếp tăng dần theo mốc tiền.
        private static readonly (decimal Minimum, decimal Percent)[] Tiers =
        {
            (Tier20Minimum, 20m),
            (Tier50Minimum, 50m),
            (Tier100Minimum, 100m)
        };

        /// <summary>Trả về % thưởng áp dụng cho số tiền nạp (0 nếu chưa đạt mốc).</summary>
        public static decimal GetBonusPercent(decimal cashAmount)
        {
            if (cashAmount <= 0)
                return 0m;

            for (int i = Tiers.Length - 1; i >= 0; i--)
            {
                if (cashAmount >= Tiers[i].Minimum)
                    return Tiers[i].Percent;
            }

            return 0m;
        }

        /// <summary>Tính tiền thưởng và tổng tiền vào ví cho số tiền nạp.</summary>
        public static DepositPromotionResult Calculate(decimal cashAmount)
        {
            if (cashAmount <= 0)
                return new DepositPromotionResult(0m, 0m, 0m, 0m);

            decimal percent = GetBonusPercent(cashAmount);
            decimal bonus = decimal.Round(cashAmount * percent / 100m, 2, MidpointRounding.AwayFromZero);
            return new DepositPromotionResult(cashAmount, percent, bonus, cashAmount + bonus);
        }

        /// <summary>
        /// Nhãn preview hiển thị trên giao diện nạp tiền:
        /// "Thưởng khuyến mãi: +X đ (Tỷ lệ: Y%) => Tổng tiền vào ví: Z đ".
        /// </summary>
        public static string BuildPreviewText(decimal cashAmount, IFormatProvider? provider = null)
        {
            provider ??= CultureInfo.CurrentCulture;
            var result = Calculate(cashAmount);
            return $"Thưởng khuyến mãi: +{result.BonusAmount.ToString("N0", provider)} đ " +
                   $"(Tỷ lệ: {result.BonusPercent.ToString("0.##", provider)}%) => " +
                   $"Tổng tiền vào ví: {result.TotalCredited.ToString("N0", provider)} đ";
        }

        /// <summary>
        /// Gợi ý cho thu ngân: cần nạp thêm bao nhiêu để lên mốc thưởng kế tiếp.
        /// Trả về chuỗi rỗng nếu số tiền không hợp lệ.
        /// </summary>
        public static string BuildNextTierHint(decimal cashAmount, IFormatProvider? provider = null)
        {
            if (cashAmount <= 0)
                return string.Empty;

            provider ??= CultureInfo.CurrentCulture;

            foreach (var (minimum, percent) in Tiers)
            {
                if (cashAmount < minimum)
                {
                    return $"Nạp thêm {(minimum - cashAmount).ToString("N0", provider)} đ " +
                           $"để nhận thưởng {percent.ToString("0.##", provider)}%";
                }
            }

            return $"Đã đạt mốc thưởng cao nhất ({Tiers[^1].Percent.ToString("0.##", provider)}%)";
        }
    }
}