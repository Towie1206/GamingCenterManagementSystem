using System.Drawing;
using System.Windows.Forms;

namespace GamingCenterManagementSystem
{
    partial class FormStationRemoteControl
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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.lblHeaderSub = new System.Windows.Forms.Label();
            this.pnlStationBanner = new System.Windows.Forms.Panel();
            this.lblStationInfo = new System.Windows.Forms.Label();
            this.lblStationStatusBadge = new System.Windows.Forms.Label();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabForceLock = new System.Windows.Forms.TabPage();
            this.grpLockControls = new System.Windows.Forms.GroupBox();
            this.lblLockReason = new System.Windows.Forms.Label();
            this.cboLockTemplates = new System.Windows.Forms.ComboBox();
            this.txtLockReason = new System.Windows.Forms.TextBox();
            this.btnForceLock = new System.Windows.Forms.Button();
            this.btnUnlock = new System.Windows.Forms.Button();
            this.grpPowerControls = new System.Windows.Forms.GroupBox();
            this.btnShutdown = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.grpAlert = new System.Windows.Forms.GroupBox();
            this.lblAlertMsg = new System.Windows.Forms.Label();
            this.cboAlertTemplates = new System.Windows.Forms.ComboBox();
            this.txtAlertMessage = new System.Windows.Forms.TextBox();
            this.chkBroadcast = new System.Windows.Forms.CheckBox();
            this.btnSendAlert = new System.Windows.Forms.Button();
            this.tabTransfer = new System.Windows.Forms.TabPage();
            this.grpTransfer = new System.Windows.Forms.GroupBox();
            this.lblSourceInfo = new System.Windows.Forms.Label();
            this.lblTargetPrompt = new System.Windows.Forms.Label();
            this.cboTargetStation = new System.Windows.Forms.ComboBox();
            this.lblTransferNotice = new System.Windows.Forms.Label();
            this.btnTransferStation = new System.Windows.Forms.Button();
            this.tabProcessManager = new System.Windows.Forms.TabPage();
            this.dgvRunningApps = new System.Windows.Forms.DataGridView();
            this.pnlAppToolbar = new System.Windows.Forms.Panel();
            this.btnKillApp = new System.Windows.Forms.Button();
            this.btnRefreshApps = new System.Windows.Forms.Button();
            this.lblAppNotice = new System.Windows.Forms.Label();
            this.tabScreenshot = new System.Windows.Forms.TabPage();
            this.picScreenshot = new System.Windows.Forms.PictureBox();
            this.pnlScreenshotToolbar = new System.Windows.Forms.Panel();
            this.btnCaptureScreen = new System.Windows.Forms.Button();
            this.btnSaveScreenshot = new System.Windows.Forms.Button();
            this.lblScreenshotMeta = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatusFooter = new System.Windows.Forms.Label();

