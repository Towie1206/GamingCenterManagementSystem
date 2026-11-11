using System;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public static class CurrentUserSession
    {
        private static readonly object _syncLock = new();
        private static AppUser? _currentUser;
        private static DateTime? _loginTime;

        public static AppUser? CurrentUser
        {
            get { lock (_syncLock) { return _currentUser; } }
        }

        public static DateTime? LoginTime
        {
            get { lock (_syncLock) { return _loginTime; } }
        }

        public static bool IsLoggedIn
        {
            get { lock (_syncLock) { return _currentUser != null; } }
        }

        public static int UserId
        {
            get { lock (_syncLock) { return _currentUser?.UserId ?? 0; } }
        }

        public static string Username
        {
            get { lock (_syncLock) { return _currentUser?.Username ?? string.Empty; } }
        }

        public static string FullName
        {
            get { lock (_syncLock) { return _currentUser?.FullName ?? string.Empty; } }
        }

        public static string Role
        {
            get { lock (_syncLock) { return _currentUser?.Role ?? string.Empty; } }
        }

        public static string MemberTier
        {
            get { lock (_syncLock) { return _currentUser?.MemberTier ?? "Standard"; } }
        }

        public static decimal Balance
        {
            get { lock (_syncLock) { return _currentUser?.Balance ?? 0m; } }
        }

        public static bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public static bool IsStaff => string.Equals(Role, "Staff", StringComparison.OrdinalIgnoreCase);
        public static bool IsMember => string.Equals(Role, "Member", StringComparison.OrdinalIgnoreCase);
        public static bool CanManageOperations => IsAdmin || IsStaff;

        public static void SetUser(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            lock (_syncLock)
            {
                _currentUser = new AppUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    PasswordHash = string.Empty,
                    FullName = user.FullName,
                    Role = user.Role,
                    Balance = user.Balance,
                    MemberTier = user.MemberTier,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                };
                _loginTime = DateTime.Now;
            }
        }

        public static void RefreshUser(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            lock (_syncLock)
            {
                if (_currentUser != null && _currentUser.UserId == user.UserId)
                {
                    _currentUser.Username = user.Username;
                    _currentUser.FullName = user.FullName;
                    _currentUser.Role = user.Role;
                    _currentUser.Balance = user.Balance;
                    _currentUser.MemberTier = user.MemberTier;
                    _currentUser.IsActive = user.IsActive;
                }
            }
        }

        public static void Clear()
        {
            lock (_syncLock)
            {
                _currentUser = null;
                _loginTime = null;
            }
        }
    }
}
