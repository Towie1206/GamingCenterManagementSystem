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
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1353, 93);
            pnlHeader.TabIndex = 0;
            // 
            // lblLeaderRole
            // 
            lblLeaderRole.AutoSize = true;
            lblLeaderRole.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
            lblLeaderRole.ForeColor = Color.FromArgb(0, 210, 255);
            lblLeaderRole.Location = new Point(25, 56);
            lblLeaderRole.Name = "lblLeaderRole";
            lblLeaderRole.Size = new Size(733, 21);
            lblLeaderRole.TabIndex = 1;
            lblLeaderRole.Text = "Trưởng nhóm / Kiến trúc sư: Nguyễn Công Bằng (Towie1206) - Bảng điều khiển kiểm soát toàn hệ thống";
            // 
            // lblMainTitle
            // 
            lblMainTitle.AutoSize = true;
            lblMainTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblMainTitle.ForeColor = Color.White;
            lblMainTitle.Location = new Point(21, 12);
            lblMainTitle.Name = "lblMainTitle";
            lblMainTitle.Size = new Size(768, 37);
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
            pnlNav.Location = new Point(0, 93);
            pnlNav.Margin = new Padding(3, 4, 3, 4);
            pnlNav.Name = "pnlNav";
            pnlNav.Padding = new Padding(11, 7, 11, 7);
            pnlNav.Size = new Size(1353, 67);
            pnlNav.TabIndex = 1;
            // 
            // btnLamMoiSoDo
            // 
            btnLamMoiSoDo.BackColor = Color.FromArgb(70, 80, 95);
            btnLamMoiSoDo.FlatStyle = FlatStyle.Flat;
            btnLamMoiSoDo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLamMoiSoDo.ForeColor = Color.White;
            btnLamMoiSoDo.Location = new Point(1166, 11);
            btnLamMoiSoDo.Margin = new Padding(3, 4, 3, 4);
            btnLamMoiSoDo.Name = "btnLamMoiSoDo";
            btnLamMoiSoDo.Size = new Size(160, 45);
            btnLamMoiSoDo.TabIndex = 4;
            btnLamMoiSoDo.Text = "🔄 Làm Mới Sơ Đồ";
            btnLamMoiSoDo.UseVisualStyleBackColor = false;
            // 
            // btnNavBilling
            // 
            btnNavBilling.BackColor = Color.FromArgb(142, 68, 173);
            btnNavBilling.FlatStyle = FlatStyle.Flat;
            btnNavBilling.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavBilling.ForeColor = Color.White;
            btnNavBilling.Location = new Point(846, 11);
            btnNavBilling.Margin = new Padding(3, 4, 3, 4);
            btnNavBilling.Name = "btnNavBilling";
            btnNavBilling.Size = new Size(263, 45);
            btnNavBilling.TabIndex = 3;
            btnNavBilling.Text = "💰 Dịch Vụ & Doanh Thu";
            btnNavBilling.UseVisualStyleBackColor = false;
            // 
            // btnNavTournament
            // 
            btnNavTournament.BackColor = Color.FromArgb(211, 84, 0);
            btnNavTournament.FlatStyle = FlatStyle.Flat;
            btnNavTournament.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavTournament.ForeColor = Color.White;
            btnNavTournament.Location = new Point(566, 11);
            btnNavTournament.Margin = new Padding(3, 4, 3, 4);
            btnNavTournament.Name = "btnNavTournament";
            btnNavTournament.Size = new Size(269, 45);
            btnNavTournament.TabIndex = 2;
            btnNavTournament.Text = "🏆 Giải Đấu LAN";
            btnNavTournament.UseVisualStyleBackColor = false;
            // 
            // btnNavMember
            // 
            btnNavMember.BackColor = Color.FromArgb(41, 128, 185);
            btnNavMember.FlatStyle = FlatStyle.Flat;
            btnNavMember.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavMember.ForeColor = Color.White;
            btnNavMember.Location = new Point(291, 11);
            btnNavMember.Margin = new Padding(3, 4, 3, 4);
            btnNavMember.Name = "btnNavMember";
            btnNavMember.Size = new Size(263, 45);
            btnNavMember.TabIndex = 1;
            btnNavMember.Text = "👤 Hội Viên & Nạp Ví";
            btnNavMember.UseVisualStyleBackColor = false;
            // 
            // btnNavStation
            // 
            btnNavStation.BackColor = Color.FromArgb(39, 174, 96);
            btnNavStation.FlatStyle = FlatStyle.Flat;
            btnNavStation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavStation.ForeColor = Color.White;
            btnNavStation.Location = new Point(17, 11);
            btnNavStation.Margin = new Padding(3, 4, 3, 4);
            btnNavStation.Name = "btnNavStation";
            btnNavStation.Size = new Size(263, 45);
            btnNavStation.TabIndex = 0;
            btnNavStation.Text = "🖥️ Máy Trạm & Khu Vực";
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
            pnlLegend.Location = new Point(0, 160);
            pnlLegend.Margin = new Padding(3, 4, 3, 4);
            pnlLegend.Name = "pnlLegend";
            pnlLegend.Size = new Size(1353, 47);
            pnlLegend.TabIndex = 2;
            // 
            // lblLegendTournament
            // 
            lblLegendTournament.AutoSize = true;
            lblLegendTournament.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendTournament.ForeColor = Color.DarkGoldenrod;
            lblLegendTournament.Location = new Point(640, 13);
            lblLegendTournament.Name = "lblLegendTournament";
            lblLegendTournament.Size = new Size(236, 20);
            lblLegendTournament.TabIndex = 4;
            lblLegendTournament.Text = "\U0001f7e8 Khóa Giải Đấu (Tournament)";
            // 
            // lblLegendMaintenance
            // 
            lblLegendMaintenance.AutoSize = true;
            lblLegendMaintenance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendMaintenance.ForeColor = Color.Crimson;
            lblLegendMaintenance.Location = new Point(446, 13);
            lblLegendMaintenance.Name = "lblLegendMaintenance";
            lblLegendMaintenance.Size = new Size(179, 20);
            lblLegendMaintenance.TabIndex = 3;
            lblLegendMaintenance.Text = "\U0001f7e5 Đang Bảo Trì (Hỏng)";
            // 
            // lblLegendInUse
            // 
            lblLegendInUse.AutoSize = true;
            lblLegendInUse.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendInUse.ForeColor = Color.FromArgb(204, 102, 0);
            lblLegendInUse.Location = new Point(263, 13);
            lblLegendInUse.Name = "lblLegendInUse";
            lblLegendInUse.Size = new Size(167, 20);
            lblLegendInUse.TabIndex = 2;
            lblLegendInUse.Text = "\U0001f7e7 Đang Chơi (In Use)";
            // 
            // lblLegendAvailable
            // 
            lblLegendAvailable.AutoSize = true;
            lblLegendAvailable.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendAvailable.ForeColor = Color.DarkGreen;
            lblLegendAvailable.Location = new Point(103, 13);
            lblLegendAvailable.Name = "lblLegendAvailable";
            lblLegendAvailable.Size = new Size(155, 20);
            lblLegendAvailable.TabIndex = 1;
            lblLegendAvailable.Text = "\U0001f7e9 Sẵn Sàng (Trống)";
            // 
            // lblLegendTitle
            // 
            lblLegendTitle.AutoSize = true;
            lblLegendTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendTitle.Location = new Point(17, 13);
            lblLegendTitle.Name = "lblLegendTitle";
            lblLegendTitle.Size = new Size(84, 20);
            lblLegendTitle.TabIndex = 0;
            lblLegendTitle.Text = "Trạng thái:";
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(250, 252, 255);
            pnlRight.Controls.Add(grpDieuKhienPhien);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(953, 207);
            pnlRight.Margin = new Padding(3, 4, 3, 4);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(11, 13, 11, 13);
            pnlRight.Size = new Size(400, 754);
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
            grpDieuKhienPhien.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpDieuKhienPhien.ForeColor = Color.FromArgb(24, 30, 54);
            grpDieuKhienPhien.Location = new Point(11, 13);
            grpDieuKhienPhien.Margin = new Padding(3, 4, 3, 4);
            grpDieuKhienPhien.Name = "grpDieuKhienPhien";
            grpDieuKhienPhien.Padding = new Padding(3, 4, 3, 4);
            grpDieuKhienPhien.Size = new Size(378, 728);
            grpDieuKhienPhien.TabIndex = 0;
            grpDieuKhienPhien.TabStop = false;
            grpDieuKhienPhien.Text = "Bảng Điều Khiển Máy Trạm";
            // 
            // lblKetQuaTinhTien
            // 
            lblKetQuaTinhTien.BackColor = Color.FromArgb(235, 245, 255);
            lblKetQuaTinhTien.BorderStyle = BorderStyle.FixedSingle;
            lblKetQuaTinhTien.Font = new Font("Segoe UI", 9F);
            lblKetQuaTinhTien.ForeColor = Color.FromArgb(0, 51, 102);
            lblKetQuaTinhTien.Location = new Point(17, 547);
            lblKetQuaTinhTien.Name = "lblKetQuaTinhTien";
            lblKetQuaTinhTien.Padding = new Padding(7, 8, 7, 8);
            lblKetQuaTinhTien.Size = new Size(343, 153);
            lblKetQuaTinhTien.TabIndex = 11;
            lblKetQuaTinhTien.Text = "Thông tin thanh toán phiên chơi sẽ hiển thị tại đây sau khi chốt phiên tính cước đa hình.";
            // 
            // btnBaoTri
            // 
            btnBaoTri.BackColor = Color.FromArgb(204, 102, 0);
            btnBaoTri.FlatStyle = FlatStyle.Flat;
            btnBaoTri.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnBaoTri.ForeColor = Color.White;
            btnBaoTri.Location = new Point(17, 480);
            btnBaoTri.Margin = new Padding(3, 4, 3, 4);
            btnBaoTri.Name = "btnBaoTri";
            btnBaoTri.Size = new Size(343, 47);
            btnBaoTri.TabIndex = 10;
            btnBaoTri.Text = "🛠️ CHUYỂN BẢO TRÌ / MỞ LẠI";
            btnBaoTri.UseVisualStyleBackColor = false;
            // 
            // btnDongMay
            // 
            btnDongMay.BackColor = Color.FromArgb(178, 34, 34);
            btnDongMay.FlatStyle = FlatStyle.Flat;
            btnDongMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnDongMay.ForeColor = Color.White;
            btnDongMay.Location = new Point(17, 420);
            btnDongMay.Margin = new Padding(3, 4, 3, 4);
            btnDongMay.Name = "btnDongMay";
            btnDongMay.Size = new Size(343, 47);
            btnDongMay.TabIndex = 9;
            btnDongMay.Text = "🔴 ĐÓNG MÁY & TÍNH TIỀN (Đa Hình)";
            btnDongMay.UseVisualStyleBackColor = false;
            // 
            // btnMoMay
            // 
            btnMoMay.BackColor = Color.FromArgb(34, 139, 34);
            btnMoMay.FlatStyle = FlatStyle.Flat;
            btnMoMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnMoMay.ForeColor = Color.White;
            btnMoMay.Location = new Point(17, 360);
            btnMoMay.Margin = new Padding(3, 4, 3, 4);
            btnMoMay.Name = "btnMoMay";
            btnMoMay.Size = new Size(343, 47);
            btnMoMay.TabIndex = 8;
            btnMoMay.Text = "\U0001f7e2 MỞ MÁY CHO KHÁCH";
            btnMoMay.UseVisualStyleBackColor = false;
            // 
            // cboGoiCuoc
            // 
            cboGoiCuoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGoiCuoc.Font = new Font("Segoe UI", 9F);
            cboGoiCuoc.FormattingEnabled = true;
            cboGoiCuoc.Items.AddRange(new object[] { "Hourly (Giờ thường)", "NightCombo (Combo đêm 45k)", "Tournament (Giải LAN - Giảm 50%)" });
            cboGoiCuoc.Location = new Point(131, 291);
            cboGoiCuoc.Margin = new Padding(3, 4, 3, 4);
            cboGoiCuoc.Name = "cboGoiCuoc";
            cboGoiCuoc.Size = new Size(228, 28);
            cboGoiCuoc.TabIndex = 7;
            // 
            // lblGoiCuoc
            // 
            lblGoiCuoc.AutoSize = true;
            lblGoiCuoc.Font = new Font("Segoe UI", 9F);
            lblGoiCuoc.Location = new Point(17, 296);
            lblGoiCuoc.Name = "lblGoiCuoc";
            lblGoiCuoc.Size = new Size(71, 20);
            lblGoiCuoc.TabIndex = 6;
            lblGoiCuoc.Text = "Gói cước:";
            // 
            // cboKhachHang
            // 
            cboKhachHang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboKhachHang.Font = new Font("Segoe UI", 9F);
            cboKhachHang.FormattingEnabled = true;
            cboKhachHang.Location = new Point(131, 237);
            cboKhachHang.Margin = new Padding(3, 4, 3, 4);
            cboKhachHang.Name = "cboKhachHang";
            cboKhachHang.Size = new Size(228, 28);
            cboKhachHang.TabIndex = 5;
            // 
            // lblKhachHang
            // 
            lblKhachHang.AutoSize = true;
            lblKhachHang.Font = new Font("Segoe UI", 9F);
            lblKhachHang.Location = new Point(17, 243);
            lblKhachHang.Name = "lblKhachHang";
            lblKhachHang.Size = new Size(89, 20);
            lblKhachHang.TabIndex = 4;
            lblKhachHang.Text = "Khách hàng:";
            // 
            // lblGiaGio
            // 
            lblGiaGio.AutoSize = true;
            lblGiaGio.Font = new Font("Segoe UI", 9.5F);
            lblGiaGio.ForeColor = Color.FromArgb(0, 102, 204);
            lblGiaGio.Location = new Point(17, 187);
            lblGiaGio.Name = "lblGiaGio";
            lblGiaGio.Size = new Size(163, 21);
            lblGiaGio.TabIndex = 3;
            lblGiaGio.Text = "Đơn giá: 8,000 VNĐ/h";
            // 
            // lblKhuVucMay
            // 
            lblKhuVucMay.AutoSize = true;
            lblKhuVucMay.Font = new Font("Segoe UI", 9.5F);
            lblKhuVucMay.Location = new Point(17, 140);
            lblKhuVucMay.Name = "lblKhuVucMay";
            lblKhuVucMay.Size = new Size(157, 21);
            lblKhuVucMay.TabIndex = 2;
            lblKhuVucMay.Text = "Khu vực: Khu Thường";
            // 
            // lblTrangThaiMay
            // 
            lblTrangThaiMay.AutoSize = true;
            lblTrangThaiMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTrangThaiMay.ForeColor = Color.Green;
            lblTrangThaiMay.Location = new Point(17, 93);
            lblTrangThaiMay.Name = "lblTrangThaiMay";
            lblTrangThaiMay.Size = new Size(167, 21);
            lblTrangThaiMay.TabIndex = 1;
            lblTrangThaiMay.Text = "Trạng thái: Available";
            // 
            // lblMayDangChon
            // 
            lblMayDangChon.AutoSize = true;
            lblMayDangChon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblMayDangChon.ForeColor = Color.FromArgb(18, 22, 38);
            lblMayDangChon.Location = new Point(17, 47);
            lblMayDangChon.Name = "lblMayDangChon";
            lblMayDangChon.Size = new Size(207, 28);
            lblMayDangChon.TabIndex = 0;
            lblMayDangChon.Text = "🖥️ Máy Trạm: PC-01";
            // 
            // pnlCenter
            // 
            pnlCenter.BackColor = Color.FromArgb(235, 238, 245);
            pnlCenter.Controls.Add(flpDanhSachMay);
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(0, 207);
            pnlCenter.Margin = new Padding(3, 4, 3, 4);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Padding = new Padding(17, 20, 17, 20);
            pnlCenter.Size = new Size(953, 754);
            pnlCenter.TabIndex = 4;
            // 
            // flpDanhSachMay
            // 
            flpDanhSachMay.AutoScroll = true;
            flpDanhSachMay.BackColor = Color.White;
            flpDanhSachMay.BorderStyle = BorderStyle.FixedSingle;
            flpDanhSachMay.Dock = DockStyle.Fill;
            flpDanhSachMay.Location = new Point(17, 20);
            flpDanhSachMay.Margin = new Padding(3, 4, 3, 4);
            flpDanhSachMay.Name = "flpDanhSachMay";
            flpDanhSachMay.Padding = new Padding(17, 20, 17, 20);
            flpDanhSachMay.Size = new Size(919, 714);
            flpDanhSachMay.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1353, 961);
            Controls.Add(pnlCenter);
            Controls.Add(pnlRight);
            Controls.Add(pnlLegend);
            Controls.Add(pnlNav);
            Controls.Add(pnlHeader);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1255, 918);
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
