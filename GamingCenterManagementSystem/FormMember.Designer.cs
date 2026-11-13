namespace GamingCenterManagementSystem
{
    partial class FormMember
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
            pnlStats = new Panel();
            lblHoiVienVip = new Label();
            lblTongSoDuVi = new Label();
            lblTongHoiVien = new Label();
            pnlLeft = new Panel();
            grpDoiMatKhau = new GroupBox();
            btnDoiMatKhau = new Button();
            txtMatKhauMoi = new TextBox();
            lblMatKhauMoi = new Label();
            txtMatKhauCu = new TextBox();
            lblMatKhauCu = new Label();
            txtDoiMatKhauUser = new TextBox();
            lblDoiMatKhauUser = new Label();
            grpNapTien = new GroupBox();
            btnNapTien = new Button();
            cboHinhThucNap = new ComboBox();
            lblHinhThuc = new Label();
            btnCong200k = new Button();
            btnCong100k = new Button();
            btnCong50k = new Button();
            btnCong20k = new Button();
            txtSoTienNap = new TextBox();
            lblSoTienNap = new Label();
            lblDangChon = new Label();
            grpThongTinHoiVien = new GroupBox();
            btnLamMoi = new Button();
            btnXoaHoiVien = new Button();
            btnSuaHoiVien = new Button();
            btnThemHoiVien = new Button();
            txtBalance = new TextBox();
            lblBalance = new Label();
            cboRole = new ComboBox();
            lblRole = new Label();
            cboMemberTier = new ComboBox();
            lblMemberTier = new Label();
            txtCccd = new TextBox();
            lblCccd = new Label();
            txtFullName = new TextBox();
            lblFullName = new Label();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            txtUserId = new TextBox();
            lblUserId = new Label();
            pnlMain = new Panel();
            dgvHoiVien = new DataGridView();
            pnlSearch = new Panel();
            cboLocHangThe = new ComboBox();
            btnDatLaiTimKiem = new Button();
            btnTimKiem = new Button();
            txtTimKiem = new TextBox();
            lblTimKiem = new Label();
            pnlHeader.SuspendLayout();
            pnlStats.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpDoiMatKhau.SuspendLayout();
            grpNapTien.SuspendLayout();
            grpThongTinHoiVien.SuspendLayout();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHoiVien).BeginInit();
            pnlSearch.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(24, 30, 54);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1084, 65);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblSubtitle.ForeColor = Color.FromArgb(0, 190, 255);
            lblSubtitle.Location = new Point(20, 38);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(495, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Phân công: Thành viên 3 (Nam) - Nhánh Nam-Alime: Quản lý hội viên, bảo mật mật khẩu & nạp tiền";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(18, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(410, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "QUẢN LÝ HỘI VIÊN & SỐ DƯ VÍ TÀI KHOẢN";
            // 
            // pnlStats
            // 
            pnlStats.BackColor = Color.FromArgb(40, 48, 76);
            pnlStats.Controls.Add(lblHoiVienVip);
            pnlStats.Controls.Add(lblTongSoDuVi);
            pnlStats.Controls.Add(lblTongHoiVien);
            pnlStats.Dock = DockStyle.Top;
            pnlStats.Location = new Point(0, 65);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1084, 45);
            pnlStats.TabIndex = 1;
            // 
            // lblHoiVienVip
            // 
            lblHoiVienVip.AutoSize = true;
            lblHoiVienVip.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblHoiVienVip.ForeColor = Color.FromArgb(255, 215, 0);
            lblHoiVienVip.Location = new Point(540, 14);
            lblHoiVienVip.Name = "lblHoiVienVip";
            lblHoiVienVip.Size = new Size(207, 17);
            lblHoiVienVip.TabIndex = 2;
            lblHoiVienVip.Text = "⭐ Hội viên VIP (Gold/Silver): 0";
            // 
            // lblTongSoDuVi
            // 
            lblTongSoDuVi.AutoSize = true;
            lblTongSoDuVi.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTongSoDuVi.ForeColor = Color.FromArgb(120, 255, 120);
            lblTongSoDuVi.Location = new Point(240, 14);
            lblTongSoDuVi.Name = "lblTongSoDuVi";
            lblTongSoDuVi.Size = new Size(183, 17);
            lblTongSoDuVi.TabIndex = 1;
            lblTongSoDuVi.Text = "💰 Tổng quỹ ví: 0 VNĐ";
            // 
            // lblTongHoiVien
            // 
            lblTongHoiVien.AutoSize = true;
            lblTongHoiVien.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTongHoiVien.ForeColor = Color.White;
            lblTongHoiVien.Location = new Point(20, 14);
            lblTongHoiVien.Name = "lblTongHoiVien";
            lblTongHoiVien.Size = new Size(142, 17);
            lblTongHoiVien.TabIndex = 0;
            lblTongHoiVien.Text = "👥 Tổng hội viên: 0";
            // 
            // pnlLeft
            // 
            pnlLeft.AutoScroll = true;
            pnlLeft.Controls.Add(grpDoiMatKhau);
            pnlLeft.Controls.Add(grpNapTien);
            pnlLeft.Controls.Add(grpThongTinHoiVien);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 110);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(350, 755);
            pnlLeft.TabIndex = 2;
            // 
            // grpDoiMatKhau
            // 
            grpDoiMatKhau.BackColor = Color.FromArgb(242, 245, 252);
            grpDoiMatKhau.Controls.Add(btnDoiMatKhau);
            grpDoiMatKhau.Controls.Add(txtMatKhauMoi);
            grpDoiMatKhau.Controls.Add(lblMatKhauMoi);
            grpDoiMatKhau.Controls.Add(txtMatKhauCu);
            grpDoiMatKhau.Controls.Add(lblMatKhauCu);
            grpDoiMatKhau.Controls.Add(txtDoiMatKhauUser);
            grpDoiMatKhau.Controls.Add(lblDoiMatKhauUser);
            grpDoiMatKhau.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpDoiMatKhau.ForeColor = Color.FromArgb(24, 30, 54);
            grpDoiMatKhau.Location = new Point(10, 555);
            grpDoiMatKhau.Name = "grpDoiMatKhau";
            grpDoiMatKhau.Size = new Size(330, 170);
            grpDoiMatKhau.TabIndex = 2;
            grpDoiMatKhau.TabStop = false;
            grpDoiMatKhau.Text = "Đổi Mật Khẩu Hội Viên (PBKDF2)";
            // 
            // btnDoiMatKhau
            // 
            btnDoiMatKhau.BackColor = Color.FromArgb(24, 30, 54);
            btnDoiMatKhau.FlatStyle = FlatStyle.Flat;
            btnDoiMatKhau.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnDoiMatKhau.ForeColor = Color.FromArgb(0, 190, 255);
            btnDoiMatKhau.Location = new Point(15, 124);
            btnDoiMatKhau.Name = "btnDoiMatKhau";
            btnDoiMatKhau.Size = new Size(295, 34);
            btnDoiMatKhau.TabIndex = 6;
            btnDoiMatKhau.Text = "🔑 CẬP NHẬT MẬT KHẨU";
            btnDoiMatKhau.UseVisualStyleBackColor = false;
            // 
            // txtMatKhauMoi
            // 
            txtMatKhauMoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtMatKhauMoi.Location = new Point(115, 90);
            txtMatKhauMoi.Name = "txtMatKhauMoi";
            txtMatKhauMoi.PlaceholderText = "Ít nhất 6 ký tự";
            txtMatKhauMoi.Size = new Size(195, 23);
            txtMatKhauMoi.TabIndex = 5;
            txtMatKhauMoi.UseSystemPasswordChar = true;
            // 
            // lblMatKhauMoi
            // 
            lblMatKhauMoi.AutoSize = true;
            lblMatKhauMoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblMatKhauMoi.Location = new Point(15, 93);
            lblMatKhauMoi.Name = "lblMatKhauMoi";
            lblMatKhauMoi.Size = new Size(80, 15);
            lblMatKhauMoi.TabIndex = 4;
            lblMatKhauMoi.Text = "Mật khẩu mới:";
            // 
            // txtMatKhauCu
            // 
            txtMatKhauCu.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtMatKhauCu.Location = new Point(115, 58);
            txtMatKhauCu.Name = "txtMatKhauCu";
            txtMatKhauCu.PlaceholderText = "Mật khẩu hiện tại";
            txtMatKhauCu.Size = new Size(195, 23);
            txtMatKhauCu.TabIndex = 3;
            txtMatKhauCu.UseSystemPasswordChar = true;
            // 
            // lblMatKhauCu
            // 
            lblMatKhauCu.AutoSize = true;
            lblMatKhauCu.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblMatKhauCu.Location = new Point(15, 61);
            lblMatKhauCu.Name = "lblMatKhauCu";
            lblMatKhauCu.Size = new Size(76, 15);
            lblMatKhauCu.TabIndex = 2;
            lblMatKhauCu.Text = "Mật khẩu cũ:";
            // 
            // txtDoiMatKhauUser
            // 
            txtDoiMatKhauUser.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtDoiMatKhauUser.Location = new Point(115, 26);
            txtDoiMatKhauUser.Name = "txtDoiMatKhauUser";
            txtDoiMatKhauUser.PlaceholderText = "Chọn dòng hoặc nhập";
            txtDoiMatKhauUser.Size = new Size(195, 23);
            txtDoiMatKhauUser.TabIndex = 1;
            // 
            // lblDoiMatKhauUser
            // 
            lblDoiMatKhauUser.AutoSize = true;
            lblDoiMatKhauUser.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblDoiMatKhauUser.Location = new Point(15, 29);
            lblDoiMatKhauUser.Name = "lblDoiMatKhauUser";
            lblDoiMatKhauUser.Size = new Size(63, 15);
            lblDoiMatKhauUser.TabIndex = 0;
            lblDoiMatKhauUser.Text = "Tài khoản:";
            // 
            // grpNapTien
            // 
            grpNapTien.BackColor = Color.FromArgb(240, 248, 255);
            grpNapTien.Controls.Add(btnNapTien);
            grpNapTien.Controls.Add(cboHinhThucNap);
            grpNapTien.Controls.Add(lblHinhThuc);
            grpNapTien.Controls.Add(btnCong200k);
            grpNapTien.Controls.Add(btnCong100k);
            grpNapTien.Controls.Add(btnCong50k);
            grpNapTien.Controls.Add(btnCong20k);
            grpNapTien.Controls.Add(txtSoTienNap);
            grpNapTien.Controls.Add(lblSoTienNap);
            grpNapTien.Controls.Add(lblDangChon);
            grpNapTien.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpNapTien.ForeColor = Color.FromArgb(0, 51, 102);
            grpNapTien.Location = new Point(10, 335);
            grpNapTien.Name = "grpNapTien";
            grpNapTien.Size = new Size(330, 210);
            grpNapTien.TabIndex = 1;
            grpNapTien.TabStop = false;
            grpNapTien.Text = "Nạp Tiền Vào Ví Hội Viên";
            // 
            // btnNapTien
            // 
            btnNapTien.BackColor = Color.FromArgb(34, 139, 34);
            btnNapTien.FlatStyle = FlatStyle.Flat;
            btnNapTien.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnNapTien.ForeColor = Color.White;
            btnNapTien.Location = new Point(15, 165);
            btnNapTien.Name = "btnNapTien";
            btnNapTien.Size = new Size(295, 34);
            btnNapTien.TabIndex = 9;
            btnNapTien.Text = "💵 XÁC NHẬN NẠP TIỀN";
            btnNapTien.UseVisualStyleBackColor = false;
            // 
            // cboHinhThucNap
            // 
            cboHinhThucNap.DropDownStyle = ComboBoxStyle.DropDownList;
            cboHinhThucNap.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboHinhThucNap.FormattingEnabled = true;
            cboHinhThucNap.Items.AddRange(new object[] { "Tiền mặt tại quầy", "Chuyển khoản QR", "Thẻ cào Cyber" });
            cboHinhThucNap.Location = new Point(110, 131);
            cboHinhThucNap.Name = "cboHinhThucNap";
            cboHinhThucNap.Size = new Size(200, 23);
            cboHinhThucNap.TabIndex = 8;
            // 
            // lblHinhThuc
            // 
            lblHinhThuc.AutoSize = true;
            lblHinhThuc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblHinhThuc.Location = new Point(15, 134);
            lblHinhThuc.Name = "lblHinhThuc";
            lblHinhThuc.Size = new Size(61, 15);
            lblHinhThuc.TabIndex = 7;
            lblHinhThuc.Text = "Hình thức:";
            // 
            // btnCong200k
            // 
            btnCong200k.BackColor = Color.FromArgb(220, 230, 245);
            btnCong200k.FlatStyle = FlatStyle.Flat;
            btnCong200k.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnCong200k.ForeColor = Color.Black;
            btnCong200k.Location = new Point(240, 96);
            btnCong200k.Name = "btnCong200k";
            btnCong200k.Size = new Size(70, 26);
            btnCong200k.TabIndex = 6;
            btnCong200k.Text = "+200k";
            btnCong200k.UseVisualStyleBackColor = false;
            // 
            // btnCong100k
            // 
            btnCong100k.BackColor = Color.FromArgb(220, 230, 245);
            btnCong100k.FlatStyle = FlatStyle.Flat;
            btnCong100k.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnCong100k.ForeColor = Color.Black;
            btnCong100k.Location = new Point(165, 96);
            btnCong100k.Name = "btnCong100k";
            btnCong100k.Size = new Size(70, 26);
            btnCong100k.TabIndex = 5;
            btnCong100k.Text = "+100k";
            btnCong100k.UseVisualStyleBackColor = false;
            // 
            // btnCong50k
            // 
            btnCong50k.BackColor = Color.FromArgb(220, 230, 245);
            btnCong50k.FlatStyle = FlatStyle.Flat;
            btnCong50k.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnCong50k.ForeColor = Color.Black;
            btnCong50k.Location = new Point(90, 96);
            btnCong50k.Name = "btnCong50k";
            btnCong50k.Size = new Size(70, 26);
            btnCong50k.TabIndex = 4;
            btnCong50k.Text = "+50k";
            btnCong50k.UseVisualStyleBackColor = false;
            // 
            // btnCong20k
            // 
            btnCong20k.BackColor = Color.FromArgb(220, 230, 245);
            btnCong20k.FlatStyle = FlatStyle.Flat;
            btnCong20k.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            btnCong20k.ForeColor = Color.Black;
            btnCong20k.Location = new Point(15, 96);
            btnCong20k.Name = "btnCong20k";
            btnCong20k.Size = new Size(70, 26);
            btnCong20k.TabIndex = 3;
            btnCong20k.Text = "+20k";
            btnCong20k.UseVisualStyleBackColor = false;
            // 
            // txtSoTienNap
            // 
            txtSoTienNap.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            txtSoTienNap.ForeColor = Color.FromArgb(0, 100, 0);
            txtSoTienNap.Location = new Point(110, 60);
            txtSoTienNap.Name = "txtSoTienNap";
            txtSoTienNap.Size = new Size(200, 25);
            txtSoTienNap.TabIndex = 2;
            txtSoTienNap.Text = "50000";
            // 
            // lblSoTienNap
            // 
            lblSoTienNap.AutoSize = true;
            lblSoTienNap.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblSoTienNap.Location = new Point(15, 64);
            lblSoTienNap.Name = "lblSoTienNap";
            lblSoTienNap.Size = new Size(68, 15);
            lblSoTienNap.TabIndex = 1;
            lblSoTienNap.Text = "Số tiền nạp:";
            // 
            // lblDangChon
            // 
            lblDangChon.AutoSize = true;
            lblDangChon.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblDangChon.ForeColor = Color.FromArgb(178, 34, 34);
            lblDangChon.Location = new Point(15, 28);
            lblDangChon.Name = "lblDangChon";
            lblDangChon.Size = new Size(211, 17);
            lblDangChon.TabIndex = 0;
            lblDangChon.Text = "Tài khoản nhận: Chưa chọn tài khoản";
            // 
            // grpThongTinHoiVien
            // 
            grpThongTinHoiVien.Controls.Add(btnLamMoi);
            grpThongTinHoiVien.Controls.Add(btnXoaHoiVien);
            grpThongTinHoiVien.Controls.Add(btnSuaHoiVien);
            grpThongTinHoiVien.Controls.Add(btnThemHoiVien);
            grpThongTinHoiVien.Controls.Add(txtBalance);
            grpThongTinHoiVien.Controls.Add(lblBalance);
            grpThongTinHoiVien.Controls.Add(cboRole);
            grpThongTinHoiVien.Controls.Add(lblRole);
            grpThongTinHoiVien.Controls.Add(cboMemberTier);
            grpThongTinHoiVien.Controls.Add(lblMemberTier);
            grpThongTinHoiVien.Controls.Add(txtCccd);
            grpThongTinHoiVien.Controls.Add(lblCccd);
            grpThongTinHoiVien.Controls.Add(txtFullName);
            grpThongTinHoiVien.Controls.Add(lblFullName);
            grpThongTinHoiVien.Controls.Add(txtPassword);
            grpThongTinHoiVien.Controls.Add(lblPassword);
            grpThongTinHoiVien.Controls.Add(txtUsername);
            grpThongTinHoiVien.Controls.Add(lblUsername);
            grpThongTinHoiVien.Controls.Add(txtUserId);
            grpThongTinHoiVien.Controls.Add(lblUserId);
            grpThongTinHoiVien.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpThongTinHoiVien.Location = new Point(10, 10);
            grpThongTinHoiVien.Name = "grpThongTinHoiVien";
            grpThongTinHoiVien.Size = new Size(330, 318);
            grpThongTinHoiVien.TabIndex = 0;
            grpThongTinHoiVien.TabStop = false;
            grpThongTinHoiVien.Text = "Thông Tin Hội Viên";
            // 
            // btnLamMoi
            // 
            btnLamMoi.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoi.FlatStyle = FlatStyle.Flat;
            btnLamMoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Location = new Point(170, 274);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(140, 30);
            btnLamMoi.TabIndex = 19;
            btnLamMoi.Text = "🔄 Làm Mới";
            btnLamMoi.UseVisualStyleBackColor = false;
            // 
            // btnXoaHoiVien
            // 
            btnXoaHoiVien.BackColor = Color.FromArgb(178, 34, 34);
            btnXoaHoiVien.FlatStyle = FlatStyle.Flat;
            btnXoaHoiVien.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnXoaHoiVien.ForeColor = Color.White;
            btnXoaHoiVien.Location = new Point(15, 274);
            btnXoaHoiVien.Name = "btnXoaHoiVien";
            btnXoaHoiVien.Size = new Size(140, 30);
            btnXoaHoiVien.TabIndex = 18;
            btnXoaHoiVien.Text = "🔒 Khóa / Mở khóa";
            btnXoaHoiVien.UseVisualStyleBackColor = false;
            // 
            // btnSuaHoiVien
            // 
            btnSuaHoiVien.BackColor = Color.FromArgb(30, 144, 255);
            btnSuaHoiVien.FlatStyle = FlatStyle.Flat;
            btnSuaHoiVien.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSuaHoiVien.ForeColor = Color.White;
            btnSuaHoiVien.Location = new Point(170, 236);
            btnSuaHoiVien.Name = "btnSuaHoiVien";
            btnSuaHoiVien.Size = new Size(140, 30);
            btnSuaHoiVien.TabIndex = 17;
            btnSuaHoiVien.Text = "✏️ Cập Nhật";
            btnSuaHoiVien.UseVisualStyleBackColor = false;
            // 
            // btnThemHoiVien
            // 
            btnThemHoiVien.BackColor = Color.FromArgb(34, 139, 34);
            btnThemHoiVien.FlatStyle = FlatStyle.Flat;
            btnThemHoiVien.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnThemHoiVien.ForeColor = Color.White;
            btnThemHoiVien.Location = new Point(15, 236);
            btnThemHoiVien.Name = "btnThemHoiVien";
            btnThemHoiVien.Size = new Size(140, 30);
            btnThemHoiVien.TabIndex = 16;
            btnThemHoiVien.Text = "➕ Thêm Mới";
            btnThemHoiVien.UseVisualStyleBackColor = false;
            // 
            // txtBalance
            // 
            txtBalance.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtBalance.ForeColor = Color.FromArgb(0, 100, 0);
            txtBalance.Location = new Point(110, 201);
            txtBalance.Name = "txtBalance";
            txtBalance.PlaceholderText = "Tối thiểu 10000 (Member)";
            txtBalance.Size = new Size(200, 23);
            txtBalance.TabIndex = 15;
            // 
            // lblBalance
            // 
            lblBalance.AutoSize = true;
            lblBalance.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBalance.Location = new Point(15, 204);
            lblBalance.Name = "lblBalance";
            lblBalance.Size = new Size(56, 15);
            lblBalance.TabIndex = 14;
            lblBalance.Text = "Số dư ví:";
            // 
            // cboRole
            // 
            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRole.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboRole.FormattingEnabled = true;
            cboRole.Items.AddRange(new object[] { "Member", "Staff", "Admin" });
            cboRole.Location = new Point(110, 175);
            cboRole.Name = "cboRole";
            cboRole.Size = new Size(200, 23);
            cboRole.TabIndex = 13;
            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblRole.Location = new Point(15, 178);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(43, 15);
            lblRole.TabIndex = 12;
            lblRole.Text = "Vai trò:";
            // 
            // cboMemberTier
            // 
            cboMemberTier.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMemberTier.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboMemberTier.FormattingEnabled = true;
            cboMemberTier.Items.AddRange(new object[] { "Standard", "Silver", "Gold" });
            cboMemberTier.Location = new Point(110, 149);
            cboMemberTier.Name = "cboMemberTier";
            cboMemberTier.Size = new Size(200, 23);
            cboMemberTier.TabIndex = 11;
            // 
            // lblMemberTier
            // 
            lblMemberTier.AutoSize = true;
            lblMemberTier.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblMemberTier.Location = new Point(15, 152);
            lblMemberTier.Name = "lblMemberTier";
            lblMemberTier.Size = new Size(54, 15);
            lblMemberTier.TabIndex = 10;
            lblMemberTier.Text = "Hạng thẻ:";
            // 
            // txtCccd
            // 
            txtCccd.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtCccd.Location = new Point(110, 123);
            txtCccd.MaxLength = 12;
            txtCccd.Name = "txtCccd";
            txtCccd.PlaceholderText = "12 số CCCD / 9 số CMND";
            txtCccd.Size = new Size(200, 23);
            txtCccd.TabIndex = 9;
            // 
            // lblCccd
            // 
            lblCccd.AutoSize = true;
            lblCccd.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCccd.Location = new Point(15, 126);
            lblCccd.Name = "lblCccd";
            lblCccd.Size = new Size(92, 15);
            lblCccd.TabIndex = 8;
            lblCccd.Text = "Số CCCD/CMND:";
            // 
            // txtFullName
            // 
            txtFullName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtFullName.Location = new Point(110, 97);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(200, 23);
            txtFullName.TabIndex = 7;
            // 
            // lblFullName
            // 
            lblFullName.AutoSize = true;
            lblFullName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFullName.Location = new Point(15, 100);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(61, 15);
            lblFullName.TabIndex = 6;
            lblFullName.Text = "Họ và tên:";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtPassword.Location = new Point(110, 71);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Tối thiểu 6 ký tự";
            txtPassword.Size = new Size(200, 23);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblPassword.Location = new Point(15, 74);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Mật khẩu:";
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtUsername.Location = new Point(110, 45);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200, 23);
            txtUsername.TabIndex = 3;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblUsername.Location = new Point(15, 48);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(88, 15);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Tên đăng nhập:";
            // 
            // txtUserId
            // 
            txtUserId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtUserId.Location = new Point(110, 19);
            txtUserId.Name = "txtUserId";
            txtUserId.ReadOnly = true;
            txtUserId.Size = new Size(200, 23);
            txtUserId.TabIndex = 1;
            // 
            // lblUserId
            // 
            lblUserId.AutoSize = true;
            lblUserId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblUserId.Location = new Point(15, 22);
            lblUserId.Name = "lblUserId";
            lblUserId.Size = new Size(47, 15);
            lblUserId.TabIndex = 0;
            lblUserId.Text = "Mã user:";
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(dgvHoiVien);
            pnlMain.Controls.Add(pnlSearch);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(350, 110);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(10);
            pnlMain.Size = new Size(790, 755);
            pnlMain.TabIndex = 3;
            // 
            // dgvHoiVien
            // 
            dgvHoiVien.AllowUserToAddRows = false;
            dgvHoiVien.AllowUserToDeleteRows = false;
            dgvHoiVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHoiVien.BackgroundColor = Color.WhiteSmoke;
            dgvHoiVien.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHoiVien.Dock = DockStyle.Fill;
            dgvHoiVien.Location = new Point(10, 55);
            dgvHoiVien.MultiSelect = false;
            dgvHoiVien.Name = "dgvHoiVien";
            dgvHoiVien.ReadOnly = true;
            dgvHoiVien.RowHeadersVisible = false;
            dgvHoiVien.RowTemplate.Height = 28;
            dgvHoiVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHoiVien.Size = new Size(770, 690);
            dgvHoiVien.TabIndex = 1;
            // 
            // pnlSearch
            // 
            pnlSearch.Controls.Add(cboLocHangThe);
            pnlSearch.Controls.Add(btnDatLaiTimKiem);
            pnlSearch.Controls.Add(btnTimKiem);
            pnlSearch.Controls.Add(txtTimKiem);
            pnlSearch.Controls.Add(lblTimKiem);
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Location = new Point(10, 10);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Size = new Size(770, 78);
            pnlSearch.TabIndex = 0;
            // 
            // cboLocHangThe
            // 
            cboLocHangThe.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocHangThe.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboLocHangThe.FormattingEnabled = true;
            cboLocHangThe.Items.AddRange(new object[] { "Tất cả hạng", "Standard", "Silver", "Gold" });
            cboLocHangThe.Location = new Point(332, 10);
            cboLocHangThe.Name = "cboLocHangThe";
            cboLocHangThe.Size = new Size(122, 23);
            cboLocHangThe.TabIndex = 4;
            // 
            // btnDatLaiTimKiem
            // 
            btnDatLaiTimKiem.BackColor = Color.FromArgb(108, 117, 125);
            btnDatLaiTimKiem.FlatStyle = FlatStyle.Flat;
            btnDatLaiTimKiem.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnDatLaiTimKiem.ForeColor = Color.White;
            btnDatLaiTimKiem.Location = new Point(565, 8);
            btnDatLaiTimKiem.Name = "btnDatLaiTimKiem";
            btnDatLaiTimKiem.Size = new Size(88, 28);
            btnDatLaiTimKiem.TabIndex = 3;
            btnDatLaiTimKiem.Text = "Đặt Lại";
            btnDatLaiTimKiem.UseVisualStyleBackColor = false;
            // 
            // btnTimKiem
            // 
            btnTimKiem.BackColor = Color.FromArgb(0, 122, 204);
            btnTimKiem.FlatStyle = FlatStyle.Flat;
            btnTimKiem.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnTimKiem.ForeColor = Color.White;
            btnTimKiem.Location = new Point(462, 8);
            btnTimKiem.Name = "btnTimKiem";
            btnTimKiem.Size = new Size(95, 28);
            btnTimKiem.TabIndex = 2;
            btnTimKiem.Text = "🔍 Tìm Kiếm";
            btnTimKiem.UseVisualStyleBackColor = false;
            // 
            // txtTimKiem
            // 
            txtTimKiem.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtTimKiem.Location = new Point(140, 10);
            txtTimKiem.Name = "txtTimKiem";
            txtTimKiem.PlaceholderText = "Gõ tên hoặc username...";
            txtTimKiem.Size = new Size(185, 24);
            txtTimKiem.TabIndex = 1;
            // 
            // lblTimKiem
            // 
            lblTimKiem.AutoSize = true;
            lblTimKiem.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblTimKiem.Location = new Point(10, 15);
            lblTimKiem.Name = "lblTimKiem";
            lblTimKiem.Size = new Size(119, 15);
            lblTimKiem.TabIndex = 0;
            lblTimKiem.Text = "Tìm kiếm hội viên:";
            // 
            // FormMember
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1140, 865);
            Controls.Add(pnlMain);
            Controls.Add(pnlLeft);
            Controls.Add(pnlStats);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(1060, 680);
            Name = "FormMember";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Hội Viên & Số Dư Ví - Cyber Game";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStats.ResumeLayout(false);
            pnlStats.PerformLayout();
            pnlLeft.ResumeLayout(false);
            grpDoiMatKhau.ResumeLayout(false);
            grpDoiMatKhau.PerformLayout();
            grpNapTien.ResumeLayout(false);
            grpNapTien.PerformLayout();
            grpThongTinHoiVien.ResumeLayout(false);
            grpThongTinHoiVien.PerformLayout();
            pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvHoiVien).EndInit();
            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblSubtitle;
        private Label lblTitle;
        private Panel pnlStats;
        private Label lblHoiVienVip;
        private Label lblTongSoDuVi;
        private Label lblTongHoiVien;
        private Panel pnlLeft;
        private GroupBox grpDoiMatKhau;
        private Label lblDoiMatKhauUser;
        private TextBox txtDoiMatKhauUser;
        private Label lblMatKhauCu;
        private TextBox txtMatKhauCu;
        private Label lblMatKhauMoi;
        private TextBox txtMatKhauMoi;
        private Button btnDoiMatKhau;
        private GroupBox grpThongTinHoiVien;
        private TextBox txtUserId;
        private Label lblUserId;
        private TextBox txtUsername;
        private Label lblUsername;
        private TextBox txtPassword;
        private Label lblPassword;
        private TextBox txtFullName;
        private Label lblFullName;
        private TextBox txtCccd;
        private Label lblCccd;
        private ComboBox cboMemberTier;
        private Label lblMemberTier;
        private ComboBox cboRole;
        private Label lblRole;
        private TextBox txtBalance;
        private Label lblBalance;
        private Button btnThemHoiVien;
        private Button btnSuaHoiVien;
        private Button btnXoaHoiVien;
        private Button btnLamMoi;
        private GroupBox grpNapTien;
        private Label lblDangChon;
        private Label lblSoTienNap;
        private TextBox txtSoTienNap;
        private Button btnCong20k;
        private Button btnCong50k;
        private Button btnCong100k;
        private Button btnCong200k;
        private Label lblHinhThuc;
        private ComboBox cboHinhThucNap;
        private Button btnNapTien;
        private Panel pnlMain;
        private Panel pnlSearch;
        private ComboBox cboLocHangThe;
        private Label lblTimKiem;
        private TextBox txtTimKiem;
        private Button btnTimKiem;
        private Button btnDatLaiTimKiem;
        private DataGridView dgvHoiVien;
        private ComboBox cboLocVaiTro;
        private Label lblLocVaiTro;
        private Label lblLocHangThe;
        private TabControl tabMain;
        private TabPage tabDanhSach;
        private TabPage tabLichSuVi;
        private TabPage tabTopHoiVien;
        private DataGridView dgvLichSuVi;
        private Panel pnlWalletHeader;
        private Label lblLichSuUser;
        private Button btnTaiLaiLichSu;
        private Label lblTopSummary;
        private SplitContainer splTopHoiVien;
        private GroupBox grpTopSoDu;
        private GroupBox grpTopNapTien;
        private DataGridView dgvTopSoDu;
        private DataGridView dgvTopNapTien;
        private ComboBox cboLocRole;
        private TabPage tabLichSuNap;
        private Button btnTopHoiVien;
        private Button btnKhoaThe;
        private Button btnMoThe;
    }
}
