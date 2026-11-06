using System;

namespace GamingCenterManagementSystem.Models
{
    public class GamingSession
    {
        public int SessionId { get; set; }
        public int StationId { get; set; }
        public int? UserId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string TariffType { get; set; } = "Hourly";
        public decimal PlayCost { get; set; } = 0;
        public decimal ServiceCost { get; set; } = 0;
        public decimal TotalCost { get; set; } = 0;
        public string Status { get; set; } = "Active"; // Active, Completed
    }
}
