using System;

namespace GamingCenterManagementSystem.Models
{
    public class AppUser
    {
        private decimal _balance;

        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Member"; // Admin, Staff, Member

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số dư ví không được âm.", nameof(value));
                _balance = value;
            }
        }

        public string MemberTier { get; set; } = "Standard"; // Standard, Silver, Gold
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Encapsulation helpers
        public bool HasSufficientBalance(decimal amount) => amount >= 0 && Balance >= amount;

        public static string ResolveUpgradedTier(string? currentTier, decimal balance)
        {
            if (balance >= 500000m)
                return "Gold";

            if (balance >= 200000m && !string.Equals(currentTier, "Gold", StringComparison.OrdinalIgnoreCase))
                return "Silver";

            return string.IsNullOrWhiteSpace(currentTier) ? "Standard" : currentTier;
        }
    }
}
