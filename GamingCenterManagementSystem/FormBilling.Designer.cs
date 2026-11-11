namespace GamingCenterManagementSystem
{
    partial class FormBilling
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
            lblTongSoPhien = new Label();
            lblTienDichVu = new Label();
            lblTienGio = new Label();
            lblTongDoanhThu = new Label();
            pnlLeft = new Panel();
            grpThuatToanDaHinh = new GroupBox();
            lblGiaiThichDaHinh = new Label();
            lblKetQuaTinh = new Label();
            btnTinhThuTien = new Button();
            txtSoGioChoi = new TextBox();
            lblSoGioChoi = new Label();
            txtDonGiaGoc = new TextBox();
            lblDonGiaGoc = new Label();
            cboThuatToanDaHinh = new ComboBox();
            lblChonThuatToan = new Label();
            grpLocBaoCao = new GroupBox();
            btnLamMoi = new Button();
            btnXuatBaoCao = new Button();
            btnLocBaoCao = new Button();
            cboLocTrangThai = new ComboBox();
            lblLocTrangThai = new Label();
            cboLocGoiCuoc = new ComboBox();
            lblLocGoiCuoc = new Label();
            dtpDenNgay = new DateTimePicker();
            lblDenNgay = new Label();
            dtpTuNgay = new DateTimePicker();
            lblTuNgay = new Label();
            pnlMain = new Panel();
            splMain = new SplitContainer();
            grpBaoCaoPhienChoi = new GroupBox();
            dgvDoanhThu = new DataGridView();
            grpGoiCuoc = new GroupBox();
            dgvGoiCuoc = new DataGridView();
            pnlGoiCuocInputs = new Panel();
            btnCapNhatGoiCuoc = new Button();
            btnThemGoiCuoc = new Button();
            txtDiscountRate = new TextBox();
            lblDiscountRate = new Label();
            txtFixedPrice = new TextBox();
            lblFixedPrice = new Label();
            cboTariffType = new ComboBox();
            lblTariffType = new Label();
            txtPlanName = new TextBox();
            lblPlanName = new Label();
            pnlHeader.SuspendLayout();
            pnlStats.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpThuatToanDaHinh.SuspendLayout();
            grpLocBaoCao.SuspendLayout();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
            splMain.Panel1.SuspendLayout();
            splMain.Panel2.SuspendLayout();
            splMain.SuspendLayout();
            grpBaoCaoPhienChoi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDoanhThu).BeginInit();
            grpGoiCuoc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGoiCuoc).BeginInit();
            pnlGoiCuocInputs.SuspendLayout();
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
            pnlHeader.Size = new Size(1144, 65);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblSubtitle.ForeColor = Color.FromArgb(0, 190, 255);
            lblSubtitle.Location = new Point(20, 38);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(545, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Phân công: Thành viên 5 (Khôi) - Nhánh dev-member-5: Quản lý dịch vụ, gói cước & báo cáo doanh thu";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(18, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(469, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "QUẢN LÝ DỊCH VỤ, GÓI CƯỚC & BÁO CÁO DOANH THU";
            // 
            // pnlStats
            // 
            pnlStats.BackColor = Color.FromArgb(40, 48, 76);
            pnlStats.Controls.Add(lblTongSoPhien);
            pnlStats.Controls.Add(lblTienDichVu);
            pnlStats.Controls.Add(lblTienGio);
            pnlStats.Controls.Add(lblTongDoanhThu);
            pnlStats.Dock = DockStyle.Top;
            pnlStats.Location = new Point(0, 65);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1144, 45);
            pnlStats.TabIndex = 1;
            // 
            // lblTongSoPhien
            // 
            lblTongSoPhien.AutoSize = true;
            lblTongSoPhien.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTongSoPhien.ForeColor = Color.FromArgb(200, 200, 255);
            lblTongSoPhien.Location = new Point(780, 14);
            lblTongSoPhien.Name = "lblTongSoPhien";
            lblTongSoPhien.Size = new Size(130, 17);
            lblTongSoPhien.TabIndex = 3;
            lblTongSoPhien.Text = "🎮 Tổng phiên: 0";
            // 
            // lblTienDichVu
            // 
            lblTienDichVu.AutoSize = true;
            lblTienDichVu.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTienDichVu.ForeColor = Color.FromArgb(255, 180, 120);
            lblTienDichVu.Location = new Point(530, 14);
            lblTienDichVu.Name = "lblTienDichVu";
            lblTienDichVu.Size = new Size(160, 17);
            lblTienDichVu.TabIndex = 2;
            lblTienDichVu.Text = "🍔 Dịch vụ F&B: 0 VNĐ";
            // 
            // lblTienGio
            // 
            lblTienGio.AutoSize = true;
            lblTienGio.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTienGio.ForeColor = Color.FromArgb(255, 215, 0);
            lblTienGio.Location = new Point(280, 14);
            lblTienGio.Name = "lblTienGio";
            lblTienGio.Size = new Size(174, 17);
            lblTienGio.TabIndex = 1;
            lblTienGio.Text = "⏱️ Tiền giờ chơi: 0 VNĐ";
            // 
            // lblTongDoanhThu
            // 
            lblTongDoanhThu.AutoSize = true;
            lblTongDoanhThu.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTongDoanhThu.ForeColor = Color.FromArgb(120, 255, 120);
            lblTongDoanhThu.Location = new Point(20, 14);
            lblTongDoanhThu.Name = "lblTongDoanhThu";
            lblTongDoanhThu.Size = new Size(187, 17);
            lblTongDoanhThu.TabIndex = 0;
            lblTongDoanhThu.Text = "💵 Tổng doanh thu: 0 VNĐ";
            // 
            // pnlLeft
            // 
            pnlLeft.AutoScroll = true;
            pnlLeft.Controls.Add(grpThuatToanDaHinh);
            pnlLeft.Controls.Add(grpLocBaoCao);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 110);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(360, 631);
            pnlLeft.TabIndex = 2;
            // 
            // grpThuatToanDaHinh
            // 
            grpThuatToanDaHinh.BackColor = Color.FromArgb(245, 245, 255);
            grpThuatToanDaHinh.Controls.Add(lblGiaiThichDaHinh);
            grpThuatToanDaHinh.Controls.Add(lblKetQuaTinh);
            grpThuatToanDaHinh.Controls.Add(btnTinhThuTien);
            grpThuatToanDaHinh.Controls.Add(txtSoGioChoi);
            grpThuatToanDaHinh.Controls.Add(lblSoGioChoi);
            grpThuatToanDaHinh.Controls.Add(txtDonGiaGoc);
            grpThuatToanDaHinh.Controls.Add(lblDonGiaGoc);
            grpThuatToanDaHinh.Controls.Add(cboThuatToanDaHinh);
            grpThuatToanDaHinh.Controls.Add(lblChonThuatToan);
            grpThuatToanDaHinh.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpThuatToanDaHinh.ForeColor = Color.FromArgb(75, 0, 130);
            grpThuatToanDaHinh.Location = new Point(10, 290);
            grpThuatToanDaHinh.Name = "grpThuatToanDaHinh";
            grpThuatToanDaHinh.Size = new Size(340, 330);
            grpThuatToanDaHinh.TabIndex = 1;
            grpThuatToanDaHinh.TabStop = false;
            grpThuatToanDaHinh.Text = "Thử Nghiệm Tính Cước Đa Hình (OOP)";
            // 
            // lblGiaiThichDaHinh
            // 
            lblGiaiThichDaHinh.Font = new Font("Segoe UI", 8F, FontStyle.Italic, GraphicsUnit.Point);
            lblGiaiThichDaHinh.ForeColor = Color.DimGray;
            lblGiaiThichDaHinh.Location = new Point(15, 280);
            lblGiaiThichDaHinh.Name = "lblGiaiThichDaHinh";
            lblGiaiThichDaHinh.Size = new Size(310, 40);
            lblGiaiThichDaHinh.TabIndex = 8;
            lblGiaiThichDaHinh.Text = "* Minh họa tính Đa hình: Lớp TariffCalculatorBase gọi CalculateCost() ứng với từng thuật toán kế thừa.";
            // 
            // lblKetQuaTinh
            // 
            lblKetQuaTinh.BackColor = Color.FromArgb(230, 230, 250);
            lblKetQuaTinh.BorderStyle = BorderStyle.FixedSingle;
            lblKetQuaTinh.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblKetQuaTinh.ForeColor = Color.DarkBlue;
            lblKetQuaTinh.Location = new Point(15, 215);
            lblKetQuaTinh.Name = "lblKetQuaTinh";
            lblKetQuaTinh.Padding = new Padding(5);
            lblKetQuaTinh.Size = new Size(310, 55);
            lblKetQuaTinh.TabIndex = 7;
            lblKetQuaTinh.Text = "Kết quả: Bấm nút để tính tiền cước";
            lblKetQuaTinh.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnTinhThuTien
            // 
            btnTinhThuTien.BackColor = Color.FromArgb(106, 90, 205);
            btnTinhThuTien.FlatStyle = FlatStyle.Flat;
            btnTinhThuTien.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnTinhThuTien.ForeColor = Color.White;
            btnTinhThuTien.Location = new Point(15, 170);
            btnTinhThuTien.Name = "btnTinhThuTien";
            btnTinhThuTien.Size = new Size(310, 36);
            btnTinhThuTien.TabIndex = 6;
            btnTinhThuTien.Text = "⚙️ TÍNH CƯỚC THỬ NGHIỆM";
            btnTinhThuTien.UseVisualStyleBackColor = false;
            // 
            // txtSoGioChoi
            // 
            txtSoGioChoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtSoGioChoi.Location = new Point(135, 128);
            txtSoGioChoi.Name = "txtSoGioChoi";
            txtSoGioChoi.Size = new Size(190, 23);
            txtSoGioChoi.TabIndex = 5;
            txtSoGioChoi.Text = "3.5";
            // 
            // lblSoGioChoi
            // 
            lblSoGioChoi.AutoSize = true;
            lblSoGioChoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblSoGioChoi.Location = new Point(15, 131);
            lblSoGioChoi.Name = "lblSoGioChoi";
            lblSoGioChoi.Size = new Size(99, 15);
            lblSoGioChoi.TabIndex = 4;
            lblSoGioChoi.Text = "Số giờ chơi (tiếng):";
            // 
            // txtDonGiaGoc
            // 
            txtDonGiaGoc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtDonGiaGoc.Location = new Point(135, 88);
            txtDonGiaGoc.Name = "txtDonGiaGoc";
            txtDonGiaGoc.Size = new Size(190, 23);
            txtDonGiaGoc.TabIndex = 3;
            txtDonGiaGoc.Text = "12000";
            // 
            // lblDonGiaGoc
            // 
            lblDonGiaGoc.AutoSize = true;
            lblDonGiaGoc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblDonGiaGoc.Location = new Point(15, 91);
            lblDonGiaGoc.Name = "lblDonGiaGoc";
            lblDonGiaGoc.Size = new Size(100, 15);
            lblDonGiaGoc.TabIndex = 2;
            lblDonGiaGoc.Text = "Đơn giá gốc / giờ:";
            // 
            // cboThuatToanDaHinh
            // 
            cboThuatToanDaHinh.DropDownStyle = ComboBoxStyle.DropDownList;
            cboThuatToanDaHinh.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboThuatToanDaHinh.FormattingEnabled = true;
            cboThuatToanDaHinh.Items.AddRange(new object[] { "1. HourlyTariff (Giờ thường)", "2. NightComboTariff (Combo đêm)", "3. TournamentTariff (Giảm 50% giải)" });
            cboThuatToanDaHinh.Location = new Point(15, 50);
            cboThuatToanDaHinh.Name = "cboThuatToanDaHinh";
            cboThuatToanDaHinh.Size = new Size(310, 23);
            cboThuatToanDaHinh.TabIndex = 1;
            // 
            // lblChonThuatToan
            // 
            lblChonThuatToan.AutoSize = true;
            lblChonThuatToan.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblChonThuatToan.Location = new Point(15, 28);
            lblChonThuatToan.Name = "lblChonThuatToan";
            lblChonThuatToan.Size = new Size(119, 15);
            lblChonThuatToan.TabIndex = 0;
            lblChonThuatToan.Text = "Gói cước kế thừa OOP:";
            // 
            // grpLocBaoCao
            // 
            grpLocBaoCao.Controls.Add(btnLamMoi);
            grpLocBaoCao.Controls.Add(btnXuatBaoCao);
            grpLocBaoCao.Controls.Add(btnLocBaoCao);
            grpLocBaoCao.Controls.Add(cboLocTrangThai);
            grpLocBaoCao.Controls.Add(lblLocTrangThai);
            grpLocBaoCao.Controls.Add(cboLocGoiCuoc);
            grpLocBaoCao.Controls.Add(lblLocGoiCuoc);
            grpLocBaoCao.Controls.Add(dtpDenNgay);
            grpLocBaoCao.Controls.Add(lblDenNgay);
            grpLocBaoCao.Controls.Add(dtpTuNgay);
            grpLocBaoCao.Controls.Add(lblTuNgay);
            grpLocBaoCao.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpLocBaoCao.Location = new Point(10, 10);
            grpLocBaoCao.Name = "grpLocBaoCao";
            grpLocBaoCao.Size = new Size(340, 270);
            grpLocBaoCao.TabIndex = 0;
            grpLocBaoCao.TabStop = false;
            grpLocBaoCao.Text = "Lọc Báo Cáo Doanh Thu";
            // 
            // btnLamMoi
            // 
            btnLamMoi.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoi.FlatStyle = FlatStyle.Flat;
            btnLamMoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Location = new Point(15, 225);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(145, 32);
            btnLamMoi.TabIndex = 10;
            btnLamMoi.Text = "🔄 Làm Mới";
            btnLamMoi.UseVisualStyleBackColor = false;
            // 
            // btnXuatBaoCao
            // 
            btnXuatBaoCao.BackColor = Color.FromArgb(204, 102, 0);
            btnXuatBaoCao.FlatStyle = FlatStyle.Flat;
            btnXuatBaoCao.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnXuatBaoCao.ForeColor = Color.White;
            btnXuatBaoCao.Location = new Point(175, 225);
            btnXuatBaoCao.Name = "btnXuatBaoCao";
            btnXuatBaoCao.Size = new Size(150, 32);
            btnXuatBaoCao.TabIndex = 9;
            btnXuatBaoCao.Text = "📊 Xuất Báo Cáo";
            btnXuatBaoCao.UseVisualStyleBackColor = false;
            // 
            // btnLocBaoCao
            // 
            btnLocBaoCao.BackColor = Color.FromArgb(0, 122, 204);
            btnLocBaoCao.FlatStyle = FlatStyle.Flat;
            btnLocBaoCao.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnLocBaoCao.ForeColor = Color.White;
            btnLocBaoCao.Location = new Point(15, 180);
            btnLocBaoCao.Name = "btnLocBaoCao";
            btnLocBaoCao.Size = new Size(310, 34);
            btnLocBaoCao.TabIndex = 8;
            btnLocBaoCao.Text = "🔍 LỌC DOANH THU";
            btnLocBaoCao.UseVisualStyleBackColor = false;
            // 
            // cboLocTrangThai
            // 
            cboLocTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocTrangThai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboLocTrangThai.FormattingEnabled = true;
            cboLocTrangThai.Items.AddRange(new object[] { "Tất cả", "Completed", "Active" });
            cboLocTrangThai.Location = new Point(110, 140);
            cboLocTrangThai.Name = "cboLocTrangThai";
            cboLocTrangThai.Size = new Size(215, 23);
            cboLocTrangThai.TabIndex = 7;
            // 
            // lblLocTrangThai
            // 
            lblLocTrangThai.AutoSize = true;
            lblLocTrangThai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblLocTrangThai.Location = new Point(15, 143);
            lblLocTrangThai.Name = "lblLocTrangThai";
            lblLocTrangThai.Size = new Size(62, 15);
            lblLocTrangThai.TabIndex = 6;
            lblLocTrangThai.Text = "Trạng thái:";
            // 
            // cboLocGoiCuoc
            // 
            cboLocGoiCuoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocGoiCuoc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboLocGoiCuoc.FormattingEnabled = true;
            cboLocGoiCuoc.Items.AddRange(new object[] { "Tất cả", "Hourly", "NightCombo", "Tournament", "MemberTier" });
            cboLocGoiCuoc.Location = new Point(110, 103);
            cboLocGoiCuoc.Name = "cboLocGoiCuoc";
            cboLocGoiCuoc.Size = new Size(215, 23);
            cboLocGoiCuoc.TabIndex = 5;
            // 
            // lblLocGoiCuoc
            // 
            lblLocGoiCuoc.AutoSize = true;
            lblLocGoiCuoc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblLocGoiCuoc.Location = new Point(15, 106);
            lblLocGoiCuoc.Name = "lblLocGoiCuoc";
            lblLocGoiCuoc.Size = new Size(57, 15);
            lblLocGoiCuoc.TabIndex = 4;
            lblLocGoiCuoc.Text = "Gói cước:";
            // 
            // dtpDenNgay
            // 
            dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            dtpDenNgay.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dtpDenNgay.Format = DateTimePickerFormat.Custom;
            dtpDenNgay.Location = new Point(110, 66);
            dtpDenNgay.Name = "dtpDenNgay";
            dtpDenNgay.Size = new Size(215, 23);
            dtpDenNgay.TabIndex = 3;
            // 
            // lblDenNgay
            // 
            lblDenNgay.AutoSize = true;
            lblDenNgay.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblDenNgay.Location = new Point(15, 70);
            lblDenNgay.Name = "lblDenNgay";
            lblDenNgay.Size = new Size(60, 15);
            lblDenNgay.TabIndex = 2;
            lblDenNgay.Text = "Đến ngày:";
            // 
            // dtpTuNgay
            // 
            dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            dtpTuNgay.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dtpTuNgay.Format = DateTimePickerFormat.Custom;
            dtpTuNgay.Location = new Point(110, 30);
            dtpTuNgay.Name = "dtpTuNgay";
            dtpTuNgay.Size = new Size(215, 23);
            dtpTuNgay.TabIndex = 1;
            // 
            // lblTuNgay
            // 
            lblTuNgay.AutoSize = true;
            lblTuNgay.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblTuNgay.Location = new Point(15, 34);
            lblTuNgay.Name = "lblTuNgay";
            lblTuNgay.Size = new Size(52, 15);
            lblTuNgay.TabIndex = 0;
            lblTuNgay.Text = "Từ ngày:";
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(splMain);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(360, 110);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(10);
            pnlMain.Size = new Size(784, 631);
            pnlMain.TabIndex = 3;
            // 
            // splMain
            // 
            splMain.Dock = DockStyle.Fill;
            splMain.Location = new Point(10, 10);
            splMain.Name = "splMain";
            splMain.Orientation = Orientation.Horizontal;
            // 
            // splMain.Panel1
            // 
            splMain.Panel1.Controls.Add(grpBaoCaoPhienChoi);
            // 
            // splMain.Panel2
            // 
            splMain.Panel2.Controls.Add(grpGoiCuoc);
            splMain.Size = new Size(764, 611);
            splMain.SplitterDistance = 330;
            splMain.TabIndex = 0;
            // 
            // grpBaoCaoPhienChoi
            // 
            grpBaoCaoPhienChoi.Controls.Add(dgvDoanhThu);
            grpBaoCaoPhienChoi.Dock = DockStyle.Fill;
            grpBaoCaoPhienChoi.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpBaoCaoPhienChoi.Location = new Point(0, 0);
            grpBaoCaoPhienChoi.Name = "grpBaoCaoPhienChoi";
            grpBaoCaoPhienChoi.Size = new Size(764, 330);
            grpBaoCaoPhienChoi.TabIndex = 0;
            grpBaoCaoPhienChoi.TabStop = false;
            grpBaoCaoPhienChoi.Text = "Báo Cáo Chi Tiết Phiên Chơi & Doanh Thu";
            // 
            // dgvDoanhThu
            // 
            dgvDoanhThu.AllowUserToAddRows = false;
            dgvDoanhThu.AllowUserToDeleteRows = false;
            dgvDoanhThu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDoanhThu.BackgroundColor = Color.WhiteSmoke;
            dgvDoanhThu.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDoanhThu.Dock = DockStyle.Fill;
            dgvDoanhThu.Location = new Point(3, 20);
            dgvDoanhThu.MultiSelect = false;
            dgvDoanhThu.Name = "dgvDoanhThu";
            dgvDoanhThu.ReadOnly = true;
            dgvDoanhThu.RowHeadersVisible = false;
            dgvDoanhThu.RowTemplate.Height = 28;
            dgvDoanhThu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDoanhThu.Size = new Size(758, 307);
            dgvDoanhThu.TabIndex = 0;
            // 
            // grpGoiCuoc
            // 
            grpGoiCuoc.Controls.Add(dgvGoiCuoc);
            grpGoiCuoc.Controls.Add(pnlGoiCuocInputs);
            grpGoiCuoc.Dock = DockStyle.Fill;
            grpGoiCuoc.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpGoiCuoc.Location = new Point(0, 0);
            grpGoiCuoc.Name = "grpGoiCuoc";
            grpGoiCuoc.Size = new Size(764, 277);
            grpGoiCuoc.TabIndex = 0;
            grpGoiCuoc.TabStop = false;
            grpGoiCuoc.Text = "Quản Lý Danh Mục Gói Cước";
            // 
            // dgvGoiCuoc
            // 
            dgvGoiCuoc.AllowUserToAddRows = false;
            dgvGoiCuoc.AllowUserToDeleteRows = false;
            dgvGoiCuoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGoiCuoc.BackgroundColor = Color.WhiteSmoke;
            dgvGoiCuoc.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGoiCuoc.Dock = DockStyle.Fill;
            dgvGoiCuoc.Location = new Point(3, 85);
            dgvGoiCuoc.MultiSelect = false;
            dgvGoiCuoc.Name = "dgvGoiCuoc";
            dgvGoiCuoc.ReadOnly = true;
            dgvGoiCuoc.RowHeadersVisible = false;
            dgvGoiCuoc.RowTemplate.Height = 26;
            dgvGoiCuoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGoiCuoc.Size = new Size(758, 189);
            dgvGoiCuoc.TabIndex = 1;
            // 
            // pnlGoiCuocInputs
            // 
            pnlGoiCuocInputs.Controls.Add(btnCapNhatGoiCuoc);
            pnlGoiCuocInputs.Controls.Add(btnThemGoiCuoc);
            pnlGoiCuocInputs.Controls.Add(txtDiscountRate);
            pnlGoiCuocInputs.Controls.Add(lblDiscountRate);
            pnlGoiCuocInputs.Controls.Add(txtFixedPrice);
            pnlGoiCuocInputs.Controls.Add(lblFixedPrice);
            pnlGoiCuocInputs.Controls.Add(cboTariffType);
            pnlGoiCuocInputs.Controls.Add(lblTariffType);
            pnlGoiCuocInputs.Controls.Add(txtPlanName);
            pnlGoiCuocInputs.Controls.Add(lblPlanName);
            pnlGoiCuocInputs.Dock = DockStyle.Top;
            pnlGoiCuocInputs.Location = new Point(3, 20);
            pnlGoiCuocInputs.Name = "pnlGoiCuocInputs";
            pnlGoiCuocInputs.Size = new Size(758, 65);
            pnlGoiCuocInputs.TabIndex = 0;
            // 
            // btnCapNhatGoiCuoc
            // 
            btnCapNhatGoiCuoc.BackColor = Color.FromArgb(30, 144, 255);
            btnCapNhatGoiCuoc.FlatStyle = FlatStyle.Flat;
            btnCapNhatGoiCuoc.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnCapNhatGoiCuoc.ForeColor = Color.White;
            btnCapNhatGoiCuoc.Location = new Point(650, 33);
            btnCapNhatGoiCuoc.Name = "btnCapNhatGoiCuoc";
            btnCapNhatGoiCuoc.Size = new Size(95, 26);
            btnCapNhatGoiCuoc.TabIndex = 9;
            btnCapNhatGoiCuoc.Text = "✏️ Cập Nhật";
            btnCapNhatGoiCuoc.UseVisualStyleBackColor = false;
            // 
            // btnThemGoiCuoc
            // 
            btnThemGoiCuoc.BackColor = Color.FromArgb(46, 139, 87);
            btnThemGoiCuoc.FlatStyle = FlatStyle.Flat;
            btnThemGoiCuoc.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnThemGoiCuoc.ForeColor = Color.White;
            btnThemGoiCuoc.Location = new Point(650, 4);
            btnThemGoiCuoc.Name = "btnThemGoiCuoc";
            btnThemGoiCuoc.Size = new Size(95, 26);
            btnThemGoiCuoc.TabIndex = 8;
            btnThemGoiCuoc.Text = "➕ Thêm Gói";
            btnThemGoiCuoc.UseVisualStyleBackColor = false;
            // 
            // txtDiscountRate
            // 
            txtDiscountRate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtDiscountRate.Location = new Point(555, 20);
            txtDiscountRate.Name = "txtDiscountRate";
            txtDiscountRate.Size = new Size(75, 23);
            txtDiscountRate.TabIndex = 7;
            txtDiscountRate.Text = "0";
            // 
            // lblDiscountRate
            // 
            lblDiscountRate.AutoSize = true;
            lblDiscountRate.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblDiscountRate.Location = new Point(485, 24);
            lblDiscountRate.Name = "lblDiscountRate";
            lblDiscountRate.Size = new Size(67, 15);
            lblDiscountRate.TabIndex = 6;
            lblDiscountRate.Text = "Chiết khấu:";
            // 
            // txtFixedPrice
            // 
            txtFixedPrice.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtFixedPrice.Location = new Point(385, 20);
            txtFixedPrice.Name = "txtFixedPrice";
            txtFixedPrice.Size = new Size(90, 23);
            txtFixedPrice.TabIndex = 5;
            // 
            // lblFixedPrice
            // 
            lblFixedPrice.AutoSize = true;
            lblFixedPrice.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblFixedPrice.Location = new Point(335, 24);
            lblFixedPrice.Name = "lblFixedPrice";
            lblFixedPrice.Size = new Size(49, 15);
            lblFixedPrice.TabIndex = 4;
            lblFixedPrice.Text = "Giá trọn:";
            // 
            // cboTariffType
            // 
            cboTariffType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTariffType.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboTariffType.FormattingEnabled = true;
            cboTariffType.Items.AddRange(new object[] { "Hourly", "NightCombo", "Tournament", "MemberTier" });
            cboTariffType.Location = new Point(220, 20);
            cboTariffType.Name = "cboTariffType";
            cboTariffType.Size = new Size(105, 23);
            cboTariffType.TabIndex = 3;
            // 
            // lblTariffType
            // 
            lblTariffType.AutoSize = true;
            lblTariffType.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblTariffType.Location = new Point(160, 24);
            lblTariffType.Name = "lblTariffType";
            lblTariffType.Size = new Size(57, 15);
            lblTariffType.TabIndex = 2;
            lblTariffType.Text = "Loại cước:";
            // 
            // txtPlanName
            // 
            txtPlanName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtPlanName.Location = new Point(60, 20);
            txtPlanName.Name = "txtPlanName";
            txtPlanName.Size = new Size(95, 23);
            txtPlanName.TabIndex = 1;
            // 
            // lblPlanName
            // 
            lblPlanName.AutoSize = true;
            lblPlanName.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblPlanName.Location = new Point(5, 24);
            lblPlanName.Name = "lblPlanName";
            lblPlanName.Size = new Size(51, 15);
            lblPlanName.TabIndex = 0;
            lblPlanName.Text = "Tên gói:";
            // 
            // FormBilling
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1144, 741);
            Controls.Add(pnlMain);
            Controls.Add(pnlLeft);
            Controls.Add(pnlStats);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(1000, 700);
            Name = "FormBilling";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Dịch Vụ, Gói Cước & Doanh Thu - Cyber Game";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStats.ResumeLayout(false);
            pnlStats.PerformLayout();
            pnlLeft.ResumeLayout(false);
            grpThuatToanDaHinh.ResumeLayout(false);
            grpThuatToanDaHinh.PerformLayout();
            grpLocBaoCao.ResumeLayout(false);
            grpLocBaoCao.PerformLayout();
            pnlMain.ResumeLayout(false);
            splMain.Panel1.ResumeLayout(false);
            splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
            splMain.ResumeLayout(false);
            grpBaoCaoPhienChoi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDoanhThu).EndInit();
            grpGoiCuoc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGoiCuoc).EndInit();
            pnlGoiCuocInputs.ResumeLayout(false);
            pnlGoiCuocInputs.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblSubtitle;
        private Label lblTitle;
        private Panel pnlStats;
        private Label lblTongSoPhien;
        private Label lblTienDichVu;
        private Label lblTienGio;
        private Label lblTongDoanhThu;
        private Panel pnlLeft;
        private GroupBox grpLocBaoCao;
        private Label lblTuNgay;
        private DateTimePicker dtpTuNgay;
        private Label lblDenNgay;
        private DateTimePicker dtpDenNgay;
        private Label lblLocGoiCuoc;
        private ComboBox cboLocGoiCuoc;
        private Label lblLocTrangThai;
        private ComboBox cboLocTrangThai;
        private Button btnLocBaoCao;
        private Button btnXuatBaoCao;
        private Button btnLamMoi;
        private GroupBox grpThuatToanDaHinh;
        private Label lblChonThuatToan;
        private ComboBox cboThuatToanDaHinh;
        private Label lblDonGiaGoc;
        private TextBox txtDonGiaGoc;
        private Label lblSoGioChoi;
        private TextBox txtSoGioChoi;
        private Button btnTinhThuTien;
        private Label lblKetQuaTinh;
        private Label lblGiaiThichDaHinh;
        private Panel pnlMain;
        private SplitContainer splMain;
        private GroupBox grpBaoCaoPhienChoi;
        private DataGridView dgvDoanhThu;
        private GroupBox grpGoiCuoc;
        private DataGridView dgvGoiCuoc;
        private Panel pnlGoiCuocInputs;
        private Label lblPlanName;
        private TextBox txtPlanName;
        private Label lblTariffType;
        private ComboBox cboTariffType;
        private Label lblFixedPrice;
        private TextBox txtFixedPrice;
        private Label lblDiscountRate;
        private TextBox txtDiscountRate;
        private Button btnThemGoiCuoc;
        private Button btnCapNhatGoiCuoc;
    }
}
