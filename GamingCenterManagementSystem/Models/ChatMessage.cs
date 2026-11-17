using System;
using System.Threading;

namespace GamingCenterManagementSystem.Models
{
    /// <summary>
    /// Model tin nhắn trao đổi 2 chiều giữa Khách tại máy trạm và Thu ngân/Kỹ thuật viên tại quầy.
    /// </summary>
    public class ChatMessage
    {
        private static int _idCounter = 0;

        public int MessageId { get; init; }
        public int StationId { get; init; }
        public string StationName { get; init; } = string.Empty;
        public string SenderRole { get; init; } = "Member"; // "Member" or "Staff"
        public string SenderUsername { get; init; } = string.Empty;
        public string SenderDisplayName { get; init; } = string.Empty;
        public string MessageText { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public bool IsReadByStaff { get; set; }
        public bool IsReadByMember { get; set; }
        public string? QuickTag { get; init; }

        public bool IsSentByStaff => string.Equals(SenderRole, "Staff", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(SenderRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(SenderRole, "Cashier", StringComparison.OrdinalIgnoreCase);
        public bool IsSentByMember => !IsSentByStaff;
        public string FormattedTime => Timestamp.ToString("HH:mm:ss");

        public ChatMessage()
        {
            MessageId = Interlocked.Increment(ref _idCounter);
        }

        public ChatMessage(
            int stationId,
            string stationName,
            string senderRole,
            string senderUsername,
            string senderDisplayName,
            string messageText,
            string? quickTag = null)
        {
            if (stationId <= 0)
                throw new ArgumentOutOfRangeException(nameof(stationId), "Mã máy trạm phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(messageText))
                throw new ArgumentException("Nội dung tin nhắn không được để trống.", nameof(messageText));

            MessageId = Interlocked.Increment(ref _idCounter);
            StationId = stationId;
            StationName = string.IsNullOrWhiteSpace(stationName) ? $"PC-{stationId:D2}" : stationName.Trim();
            SenderRole = string.IsNullOrWhiteSpace(senderRole) ? "Member" : senderRole.Trim();
            SenderUsername = senderUsername?.Trim() ?? string.Empty;
            SenderDisplayName = string.IsNullOrWhiteSpace(senderDisplayName)
                ? (string.IsNullOrWhiteSpace(senderUsername) ? SenderRole : senderUsername)
                : senderDisplayName.Trim();
            MessageText = messageText.Trim();
            Timestamp = DateTime.Now;
            QuickTag = quickTag?.Trim();

            if (IsSentByStaff)
            {
                IsReadByStaff = true;
                IsReadByMember = false;
            }
            else
            {
                IsReadByMember = true;
                IsReadByStaff = false;
            }
        }

        public static void ResetIdCounter(int startValue = 0)
        {
            Interlocked.Exchange(ref _idCounter, startValue);
        }

        public string GetDisplayHeader()
        {
            if (IsSentByStaff)
            {
                string rolePrefix = string.Equals(SenderRole, "Admin", StringComparison.OrdinalIgnoreCase)
                    ? "Quản Lý"
                    : "Thu Ngân";
                return $"[{rolePrefix} - {SenderDisplayName}] ({FormattedTime})";
            }

            return $"[{StationName} - {SenderDisplayName}] ({FormattedTime})";
        }
    }
}
