using System;

namespace GamingCenterManagementSystem.OOP
{
    public class HourlyTariff : TariffCalculatorBase
    {
        public override string TariffName => "Cước Theo Giờ Thường";

        public override decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour)
        {
            double totalHours = CalculateTotalHours(startTime, endTime);
            return Math.Round((decimal)totalHours * basePricePerHour, 0);
        }
    }
}
