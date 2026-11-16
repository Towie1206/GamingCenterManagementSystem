using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GamingCenterManagementSystem.Models
{
    public class StaffCallNotification
    {
        private static int _idSeed = 0;

        public int CallId { get; init; } = Interlocked.Increment(ref _idSeed);
        public int StationId { get; init; }
        public string StationName { get; init; } = string.Empty;
        public string MemberUsername { get; init; } = string.Empty;
        public string MemberFullName { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending, Acknowledged, Resolved
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public void MarkResolved(string staffUsername)
        {
            Status = "Resolved";
            ResolvedBy = staffUsername;
            ResolvedAt = DateTime.Now;
        }
    }

    public static class StaffCallQueue
    {
        private static readonly List<StaffCallNotification> _calls = new();
        private static readonly object _lock = new();

        public static event Action<StaffCallNotification>? OnCallReceived;
        public static event Action<StaffCallNotification>? OnCallResolved;

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

        public static bool ResolveCall(int callId, string staffUsername)
        {
            StaffCallNotification? target;
            lock (_lock)
            {
                target = _calls.FirstOrDefault(c => c.CallId == callId && c.Status != "Resolved");
                if (target == null) return false;
                target.MarkResolved(staffUsername);
            }

            try
            {
                OnCallResolved?.Invoke(target);
            }
            catch { }

            return true;
        }

        public static IReadOnlyList<StaffCallNotification> GetPendingCalls()
        {
            lock (_lock)
            {
                return _calls.Where(c => c.Status != "Resolved").ToList();
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
