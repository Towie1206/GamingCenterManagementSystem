namespace GamingCenterManagementSystem
{
    partial class FormChangePasswordDialog
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

        private void InitializeComponent()
        {
            lblTitle = new System.Windows.Forms.Label();
            lblAccount = new System.Windows.Forms.Label();
            lblOldPassword = new System.Windows.Forms.Label();
            txtOldPassword = new System.Windows.Forms.TextBox();
            lblNewPassword = new System.Windows.Forms.Label();
            txtNewPassword = new System.Windows.Forms.TextBox();
            lblConfirmPassword = new System.Windows.Forms.Label();
            txtConfirmPassword = new System.Windows.Forms.TextBox();
            chkShowPassword = new System.Windows.Forms.CheckBox();
            lblError = new System.Windows.Forms.Label();
            btnConfirm = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 210, 255);
            lblTitle.Location = new System.Drawing.Point(24, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(326, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🔑 ĐỔI MẬT KHẨU TÀI KHOẢN";
            // 
            // lblAccount
            // 
            lblAccount.AutoSize = true;
            lblAccount.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            lblAccount.ForeColor = System.Drawing.Color.FromArgb(200, 215, 230);
            lblAccount.Location = new System.Drawing.Point(26, 56);
            lblAccount.Name = "lblAccount";
            lblAccount.Size = new System.Drawing.Size(155, 21);
            lblAccount.TabIndex = 1;
            lblAccount.Text = "Tài khoản: [username]";
            // 
            // lblOldPassword
            // 
            lblOldPassword.AutoSize = true;
            lblOldPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblOldPassword.ForeColor = System.Drawing.Color.White;
            lblOldPassword.Location = new System.Drawing.Point(26, 95);
            lblOldPassword.Name = "lblOldPassword";
            lblOldPassword.Size = new System.Drawing.Size(135, 20);
            lblOldPassword.TabIndex = 2;
            lblOldPassword.Text = "Mật khẩu hiện tại:";
            // 
            // txtOldPassword
            // 
            txtOldPassword.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtOldPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtOldPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtOldPassword.ForeColor = System.Drawing.Color.White;
            txtOldPassword.Location = new System.Drawing.Point(28, 120);
            txtOldPassword.Name = "txtOldPassword";
            txtOldPassword.Size = new System.Drawing.Size(350, 30);
            txtOldPassword.TabIndex = 3;
            txtOldPassword.UseSystemPasswordChar = true;
            // 
            // lblNewPassword
            // 
            lblNewPassword.AutoSize = true;
            lblNewPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblNewPassword.ForeColor = System.Drawing.Color.White;
            lblNewPassword.Location = new System.Drawing.Point(26, 165);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new System.Drawing.Size(186, 20);
            lblNewPassword.TabIndex = 4;
            lblNewPassword.Text = "Mật khẩu mới (>= 6 ký tự):";
            // 
            // txtNewPassword
            // 
            txtNewPassword.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtNewPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtNewPassword.ForeColor = System.Drawing.Color.White;
            txtNewPassword.Location = new System.Drawing.Point(28, 190);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new System.Drawing.Size(350, 30);
            txtNewPassword.TabIndex = 5;
            txtNewPassword.UseSystemPasswordChar = true;
            // 
            // lblConfirmPassword
            // 
            lblConfirmPassword.AutoSize = true;
            lblConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblConfirmPassword.ForeColor = System.Drawing.Color.White;
            lblConfirmPassword.Location = new System.Drawing.Point(26, 235);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new System.Drawing.Size(176, 20);
            lblConfirmPassword.TabIndex = 6;
            lblConfirmPassword.Text = "Xác nhận mật khẩu mới:";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtConfirmPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtConfirmPassword.ForeColor = System.Drawing.Color.White;
            txtConfirmPassword.Location = new System.Drawing.Point(28, 260);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new System.Drawing.Size(350, 30);
            txtConfirmPassword.TabIndex = 7;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPassword
            // 
            chkShowPassword.AutoSize = true;
            chkShowPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            chkShowPassword.ForeColor = System.Drawing.Color.FromArgb(180, 200, 220);
            chkShowPassword.Location = new System.Drawing.Point(28, 305);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new System.Drawing.Size(150, 24);
            chkShowPassword.TabIndex = 8;
            chkShowPassword.Text = "Hiển thị mật khẩu";
            chkShowPassword.UseVisualStyleBackColor = true;
            // 
            // lblError
            // 
            lblError.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblError.ForeColor = System.Drawing.Color.FromArgb(255, 99, 99);
            lblError.Location = new System.Drawing.Point(26, 335);
            lblError.Name = "lblError";
            lblError.Size = new System.Drawing.Size(352, 45);
            lblError.TabIndex = 9;
            lblError.Text = "Lỗi xác thực...";
            lblError.Visible = false;
            // 
            // btnConfirm
            // 
            btnConfirm.BackColor = System.Drawing.Color.FromArgb(0, 180, 136);
            btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnConfirm.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnConfirm.ForeColor = System.Drawing.Color.White;
            btnConfirm.Location = new System.Drawing.Point(28, 390);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new System.Drawing.Size(170, 42);
            btnConfirm.TabIndex = 10;
            btnConfirm.Text = "XÁC NHẬN ĐỔI";
            btnConfirm.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = System.Drawing.Color.FromArgb(70, 80, 95);
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnCancel.ForeColor = System.Drawing.Color.White;
            btnCancel.Location = new System.Drawing.Point(208, 390);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(170, 42);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "HỦY BỎ";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // FormChangePasswordDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            ClientSize = new System.Drawing.Size(410, 455);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Controls.Add(lblError);
            Controls.Add(chkShowPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(lblConfirmPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(lblNewPassword);
            Controls.Add(txtOldPassword);
            Controls.Add(lblOldPassword);
            Controls.Add(lblAccount);
            Controls.Add(lblTitle);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormChangePasswordDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Đổi Mật Khẩu Hội Viên";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAccount;
        private System.Windows.Forms.Label lblOldPassword;
        private System.Windows.Forms.TextBox txtOldPassword;
        private System.Windows.Forms.Label lblNewPassword;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
    }
}
