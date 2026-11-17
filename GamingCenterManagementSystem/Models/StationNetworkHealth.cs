using System;

namespace GamingCenterManagementSystem.Models
{
    public enum NetworkHealthStatus
    {
        Good,       // Latency < 30ms (Mạng mượt, tối ưu cho game FPS)
        Warning,    // Latency 30 - 80ms (Độ trễ trung bình)
        Critical,   // Latency > 80ms hoặc rớt gói tin
        Offline     // Mất kết nối / Timeout
    }

    /// <summary>
    /// Model quản lý trạng thái kết nối mạng và độ trễ (Ping) của máy trạm.
    /// </summary>
    public class StationNetworkHealth
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int LatencyMs { get; set; }
        public double PacketLossPercent { get; set; }
        public NetworkHealthStatus Status { get; set; }
        public DateTime LastCheckedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Phân loại tình trạng mạng dựa trên độ trễ Ping (ms) và tỷ lệ rớt gói tin.
        /// </summary>
        public static NetworkHealthStatus EvaluateStatus(int latencyMs, double packetLossPercent)
        {
            if (latencyMs < 0 || packetLossPercent >= 100.0)
                return NetworkHealthStatus.Offline;
            if (latencyMs > 80 || packetLossPercent > 5.0)
                return NetworkHealthStatus.Critical;
            if (latencyMs >= 30 || packetLossPercent > 0.0)
                return NetworkHealthStatus.Warning;
            return NetworkHealthStatus.Good;
        }

        /// <summary>
        /// Trả về chuỗi hiển thị trạng thái kèm icon trực quan.
        /// </summary>
        public string GetStatusBadge() => Status switch
        {
            NetworkHealthStatus.Good => "🟢 Ổn định",
            NetworkHealthStatus.Warning => "🟡 Chậm",
            NetworkHealthStatus.Critical => "🔴 Lag cao",
            NetworkHealthStatus.Offline => "⚪ Mất mạng",
            _ => "❓ Không rõ"
        };
    }
}