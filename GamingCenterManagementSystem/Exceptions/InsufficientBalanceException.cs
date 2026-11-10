using System;

namespace GamingCenterManagementSystem.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public decimal CurrentBalance { get; }
        public decimal RequiredAmount { get; }

        public InsufficientBalanceException(string message) : base(message) { }

        public InsufficientBalanceException(decimal currentBalance, decimal requiredAmount)
            : base($"Số dư ví không đủ (Hiện có: {currentBalance:N0}đ, Cần: {requiredAmount:N0}đ)!")
        {
            CurrentBalance = currentBalance;
            RequiredAmount = requiredAmount;
        }
    }
}
