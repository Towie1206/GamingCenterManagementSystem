using System;
using System.Collections.Generic;
using System.Linq;

namespace GamingCenterManagementSystem.Models
{
    public class StaffCallNotification
    {
        public int StationId { get; init; }
        public string StationName { get; init; } = string.Empty;
        public string MemberUsername { get; init; } = string.Empty;
        public string MemberFullName { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.Now;
    }

    public static class StaffCallQueue
    {
        private static readonly List<StaffCallNotification> _calls = new();
        private static readonly object _lock = new();

        public static event Action<StaffCallNotification>? OnCallReceived;

        public static void Enqueue(StaffCallNotification call)
        {
            ArgumentNullException.ThrowIfNull(call);

            lock (_lock)
            {
                _calls.Add(call);
            }

            try
            {
                OnCallReceived?.Invoke(call);
            }
            catch
            {
                // Suppress subscriber errors
            }
        }

        public static IReadOnlyList<StaffCallNotification> GetAll()
        {
            lock (_lock)
            {
                return _calls.ToList();
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _calls.Clear();
            }
        }
    }
}
