namespace GamingCenterManagementSystem
{
    partial class Form1
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
            lblLeaderRole = new Label();
            lblMainTitle = new Label();
            pnlNav = new Panel();
            btnLamMoiSoDo = new Button();
            btnNavBilling = new Button();
            btnNavTournament = new Button();
            btnNavMember = new Button();
            btnNavStation = new Button();
            pnlLegend = new Panel();
            lblLegendTournament = new Label();
            lblLegendMaintenance = new Label();
            lblLegendInUse = new Label();
            lblLegendAvailable = new Label();
            lblLegendTitle = new Label();
            pnlRight = new Panel();
            grpDieuKhienPhien = new GroupBox();
            lblKetQuaTinhTien = new Label();
            btnBaoTri = new Button();
            btnDongMay = new Button();
            btnMoMay = new Button();
            cboGoiCuoc = new ComboBox();
            lblGoiCuoc = new Label();
            cboKhachHang = new ComboBox();
            lblKhachHang = new Label();
            lblGiaGio = new Label();
            lblKhuVucMay = new Label();
            lblTrangThaiMay = new Label();
            lblMayDangChon = new Label();
            pnlCenter = new Panel();
            flpDanhSachMay = new FlowLayoutPanel();
            pnlHeader.SuspendLayout();
            pnlNav.SuspendLayout();
            pnlLegend.SuspendLayout();
            pnlRight.SuspendLayout();
            grpDieuKhienPhien.SuspendLayout();
            pnlCenter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(18, 22, 38);
            pnlHeader.Controls.Add(lblLeaderRole);
            pnlHeader.Controls.Add(lblMainTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1184, 70);
            pnlHeader.TabIndex = 0;
            // 
            // lblLeaderRole
            // 
            lblLeaderRole.AutoSize = true;
            lblLeaderRole.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic, GraphicsUnit.Point);
            lblLeaderRole.ForeColor = Color.FromArgb(0, 210, 255);
            lblLeaderRole.Location = new Point(22, 42);
            lblLeaderRole.Name = "lblLeaderRole";
            lblLeaderRole.Size = new Size(540, 17);
            lblLeaderRole.TabIndex = 1;
            lblLeaderRole.Text = "Trưởng nhóm / Kiến trúc sư: Nguyễn Công Bằng (Towie1206) - Bảng điều khiển kiểm soát toàn hệ thống";
            // 
            // lblMainTitle
            // 
            lblMainTitle.AutoSize = true;
            lblMainTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            lblMainTitle.ForeColor = Color.White;
            lblMainTitle.Location = new Point(18, 9);
            lblMainTitle.Name = "lblMainTitle";
            lblMainTitle.Size = new Size(612, 30);
            lblMainTitle.TabIndex = 0;
            lblMainTitle.Text = "GAMING CENTER MANAGEMENT SYSTEM (CYBER MATRIX)";
            // 
            // pnlNav
            // 
            pnlNav.BackColor = Color.FromArgb(30, 36, 60);
            pnlNav.Controls.Add(btnLamMoiSoDo);
            pnlNav.Controls.Add(btnNavBilling);
            pnlNav.Controls.Add(btnNavTournament);
            pnlNav.Controls.Add(btnNavMember);
            pnlNav.Controls.Add(btnNavStation);
            pnlNav.Dock = DockStyle.Top;
            pnlNav.Location = new Point(0, 70);
            pnlNav.Name = "pnlNav";
            pnlNav.Padding = new Padding(10, 5, 10, 5);
            pnlNav.Size = new Size(1184, 50);
            pnlNav.TabIndex = 1;
            // 
            // btnLamMoiSoDo
            // 
            btnLamMoiSoDo.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoiSoDo.FlatStyle = FlatStyle.Flat;
            btnLamMoiSoDo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnLamMoiSoDo.ForeColor = Color.White;
            btnLamMoiSoDo.Location = new Point(1020, 8);
            btnLamMoiSoDo.Name = "btnLamMoiSoDo";
            btnLamMoiSoDo.Size = new Size(140, 34);
            btnLamMoiSoDo.TabIndex = 4;
            btnLamMoiSoDo.Text = "🔄 Làm Mới Sơ Đồ";
            btnLamMoiSoDo.UseVisualStyleBackColor = false;
            // 
            // btnNavBilling
            // 
            btnNavBilling.BackColor = Color.FromArgb(142, 68, 173);
            btnNavBilling.FlatStyle = FlatStyle.Flat;
            btnNavBilling.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnNavBilling.ForeColor = Color.White;
            btnNavBilling.Location = new Point(740, 8);
            btnNavBilling.Name = "btnNavBilling";
            btnNavBilling.Size = new Size(230, 34);
            btnNavBilling.TabIndex = 3;
            btnNavBilling.Text = "💰 Dịch Vụ & Doanh Thu (Khôi)";
            btnNavBilling.UseVisualStyleBackColor = false;
            // 
            // btnNavTournament
            // 
            btnNavTournament.BackColor = Color.FromArgb(211, 84, 0);
            btnNavTournament.FlatStyle = FlatStyle.Flat;
            btnNavTournament.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnNavTournament.ForeColor = Color.White;
            btnNavTournament.Location = new Point(495, 8);
            btnNavTournament.Name = "btnNavTournament";
            btnNavTournament.Size = new Size(235, 34);
            btnNavTournament.TabIndex = 2;
            btnNavTournament.Text = "🏆 Giải Đấu LAN (Kiên)";
            btnNavTournament.UseVisualStyleBackColor = false;
            // 
            // btnNavMember
            // 
            btnNavMember.BackColor = Color.FromArgb(41, 128, 185);
            btnNavMember.FlatStyle = FlatStyle.Flat;
            btnNavMember.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnNavMember.ForeColor = Color.White;
            btnNavMember.Location = new Point(255, 8);
            btnNavMember.Name = "btnNavMember";
            btnNavMember.Size = new Size(230, 34);
            btnNavMember.TabIndex = 1;
            btnNavMember.Text = "👤 Hội Viên & Nạp Ví (Nam)";
            btnNavMember.UseVisualStyleBackColor = false;
            // 
            // btnNavStation
            // 
            btnNavStation.BackColor = Color.FromArgb(39, 174, 96);
            btnNavStation.FlatStyle = FlatStyle.Flat;
            btnNavStation.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnNavStation.ForeColor = Color.White;
            btnNavStation.Location = new Point(15, 8);
            btnNavStation.Name = "btnNavStation";
            btnNavStation.Size = new Size(230, 34);
            btnNavStation.TabIndex = 0;
            btnNavStation.Text = "🖥️ Máy Trạm & Khu Vực (Văn)";
            btnNavStation.UseVisualStyleBackColor = false;
            // 
            // pnlLegend
            // 
            pnlLegend.BackColor = Color.FromArgb(240, 243, 248);
            pnlLegend.Controls.Add(lblLegendTournament);
            pnlLegend.Controls.Add(lblLegendMaintenance);
            pnlLegend.Controls.Add(lblLegendInUse);
            pnlLegend.Controls.Add(lblLegendAvailable);
            pnlLegend.Controls.Add(lblLegendTitle);
            pnlLegend.Dock = DockStyle.Top;
            pnlLegend.Location = new Point(0, 120);
            pnlLegend.Name = "pnlLegend";
            pnlLegend.Size = new Size(1184, 35);
            pnlLegend.TabIndex = 2;
            // 
            // lblLegendTournament
            // 
            lblLegendTournament.AutoSize = true;
            lblLegendTournament.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLegendTournament.ForeColor = Color.DarkGoldenrod;
            lblLegendTournament.Location = new Point(560, 10);
            lblLegendTournament.Name = "lblLegendTournament";
            lblLegendTournament.Size = new Size(183, 15);
            lblLegendTournament.TabIndex = 4;
            lblLegendTournament.Text = "🟨 Khóa Giải Đấu (Tournament)";
            // 
            // lblLegendMaintenance
            // 
            lblLegendMaintenance.AutoSize = true;
            lblLegendMaintenance.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLegendMaintenance.ForeColor = Color.Crimson;
            lblLegendMaintenance.Location = new Point(390, 10);
            lblLegendMaintenance.Name = "lblLegendMaintenance";
            lblLegendMaintenance.Size = new Size(140, 15);
            lblLegendMaintenance.TabIndex = 3;
            lblLegendMaintenance.Text = "🟥 Đang Bảo Trì (Hỏng)";
            // 
            // lblLegendInUse
            // 
            lblLegendInUse.AutoSize = true;
            lblLegendInUse.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLegendInUse.ForeColor = Color.FromArgb(204, 102, 0);
            lblLegendInUse.Location = new Point(230, 10);
            lblLegendInUse.Name = "lblLegendInUse";
            lblLegendInUse.Size = new Size(130, 15);
            lblLegendInUse.TabIndex = 2;
            lblLegendInUse.Text = "🟧 Đang Chơi (In Use)";
            // 
            // lblLegendAvailable
            // 
            lblLegendAvailable.AutoSize = true;
            lblLegendAvailable.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLegendAvailable.ForeColor = Color.DarkGreen;
            lblLegendAvailable.Location = new Point(90, 10);
            lblLegendAvailable.Name = "lblLegendAvailable";
            lblLegendAvailable.Size = new Size(115, 15);
            lblLegendAvailable.TabIndex = 1;
            lblLegendAvailable.Text = "🟩 Sẵn Sàng (Trống)";
            // 
            // lblLegendTitle
            // 
            lblLegendTitle.AutoSize = true;
            lblLegendTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblLegendTitle.Location = new Point(15, 10);
            lblLegendTitle.Name = "lblLegendTitle";
            lblLegendTitle.Size = new Size(61, 15);
            lblLegendTitle.TabIndex = 0;
            lblLegendTitle.Text = "Trạng thái:";
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(250, 252, 255);
            pnlRight.Controls.Add(grpDieuKhienPhien);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(834, 155);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(10);
            pnlRight.Size = new Size(350, 566);
            pnlRight.TabIndex = 3;
            // 
            // grpDieuKhienPhien
            // 
            grpDieuKhienPhien.Controls.Add(lblKetQuaTinhTien);
            grpDieuKhienPhien.Controls.Add(btnBaoTri);
            grpDieuKhienPhien.Controls.Add(btnDongMay);
            grpDieuKhienPhien.Controls.Add(btnMoMay);
            grpDieuKhienPhien.Controls.Add(cboGoiCuoc);
            grpDieuKhienPhien.Controls.Add(lblGoiCuoc);
            grpDieuKhienPhien.Controls.Add(cboKhachHang);
            grpDieuKhienPhien.Controls.Add(lblKhachHang);
            grpDieuKhienPhien.Controls.Add(lblGiaGio);
            grpDieuKhienPhien.Controls.Add(lblKhuVucMay);
            grpDieuKhienPhien.Controls.Add(lblTrangThaiMay);
            grpDieuKhienPhien.Controls.Add(lblMayDangChon);
            grpDieuKhienPhien.Dock = DockStyle.Fill;
            grpDieuKhienPhien.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            grpDieuKhienPhien.ForeColor = Color.FromArgb(24, 30, 54);
            grpDieuKhienPhien.Location = new Point(10, 10);
            grpDieuKhienPhien.Name = "grpDieuKhienPhien";
            grpDieuKhienPhien.Size = new Size(330, 546);
            grpDieuKhienPhien.TabIndex = 0;
            grpDieuKhienPhien.TabStop = false;
            grpDieuKhienPhien.Text = "Bảng Điều Khiển Máy Trạm";
            // 
            // lblKetQuaTinhTien
            // 
            lblKetQuaTinhTien.BackColor = Color.FromArgb(235, 245, 255);
            lblKetQuaTinhTien.BorderStyle = BorderStyle.FixedSingle;
            lblKetQuaTinhTien.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblKetQuaTinhTien.ForeColor = Color.FromArgb(0, 51, 102);
            lblKetQuaTinhTien.Location = new Point(15, 410);
            lblKetQuaTinhTien.Name = "lblKetQuaTinhTien";
            lblKetQuaTinhTien.Padding = new Padding(6);
            lblKetQuaTinhTien.Size = new Size(300, 115);
            lblKetQuaTinhTien.TabIndex = 11;
            lblKetQuaTinhTien.Text = "Thông tin thanh toán phiên chơi sẽ hiển thị tại đây sau khi chốt phiên tính cước đa hình.";
            // 
            // btnBaoTri
            // 
            btnBaoTri.BackColor = Color.FromArgb(204, 102, 0);
            btnBaoTri.FlatStyle = FlatStyle.Flat;
            btnBaoTri.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnBaoTri.ForeColor = Color.White;
            btnBaoTri.Location = new Point(15, 360);
            btnBaoTri.Name = "btnBaoTri";
            btnBaoTri.Size = new Size(300, 35);
            btnBaoTri.TabIndex = 10;
            btnBaoTri.Text = "🛠️ CHUYỂN BẢO TRÌ / MỞ LẠI";
            btnBaoTri.UseVisualStyleBackColor = false;
            // 
            // btnDongMay
            // 
            btnDongMay.BackColor = Color.FromArgb(178, 34, 34);
            btnDongMay.FlatStyle = FlatStyle.Flat;
            btnDongMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnDongMay.ForeColor = Color.White;
            btnDongMay.Location = new Point(15, 315);
            btnDongMay.Name = "btnDongMay";
            btnDongMay.Size = new Size(300, 35);
            btnDongMay.TabIndex = 9;
            btnDongMay.Text = "🔴 ĐÓNG MÁY & TÍNH TIỀN (Đa Hình)";
            btnDongMay.UseVisualStyleBackColor = false;
            // 
            // btnMoMay
            // 
            btnMoMay.BackColor = Color.FromArgb(34, 139, 34);
            btnMoMay.FlatStyle = FlatStyle.Flat;
            btnMoMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            btnMoMay.ForeColor = Color.White;
            btnMoMay.Location = new Point(15, 270);
            btnMoMay.Name = "btnMoMay";
            btnMoMay.Size = new Size(300, 35);
            btnMoMay.TabIndex = 8;
            btnMoMay.Text = "🟢 MỞ MÁY CHO KHÁCH";
            btnMoMay.UseVisualStyleBackColor = false;
            // 
            // cboGoiCuoc
            // 
            cboGoiCuoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGoiCuoc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboGoiCuoc.FormattingEnabled = true;
            cboGoiCuoc.Items.AddRange(new object[] { "Hourly (Giờ thường)", "NightCombo (Combo đêm 45k)", "Tournament (Giải LAN - Giảm 50%)" });
            cboGoiCuoc.Location = new Point(115, 218);
            cboGoiCuoc.Name = "cboGoiCuoc";
            cboGoiCuoc.Size = new Size(200, 23);
            cboGoiCuoc.TabIndex = 7;
            // 
            // lblGoiCuoc
            // 
            lblGoiCuoc.AutoSize = true;
            lblGoiCuoc.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblGoiCuoc.Location = new Point(15, 222);
            lblGoiCuoc.Name = "lblGoiCuoc";
            lblGoiCuoc.Size = new Size(57, 15);
            lblGoiCuoc.TabIndex = 6;
            lblGoiCuoc.Text = "Gói cước:";
            // 
            // cboKhachHang
            // 
            cboKhachHang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboKhachHang.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            cboKhachHang.FormattingEnabled = true;
            cboKhachHang.Location = new Point(115, 178);
            cboKhachHang.Name = "cboKhachHang";
            cboKhachHang.Size = new Size(200, 23);
            cboKhachHang.TabIndex = 5;
            // 
            // lblKhachHang
            // 
            lblKhachHang.AutoSize = true;
            lblKhachHang.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblKhachHang.Location = new Point(15, 182);
            lblKhachHang.Name = "lblKhachHang";
            lblKhachHang.Size = new Size(73, 15);
            lblKhachHang.TabIndex = 4;
            lblKhachHang.Text = "Khách hàng:";
            // 
            // lblGiaGio
            // 
            lblGiaGio.AutoSize = true;
            lblGiaGio.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblGiaGio.ForeColor = Color.FromArgb(0, 102, 204);
            lblGiaGio.Location = new Point(15, 140);
            lblGiaGio.Name = "lblGiaGio";
            lblGiaGio.Size = new Size(137, 17);
            lblGiaGio.TabIndex = 3;
            lblGiaGio.Text = "Đơn giá: 8,000 VNĐ/h";
            // 
            // lblKhuVucMay
            // 
            lblKhuVucMay.AutoSize = true;
            lblKhuVucMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblKhuVucMay.Location = new Point(15, 105);
            lblKhuVucMay.Name = "lblKhuVucMay";
            lblKhuVucMay.Size = new Size(125, 17);
            lblKhuVucMay.TabIndex = 2;
            lblKhuVucMay.Text = "Khu vực: Khu Thường";
            // 
            // lblTrangThaiMay
            // 
            lblTrangThaiMay.AutoSize = true;
            lblTrangThaiMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            lblTrangThaiMay.ForeColor = Color.Green;
            lblTrangThaiMay.Location = new Point(15, 70);
            lblTrangThaiMay.Name = "lblTrangThaiMay";
            lblTrangThaiMay.Size = new Size(139, 17);
            lblTrangThaiMay.TabIndex = 1;
            lblTrangThaiMay.Text = "Trạng thái: Available";
            // 
            // lblMayDangChon
            // 
            lblMayDangChon.AutoSize = true;
            lblMayDangChon.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblMayDangChon.ForeColor = Color.FromArgb(18, 22, 38);
            lblMayDangChon.Location = new Point(15, 35);
            lblMayDangChon.Name = "lblMayDangChon";
            lblMayDangChon.Size = new Size(157, 21);
            lblMayDangChon.TabIndex = 0;
            lblMayDangChon.Text = "🖥️ Máy Trạm: PC-01";
            // 
            // pnlCenter
            // 
            pnlCenter.BackColor = Color.FromArgb(235, 238, 245);
            pnlCenter.Controls.Add(flpDanhSachMay);
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(0, 155);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Padding = new Padding(15);
            pnlCenter.Size = new Size(834, 566);
            pnlCenter.TabIndex = 4;
            // 
            // flpDanhSachMay
            // 
            flpDanhSachMay.AutoScroll = true;
            flpDanhSachMay.BackColor = Color.White;
            flpDanhSachMay.BorderStyle = BorderStyle.FixedSingle;
            flpDanhSachMay.Dock = DockStyle.Fill;
            flpDanhSachMay.Location = new Point(15, 15);
            flpDanhSachMay.Name = "flpDanhSachMay";
            flpDanhSachMay.Padding = new Padding(15);
            flpDanhSachMay.Size = new Size(804, 536);
            flpDanhSachMay.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 721);
            Controls.Add(pnlCenter);
            Controls.Add(pnlRight);
            Controls.Add(pnlLegend);
            Controls.Add(pnlNav);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(1100, 700);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gaming Center Management System - Cyber Matrix (Leader Dashboard)";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlNav.ResumeLayout(false);
            pnlLegend.ResumeLayout(false);
            pnlLegend.PerformLayout();
            pnlRight.ResumeLayout(false);
            grpDieuKhienPhien.ResumeLayout(false);
            grpDieuKhienPhien.PerformLayout();
            pnlCenter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblMainTitle;
        private Label lblLeaderRole;
        private Panel pnlNav;
        private Button btnNavStation;
        private Button btnNavMember;
        private Button btnNavTournament;
        private Button btnNavBilling;
        private Button btnLamMoiSoDo;
        private Panel pnlLegend;
        private Label lblLegendTitle;
        private Label lblLegendAvailable;
        private Label lblLegendInUse;
        private Label lblLegendMaintenance;
        private Label lblLegendTournament;
        private Panel pnlRight;
        private GroupBox grpDieuKhienPhien;
        private Label lblMayDangChon;
        private Label lblTrangThaiMay;
        private Label lblKhuVucMay;
        private Label lblGiaGio;
        private Label lblKhachHang;
        private ComboBox cboKhachHang;
        private Label lblGoiCuoc;
        private ComboBox cboGoiCuoc;
        private Button btnMoMay;
        private Button btnDongMay;
        private Button btnBaoTri;
        private Label lblKetQuaTinhTien;
        private Panel pnlCenter;
        private FlowLayoutPanel flpDanhSachMay;
    }
}
