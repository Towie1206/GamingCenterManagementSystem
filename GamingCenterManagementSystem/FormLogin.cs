using System;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormLogin : Form
    {
        private readonly DBUser _dbUser;

        public AppUser? AuthenticatedUser { get; private set; }

        public FormLogin() : this(new DBUser())
        {
        }

        public FormLogin(DBUser dbUser)
        {
            ArgumentNullException.ThrowIfNull(dbUser);
            _dbUser = dbUser;
            InitializeComponent();
            SetupEvents();
        }

        private void SetupEvents()
        {
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            txtUsername.TextChanged += (s, e) => ClearError();
            txtPassword.TextChanged += (s, e) => ClearError();

            btnLogin.Click += BtnLogin_Click;
            btnExit.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Shown += (s, e) => txtUsername.Focus();
        }

        private void ClearError()
        {
            if (lblError.Visible)
            {
                lblError.Visible = false;
                lblError.Text = string.Empty;
            }
        }

        public bool TryPerformLogin(out string errorMessage)
        {
            AuthenticatedUser = null;
            try
            {
                if (_dbUser.TryAuthenticate(txtUsername.Text, txtPassword.Text, out AppUser? user, out errorMessage)
                    && user != null)
                {
                    AuthenticatedUser = user;
                    CurrentUserSession.SetUser(user);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi xác thực hệ thống: {ex.Message}";
                return false;
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            try
            {
                if (TryPerformLogin(out string errorMessage))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                lblError.Text = errorMessage;
                lblError.Visible = true;
                txtPassword.SelectAll();
                txtPassword.Focus();
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnLogin.Enabled = true;
                }
            }
        }
    }
}
