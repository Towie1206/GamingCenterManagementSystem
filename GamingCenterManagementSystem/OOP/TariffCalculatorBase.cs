using System;

namespace GamingCenterManagementSystem.OOP
{
    // ABSTRACT CLASS - Cốt lõi tính chất trừu tượng và kế thừa
    public abstract class TariffCalculatorBase
    {
        public abstract string TariffName { get; }

        // Phương thức trừu tượng: Các lớp con BẮT BUỘC phải override tính tiền theo thuật toán riêng (ĐA HÌNH)
        public abstract decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour);

        // Phương thức ảo có thể dùng chung
        public virtual double CalculateTotalHours(DateTime startTime, DateTime endTime)
        {
            TimeSpan duration = endTime - startTime;
            // Tối thiểu tính 0.1 giờ (6 phút)
            return Math.Max(0.1, duration.TotalHours);
        }
    }
}
