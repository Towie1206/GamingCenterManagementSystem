using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Hộp thoại xác thực OTP 6 số để khôi phục hoặc đặt lại mật khẩu cho hội viên quên mật khẩu.
    /// </summary>
    public class FormAccountRecoveryDialog : Form
    {
        private readonly DBUser _dbUser;
        private readonly AccountRecoveryService _recoveryService;

        private readonly TextBox _txtUsername = new();
        private readonly TextBox _txtOtp = new();
        private readonly TextBox _txtNewPassword = new();
        private readonly Button _btnSendOtp = new();
        private readonly Button _btnConfirmReset = new();
        private readonly Label _lblStatus = new();

        private int? _targetUserId;
        public string? GeneratedOtpForDemo { get; private set; }

        public FormAccountRecoveryDialog(DBUser dbUser, AccountRecoveryService recoveryService)
        {
            _dbUser = dbUser ?? throw new ArgumentNullException(nameof(dbUser));
            _recoveryService = recoveryService ?? throw new ArgumentNullException(nameof(recoveryService));

            Text = "🔐 Khôi Phục Mật Khẩu Hội Viên Bằng Mã OTP";
            Size = new Size(480, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            var lblTitle = new Label
            {
                Text = "KHÔI PHỤC TÀI KHOẢN HỘI VIÊN",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(24, 20),
                Size = new Size(420, 28)
            };

            var lblUser = new Label { Text = "Tên đăng nhập:", Location = new Point(24, 60), Size = new Size(200, 20), Font = new Font("Segoe UI", 9.5f) };
            _txtUsername.Location = new Point(24, 85);
            _txtUsername.Size = new Size(280, 26);
            _txtUsername.BackColor = Color.FromArgb(30, 41, 59);
            _txtUsername.ForeColor = Color.White;

            _btnSendOtp.Text = "📲 Nhận OTP";
            _btnSendOtp.Location = new Point(315, 83);
            _btnSendOtp.Size = new Size(125, 30);
            _btnSendOtp.BackColor = Color.FromArgb(37, 99, 235);
            _btnSendOtp.ForeColor = Color.White;
            _btnSendOtp.FlatStyle = FlatStyle.Flat;
            _btnSendOtp.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _btnSendOtp.Click += BtnSendOtp_Click;

            var lblOtp = new Label { Text = "Mã xác thực OTP (6 chữ số):", Location = new Point(24, 130), Size = new Size(200, 20), Font = new Font("Segoe UI", 9.5f) };
            _txtOtp.Location = new Point(24, 155);
            _txtOtp.Size = new Size(415, 26);
            _txtOtp.BackColor = Color.FromArgb(30, 41, 59);
            _txtOtp.ForeColor = Color.White;

            var lblPass = new Label { Text = "Mật khẩu mới:", Location = new Point(24, 200), Size = new Size(200, 20), Font = new Font("Segoe UI", 9.5f) };
            _txtNewPassword.Location = new Point(24, 225);
            _txtNewPassword.Size = new Size(415, 26);
            _txtNewPassword.UseSystemPasswordChar = true;
            _txtNewPassword.BackColor = Color.FromArgb(30, 41, 59);
            _txtNewPassword.ForeColor = Color.White;

            _btnConfirmReset.Text = "✅ Xác Nhận Đặt Lại Mật Khẩu";
            _btnConfirmReset.Location = new Point(24, 275);
            _btnConfirmReset.Size = new Size(415, 38);
            _btnConfirmReset.BackColor = Color.FromArgb(16, 185, 129);
            _btnConfirmReset.ForeColor = Color.White;
            _btnConfirmReset.FlatStyle = FlatStyle.Flat;
            _btnConfirmReset.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnConfirmReset.Click += BtnConfirmReset_Click;

            _lblStatus.Location = new Point(24, 325);
            _lblStatus.Size = new Size(415, 45);
            _lblStatus.Font = new Font("Segoe UI", 9f);
            _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);

            // Đặt Name để unit test tìm control theo tên thay vì phụ thuộc thứ tự.
            _txtUsername.Name = "_txtUsername";
            _btnSendOtp.Name = "btnSendOtp";
            _lblStatus.Name = "lblStatus";

            Controls.AddRange(new Control[] { lblTitle, lblUser, _txtUsername, _btnSendOtp, lblOtp, _txtOtp, lblPass, _txtNewPassword, _btnConfirmReset, _lblStatus });
        }

        private void BtnSendOtp_Click(object? sender, EventArgs e)
        {
            string input = _txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                _lblStatus.Text = "⚠️ Vui lòng nhập Tên đăng nhập của hội viên.";
                return;
            }

            // Tra cứu bằng GetAll() vì DBUser hiện chưa có GetByUsername.
            var user = _dbUser.GetAll()
                .FirstOrDefault(u => u.Username.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                _lblStatus.Text = "❌ Không tìm thấy tài khoản hội viên phù hợp.";
                return;
            }

            _targetUserId = user.UserId;
            GeneratedOtpForDemo = _recoveryService.GenerateOtp(user.UserId);
            _lblStatus.ForeColor = Color.FromArgb(52, 211, 153);
            _lblStatus.Text = $"✅ Mã OTP đã được gửi đến số điện thoại hội viên!\n(Mô phỏng Demo OTP: {GeneratedOtpForDemo})";
        }

        private void BtnConfirmReset_Click(object? sender, EventArgs e)
        {
            if (!_targetUserId.HasValue)
            {
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                _lblStatus.Text = "⚠️ Vui lòng bấm Nhận OTP trước khi xác nhận.";
                return;
            }

            string otp = _txtOtp.Text.Trim();
            string newPass = _txtNewPassword.Text.Trim();

            if (newPass.Length < 6)
            {
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                _lblStatus.Text = "⚠️ Mật khẩu mới phải có ít nhất 6 ký tự.";
                return;
            }

            if (!_recoveryService.ValidateOtp(_targetUserId.Value, otp, out string error))
            {
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                _lblStatus.Text = $"❌ Lỗi OTP: {error}";
                return;
            }

            // LƯU Ý: dòng dưới cần DBUser có hàm UpdatePassword(int userId, string newPassword).
            bool updated = _dbUser.UpdatePassword(_targetUserId.Value, newPass);
            if (updated)
            {
                MessageBox.Show("Đặt lại mật khẩu hội viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                _lblStatus.Text = "❌ Lỗi hệ thống: Không thể cập nhật mật khẩu vào CSDL.";
            }
        }
    }
}