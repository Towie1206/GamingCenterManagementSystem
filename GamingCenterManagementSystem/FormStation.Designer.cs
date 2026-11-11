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
            dgvMayTram = new DataGridView();
            pnlFilter = new Panel();
            btnApDungLoc = new Button();
            cboLocTrangThai = new ComboBox();
            lblLocTrangThai = new Label();
            cboLocKhuVuc = new ComboBox();
            lblLocKhuVuc = new Label();
            pnlHeader.SuspendLayout();
            pnlStats.SuspendLayout();
            grpThongTinMay.SuspendLayout();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMayTram).BeginInit();
            pnlFilter.SuspendLayout();
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
            pnlHeader.Size = new Size(1008, 65);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblSubtitle.ForeColor = Color.FromArgb(0, 190, 255);
            lblSubtitle.Location = new Point(20, 38);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(424, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Phân công: Thành viên 2 (Văn) - Nhánh dev-member-2: Quản lý máy trạm & khu vực";
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
            pnlStats.Location = new Point(0, 65);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1008, 45);
            pnlStats.TabIndex = 1;
            // 
            // lblSoMayBaoTri
            // 
            lblSoMayBaoTri.AutoSize = true;
            lblSoMayBaoTri.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblSoMayBaoTri.ForeColor = Color.FromArgb(255, 128, 128);
            lblSoMayBaoTri.Location = new Point(570, 14);
            lblSoMayBaoTri.Name = "lblSoMayBaoTri";
            lblSoMayBaoTri.Size = new Size(125, 17);
            lblSoMayBaoTri.TabIndex = 3;
            lblSoMayBaoTri.Text = "🛠️ Đang bảo trì: 0";
            // 
            // lblSoMayDangChoi
            // 
            lblSoMayDangChoi.AutoSize = true;
            lblSoMayDangChoi.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblSoMayDangChoi.ForeColor = Color.FromArgb(255, 215, 0);
            lblSoMayDangChoi.Location = new Point(370, 14);
            lblSoMayDangChoi.Name = "lblSoMayDangChoi";
            lblSoMayDangChoi.Size = new Size(144, 17);
            lblSoMayDangChoi.TabIndex = 2;
            lblSoMayDangChoi.Text = "🎮 Đang sử dụng: 0";
            // 
            // lblSoMayTrong
            // 
            lblSoMayTrong.AutoSize = true;
            lblSoMayTrong.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblSoMayTrong.ForeColor = Color.FromArgb(120, 255, 120);
            lblSoMayTrong.Location = new Point(190, 14);
            lblSoMayTrong.Name = "lblSoMayTrong";
            lblSoMayTrong.Size = new Size(127, 17);
            lblSoMayTrong.TabIndex = 1;
            lblSoMayTrong.Text = "🟢 Máy sẵn sàng: 0";
            // 
            // lblTongSoMay
            // 
            lblTongSoMay.AutoSize = true;
            lblTongSoMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTongSoMay.ForeColor = Color.White;
            lblTongSoMay.Location = new Point(20, 14);
            lblTongSoMay.Name = "lblTongSoMay";
            lblTongSoMay.Size = new Size(122, 17);
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
            grpThongTinMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            grpThongTinMay.Location = new Point(0, 110);
            grpThongTinMay.Name = "grpThongTinMay";
            grpThongTinMay.Size = new Size(320, 571);
            grpThongTinMay.TabIndex = 2;
            grpThongTinMay.TabStop = false;
            grpThongTinMay.Text = "Thông Tin Máy Trạm";
            // 
            // btnLamMoi
            // 
            btnLamMoi.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoi.FlatStyle = FlatStyle.Flat;
            btnLamMoi.ForeColor = Color.White;
            btnLamMoi.Location = new Point(160, 385);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(135, 34);
            btnLamMoi.TabIndex = 15;
            btnLamMoi.Text = "🔄 Làm Mới";
            btnLamMoi.UseVisualStyleBackColor = false;
            // 
            // btnMoMay
            // 
            btnMoMay.BackColor = Color.FromArgb(46, 139, 87);
            btnMoMay.FlatStyle = FlatStyle.Flat;
            btnMoMay.ForeColor = Color.White;
            btnMoMay.Location = new Point(160, 335);
            btnMoMay.Name = "btnMoMay";
            btnMoMay.Size = new Size(135, 34);
            btnMoMay.TabIndex = 14;
            btnMoMay.Text = "🟢 Mở Sẵn Sàng";
            btnMoMay.UseVisualStyleBackColor = false;
            // 
            // btnBaoTri
            // 
            btnBaoTri.BackColor = Color.FromArgb(204, 102, 0);
            btnBaoTri.FlatStyle = FlatStyle.Flat;
            btnBaoTri.ForeColor = Color.White;
            btnBaoTri.Location = new Point(15, 385);
            btnBaoTri.Name = "btnBaoTri";
            btnBaoTri.Size = new Size(135, 34);
            btnBaoTri.TabIndex = 13;
            btnBaoTri.Text = "🛠️ Đặt Bảo Trì";
            btnBaoTri.UseVisualStyleBackColor = false;
            // 
            // btnXoaMay
            // 
            btnXoaMay.BackColor = Color.FromArgb(178, 34, 34);
            btnXoaMay.FlatStyle = FlatStyle.Flat;
            btnXoaMay.ForeColor = Color.White;
            btnXoaMay.Location = new Point(15, 335);
            btnXoaMay.Name = "btnXoaMay";
            btnXoaMay.Size = new Size(135, 34);
            btnXoaMay.TabIndex = 12;
            btnXoaMay.Text = "🗑️ Xóa Máy";
            btnXoaMay.UseVisualStyleBackColor = false;
            // 
            // btnSuaMay
            // 
            btnSuaMay.BackColor = Color.FromArgb(30, 144, 255);
            btnSuaMay.FlatStyle = FlatStyle.Flat;
            btnSuaMay.ForeColor = Color.White;
            btnSuaMay.Location = new Point(160, 285);
            btnSuaMay.Name = "btnSuaMay";
            btnSuaMay.Size = new Size(135, 34);
            btnSuaMay.TabIndex = 11;
            btnSuaMay.Text = "✏️ Cập Nhật";
            btnSuaMay.UseVisualStyleBackColor = false;
            // 
            // btnThemMay
            // 
            btnThemMay.BackColor = Color.FromArgb(34, 139, 34);
            btnThemMay.FlatStyle = FlatStyle.Flat;
            btnThemMay.ForeColor = Color.White;
            btnThemMay.Location = new Point(15, 285);
            btnThemMay.Name = "btnThemMay";
            btnThemMay.Size = new Size(135, 34);
            btnThemMay.TabIndex = 10;
            btnThemMay.Text = "➕ Thêm Máy";
            btnThemMay.UseVisualStyleBackColor = false;
            // 
            // cboTrangThai
            // 
            cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTrangThai.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            cboTrangThai.FormattingEnabled = true;
            cboTrangThai.Items.AddRange(new object[] { "Available", "InUse", "Maintenance", "TournamentLocked" });
            cboTrangThai.Location = new Point(110, 215);
            cboTrangThai.Name = "cboTrangThai";
            cboTrangThai.Size = new Size(185, 24);
            cboTrangThai.TabIndex = 9;
            // 
            // lblTrangThai
            // 
            lblTrangThai.AutoSize = true;
            lblTrangThai.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblTrangThai.Location = new Point(15, 220);
            lblTrangThai.Name = "lblTrangThai";
            lblTrangThai.Size = new Size(62, 15);
            lblTrangThai.TabIndex = 8;
            lblTrangThai.Text = "Trạng thái:";
            // 
            // txtIpAddress
            // 
            txtIpAddress.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtIpAddress.Location = new Point(110, 170);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(185, 24);
            txtIpAddress.TabIndex = 7;
            // 
            // lblIpAddress
            // 
            lblIpAddress.AutoSize = true;
            lblIpAddress.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblIpAddress.Location = new Point(15, 175);
            lblIpAddress.Name = "lblIpAddress";
            lblIpAddress.Size = new Size(59, 15);
            lblIpAddress.TabIndex = 6;
            lblIpAddress.Text = "Địa chỉ IP:";
            // 
            // cboKhuVuc
            // 
            cboKhuVuc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboKhuVuc.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            cboKhuVuc.FormattingEnabled = true;
            cboKhuVuc.Location = new Point(110, 125);
            cboKhuVuc.Name = "cboKhuVuc";
            cboKhuVuc.Size = new Size(185, 24);
            cboKhuVuc.TabIndex = 5;
            // 
            // lblKhuVuc
            // 
            lblKhuVuc.AutoSize = true;
            lblKhuVuc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblKhuVuc.Location = new Point(15, 130);
            lblKhuVuc.Name = "lblKhuVuc";
            lblKhuVuc.Size = new Size(54, 15);
            lblKhuVuc.TabIndex = 4;
            lblKhuVuc.Text = "Khu vực:";
            // 
            // txtStationName
            // 
            txtStationName.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtStationName.Location = new Point(110, 80);
            txtStationName.Name = "txtStationName";
            txtStationName.Size = new Size(185, 24);
            txtStationName.TabIndex = 3;
            // 
            // lblStationName
            // 
            lblStationName.AutoSize = true;
            lblStationName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblStationName.Location = new Point(15, 85);
            lblStationName.Name = "lblStationName";
            lblStationName.Size = new Size(55, 15);
            lblStationName.TabIndex = 2;
            lblStationName.Text = "Tên máy:";
            // 
            // txtStationId
            // 
            txtStationId.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            txtStationId.Location = new Point(110, 35);
            txtStationId.Name = "txtStationId";
            txtStationId.ReadOnly = true;
            txtStationId.Size = new Size(185, 24);
            txtStationId.TabIndex = 1;
            // 
            // lblStationId
            // 
            lblStationId.AutoSize = true;
            lblStationId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblStationId.Location = new Point(15, 40);
            lblStationId.Name = "lblStationId";
            lblStationId.Size = new Size(52, 15);
            lblStationId.TabIndex = 0;
            lblStationId.Text = "Mã máy:";
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(dgvMayTram);
            pnlMain.Controls.Add(pnlFilter);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(320, 110);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(10);
            pnlMain.Size = new Size(688, 571);
            pnlMain.TabIndex = 3;
            // 
            // dgvMayTram
            // 
            dgvMayTram.AllowUserToAddRows = false;
            dgvMayTram.AllowUserToDeleteRows = false;
            dgvMayTram.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMayTram.BackgroundColor = Color.WhiteSmoke;
            dgvMayTram.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMayTram.Dock = DockStyle.Fill;
            dgvMayTram.Location = new Point(10, 55);
            dgvMayTram.MultiSelect = false;
            dgvMayTram.Name = "dgvMayTram";
            dgvMayTram.ReadOnly = true;
            dgvMayTram.RowHeadersVisible = false;
            dgvMayTram.RowTemplate.Height = 28;
            dgvMayTram.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMayTram.Size = new Size(668, 506);
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
            pnlFilter.Location = new Point(10, 10);
            pnlFilter.Name = "pnlFilter";
            pnlFilter.Size = new Size(668, 45);
            pnlFilter.TabIndex = 0;
            // 
            // btnApDungLoc
            // 
            btnApDungLoc.BackColor = Color.FromArgb(0, 122, 204);
            btnApDungLoc.FlatStyle = FlatStyle.Flat;
            btnApDungLoc.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnApDungLoc.ForeColor = Color.White;
            btnApDungLoc.Location = new Point(540, 7);
            btnApDungLoc.Name = "btnApDungLoc";
            btnApDungLoc.Size = new Size(110, 30);
            btnApDungLoc.TabIndex = 4;
            btnApDungLoc.Text = "🔍 Lọc Dữ Liệu";
            btnApDungLoc.UseVisualStyleBackColor = false;
            // 
            // cboLocTrangThai
            // 
            cboLocTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocTrangThai.FormattingEnabled = true;
            cboLocTrangThai.Location = new Point(370, 11);
            cboLocTrangThai.Name = "cboLocTrangThai";
            cboLocTrangThai.Size = new Size(150, 23);
            cboLocTrangThai.TabIndex = 3;
            // 
            // lblLocTrangThai
            // 
            lblLocTrangThai.AutoSize = true;
            lblLocTrangThai.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLocTrangThai.Location = new Point(290, 15);
            lblLocTrangThai.Name = "lblLocTrangThai";
            lblLocTrangThai.Size = new Size(66, 15);
            lblLocTrangThai.TabIndex = 2;
            lblLocTrangThai.Text = "Trạng thái:";
            // 
            // cboLocKhuVuc
            // 
            cboLocKhuVuc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocKhuVuc.FormattingEnabled = true;
            cboLocKhuVuc.Location = new Point(110, 11);
            cboLocKhuVuc.Name = "cboLocKhuVuc";
            cboLocKhuVuc.Size = new Size(160, 23);
            cboLocKhuVuc.TabIndex = 1;
            // 
            // lblLocKhuVuc
            // 
            lblLocKhuVuc.AutoSize = true;
            lblLocKhuVuc.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLocKhuVuc.Location = new Point(10, 15);
            lblLocKhuVuc.Name = "lblLocKhuVuc";
            lblLocKhuVuc.Size = new Size(54, 15);
            lblLocKhuVuc.TabIndex = 0;
            lblLocKhuVuc.Text = "Khu vực:";
            // 
            // FormStation
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1008, 681);
            Controls.Add(pnlMain);
            Controls.Add(grpThongTinMay);
            Controls.Add(pnlStats);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(900, 600);
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
            ((System.ComponentModel.ISupportInitialize)dgvMayTram).EndInit();
            pnlFilter.ResumeLayout(false);
            pnlFilter.PerformLayout();
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
        private Panel pnlFilter;
        private Label lblLocKhuVuc;
        private ComboBox cboLocKhuVuc;
        private Label lblLocTrangThai;
        private ComboBox cboLocTrangThai;
        private Button btnApDungLoc;
        private DataGridView dgvMayTram;
    }
}
