using System;

namespace GamingCenterManagementSystem.OOP
{
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
