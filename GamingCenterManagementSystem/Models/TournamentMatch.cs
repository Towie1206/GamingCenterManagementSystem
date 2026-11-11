namespace GamingCenterManagementSystem.Models
{
    public class TournamentMatch
    {
        public int MatchId { get; set; }
        public int TournamentId { get; set; }
        public int RoundNumber { get; set; } = 1;
        public string Team1Name { get; set; } = string.Empty;
        public string Team2Name { get; set; } = string.Empty;
        public string? WinnerTeamName { get; set; }
        public string? Score { get; set; }
        public string? AssignedStationIds { get; set; } // e.g. "PC-06,PC-07,PC-08,PC-09,PC-10"
        public string Status { get; set; } = "Pending"; // Pending, Playing, Finished
    }
}
