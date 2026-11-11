using System;

namespace GamingCenterManagementSystem.OOP
{
    public class MemberTierTariff : TariffCalculatorBase
    {
        public string MemberTier { get; }
        public decimal DiscountRate { get; }

        public MemberTierTariff() : this("Gold")
        {
        }

        public MemberTierTariff(string? memberTier)
        {
            MemberTier = NormalizeTier(memberTier);
            DiscountRate = GetDefaultDiscountRate(MemberTier);
        }

        public MemberTierTariff(string? memberTier, decimal discountRate)
        {
            if (discountRate < 0m || discountRate > 1m)
                throw new ArgumentOutOfRangeException(nameof(discountRate), "Tỉ lệ chiết khấu phải nằm trong khoảng từ 0 đến 1.");

            MemberTier = NormalizeTier(memberTier);
            DiscountRate = discountRate;
        }

        public override string TariffName => $"Gói Ưu Đãi Hội Viên ({MemberTier} - Giảm {DiscountRate * 100m:0.##}%)";

        public static string NormalizeTier(string? memberTier)
        {
            if (string.IsNullOrWhiteSpace(memberTier))
                return "Standard";

            string trimmed = memberTier.Trim();
            if (trimmed.Equals("Gold", StringComparison.OrdinalIgnoreCase))
                return "Gold";
            if (trimmed.Equals("Silver", StringComparison.OrdinalIgnoreCase))
                return "Silver";
            if (trimmed.Equals("Standard", StringComparison.OrdinalIgnoreCase))
                return "Standard";

            throw new ArgumentException("Hạng hội viên không hợp lệ (Standard, Silver, Gold).", nameof(memberTier));
        }

        public static decimal GetDefaultDiscountRate(string? memberTier)
        {
            return NormalizeTier(memberTier) switch
            {
                "Gold" => 0.15m,
                "Silver" => 0.10m,
                _ => 0.00m
            };
        }

        public override decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour)
        {
            if (basePricePerHour < 0m)
                throw new ArgumentOutOfRangeException(nameof(basePricePerHour), "Đơn giá giờ không được âm.");

            double totalHours = CalculateTotalHours(startTime, endTime);
            decimal standardCost = (decimal)totalHours * basePricePerHour;
            return Math.Round(standardCost * (1m - DiscountRate), 0);
        }
    }
}
