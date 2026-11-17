using System;
using System.Drawing;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormAccountRecoveryDialog : Form
    {
        private readonly DBUser _dbUser;
        private readonly AccountRecoveryService _recoveryService;

        public AppUser? FoundUser { get; private set; }

        public FormAccountRecoveryDialog() : this(new DBUser(), new AccountRecoveryService())
        {
        }

        public FormAccountRecoveryDialog(DBUser dbUser) : this(dbUser, new AccountRecoveryService())
        {
        }

        public FormAccountRecoveryDialog(DBUser dbUser, AccountRecoveryService recoveryService)
        {
            ArgumentNullException.ThrowIfNull(dbUser);
            ArgumentNullException.ThrowIfNull(recoveryService);

            _dbUser = dbUser;
            _recoveryService = recoveryService;

            InitializeComponent();
            SetupEvents();
        }

        private void SetupEvents()
        {
            btnLookup.Click += (s, e) => PerformLookup();
            btnSendOtp.Click += (s, e) => PerformSendOtp();
            btnResetPassword.Click += (s, e) => PerformResetPassword();
            btnClose.Click += (s, e) => Close();

            txtIdentifier.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    PerformLookup();
                }
            };
        }

        public AppUser? LookupUser(string? identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                SetStatus("Vui lòng nhập tên đăng nhập, số điện thoại hoặc CCCD.", isError: true);
                ClearFoundUserInfo();
                return null;
            }

            string clean = identifier.Trim();
            AppUser? user = _dbUser.GetByUsernameOrPhone(clean);

            if (user == null)
            {
                SetStatus($"Không tìm thấy tài khoản phù hợp với '{clean}'.", isError: true);
                ClearFoundUserInfo();
                return null;
            }

            FoundUser = user;
            DisplayFoundUser(user);
            SetStatus($"Đã tìm thấy tài khoản '{user.Username}' ({user.FullName}).", isError: false);
            return user;
        }

        private void PerformLookup()
        {
            LookupUser(txtIdentifier.Text);
        }

        private void DisplayFoundUser(AppUser user)
        {
            txtFoundUsername.Text = user.Username;
            txtFoundFullName.Text = user.FullName;
            txtFoundPhone.Text = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "Chưa đăng ký SĐT" : user.PhoneNumber;
            txtFoundCccd.Text = string.IsNullOrWhiteSpace(user.CitizenId) ? "Chưa đăng ký CCCD" : user.CitizenId;
            txtFoundStatus.Text = user.IsActive ? "Hoạt động" : "Đã khóa";
            txtFoundStatus.ForeColor = user.IsActive ? Color.FromArgb(100, 255, 100) : Color.FromArgb(255, 100, 100);

            grpReset.Enabled = true;
            btnSendOtp.Focus();
        }

        private void ClearFoundUserInfo()
        {
            FoundUser = null;
            txtFoundUsername.Clear();
            txtFoundFullName.Clear();
            txtFoundPhone.Clear();
            txtFoundCccd.Clear();
            txtFoundStatus.Clear();
            grpReset.Enabled = false;
            txtOtp.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
        }

        public string GenerateOtpForUser(int userId)
        {
            return _recoveryService.GenerateOtp(userId);
        }

        private void PerformSendOtp()
        {
            if (FoundUser == null)
            {
                SetStatus("Vui lòng tra cứu tài khoản trước khi gửi OTP.", isError: true);
                return;
            }

            string otp = GenerateOtpForUser(FoundUser.UserId);
            SetStatus($"Mã OTP xác thực đã được tạo (mã demo: {otp}) có hiệu lực trong 5 phút.", isError: false);
            txtOtp.Focus();
        }

        public bool TryResetPassword(string identifier, string otp, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;
            AppUser? user = FoundUser;
            if (user == null || (!string.IsNullOrWhiteSpace(identifier) &&
                !user.Username.Equals(identifier.Trim(), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.PhoneNumber, identifier.Trim(), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.CitizenId, identifier.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                user = LookupUser(identifier);
            }

            if (user == null)
            {
                errorMessage = "Không tìm thấy tài khoản để đặt lại mật khẩu.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                errorMessage = "Vui lòng nhập mã xác thực OTP.";
                return false;
            }

            if (!_recoveryService.ValidateOtp(user.UserId, otp, out string otpError))
            {
                errorMessage = otpError;
                return false;
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                errorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return false;
            }

            return _dbUser.ResetPassword(user.UserId, newPassword, out errorMessage);
        }

        private void PerformResetPassword()
        {
            if (FoundUser == null)
            {
                SetStatus("Vui lòng tra cứu tài khoản trước.", isError: true);
                return;
            }

            string otp = txtOtp.Text.Trim();
            string newPass = txtNewPassword.Text;
            string confirmPass = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(otp))
            {
                SetStatus("Vui lòng nhập mã OTP.", isError: true);
                txtOtp.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newPass) || newPass.Length < 6)
            {
                SetStatus("Mật khẩu mới phải có độ dài từ 6 ký tự trở lên.", isError: true);
                txtNewPassword.Focus();
                return;
            }

            if (!string.Equals(newPass, confirmPass, StringComparison.Ordinal))
            {
                SetStatus("Xác nhận mật khẩu mới không khớp.", isError: true);
                txtConfirmPassword.Focus();
                return;
            }

            if (TryResetPassword(FoundUser.Username, otp, newPass, out string resetErr))
            {
                SetStatus("Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bằng mật khẩu mới.", isError: false);
                MessageBox.Show(
                    "Đặt lại mật khẩu thành công! Bạn có thể dùng mật khẩu mới để đăng nhập ngay bây giờ.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                SetStatus(resetErr, isError: true);
            }
        }

        private void SetStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.FromArgb(255, 120, 120) : Color.FromArgb(100, 255, 100);
        }
    }
}
