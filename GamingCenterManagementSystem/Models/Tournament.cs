using System;

namespace GamingCenterManagementSystem.Models
{
    public class Tournament
    {
        public int TournamentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public string FormatType { get; set; } = "SingleElimination";
        public decimal PrizePool { get; set; } = 0;
        public string Status { get; set; } = "Upcoming"; // Upcoming, Ongoing, Finished
        public DateTime StartDate { get; set; } = DateTime.Now;
    }
}
