using System;

namespace GamingCenterManagementSystem.Models
{
    public class WalletTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int? SessionId { get; set; }
        public string TransactionType { get; set; } = "TopUp"; // TopUp, SessionPayment, Refund, Adjustment
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Description { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
