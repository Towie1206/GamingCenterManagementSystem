using System;
using System.Collections.Generic;
using System.Linq;

namespace GamingCenterManagementSystem.Models
{
    /// <summary>
    /// Service quản lý hệ thống Chatbox 2 chiều thời gian thực giữa Khách máy trạm và Quầy thu ngân.
    /// Thread-safe in-memory store hỗ trợ quick reply templates, unread badges và events.
    /// </summary>
    public static class StationChatService
    {
        private static readonly List<ChatMessage> _messages = new();
        private static readonly object _lock = new();

        public static event Action<ChatMessage>? OnMessageSent;
        public static event Action<int>? OnUnreadCountChanged; // Param: stationId

        public static readonly IReadOnlyList<string> MemberQuickTemplates = new List<string>
        {
            "Xin thêm nước lọc/đá",
            "Nạp 50k ví",
            "Kẹt phím/chuột",
            "Chỉnh điều hòa",
            "Bao lâu nữa có đồ ăn ạ?"
        }.AsReadOnly();

        public static readonly IReadOnlyList<string> StaffQuickTemplates = new List<string>
        {
            "Nhân viên đang mang qua ngay",
            "Đã nạp tiền thành công nhé",
            "Kỹ thuật đang đến hỗ trợ",
            "Bếp đang làm đồ, đợi 2 phút nhé"
        }.AsReadOnly();

        public static ChatMessage SendMessage(
            int stationId,
            string stationName,
            string senderRole,
            string senderUsername,
            string senderDisplayName,
            string messageText,
            string? quickTag = null)
        {
            var msg = new ChatMessage(
                stationId,
                stationName,
                senderRole,
                senderUsername,
                senderDisplayName,
                messageText,
                quickTag);

            return SendMessage(msg);
        }

        public static ChatMessage SendMessage(ChatMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            lock (_lock)
            {
                _messages.Add(message);
            }

            try
            {
                OnMessageSent?.Invoke(message);
            }
            catch
            {
                // Suppress subscriber errors
            }

            try
            {
                OnUnreadCountChanged?.Invoke(message.StationId);
            }
            catch
            {
                // Suppress subscriber errors
            }

            return message;
        }

        public static IReadOnlyList<ChatMessage> GetMessages(int stationId)
        {
            lock (_lock)
            {
                return _messages
                    .Where(m => m.StationId == stationId)
                    .OrderBy(m => m.Timestamp)
                    .ToList();
            }
        }

        public static IReadOnlyList<ChatMessage> GetAllMessages()
        {
            lock (_lock)
            {
                return _messages.OrderBy(m => m.Timestamp).ToList();
            }
        }

        public static int GetUnreadCountForStaff(int? stationId = null)
        {
            lock (_lock)
            {
                var query = _messages.Where(m => m.IsSentByMember && !m.IsReadByStaff);
                if (stationId.HasValue)
                {
                    query = query.Where(m => m.StationId == stationId.Value);
                }
                return query.Count();
            }
        }

        public static int GetUnreadCountForStation(int stationId)
        {
            lock (_lock)
            {
                return _messages.Count(m => m.StationId == stationId && m.IsSentByStaff && !m.IsReadByMember);
            }
        }

        public static int MarkAsReadByStaff(int stationId)
        {
            int count = 0;
            lock (_lock)
            {
                foreach (var msg in _messages.Where(m => m.StationId == stationId && m.IsSentByMember && !m.IsReadByStaff))
                {
                    msg.IsReadByStaff = true;
                    count++;
                }
            }

            if (count > 0)
            {
                try
                {
                    OnUnreadCountChanged?.Invoke(stationId);
                }
                catch { }
            }

            return count;
        }

        public static int MarkAsReadByMember(int stationId)
        {
            int count = 0;
            lock (_lock)
            {
                foreach (var msg in _messages.Where(m => m.StationId == stationId && m.IsSentByStaff && !m.IsReadByMember))
                {
                    msg.IsReadByMember = true;
                    count++;
                }
            }

            if (count > 0)
            {
                try
                {
                    OnUnreadCountChanged?.Invoke(stationId);
                }
                catch { }
            }

            return count;
        }

        public static IReadOnlyList<int> GetActiveChatStations()
        {
            lock (_lock)
            {
                return _messages
                    .GroupBy(m => m.StationId)
                    .OrderByDescending(g => g.Max(m => m.Timestamp))
                    .Select(g => g.Key)
                    .ToList();
            }
        }

        public static ChatMessage? GetLatestMessage(int stationId)
        {
            lock (_lock)
            {
                return _messages
                    .Where(m => m.StationId == stationId)
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefault();
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
            ChatMessage.ResetIdCounter(0);
        }
    }
}
