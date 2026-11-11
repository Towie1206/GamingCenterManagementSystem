namespace GamingCenterManagementSystem
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlBanner = new Panel();
            lblSubtitle = new Label();
            lblBrand = new Label();
            pnlCard = new Panel();
            lblRoleHint = new Label();
            btnExit = new Button();
            btnLogin = new Button();
            lblError = new Label();
            chkShowPassword = new CheckBox();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            lblLoginTitle = new Label();
            pnlBanner.SuspendLayout();
            pnlCard.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBanner
            // 
            pnlBanner.BackColor = Color.FromArgb(18, 22, 38);
            pnlBanner.Controls.Add(lblSubtitle);
            pnlBanner.Controls.Add(lblBrand);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Location = new Point(0, 0);
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new Size(460, 88);
            pnlBanner.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblSubtitle.ForeColor = Color.FromArgb(0, 210, 255);
            lblSubtitle.Location = new Point(28, 52);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(321, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "GAMING CENTER MANAGEMENT SYSTEM - PHASE 2";
            // 
            // lblBrand
            // 
            lblBrand.AutoSize = true;
            lblBrand.Font = new Font("Segoe UI", 17F, FontStyle.Bold, GraphicsUnit.Point);
            lblBrand.ForeColor = Color.White;
            lblBrand.Location = new Point(25, 16);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(192, 31);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "CYBER MATRIX";
            // 
            // pnlCard
            // 
            pnlCard.BackColor = Color.FromArgb(25, 29, 45);
            pnlCard.Controls.Add(lblRoleHint);
            pnlCard.Controls.Add(btnExit);
            pnlCard.Controls.Add(btnLogin);
            pnlCard.Controls.Add(lblError);
            pnlCard.Controls.Add(chkShowPassword);
            pnlCard.Controls.Add(txtPassword);
            pnlCard.Controls.Add(lblPassword);
            pnlCard.Controls.Add(txtUsername);
            pnlCard.Controls.Add(lblUsername);
            pnlCard.Controls.Add(lblLoginTitle);
            pnlCard.Location = new Point(28, 108);
            pnlCard.Name = "pnlCard";
            pnlCard.Size = new Size(404, 330);
            pnlCard.TabIndex = 1;
            // 
            // lblRoleHint
            // 
            lblRoleHint.AutoSize = true;
            lblRoleHint.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic, GraphicsUnit.Point);
            lblRoleHint.ForeColor = Color.FromArgb(145, 155, 175);
            lblRoleHint.Location = new Point(24, 298);
            lblRoleHint.Name = "lblRoleHint";
            lblRoleHint.Size = new Size(274, 13);
            lblRoleHint.TabIndex = 9;
            lblRoleHint.Text = "Hỗ trợ phân quyền: Admin / Staff / Member (PBKDF2)";
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(52, 60, 88);
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(276, 240);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(104, 42);
            btnExit.TabIndex = 8;
            btnExit.Text = "Thoát";
            btnExit.UseVisualStyleBackColor = false;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(0, 200, 235);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnLogin.ForeColor = Color.FromArgb(13, 15, 25);
            btnLogin.Location = new Point(24, 240);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(240, 42);
            btnLogin.TabIndex = 7;
            btnLogin.Text = "🔐 ĐĂNG NHẬP";
            btnLogin.UseVisualStyleBackColor = false;
            // 
            // lblError
            // 
            lblError.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblError.ForeColor = Color.FromArgb(255, 110, 110);
            lblError.Location = new Point(24, 198);
            lblError.Name = "lblError";
            lblError.Size = new Size(356, 36);
            lblError.TabIndex = 6;
            lblError.Text = "";
            lblError.Visible = false;
            // 
            // chkShowPassword
            // 
            chkShowPassword.AutoSize = true;
            chkShowPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            chkShowPassword.ForeColor = Color.FromArgb(190, 200, 220);
            chkShowPassword.Location = new Point(24, 170);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(105, 19);
            chkShowPassword.TabIndex = 5;
            chkShowPassword.Text = "Hiện mật khẩu";
            chkShowPassword.UseVisualStyleBackColor = true;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(35, 41, 64);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtPassword.ForeColor = Color.White;
            txtPassword.Location = new Point(24, 134);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Nhập mật khẩu...";
            txtPassword.Size = new Size(356, 26);
            txtPassword.TabIndex = 4;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblPassword.ForeColor = Color.FromArgb(220, 228, 242);
            lblPassword.Location = new Point(24, 112);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(69, 17);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Mật khẩu:";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(35, 41, 64);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtUsername.ForeColor = Color.White;
            txtUsername.Location = new Point(24, 72);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Nhập tên đăng nhập (vd: admin, staff01, gamer01)...";
            txtUsername.Size = new Size(356, 26);
            txtUsername.TabIndex = 2;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblUsername.ForeColor = Color.FromArgb(220, 228, 242);
            lblUsername.Location = new Point(24, 50);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(102, 17);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "Tên đăng nhập:";
            // 
            // lblLoginTitle
            // 
            lblLoginTitle.AutoSize = true;
            lblLoginTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblLoginTitle.ForeColor = Color.White;
            lblLoginTitle.Location = new Point(24, 16);
            lblLoginTitle.Name = "lblLoginTitle";
            lblLoginTitle.Size = new Size(192, 21);
            lblLoginTitle.TabIndex = 0;
            lblLoginTitle.Text = "ĐĂNG NHẬP TÀI KHOẢN";
            // 
            // FormLogin
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(13, 15, 25);
            CancelButton = btnExit;
            ClientSize = new Size(460, 462);
            Controls.Add(pnlCard);
            Controls.Add(pnlBanner);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Đăng Nhập - Cyber Matrix";
            pnlBanner.ResumeLayout(false);
            pnlBanner.PerformLayout();
            pnlCard.ResumeLayout(false);
            pnlCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBanner;
        private Label lblBrand;
        private Label lblSubtitle;
        private Panel pnlCard;
        private Label lblLoginTitle;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Label lblError;
        private Button btnLogin;
        private Button btnExit;
        private Label lblRoleHint;
    }
}
