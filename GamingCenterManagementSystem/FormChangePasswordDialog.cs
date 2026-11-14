using System;
using System.Drawing;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;

namespace GamingCenterManagementSystem
{
    public partial class FormChangePasswordDialog : Form
    {
        private readonly DBUser _dbUser;
        private readonly string _username;

        public FormChangePasswordDialog() : this(new DBUser(), CurrentUserSession.Username)
        {
        }

        public FormChangePasswordDialog(DBUser dbUser, string username)
        {
            ArgumentNullException.ThrowIfNull(dbUser);
            _dbUser = dbUser;
            _username = string.IsNullOrWhiteSpace(username) ? CurrentUserSession.Username : username.Trim();

            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            lblAccount.Text = $"Tài khoản: {_username}";

            chkShowPassword.CheckedChanged += (s, e) =>
            {
                bool hide = !chkShowPassword.Checked;
                txtOldPassword.UseSystemPasswordChar = hide;
                txtNewPassword.UseSystemPasswordChar = hide;
                txtConfirmPassword.UseSystemPasswordChar = hide;
            };

            txtOldPassword.TextChanged += (s, e) => ClearError();
            txtNewPassword.TextChanged += (s, e) => ClearError();
            txtConfirmPassword.TextChanged += (s, e) => ClearError();

            AcceptButton = btnConfirm;
            CancelButton = btnCancel;

            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Shown += (s, e) => txtOldPassword.Focus();
        }

        private void ClearError()
        {
            if (lblError.Visible)
            {
                lblError.Visible = false;
                lblError.Text = string.Empty;
            }
        }

        public bool TryChangePassword(out string errorMessage)
        {
            string oldPass = txtOldPassword.Text;
            string newPass = txtNewPassword.Text;
            string confirmPass = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(oldPass))
            {
                errorMessage = "Vui lòng nhập mật khẩu hiện tại.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newPass))
            {
                errorMessage = "Vui lòng nhập mật khẩu mới.";
                return false;
            }

            if (newPass.Length < 6)
            {
                errorMessage = "Mật khẩu mới phải có độ dài tối thiểu từ 6 ký tự.";
                return false;
            }

            if (!string.Equals(newPass, confirmPass, StringComparison.Ordinal))
            {
                errorMessage = "Xác nhận mật khẩu mới không khớp.";
                return false;
            }

            if (string.Equals(oldPass, newPass, StringComparison.Ordinal))
            {
                errorMessage = "Mật khẩu mới không được trùng với mật khẩu cũ.";
                return false;
            }

            try
            {
                bool success = _dbUser.ChangePassword(_username, oldPass, newPass, out errorMessage);
                return success;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi đổi mật khẩu: {ex.Message}";
                return false;
            }
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            btnConfirm.Enabled = false;
            try
            {
                if (TryChangePassword(out string errorMessage))
                {
                    MessageBox.Show(
                        "Đổi mật khẩu tài khoản thành công!\nVui lòng ghi nhớ mật khẩu mới để đăng nhập ở các phiên tiếp theo.",
                        "Đổi mật khẩu thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                lblError.Text = errorMessage;
                lblError.Visible = true;
                txtOldPassword.Focus();
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnConfirm.Enabled = true;
                }
            }
        }
    }
}
