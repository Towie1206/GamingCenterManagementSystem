using System;
using System.Collections.Concurrent;

namespace GamingCenterManagementSystem.Models
{
    public class RecoveryToken
    {
        public int UserId { get; set; }
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsUsed { get; set; }
    }

    /// <summary>
    /// Dịch vụ sinh mã OTP ngẫu nhiên 6 số phục vụ mở khóa hoặc đặt lại mật khẩu cho hội viên.
    /// </summary>
    public class AccountRecoveryService
    {
        private readonly ConcurrentDictionary<int, RecoveryToken> _tokens = new();
        private readonly Random _random = new();

        /// <summary>
        /// Sinh mã OTP mới có hiệu lực trong 5 phút.
        /// </summary>
        public string GenerateOtp(int userId, int validityMinutes = 5)
        {
            string code = _random.Next(100000, 999999).ToString();
            var token = new RecoveryToken
            {
                UserId = userId,
                OtpCode = code,
                ExpireAt = DateTime.Now.AddMinutes(validityMinutes),
                FailedAttempts = 0,
                IsUsed = false
            };
            _tokens[userId] = token;
            return code;
        }

        /// <summary>
        /// Xác thực mã OTP. Chặn nếu hết hạn hoặc nhập sai quá 3 lần.
        /// </summary>
        public bool ValidateOtp(int userId, string inputOtp, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!_tokens.TryGetValue(userId, out var token))
            {
                errorMessage = "Không tìm thấy yêu cầu xác thực OTP.";
                return false;
            }

            if (token.IsUsed)
            {
                errorMessage = "Mã OTP này đã được sử dụng trước đó.";
                return false;
            }

            if (DateTime.Now > token.ExpireAt)
            {
                errorMessage = "Mã OTP đã hết hạn (quá thời hạn 5 phút).";
                return false;
            }

            if (token.FailedAttempts >= 3)
            {
                errorMessage = "Bạn đã nhập sai OTP quá 3 lần. Vui lòng liên hệ quầy thu ngân.";
                return false;
            }

            if (token.OtpCode != inputOtp)
            {
                token.FailedAttempts++;
                errorMessage = $"Mã OTP không chính xác. Bạn còn {3 - token.FailedAttempts} lần thử.";
                return false;
            }

            token.IsUsed = true;
            return true;
        }
    }
}
