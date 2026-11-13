namespace GamingCenterManagementSystem
{
    partial class FormStation
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
            lblSoMayBaoTri = new Label();
            lblSoMayDangChoi = new Label();
            lblSoMayTrong = new Label();
            lblTongSoMay = new Label();
            grpThongTinMay = new GroupBox();
            btnLamMoi = new Button();
            btnMoMay = new Button();
            btnBaoTri = new Button();
            btnXoaMay = new Button();
            btnSuaMay = new Button();
            btnThemMay = new Button();
            cboTrangThai = new ComboBox();
            lblTrangThai = new Label();
            txtIpAddress = new TextBox();
            lblIpAddress = new Label();
            cboKhuVuc = new ComboBox();
            lblKhuVuc = new Label();
            txtStationName = new TextBox();
            lblStationName = new Label();
            txtStationId = new TextBox();
            lblStationId = new Label();
            pnlMain = new Panel();
            tabControlStation = new TabControl();
            tabMayTram = new TabPage();
            dgvMayTram = new DataGridView();
            pnlFilter = new Panel();
            btnApDungLoc = new Button();
            cboLocTrangThai = new ComboBox();
            lblLocTrangThai = new Label();
            cboLocKhuVuc = new ComboBox();
            lblLocKhuVuc = new Label();
            pnlChuyenMay = new Panel();
            btnChuyenMay = new Button();
            cboMayDich = new ComboBox();
            lblMayDich = new Label();
            lblMayNguon = new Label();
            lblChuyenMayTitle = new Label();
            tabKhuVuc = new TabPage();
            dgvKhuVuc = new DataGridView();
            grpThongTinKhuVuc = new GroupBox();
            btnLamMoiZone = new Button();
            btnXoaZone = new Button();
            btnSuaZone = new Button();
            btnThemZone = new Button();
            txtZoneDescription = new TextBox();
            lblZoneDescription = new Label();
            txtZonePrice = new TextBox();
            lblZonePrice = new Label();
            txtZoneName = new TextBox();
            lblZoneName = new Label();
            txtZoneId = new TextBox();
            lblZoneId = new Label();

            pnlHeader.SuspendLayout();
            pnlStats.SuspendLayout();
            grpThongTinMay.SuspendLayout();
            pnlMain.SuspendLayout();
            tabControlStation.SuspendLayout();
            tabMayTram.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMayTram).BeginInit();
            pnlFilter.SuspendLayout();
            pnlChuyenMay.SuspendLayout();
            tabKhuVuc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKhuVuc).BeginInit();
            grpThongTinKhuVuc.SuspendLayout();
            SuspendLayout();

            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(24, 30, 54);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1152, 87);
            pnlHeader.TabIndex = 0;

            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblSubtitle.ForeColor = Color.FromArgb(0, 190, 255);
            lblSubtitle.Location = new Point(23, 51);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(543, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Phân công: Thành viên 2 (Văn) - Nhánh Van-Tks: Quản lý máy trạm & khu vực";

            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(21, 13);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(531, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "QUẢN LÝ MÁY TRẠM & KHU VỰC PHÒNG MÁY";

            // 
            // pnlStats
            // 
            pnlStats.BackColor = Color.FromArgb(40, 48, 76);
            pnlStats.Controls.Add(lblSoMayBaoTri);
            pnlStats.Controls.Add(lblSoMayDangChoi);
            pnlStats.Controls.Add(lblSoMayTrong);
            pnlStats.Controls.Add(lblTongSoMay);
            pnlStats.Dock = DockStyle.Top;
            pnlStats.Location = new Point(0, 87);
            pnlStats.Margin = new Padding(3, 4, 3, 4);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1152, 60);
            pnlStats.TabIndex = 1;

            // 
            // lblSoMayBaoTri
            // 
            lblSoMayBaoTri.AutoSize = true;
            lblSoMayBaoTri.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSoMayBaoTri.ForeColor = Color.FromArgb(255, 128, 128);
            lblSoMayBaoTri.Location = new Point(651, 19);
            lblSoMayBaoTri.Name = "lblSoMayBaoTri";
            lblSoMayBaoTri.Size = new Size(149, 21);
            lblSoMayBaoTri.TabIndex = 3;
            lblSoMayBaoTri.Text = "🛠️ Đang bảo trì: 0";

            // 
            // lblSoMayDangChoi
            // 
            lblSoMayDangChoi.AutoSize = true;
            lblSoMayDangChoi.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSoMayDangChoi.ForeColor = Color.FromArgb(255, 215, 0);
            lblSoMayDangChoi.Location = new Point(423, 19);
            lblSoMayDangChoi.Name = "lblSoMayDangChoi";
            lblSoMayDangChoi.Size = new Size(161, 21);
            lblSoMayDangChoi.TabIndex = 2;
            lblSoMayDangChoi.Text = "🎮 Đang sử dụng: 0";

            // 
            // lblSoMayTrong
            // 
            lblSoMayTrong.AutoSize = true;
            lblSoMayTrong.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSoMayTrong.ForeColor = Color.FromArgb(120, 255, 120);
            lblSoMayTrong.Location = new Point(217, 19);
            lblSoMayTrong.Name = "lblSoMayTrong";
            lblSoMayTrong.Size = new Size(157, 21);
            lblSoMayTrong.TabIndex = 1;
            lblSoMayTrong.Text = "🟢 Máy sẵn sàng: 0";

            // 
            // lblTongSoMay
            // 
            lblTongSoMay.AutoSize = true;
            lblTongSoMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTongSoMay.ForeColor = Color.White;
            lblTongSoMay.Location = new Point(23, 19);
            lblTongSoMay.Name = "lblTongSoMay";
            lblTongSoMay.Size = new Size(151, 21);
            lblTongSoMay.TabIndex = 0;
            lblTongSoMay.Text = "🖥️ Tổng số máy: 0";

            // 
            // grpThongTinMay
            // 
            grpThongTinMay.Controls.Add(btnLamMoi);
            grpThongTinMay.Controls.Add(btnMoMay);
            grpThongTinMay.Controls.Add(btnBaoTri);
            grpThongTinMay.Controls.Add(btnXoaMay);
            grpThongTinMay.Controls.Add(btnSuaMay);
            grpThongTinMay.Controls.Add(btnThemMay);
            grpThongTinMay.Controls.Add(cboTrangThai);
            grpThongTinMay.Controls.Add(lblTrangThai);
            grpThongTinMay.Controls.Add(txtIpAddress);
            grpThongTinMay.Controls.Add(lblIpAddress);
            grpThongTinMay.Controls.Add(cboKhuVuc);
            grpThongTinMay.Controls.Add(lblKhuVuc);
            grpThongTinMay.Controls.Add(txtStationName);
            grpThongTinMay.Controls.Add(lblStationName);
            grpThongTinMay.Controls.Add(txtStationId);
            grpThongTinMay.Controls.Add(lblStationId);
            grpThongTinMay.Dock = DockStyle.Left;
            grpThongTinMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grpThongTinMay.Location = new Point(0, 147);
            grpThongTinMay.Margin = new Padding(3, 4, 3, 4);
            grpThongTinMay.Name = "grpThongTinMay";
            grpThongTinMay.Padding = new Padding(3, 4, 3, 4);
            grpThongTinMay.Size = new Size(366, 761);
            grpThongTinMay.TabIndex = 2;
            grpThongTinMay.TabStop = false;
            grpThongTinMay.Text = "Thông Tin Máy Trạm";

            // 
            // btnLamMoi
            // 
            btnLamMoi.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoi.FlatStyle = FlatStyle.Flat;
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Location = new Point(183, 513);
            btnLamMoi.Margin = new Padding(3, 4, 3, 4);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(154, 45);
            btnLamMoi.TabIndex = 15;
            btnLamMoi.Text = "🔄 Làm Mới";
            btnLamMoi.UseVisualStyleBackColor = false;

            // 
            // btnMoMay
            // 
            btnMoMay.BackColor = Color.FromArgb(46, 139, 87);
            btnMoMay.FlatStyle = FlatStyle.Flat;
            btnMoMay.ForeColor = Color.White;
            btnMoMay.Location = new Point(183, 447);
            btnMoMay.Margin = new Padding(3, 4, 3, 4);
            btnMoMay.Name = "btnMoMay";
            btnMoMay.Size = new Size(154, 45);
            btnMoMay.TabIndex = 14;
            btnMoMay.Text = "🟢 Mở Sẵn Sàng";
            btnMoMay.UseVisualStyleBackColor = false;

            // 
            // btnBaoTri
            // 
            btnBaoTri.BackColor = Color.FromArgb(204, 102, 0);
            btnBaoTri.FlatStyle = FlatStyle.Flat;
            btnBaoTri.ForeColor = Color.White;
            btnBaoTri.Location = new Point(17, 513);
            btnBaoTri.Margin = new Padding(3, 4, 3, 4);
            btnBaoTri.Name = "btnBaoTri";
            btnBaoTri.Size = new Size(154, 45);
            btnBaoTri.TabIndex = 13;
            btnBaoTri.Text = "🛠️ Đặt Bảo Trì";
            btnBaoTri.UseVisualStyleBackColor = false;

            // 
            // btnXoaMay
            // 
            btnXoaMay.BackColor = Color.FromArgb(178, 34, 34);
            btnXoaMay.FlatStyle = FlatStyle.Flat;
            btnXoaMay.ForeColor = Color.White;
            btnXoaMay.Location = new Point(17, 447);
            btnXoaMay.Margin = new Padding(3, 4, 3, 4);
            btnXoaMay.Name = "btnXoaMay";
            btnXoaMay.Size = new Size(154, 45);
            btnXoaMay.TabIndex = 12;
            btnXoaMay.Text = "🗑️ Xóa Máy";
            btnXoaMay.UseVisualStyleBackColor = false;

            // 
            // btnSuaMay
            // 
            btnSuaMay.BackColor = Color.FromArgb(30, 144, 255);
            btnSuaMay.FlatStyle = FlatStyle.Flat;
            btnSuaMay.ForeColor = Color.White;
            btnSuaMay.Location = new Point(183, 380);
            btnSuaMay.Margin = new Padding(3, 4, 3, 4);
            btnSuaMay.Name = "btnSuaMay";
            btnSuaMay.Size = new Size(154, 45);
            btnSuaMay.TabIndex = 11;
            btnSuaMay.Text = "✏️ Cập Nhật";
            btnSuaMay.UseVisualStyleBackColor = false;

            // 
            // btnThemMay
            // 
            btnThemMay.BackColor = Color.FromArgb(34, 139, 34);
            btnThemMay.FlatStyle = FlatStyle.Flat;
            btnThemMay.ForeColor = Color.White;
            btnThemMay.Location = new Point(17, 380);
            btnThemMay.Margin = new Padding(3, 4, 3, 4);
            btnThemMay.Name = "btnThemMay";
            btnThemMay.Size = new Size(154, 45);
            btnThemMay.TabIndex = 10;
            btnThemMay.Text = "➕ Thêm Máy";
            btnThemMay.UseVisualStyleBackColor = false;

            // 
            // cboTrangThai
            // 
            cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTrangThai.Font = new Font("Segoe UI", 9.5F);
            cboTrangThai.FormattingEnabled = true;
            cboTrangThai.Items.AddRange(new object[] { "Available", "InUse", "Maintenance", "TournamentLocked" });
            cboTrangThai.Location = new Point(126, 287);
            cboTrangThai.Margin = new Padding(3, 4, 3, 4);
            cboTrangThai.Name = "cboTrangThai";
            cboTrangThai.Size = new Size(211, 29);
            cboTrangThai.TabIndex = 9;

            // 
            // lblTrangThai
            // 
            lblTrangThai.AutoSize = true;
            lblTrangThai.Font = new Font("Segoe UI", 9F);
            lblTrangThai.Location = new Point(17, 293);
            lblTrangThai.Name = "lblTrangThai";
            lblTrangThai.Size = new Size(78, 20);
            lblTrangThai.TabIndex = 8;
            lblTrangThai.Text = "Trạng thái:";

            // 
            // txtIpAddress
            // 
            txtIpAddress.Font = new Font("Segoe UI", 9.5F);
            txtIpAddress.Location = new Point(126, 227);
            txtIpAddress.Margin = new Padding(3, 4, 3, 4);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(211, 29);
            txtIpAddress.TabIndex = 7;

            // 
            // lblIpAddress
            // 
            lblIpAddress.AutoSize = true;
            lblIpAddress.Font = new Font("Segoe UI", 9F);
            lblIpAddress.Location = new Point(17, 233);
            lblIpAddress.Name = "lblIpAddress";
            lblIpAddress.Size = new Size(74, 20);
            lblIpAddress.TabIndex = 6;
            lblIpAddress.Text = "Địa chỉ IP:";

            // 
            // cboKhuVuc
            // 
            cboKhuVuc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboKhuVuc.Font = new Font("Segoe UI", 9.5F);
            cboKhuVuc.FormattingEnabled = true;
            cboKhuVuc.Location = new Point(126, 167);
            cboKhuVuc.Margin = new Padding(3, 4, 3, 4);
            cboKhuVuc.Name = "cboKhuVuc";
            cboKhuVuc.Size = new Size(211, 29);
            cboKhuVuc.TabIndex = 5;

            // 
            // lblKhuVuc
            // 
            lblKhuVuc.AutoSize = true;
            lblKhuVuc.Font = new Font("Segoe UI", 9F);
            lblKhuVuc.Location = new Point(17, 173);
            lblKhuVuc.Name = "lblKhuVuc";
            lblKhuVuc.Size = new Size(64, 20);
            lblKhuVuc.TabIndex = 4;
            lblKhuVuc.Text = "Khu vực:";

            // 
            // txtStationName
            // 
            txtStationName.Font = new Font("Segoe UI", 9.5F);
            txtStationName.Location = new Point(126, 107);
            txtStationName.Margin = new Padding(3, 4, 3, 4);
            txtStationName.Name = "txtStationName";
            txtStationName.Size = new Size(211, 29);
            txtStationName.TabIndex = 3;

            // 
            // lblStationName
            // 
            lblStationName.AutoSize = true;
            lblStationName.Font = new Font("Segoe UI", 9F);
            lblStationName.Location = new Point(17, 113);
            lblStationName.Name = "lblStationName";
            lblStationName.Size = new Size(67, 20);
            lblStationName.TabIndex = 2;
            lblStationName.Text = "Tên máy:";

            // 
            // txtStationId
            // 
            txtStationId.Font = new Font("Segoe UI", 9.5F);
            txtStationId.Location = new Point(126, 47);
            txtStationId.Margin = new Padding(3, 4, 3, 4);
            txtStationId.Name = "txtStationId";
            txtStationId.ReadOnly = true;
            txtStationId.Size = new Size(211, 29);
            txtStationId.TabIndex = 1;

            // 
            // lblStationId
            // 
            lblStationId.AutoSize = true;
            lblStationId.Font = new Font("Segoe UI", 9F);
            lblStationId.Location = new Point(17, 53);
            lblStationId.Name = "lblStationId";
            lblStationId.Size = new Size(65, 20);
            lblStationId.TabIndex = 0;
            lblStationId.Text = "Mã máy:";

            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(tabControlStation);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(366, 147);
            pnlMain.Margin = new Padding(3, 4, 3, 4);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(11, 13, 11, 13);
            pnlMain.Size = new Size(786, 761);
            pnlMain.TabIndex = 3;

            // 
            // tabControlStation
            // 
            tabControlStation.Controls.Add(tabMayTram);
            tabControlStation.Controls.Add(tabKhuVuc);
            tabControlStation.Dock = DockStyle.Fill;
            tabControlStation.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            tabControlStation.Location = new Point(11, 13);
            tabControlStation.Name = "tabControlStation";
            tabControlStation.SelectedIndex = 0;
            tabControlStation.Size = new Size(764, 735);
            tabControlStation.TabIndex = 0;

            // 
            // tabMayTram
            // 
            tabMayTram.BackColor = Color.FromArgb(245, 247, 250);
            tabMayTram.Controls.Add(dgvMayTram);
            tabMayTram.Controls.Add(pnlFilter);
            tabMayTram.Controls.Add(pnlChuyenMay);
            tabMayTram.Location = new Point(4, 30);
            tabMayTram.Name = "tabMayTram";
            tabMayTram.Padding = new Padding(4);
            tabMayTram.Size = new Size(756, 701);
            tabMayTram.TabIndex = 0;
            tabMayTram.Text = "🖥️ Danh Sách Máy Trạm";

            // 
            // dgvMayTram (Add trước pnlFilter và pnlChuyenMay để Dock.Fill chuẩn Z-order)
            // 
            dgvMayTram.AllowUserToAddRows = false;
            dgvMayTram.AllowUserToDeleteRows = false;
            dgvMayTram.AllowUserToResizeColumns = false;
            dgvMayTram.AllowUserToResizeRows = false;
            dgvMayTram.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMayTram.BackgroundColor = Color.WhiteSmoke;
            dgvMayTram.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMayTram.Dock = DockStyle.Fill;
            dgvMayTram.Location = new Point(4, 64);
            dgvMayTram.Margin = new Padding(3, 4, 3, 4);
            dgvMayTram.MultiSelect = false;
            dgvMayTram.Name = "dgvMayTram";
            dgvMayTram.ReadOnly = true;
            dgvMayTram.RowHeadersVisible = false;
            dgvMayTram.RowHeadersWidth = 51;
            dgvMayTram.RowTemplate.Height = 28;
            dgvMayTram.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMayTram.Size = new Size(748, 557);
            dgvMayTram.TabIndex = 1;

            // 
            // pnlFilter
            // 
            pnlFilter.Controls.Add(btnApDungLoc);
            pnlFilter.Controls.Add(cboLocTrangThai);
            pnlFilter.Controls.Add(lblLocTrangThai);
            pnlFilter.Controls.Add(cboLocKhuVuc);
            pnlFilter.Controls.Add(lblLocKhuVuc);
            pnlFilter.Dock = DockStyle.Top;
            pnlFilter.Location = new Point(4, 4);
            pnlFilter.Margin = new Padding(3, 4, 3, 4);
            pnlFilter.Name = "pnlFilter";
            pnlFilter.Size = new Size(748, 60);
            pnlFilter.TabIndex = 0;

            // 
            // btnApDungLoc
            // 
            btnApDungLoc.BackColor = Color.FromArgb(0, 122, 204);
            btnApDungLoc.FlatStyle = FlatStyle.Flat;
            btnApDungLoc.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnApDungLoc.ForeColor = Color.White;
            btnApDungLoc.Location = new Point(460, 10);
            btnApDungLoc.Margin = new Padding(3, 4, 3, 4);
            btnApDungLoc.Name = "btnApDungLoc";
            btnApDungLoc.Size = new Size(135, 38);
            btnApDungLoc.TabIndex = 4;
            btnApDungLoc.Text = "🔍 Lọc Dữ Liệu";
            btnApDungLoc.UseVisualStyleBackColor = false;

            // 
            // cboLocTrangThai
            // 
            cboLocTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocTrangThai.Font = new Font("Segoe UI", 9F);
            cboLocTrangThai.FormattingEnabled = true;
            cboLocTrangThai.Location = new Point(305, 15);
            cboLocTrangThai.Margin = new Padding(3, 4, 3, 4);
            cboLocTrangThai.Name = "cboLocTrangThai";
            cboLocTrangThai.Size = new Size(145, 28);
            cboLocTrangThai.TabIndex = 3;

            // 
            // lblLocTrangThai
            // 
            lblLocTrangThai.AutoSize = true;
            lblLocTrangThai.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLocTrangThai.Location = new Point(225, 19);
            lblLocTrangThai.Name = "lblLocTrangThai";
            lblLocTrangThai.Size = new Size(80, 20);
            lblLocTrangThai.TabIndex = 2;
            lblLocTrangThai.Text = "Trạng thái:";

            // 
            // cboLocKhuVuc
            // 
            cboLocKhuVuc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocKhuVuc.Font = new Font("Segoe UI", 9F);
            cboLocKhuVuc.FormattingEnabled = true;
            cboLocKhuVuc.Location = new Point(75, 15);
            cboLocKhuVuc.Margin = new Padding(3, 4, 3, 4);
            cboLocKhuVuc.Name = "cboLocKhuVuc";
            cboLocKhuVuc.Size = new Size(142, 28);
            cboLocKhuVuc.TabIndex = 1;

            // 
            // lblLocKhuVuc
            // 
            lblLocKhuVuc.AutoSize = true;
            lblLocKhuVuc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLocKhuVuc.Location = new Point(6, 19);
            lblLocKhuVuc.Name = "lblLocKhuVuc";
            lblLocKhuVuc.Size = new Size(68, 20);
            lblLocKhuVuc.TabIndex = 0;
            lblLocKhuVuc.Text = "Khu vực:";

            // 
            // pnlChuyenMay (Dark Theme đồng bộ pnlStats)
            // 
            pnlChuyenMay.BackColor = Color.FromArgb(40, 48, 76);
            pnlChuyenMay.Controls.Add(btnChuyenMay);
            pnlChuyenMay.Controls.Add(cboMayDich);
            pnlChuyenMay.Controls.Add(lblMayDich);
            pnlChuyenMay.Controls.Add(lblMayNguon);
            pnlChuyenMay.Controls.Add(lblChuyenMayTitle);
            pnlChuyenMay.Dock = DockStyle.Bottom;
            pnlChuyenMay.Location = new Point(4, 621);
            pnlChuyenMay.Name = "pnlChuyenMay";
            pnlChuyenMay.Size = new Size(748, 76);
            pnlChuyenMay.TabIndex = 2;

            // 
            // lblChuyenMayTitle
            // 
            lblChuyenMayTitle.AutoSize = true;
            lblChuyenMayTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblChuyenMayTitle.ForeColor = Color.FromArgb(0, 190, 255);
            lblChuyenMayTitle.Location = new Point(12, 10);
            lblChuyenMayTitle.Name = "lblChuyenMayTitle";
            lblChuyenMayTitle.Size = new Size(202, 21);
            lblChuyenMayTitle.TabIndex = 0;
            lblChuyenMayTitle.Text = "🔀 CHUYỂN MÁY ĐANG CHƠI:";

            // 
            // lblMayNguon
            // 
            lblMayNguon.AutoSize = true;
            lblMayNguon.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblMayNguon.ForeColor = Color.White;
            lblMayNguon.Location = new Point(265, 10);
            lblMayNguon.Name = "lblMayNguon";
            lblMayNguon.Size = new Size(305, 21);
            lblMayNguon.TabIndex = 1;
            lblMayNguon.Text = "Máy nguồn: Chọn máy đang InUse để chuyển";

            // 
            // lblMayDich
            // 
            lblMayDich.AutoSize = true;
            lblMayDich.Font = new Font("Segoe UI", 9F);
            lblMayDich.ForeColor = Color.White;
            lblMayDich.Location = new Point(12, 43);
            lblMayDich.Name = "lblMayDich";
            lblMayDich.Size = new Size(158, 20);
            lblMayDich.TabIndex = 2;
            lblMayDich.Text = "Chuyển sang máy trống:";

            // 
            // cboMayDich
            // 
            cboMayDich.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMayDich.Font = new Font("Segoe UI", 9F);
            cboMayDich.FormattingEnabled = true;
            cboMayDich.Location = new Point(175, 40);
            cboMayDich.Name = "cboMayDich";
            cboMayDich.Size = new Size(260, 28);
            cboMayDich.TabIndex = 3;

            // 
            // btnChuyenMay
            // 
            btnChuyenMay.BackColor = Color.FromArgb(204, 102, 0);
            btnChuyenMay.FlatStyle = FlatStyle.Flat;
            btnChuyenMay.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnChuyenMay.ForeColor = Color.White;
            btnChuyenMay.Location = new Point(450, 36);
            btnChuyenMay.Name = "btnChuyenMay";
            btnChuyenMay.Size = new Size(145, 32);
            btnChuyenMay.TabIndex = 4;
            btnChuyenMay.Text = "🔀 Chuyển Máy";
            btnChuyenMay.UseVisualStyleBackColor = false;

            // 
            // tabKhuVuc
            // 
            tabKhuVuc.BackColor = Color.FromArgb(245, 247, 250);
            tabKhuVuc.Controls.Add(dgvKhuVuc);
            tabKhuVuc.Controls.Add(grpThongTinKhuVuc);
            tabKhuVuc.Location = new Point(4, 30);
            tabKhuVuc.Name = "tabKhuVuc";
            tabKhuVuc.Padding = new Padding(4);
            tabKhuVuc.Size = new Size(756, 701);
            tabKhuVuc.TabIndex = 1;
            tabKhuVuc.Text = "🏷️ Khu Vực & Bảng Giá";

            // 
            // dgvKhuVuc (Add trước grpThongTinKhuVuc để Dock.Fill chuẩn)
            // 
            dgvKhuVuc.AllowUserToAddRows = false;
            dgvKhuVuc.AllowUserToDeleteRows = false;
            dgvKhuVuc.AllowUserToResizeColumns = false;
            dgvKhuVuc.AllowUserToResizeRows = false;
            dgvKhuVuc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKhuVuc.BackgroundColor = Color.WhiteSmoke;
            dgvKhuVuc.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKhuVuc.Dock = DockStyle.Fill;
            dgvKhuVuc.Location = new Point(4, 199);
            dgvKhuVuc.MultiSelect = false;
            dgvKhuVuc.Name = "dgvKhuVuc";
            dgvKhuVuc.ReadOnly = true;
            dgvKhuVuc.RowHeadersVisible = false;
            dgvKhuVuc.RowTemplate.Height = 28;
            dgvKhuVuc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKhuVuc.Size = new Size(748, 498);
            dgvKhuVuc.TabIndex = 1;

            // 
            // grpThongTinKhuVuc
            // 
            grpThongTinKhuVuc.Controls.Add(btnLamMoiZone);
            grpThongTinKhuVuc.Controls.Add(btnXoaZone);
            grpThongTinKhuVuc.Controls.Add(btnSuaZone);
            grpThongTinKhuVuc.Controls.Add(btnThemZone);
            grpThongTinKhuVuc.Controls.Add(txtZoneDescription);
            grpThongTinKhuVuc.Controls.Add(lblZoneDescription);
            grpThongTinKhuVuc.Controls.Add(txtZonePrice);
            grpThongTinKhuVuc.Controls.Add(lblZonePrice);
            grpThongTinKhuVuc.Controls.Add(txtZoneName);
            grpThongTinKhuVuc.Controls.Add(lblZoneName);
            grpThongTinKhuVuc.Controls.Add(txtZoneId);
            grpThongTinKhuVuc.Controls.Add(lblZoneId);
            grpThongTinKhuVuc.Dock = DockStyle.Top;
            grpThongTinKhuVuc.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grpThongTinKhuVuc.Location = new Point(4, 4);
            grpThongTinKhuVuc.Name = "grpThongTinKhuVuc";
            grpThongTinKhuVuc.Size = new Size(748, 195);
            grpThongTinKhuVuc.TabIndex = 0;
            grpThongTinKhuVuc.TabStop = false;
            grpThongTinKhuVuc.Text = "Quản Lý Khu Vực & Đơn Giá Giờ";

            // 
            // lblZoneId & txtZoneId
            // 
            lblZoneId.AutoSize = true;
            lblZoneId.Font = new Font("Segoe UI", 9F);
            lblZoneId.Location = new Point(15, 30);
            lblZoneId.Name = "lblZoneId";
            lblZoneId.Size = new Size(60, 20);
            lblZoneId.TabIndex = 0;
            lblZoneId.Text = "Mã khu:";

            txtZoneId.Font = new Font("Segoe UI", 9.5F);
            txtZoneId.Location = new Point(115, 27);
            txtZoneId.Name = "txtZoneId";
            txtZoneId.ReadOnly = true;
            txtZoneId.Size = new Size(90, 29);
            txtZoneId.TabIndex = 1;

            // 
            // lblZoneName & txtZoneName
            // 
            lblZoneName.AutoSize = true;
            lblZoneName.Font = new Font("Segoe UI", 9F);
            lblZoneName.Location = new Point(225, 30);
            lblZoneName.Name = "lblZoneName";
            lblZoneName.Size = new Size(88, 20);
            lblZoneName.TabIndex = 2;
            lblZoneName.Text = "Tên khu vực:";

            txtZoneName.Font = new Font("Segoe UI", 9.5F);
            txtZoneName.Location = new Point(320, 27);
            txtZoneName.Name = "txtZoneName";
            txtZoneName.Size = new Size(260, 29);
            txtZoneName.TabIndex = 3;

            // 
            // lblZonePrice & txtZonePrice
            // 
            lblZonePrice.AutoSize = true;
            lblZonePrice.Font = new Font("Segoe UI", 9F);
            lblZonePrice.Location = new Point(15, 68);
            lblZonePrice.Name = "lblZonePrice";
            lblZonePrice.Size = new Size(92, 20);
            lblZonePrice.TabIndex = 4;
            lblZonePrice.Text = "Giá / giờ (đ):";

            txtZonePrice.Font = new Font("Segoe UI", 9.5F);
            txtZonePrice.Location = new Point(115, 65);
            txtZonePrice.Name = "txtZonePrice";
            txtZonePrice.Size = new Size(170, 29);
            txtZonePrice.TabIndex = 5;

            // 
            // lblZoneDescription & txtZoneDescription
            // 
            lblZoneDescription.AutoSize = true;
            lblZoneDescription.Font = new Font("Segoe UI", 9F);
            lblZoneDescription.Location = new Point(15, 106);
            lblZoneDescription.Name = "lblZoneDescription";
            lblZoneDescription.Size = new Size(51, 20);
            lblZoneDescription.TabIndex = 6;
            lblZoneDescription.Text = "Mô tả:";

            txtZoneDescription.Font = new Font("Segoe UI", 9.5F);
            txtZoneDescription.Location = new Point(115, 103);
            txtZoneDescription.Name = "txtZoneDescription";
            txtZoneDescription.Size = new Size(465, 29);
            txtZoneDescription.TabIndex = 7;

            // 
            // 4 nút CRUD Zone (Tối đa X = 580, không bị cắt ở MinimumSize 1026x784)
            // 
            btnThemZone.BackColor = Color.FromArgb(34, 139, 34);
            btnThemZone.FlatStyle = FlatStyle.Flat;
            btnThemZone.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnThemZone.ForeColor = Color.White;
            btnThemZone.Location = new Point(15, 145);
            btnThemZone.Name = "btnThemZone";
            btnThemZone.Size = new Size(130, 38);
            btnThemZone.TabIndex = 8;
            btnThemZone.Text = "➕ Thêm Khu";
            btnThemZone.UseVisualStyleBackColor = false;

            btnSuaZone.BackColor = Color.FromArgb(30, 144, 255);
            btnSuaZone.FlatStyle = FlatStyle.Flat;
            btnSuaZone.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSuaZone.ForeColor = Color.White;
            btnSuaZone.Location = new Point(155, 145);
            btnSuaZone.Name = "btnSuaZone";
            btnSuaZone.Size = new Size(130, 38);
            btnSuaZone.TabIndex = 9;
            btnSuaZone.Text = "✏️ Cập Nhật Khu";
            btnSuaZone.UseVisualStyleBackColor = false;

            btnXoaZone.BackColor = Color.FromArgb(178, 34, 34);
            btnXoaZone.FlatStyle = FlatStyle.Flat;
            btnXoaZone.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnXoaZone.ForeColor = Color.White;
            btnXoaZone.Location = new Point(295, 145);
            btnXoaZone.Name = "btnXoaZone";
            btnXoaZone.Size = new Size(130, 38);
            btnXoaZone.TabIndex = 10;
            btnXoaZone.Text = "🗑️ Xóa Khu";
            btnXoaZone.UseVisualStyleBackColor = false;

            btnLamMoiZone.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoiZone.FlatStyle = FlatStyle.Flat;
            btnLamMoiZone.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLamMoiZone.ForeColor = Color.White;
            btnLamMoiZone.Location = new Point(435, 145);
            btnLamMoiZone.Name = "btnLamMoiZone";
            btnLamMoiZone.Size = new Size(145, 38);
            btnLamMoiZone.TabIndex = 11;
            btnLamMoiZone.Text = "🔄 Làm Mới";
            btnLamMoiZone.UseVisualStyleBackColor = false;

            // 
            // FormStation
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1152, 908);
            Controls.Add(pnlMain);
            Controls.Add(grpThongTinMay);
            Controls.Add(pnlStats);
            Controls.Add(pnlHeader);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1026, 784);
            Name = "FormStation";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Máy Trạm & Khu Vực - Cyber Game";

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStats.ResumeLayout(false);
            pnlStats.PerformLayout();
            grpThongTinMay.ResumeLayout(false);
            grpThongTinMay.PerformLayout();
            pnlMain.ResumeLayout(false);
            tabControlStation.ResumeLayout(false);
            tabMayTram.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMayTram).EndInit();
            pnlFilter.ResumeLayout(false);
            pnlFilter.PerformLayout();
            pnlChuyenMay.ResumeLayout(false);
            pnlChuyenMay.PerformLayout();
            tabKhuVuc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvKhuVuc).EndInit();
            grpThongTinKhuVuc.ResumeLayout(false);
            grpThongTinKhuVuc.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblSubtitle;
        private Label lblTitle;
        private Panel pnlStats;
        private Label lblSoMayBaoTri;
        private Label lblSoMayDangChoi;
        private Label lblSoMayTrong;
        private Label lblTongSoMay;
        private GroupBox grpThongTinMay;
        private TextBox txtStationId;
        private Label lblStationId;
        private TextBox txtStationName;
        private Label lblStationName;
        private ComboBox cboKhuVuc;
        private Label lblKhuVuc;
        private TextBox txtIpAddress;
        private Label lblIpAddress;
        private ComboBox cboTrangThai;
        private Label lblTrangThai;
        private Button btnThemMay;
        private Button btnSuaMay;
        private Button btnXoaMay;
        private Button btnBaoTri;
        private Button btnMoMay;
        private Button btnLamMoi;
        private Panel pnlMain;
        private TabControl tabControlStation;
        private TabPage tabMayTram;
        private Panel pnlFilter;
        private Label lblLocKhuVuc;
        private ComboBox cboLocKhuVuc;
        private Label lblLocTrangThai;
        private ComboBox cboLocTrangThai;
        private Button btnApDungLoc;
        private DataGridView dgvMayTram;
        private Panel pnlChuyenMay;
        private Label lblChuyenMayTitle;
        private Label lblMayNguon;
        private Label lblMayDich;
        private ComboBox cboMayDich;
        private Button btnChuyenMay;
        private TabPage tabKhuVuc;
        private GroupBox grpThongTinKhuVuc;
        private Label lblZoneId;
        private TextBox txtZoneId;
        private Label lblZoneName;
        private TextBox txtZoneName;
        private Label lblZonePrice;
        private TextBox txtZonePrice;
        private Label lblZoneDescription;
        private TextBox txtZoneDescription;
        private Button btnThemZone;
        private Button btnSuaZone;
        private Button btnXoaZone;
        private Button btnLamMoiZone;
        private DataGridView dgvKhuVuc;
    }
}