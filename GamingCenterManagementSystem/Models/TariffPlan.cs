namespace GamingCenterManagementSystem.Models
{
    public class TariffPlan
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string TariffType { get; set; } = "Hourly"; // Hourly, NightCombo, Tournament, MemberTier
        public decimal? FixedPrice { get; set; }
        public double DiscountRate { get; set; } = 0;
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }
    }
}
