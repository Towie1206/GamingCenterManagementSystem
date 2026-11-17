using System;
using System.Collections.Generic;

namespace GamingCenterManagementSystem.Models
{
    public class TournamentPrizeAllocation
    {
        public int Placement { get; set; } // 1: Vô địch, 2: Á quân, 3: Hạng ba
        public string Title { get; set; } = string.Empty;
        public double Percentage { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Dịch vụ tính toán phân bổ quỹ giải thưởng (Prize Pool) cho giải đấu Cyber Game.
    /// </summary>
    public static class TournamentPrizeService
    {
        public static List<TournamentPrizeAllocation> CalculatePrizeDistribution(
            decimal totalPrizePool,
            double top1Pct = 50.0,
            double top2Pct = 30.0,
            double top3Pct = 20.0)
        {
            if (totalPrizePool <= 0)
                throw new ArgumentException("Tổng quỹ giải thưởng phải lớn hơn 0 VNĐ.");

            if (Math.Abs((top1Pct + top2Pct + top3Pct) - 100.0) > 0.001)
                throw new ArgumentException("Tổng tỷ lệ phần trăm phân bổ phải bằng đúng 100%.");

            return new List<TournamentPrizeAllocation>
            {
                new() { Placement = 1, Title = "🥇 Quán Quân (Vô Địch)", Percentage = top1Pct, Amount = Math.Round(totalPrizePool * (decimal)(top1Pct / 100.0), 0) },
                new() { Placement = 2, Title = "🥈 Á Quân (Hạng Nhì)", Percentage = top2Pct, Amount = Math.Round(totalPrizePool * (decimal)(top2Pct / 100.0), 0) },
                new() { Placement = 3, Title = "🥉 Quý Quân (Hạng Ba)", Percentage = top3Pct, Amount = Math.Round(totalPrizePool * (decimal)(top3Pct / 100.0), 0) }
            };
        }
    }
}
