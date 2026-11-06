namespace GamingCenterManagementSystem.Models
{
    public class StationZone
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public decimal BasePricePerHour { get; set; }
        public string? Description { get; set; }
    }
}
