using System;

namespace GamingCenterManagementSystem.OOP
{
    // 1. Tính cước giờ thường theo phút
    public class HourlyTariff : TariffCalculatorBase
    {
        public override string TariffName => "Cước Theo Giờ Thường";

        public override decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour)
        {
            double totalHours = CalculateTotalHours(startTime, endTime);
            return Math.Round((decimal)totalHours * basePricePerHour, 0);
        }
    }

    // 2. Gói cước Combo đêm trọn gói (22h - 6h sáng: 45k)
    public class NightComboTariff : TariffCalculatorBase
    {
        public override string TariffName => "Gói Combo Đêm Trọn Gói";
        private const decimal NightComboFixedPrice = 45000;

        public override decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour)
        {
            if (endTime.Hour > 6 && endTime.Hour < 22)
            {
                DateTime morningLimit = new DateTime(endTime.Year, endTime.Month, endTime.Day, 6, 0, 0);
                double extraHours = (endTime - morningLimit).TotalHours;
                return NightComboFixedPrice + Math.Round((decimal)extraHours * basePricePerHour, 0);
            }
            return NightComboFixedPrice;
        }
    }

    // 3. Gói cước Giải đấu LAN Party (Giảm 50% tiền máy cho tuyển thủ)
    public class TournamentTariff : TariffCalculatorBase
    {
        public override string TariffName => "Gói Cước Ưu Đãi Giải Đấu";

        public override decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour)
        {
            double totalHours = CalculateTotalHours(startTime, endTime);
            decimal standardCost = (decimal)totalHours * basePricePerHour;
            return Math.Round(standardCost * 0.5m, 0);
        }
    }
}