            // Form configuration
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(920, 680);
            this.MinimumSize = new System.Drawing.Size(860, 620);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trung Tâm Điều Khiển & Quản Trị Máy Trạm Từ Xa - Master Game";
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);

            // pnlHeader
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 70;
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(18, 10, 18, 10);
            this.pnlHeader.Controls.Add(this.lblHeaderSub);
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);

            // lblHeaderTitle
            this.lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Text = "🕹️ TRUNG TÂM ĐIỀU KHIỂN & CƯỠNG CHẾ MÁY TRẠM TỪ XA";
            this.lblHeaderTitle.Height = 26;

            // lblHeaderSub
            this.lblHeaderSub.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblHeaderSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblHeaderSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblHeaderSub.Text = "Master Game 165 Hoàng Công Chất • Quyền Quản Trị Máy Tổng • Tắt game, chuyển trạm, khóa cưỡng chế & chụp màn hình";
            this.lblHeaderSub.Height = 20;

            // pnlStationBanner
            this.pnlStationBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStationBanner.Height = 44;
            this.pnlStationBanner.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.pnlStationBanner.Padding = new System.Windows.Forms.Padding(18, 6, 18, 6);
            this.pnlStationBanner.Controls.Add(this.lblStationStatusBadge);
            this.pnlStationBanner.Controls.Add(this.lblStationInfo);

            // lblStationInfo
            this.lblStationInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStationInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStationInfo.ForeColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.lblStationInfo.Text = "Máy trạm: PC-01 | Zone: Dàn Standard (10.000đ/h) | Khách: Khách vãng lai";
            this.lblStationInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStationInfo.AutoSize = true;

            // lblStationStatusBadge
            this.lblStationStatusBadge.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblStationStatusBadge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStationStatusBadge.ForeColor = System.Drawing.Color.White;
            this.lblStationStatusBadge.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.lblStationStatusBadge.Text = "TRẠNG THÁI: BÌNH THƯỜNG";
            this.lblStationStatusBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStationStatusBadge.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.lblStationStatusBadge.AutoSize = true;

            // tabMain
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Padding = new System.Drawing.Point(12, 8);
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.tabMain.Controls.Add(this.tabForceLock);
            this.tabMain.Controls.Add(this.tabProcessManager);
            this.tabMain.Controls.Add(this.tabTransfer);
            this.tabMain.Controls.Add(this.tabScreenshot);

            // tabForceLock
            this.tabForceLock.Text = "🔒 Khóa Cưỡng Chế & Nguồn";
            this.tabForceLock.BackColor = System.Drawing.Color.White;
            this.tabForceLock.Padding = new System.Windows.Forms.Padding(16);
            this.tabForceLock.Controls.Add(this.grpAlert);
            this.tabForceLock.Controls.Add(this.grpPowerControls);
            this.tabForceLock.Controls.Add(this.grpLockControls);

            // grpLockControls
            this.grpLockControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLockControls.Height = 150;
            this.grpLockControls.Text = "Khóa Màn Hình Cưỡng Chế (Client Force Lock)";
            this.grpLockControls.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.grpLockControls.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpLockControls.Padding = new System.Windows.Forms.Padding(12);

            this.lblLockReason.Location = new System.Drawing.Point(14, 25);
            this.lblLockReason.Size = new System.Drawing.Size(250, 18);
            this.lblLockReason.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLockReason.Text = "Lý do khóa / Chọn mẫu quy định vi phạm:";

            this.cboLockTemplates.Location = new System.Drawing.Point(16, 45);
            this.cboLockTemplates.Size = new System.Drawing.Size(520, 23);
            this.cboLockTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.txtLockReason.Location = new System.Drawing.Point(16, 75);
            this.txtLockReason.Size = new System.Drawing.Size(520, 23);
            this.txtLockReason.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtLockReason.PlaceholderText = "Nhập lý do cụ thể hiển thị trực tiếp lên màn hình máy con...";

            this.btnForceLock.Location = new System.Drawing.Point(16, 106);
            this.btnForceLock.Size = new System.Drawing.Size(240, 32);
            this.btnForceLock.Text = "🔒 KHÓA CƯỠNG CHẾ MÁY NÀY";
            this.btnForceLock.BackColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnForceLock.ForeColor = System.Drawing.Color.White;
            this.btnForceLock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForceLock.FlatAppearance.BorderSize = 0;
            this.btnForceLock.Cursor = System.Windows.Forms.Cursors.Hand;

            this.btnUnlock.Location = new System.Drawing.Point(265, 106);
            this.btnUnlock.Size = new System.Drawing.Size(220, 32);
            this.btnUnlock.Text = "🔓 MỞ KHÓA MÁY TRẠM";
            this.btnUnlock.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnUnlock.ForeColor = System.Drawing.Color.White;
            this.btnUnlock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUnlock.FlatAppearance.BorderSize = 0;
            this.btnUnlock.Cursor = System.Windows.Forms.Cursors.Hand;

            this.grpLockControls.Controls.Add(this.lblLockReason);
            this.grpLockControls.Controls.Add(this.cboLockTemplates);
            this.grpLockControls.Controls.Add(this.txtLockReason);
            this.grpLockControls.Controls.Add(this.btnForceLock);
            this.grpLockControls.Controls.Add(this.btnUnlock);

            // grpPowerControls
            this.grpPowerControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpPowerControls.Height = 85;
            this.grpPowerControls.Text = "Điều Khiển Nguồn Từ Xa (Remote Power Control)";
            this.grpPowerControls.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.grpPowerControls.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPowerControls.Padding = new System.Windows.Forms.Padding(12);

            this.btnShutdown.Location = new System.Drawing.Point(16, 32);
            this.btnShutdown.Size = new System.Drawing.Size(240, 34);
            this.btnShutdown.Text = "⚡ TẮT MÁY TỪ XA (10s Countdown)";
            this.btnShutdown.BackColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnShutdown.ForeColor = System.Drawing.Color.White;
            this.btnShutdown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShutdown.FlatAppearance.BorderSize = 0;
            this.btnShutdown.Cursor = System.Windows.Forms.Cursors.Hand;

            this.btnRestart.Location = new System.Drawing.Point(265, 32);
            this.btnRestart.Size = new System.Drawing.Size(220, 34);
            this.btnRestart.Text = "🔄 KHỞI ĐỘNG LẠI (Reboot)";
            this.btnRestart.BackColor = System.Drawing.Color.FromArgb(13, 148, 136);
            this.btnRestart.ForeColor = System.Drawing.Color.White;
            this.btnRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestart.FlatAppearance.BorderSize = 0;
            this.btnRestart.Cursor = System.Windows.Forms.Cursors.Hand;

            this.grpPowerControls.Controls.Add(this.btnShutdown);
            this.grpPowerControls.Controls.Add(this.btnRestart);

            // grpAlert
            this.grpAlert.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAlert.Text = "Bắn Cảnh Báo Trực Tiếp Giữa Màn Hình (Direct Screen Popup Alert)";
            this.grpAlert.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.grpAlert.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAlert.Padding = new System.Windows.Forms.Padding(12);

            this.lblAlertMsg.Location = new System.Drawing.Point(14, 25);
            this.lblAlertMsg.Size = new System.Drawing.Size(350, 18);
            this.lblAlertMsg.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAlertMsg.Text = "Nội dung cảnh báo đập thẳng vào mắt người chơi:";

            this.cboAlertTemplates.Location = new System.Drawing.Point(16, 45);
            this.cboAlertTemplates.Size = new System.Drawing.Size(520, 23);
            this.cboAlertTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.txtAlertMessage.Location = new System.Drawing.Point(16, 75);
            this.txtAlertMessage.Size = new System.Drawing.Size(520, 48);
            this.txtAlertMessage.Multiline = true;
            this.txtAlertMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtAlertMessage.PlaceholderText = "Nhập nội dung cảnh báo / nhắc nhở gửi riêng hoặc toàn quán...";

            this.chkBroadcast.Location = new System.Drawing.Point(16, 132);
            this.chkBroadcast.Size = new System.Drawing.Size(300, 22);
            this.chkBroadcast.Text = "📢 Gửi cho TOÀN BỘ 30 MÁY trong quán (Broadcast)";
            this.chkBroadcast.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkBroadcast.ForeColor = System.Drawing.Color.FromArgb(180, 83, 9);

            this.btnSendAlert.Location = new System.Drawing.Point(16, 160);
            this.btnSendAlert.Size = new System.Drawing.Size(260, 34);
            this.btnSendAlert.Text = "📢 BẮN CẢNH BÁO POP-UP NGAY";
            this.btnSendAlert.BackColor = System.Drawing.Color.FromArgb(234, 88, 12);
            this.btnSendAlert.ForeColor = System.Drawing.Color.White;
            this.btnSendAlert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendAlert.FlatAppearance.BorderSize = 0;
            this.btnSendAlert.Cursor = System.Windows.Forms.Cursors.Hand;

            this.grpAlert.Controls.Add(this.lblAlertMsg);
            this.grpAlert.Controls.Add(this.cboAlertTemplates);
            this.grpAlert.Controls.Add(this.txtAlertMessage);
            this.grpAlert.Controls.Add(this.chkBroadcast);
            this.grpAlert.Controls.Add(this.btnSendAlert);

            // tabProcessManager
            this.tabProcessManager.Text = "❌ Tắt Game & Quản Lý App";
            this.tabProcessManager.BackColor = System.Drawing.Color.White;
            this.tabProcessManager.Padding = new System.Windows.Forms.Padding(12);

            this.dgvRunningApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRunningApps.BackgroundColor = System.Drawing.Color.White;
            this.dgvRunningApps.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvRunningApps.RowHeadersVisible = false;
            this.dgvRunningApps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRunningApps.MultiSelect = false;
            this.dgvRunningApps.AllowUserToAddRows = false;
            this.dgvRunningApps.AllowUserToDeleteRows = false;
            this.dgvRunningApps.ReadOnly = true;
            this.dgvRunningApps.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            this.pnlAppToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAppToolbar.Height = 56;
            this.pnlAppToolbar.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);

            this.btnKillApp.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnKillApp.Width = 260;
            this.btnKillApp.Text = "❌ TẮT ỨNG DỤNG / GAME NÀY NGAY";
            this.btnKillApp.BackColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnKillApp.ForeColor = System.Drawing.Color.White;
            this.btnKillApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKillApp.FlatAppearance.BorderSize = 0;
            this.btnKillApp.Cursor = System.Windows.Forms.Cursors.Hand;

            this.btnRefreshApps.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRefreshApps.Width = 140;
            this.btnRefreshApps.Text = "🔄 Làm mới";
            this.btnRefreshApps.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnRefreshApps.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnRefreshApps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshApps.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblAppNotice.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblAppNotice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblAppNotice.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblAppNotice.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblAppNotice.Text = "Quyền máy tổng: Ép đóng game bị treo hoặc phần mềm gian lận không cho phép.";
            this.lblAppNotice.Width = 400;

            this.pnlAppToolbar.Controls.Add(this.lblAppNotice);
            this.pnlAppToolbar.Controls.Add(this.btnRefreshApps);
            this.pnlAppToolbar.Controls.Add(this.btnKillApp);

            this.tabProcessManager.Controls.Add(this.dgvRunningApps);
            this.tabProcessManager.Controls.Add(this.pnlAppToolbar);

            // tabTransfer
            this.tabTransfer.Text = "🔄 Chuyển Trạm Chơi";
            this.tabTransfer.BackColor = System.Drawing.Color.White;
            this.tabTransfer.Padding = new System.Windows.Forms.Padding(18);

            this.grpTransfer.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTransfer.Height = 280;
            this.grpTransfer.Text = "Chuyển Phiên Chơi Sang Máy Khác (Session Transfer)";
            this.grpTransfer.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.grpTransfer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTransfer.Padding = new System.Windows.Forms.Padding(16);

            this.lblSourceInfo.Location = new System.Drawing.Point(16, 32);
            this.lblSourceInfo.Size = new System.Drawing.Size(600, 30);
            this.lblSourceInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSourceInfo.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblSourceInfo.Text = "Máy nguồn hiện tại: PC-01 (Khu vực: Dàn Standard • 10.000đ/h)";

            this.lblTargetPrompt.Location = new System.Drawing.Point(16, 75);
            this.lblTargetPrompt.Size = new System.Drawing.Size(350, 18);
            this.lblTargetPrompt.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTargetPrompt.Text = "Chọn máy đích muốn chuyển sang (Chỉ các máy đang trống):";

            this.cboTargetStation.Location = new System.Drawing.Point(16, 98);
            this.cboTargetStation.Size = new System.Drawing.Size(520, 24);
            this.cboTargetStation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.lblTransferNotice.Location = new System.Drawing.Point(16, 135);
            this.lblTransferNotice.Size = new System.Drawing.Size(550, 60);
            this.lblTransferNotice.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTransferNotice.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblTransferNotice.Text = "• Khi bấm xác nhận chuyển, toàn bộ phiên chơi, tài khoản hội viên và thời gian của khách sẽ được bốc sang máy đích.\n• Máy cũ sẽ tự động đóng lại và trở về trạng thái Trống.\n• Nếu chuyển khác khu vực, cước phí giờ chơi sẽ tự động tính theo đơn giá của dàn máy mới.";

            this.btnTransferStation.Location = new System.Drawing.Point(16, 210);
            this.btnTransferStation.Size = new System.Drawing.Size(260, 36);
            this.btnTransferStation.Text = "🚀 XÁC NHẬN CHUYỂN MÁY NGAY";
            this.btnTransferStation.BackColor = System.Drawing.Color.FromArgb(13, 148, 136);
            this.btnTransferStation.ForeColor = System.Drawing.Color.White;
            this.btnTransferStation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTransferStation.FlatAppearance.BorderSize = 0;
            this.btnTransferStation.Cursor = System.Windows.Forms.Cursors.Hand;

            this.grpTransfer.Controls.Add(this.lblSourceInfo);
            this.grpTransfer.Controls.Add(this.lblTargetPrompt);
            this.grpTransfer.Controls.Add(this.cboTargetStation);
            this.grpTransfer.Controls.Add(this.lblTransferNotice);
            this.grpTransfer.Controls.Add(this.btnTransferStation);

            this.tabTransfer.Controls.Add(this.grpTransfer);

            // tabScreenshot
            this.tabScreenshot.Text = "📸 Chụp Màn Hình Từ Xa";
            this.tabScreenshot.BackColor = System.Drawing.Color.White;
            this.tabScreenshot.Padding = new System.Windows.Forms.Padding(12);

            this.picScreenshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picScreenshot.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.picScreenshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picScreenshot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.pnlScreenshotToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlScreenshotToolbar.Height = 50;
            this.pnlScreenshotToolbar.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);

            this.btnCaptureScreen.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCaptureScreen.Width = 200;
            this.btnCaptureScreen.Text = "📸 CHỤP MÀN HÌNH NGAY";
            this.btnCaptureScreen.BackColor = System.Drawing.Color.FromArgb(2, 132, 199);
            this.btnCaptureScreen.ForeColor = System.Drawing.Color.White;
            this.btnCaptureScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCaptureScreen.FlatAppearance.BorderSize = 0;
            this.btnCaptureScreen.Cursor = System.Windows.Forms.Cursors.Hand;

            this.btnSaveScreenshot.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSaveScreenshot.Width = 140;
            this.btnSaveScreenshot.Text = "💾 Lưu File Ảnh";
            this.btnSaveScreenshot.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnSaveScreenshot.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnSaveScreenshot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveScreenshot.Cursor = System.Windows.Forms.Cursors.Hand;

            this.lblScreenshotMeta.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblScreenshotMeta.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblScreenshotMeta.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblScreenshotMeta.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblScreenshotMeta.Text = "Giám sát màn hình trực tiếp chống gian lận & hỗ trợ sự cố từ xa.";
            this.lblScreenshotMeta.Width = 400;

            this.pnlScreenshotToolbar.Controls.Add(this.lblScreenshotMeta);
            this.pnlScreenshotToolbar.Controls.Add(this.btnSaveScreenshot);
            this.pnlScreenshotToolbar.Controls.Add(this.btnCaptureScreen);

            this.tabScreenshot.Controls.Add(this.picScreenshot);
            this.tabScreenshot.Controls.Add(this.pnlScreenshotToolbar);

            // pnlBottom
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height = 48;
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(18, 8, 18, 8);
            this.pnlBottom.Controls.Add(this.lblStatusFooter);
            this.pnlBottom.Controls.Add(this.btnClose);

            // lblStatusFooter
            this.lblStatusFooter.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStatusFooter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatusFooter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusFooter.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.lblStatusFooter.Text = "Sẵn sàng nhận lệnh điều khiển máy trạm.";
            this.lblStatusFooter.AutoSize = true;

            // btnClose
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Width = 110;
            this.btnClose.Text = "Đóng";
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;

            // Add top-level controls
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlStationBanner);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlBottom);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Label lblHeaderSub;
        private System.Windows.Forms.Panel pnlStationBanner;
        private System.Windows.Forms.Label lblStationInfo;
        private System.Windows.Forms.Label lblStationStatusBadge;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabForceLock;
        private System.Windows.Forms.GroupBox grpLockControls;
        private System.Windows.Forms.Label lblLockReason;
        private System.Windows.Forms.ComboBox cboLockTemplates;
        private System.Windows.Forms.TextBox txtLockReason;
        private System.Windows.Forms.Button btnForceLock;
        private System.Windows.Forms.Button btnUnlock;
        private System.Windows.Forms.GroupBox grpPowerControls;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.GroupBox grpAlert;
        private System.Windows.Forms.Label lblAlertMsg;
        private System.Windows.Forms.ComboBox cboAlertTemplates;
        private System.Windows.Forms.TextBox txtAlertMessage;
        private System.Windows.Forms.CheckBox chkBroadcast;
        private System.Windows.Forms.Button btnSendAlert;
        private System.Windows.Forms.TabPage tabTransfer;
        private System.Windows.Forms.GroupBox grpTransfer;
        private System.Windows.Forms.Label lblSourceInfo;
        private System.Windows.Forms.Label lblTargetPrompt;
        private System.Windows.Forms.ComboBox cboTargetStation;
        private System.Windows.Forms.Label lblTransferNotice;
        private System.Windows.Forms.Button btnTransferStation;
        private System.Windows.Forms.TabPage tabProcessManager;
        private System.Windows.Forms.DataGridView dgvRunningApps;
        private System.Windows.Forms.Panel pnlAppToolbar;
        private System.Windows.Forms.Button btnKillApp;
        private System.Windows.Forms.Button btnRefreshApps;
        private System.Windows.Forms.Label lblAppNotice;
        private System.Windows.Forms.TabPage tabScreenshot;
        private System.Windows.Forms.PictureBox picScreenshot;
        private System.Windows.Forms.Panel pnlScreenshotToolbar;
        private System.Windows.Forms.Button btnCaptureScreen;
        private System.Windows.Forms.Button btnSaveScreenshot;
        private System.Windows.Forms.Label lblScreenshotMeta;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatusFooter;
    }
}
