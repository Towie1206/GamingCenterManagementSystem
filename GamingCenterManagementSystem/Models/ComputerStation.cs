namespace GamingCenterManagementSystem.Models
{
    public class ComputerStation
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int ZoneId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Available"; // Available, InUse, Maintenance, TournamentLocked

        // Encapsulation helper
        public bool IsAvailable() => Status == "Available";
    }
}
