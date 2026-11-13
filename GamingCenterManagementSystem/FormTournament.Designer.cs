namespace GamingCenterManagementSystem
{
    partial class FormTournament
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
            lblSoTranDau = new Label();
            lblGiaiDangDienRa = new Label();
            lblTongSoGiai = new Label();
            pnlLeft = new Panel();
            grpDieuPhoiMay = new GroupBox();
            btnMoKhoaMay = new Button();
            btnKhoaMayGiaiDau = new Button();
            clbDanMay = new CheckedListBox();
            lblSoMayDaChon = new Label();
            lblChonDanMay = new Label();
            grpGiaiDau = new GroupBox();
            btnLamMoi = new Button();
            btnXoaGiai = new Button();
            btnCapNhatGiai = new Button();
            btnTaoGiai = new Button();
            cboTournamentStatus = new ComboBox();
            lblTournamentStatus = new Label();
            dtpStartDate = new DateTimePicker();
            lblStartDate = new Label();
            txtPrizePool = new TextBox();
            lblPrizePool = new Label();
            cboFormatType = new ComboBox();
            lblFormatType = new Label();
            txtGameTitle = new TextBox();
            lblGameTitle = new Label();
            txtTenGiai = new TextBox();
            lblTenGiai = new Label();
            txtTournamentId = new TextBox();
            lblTournamentId = new Label();
            pnlMain = new Panel();
            splMain = new SplitContainer();
            grpDanhSachGiai = new GroupBox();
            dgvGiaiDau = new DataGridView();
            grpQuanLyTran = new GroupBox();
            dgvTranDau = new DataGridView();
            pnlTranDauControl = new Panel();
            btnTaoVongSau = new Button();
            btnCapNhatKetQua = new Button();
            btnThemTranDau = new Button();
            cboMatchStatus = new ComboBox();
            lblMatchStatus = new Label();
            txtScore = new TextBox();
            lblScore = new Label();
            cboWinnerTeam = new ComboBox();
            lblWinnerTeam = new Label();
            txtTeam2 = new TextBox();
            lblTeam2 = new Label();
            txtTeam1 = new TextBox();
            lblTeam1 = new Label();
            txtRoundNumber = new TextBox();
            lblRoundNumber = new Label();
            pnlHeader.SuspendLayout();
            pnlStats.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpDieuPhoiMay.SuspendLayout();
            grpGiaiDau.SuspendLayout();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
            splMain.Panel1.SuspendLayout();
            splMain.Panel2.SuspendLayout();
            splMain.SuspendLayout();
            grpDanhSachGiai.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGiaiDau).BeginInit();
            grpQuanLyTran.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTranDau).BeginInit();
            pnlTranDauControl.SuspendLayout();
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
            pnlHeader.Size = new Size(1134, 65);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblSubtitle.ForeColor = Color.FromArgb(0, 190, 255);
            lblSubtitle.Location = new Point(20, 38);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(517, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Phân công: Thành viên 4 (Kiên) - Nhánh dev-member-4: Quản lý giải đấu LAN, đăng ký đội & xếp trận";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(18, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(424, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "QUẢN LÝ GIẢI ĐẤU LAN PARTY & XẾP TRẬN ĐẤU";
            // 
            // pnlStats
            // 
            pnlStats.BackColor = Color.FromArgb(40, 48, 76);
            pnlStats.Controls.Add(lblSoTranDau);
            pnlStats.Controls.Add(lblGiaiDangDienRa);
            pnlStats.Controls.Add(lblTongSoGiai);
            pnlStats.Dock = DockStyle.Top;
            pnlStats.Location = new Point(0, 65);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1134, 45);
            pnlStats.TabIndex = 1;
            // 
            // lblSoTranDau
            // 
            lblSoTranDau.AutoSize = true;
            lblSoTranDau.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblSoTranDau.ForeColor = Color.FromArgb(120, 255, 120);
            lblSoTranDau.Location = new Point(480, 14);
            lblSoTranDau.Name = "lblSoTranDau";
            lblSoTranDau.Size = new Size(147, 17);
            lblSoTranDau.TabIndex = 2;
            lblSoTranDau.Text = "⚔️ Tổng số trận đấu: 0";
            // 
            // lblGiaiDangDienRa
            // 
            lblGiaiDangDienRa.AutoSize = true;
            lblGiaiDangDienRa.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblGiaiDangDienRa.ForeColor = Color.FromArgb(255, 215, 0);
            lblGiaiDangDienRa.Location = new Point(240, 14);
            lblGiaiDangDienRa.Name = "lblGiaiDangDienRa";
            lblGiaiDangDienRa.Size = new Size(139, 17);
            lblGiaiDangDienRa.TabIndex = 1;
            lblGiaiDangDienRa.Text = "🔥 Đang diễn ra: 0";
            // 
            // lblTongSoGiai
            // 
            lblTongSoGiai.AutoSize = true;
            lblTongSoGiai.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTongSoGiai.ForeColor = Color.White;
            lblTongSoGiai.Location = new Point(20, 14);
            lblTongSoGiai.Name = "lblTongSoGiai";
            lblTongSoGiai.Size = new Size(130, 17);
            lblTongSoGiai.TabIndex = 0;
            lblTongSoGiai.Text = "🏆 Tổng số giải: 0";
            // 
            // pnlLeft
            // 
            pnlLeft.AutoScroll = true;
            pnlLeft.Controls.Add(grpDieuPhoiMay);
            pnlLeft.Controls.Add(grpGiaiDau);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 110);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(360, 631);
            pnlLeft.TabIndex = 2;
            // 
            // grpDieuPhoiMay
            // 
            grpDieuPhoiMay.BackColor = Color.FromArgb(255, 250, 240);
            grpDieuPhoiMay.Controls.Add(btnMoKhoaMay);
            grpDieuPhoiMay.Controls.Add(btnKhoaMayGiaiDau);
            grpDieuPhoiMay.Controls.Add(clbDanMay);
            grpDieuPhoiMay.Controls.Add(lblSoMayDaChon);
            grpDieuPhoiMay.Controls.Add(lblChonDanMay);
            grpDieuPhoiMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpDieuPhoiMay.ForeColor = Color.FromArgb(139, 69, 19);
            grpDieuPhoiMay.Location = new Point(10, 395);
            grpDieuPhoiMay.Name = "grpDieuPhoiMay";
            grpDieuPhoiMay.Size = new Size(340, 295);
            grpDieuPhoiMay.TabIndex = 1;
            grpDieuPhoiMay.TabStop = false;
            grpDieuPhoiMay.Text = "Điều Phối Dàn Máy Thi Đấu";
            // 
            // btnMoKhoaMay
            // 
            btnMoKhoaMay.BackColor = Color.FromArgb(46, 139, 87);
            btnMoKhoaMay.FlatStyle = FlatStyle.Flat;
            btnMoKhoaMay.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnMoKhoaMay.ForeColor = Color.White;
            btnMoKhoaMay.Location = new Point(15, 246);
            btnMoKhoaMay.Name = "btnMoKhoaMay";
            btnMoKhoaMay.Size = new Size(305, 34);
            btnMoKhoaMay.TabIndex = 4;
            btnMoKhoaMay.Text = "🔓 GIẢI PHÓNG DÀN MÁY (Available)";
            btnMoKhoaMay.UseVisualStyleBackColor = false;
            // 
            // btnKhoaMayGiaiDau
            // 
            btnKhoaMayGiaiDau.BackColor = Color.FromArgb(218, 112, 214);
            btnKhoaMayGiaiDau.FlatStyle = FlatStyle.Flat;
            btnKhoaMayGiaiDau.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnKhoaMayGiaiDau.ForeColor = Color.Black;
            btnKhoaMayGiaiDau.Location = new Point(15, 205);
            btnKhoaMayGiaiDau.Name = "btnKhoaMayGiaiDau";
            btnKhoaMayGiaiDau.Size = new Size(305, 34);
            btnKhoaMayGiaiDau.TabIndex = 3;
            btnKhoaMayGiaiDau.Text = "🔒 KHÓA DÀN MÁY THI ĐẤU";
            btnKhoaMayGiaiDau.UseVisualStyleBackColor = false;
            // 
            // clbDanMay
            //
            clbDanMay.CheckOnClick = true;
            clbDanMay.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            clbDanMay.FormattingEnabled = true;
            clbDanMay.Location = new Point(15, 46);
            clbDanMay.Name = "clbDanMay";
            clbDanMay.Size = new Size(305, 128);
            clbDanMay.TabIndex = 1;
            //
            // lblSoMayDaChon
            //
            lblSoMayDaChon.AutoSize = true;
            lblSoMayDaChon.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblSoMayDaChon.Location = new Point(15, 180);
            lblSoMayDaChon.Name = "lblSoMayDaChon";
            lblSoMayDaChon.Size = new Size(268, 15);
            lblSoMayDaChon.TabIndex = 2;
            lblSoMayDaChon.Text = "Đã chọn: 0/5 máy (Sẵn sàng khóa: 0/5)";
            // 
            // lblChonDanMay
            // 
            lblChonDanMay.AutoSize = true;
            lblChonDanMay.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblChonDanMay.Location = new Point(15, 24);
            lblChonDanMay.Name = "lblChonDanMay";
            lblChonDanMay.Size = new Size(250, 15);
            lblChonDanMay.TabIndex = 0;
            lblChonDanMay.Text = "Chọn đúng 5 máy Available cho trận:";
            // 
            // grpGiaiDau
            // 
            grpGiaiDau.Controls.Add(btnLamMoi);
            grpGiaiDau.Controls.Add(btnXoaGiai);
            grpGiaiDau.Controls.Add(btnCapNhatGiai);
            grpGiaiDau.Controls.Add(btnTaoGiai);
            grpGiaiDau.Controls.Add(cboTournamentStatus);
            grpGiaiDau.Controls.Add(lblTournamentStatus);
            grpGiaiDau.Controls.Add(dtpStartDate);
            grpGiaiDau.Controls.Add(lblStartDate);
            grpGiaiDau.Controls.Add(txtPrizePool);
            grpGiaiDau.Controls.Add(lblPrizePool);
            grpGiaiDau.Controls.Add(cboFormatType);
            grpGiaiDau.Controls.Add(lblFormatType);
            grpGiaiDau.Controls.Add(txtGameTitle);
            grpGiaiDau.Controls.Add(lblGameTitle);
            grpGiaiDau.Controls.Add(txtTenGiai);
            grpGiaiDau.Controls.Add(lblTenGiai);
            grpGiaiDau.Controls.Add(txtTournamentId);
            grpGiaiDau.Controls.Add(lblTournamentId);
            grpGiaiDau.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpGiaiDau.Location = new Point(10, 10);
            grpGiaiDau.Name = "grpGiaiDau";
            grpGiaiDau.Size = new Size(340, 375);
            grpGiaiDau.TabIndex = 0;
            grpGiaiDau.TabStop = false;
            grpGiaiDau.Text = "Thông Tin Giải Đấu";
            // 
            // btnLamMoi
            // 
            btnLamMoi.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoi.FlatStyle = FlatStyle.Flat;
            btnLamMoi.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Location = new Point(175, 330);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(145, 30);
            btnLamMoi.TabIndex = 17;
            btnLamMoi.Text = "🔄 Làm Mới";
            btnLamMoi.UseVisualStyleBackColor = false;
            // 
            // btnXoaGiai
            // 
            btnXoaGiai.BackColor = Color.FromArgb(178, 34, 34);
            btnXoaGiai.FlatStyle = FlatStyle.Flat;
            btnXoaGiai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnXoaGiai.ForeColor = Color.White;
            btnXoaGiai.Location = new Point(15, 330);
            btnXoaGiai.Name = "btnXoaGiai";
            btnXoaGiai.Size = new Size(145, 30);
            btnXoaGiai.TabIndex = 16;
            btnXoaGiai.Text = "🗑️ Xóa Giải";
            btnXoaGiai.UseVisualStyleBackColor = false;
            // 
            // btnCapNhatGiai
            // 
            btnCapNhatGiai.BackColor = Color.FromArgb(30, 144, 255);
            btnCapNhatGiai.FlatStyle = FlatStyle.Flat;
            btnCapNhatGiai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnCapNhatGiai.ForeColor = Color.White;
            btnCapNhatGiai.Location = new Point(175, 290);
            btnCapNhatGiai.Name = "btnCapNhatGiai";
            btnCapNhatGiai.Size = new Size(145, 30);
            btnCapNhatGiai.TabIndex = 15;
            btnCapNhatGiai.Text = "✏️ Cập Nhật";
            btnCapNhatGiai.UseVisualStyleBackColor = false;
            // 
            // btnTaoGiai
            // 
            btnTaoGiai.BackColor = Color.FromArgb(34, 139, 34);
            btnTaoGiai.FlatStyle = FlatStyle.Flat;
            btnTaoGiai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnTaoGiai.ForeColor = Color.White;
            btnTaoGiai.Location = new Point(15, 290);
            btnTaoGiai.Name = "btnTaoGiai";
            btnTaoGiai.Size = new Size(145, 30);
            btnTaoGiai.TabIndex = 14;
            btnTaoGiai.Text = "➕ Tạo Giải Mới";
            btnTaoGiai.UseVisualStyleBackColor = false;
            // 
            // cboTournamentStatus
            // 
            cboTournamentStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTournamentStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboTournamentStatus.FormattingEnabled = true;
            cboTournamentStatus.Items.AddRange(new object[] { "Upcoming", "Ongoing", "Finished" });
            cboTournamentStatus.Location = new Point(120, 245);
            cboTournamentStatus.Name = "cboTournamentStatus";
            cboTournamentStatus.Size = new Size(200, 23);
            cboTournamentStatus.TabIndex = 13;
            // 
            // lblTournamentStatus
            // 
            lblTournamentStatus.AutoSize = true;
            lblTournamentStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblTournamentStatus.Location = new Point(15, 248);
            lblTournamentStatus.Name = "lblTournamentStatus";
            lblTournamentStatus.Size = new Size(62, 15);
            lblTournamentStatus.TabIndex = 12;
            lblTournamentStatus.Text = "Trạng thái:";
            // 
            // dtpStartDate
            // 
            dtpStartDate.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpStartDate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dtpStartDate.Format = DateTimePickerFormat.Custom;
            dtpStartDate.Location = new Point(120, 210);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(200, 23);
            dtpStartDate.TabIndex = 11;
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblStartDate.Location = new Point(15, 215);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(82, 15);
            lblStartDate.TabIndex = 10;
            lblStartDate.Text = "Ngày bắt đầu:";
            // 
            // txtPrizePool
            // 
            txtPrizePool.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtPrizePool.Location = new Point(120, 175);
            txtPrizePool.Name = "txtPrizePool";
            txtPrizePool.Size = new Size(200, 23);
            txtPrizePool.TabIndex = 9;
            txtPrizePool.Text = "10000000";
            // 
            // lblPrizePool
            // 
            lblPrizePool.AutoSize = true;
            lblPrizePool.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblPrizePool.Location = new Point(15, 178);
            lblPrizePool.Name = "lblPrizePool";
            lblPrizePool.Size = new Size(80, 15);
            lblPrizePool.TabIndex = 8;
            lblPrizePool.Text = "Tổng thưởng:";
            // 
            // cboFormatType
            // 
            cboFormatType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFormatType.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboFormatType.FormattingEnabled = true;
            cboFormatType.Items.AddRange(new object[] { "SingleElimination", "DoubleElimination", "RoundRobin" });
            cboFormatType.Location = new Point(120, 138);
            cboFormatType.Name = "cboFormatType";
            cboFormatType.Size = new Size(200, 23);
            cboFormatType.TabIndex = 7;
            // 
            // lblFormatType
            // 
            lblFormatType.AutoSize = true;
            lblFormatType.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblFormatType.Location = new Point(15, 142);
            lblFormatType.Name = "lblFormatType";
            lblFormatType.Size = new Size(81, 15);
            lblFormatType.TabIndex = 6;
            lblFormatType.Text = "Thể thức đấu:";
            // 
            // txtGameTitle
            // 
            txtGameTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtGameTitle.Location = new Point(120, 102);
            txtGameTitle.Name = "txtGameTitle";
            txtGameTitle.Size = new Size(200, 23);
            txtGameTitle.TabIndex = 5;
            // 
            // lblGameTitle
            // 
            lblGameTitle.AutoSize = true;
            lblGameTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblGameTitle.Location = new Point(15, 106);
            lblGameTitle.Name = "lblGameTitle";
            lblGameTitle.Size = new Size(59, 15);
            lblGameTitle.TabIndex = 4;
            lblGameTitle.Text = "Tựa game:";
            // 
            // txtTenGiai
            // 
            txtTenGiai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtTenGiai.Location = new Point(120, 66);
            txtTenGiai.Name = "txtTenGiai";
            txtTenGiai.Size = new Size(200, 23);
            txtTenGiai.TabIndex = 3;
            // 
            // lblTenGiai
            // 
            lblTenGiai.AutoSize = true;
            lblTenGiai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblTenGiai.Location = new Point(15, 70);
            lblTenGiai.Name = "lblTenGiai";
            lblTenGiai.Size = new Size(72, 15);
            lblTenGiai.TabIndex = 2;
            lblTenGiai.Text = "Tên giải đấu:";
            // 
            // txtTournamentId
            // 
            txtTournamentId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtTournamentId.Location = new Point(120, 30);
            txtTournamentId.Name = "txtTournamentId";
            txtTournamentId.ReadOnly = true;
            txtTournamentId.Size = new Size(200, 23);
            txtTournamentId.TabIndex = 1;
            // 
            // lblTournamentId
            // 
            lblTournamentId.AutoSize = true;
            lblTournamentId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblTournamentId.Location = new Point(15, 34);
            lblTournamentId.Name = "lblTournamentId";
            lblTournamentId.Size = new Size(49, 15);
            lblTournamentId.TabIndex = 0;
            lblTournamentId.Text = "Mã giải:";
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(splMain);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(360, 110);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(10);
            pnlMain.Size = new Size(774, 631);
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
            splMain.Panel1.Controls.Add(grpDanhSachGiai);
            // 
            // splMain.Panel2
            // 
            splMain.Panel2.Controls.Add(grpQuanLyTran);
            splMain.Size = new Size(754, 611);
            splMain.SplitterDistance = 250;
            splMain.TabIndex = 0;
            // 
            // grpDanhSachGiai
            // 
            grpDanhSachGiai.Controls.Add(dgvGiaiDau);
            grpDanhSachGiai.Dock = DockStyle.Fill;
            grpDanhSachGiai.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpDanhSachGiai.Location = new Point(0, 0);
            grpDanhSachGiai.Name = "grpDanhSachGiai";
            grpDanhSachGiai.Size = new Size(754, 250);
            grpDanhSachGiai.TabIndex = 0;
            grpDanhSachGiai.TabStop = false;
            grpDanhSachGiai.Text = "Danh Sách Giải Đấu LAN";
            // 
            // dgvGiaiDau
            // 
            dgvGiaiDau.AllowUserToAddRows = false;
            dgvGiaiDau.AllowUserToDeleteRows = false;
            dgvGiaiDau.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGiaiDau.BackgroundColor = Color.WhiteSmoke;
            dgvGiaiDau.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGiaiDau.Dock = DockStyle.Fill;
            dgvGiaiDau.Location = new Point(3, 20);
            dgvGiaiDau.MultiSelect = false;
            dgvGiaiDau.Name = "dgvGiaiDau";
            dgvGiaiDau.ReadOnly = true;
            dgvGiaiDau.RowHeadersVisible = false;
            dgvGiaiDau.RowTemplate.Height = 28;
            dgvGiaiDau.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGiaiDau.Size = new Size(748, 227);
            dgvGiaiDau.TabIndex = 0;
            // 
            // grpQuanLyTran
            // 
            grpQuanLyTran.Controls.Add(dgvTranDau);
            grpQuanLyTran.Controls.Add(pnlTranDauControl);
            grpQuanLyTran.Dock = DockStyle.Fill;
            grpQuanLyTran.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpQuanLyTran.Location = new Point(0, 0);
            grpQuanLyTran.Name = "grpQuanLyTran";
            grpQuanLyTran.Size = new Size(754, 357);
            grpQuanLyTran.TabIndex = 0;
            grpQuanLyTran.TabStop = false;
            grpQuanLyTran.Text = "Xếp Trận Đấu & Cập Nhật Tỉ Số";
            // 
            // dgvTranDau
            // 
            dgvTranDau.AllowUserToAddRows = false;
            dgvTranDau.AllowUserToDeleteRows = false;
            dgvTranDau.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTranDau.BackgroundColor = Color.WhiteSmoke;
            dgvTranDau.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTranDau.Dock = DockStyle.Fill;
            dgvTranDau.Location = new Point(3, 152);
            dgvTranDau.MultiSelect = false;
            dgvTranDau.Name = "dgvTranDau";
            dgvTranDau.ReadOnly = true;
            dgvTranDau.RowHeadersVisible = false;
            dgvTranDau.RowTemplate.Height = 28;
            dgvTranDau.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTranDau.Size = new Size(748, 202);
            dgvTranDau.TabIndex = 1;
            // 
            // pnlTranDauControl
            // 
            pnlTranDauControl.Controls.Add(btnCapNhatKetQua);
            pnlTranDauControl.Controls.Add(btnTaoVongSau);
            pnlTranDauControl.Controls.Add(btnThemTranDau);
            pnlTranDauControl.Controls.Add(cboMatchStatus);
            pnlTranDauControl.Controls.Add(lblMatchStatus);
            pnlTranDauControl.Controls.Add(txtScore);
            pnlTranDauControl.Controls.Add(lblScore);
            pnlTranDauControl.Controls.Add(cboWinnerTeam);
            pnlTranDauControl.Controls.Add(lblWinnerTeam);
            pnlTranDauControl.Controls.Add(txtTeam2);
            pnlTranDauControl.Controls.Add(lblTeam2);
            pnlTranDauControl.Controls.Add(txtTeam1);
            pnlTranDauControl.Controls.Add(lblTeam1);
            pnlTranDauControl.Controls.Add(txtRoundNumber);
            pnlTranDauControl.Controls.Add(lblRoundNumber);
            pnlTranDauControl.Dock = DockStyle.Top;
            pnlTranDauControl.Location = new Point(3, 20);
            pnlTranDauControl.Name = "pnlTranDauControl";
            pnlTranDauControl.Size = new Size(748, 132);
            pnlTranDauControl.TabIndex = 0;
            //
            // btnTaoVongSau
            //
            btnTaoVongSau.BackColor = Color.FromArgb(138, 43, 226);
            btnTaoVongSau.FlatStyle = FlatStyle.Flat;
            btnTaoVongSau.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnTaoVongSau.ForeColor = Color.White;
            btnTaoVongSau.Location = new Point(10, 90);
            btnTaoVongSau.Name = "btnTaoVongSau";
            btnTaoVongSau.Size = new Size(255, 32);
            btnTaoVongSau.TabIndex = 14;
            btnTaoVongSau.Text = "⚡ Tạo cặp đấu vòng sau";
            btnTaoVongSau.UseVisualStyleBackColor = false;
            // 
            // btnCapNhatKetQua
            // 
            btnCapNhatKetQua.BackColor = Color.FromArgb(0, 122, 204);
            btnCapNhatKetQua.FlatStyle = FlatStyle.Flat;
            btnCapNhatKetQua.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnCapNhatKetQua.ForeColor = Color.White;
            btnCapNhatKetQua.Location = new Point(590, 52);
            btnCapNhatKetQua.Name = "btnCapNhatKetQua";
            btnCapNhatKetQua.Size = new Size(145, 32);
            btnCapNhatKetQua.TabIndex = 13;
            btnCapNhatKetQua.Text = "🏆 Cập Nhật Tỉ Số";
            btnCapNhatKetQua.UseVisualStyleBackColor = false;
            // 
            // btnThemTranDau
            // 
            btnThemTranDau.BackColor = Color.FromArgb(46, 139, 87);
            btnThemTranDau.FlatStyle = FlatStyle.Flat;
            btnThemTranDau.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnThemTranDau.ForeColor = Color.White;
            btnThemTranDau.Location = new Point(590, 10);
            btnThemTranDau.Name = "btnThemTranDau";
            btnThemTranDau.Size = new Size(145, 32);
            btnThemTranDau.TabIndex = 12;
            btnThemTranDau.Text = "➕ Xếp Cặp Đấu";
            btnThemTranDau.UseVisualStyleBackColor = false;
            // 
            // cboMatchStatus
            // 
            cboMatchStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMatchStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboMatchStatus.FormattingEnabled = true;
            cboMatchStatus.Items.AddRange(new object[] { "Pending", "Playing", "Finished" });
            cboMatchStatus.Location = new Point(445, 56);
            cboMatchStatus.Name = "cboMatchStatus";
            cboMatchStatus.Size = new Size(125, 23);
            cboMatchStatus.TabIndex = 11;
            // 
            // lblMatchStatus
            // 
            lblMatchStatus.AutoSize = true;
            lblMatchStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblMatchStatus.Location = new Point(375, 60);
            lblMatchStatus.Name = "lblMatchStatus";
            lblMatchStatus.Size = new Size(62, 15);
            lblMatchStatus.TabIndex = 10;
            lblMatchStatus.Text = "Trạng thái:";
            // 
            // txtScore
            // 
            txtScore.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtScore.Location = new Point(275, 56);
            txtScore.Name = "txtScore";
            txtScore.Size = new Size(85, 23);
            txtScore.TabIndex = 9;
            txtScore.Text = "0 - 0";
            // 
            // lblScore
            // 
            lblScore.AutoSize = true;
            lblScore.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblScore.Location = new Point(225, 60);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(41, 15);
            lblScore.TabIndex = 8;
            lblScore.Text = "Tỉ số:";
            // 
            // cboWinnerTeam
            // 
            cboWinnerTeam.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWinnerTeam.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboWinnerTeam.FormattingEnabled = true;
            cboWinnerTeam.Location = new Point(80, 56);
            cboWinnerTeam.Name = "cboWinnerTeam";
            cboWinnerTeam.Size = new Size(130, 23);
            cboWinnerTeam.TabIndex = 7;
            // 
            // lblWinnerTeam
            // 
            lblWinnerTeam.AutoSize = true;
            lblWinnerTeam.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblWinnerTeam.Location = new Point(10, 60);
            lblWinnerTeam.Name = "lblWinnerTeam";
            lblWinnerTeam.Size = new Size(67, 15);
            lblWinnerTeam.TabIndex = 6;
            lblWinnerTeam.Text = "Đội thắng:";
            // 
            // txtTeam2
            // 
            txtTeam2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtTeam2.Location = new Point(445, 15);
            txtTeam2.Name = "txtTeam2";
            txtTeam2.Size = new Size(125, 23);
            txtTeam2.TabIndex = 5;
            // 
            // lblTeam2
            // 
            lblTeam2.AutoSize = true;
            lblTeam2.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblTeam2.Location = new Point(375, 18);
            lblTeam2.Name = "lblTeam2";
            lblTeam2.Size = new Size(41, 15);
            lblTeam2.TabIndex = 4;
            lblTeam2.Text = "Đội 2:";
            // 
            // txtTeam1
            // 
            txtTeam1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtTeam1.Location = new Point(230, 15);
            txtTeam1.Name = "txtTeam1";
            txtTeam1.Size = new Size(130, 23);
            txtTeam1.TabIndex = 3;
            // 
            // lblTeam1
            // 
            lblTeam1.AutoSize = true;
            lblTeam1.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblTeam1.Location = new Point(175, 18);
            lblTeam1.Name = "lblTeam1";
            lblTeam1.Size = new Size(41, 15);
            lblTeam1.TabIndex = 2;
            lblTeam1.Text = "Đội 1:";
            // 
            // txtRoundNumber
            // 
            txtRoundNumber.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtRoundNumber.Location = new Point(80, 15);
            txtRoundNumber.Name = "txtRoundNumber";
            txtRoundNumber.Size = new Size(80, 23);
            txtRoundNumber.TabIndex = 1;
            txtRoundNumber.Text = "1";
            // 
            // lblRoundNumber
            // 
            lblRoundNumber.AutoSize = true;
            lblRoundNumber.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblRoundNumber.Location = new Point(10, 18);
            lblRoundNumber.Name = "lblRoundNumber";
            lblRoundNumber.Size = new Size(61, 15);
            lblRoundNumber.TabIndex = 0;
            lblRoundNumber.Text = "Vòng đấu:";
            // 
            // FormTournament
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1134, 741);
            Controls.Add(pnlMain);
            Controls.Add(pnlLeft);
            Controls.Add(pnlStats);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(1000, 700);
            Name = "FormTournament";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Giải Đấu LAN Party & Xếp Trận - Cyber Game";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStats.ResumeLayout(false);
            pnlStats.PerformLayout();
            pnlLeft.ResumeLayout(false);
            grpDieuPhoiMay.ResumeLayout(false);
            grpDieuPhoiMay.PerformLayout();
            grpGiaiDau.ResumeLayout(false);
            grpGiaiDau.PerformLayout();
            pnlMain.ResumeLayout(false);
            splMain.Panel1.ResumeLayout(false);
            splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
            splMain.ResumeLayout(false);
            grpDanhSachGiai.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGiaiDau).EndInit();
            grpQuanLyTran.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTranDau).EndInit();
            pnlTranDauControl.ResumeLayout(false);
            pnlTranDauControl.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblSubtitle;
        private Label lblTitle;
        private Panel pnlStats;
        private Label lblSoTranDau;
        private Label lblGiaiDangDienRa;
        private Label lblTongSoGiai;
        private Panel pnlLeft;
        private GroupBox grpGiaiDau;
        private TextBox txtTournamentId;
        private Label lblTournamentId;
        private TextBox txtTenGiai;
        private Label lblTenGiai;
        private TextBox txtGameTitle;
        private Label lblGameTitle;
        private ComboBox cboFormatType;
        private Label lblFormatType;
        private TextBox txtPrizePool;
        private Label lblPrizePool;
        private Label lblStartDate;
        private DateTimePicker dtpStartDate;
        private ComboBox cboTournamentStatus;
        private Label lblTournamentStatus;
        private Button btnTaoGiai;
        private Button btnCapNhatGiai;
        private Button btnXoaGiai;
        private Button btnLamMoi;
        private GroupBox grpDieuPhoiMay;
        private Label lblChonDanMay;
        private CheckedListBox clbDanMay;
        private Label lblSoMayDaChon;
        private Button btnKhoaMayGiaiDau;
        private Button btnMoKhoaMay;
        private Panel pnlMain;
        private SplitContainer splMain;
        private GroupBox grpDanhSachGiai;
        private DataGridView dgvGiaiDau;
        private GroupBox grpQuanLyTran;
        private Panel pnlTranDauControl;
        private Button btnTaoVongSau;
        private Label lblRoundNumber;
        private TextBox txtRoundNumber;
        private Label lblTeam1;
        private TextBox txtTeam1;
        private Label lblTeam2;
        private TextBox txtTeam2;
        private Label lblWinnerTeam;
        private ComboBox cboWinnerTeam;
        private Label lblScore;
        private TextBox txtScore;
        private Label lblMatchStatus;
        private ComboBox cboMatchStatus;
        private Button btnThemTranDau;
        private Button btnCapNhatKetQua;
        private DataGridView dgvTranDau;
    }
}
