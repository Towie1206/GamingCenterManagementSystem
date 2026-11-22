namespace GamingCenterManagementSystem
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeManagedResources();
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            btnDangXuat = new Button();
            lblCurrentUser = new Label();
            lblLeaderRole = new Label();
            lblMainTitle = new Label();
            pnlNav = new Panel();
            btnLamMoiSoDo = new Button();
            btnChatHub = new Button();
            btnClientMode = new Button();
            btnNavBilling = new Button();
            btnNavTournament = new Button();
            btnNavMember = new Button();
            btnNavStation = new Button();
            pnlLegend = new Panel();
            lblRealtimeSummary = new Label();
            lblLegendTournament = new Label();
            lblLegendMaintenance = new Label();
            lblLegendInUse = new Label();
            lblLegendAvailable = new Label();
            lblLegendTitle = new Label();
            pnlRight = new Panel();
            grpDieuKhienPhien = new GroupBox();
            lblKetQuaTinhTien = new Label();
            btnChatStation = new Button();
            btnBaoTri = new Button();
            btnDongMay = new Button();
            btnGoiMon = new Button();
            btnMoMay = new Button();
            cboGoiCuoc = new ComboBox();
            lblGoiCuoc = new Label();
            cboKhachHang = new ComboBox();
            lblKhachHang = new Label();
            lblCauHinhMay = new Label();
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
            pnlHeader.BackColor = Color.FromArgb(15, 23, 42);
            pnlHeader.Controls.Add(btnDangXuat);
            pnlHeader.Controls.Add(lblCurrentUser);
            pnlHeader.Controls.Add(lblLeaderRole);
            pnlHeader.Controls.Add(lblMainTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1353, 80);
            pnlHeader.TabIndex = 0;
            // 
            // btnDangXuat
            // 
            btnDangXuat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDangXuat.BackColor = Color.FromArgb(220, 38, 38);
            btnDangXuat.Cursor = Cursors.Hand;
            btnDangXuat.FlatAppearance.BorderSize = 0;
            btnDangXuat.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 28, 28);
            btnDangXuat.FlatStyle = FlatStyle.Flat;
            btnDangXuat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDangXuat.ForeColor = Color.White;
            btnDangXuat.Location = new Point(1214, 21);
            btnDangXuat.Name = "btnDangXuat";
            btnDangXuat.Size = new Size(120, 38);
            btnDangXuat.TabIndex = 3;
            btnDangXuat.Text = "🚪 Đăng xuất";
            btnDangXuat.UseVisualStyleBackColor = false;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCurrentUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCurrentUser.ForeColor = Color.FromArgb(226, 232, 240);
            lblCurrentUser.Location = new Point(822, 10);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(380, 60);
            lblCurrentUser.TabIndex = 2;
            lblCurrentUser.Text = "👤 Chưa đăng nhập";
            lblCurrentUser.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblLeaderRole
            // 
            lblLeaderRole.AutoSize = true;
            lblLeaderRole.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblLeaderRole.ForeColor = Color.FromArgb(56, 189, 248);
            lblLeaderRole.Location = new Point(22, 48);
            lblLeaderRole.MaximumSize = new Size(640, 0);
            lblLeaderRole.Name = "lblLeaderRole";
            lblLeaderRole.Size = new Size(520, 20);
            lblLeaderRole.TabIndex = 1;
            lblLeaderRole.Text = "📍 165 Hoàng Công Chất, Bắc Từ Liêm, Hà Nội  •  ⚡ Happy Hours 23h-11h giảm 40%";
            // 
            // lblMainTitle
            // 
            lblMainTitle.AutoSize = true;
            lblMainTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblMainTitle.ForeColor = Color.White;
            lblMainTitle.Location = new Point(20, 12);
            lblMainTitle.Name = "lblMainTitle";
            lblMainTitle.Size = new Size(410, 33);
            lblMainTitle.TabIndex = 0;
            lblMainTitle.Text = "MASTER GAME — QUẢN LÝ PHÒNG MÁY";
            // 
            // pnlNav
            // 
            pnlNav.BackColor = Color.White;
            pnlNav.Controls.Add(btnLamMoiSoDo);
            pnlNav.Controls.Add(btnChatHub);
            pnlNav.Controls.Add(btnClientMode);
            pnlNav.Controls.Add(btnNavBilling);
            pnlNav.Controls.Add(btnNavTournament);
            pnlNav.Controls.Add(btnNavMember);
            pnlNav.Controls.Add(btnNavStation);
            pnlNav.Dock = DockStyle.Top;
            pnlNav.Location = new Point(0, 80);
            pnlNav.Name = "pnlNav";
            pnlNav.Padding = new Padding(12, 8, 12, 8);
            pnlNav.Size = new Size(1353, 56);
            pnlNav.TabIndex = 1;
            // 
            // btnLamMoiSoDo
            // 
            btnLamMoiSoDo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLamMoiSoDo.BackColor = Color.FromArgb(71, 85, 105);
            btnLamMoiSoDo.Cursor = Cursors.Hand;
            btnLamMoiSoDo.FlatAppearance.BorderSize = 0;
            btnLamMoiSoDo.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 65, 85);
            btnLamMoiSoDo.FlatStyle = FlatStyle.Flat;
            btnLamMoiSoDo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLamMoiSoDo.ForeColor = Color.White;
            btnLamMoiSoDo.Location = new Point(1204, 9);
            btnLamMoiSoDo.Name = "btnLamMoiSoDo";
            btnLamMoiSoDo.Size = new Size(130, 38);
            btnLamMoiSoDo.TabIndex = 5;
            btnLamMoiSoDo.Text = "🔄 Làm Mới";
            btnLamMoiSoDo.UseVisualStyleBackColor = false;
            // 
            // btnChatHub
            // 
            btnChatHub.BackColor = Color.FromArgb(13, 148, 136);
            btnChatHub.Cursor = Cursors.Hand;
            btnChatHub.FlatAppearance.BorderSize = 0;
            btnChatHub.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 118, 110);
            btnChatHub.FlatStyle = FlatStyle.Flat;
            btnChatHub.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnChatHub.ForeColor = Color.White;
            btnChatHub.Location = new Point(940, 9);
            btnChatHub.Name = "btnChatHub";
            btnChatHub.Size = new Size(135, 38);
            btnChatHub.TabIndex = 6;
            btnChatHub.Text = "💬 Chat Quầy";
            btnChatHub.UseVisualStyleBackColor = false;
            // 
            // btnClientMode
            // 
            btnClientMode.BackColor = Color.FromArgb(37, 99, 235);
            btnClientMode.Cursor = Cursors.Hand;
            btnClientMode.FlatAppearance.BorderSize = 0;
            btnClientMode.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnClientMode.FlatStyle = FlatStyle.Flat;
            btnClientMode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClientMode.ForeColor = Color.White;
            btnClientMode.Location = new Point(774, 9);
            btnClientMode.Name = "btnClientMode";
            btnClientMode.Size = new Size(156, 38);
            btnClientMode.TabIndex = 4;
            btnClientMode.Text = "🖥️ Máy Trạm Client";
            btnClientMode.UseVisualStyleBackColor = false;
            // 
            // btnNavBilling
            // 
            btnNavBilling.BackColor = Color.FromArgb(248, 250, 252);
            btnNavBilling.Cursor = Cursors.Hand;
            btnNavBilling.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnNavBilling.FlatAppearance.BorderSize = 1;
            btnNavBilling.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnNavBilling.FlatStyle = FlatStyle.Flat;
            btnNavBilling.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavBilling.ForeColor = Color.FromArgb(15, 23, 42);
            btnNavBilling.Location = new Point(572, 9);
            btnNavBilling.Name = "btnNavBilling";
            btnNavBilling.Size = new Size(188, 38);
            btnNavBilling.TabIndex = 3;
            btnNavBilling.Text = "💰 Dịch Vụ & Doanh Thu";
            btnNavBilling.UseVisualStyleBackColor = false;
            // 
            // btnNavTournament
            // 
            btnNavTournament.BackColor = Color.FromArgb(248, 250, 252);
            btnNavTournament.Cursor = Cursors.Hand;
            btnNavTournament.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnNavTournament.FlatAppearance.BorderSize = 1;
            btnNavTournament.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnNavTournament.FlatStyle = FlatStyle.Flat;
            btnNavTournament.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavTournament.ForeColor = Color.FromArgb(15, 23, 42);
            btnNavTournament.Location = new Point(398, 9);
            btnNavTournament.Name = "btnNavTournament";
            btnNavTournament.Size = new Size(164, 38);
            btnNavTournament.TabIndex = 2;
            btnNavTournament.Text = "🏆 Giải Đấu Quán";
            btnNavTournament.UseVisualStyleBackColor = false;
            // 
            // btnNavMember
            // 
            btnNavMember.BackColor = Color.FromArgb(248, 250, 252);
            btnNavMember.Cursor = Cursors.Hand;
            btnNavMember.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnNavMember.FlatAppearance.BorderSize = 1;
            btnNavMember.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnNavMember.FlatStyle = FlatStyle.Flat;
            btnNavMember.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavMember.ForeColor = Color.FromArgb(15, 23, 42);
            btnNavMember.Location = new Point(210, 9);
            btnNavMember.Name = "btnNavMember";
            btnNavMember.Size = new Size(178, 38);
            btnNavMember.TabIndex = 1;
            btnNavMember.Text = "👤 Hội Viên & Nạp Ví";
            btnNavMember.UseVisualStyleBackColor = false;
            // 
            // btnNavStation
            // 
            btnNavStation.BackColor = Color.FromArgb(248, 250, 252);
            btnNavStation.Cursor = Cursors.Hand;
            btnNavStation.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnNavStation.FlatAppearance.BorderSize = 1;
            btnNavStation.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnNavStation.FlatStyle = FlatStyle.Flat;
            btnNavStation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNavStation.ForeColor = Color.FromArgb(15, 23, 42);
            btnNavStation.Location = new Point(16, 9);
            btnNavStation.Name = "btnNavStation";
            btnNavStation.Size = new Size(184, 38);
            btnNavStation.TabIndex = 0;
            btnNavStation.Text = "🖥️ Máy Trạm & Dàn Máy";
            btnNavStation.UseVisualStyleBackColor = false;
            // 
            // pnlLegend
            // 
            pnlLegend.BackColor = Color.FromArgb(248, 250, 252);
            pnlLegend.Controls.Add(lblRealtimeSummary);
            pnlLegend.Controls.Add(lblLegendTournament);
            pnlLegend.Controls.Add(lblLegendMaintenance);
            pnlLegend.Controls.Add(lblLegendInUse);
            pnlLegend.Controls.Add(lblLegendAvailable);
            pnlLegend.Controls.Add(lblLegendTitle);
            pnlLegend.Dock = DockStyle.Top;
            pnlLegend.Location = new Point(0, 136);
            pnlLegend.Name = "pnlLegend";
            pnlLegend.Size = new Size(1353, 42);
            pnlLegend.TabIndex = 2;
            // 
            // lblRealtimeSummary
            // 
            lblRealtimeSummary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRealtimeSummary.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRealtimeSummary.ForeColor = Color.FromArgb(15, 23, 42);
            lblRealtimeSummary.Location = new Point(965, 6);
            lblRealtimeSummary.Name = "lblRealtimeSummary";
            lblRealtimeSummary.Size = new Size(370, 30);
            lblRealtimeSummary.TabIndex = 5;
            lblRealtimeSummary.Text = "🟢 Trống: 0 | 🟠 Đang chơi: 0 | 🕒 --:--:--";
            lblRealtimeSummary.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblLegendTournament
            // 
            lblLegendTournament.AutoSize = true;
            lblLegendTournament.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendTournament.ForeColor = Color.FromArgb(161, 98, 7);
            lblLegendTournament.Location = new Point(512, 11);
            lblLegendTournament.Name = "lblLegendTournament";
            lblLegendTournament.Size = new Size(130, 20);
            lblLegendTournament.TabIndex = 4;
            lblLegendTournament.Text = "🟡 Khóa Giải Đấu";
            // 
            // lblLegendMaintenance
            // 
            lblLegendMaintenance.AutoSize = true;
            lblLegendMaintenance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendMaintenance.ForeColor = Color.FromArgb(185, 28, 28);
            lblLegendMaintenance.Location = new Point(372, 11);
            lblLegendMaintenance.Name = "lblLegendMaintenance";
            lblLegendMaintenance.Size = new Size(125, 20);
            lblLegendMaintenance.TabIndex = 3;
            lblLegendMaintenance.Text = "🔴 Đang Bảo Trì";
            // 
            // lblLegendInUse
            // 
            lblLegendInUse.AutoSize = true;
            lblLegendInUse.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendInUse.ForeColor = Color.FromArgb(194, 65, 12);
            lblLegendInUse.Location = new Point(236, 11);
            lblLegendInUse.Name = "lblLegendInUse";
            lblLegendInUse.Size = new Size(120, 20);
            lblLegendInUse.TabIndex = 2;
            lblLegendInUse.Text = "🟠 Đang Chơi";
            // 
            // lblLegendAvailable
            // 
            lblLegendAvailable.AutoSize = true;
            lblLegendAvailable.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendAvailable.ForeColor = Color.FromArgb(21, 128, 61);
            lblLegendAvailable.Location = new Point(102, 11);
            lblLegendAvailable.Name = "lblLegendAvailable";
            lblLegendAvailable.Size = new Size(120, 20);
            lblLegendAvailable.TabIndex = 1;
            lblLegendAvailable.Text = "🟢 Sẵn Sàng";
            // 
            // lblLegendTitle
            // 
            lblLegendTitle.AutoSize = true;
            lblLegendTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegendTitle.ForeColor = Color.FromArgb(71, 85, 105);
            lblLegendTitle.Location = new Point(18, 11);
            lblLegendTitle.Name = "lblLegendTitle";
            lblLegendTitle.Size = new Size(80, 20);
            lblLegendTitle.TabIndex = 0;
            lblLegendTitle.Text = "Trạng thái:";
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(241, 245, 249);
            pnlRight.Controls.Add(grpDieuKhienPhien);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(973, 178);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(8, 10, 12, 12);
            pnlRight.Size = new Size(380, 783);
            pnlRight.TabIndex = 3;
            // 
            // grpDieuKhienPhien
            // 
            grpDieuKhienPhien.BackColor = Color.White;
            grpDieuKhienPhien.Controls.Add(lblKetQuaTinhTien);
            grpDieuKhienPhien.Controls.Add(btnChatStation);
            grpDieuKhienPhien.Controls.Add(btnBaoTri);
            grpDieuKhienPhien.Controls.Add(btnDongMay);
            grpDieuKhienPhien.Controls.Add(btnGoiMon);
            grpDieuKhienPhien.Controls.Add(btnMoMay);
            grpDieuKhienPhien.Controls.Add(cboGoiCuoc);
            grpDieuKhienPhien.Controls.Add(lblGoiCuoc);
            grpDieuKhienPhien.Controls.Add(cboKhachHang);
            grpDieuKhienPhien.Controls.Add(lblKhachHang);
            grpDieuKhienPhien.Controls.Add(lblCauHinhMay);
            grpDieuKhienPhien.Controls.Add(lblGiaGio);
            grpDieuKhienPhien.Controls.Add(lblKhuVucMay);
            grpDieuKhienPhien.Controls.Add(lblTrangThaiMay);
            grpDieuKhienPhien.Controls.Add(lblMayDangChon);
            grpDieuKhienPhien.Dock = DockStyle.Fill;
            grpDieuKhienPhien.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpDieuKhienPhien.ForeColor = Color.FromArgb(15, 23, 42);
            grpDieuKhienPhien.Location = new Point(8, 10);
            grpDieuKhienPhien.Name = "grpDieuKhienPhien";
            grpDieuKhienPhien.Padding = new Padding(6);
            grpDieuKhienPhien.Size = new Size(360, 761);
            grpDieuKhienPhien.TabIndex = 0;
            grpDieuKhienPhien.TabStop = false;
            grpDieuKhienPhien.Text = "Điều Khiển Máy Trạm";
            // 
            // lblKetQuaTinhTien
            // 
            lblKetQuaTinhTien.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblKetQuaTinhTien.BackColor = Color.FromArgb(248, 250, 252);
            lblKetQuaTinhTien.BorderStyle = BorderStyle.FixedSingle;
            lblKetQuaTinhTien.Font = new Font("Segoe UI", 9F);
            lblKetQuaTinhTien.ForeColor = Color.FromArgb(30, 41, 59);
            lblKetQuaTinhTien.Location = new Point(16, 476);
            lblKetQuaTinhTien.Name = "lblKetQuaTinhTien";
            lblKetQuaTinhTien.Padding = new Padding(10, 8, 10, 8);
            lblKetQuaTinhTien.Size = new Size(328, 268);
            lblKetQuaTinhTien.TabIndex = 13;
            lblKetQuaTinhTien.Text = "Thông tin phiên chơi và tạm tính tiền sẽ hiển thị chi tiết tại đây.";
            // 
            // btnChatStation
            // 
            btnChatStation.BackColor = Color.FromArgb(13, 148, 136);
            btnChatStation.Cursor = Cursors.Hand;
            btnChatStation.FlatAppearance.BorderSize = 0;
            btnChatStation.FlatStyle = FlatStyle.Flat;
            btnChatStation.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnChatStation.ForeColor = Color.White;
            btnChatStation.Location = new Point(16, 430);
            btnChatStation.Name = "btnChatStation";
            btnChatStation.Size = new Size(328, 36);
            btnChatStation.TabIndex = 12;
            btnChatStation.Text = "💬 CHAT VỚI MÁY TRẠM";
            btnChatStation.UseVisualStyleBackColor = false;
            // 
            // btnBaoTri
            // 
            btnBaoTri.BackColor = Color.FromArgb(71, 85, 105);
            btnBaoTri.Cursor = Cursors.Hand;
            btnBaoTri.FlatAppearance.BorderSize = 0;
            btnBaoTri.FlatStyle = FlatStyle.Flat;
            btnBaoTri.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnBaoTri.ForeColor = Color.White;
            btnBaoTri.Location = new Point(16, 388);
            btnBaoTri.Name = "btnBaoTri";
            btnBaoTri.Size = new Size(328, 36);
            btnBaoTri.TabIndex = 11;
            btnBaoTri.Text = "🛠️ CHUYỂN BẢO TRÌ / MỞ LẠI";
            btnBaoTri.UseVisualStyleBackColor = false;
            // 
            // btnDongMay
            // 
            btnDongMay.BackColor = Color.FromArgb(220, 38, 38);
            btnDongMay.Cursor = Cursors.Hand;
            btnDongMay.FlatAppearance.BorderSize = 0;
            btnDongMay.FlatStyle = FlatStyle.Flat;
            btnDongMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnDongMay.ForeColor = Color.White;
            btnDongMay.Location = new Point(16, 346);
            btnDongMay.Name = "btnDongMay";
            btnDongMay.Size = new Size(328, 36);
            btnDongMay.TabIndex = 10;
            btnDongMay.Text = "🔴 ĐÓNG MÁY & TÍNH TIỀN";
            btnDongMay.UseVisualStyleBackColor = false;
            // 
            // btnGoiMon
            // 
            btnGoiMon.BackColor = Color.FromArgb(230, 126, 34);
            btnGoiMon.Cursor = Cursors.Hand;
            btnGoiMon.FlatAppearance.BorderSize = 0;
            btnGoiMon.FlatStyle = FlatStyle.Flat;
            btnGoiMon.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnGoiMon.ForeColor = Color.White;
            btnGoiMon.Location = new Point(16, 304);
            btnGoiMon.Name = "btnGoiMon";
            btnGoiMon.Size = new Size(328, 36);
            btnGoiMon.TabIndex = 9;
            btnGoiMon.Text = "🛒 GỌI MÓN F&B CHO MÁY";
            btnGoiMon.UseVisualStyleBackColor = false;
            // 
            // btnMoMay
            // 
            btnMoMay.BackColor = Color.FromArgb(22, 163, 74);
            btnMoMay.Cursor = Cursors.Hand;
            btnMoMay.FlatAppearance.BorderSize = 0;
            btnMoMay.FlatStyle = FlatStyle.Flat;
            btnMoMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnMoMay.ForeColor = Color.White;
            btnMoMay.Location = new Point(16, 262);
            btnMoMay.Name = "btnMoMay";
            btnMoMay.Size = new Size(328, 36);
            btnMoMay.TabIndex = 8;
            btnMoMay.Text = "🟢 MỞ MÁY CHO KHÁCH";
            btnMoMay.UseVisualStyleBackColor = false;
            // 
            // cboGoiCuoc
            // 
            cboGoiCuoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGoiCuoc.Font = new Font("Segoe UI", 9F);
            cboGoiCuoc.FormattingEnabled = true;
            cboGoiCuoc.Items.AddRange(new object[] { "Hourly (Giờ thường)", "NightCombo (Combo đêm 45k)", "Tournament (Giải Đấu - Giảm 50%)", "MemberTier (Ưu đãi hội viên)" });
            cboGoiCuoc.Location = new Point(116, 222);
            cboGoiCuoc.Name = "cboGoiCuoc";
            cboGoiCuoc.Size = new Size(228, 28);
            cboGoiCuoc.TabIndex = 7;
            // 
            // lblGoiCuoc
            // 
            lblGoiCuoc.AutoSize = true;
            lblGoiCuoc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGoiCuoc.ForeColor = Color.FromArgb(71, 85, 105);
            lblGoiCuoc.Location = new Point(16, 226);
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
            cboKhachHang.Location = new Point(116, 186);
            cboKhachHang.Name = "cboKhachHang";
            cboKhachHang.Size = new Size(228, 28);
            cboKhachHang.TabIndex = 5;
            // 
            // lblKhachHang
            // 
            lblKhachHang.AutoSize = true;
            lblKhachHang.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKhachHang.ForeColor = Color.FromArgb(71, 85, 105);
            lblKhachHang.Location = new Point(16, 190);
            lblKhachHang.Name = "lblKhachHang";
            lblKhachHang.Size = new Size(89, 20);
            lblKhachHang.TabIndex = 4;
            lblKhachHang.Text = "Khách hàng:";
            // 
            // lblCauHinhMay
            // 
            lblCauHinhMay.BackColor = Color.FromArgb(241, 245, 249);
            lblCauHinhMay.BorderStyle = BorderStyle.FixedSingle;
            lblCauHinhMay.Font = new Font("Segoe UI", 8F);
            lblCauHinhMay.ForeColor = Color.FromArgb(51, 65, 85);
            lblCauHinhMay.Location = new Point(16, 132);
            lblCauHinhMay.Name = "lblCauHinhMay";
            lblCauHinhMay.Padding = new Padding(6, 4, 6, 4);
            lblCauHinhMay.Size = new Size(328, 44);
            lblCauHinhMay.TabIndex = 14;
            lblCauHinhMay.Text = "⚙️ Cấu hình dàn máy sẽ hiển thị tại đây.";
            // 
            // lblGiaGio
            // 
            lblGiaGio.AutoSize = true;
            lblGiaGio.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblGiaGio.ForeColor = Color.FromArgb(37, 99, 235);
            lblGiaGio.Location = new Point(16, 106);
            lblGiaGio.Name = "lblGiaGio";
            lblGiaGio.Size = new Size(180, 21);
            lblGiaGio.TabIndex = 3;
            lblGiaGio.Text = "Đơn giá: 10,000 VNĐ/h";
            // 
            // lblKhuVucMay
            // 
            lblKhuVucMay.AutoSize = true;
            lblKhuVucMay.Font = new Font("Segoe UI", 9.5F);
            lblKhuVucMay.ForeColor = Color.FromArgb(51, 65, 85);
            lblKhuVucMay.Location = new Point(16, 82);
            lblKhuVucMay.Name = "lblKhuVucMay";
            lblKhuVucMay.Size = new Size(165, 21);
            lblKhuVucMay.TabIndex = 2;
            lblKhuVucMay.Text = "Dàn máy: Dàn Standard";
            // 
            // lblTrangThaiMay
            // 
            lblTrangThaiMay.AutoSize = true;
            lblTrangThaiMay.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTrangThaiMay.ForeColor = Color.FromArgb(22, 163, 74);
            lblTrangThaiMay.Location = new Point(16, 56);
            lblTrangThaiMay.Name = "lblTrangThaiMay";
            lblTrangThaiMay.Size = new Size(167, 21);
            lblTrangThaiMay.TabIndex = 1;
            lblTrangThaiMay.Text = "Trạng thái: Available";
            // 
            // lblMayDangChon
            // 
            lblMayDangChon.AutoSize = true;
            lblMayDangChon.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            lblMayDangChon.ForeColor = Color.FromArgb(15, 23, 42);
            lblMayDangChon.Location = new Point(16, 26);
            lblMayDangChon.Name = "lblMayDangChon";
            lblMayDangChon.Size = new Size(207, 28);
            lblMayDangChon.TabIndex = 0;
            lblMayDangChon.Text = "🖥️ Máy Trạm: PC-01";
            // 
            // pnlCenter
            // 
            pnlCenter.BackColor = Color.FromArgb(241, 245, 249);
            pnlCenter.Controls.Add(flpDanhSachMay);
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(0, 178);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Padding = new Padding(12, 10, 6, 12);
            pnlCenter.Size = new Size(973, 783);
            pnlCenter.TabIndex = 4;
            // 
            // flpDanhSachMay
            // 
            flpDanhSachMay.AutoScroll = true;
            flpDanhSachMay.BackColor = Color.FromArgb(241, 245, 249);
            flpDanhSachMay.BorderStyle = BorderStyle.None;
            flpDanhSachMay.Dock = DockStyle.Fill;
            flpDanhSachMay.FlowDirection = FlowDirection.TopDown;
            flpDanhSachMay.Location = new Point(12, 10);
            flpDanhSachMay.Name = "flpDanhSachMay";
            flpDanhSachMay.Padding = new Padding(2, 2, 10, 10);
            flpDanhSachMay.Size = new Size(955, 761);
            flpDanhSachMay.TabIndex = 0;
            flpDanhSachMay.WrapContents = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            ClientSize = new Size(1353, 961);
            Controls.Add(pnlCenter);
            Controls.Add(pnlRight);
            Controls.Add(pnlLegend);
            Controls.Add(pnlNav);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(1280, 930);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Master Game - Hệ Thống Quản Lý Phòng Máy (Cyber Matrix)";
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
        private Label lblCurrentUser;
        private Button btnDangXuat;
        private Panel pnlNav;
        private Button btnNavStation;
        private Button btnNavMember;
        private Button btnNavTournament;
        private Button btnNavBilling;
        private Button btnClientMode;
        public Button btnChatHub;
        private Button btnLamMoiSoDo;
        private Panel pnlLegend;
        private Label lblLegendTitle;
        private Label lblLegendAvailable;
        private Label lblLegendInUse;
        private Label lblLegendMaintenance;
        private Label lblLegendTournament;
        private Label lblRealtimeSummary;
        private Panel pnlRight;
        private GroupBox grpDieuKhienPhien;
        private Label lblMayDangChon;
        private Label lblTrangThaiMay;
        private Label lblKhuVucMay;
        private Label lblGiaGio;
        private Label lblCauHinhMay;
        private Label lblKhachHang;
        private ComboBox cboKhachHang;
        private Label lblGoiCuoc;
        private ComboBox cboGoiCuoc;
        private Button btnMoMay;
        private Button btnGoiMon;
        private Button btnDongMay;
        private Button btnBaoTri;
        public Button btnChatStation;
        private Label lblKetQuaTinhTien;
        private Panel pnlCenter;
        private FlowLayoutPanel flpDanhSachMay;
    }
}
