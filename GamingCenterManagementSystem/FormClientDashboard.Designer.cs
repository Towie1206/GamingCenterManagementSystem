namespace GamingCenterManagementSystem
{
    partial class FormClientDashboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_onRemoteCommandHandler != null)
                    Models.StationRemoteControlService.OnCommandDispatched -= _onRemoteCommandHandler;
                if (_onChatMessageSentHandler != null)
                    Models.StationChatService.OnMessageSent -= _onChatMessageSentHandler;
                if (_onChatUnreadChangedHandler != null)
                    Models.StationChatService.OnUnreadCountChanged -= _onChatUnreadChangedHandler;

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dgvHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dgvRowStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dgvOrdersHeaderStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dgvOrdersRowStyle = new System.Windows.Forms.DataGridViewCellStyle();
            pnlTopHud = new System.Windows.Forms.Panel();
            pnlHardwareHud = new System.Windows.Forms.Panel();
            lblHardwareLatencyHud = new System.Windows.Forms.Label();
            pnlStationInfo = new System.Windows.Forms.Panel();
            lblStationInfo = new System.Windows.Forms.Label();
            lblCenterBranding = new System.Windows.Forms.Label();
            pnlSessionTimers = new System.Windows.Forms.Panel();
            lblRemainingBalance = new System.Windows.Forms.Label();
            lblServiceCost = new System.Windows.Forms.Label();
            lblPlayCost = new System.Windows.Forms.Label();
            lblRemainingTime = new System.Windows.Forms.Label();
            lblPlayedTime = new System.Windows.Forms.Label();
            pnlAccountActions = new System.Windows.Forms.Panel();
            btnLogoutSession = new System.Windows.Forms.Button();
            btnCallStaff = new System.Windows.Forms.Button();
            btnChangePassword = new System.Windows.Forms.Button();
            btnLockScreen = new System.Windows.Forms.Button();
            lblWalletBalance = new System.Windows.Forms.Label();
            lblMemberTier = new System.Windows.Forms.Label();
            lblAccountInfo = new System.Windows.Forms.Label();
            tabMain = new System.Windows.Forms.TabControl();
            tabGameHub = new System.Windows.Forms.TabPage();
            pnlGameTop = new System.Windows.Forms.Panel();
            lblLauncherStatus = new System.Windows.Forms.Label();
            txtSearchGame = new System.Windows.Forms.TextBox();
            lblSearchGame = new System.Windows.Forms.Label();
            btnCatSports = new System.Windows.Forms.Button();
            btnCatRpg = new System.Windows.Forms.Button();
            btnCatFps = new System.Windows.Forms.Button();
            btnCatMoba = new System.Windows.Forms.Button();
            btnCatAll = new System.Windows.Forms.Button();
            flpGameCards = new System.Windows.Forms.FlowLayoutPanel();
            pnlActiveGame = new System.Windows.Forms.Panel();
            btnStopGame = new System.Windows.Forms.Button();
            lblActiveGameTitle = new System.Windows.Forms.Label();
            tabFoodMenu = new System.Windows.Forms.TabPage();
            pnlFoodLeft = new System.Windows.Forms.Panel();
            dgvFoodItems = new System.Windows.Forms.DataGridView();
            pnlFoodFilter = new System.Windows.Forms.Panel();
            txtSearchFood = new System.Windows.Forms.TextBox();
            lblSearchFood = new System.Windows.Forms.Label();
            btnFoodSnack = new System.Windows.Forms.Button();
            btnFoodMeal = new System.Windows.Forms.Button();
            btnFoodDrink = new System.Windows.Forms.Button();
            btnFoodAll = new System.Windows.Forms.Button();
            pnlFoodRight = new System.Windows.Forms.Panel();
            lblOrdersHeader = new System.Windows.Forms.Label();
            dgvSessionOrders = new System.Windows.Forms.DataGridView();
            pnlOrderAction = new System.Windows.Forms.Panel();
            btnOrderFood = new System.Windows.Forms.Button();
            lblOrderTotal = new System.Windows.Forms.Label();
            txtFoodNote = new System.Windows.Forms.TextBox();
            lblFoodNote = new System.Windows.Forms.Label();
            nudFoodQty = new System.Windows.Forms.NumericUpDown();
            lblFoodQty = new System.Windows.Forms.Label();
            lblSelectedFoodPrice = new System.Windows.Forms.Label();
            lblSelectedFoodName = new System.Windows.Forms.Label();
            tabChat = new System.Windows.Forms.TabPage();
            pnlChatHeader = new System.Windows.Forms.Panel();
            lblChatHeaderTitle = new System.Windows.Forms.Label();
            lblChatHeaderInfo = new System.Windows.Forms.Label();
            btnOpenVietQrTopUp = new System.Windows.Forms.Button();
            pnlChatContainer = new System.Windows.Forms.Panel();
            flpClientChat = new System.Windows.Forms.FlowLayoutPanel();
            pnlVietQrDrawer = new System.Windows.Forms.Panel();
            lblDrawerHeader = new System.Windows.Forms.Label();
            lblDrawerAmountPrompt = new System.Windows.Forms.Label();
            cboDrawerAmount = new System.Windows.Forms.ComboBox();
            picDrawerQr = new System.Windows.Forms.PictureBox();
            lblDrawerSyntax = new System.Windows.Forms.Label();
            btnCloseDrawer = new System.Windows.Forms.Button();
            pnlQuickChips = new System.Windows.Forms.Panel();
            lblQuickChipsPrompt = new System.Windows.Forms.Label();
            btnChipWater = new System.Windows.Forms.Button();
            btnChipDeposit = new System.Windows.Forms.Button();
            btnChipHardware = new System.Windows.Forms.Button();
            btnChipAC = new System.Windows.Forms.Button();
            btnChipFood = new System.Windows.Forms.Button();
            pnlChatInputBar = new System.Windows.Forms.Panel();
            txtClientChatMessage = new System.Windows.Forms.TextBox();
            btnSendClientChat = new System.Windows.Forms.Button();
            pnlLockOverlay = new System.Windows.Forms.Panel();
            lblUnlockError = new System.Windows.Forms.Label();
            btnUnlock = new System.Windows.Forms.Button();
            txtUnlockPassword = new System.Windows.Forms.TextBox();
            lblUnlockPasswordPrompt = new System.Windows.Forms.Label();
            lblLockSubtitle = new System.Windows.Forms.Label();
            lblLockTitle = new System.Windows.Forms.Label();
            lblLockIcon = new System.Windows.Forms.Label();
            tmrRealtime = new System.Windows.Forms.Timer();
            pnlTopHud.SuspendLayout();
            pnlStationInfo.SuspendLayout();
            pnlSessionTimers.SuspendLayout();
            pnlAccountActions.SuspendLayout();
            tabMain.SuspendLayout();
            tabGameHub.SuspendLayout();
            pnlGameTop.SuspendLayout();
            pnlActiveGame.SuspendLayout();
            tabFoodMenu.SuspendLayout();
            pnlFoodLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFoodItems).BeginInit();
            pnlFoodFilter.SuspendLayout();
            pnlFoodRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSessionOrders).BeginInit();
            pnlOrderAction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudFoodQty).BeginInit();
            pnlLockOverlay.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTopHud
            // 
            pnlTopHud.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            pnlTopHud.Controls.Add(pnlAccountActions);
            pnlTopHud.Controls.Add(pnlSessionTimers);
            pnlTopHud.Controls.Add(pnlStationInfo);
            pnlTopHud.Controls.Add(pnlHardwareHud);
            pnlTopHud.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTopHud.Location = new System.Drawing.Point(0, 0);
            pnlTopHud.Name = "pnlTopHud";
            pnlTopHud.Size = new System.Drawing.Size(1300, 158);
            pnlTopHud.TabIndex = 0;
            // 
            // pnlHardwareHud
            // 
            pnlHardwareHud.BackColor = System.Drawing.Color.FromArgb(12, 16, 24);
            pnlHardwareHud.Controls.Add(lblHardwareLatencyHud);
            pnlHardwareHud.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlHardwareHud.Location = new System.Drawing.Point(0, 125);
            pnlHardwareHud.Name = "pnlHardwareHud";
            pnlHardwareHud.Size = new System.Drawing.Size(1300, 33);
            pnlHardwareHud.TabIndex = 3;
            // 
            // lblHardwareLatencyHud
            // 
            lblHardwareLatencyHud.Dock = System.Windows.Forms.DockStyle.Fill;
            lblHardwareLatencyHud.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblHardwareLatencyHud.ForeColor = System.Drawing.Color.FromArgb(0, 220, 255);
            lblHardwareLatencyHud.Location = new System.Drawing.Point(0, 0);
            lblHardwareLatencyHud.Name = "lblHardwareLatencyHud";
            lblHardwareLatencyHud.Size = new System.Drawing.Size(1300, 33);
            lblHardwareLatencyHud.TabIndex = 0;
            lblHardwareLatencyHud.Text = "🖥️ i7-14700K | 32GB DDR5 | RTX 4070 Ti | 240Hz Fast-IPS | 🟢 Ping 12ms";
            lblHardwareLatencyHud.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStationInfo
            // 
            pnlStationInfo.BackColor = System.Drawing.Color.FromArgb(16, 21, 31);
            pnlStationInfo.Controls.Add(lblStationInfo);
            pnlStationInfo.Controls.Add(lblCenterBranding);
            pnlStationInfo.Dock = System.Windows.Forms.DockStyle.Left;
            pnlStationInfo.Location = new System.Drawing.Point(0, 0);
            pnlStationInfo.Name = "pnlStationInfo";
            pnlStationInfo.Size = new System.Drawing.Size(260, 125);
            pnlStationInfo.TabIndex = 0;
            // 
            // lblStationInfo
            // 
            lblStationInfo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblStationInfo.ForeColor = System.Drawing.Color.FromArgb(240, 245, 255);
            lblStationInfo.Location = new System.Drawing.Point(14, 45);
            lblStationInfo.Name = "lblStationInfo";
            lblStationInfo.Size = new System.Drawing.Size(235, 68);
            lblStationInfo.TabIndex = 1;
            lblStationInfo.Text = "🖥️ Máy: PC-02\nKhu: VIP (12,000đ/h)\nIP: 192.168.1.102";
            // 
            // lblCenterBranding
            // 
            lblCenterBranding.AutoSize = true;
            lblCenterBranding.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblCenterBranding.ForeColor = System.Drawing.Color.FromArgb(0, 210, 255);
            lblCenterBranding.Location = new System.Drawing.Point(12, 12);
            lblCenterBranding.Name = "lblCenterBranding";
            lblCenterBranding.Size = new System.Drawing.Size(230, 28);
            lblCenterBranding.TabIndex = 0;
            lblCenterBranding.Text = "⚡ CYBER CLIENT HUD";
            // 
            // pnlSessionTimers
            // 
            pnlSessionTimers.BackColor = System.Drawing.Color.FromArgb(24, 32, 47);
            pnlSessionTimers.Controls.Add(lblRemainingBalance);
            pnlSessionTimers.Controls.Add(lblServiceCost);
            pnlSessionTimers.Controls.Add(lblPlayCost);
            pnlSessionTimers.Controls.Add(lblRemainingTime);
            pnlSessionTimers.Controls.Add(lblPlayedTime);
            pnlSessionTimers.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlSessionTimers.Location = new System.Drawing.Point(260, 0);
            pnlSessionTimers.Name = "pnlSessionTimers";
            pnlSessionTimers.Size = new System.Drawing.Size(560, 125);
            pnlSessionTimers.TabIndex = 1;
            // 
            // lblRemainingBalance
            // 
            lblRemainingBalance.AutoSize = true;
            lblRemainingBalance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblRemainingBalance.ForeColor = System.Drawing.Color.FromArgb(170, 200, 230);
            lblRemainingBalance.Location = new System.Drawing.Point(20, 92);
            lblRemainingBalance.Name = "lblRemainingBalance";
            lblRemainingBalance.Size = new System.Drawing.Size(262, 20);
            lblRemainingBalance.TabIndex = 4;
            lblRemainingBalance.Text = "💵 Số dư khả dụng tạm tính: 0 VNĐ";
            // 
            // lblServiceCost
            // 
            lblServiceCost.AutoSize = true;
            lblServiceCost.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblServiceCost.ForeColor = System.Drawing.Color.FromArgb(255, 170, 50);
            lblServiceCost.Location = new System.Drawing.Point(290, 55);
            lblServiceCost.Name = "lblServiceCost";
            lblServiceCost.Size = new System.Drawing.Size(206, 23);
            lblServiceCost.TabIndex = 3;
            lblServiceCost.Text = "🥤 Dịch vụ F&B: 0 VNĐ";
            // 
            // lblPlayCost
            // 
            lblPlayCost.AutoSize = true;
            lblPlayCost.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblPlayCost.ForeColor = System.Drawing.Color.FromArgb(230, 240, 255);
            lblPlayCost.Location = new System.Drawing.Point(290, 16);
            lblPlayCost.Name = "lblPlayCost";
            lblPlayCost.Size = new System.Drawing.Size(186, 23);
            lblPlayCost.TabIndex = 2;
            lblPlayCost.Text = "💰 Tiền giờ: 0 VNĐ";
            // 
            // lblRemainingTime
            // 
            lblRemainingTime.AutoSize = true;
            lblRemainingTime.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblRemainingTime.ForeColor = System.Drawing.Color.FromArgb(0, 220, 255);
            lblRemainingTime.Location = new System.Drawing.Point(18, 51);
            lblRemainingTime.Name = "lblRemainingTime";
            lblRemainingTime.Size = new System.Drawing.Size(208, 28);
            lblRemainingTime.TabIndex = 1;
            lblRemainingTime.Text = "⏳ Còn lại: 00:00:00";
            // 
            // lblPlayedTime
            // 
            lblPlayedTime.AutoSize = true;
            lblPlayedTime.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblPlayedTime.ForeColor = System.Drawing.Color.FromArgb(255, 215, 0);
            lblPlayedTime.Location = new System.Drawing.Point(18, 12);
            lblPlayedTime.Name = "lblPlayedTime";
            lblPlayedTime.Size = new System.Drawing.Size(212, 28);
            lblPlayedTime.TabIndex = 0;
            lblPlayedTime.Text = "⏱️ Đã chơi: 00:00:00";
            // 
            // pnlAccountActions
            // 
            pnlAccountActions.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            pnlAccountActions.Controls.Add(btnLogoutSession);
            pnlAccountActions.Controls.Add(btnCallStaff);
            pnlAccountActions.Controls.Add(btnChangePassword);
            pnlAccountActions.Controls.Add(btnLockScreen);
            pnlAccountActions.Controls.Add(lblWalletBalance);
            pnlAccountActions.Controls.Add(lblMemberTier);
            pnlAccountActions.Controls.Add(lblAccountInfo);
            pnlAccountActions.Dock = System.Windows.Forms.DockStyle.Right;
            pnlAccountActions.Location = new System.Drawing.Point(820, 0);
            pnlAccountActions.Name = "pnlAccountActions";
            pnlAccountActions.Size = new System.Drawing.Size(480, 125);
            pnlAccountActions.TabIndex = 2;
            // 
            // btnLogoutSession
            // 
            btnLogoutSession.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            btnLogoutSession.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLogoutSession.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnLogoutSession.ForeColor = System.Drawing.Color.White;
            btnLogoutSession.Location = new System.Drawing.Point(348, 72);
            btnLogoutSession.Name = "btnLogoutSession";
            btnLogoutSession.Size = new System.Drawing.Size(120, 42);
            btnLogoutSession.TabIndex = 6;
            btnLogoutSession.Text = "🚪 Đăng xuất";
            btnLogoutSession.UseVisualStyleBackColor = false;
            // 
            // btnCallStaff
            // 
            btnCallStaff.BackColor = System.Drawing.Color.FromArgb(217, 119, 6);
            btnCallStaff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCallStaff.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnCallStaff.ForeColor = System.Drawing.Color.White;
            btnCallStaff.Location = new System.Drawing.Point(232, 72);
            btnCallStaff.Name = "btnCallStaff";
            btnCallStaff.Size = new System.Drawing.Size(110, 42);
            btnCallStaff.TabIndex = 5;
            btnCallStaff.Text = "🔔 Gọi quầy";
            btnCallStaff.UseVisualStyleBackColor = false;
            // 
            // btnChangePassword
            // 
            btnChangePassword.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnChangePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChangePassword.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnChangePassword.ForeColor = System.Drawing.Color.White;
            btnChangePassword.Location = new System.Drawing.Point(116, 72);
            btnChangePassword.Name = "btnChangePassword";
            btnChangePassword.Size = new System.Drawing.Size(110, 42);
            btnChangePassword.TabIndex = 4;
            btnChangePassword.Text = "🔑 Mật khẩu";
            btnChangePassword.UseVisualStyleBackColor = false;
            // 
            // btnLockScreen
            // 
            btnLockScreen.BackColor = System.Drawing.Color.FromArgb(71, 85, 105);
            btnLockScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLockScreen.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnLockScreen.ForeColor = System.Drawing.Color.White;
            btnLockScreen.Location = new System.Drawing.Point(12, 72);
            btnLockScreen.Name = "btnLockScreen";
            btnLockScreen.Size = new System.Drawing.Size(98, 42);
            btnLockScreen.TabIndex = 3;
            btnLockScreen.Text = "🔒 Khóa máy";
            btnLockScreen.UseVisualStyleBackColor = false;
            // 
            // lblWalletBalance
            // 
            lblWalletBalance.AutoSize = true;
            lblWalletBalance.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            lblWalletBalance.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            lblWalletBalance.Location = new System.Drawing.Point(12, 38);
            lblWalletBalance.Name = "lblWalletBalance";
            lblWalletBalance.Size = new System.Drawing.Size(183, 25);
            lblWalletBalance.TabIndex = 2;
            lblWalletBalance.Text = "💵 Ví: 120,000 VNĐ";
            // 
            // lblMemberTier
            // 
            lblMemberTier.AutoSize = true;
            lblMemberTier.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblMemberTier.ForeColor = System.Drawing.Color.FromArgb(255, 215, 0);
            lblMemberTier.Location = new System.Drawing.Point(310, 12);
            lblMemberTier.Name = "lblMemberTier";
            lblMemberTier.Size = new System.Drawing.Size(147, 21);
            lblMemberTier.TabIndex = 1;
            lblMemberTier.Text = "🥇 Vàng (Giảm 15%)";
            // 
            // lblAccountInfo
            // 
            lblAccountInfo.AutoSize = true;
            lblAccountInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblAccountInfo.ForeColor = System.Drawing.Color.White;
            lblAccountInfo.Location = new System.Drawing.Point(12, 12);
            lblAccountInfo.Name = "lblAccountInfo";
            lblAccountInfo.Size = new System.Drawing.Size(217, 23);
            lblAccountInfo.TabIndex = 0;
            lblAccountInfo.Text = "👤 gamer01 - Nguyễn Văn A";
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabGameHub);
            tabMain.Controls.Add(tabFoodMenu);
            tabMain.Controls.Add(tabChat);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            tabMain.Location = new System.Drawing.Point(0, 158);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new System.Drawing.Size(1300, 692);
            tabMain.TabIndex = 1;
            // 
            // tabGameHub
            // 
            tabGameHub.BackColor = System.Drawing.Color.FromArgb(15, 20, 28);
            tabGameHub.Controls.Add(flpGameCards);
            tabGameHub.Controls.Add(pnlActiveGame);
            tabGameHub.Controls.Add(pnlGameTop);
            tabGameHub.Location = new System.Drawing.Point(4, 32);
            tabGameHub.Name = "tabGameHub";
            tabGameHub.Padding = new System.Windows.Forms.Padding(3);
            tabGameHub.Size = new System.Drawing.Size(1292, 689);
            tabGameHub.TabIndex = 0;
            tabGameHub.Text = "🎮 KHO GAME (GAME HUB)";
            // 
            // pnlGameTop
            // 
            pnlGameTop.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            pnlGameTop.Controls.Add(lblLauncherStatus);
            pnlGameTop.Controls.Add(txtSearchGame);
            pnlGameTop.Controls.Add(lblSearchGame);
            pnlGameTop.Controls.Add(btnCatSports);
            pnlGameTop.Controls.Add(btnCatRpg);
            pnlGameTop.Controls.Add(btnCatFps);
            pnlGameTop.Controls.Add(btnCatMoba);
            pnlGameTop.Controls.Add(btnCatAll);
            pnlGameTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlGameTop.Location = new System.Drawing.Point(3, 3);
            pnlGameTop.Name = "pnlGameTop";
            pnlGameTop.Size = new System.Drawing.Size(1286, 92);
            pnlGameTop.TabIndex = 0;
            // 
            // lblLauncherStatus
            // 
            lblLauncherStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            lblLauncherStatus.ForeColor = System.Drawing.Color.FromArgb(0, 210, 255);
            lblLauncherStatus.Location = new System.Drawing.Point(12, 58);
            lblLauncherStatus.Name = "lblLauncherStatus";
            lblLauncherStatus.Size = new System.Drawing.Size(1260, 26);
            lblLauncherStatus.TabIndex = 7;
            lblLauncherStatus.Text = "🚀 Trạng thái máy trạm: Kết nối mạng Gigabit LAN ổn định | Sẵn sàng leo rank!";
            // 
            // txtSearchGame
            // 
            txtSearchGame.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtSearchGame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtSearchGame.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            txtSearchGame.ForeColor = System.Drawing.Color.White;
            txtSearchGame.Location = new System.Drawing.Point(1010, 16);
            txtSearchGame.Name = "txtSearchGame";
            txtSearchGame.Size = new System.Drawing.Size(260, 29);
            txtSearchGame.TabIndex = 6;
            // 
            // lblSearchGame
            // 
            lblSearchGame.AutoSize = true;
            lblSearchGame.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblSearchGame.ForeColor = System.Drawing.Color.FromArgb(180, 200, 220);
            lblSearchGame.Location = new System.Drawing.Point(920, 21);
            lblSearchGame.Name = "lblSearchGame";
            lblSearchGame.Size = new System.Drawing.Size(86, 20);
            lblSearchGame.TabIndex = 5;
            lblSearchGame.Text = "🔍 Tìm kiếm:";
            // 
            // btnCatSports
            // 
            btnCatSports.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnCatSports.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCatSports.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCatSports.ForeColor = System.Drawing.Color.White;
            btnCatSports.Location = new System.Drawing.Point(636, 12);
            btnCatSports.Name = "btnCatSports";
            btnCatSports.Size = new System.Drawing.Size(140, 38);
            btnCatSports.TabIndex = 4;
            btnCatSports.Text = "⚽ Thể thao";
            btnCatSports.UseVisualStyleBackColor = false;
            // 
            // btnCatRpg
            // 
            btnCatRpg.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnCatRpg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCatRpg.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCatRpg.ForeColor = System.Drawing.Color.White;
            btnCatRpg.Location = new System.Drawing.Point(448, 12);
            btnCatRpg.Name = "btnCatRpg";
            btnCatRpg.Size = new System.Drawing.Size(182, 38);
            btnCatRpg.TabIndex = 3;
            btnCatRpg.Text = "🗡️ Sinh tồn / Nhập vai";
            btnCatRpg.UseVisualStyleBackColor = false;
            // 
            // btnCatFps
            // 
            btnCatFps.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnCatFps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCatFps.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCatFps.ForeColor = System.Drawing.Color.White;
            btnCatFps.Location = new System.Drawing.Point(302, 12);
            btnCatFps.Name = "btnCatFps";
            btnCatFps.Size = new System.Drawing.Size(140, 38);
            btnCatFps.TabIndex = 2;
            btnCatFps.Text = "🎯 Bắn súng FPS";
            btnCatFps.UseVisualStyleBackColor = false;
            // 
            // btnCatMoba
            // 
            btnCatMoba.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnCatMoba.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCatMoba.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCatMoba.ForeColor = System.Drawing.Color.White;
            btnCatMoba.Location = new System.Drawing.Point(146, 12);
            btnCatMoba.Name = "btnCatMoba";
            btnCatMoba.Size = new System.Drawing.Size(150, 38);
            btnCatMoba.TabIndex = 1;
            btnCatMoba.Text = "⚔️ Esports / MOBA";
            btnCatMoba.UseVisualStyleBackColor = false;
            // 
            // btnCatAll
            // 
            btnCatAll.BackColor = System.Drawing.Color.FromArgb(0, 180, 136);
            btnCatAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCatAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCatAll.ForeColor = System.Drawing.Color.White;
            btnCatAll.Location = new System.Drawing.Point(12, 12);
            btnCatAll.Name = "btnCatAll";
            btnCatAll.Size = new System.Drawing.Size(128, 38);
            btnCatAll.TabIndex = 0;
            btnCatAll.Text = "⭐ Tất cả game";
            btnCatAll.UseVisualStyleBackColor = false;
            // 
            // flpGameCards
            // 
            flpGameCards.AutoScroll = true;
            flpGameCards.BackColor = System.Drawing.Color.FromArgb(15, 20, 28);
            flpGameCards.Dock = System.Windows.Forms.DockStyle.Fill;
            flpGameCards.Location = new System.Drawing.Point(3, 95);
            flpGameCards.Name = "flpGameCards";
            flpGameCards.Padding = new System.Windows.Forms.Padding(12);
            flpGameCards.Size = new System.Drawing.Size(1286, 539);
            flpGameCards.TabIndex = 1;
            // 
            // pnlActiveGame
            // 
            pnlActiveGame.BackColor = System.Drawing.Color.FromArgb(24, 32, 47);
            pnlActiveGame.Controls.Add(btnStopGame);
            pnlActiveGame.Controls.Add(lblActiveGameTitle);
            pnlActiveGame.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlActiveGame.Location = new System.Drawing.Point(3, 634);
            pnlActiveGame.Name = "pnlActiveGame";
            pnlActiveGame.Size = new System.Drawing.Size(1286, 52);
            pnlActiveGame.TabIndex = 2;
            pnlActiveGame.Visible = false;
            // 
            // btnStopGame
            // 
            btnStopGame.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            btnStopGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnStopGame.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnStopGame.ForeColor = System.Drawing.Color.White;
            btnStopGame.Location = new System.Drawing.Point(1110, 8);
            btnStopGame.Name = "btnStopGame";
            btnStopGame.Size = new System.Drawing.Size(160, 36);
            btnStopGame.TabIndex = 1;
            btnStopGame.Text = "⏹️ ĐÓNG GAME";
            btnStopGame.UseVisualStyleBackColor = false;
            // 
            // lblActiveGameTitle
            // 
            lblActiveGameTitle.AutoSize = true;
            lblActiveGameTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            lblActiveGameTitle.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            lblActiveGameTitle.Location = new System.Drawing.Point(16, 14);
            lblActiveGameTitle.Name = "lblActiveGameTitle";
            lblActiveGameTitle.Size = new System.Drawing.Size(534, 25);
            lblActiveGameTitle.TabIndex = 0;
            lblActiveGameTitle.Text = "🎮 Đang chạy: Liên Minh Huyền Thoại (v14.22) - Tiến trình an toàn";
            // 
            // tabFoodMenu
            // 
            tabFoodMenu.BackColor = System.Drawing.Color.FromArgb(15, 20, 28);
            tabFoodMenu.Controls.Add(pnlFoodRight);
            tabFoodMenu.Controls.Add(pnlFoodLeft);
            tabFoodMenu.Location = new System.Drawing.Point(4, 32);
            tabFoodMenu.Name = "tabFoodMenu";
            tabFoodMenu.Padding = new System.Windows.Forms.Padding(3);
            tabFoodMenu.Size = new System.Drawing.Size(1292, 689);
            tabFoodMenu.TabIndex = 1;
            tabFoodMenu.Text = "🍔 ĐẶT MÓN F&B TẠI CHỖ";
            // 
            // pnlFoodLeft
            // 
            pnlFoodLeft.Controls.Add(dgvFoodItems);
            pnlFoodLeft.Controls.Add(pnlFoodFilter);
            pnlFoodLeft.Dock = System.Windows.Forms.DockStyle.Left;
            pnlFoodLeft.Location = new System.Drawing.Point(3, 3);
            pnlFoodLeft.Name = "pnlFoodLeft";
            pnlFoodLeft.Size = new System.Drawing.Size(760, 683);
            pnlFoodLeft.TabIndex = 0;
            // 
            // dgvFoodItems
            // 
            dgvFoodItems.AllowUserToAddRows = false;
            dgvFoodItems.AllowUserToDeleteRows = false;
            dgvFoodItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvFoodItems.BackgroundColor = System.Drawing.Color.FromArgb(20, 26, 38);
            dgvFoodItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dgvHeaderStyle.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            dgvHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dgvHeaderStyle.ForeColor = System.Drawing.Color.White;
            dgvFoodItems.ColumnHeadersDefaultCellStyle = dgvHeaderStyle;
            dgvFoodItems.ColumnHeadersHeight = 36;
            dgvRowStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dgvRowStyle.BackColor = System.Drawing.Color.FromArgb(24, 32, 47);
            dgvRowStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            dgvRowStyle.ForeColor = System.Drawing.Color.White;
            dgvFoodItems.DefaultCellStyle = dgvRowStyle;
            dgvFoodItems.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvFoodItems.EnableHeadersVisualStyles = false;
            dgvFoodItems.Location = new System.Drawing.Point(0, 56);
            dgvFoodItems.MultiSelect = false;
            dgvFoodItems.Name = "dgvFoodItems";
            dgvFoodItems.ReadOnly = true;
            dgvFoodItems.RowHeadersVisible = false;
            dgvFoodItems.RowTemplate.Height = 34;
            dgvFoodItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvFoodItems.Size = new System.Drawing.Size(760, 627);
            dgvFoodItems.TabIndex = 1;
            // 
            // pnlFoodFilter
            // 
            pnlFoodFilter.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            pnlFoodFilter.Controls.Add(txtSearchFood);
            pnlFoodFilter.Controls.Add(lblSearchFood);
            pnlFoodFilter.Controls.Add(btnFoodSnack);
            pnlFoodFilter.Controls.Add(btnFoodMeal);
            pnlFoodFilter.Controls.Add(btnFoodDrink);
            pnlFoodFilter.Controls.Add(btnFoodAll);
            pnlFoodFilter.Dock = System.Windows.Forms.DockStyle.Top;
            pnlFoodFilter.Location = new System.Drawing.Point(0, 0);
            pnlFoodFilter.Name = "pnlFoodFilter";
            pnlFoodFilter.Size = new System.Drawing.Size(760, 56);
            pnlFoodFilter.TabIndex = 0;
            // 
            // txtSearchFood
            // 
            txtSearchFood.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtSearchFood.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtSearchFood.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtSearchFood.ForeColor = System.Drawing.Color.White;
            txtSearchFood.Location = new System.Drawing.Point(542, 14);
            txtSearchFood.Name = "txtSearchFood";
            txtSearchFood.Size = new System.Drawing.Size(205, 27);
            txtSearchFood.TabIndex = 5;
            // 
            // lblSearchFood
            // 
            lblSearchFood.AutoSize = true;
            lblSearchFood.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblSearchFood.ForeColor = System.Drawing.Color.FromArgb(180, 200, 220);
            lblSearchFood.Location = new System.Drawing.Point(468, 17);
            lblSearchFood.Name = "lblSearchFood";
            lblSearchFood.Size = new System.Drawing.Size(71, 20);
            lblSearchFood.TabIndex = 4;
            lblSearchFood.Text = "🔍 Tìm:";
            // 
            // btnFoodSnack
            // 
            btnFoodSnack.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnFoodSnack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFoodSnack.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnFoodSnack.ForeColor = System.Drawing.Color.White;
            btnFoodSnack.Location = new System.Drawing.Point(344, 10);
            btnFoodSnack.Name = "btnFoodSnack";
            btnFoodSnack.Size = new System.Drawing.Size(108, 36);
            btnFoodSnack.TabIndex = 3;
            btnFoodSnack.Text = "🍿 Ăn vặt";
            btnFoodSnack.UseVisualStyleBackColor = false;
            // 
            // btnFoodMeal
            // 
            btnFoodMeal.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnFoodMeal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFoodMeal.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnFoodMeal.ForeColor = System.Drawing.Color.White;
            btnFoodMeal.Location = new System.Drawing.Point(230, 10);
            btnFoodMeal.Name = "btnFoodMeal";
            btnFoodMeal.Size = new System.Drawing.Size(108, 36);
            btnFoodMeal.TabIndex = 2;
            btnFoodMeal.Text = "🍜 Món ăn";
            btnFoodMeal.UseVisualStyleBackColor = false;
            // 
            // btnFoodDrink
            // 
            btnFoodDrink.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            btnFoodDrink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFoodDrink.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnFoodDrink.ForeColor = System.Drawing.Color.White;
            btnFoodDrink.Location = new System.Drawing.Point(116, 10);
            btnFoodDrink.Name = "btnFoodDrink";
            btnFoodDrink.Size = new System.Drawing.Size(108, 36);
            btnFoodDrink.TabIndex = 1;
            btnFoodDrink.Text = "🥤 Đồ uống";
            btnFoodDrink.UseVisualStyleBackColor = false;
            // 
            // btnFoodAll
            // 
            btnFoodAll.BackColor = System.Drawing.Color.FromArgb(0, 180, 136);
            btnFoodAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFoodAll.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnFoodAll.ForeColor = System.Drawing.Color.White;
            btnFoodAll.Location = new System.Drawing.Point(10, 10);
            btnFoodAll.Name = "btnFoodAll";
            btnFoodAll.Size = new System.Drawing.Size(100, 36);
            btnFoodAll.TabIndex = 0;
            btnFoodAll.Text = "⭐ Tất cả";
            btnFoodAll.UseVisualStyleBackColor = false;
            // 
            // pnlFoodRight
            // 
            pnlFoodRight.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            pnlFoodRight.Controls.Add(dgvSessionOrders);
            pnlFoodRight.Controls.Add(lblOrdersHeader);
            pnlFoodRight.Controls.Add(pnlOrderAction);
            pnlFoodRight.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlFoodRight.Location = new System.Drawing.Point(763, 3);
            pnlFoodRight.Name = "pnlFoodRight";
            pnlFoodRight.Padding = new System.Windows.Forms.Padding(12);
            pnlFoodRight.Size = new System.Drawing.Size(526, 683);
            pnlFoodRight.TabIndex = 1;
            // 
            // lblOrdersHeader
            // 
            lblOrdersHeader.AutoSize = true;
            lblOrdersHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblOrdersHeader.ForeColor = System.Drawing.Color.FromArgb(0, 210, 255);
            lblOrdersHeader.Location = new System.Drawing.Point(14, 345);
            lblOrdersHeader.Name = "lblOrdersHeader";
            lblOrdersHeader.Size = new System.Drawing.Size(325, 23);
            lblOrdersHeader.TabIndex = 1;
            lblOrdersHeader.Text = "📋 CÁC MÓN ĐÃ GỌI TRONG PHIÊN:";
            // 
            // dgvSessionOrders
            // 
            dgvSessionOrders.AllowUserToAddRows = false;
            dgvSessionOrders.AllowUserToDeleteRows = false;
            dgvSessionOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvSessionOrders.BackgroundColor = System.Drawing.Color.FromArgb(24, 32, 47);
            dgvSessionOrders.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvOrdersHeaderStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dgvOrdersHeaderStyle.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            dgvOrdersHeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvOrdersHeaderStyle.ForeColor = System.Drawing.Color.White;
            dgvSessionOrders.ColumnHeadersDefaultCellStyle = dgvOrdersHeaderStyle;
            dgvSessionOrders.ColumnHeadersHeight = 32;
            dgvOrdersRowStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dgvOrdersRowStyle.BackColor = System.Drawing.Color.FromArgb(20, 26, 38);
            dgvOrdersRowStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvOrdersRowStyle.ForeColor = System.Drawing.Color.White;
            dgvSessionOrders.DefaultCellStyle = dgvOrdersRowStyle;
            dgvSessionOrders.Dock = System.Windows.Forms.DockStyle.Bottom;
            dgvSessionOrders.EnableHeadersVisualStyles = false;
            dgvSessionOrders.Location = new System.Drawing.Point(12, 375);
            dgvSessionOrders.MultiSelect = false;
            dgvSessionOrders.Name = "dgvSessionOrders";
            dgvSessionOrders.ReadOnly = true;
            dgvSessionOrders.RowHeadersVisible = false;
            dgvSessionOrders.RowTemplate.Height = 30;
            dgvSessionOrders.Size = new System.Drawing.Size(502, 296);
            dgvSessionOrders.TabIndex = 2;
            // 
            // pnlOrderAction
            // 
            pnlOrderAction.BackColor = System.Drawing.Color.FromArgb(24, 32, 47);
            pnlOrderAction.Controls.Add(btnOrderFood);
            pnlOrderAction.Controls.Add(lblOrderTotal);
            pnlOrderAction.Controls.Add(txtFoodNote);
            pnlOrderAction.Controls.Add(lblFoodNote);
            pnlOrderAction.Controls.Add(nudFoodQty);
            pnlOrderAction.Controls.Add(lblFoodQty);
            pnlOrderAction.Controls.Add(lblSelectedFoodPrice);
            pnlOrderAction.Controls.Add(lblSelectedFoodName);
            pnlOrderAction.Dock = System.Windows.Forms.DockStyle.Top;
            pnlOrderAction.Location = new System.Drawing.Point(12, 12);
            pnlOrderAction.Name = "pnlOrderAction";
            pnlOrderAction.Size = new System.Drawing.Size(502, 320);
            pnlOrderAction.TabIndex = 0;
            // 
            // btnOrderFood
            // 
            btnOrderFood.BackColor = System.Drawing.Color.FromArgb(245, 158, 11);
            btnOrderFood.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOrderFood.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnOrderFood.ForeColor = System.Drawing.Color.White;
            btnOrderFood.Location = new System.Drawing.Point(18, 256);
            btnOrderFood.Name = "btnOrderFood";
            btnOrderFood.Size = new System.Drawing.Size(465, 48);
            btnOrderFood.TabIndex = 7;
            btnOrderFood.Text = "🛒 GỌI MÓN (ĐƯA VÀO PHIÊN CHƠI)";
            btnOrderFood.UseVisualStyleBackColor = false;
            // 
            // lblOrderTotal
            // 
            lblOrderTotal.AutoSize = true;
            lblOrderTotal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblOrderTotal.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            lblOrderTotal.Location = new System.Drawing.Point(16, 215);
            lblOrderTotal.Name = "lblOrderTotal";
            lblOrderTotal.Size = new System.Drawing.Size(261, 28);
            lblOrderTotal.TabIndex = 6;
            lblOrderTotal.Text = "TỔNG TIỀN TÍNH: 0 VNĐ";
            // 
            // txtFoodNote
            // 
            txtFoodNote.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtFoodNote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtFoodNote.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            txtFoodNote.ForeColor = System.Drawing.Color.White;
            txtFoodNote.Location = new System.Drawing.Point(18, 172);
            txtFoodNote.Name = "txtFoodNote";
            txtFoodNote.Size = new System.Drawing.Size(465, 29);
            txtFoodNote.TabIndex = 5;
            // 
            // lblFoodNote
            // 
            lblFoodNote.AutoSize = true;
            lblFoodNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblFoodNote.ForeColor = System.Drawing.Color.White;
            lblFoodNote.Location = new System.Drawing.Point(16, 146);
            lblFoodNote.Name = "lblFoodNote";
            lblFoodNote.Size = new System.Drawing.Size(315, 20);
            lblFoodNote.TabIndex = 4;
            lblFoodNote.Text = "Ghi chú chế biến (ít đá, trứng chín, v.v.):";
            // 
            // nudFoodQty
            // 
            nudFoodQty.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            nudFoodQty.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            nudFoodQty.ForeColor = System.Drawing.Color.White;
            nudFoodQty.Location = new System.Drawing.Point(18, 106);
            nudFoodQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudFoodQty.Name = "nudFoodQty";
            nudFoodQty.Size = new System.Drawing.Size(120, 30);
            nudFoodQty.TabIndex = 3;
            nudFoodQty.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblFoodQty
            // 
            lblFoodQty.AutoSize = true;
            lblFoodQty.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblFoodQty.ForeColor = System.Drawing.Color.White;
            lblFoodQty.Location = new System.Drawing.Point(16, 80);
            lblFoodQty.Name = "lblFoodQty";
            lblFoodQty.Size = new System.Drawing.Size(109, 20);
            lblFoodQty.TabIndex = 2;
            lblFoodQty.Text = "Số lượng gọi:";
            // 
            // lblSelectedFoodPrice
            // 
            lblSelectedFoodPrice.AutoSize = true;
            lblSelectedFoodPrice.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblSelectedFoodPrice.ForeColor = System.Drawing.Color.FromArgb(255, 215, 0);
            lblSelectedFoodPrice.Location = new System.Drawing.Point(16, 46);
            lblSelectedFoodPrice.Name = "lblSelectedFoodPrice";
            lblSelectedFoodPrice.Size = new System.Drawing.Size(248, 23);
            lblSelectedFoodPrice.TabIndex = 1;
            lblSelectedFoodPrice.Text = "Đơn giá: 0 VNĐ | Còn kho: --";
            // 
            // lblSelectedFoodName
            // 
            lblSelectedFoodName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblSelectedFoodName.ForeColor = System.Drawing.Color.White;
            lblSelectedFoodName.Location = new System.Drawing.Point(16, 12);
            lblSelectedFoodName.Name = "lblSelectedFoodName";
            lblSelectedFoodName.Size = new System.Drawing.Size(467, 30);
            lblSelectedFoodName.TabIndex = 0;
            lblSelectedFoodName.Text = "Vui lòng chọn món từ menu bên trái";
            // 
            // tabChat
            // 
            tabChat.BackColor = System.Drawing.Color.FromArgb(15, 20, 28);
            tabChat.Controls.Add(pnlChatContainer);
            tabChat.Controls.Add(pnlVietQrDrawer);
            tabChat.Controls.Add(pnlQuickChips);
            tabChat.Controls.Add(pnlChatInputBar);
            tabChat.Controls.Add(pnlChatHeader);
            tabChat.Location = new System.Drawing.Point(4, 32);
            tabChat.Name = "tabChat";
            tabChat.Padding = new System.Windows.Forms.Padding(3);
            tabChat.Size = new System.Drawing.Size(1292, 656);
            tabChat.TabIndex = 2;
            tabChat.Text = "💬 HỖ TRỢ & CHAT QUẦY";
            // 
            // pnlChatHeader
            // 
            pnlChatHeader.BackColor = System.Drawing.Color.FromArgb(18, 24, 38);
            pnlChatHeader.Controls.Add(btnOpenVietQrTopUp);
            pnlChatHeader.Controls.Add(lblChatHeaderInfo);
            pnlChatHeader.Controls.Add(lblChatHeaderTitle);
            pnlChatHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlChatHeader.Location = new System.Drawing.Point(3, 3);
            pnlChatHeader.Name = "pnlChatHeader";
            pnlChatHeader.Size = new System.Drawing.Size(1286, 50);
            pnlChatHeader.TabIndex = 0;
            // 
            // lblChatHeaderTitle
            // 
            lblChatHeaderTitle.AutoSize = true;
            lblChatHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblChatHeaderTitle.ForeColor = System.Drawing.Color.FromArgb(0, 210, 255);
            lblChatHeaderTitle.Location = new System.Drawing.Point(12, 12);
            lblChatHeaderTitle.Name = "lblChatHeaderTitle";
            lblChatHeaderTitle.Size = new System.Drawing.Size(435, 25);
            lblChatHeaderTitle.TabIndex = 0;
            lblChatHeaderTitle.Text = "💬 TRÒ CHUYỆN TRỰC TIẾP VỚI THU NGÂN QUẦY";
            // 
            // lblChatHeaderInfo
            // 
            lblChatHeaderInfo.AutoSize = true;
            lblChatHeaderInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblChatHeaderInfo.ForeColor = System.Drawing.Color.FromArgb(160, 220, 180);
            lblChatHeaderInfo.Location = new System.Drawing.Point(460, 16);
            lblChatHeaderInfo.Name = "lblChatHeaderInfo";
            lblChatHeaderInfo.Size = new System.Drawing.Size(340, 20);
            lblChatHeaderInfo.TabIndex = 1;
            lblChatHeaderInfo.Text = "🟢 Thu ngân đang trực tuyến | Phản hồi siêu tốc 24/7";
            // 
            // btnOpenVietQrTopUp
            // 
            btnOpenVietQrTopUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnOpenVietQrTopUp.BackColor = System.Drawing.Color.FromArgb(13, 148, 136);
            btnOpenVietQrTopUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenVietQrTopUp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnOpenVietQrTopUp.ForeColor = System.Drawing.Color.White;
            btnOpenVietQrTopUp.Location = new System.Drawing.Point(1070, 8);
            btnOpenVietQrTopUp.Name = "btnOpenVietQrTopUp";
            btnOpenVietQrTopUp.Size = new System.Drawing.Size(205, 34);
            btnOpenVietQrTopUp.TabIndex = 2;
            btnOpenVietQrTopUp.Text = "⚡ NẠP VÍ TỰ ĐỘNG VIETQR";
            btnOpenVietQrTopUp.UseVisualStyleBackColor = false;
            // 
            // pnlVietQrDrawer
            // 
            pnlVietQrDrawer.BackColor = System.Drawing.Color.FromArgb(20, 26, 40);
            pnlVietQrDrawer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlVietQrDrawer.Controls.Add(btnCloseDrawer);
            pnlVietQrDrawer.Controls.Add(lblDrawerSyntax);
            pnlVietQrDrawer.Controls.Add(picDrawerQr);
            pnlVietQrDrawer.Controls.Add(cboDrawerAmount);
            pnlVietQrDrawer.Controls.Add(lblDrawerAmountPrompt);
            pnlVietQrDrawer.Controls.Add(lblDrawerHeader);
            pnlVietQrDrawer.Dock = System.Windows.Forms.DockStyle.Right;
            pnlVietQrDrawer.Location = new System.Drawing.Point(976, 53);
            pnlVietQrDrawer.Name = "pnlVietQrDrawer";
            pnlVietQrDrawer.Padding = new System.Windows.Forms.Padding(10);
            pnlVietQrDrawer.Size = new System.Drawing.Size(310, 507);
            pnlVietQrDrawer.TabIndex = 3;
            pnlVietQrDrawer.Visible = false;
            // 
            // lblDrawerHeader
            // 
            lblDrawerHeader.Dock = System.Windows.Forms.DockStyle.Top;
            lblDrawerHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblDrawerHeader.ForeColor = System.Drawing.Color.FromArgb(0, 220, 255);
            lblDrawerHeader.Location = new System.Drawing.Point(10, 10);
            lblDrawerHeader.Name = "lblDrawerHeader";
            lblDrawerHeader.Size = new System.Drawing.Size(288, 25);
            lblDrawerHeader.TabIndex = 0;
            lblDrawerHeader.Text = "⚡ NẠP VÍ NHANH VIETQR";
            lblDrawerHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDrawerAmountPrompt
            // 
            lblDrawerAmountPrompt.AutoSize = true;
            lblDrawerAmountPrompt.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblDrawerAmountPrompt.ForeColor = System.Drawing.Color.White;
            lblDrawerAmountPrompt.Location = new System.Drawing.Point(12, 42);
            lblDrawerAmountPrompt.Name = "lblDrawerAmountPrompt";
            lblDrawerAmountPrompt.Size = new System.Drawing.Size(107, 19);
            lblDrawerAmountPrompt.TabIndex = 1;
            lblDrawerAmountPrompt.Text = "Chọn số tiền:";
            // 
            // cboDrawerAmount
            // 
            cboDrawerAmount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDrawerAmount.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            cboDrawerAmount.FormattingEnabled = true;
            cboDrawerAmount.Items.AddRange(new object[] { "20,000 VNĐ", "50,000 VNĐ", "100,000 VNĐ", "200,000 VNĐ", "500,000 VNĐ" });
            cboDrawerAmount.Location = new System.Drawing.Point(120, 38);
            cboDrawerAmount.Name = "cboDrawerAmount";
            cboDrawerAmount.Size = new System.Drawing.Size(160, 29);
            cboDrawerAmount.TabIndex = 2;
            // 
            // picDrawerQr
            // 
            picDrawerQr.BackColor = System.Drawing.Color.White;
            picDrawerQr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picDrawerQr.Location = new System.Drawing.Point(35, 78);
            picDrawerQr.Name = "picDrawerQr";
            picDrawerQr.Size = new System.Drawing.Size(240, 240);
            picDrawerQr.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picDrawerQr.TabIndex = 3;
            picDrawerQr.TabStop = false;
            // 
            // lblDrawerSyntax
            // 
            lblDrawerSyntax.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblDrawerSyntax.ForeColor = System.Drawing.Color.FromArgb(255, 215, 0);
            lblDrawerSyntax.Location = new System.Drawing.Point(10, 325);
            lblDrawerSyntax.Name = "lblDrawerSyntax";
            lblDrawerSyntax.Size = new System.Drawing.Size(288, 40);
            lblDrawerSyntax.TabIndex = 4;
            lblDrawerSyntax.Text = "Cú pháp: NAP PC02 GAMER01";
            lblDrawerSyntax.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCloseDrawer
            // 
            btnCloseDrawer.BackColor = System.Drawing.Color.FromArgb(50, 60, 80);
            btnCloseDrawer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCloseDrawer.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnCloseDrawer.ForeColor = System.Drawing.Color.White;
            btnCloseDrawer.Location = new System.Drawing.Point(85, 372);
            btnCloseDrawer.Name = "btnCloseDrawer";
            btnCloseDrawer.Size = new System.Drawing.Size(140, 32);
            btnCloseDrawer.TabIndex = 5;
            btnCloseDrawer.Text = "Đóng Nạp Ví";
            btnCloseDrawer.UseVisualStyleBackColor = false;
            // 
            // pnlChatContainer
            // 
            pnlChatContainer.BackColor = System.Drawing.Color.FromArgb(14, 18, 26);
            pnlChatContainer.Controls.Add(flpClientChat);
            pnlChatContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlChatContainer.Location = new System.Drawing.Point(3, 53);
            pnlChatContainer.Name = "pnlChatContainer";
            pnlChatContainer.Padding = new System.Windows.Forms.Padding(8);
            pnlChatContainer.Size = new System.Drawing.Size(973, 507);
            pnlChatContainer.TabIndex = 4;
            // 
            // flpClientChat
            // 
            flpClientChat.AutoScroll = true;
            flpClientChat.BackColor = System.Drawing.Color.FromArgb(16, 22, 34);
            flpClientChat.Dock = System.Windows.Forms.DockStyle.Fill;
            flpClientChat.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flpClientChat.Location = new System.Drawing.Point(8, 8);
            flpClientChat.Name = "flpClientChat";
            flpClientChat.Padding = new System.Windows.Forms.Padding(10);
            flpClientChat.Size = new System.Drawing.Size(957, 491);
            flpClientChat.TabIndex = 0;
            flpClientChat.WrapContents = false;
            // 
            // pnlQuickChips
            // 
            pnlQuickChips.BackColor = System.Drawing.Color.FromArgb(22, 28, 42);
            pnlQuickChips.Controls.Add(btnChipFood);
            pnlQuickChips.Controls.Add(btnChipAC);
            pnlQuickChips.Controls.Add(btnChipHardware);
            pnlQuickChips.Controls.Add(btnChipDeposit);
            pnlQuickChips.Controls.Add(btnChipWater);
            pnlQuickChips.Controls.Add(lblQuickChipsPrompt);
            pnlQuickChips.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlQuickChips.Location = new System.Drawing.Point(3, 560);
            pnlQuickChips.Name = "pnlQuickChips";
            pnlQuickChips.Size = new System.Drawing.Size(1286, 44);
            pnlQuickChips.TabIndex = 1;
            // 
            // lblQuickChipsPrompt
            // 
            lblQuickChipsPrompt.AutoSize = true;
            lblQuickChipsPrompt.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblQuickChipsPrompt.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            lblQuickChipsPrompt.Location = new System.Drawing.Point(12, 13);
            lblQuickChipsPrompt.Name = "lblQuickChipsPrompt";
            lblQuickChipsPrompt.Size = new System.Drawing.Size(107, 19);
            lblQuickChipsPrompt.TabIndex = 0;
            lblQuickChipsPrompt.Text = "⚡ Chọn nhanh:";
            // 
            // btnChipWater
            // 
            btnChipWater.BackColor = System.Drawing.Color.FromArgb(34, 46, 68);
            btnChipWater.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChipWater.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnChipWater.ForeColor = System.Drawing.Color.White;
            btnChipWater.Location = new System.Drawing.Point(125, 6);
            btnChipWater.Name = "btnChipWater";
            btnChipWater.Size = new System.Drawing.Size(165, 32);
            btnChipWater.TabIndex = 1;
            btnChipWater.Text = "Xin thêm nước lọc/đá";
            btnChipWater.UseVisualStyleBackColor = false;
            // 
            // btnChipDeposit
            // 
            btnChipDeposit.BackColor = System.Drawing.Color.FromArgb(34, 46, 68);
            btnChipDeposit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChipDeposit.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnChipDeposit.ForeColor = System.Drawing.Color.White;
            btnChipDeposit.Location = new System.Drawing.Point(298, 6);
            btnChipDeposit.Name = "btnChipDeposit";
            btnChipDeposit.Size = new System.Drawing.Size(135, 32);
            btnChipDeposit.TabIndex = 2;
            btnChipDeposit.Text = "Nạp 50k ví";
            btnChipDeposit.UseVisualStyleBackColor = false;
            // 
            // btnChipHardware
            // 
            btnChipHardware.BackColor = System.Drawing.Color.FromArgb(34, 46, 68);
            btnChipHardware.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChipHardware.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnChipHardware.ForeColor = System.Drawing.Color.White;
            btnChipHardware.Location = new System.Drawing.Point(441, 6);
            btnChipHardware.Name = "btnChipHardware";
            btnChipHardware.Size = new System.Drawing.Size(145, 32);
            btnChipHardware.TabIndex = 3;
            btnChipHardware.Text = "Kẹt phím/chuột";
            btnChipHardware.UseVisualStyleBackColor = false;
            // 
            // btnChipAC
            // 
            btnChipAC.BackColor = System.Drawing.Color.FromArgb(34, 46, 68);
            btnChipAC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChipAC.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnChipAC.ForeColor = System.Drawing.Color.White;
            btnChipAC.Location = new System.Drawing.Point(594, 6);
            btnChipAC.Name = "btnChipAC";
            btnChipAC.Size = new System.Drawing.Size(145, 32);
            btnChipAC.TabIndex = 4;
            btnChipAC.Text = "Chỉnh điều hòa";
            btnChipAC.UseVisualStyleBackColor = false;
            // 
            // btnChipFood
            // 
            btnChipFood.BackColor = System.Drawing.Color.FromArgb(34, 46, 68);
            btnChipFood.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChipFood.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            btnChipFood.ForeColor = System.Drawing.Color.White;
            btnChipFood.Location = new System.Drawing.Point(747, 6);
            btnChipFood.Name = "btnChipFood";
            btnChipFood.Size = new System.Drawing.Size(175, 32);
            btnChipFood.TabIndex = 5;
            btnChipFood.Text = "Bao lâu nữa có đồ ăn ạ?";
            btnChipFood.UseVisualStyleBackColor = false;
            // 
            // pnlChatInputBar
            // 
            pnlChatInputBar.BackColor = System.Drawing.Color.FromArgb(18, 24, 38);
            pnlChatInputBar.Controls.Add(btnSendClientChat);
            pnlChatInputBar.Controls.Add(txtClientChatMessage);
            pnlChatInputBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlChatInputBar.Location = new System.Drawing.Point(3, 604);
            pnlChatInputBar.Name = "pnlChatInputBar";
            pnlChatInputBar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            pnlChatInputBar.Size = new System.Drawing.Size(1286, 49);
            pnlChatInputBar.TabIndex = 2;
            // 
            // txtClientChatMessage
            // 
            txtClientChatMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtClientChatMessage.BackColor = System.Drawing.Color.FromArgb(30, 38, 56);
            txtClientChatMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtClientChatMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtClientChatMessage.ForeColor = System.Drawing.Color.White;
            txtClientChatMessage.Location = new System.Drawing.Point(12, 10);
            txtClientChatMessage.Name = "txtClientChatMessage";
            txtClientChatMessage.PlaceholderText = "Nhập tin nhắn gửi tới thu ngân quầy (Nhấn Enter để gửi)...";
            txtClientChatMessage.Size = new System.Drawing.Size(1140, 30);
            txtClientChatMessage.TabIndex = 0;
            // 
            // btnSendClientChat
            // 
            btnSendClientChat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnSendClientChat.BackColor = System.Drawing.Color.FromArgb(0, 180, 136);
            btnSendClientChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSendClientChat.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnSendClientChat.ForeColor = System.Drawing.Color.White;
            btnSendClientChat.Location = new System.Drawing.Point(1162, 8);
            btnSendClientChat.Name = "btnSendClientChat";
            btnSendClientChat.Size = new System.Drawing.Size(110, 34);
            btnSendClientChat.TabIndex = 1;
            btnSendClientChat.Text = "Gửi 📤";
            btnSendClientChat.UseVisualStyleBackColor = false;
            // 
            // pnlLockOverlay
            // 
            pnlLockOverlay.BackColor = System.Drawing.Color.FromArgb(12, 16, 24);
            pnlLockOverlay.Controls.Add(lblUnlockError);
            pnlLockOverlay.Controls.Add(btnUnlock);
            pnlLockOverlay.Controls.Add(txtUnlockPassword);
            pnlLockOverlay.Controls.Add(lblUnlockPasswordPrompt);
            pnlLockOverlay.Controls.Add(lblLockSubtitle);
            pnlLockOverlay.Controls.Add(lblLockTitle);
            pnlLockOverlay.Controls.Add(lblLockIcon);
            pnlLockOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLockOverlay.Location = new System.Drawing.Point(0, 0);
            pnlLockOverlay.Name = "pnlLockOverlay";
            pnlLockOverlay.Size = new System.Drawing.Size(1300, 850);
            pnlLockOverlay.TabIndex = 10;
            pnlLockOverlay.Visible = false;
            // 
            // lblUnlockError
            // 
            lblUnlockError.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblUnlockError.ForeColor = System.Drawing.Color.FromArgb(255, 99, 99);
            lblUnlockError.Location = new System.Drawing.Point(400, 520);
            lblUnlockError.Name = "lblUnlockError";
            lblUnlockError.Size = new System.Drawing.Size(500, 30);
            lblUnlockError.TabIndex = 6;
            lblUnlockError.Text = "Mật khẩu không chính xác!";
            lblUnlockError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblUnlockError.Visible = false;
            // 
            // btnUnlock
            // 
            btnUnlock.BackColor = System.Drawing.Color.FromArgb(0, 180, 136);
            btnUnlock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnUnlock.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnUnlock.ForeColor = System.Drawing.Color.White;
            btnUnlock.Location = new System.Drawing.Point(520, 460);
            btnUnlock.Name = "btnUnlock";
            btnUnlock.Size = new System.Drawing.Size(260, 48);
            btnUnlock.TabIndex = 5;
            btnUnlock.Text = "🔓 MỞ KHÓA MÁY";
            btnUnlock.UseVisualStyleBackColor = false;
            // 
            // txtUnlockPassword
            // 
            txtUnlockPassword.BackColor = System.Drawing.Color.FromArgb(30, 38, 52);
            txtUnlockPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtUnlockPassword.Font = new System.Drawing.Font("Segoe UI", 12F);
            txtUnlockPassword.ForeColor = System.Drawing.Color.White;
            txtUnlockPassword.Location = new System.Drawing.Point(450, 405);
            txtUnlockPassword.Name = "txtUnlockPassword";
            txtUnlockPassword.Size = new System.Drawing.Size(400, 34);
            txtUnlockPassword.TabIndex = 4;
            txtUnlockPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtUnlockPassword.UseSystemPasswordChar = true;
            // 
            // lblUnlockPasswordPrompt
            // 
            lblUnlockPasswordPrompt.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            lblUnlockPasswordPrompt.ForeColor = System.Drawing.Color.White;
            lblUnlockPasswordPrompt.Location = new System.Drawing.Point(400, 370);
            lblUnlockPasswordPrompt.Name = "lblUnlockPasswordPrompt";
            lblUnlockPasswordPrompt.Size = new System.Drawing.Size(500, 25);
            lblUnlockPasswordPrompt.TabIndex = 3;
            lblUnlockPasswordPrompt.Text = "Nhập mật khẩu tài khoản để mở khóa:";
            lblUnlockPasswordPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLockSubtitle
            // 
            lblLockSubtitle.Font = new System.Drawing.Font("Segoe UI", 11F);
            lblLockSubtitle.ForeColor = System.Drawing.Color.FromArgb(170, 200, 230);
            lblLockSubtitle.Location = new System.Drawing.Point(250, 300);
            lblLockSubtitle.Name = "lblLockSubtitle";
            lblLockSubtitle.Size = new System.Drawing.Size(800, 50);
            lblLockSubtitle.TabIndex = 2;
            lblLockSubtitle.Text = "Thời gian phiên chơi vẫn đang chạy và bảo lưu dữ liệu an toàn.\nNgười khác không thể truy cập tài khoản và máy của bạn.";
            lblLockSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLockTitle
            // 
            lblLockTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblLockTitle.ForeColor = System.Drawing.Color.FromArgb(255, 215, 0);
            lblLockTitle.Location = new System.Drawing.Point(250, 240);
            lblLockTitle.Name = "lblLockTitle";
            lblLockTitle.Size = new System.Drawing.Size(800, 45);
            lblLockTitle.TabIndex = 1;
            lblLockTitle.Text = "MÁY ĐANG TẠM KHÓA - RỜI BÀN AN TOÀN";
            lblLockTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLockIcon
            // 
            lblLockIcon.Font = new System.Drawing.Font("Segoe UI", 48F);
            lblLockIcon.ForeColor = System.Drawing.Color.FromArgb(255, 215, 0);
            lblLockIcon.Location = new System.Drawing.Point(550, 120);
            lblLockIcon.Name = "lblLockIcon";
            lblLockIcon.Size = new System.Drawing.Size(200, 110);
            lblLockIcon.TabIndex = 0;
            lblLockIcon.Text = "🔒";
            lblLockIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrRealtime
            // 
            tmrRealtime.Interval = 1000;
            // 
            // FormClientDashboard
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(15, 20, 28);
            ClientSize = new System.Drawing.Size(1280, 800);
            Controls.Add(pnlLockOverlay);
            Controls.Add(tabMain);
            Controls.Add(pnlTopHud);
            MinimumSize = new System.Drawing.Size(1180, 720);
            Name = "FormClientDashboard";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Master Game Client HUD - Màn Hình Máy Trạm Khách Chơi";
            pnlTopHud.ResumeLayout(false);
            pnlStationInfo.ResumeLayout(false);
            pnlStationInfo.PerformLayout();
            pnlSessionTimers.ResumeLayout(false);
            pnlSessionTimers.PerformLayout();
            pnlAccountActions.ResumeLayout(false);
            pnlAccountActions.PerformLayout();
            tabMain.ResumeLayout(false);
            tabGameHub.ResumeLayout(false);
            pnlGameTop.ResumeLayout(false);
            pnlGameTop.PerformLayout();
            pnlActiveGame.ResumeLayout(false);
            pnlActiveGame.PerformLayout();
            tabFoodMenu.ResumeLayout(false);
            pnlFoodLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFoodItems).EndInit();
            pnlFoodFilter.ResumeLayout(false);
            pnlFoodFilter.PerformLayout();
            pnlFoodRight.ResumeLayout(false);
            pnlFoodRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSessionOrders).EndInit();
            pnlOrderAction.ResumeLayout(false);
            pnlOrderAction.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudFoodQty).EndInit();
            pnlLockOverlay.ResumeLayout(false);
            pnlLockOverlay.PerformLayout();
            pnlHardwareHud.ResumeLayout(false);
            tabChat.ResumeLayout(false);
            pnlChatHeader.ResumeLayout(false);
            pnlChatHeader.PerformLayout();
            pnlVietQrDrawer.ResumeLayout(false);
            pnlVietQrDrawer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picDrawerQr).EndInit();
            pnlChatContainer.ResumeLayout(false);
            pnlQuickChips.ResumeLayout(false);
            pnlQuickChips.PerformLayout();
            pnlChatInputBar.ResumeLayout(false);
            pnlChatInputBar.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlTopHud;
        private System.Windows.Forms.Panel pnlHardwareHud;
        public System.Windows.Forms.Label lblHardwareLatencyHud;
        private System.Windows.Forms.Panel pnlStationInfo;
        private System.Windows.Forms.Label lblCenterBranding;
        private System.Windows.Forms.Label lblStationInfo;
        private System.Windows.Forms.Panel pnlSessionTimers;
        private System.Windows.Forms.Label lblPlayedTime;
        private System.Windows.Forms.Label lblRemainingTime;
        private System.Windows.Forms.Label lblPlayCost;
        private System.Windows.Forms.Label lblServiceCost;
        private System.Windows.Forms.Label lblRemainingBalance;
        private System.Windows.Forms.Panel pnlAccountActions;
        private System.Windows.Forms.Label lblAccountInfo;
        private System.Windows.Forms.Label lblMemberTier;
        private System.Windows.Forms.Label lblWalletBalance;
        private System.Windows.Forms.Button btnLockScreen;
        private System.Windows.Forms.Button btnChangePassword;
        private System.Windows.Forms.Button btnCallStaff;
        private System.Windows.Forms.Button btnLogoutSession;
        public System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabGameHub;
        private System.Windows.Forms.Panel pnlGameTop;
        private System.Windows.Forms.Button btnCatAll;
        private System.Windows.Forms.Button btnCatMoba;
        private System.Windows.Forms.Button btnCatFps;
        private System.Windows.Forms.Button btnCatRpg;
        private System.Windows.Forms.Button btnCatSports;
        private System.Windows.Forms.Label lblSearchGame;
        private System.Windows.Forms.TextBox txtSearchGame;
        private System.Windows.Forms.Label lblLauncherStatus;
        private System.Windows.Forms.FlowLayoutPanel flpGameCards;
        private System.Windows.Forms.Panel pnlActiveGame;
        private System.Windows.Forms.Label lblActiveGameTitle;
        private System.Windows.Forms.Button btnStopGame;
        private System.Windows.Forms.TabPage tabFoodMenu;
        private System.Windows.Forms.Panel pnlFoodLeft;
        private System.Windows.Forms.Panel pnlFoodFilter;
        private System.Windows.Forms.Button btnFoodAll;
        private System.Windows.Forms.Button btnFoodDrink;
        private System.Windows.Forms.Button btnFoodMeal;
        private System.Windows.Forms.Button btnFoodSnack;
        private System.Windows.Forms.Label lblSearchFood;
        private System.Windows.Forms.TextBox txtSearchFood;
        private System.Windows.Forms.DataGridView dgvFoodItems;
        private System.Windows.Forms.Panel pnlFoodRight;
        private System.Windows.Forms.Panel pnlOrderAction;
        private System.Windows.Forms.Label lblSelectedFoodName;
        private System.Windows.Forms.Label lblSelectedFoodPrice;
        private System.Windows.Forms.Label lblFoodQty;
        private System.Windows.Forms.NumericUpDown nudFoodQty;
        private System.Windows.Forms.Label lblFoodNote;
        private System.Windows.Forms.TextBox txtFoodNote;
        private System.Windows.Forms.Label lblOrderTotal;
        private System.Windows.Forms.Button btnOrderFood;
        private System.Windows.Forms.Label lblOrdersHeader;
        private System.Windows.Forms.DataGridView dgvSessionOrders;
        public System.Windows.Forms.TabPage tabChat;
        private System.Windows.Forms.Panel pnlChatHeader;
        private System.Windows.Forms.Label lblChatHeaderTitle;
        private System.Windows.Forms.Label lblChatHeaderInfo;
        public System.Windows.Forms.Button btnOpenVietQrTopUp;
        private System.Windows.Forms.Panel pnlChatContainer;
        public System.Windows.Forms.FlowLayoutPanel flpClientChat;
        public System.Windows.Forms.Panel pnlVietQrDrawer;
        private System.Windows.Forms.Label lblDrawerHeader;
        private System.Windows.Forms.Label lblDrawerAmountPrompt;
        public System.Windows.Forms.ComboBox cboDrawerAmount;
        public System.Windows.Forms.PictureBox picDrawerQr;
        public System.Windows.Forms.Label lblDrawerSyntax;
        public System.Windows.Forms.Button btnCloseDrawer;
        private System.Windows.Forms.Panel pnlQuickChips;
        private System.Windows.Forms.Label lblQuickChipsPrompt;
        public System.Windows.Forms.Button btnChipWater;
        public System.Windows.Forms.Button btnChipDeposit;
        public System.Windows.Forms.Button btnChipHardware;
        public System.Windows.Forms.Button btnChipAC;
        public System.Windows.Forms.Button btnChipFood;
        private System.Windows.Forms.Panel pnlChatInputBar;
        public System.Windows.Forms.TextBox txtClientChatMessage;
        public System.Windows.Forms.Button btnSendClientChat;
        private System.Windows.Forms.Panel pnlLockOverlay;
        private System.Windows.Forms.Label lblLockIcon;
        private System.Windows.Forms.Label lblLockTitle;
        private System.Windows.Forms.Label lblLockSubtitle;
        private System.Windows.Forms.Label lblUnlockPasswordPrompt;
        private System.Windows.Forms.TextBox txtUnlockPassword;
        private System.Windows.Forms.Button btnUnlock;
        private System.Windows.Forms.Label lblUnlockError;
        private System.Windows.Forms.Timer tmrRealtime;
    }
}
