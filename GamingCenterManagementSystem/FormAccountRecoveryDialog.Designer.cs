namespace GamingCenterManagementSystem
{
    partial class FormAccountRecoveryDialog
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
            pnlHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            grpLookup = new GroupBox();
            btnLookup = new Button();
            txtIdentifier = new TextBox();
            lblIdentifier = new Label();
            grpUserInfo = new GroupBox();
            txtFoundStatus = new TextBox();
            lblFoundStatus = new Label();
            txtFoundCccd = new TextBox();
            lblFoundCccd = new Label();
            txtFoundPhone = new TextBox();
            lblFoundPhone = new Label();
            txtFoundFullName = new TextBox();
            lblFoundFullName = new Label();
            txtFoundUsername = new TextBox();
            lblFoundUsername = new Label();
            grpReset = new GroupBox();
            lblStatus = new Label();
            btnResetPassword = new Button();
            txtConfirmPassword = new TextBox();
            lblConfirmPassword = new Label();
            txtNewPassword = new TextBox();
            lblNewPassword = new Label();
            txtOtp = new TextBox();
            lblOtp = new Label();
            btnSendOtp = new Button();
            btnClose = new Button();
            pnlHeader.SuspendLayout();
            grpLookup.SuspendLayout();
            grpUserInfo.SuspendLayout();
            grpReset.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(18, 22, 38);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(540, 75);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblSubtitle.ForeColor = Color.FromArgb(170, 185, 210);
            lblSubtitle.Location = new Point(22, 45);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(393, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Hỗ trợ tìm kiếm theo Tên đăng nhập, Số điện thoại hoặc CCCD/CMND";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.FromArgb(0, 210, 255);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(392, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🔍 KHÔI PHỤC TÀI KHOẢN & ĐẶT LẠI MẬT KHẨU";
            // 
            // grpLookup
            // 
            grpLookup.Controls.Add(btnLookup);
            grpLookup.Controls.Add(txtIdentifier);
            grpLookup.Controls.Add(lblIdentifier);
            grpLookup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpLookup.ForeColor = Color.FromArgb(0, 210, 255);
            grpLookup.Location = new Point(20, 85);
            grpLookup.Name = "grpLookup";
            grpLookup.Size = new Size(500, 85);
            grpLookup.TabIndex = 1;
            grpLookup.TabStop = false;
            grpLookup.Text = "Tra Cứu Tài Khoản";
            // 
            // btnLookup
            // 
            btnLookup.BackColor = Color.FromArgb(0, 180, 220);
            btnLookup.FlatStyle = FlatStyle.Flat;
            btnLookup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnLookup.ForeColor = Color.FromArgb(15, 20, 35);
            btnLookup.Location = new Point(375, 42);
            btnLookup.Name = "btnLookup";
            btnLookup.Size = new Size(110, 30);
            btnLookup.TabIndex = 2;
            btnLookup.Text = "🔍 Tra Cứu";
            btnLookup.UseVisualStyleBackColor = false;
            // 
            // txtIdentifier
            // 
            txtIdentifier.BackColor = Color.FromArgb(35, 41, 64);
            txtIdentifier.BorderStyle = BorderStyle.FixedSingle;
            txtIdentifier.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtIdentifier.ForeColor = Color.White;
            txtIdentifier.Location = new Point(15, 44);
            txtIdentifier.Name = "txtIdentifier";
            txtIdentifier.PlaceholderText = "Nhập username, SĐT (vd: 0981000002) hoặc CCCD...";
            txtIdentifier.Size = new Size(350, 25);
            txtIdentifier.TabIndex = 1;
            // 
            // lblIdentifier
            // 
            lblIdentifier.AutoSize = true;
            lblIdentifier.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblIdentifier.ForeColor = Color.FromArgb(220, 230, 245);
            lblIdentifier.Location = new Point(15, 24);
            lblIdentifier.Name = "lblIdentifier";
            lblIdentifier.Size = new Size(244, 15);
            lblIdentifier.TabIndex = 0;
            lblIdentifier.Text = "Tên đăng nhập / Số điện thoại / CCCD-CMND:";
            // 
            // grpUserInfo
            // 
            grpUserInfo.Controls.Add(txtFoundStatus);
            grpUserInfo.Controls.Add(lblFoundStatus);
            grpUserInfo.Controls.Add(txtFoundCccd);
            grpUserInfo.Controls.Add(lblFoundCccd);
            grpUserInfo.Controls.Add(txtFoundPhone);
            grpUserInfo.Controls.Add(lblFoundPhone);
            grpUserInfo.Controls.Add(txtFoundFullName);
            grpUserInfo.Controls.Add(lblFoundFullName);
            grpUserInfo.Controls.Add(txtFoundUsername);
            grpUserInfo.Controls.Add(lblFoundUsername);
            grpUserInfo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpUserInfo.ForeColor = Color.FromArgb(0, 210, 255);
            grpUserInfo.Location = new Point(20, 178);
            grpUserInfo.Name = "grpUserInfo";
            grpUserInfo.Size = new Size(500, 140);
            grpUserInfo.TabIndex = 2;
            grpUserInfo.TabStop = false;
            grpUserInfo.Text = "Thông Tin Hội Viên";
            // 
            // txtFoundStatus
            // 
            txtFoundStatus.BackColor = Color.FromArgb(28, 33, 50);
            txtFoundStatus.BorderStyle = BorderStyle.FixedSingle;
            txtFoundStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtFoundStatus.ForeColor = Color.FromArgb(100, 255, 100);
            txtFoundStatus.Location = new Point(345, 95);
            txtFoundStatus.Name = "txtFoundStatus";
            txtFoundStatus.ReadOnly = true;
            txtFoundStatus.Size = new Size(140, 23);
            txtFoundStatus.TabIndex = 9;
            // 
            // lblFoundStatus
            // 
            lblFoundStatus.AutoSize = true;
            lblFoundStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFoundStatus.ForeColor = Color.FromArgb(200, 210, 230);
            lblFoundStatus.Location = new Point(265, 98);
            lblFoundStatus.Name = "lblFoundStatus";
            lblFoundStatus.Size = new Size(62, 15);
            lblFoundStatus.TabIndex = 8;
            lblFoundStatus.Text = "Trạng thái:";
            // 
            // txtFoundCccd
            // 
            txtFoundCccd.BackColor = Color.FromArgb(28, 33, 50);
            txtFoundCccd.BorderStyle = BorderStyle.FixedSingle;
            txtFoundCccd.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtFoundCccd.ForeColor = Color.White;
            txtFoundCccd.Location = new Point(110, 95);
            txtFoundCccd.Name = "txtFoundCccd";
            txtFoundCccd.ReadOnly = true;
            txtFoundCccd.Size = new Size(140, 23);
            txtFoundCccd.TabIndex = 7;
            // 
            // lblFoundCccd
            // 
            lblFoundCccd.AutoSize = true;
            lblFoundCccd.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFoundCccd.ForeColor = Color.FromArgb(200, 210, 230);
            lblFoundCccd.Location = new Point(15, 98);
            lblFoundCccd.Name = "lblFoundCccd";
            lblFoundCccd.Size = new Size(92, 15);
            lblFoundCccd.TabIndex = 6;
            lblFoundCccd.Text = "Số CCCD/CMND:";
            // 
            // txtFoundPhone
            // 
            txtFoundPhone.BackColor = Color.FromArgb(28, 33, 50);
            txtFoundPhone.BorderStyle = BorderStyle.FixedSingle;
            txtFoundPhone.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtFoundPhone.ForeColor = Color.White;
            txtFoundPhone.Location = new Point(345, 60);
            txtFoundPhone.Name = "txtFoundPhone";
            txtFoundPhone.ReadOnly = true;
            txtFoundPhone.Size = new Size(140, 23);
            txtFoundPhone.TabIndex = 5;
            // 
            // lblFoundPhone
            // 
            lblFoundPhone.AutoSize = true;
            lblFoundPhone.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFoundPhone.ForeColor = Color.FromArgb(200, 210, 230);
            lblFoundPhone.Location = new Point(265, 63);
            lblFoundPhone.Name = "lblFoundPhone";
            lblFoundPhone.Size = new Size(79, 15);
            lblFoundPhone.TabIndex = 4;
            lblFoundPhone.Text = "Số điện thoại:";
            // 
            // txtFoundFullName
            // 
            txtFoundFullName.BackColor = Color.FromArgb(28, 33, 50);
            txtFoundFullName.BorderStyle = BorderStyle.FixedSingle;
            txtFoundFullName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtFoundFullName.ForeColor = Color.White;
            txtFoundFullName.Location = new Point(110, 60);
            txtFoundFullName.Name = "txtFoundFullName";
            txtFoundFullName.ReadOnly = true;
            txtFoundFullName.Size = new Size(140, 23);
            txtFoundFullName.TabIndex = 3;
            // 
            // lblFoundFullName
            // 
            lblFoundFullName.AutoSize = true;
            lblFoundFullName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFoundFullName.ForeColor = Color.FromArgb(200, 210, 230);
            lblFoundFullName.Location = new Point(15, 63);
            lblFoundFullName.Name = "lblFoundFullName";
            lblFoundFullName.Size = new Size(61, 15);
            lblFoundFullName.TabIndex = 2;
            lblFoundFullName.Text = "Họ và tên:";
            // 
            // txtFoundUsername
            // 
            txtFoundUsername.BackColor = Color.FromArgb(28, 33, 50);
            txtFoundUsername.BorderStyle = BorderStyle.FixedSingle;
            txtFoundUsername.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtFoundUsername.ForeColor = Color.FromArgb(0, 210, 255);
            txtFoundUsername.Location = new Point(110, 25);
            txtFoundUsername.Name = "txtFoundUsername";
            txtFoundUsername.ReadOnly = true;
            txtFoundUsername.Size = new Size(375, 23);
            txtFoundUsername.TabIndex = 1;
            // 
            // lblFoundUsername
            // 
            lblFoundUsername.AutoSize = true;
            lblFoundUsername.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFoundUsername.ForeColor = Color.FromArgb(200, 210, 230);
            lblFoundUsername.Location = new Point(15, 28);
            lblFoundUsername.Name = "lblFoundUsername";
            lblFoundUsername.Size = new Size(88, 15);
            lblFoundUsername.TabIndex = 0;
            lblFoundUsername.Text = "Tên đăng nhập:";
            // 
            // grpReset
            // 
            grpReset.Controls.Add(lblStatus);
            grpReset.Controls.Add(btnResetPassword);
            grpReset.Controls.Add(txtConfirmPassword);
            grpReset.Controls.Add(lblConfirmPassword);
            grpReset.Controls.Add(txtNewPassword);
            grpReset.Controls.Add(lblNewPassword);
            grpReset.Controls.Add(txtOtp);
            grpReset.Controls.Add(lblOtp);
            grpReset.Controls.Add(btnSendOtp);
            grpReset.Enabled = false;
            grpReset.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpReset.ForeColor = Color.FromArgb(0, 210, 255);
            grpReset.Location = new Point(20, 326);
            grpReset.Name = "grpReset";
            grpReset.Size = new Size(500, 190);
            grpReset.TabIndex = 3;
            grpReset.TabStop = false;
            grpReset.Text = "Xác Thực OTP & Đặt Lại Mật Khẩu";
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblStatus.ForeColor = Color.FromArgb(255, 120, 120);
            lblStatus.Location = new Point(15, 158);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(470, 22);
            lblStatus.TabIndex = 8;
            // 
            // btnResetPassword
            // 
            btnResetPassword.BackColor = Color.FromArgb(34, 139, 34);
            btnResetPassword.FlatStyle = FlatStyle.Flat;
            btnResetPassword.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnResetPassword.ForeColor = Color.White;
            btnResetPassword.Location = new Point(140, 122);
            btnResetPassword.Name = "btnResetPassword";
            btnResetPassword.Size = new Size(220, 32);
            btnResetPassword.TabIndex = 7;
            btnResetPassword.Text = "✅ Xác Nhận Đặt Lại Mật Khẩu";
            btnResetPassword.UseVisualStyleBackColor = false;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BackColor = Color.FromArgb(35, 41, 64);
            txtConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
            txtConfirmPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtConfirmPassword.ForeColor = Color.White;
            txtConfirmPassword.Location = new Point(345, 78);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(140, 23);
            txtConfirmPassword.TabIndex = 6;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // lblConfirmPassword
            // 
            lblConfirmPassword.AutoSize = true;
            lblConfirmPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblConfirmPassword.ForeColor = Color.FromArgb(200, 210, 230);
            lblConfirmPassword.Location = new Point(230, 81);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new Size(112, 15);
            lblConfirmPassword.TabIndex = 5;
            lblConfirmPassword.Text = "Nhập lại mật khẩu:";
            // 
            // txtNewPassword
            // 
            txtNewPassword.BackColor = Color.FromArgb(35, 41, 64);
            txtNewPassword.BorderStyle = BorderStyle.FixedSingle;
            txtNewPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtNewPassword.ForeColor = Color.White;
            txtNewPassword.Location = new Point(95, 78);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new Size(125, 23);
            txtNewPassword.TabIndex = 4;
            txtNewPassword.UseSystemPasswordChar = true;
            // 
            // lblNewPassword
            // 
            lblNewPassword.AutoSize = true;
            lblNewPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblNewPassword.ForeColor = Color.FromArgb(200, 210, 230);
            lblNewPassword.Location = new Point(15, 81);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new Size(84, 15);
            lblNewPassword.TabIndex = 3;
            lblNewPassword.Text = "Mật khẩu mới:";
            // 
            // txtOtp
            // 
            txtOtp.BackColor = Color.FromArgb(35, 41, 64);
            txtOtp.BorderStyle = BorderStyle.FixedSingle;
            txtOtp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtOtp.ForeColor = Color.Yellow;
            txtOtp.Location = new Point(345, 33);
            txtOtp.MaxLength = 6;
            txtOtp.Name = "txtOtp";
            txtOtp.PlaceholderText = "6 chữ số...";
            txtOtp.Size = new Size(140, 23);
            txtOtp.TabIndex = 2;
            // 
            // lblOtp
            // 
            lblOtp.AutoSize = true;
            lblOtp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblOtp.ForeColor = Color.FromArgb(200, 210, 230);
            lblOtp.Location = new Point(230, 36);
            lblOtp.Name = "lblOtp";
            lblOtp.Size = new Size(76, 15);
            lblOtp.TabIndex = 1;
            lblOtp.Text = "Mã OTP 6 số:";
            // 
            // btnSendOtp
            // 
            btnSendOtp.BackColor = Color.FromArgb(70, 80, 105);
            btnSendOtp.FlatStyle = FlatStyle.Flat;
            btnSendOtp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSendOtp.ForeColor = Color.White;
            btnSendOtp.Location = new Point(15, 30);
            btnSendOtp.Name = "btnSendOtp";
            btnSendOtp.Size = new Size(185, 28);
            btnSendOtp.TabIndex = 0;
            btnSendOtp.Text = "📩 Gửi Mã Xác Thực OTP";
            btnSendOtp.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(45, 52, 75);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(415, 526);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(105, 35);
            btnClose.TabIndex = 4;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // FormAccountRecoveryDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 29, 45);
            ClientSize = new Size(540, 575);
            Controls.Add(btnClose);
            Controls.Add(grpReset);
            Controls.Add(grpUserInfo);
            Controls.Add(grpLookup);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAccountRecoveryDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Khôi phục tài khoản hội viên";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            grpLookup.ResumeLayout(false);
            grpLookup.PerformLayout();
            grpUserInfo.ResumeLayout(false);
            grpUserInfo.PerformLayout();
            grpReset.ResumeLayout(false);
            grpReset.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private GroupBox grpLookup;
        private Label lblIdentifier;
        private TextBox txtIdentifier;
        private Button btnLookup;
        private GroupBox grpUserInfo;
        private Label lblFoundUsername;
        private TextBox txtFoundUsername;
        private Label lblFoundFullName;
        private TextBox txtFoundFullName;
        private Label lblFoundPhone;
        private TextBox txtFoundPhone;
        private Label lblFoundCccd;
        private TextBox txtFoundCccd;
        private Label lblFoundStatus;
        private TextBox txtFoundStatus;
        private GroupBox grpReset;
        private Button btnSendOtp;
        private Label lblOtp;
        private TextBox txtOtp;
        private Label lblNewPassword;
        private TextBox txtNewPassword;
        private Label lblConfirmPassword;
        private TextBox txtConfirmPassword;
        private Button btnResetPassword;
        private Label lblStatus;
        private Button btnClose;
    }
}
