using System;

namespace GamingCenterManagementSystem.Models
{
    /// <summary>
    /// Quản lý cơ chế tích điểm đổi quà và voucher giờ chơi cho hội viên.
    /// </summary>
    public class LoyaltyReward
    {
        public const decimal BasePointsRate = 1000m; // 1,000 VNĐ nạp = 1 Điểm

        /// <summary>
        /// Tính số điểm tích lũy được dựa trên số tiền nạp và cấp bậc thành viên.
        /// </summary>
        public static int CalculatePoints(decimal topUpAmount, string userRole)
        {
            if (topUpAmount <= 0) return 0;
            int basePoints = (int)(topUpAmount / BasePointsRate);

            // Nhân hệ số tích điểm theo hạng thành viên
            double multiplier = userRole switch
            {
                "VIP" => 1.5,
                "Tournament" => 2.0,
                _ => 1.0
            };

            return (int)Math.Round(basePoints * multiplier);
        }

        /// <summary>
        /// Quy đổi điểm thưởng ra tiền cộng vào ví tài khoản (1 điểm = 100 VNĐ).
        /// </summary>
        public static decimal RedeemPointsToCash(int points, decimal pointToCashRate = 100m)
        {
            if (points < 50) return 0m; // Tối thiểu 50 điểm mới được đổi
            return points * pointToCashRate;
        }
    }
}
