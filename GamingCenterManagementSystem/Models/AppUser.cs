using System;

namespace GamingCenterManagementSystem.Models
{
    public class AppUser
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Member"; // Admin, Staff, Member
        public decimal Balance { get; set; } = 0;
        public string MemberTier { get; set; } = "Standard"; // Standard, Silver, Gold
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
