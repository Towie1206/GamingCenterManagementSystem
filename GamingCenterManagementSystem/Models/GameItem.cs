using System;

namespace GamingCenterManagementSystem.Models
{
    public class GameItem
    {
        public string GameId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Esports / MOBA, Bắn súng FPS, Sinh tồn / Nhập vai, Thể thao
        public string Publisher { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Sẵn sàng"; // Sẵn sàng, Đang chạy, Đang cập nhật
        public string IconEmoji { get; set; } = "🎮";
        public string Aliases { get; set; } = string.Empty;
        public DateTime? LastPlayed { get; set; }

        public override string ToString() => $"{Name} ({Category}) - {Status}";
    }
}
