using System;

namespace GamingCenterManagementSystem.OOP
{
    public class NightComboTariff : TariffCalculatorBase
    {
        public override string TariffName => "Gói Combo Đêm Trọn Gói";
        private const decimal NightComboFixedPrice = 45000;

        public override decimal CalculateCost(DateTime startTime, DateTime endTime, decimal basePricePerHour)
        {
            DateTime morningLimit = new DateTime(endTime.Year, endTime.Month, endTime.Day, 6, 0, 0);

            if (endTime > morningLimit && endTime.Hour < 22)
            {
                double extraHours = (endTime - morningLimit).TotalHours;
                return NightComboFixedPrice + Math.Round((decimal)extraHours * basePricePerHour, 0);
            }
            return NightComboFixedPrice;
        }
    }
}
