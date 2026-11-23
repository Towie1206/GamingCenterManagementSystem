using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem
{
    public partial class Form1 : Form
    {
        private readonly DBStation _dbStation;
        private readonly DBUser _dbUser;
        private readonly DBBilling _dbBilling;

        private List<ComputerStation> _stations = new();
        private List<StationZone> _zones = new();
        private List<AppUser> _users = new();
        private readonly List<AppUser> _selectableUsers = new();
        private ComputerStation? _selectedStation = null;
        private readonly Dictionary<int, GamingSession> _activeSessions = new();
        private readonly Dictionary<int, Button> _stationButtons = new();
        private readonly System.Windows.Forms.Timer _realtimeTimer = new();
        private readonly Font _stationButtonFont = new("Segoe UI", 8.5F, FontStyle.Bold);
        private readonly ToolTip _stationToolTip = new();
        private int? _selectedZoneFilterId = null;
        private Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? _promptHandler;
        private Action<ChatMessage>? _onChatMessageSentHandler;
        private Action<int>? _onChatUnreadChangedHandler;
        private FormCashierChatHub? _activeChatHub;

        public bool IsLogoutRequested { get; private set; }

        public Form1() : this(new DBStation(), new DBUser(), new DBBilling())
        {
        }

        public Form1(DBStation dbStation, DBUser dbUser, DBBilling dbBilling)
        {
            ArgumentNullException.ThrowIfNull(dbStation);
            ArgumentNullException.ThrowIfNull(dbUser);
            ArgumentNullException.ThrowIfNull(dbBilling);

            _dbStation = dbStation;
            _dbUser = dbUser;
            _dbBilling = dbBilling;

            InitializeComponent();
            DoubleBuffered = true;
            SetupNavigation();
            SetupStationControls();
            SetupRealtimeTimer();
            LoadDashboardData();
            ApplyRolePermissions();
            Shown += (s, e) => AdjustFloorplanCardWidths();
            SizeChanged += (s, e) => AdjustFloorplanCardWidths();
        }

        private void SetupNavigation()
        {
            btnNavStation.Click += (s, e) =>
            {
                if (!EnsureStaffOrAdminAccess("Quản lý máy trạm & khu vực"))
                    return;

                FormStation form = new FormStation();
                form.OnStationNotification += msg => LoadDashboardData();
                form.Show(this);
            };

            btnNavMember.Click += (s, e) =>
            {
                if (!EnsureStaffOrAdminAccess("Quản lý hội viên & nạp ví"))
                    return;

                FormMember form = new FormMember();
                form.OnBalanceTopUpSuccess += (user, amt) => LoadDashboardData();
                form.OnMemberChanged += msg => LoadDashboardData();
                form.Show(this);
            };

            btnNavTournament.Click += (s, e) =>
            {
                if (!EnsureStaffOrAdminAccess("Quản lý giải đấu phòng máy"))
                    return;

                FormTournament form = new FormTournament();
                form.OnTournamentEvent += msg => LoadDashboardData();
                form.Show(this);
            };

            btnNavBilling.Click += (s, e) =>
            {
                if (CurrentUserSession.IsLoggedIn && !CurrentUserSession.IsAdmin)
                {
                    ShowNotification(
                        "Chỉ tài khoản Quản trị viên (Admin) mới được phép truy cập báo cáo doanh thu & cấu hình gói cước!",
                        "Giới hạn phân quyền",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                FormBilling form = new FormBilling();
                form.OnBillingNotification += msg => LoadDashboardData();
                form.Show(this);
            };

            btnLamMoiSoDo.Click += (s, e) => LoadDashboardData();

            btnClientMode.Click += (s, e) =>
            {
                int stationId = _selectedStation?.StationId ?? 1;
                using var clientForm = new FormClientDashboard(_dbStation, _dbBilling, _dbUser, stationId);
                clientForm.ShowDialog(this);
                LoadDashboardData();
            };

            btnChatHub.Click += (s, e) => OpenChatHub(_selectedStation?.StationId);
            btnChatStation.Click += (s, e) => OpenChatHub(_selectedStation?.StationId ?? 1);

            _onChatMessageSentHandler = msg =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        int unread = StationChatService.GetUnreadCountForStaff();
                        if (unread > 0)
                        {
                            btnChatHub.Text = $"💬 Chat Quầy ({unread})";
                            btnChatHub.BackColor = Color.FromArgb(234, 88, 12);
                            lblRealtimeSummary.Text = $"💬 [{msg.StationName}] {msg.SenderDisplayName}: {msg.MessageText} [Click để Chat]";
                        }
                        else
                        {
                            btnChatHub.Text = "💬 Chat Quầy";
                            btnChatHub.BackColor = Color.FromArgb(13, 148, 136);
                        }
                    }));
                }
                catch { }
            };

            _onChatUnreadChangedHandler = stId =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        int unread = StationChatService.GetUnreadCountForStaff();
                        if (unread > 0)
                        {
                            btnChatHub.Text = $"💬 Chat Quầy ({unread})";
                            btnChatHub.BackColor = Color.FromArgb(234, 88, 12);
                        }
                        else
                        {
                            btnChatHub.Text = "💬 Chat Quầy";
                            btnChatHub.BackColor = Color.FromArgb(13, 148, 136);
                        }
                    }));
                }
                catch { }
            };

            StationChatService.OnMessageSent += _onChatMessageSentHandler;
            StationChatService.OnUnreadCountChanged += _onChatUnreadChangedHandler;

            StaffCallQueue.OnCallReceived += call =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        lblRealtimeSummary.Text = $"🔔 Máy {call.StationName} gọi quầy: {call.Reason} ({call.Timestamp:HH:mm:ss}) [Click để xử lý]";
                    }));
                }
                catch { }
            };

            StaffCallQueue.OnCallResolved += call =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        lblRealtimeSummary.Text = $"✅ Đã xử lý yêu cầu từ {call.StationName} ({call.ResolvedBy} lúc {call.ResolvedAt:HH:mm:ss})";
                    }));
                }
                catch { }
            };

            lblRealtimeSummary.Cursor = Cursors.Hand;
            lblRealtimeSummary.Click += (s, e) =>
            {
                if (StationChatService.GetUnreadCountForStaff() > 0)
                {
                    OpenChatHub(_selectedStation?.StationId);
                    return;
                }

                var pending = StaffCallQueue.GetPendingCalls();
                if (pending.Count == 0)
                {
                    ShowNotification("Hiện không có cuộc gọi máy trạm nào đang chờ xử lý.", "Trạng thái hỗ trợ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var latest = pending.Last();
                var ans = ShowNotification(
                    $"Yêu cầu từ máy {latest.StationName} ({latest.MemberUsername}):\nLý do: {latest.Reason}\nThời gian: {latest.Timestamp:HH:mm:ss}\n\nXác nhận nhân viên '{CurrentUserSession.Username}' đã xử lý xong yêu cầu này?",
                    "Xử lý cuộc gọi máy trạm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (ans == DialogResult.Yes)
                {
                    StaffCallQueue.ResolveCall(latest.CallId, CurrentUserSession.Username);
                }
            };

            btnDangXuat.Click += (s, e) =>
            {
                var confirm = ShowNotification(
                    $"Bạn có chắc chắn muốn đăng xuất tài khoản '{CurrentUserSession.Username}'?",
                    "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    IsLogoutRequested = true;
                    Close();
                }
            };
        }

        internal void SetPromptHandler(Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? handler)
        {
            _promptHandler = handler;
        }

        private DialogResult ShowNotification(
            string text,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon)
        {
            if (_promptHandler != null)
                return _promptHandler(text, caption, buttons, icon);

            return MessageBox.Show(this, text, caption, buttons, icon);
        }

        private void DisposeManagedResources()
        {
            _realtimeTimer.Stop();
            _realtimeTimer.Dispose();
            _stationButtonFont.Dispose();
            _stationToolTip.Dispose();
        }

        private void SetupStationControls()
        {
            btnMoMay.Click += BtnMoMay_Click;
            btnGoiMon.Click += BtnGoiMon_Click;
            btnDongMay.Click += BtnDongMay_Click;
            btnBaoTri.Click += BtnBaoTri_Click;
            btnDieuKhienMay.Click += (s, e) => OpenRemoteControlDialog(_selectedStation?.StationId);
            flpDanhSachMay.SizeChanged += (s, e) => AdjustFloorplanCardWidths();
            cboGoiCuoc.SelectedIndex = 0;
        }

        public void OpenRemoteControlDialog(int? stationId = null)
        {
            if (!EnsureStaffOrAdminAccess("Điều khiển máy trạm từ xa")) return;

            int targetId = stationId ?? _selectedStation?.StationId ?? 1;
            var targetStation = _stations.FirstOrDefault(s => s.StationId == targetId) ?? _selectedStation;
            if (targetStation == null)
            {
                ShowNotification("Vui lòng chọn một máy trạm để điều khiển!", "Chưa chọn máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new FormStationRemoteControl(targetStation, _stations, _zones, _dbBilling, _dbStation);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                LoadDashboardData();
                if (dlg.TargetStationId.HasValue)
                {
                    var newSt = _stations.FirstOrDefault(s => s.StationId == dlg.TargetStationId.Value);
                    if (newSt != null) SelectStation(newSt);
                }
            }
        }

        private void SetupRealtimeTimer()
        {
            _realtimeTimer.Interval = 1000;
            _realtimeTimer.Tick += (s, e) => UpdateRealtimeDisplay();
            _realtimeTimer.Start();

            FormClosed += (s, e) =>
            {
                if (_onChatMessageSentHandler != null)
                    StationChatService.OnMessageSent -= _onChatMessageSentHandler;
                if (_onChatUnreadChangedHandler != null)
                    StationChatService.OnUnreadCountChanged -= _onChatUnreadChangedHandler;

                _realtimeTimer.Stop();
                _realtimeTimer.Dispose();
            };
        }

        private bool EnsureStaffOrAdminAccess(string moduleName)
        {
            if (!CurrentUserSession.IsLoggedIn || CurrentUserSession.CanManageOperations)
                return true;

            ShowNotification(
                $"Tài khoản hội viên ({CurrentUserSession.Username}) không có quyền truy cập chức năng '{moduleName}'.",
                "Không đủ quyền truy cập",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        public void ApplyRolePermissions()
        {
            if (!CurrentUserSession.IsLoggedIn)
            {
                lblCurrentUser.Text = "👤 Chế độ xem trước\n(Chưa đăng nhập)";
                btnDangXuat.Visible = false;
                btnNavStation.Enabled = true;
                btnNavMember.Enabled = true;
                btnNavTournament.Enabled = true;
                btnNavBilling.Enabled = true;
                btnNavBilling.Text = "💰 Dịch Vụ & Doanh Thu";
                btnChatHub.Enabled = true;
                btnChatStation.Enabled = true;
                btnDieuKhienMay.Enabled = true;
                cboKhachHang.Enabled = true;
                cboGoiCuoc.Enabled = true;
                return;
            }

            btnDangXuat.Visible = true;

            if (CurrentUserSession.IsAdmin)
            {
                lblCurrentUser.Text = $"👤 {CurrentUserSession.FullName} ({CurrentUserSession.Username})\nVai trò: Admin | Toàn quyền hệ thống";
                lblCurrentUser.ForeColor = Color.FromArgb(120, 255, 180);
                btnNavStation.Enabled = true;
                btnNavMember.Enabled = true;
                btnNavTournament.Enabled = true;
                btnNavBilling.Enabled = true;
                btnNavBilling.Text = "💰 Dịch Vụ & Doanh Thu";
                btnClientMode.Enabled = true;
                btnChatHub.Enabled = true;
                btnChatStation.Enabled = true;
                btnDieuKhienMay.Enabled = true;
                cboKhachHang.Enabled = true;
                cboGoiCuoc.Enabled = true;
            }
            else if (CurrentUserSession.IsStaff)
            {
                lblCurrentUser.Text = $"👤 {CurrentUserSession.FullName} ({CurrentUserSession.Username})\nVai trò: Staff | Vận hành quầy máy";
                lblCurrentUser.ForeColor = Color.FromArgb(0, 210, 255);
                btnNavStation.Enabled = true;
                btnNavMember.Enabled = true;
                btnNavTournament.Enabled = true;
                btnNavBilling.Enabled = false;
                btnNavBilling.Text = "🔒 Doanh Thu (Chỉ Admin)";
                btnClientMode.Enabled = true;
                btnChatHub.Enabled = true;
                btnChatStation.Enabled = true;
                btnDieuKhienMay.Enabled = true;
                cboKhachHang.Enabled = true;
                cboGoiCuoc.Enabled = true;
            }
            else
            {
                lblCurrentUser.Text = $"👤 {CurrentUserSession.FullName} ({CurrentUserSession.Username})\nHội viên {CurrentUserSession.MemberTier} | Ví: {CurrentUserSession.Balance:N0} VNĐ";
                lblCurrentUser.ForeColor = Color.FromArgb(255, 215, 100);
                btnNavStation.Enabled = false;
                btnNavMember.Enabled = false;
                btnNavTournament.Enabled = false;
                btnNavBilling.Enabled = false;
                btnNavBilling.Text = "💰 Dịch Vụ & Doanh Thu";
                btnClientMode.Enabled = true;
                btnChatHub.Enabled = false;
                btnChatStation.Enabled = false;
                btnDieuKhienMay.Enabled = false;
                btnMoMay.Enabled = false;
                btnGoiMon.Enabled = false;
                btnDongMay.Enabled = false;
                btnBaoTri.Enabled = false;
                cboKhachHang.Enabled = false;
                cboGoiCuoc.Enabled = false;
            }

            if (_selectedStation != null)
            {
                SelectStation(_selectedStation);
            }
        }

        public FormCashierChatHub OpenChatHub(int? stationId = null)
        {
            int targetStationId = stationId ?? _selectedStation?.StationId ?? 1;

            if (_activeChatHub != null && !_activeChatHub.IsDisposed)
            {
                _activeChatHub.SelectStation(targetStationId);
                if (_activeChatHub.WindowState == FormWindowState.Minimized)
                {
                    _activeChatHub.WindowState = FormWindowState.Normal;
                }
                _activeChatHub.BringToFront();
                _activeChatHub.Focus();
                return _activeChatHub;
            }

            _activeChatHub = new FormCashierChatHub(_dbStation, targetStationId);
            _activeChatHub.Show(this);
            return _activeChatHub;
        }

        public void LoadDashboardData()
        {
            try
            {
                _zones = _dbStation.GetAllZones();
                _stations = _dbStation.GetAll();
                _users = _dbUser.GetAll();

                if (CurrentUserSession.IsLoggedIn)
                {
                    var latestCurrentUser = _users.FirstOrDefault(u => u.UserId == CurrentUserSession.UserId);
                    if (latestCurrentUser != null)
                    {
                        CurrentUserSession.RefreshUser(latestCurrentUser);
                    }
                }

                // Sync active sessions from database
                _activeSessions.Clear();
                var activeList = _dbBilling.GetActiveSessions();
                foreach (var s in activeList)
                {
                    _activeSessions[s.StationId] = s;
                }

                // Populate users in combo (mask other members' balances when logged in as Member)
                cboKhachHang.Items.Clear();
                _selectableUsers.Clear();

                if (CurrentUserSession.IsMember)
                {
                    var self = _users.FirstOrDefault(u => u.UserId == CurrentUserSession.UserId);
                    if (self != null)
                    {
                        _selectableUsers.Add(self);
                        cboKhachHang.Items.Add($"{self.Username} - {self.FullName} ({self.Balance:N0}đ)");
                        cboKhachHang.SelectedIndex = 0;
                    }
                    else
                    {
                        cboKhachHang.Items.Add($"{CurrentUserSession.Username} - {CurrentUserSession.FullName}");
                        cboKhachHang.SelectedIndex = 0;
                    }
                }
                else
                {
                    cboKhachHang.Items.Add("Khách vãng lai (Tiền mặt)");
                    foreach (var user in _users)
                    {
                        _selectableUsers.Add(user);
                        string lockedTag = user.IsActive ? string.Empty : " [BỊ KHÓA]";
                        cboKhachHang.Items.Add($"{user.Username} - {user.FullName} ({user.Balance:N0}đ){lockedTag}");
                    }
                    cboKhachHang.SelectedIndex = 0;
                }

                RenderStationGrid();
                ApplyRolePermissions();
            }
            catch (Exception ex)
            {
                ShowNotification($"Lỗi tải dữ liệu phòng máy: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Sơ đồ phòng máy Master Game trực quan theo 3 dàn máy (Maximum / Master / Standard) & đổi màu Realtime
        private void RenderStationGrid()
        {
            flpDanhSachMay.SuspendLayout();
            var oldControls = flpDanhSachMay.Controls.Cast<Control>().ToList();
            flpDanhSachMay.Controls.Clear();
            foreach (var oldCtrl in oldControls)
            {
                oldCtrl.Dispose();
            }
            _stationButtons.Clear();

            int cardWidth = GetFloorplanCardWidth();

            // 1. Thanh lọc dàn máy & bảng khuyến mãi Master Game
            flpDanhSachMay.Controls.Add(BuildFilterAndPromoCard(cardWidth));

            // 2. Sắp xếp các dàn máy đúng theo sơ đồ Master Game:
            //    Cánh trái: Dàn Maximum (15K) -> Dàn Master (13K) -> Quầy Lễ Tân (Reception) -> Cánh phải: Dàn Standard (10K) -> Các khu thêm mới
            var orderedZones = _zones
                .OrderBy(z => GetZoneDisplayOrder(z.ZoneName))
                .ThenBy(z => z.ZoneId)
                .ToList();

            var filteredZones = _selectedZoneFilterId.HasValue
                ? orderedZones.Where(z => z.ZoneId == _selectedZoneFilterId.Value).ToList()
                : orderedZones;

            bool receptionInserted = false;
            bool showReceptionDivider = !_selectedZoneFilterId.HasValue && filteredZones.Count >= 2;

            foreach (var zone in filteredZones)
            {
                var zoneStations = _stations
                    .Where(s => s.ZoneId == zone.ZoneId)
                    .OrderBy(s => s.StationId)
                    .ToList();

                if (showReceptionDivider && !receptionInserted && zone.ZoneName.Contains("Standard", StringComparison.OrdinalIgnoreCase))
                {
                    flpDanhSachMay.Controls.Add(BuildReceptionDivider(cardWidth));
                    receptionInserted = true;
                }

                flpDanhSachMay.Controls.Add(BuildZoneSectionCard(zone, zoneStations, cardWidth));
            }

            // Hiển thị các máy có ZoneId không khớp danh mục (nếu có)
            if (!_selectedZoneFilterId.HasValue)
            {
                var knownZoneIds = new HashSet<int>(_zones.Select(z => z.ZoneId));
                var orphanStations = _stations.Where(s => !knownZoneIds.Contains(s.ZoneId)).OrderBy(s => s.StationId).ToList();
                if (orphanStations.Count > 0)
                {
                    var fallbackZone = new StationZone { ZoneId = 0, ZoneName = "Khu vực khác", BasePricePerHour = 10000m };
                    flpDanhSachMay.Controls.Add(BuildZoneSectionCard(fallbackZone, orphanStations, cardWidth));
                }
            }

            flpDanhSachMay.ResumeLayout(true);

            // Reselect current station if still present
            if (_selectedStation != null)
            {
                var refreshed = _stations.FirstOrDefault(s => s.StationId == _selectedStation.StationId);
                if (refreshed != null)
                    SelectStation(refreshed);
                else if (_stations.Count > 0)
                    SelectStation(_stations[0]);
            }
            else if (_stations.Count > 0)
            {
                SelectStation(_stations[0]);
            }

            UpdateRealtimeDisplay();
        }

        private int GetFloorplanCardWidth()
        {
            int available = flpDanhSachMay.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 18;
            return Math.Max(520, available);
        }

        private void AdjustFloorplanCardWidths()
        {
            if (IsDisposed || WindowState == FormWindowState.Minimized || flpDanhSachMay.Controls.Count == 0)
                return;

            if (flpDanhSachMay.ClientSize.Width < 200)
                return;

            int cardWidth = GetFloorplanCardWidth();
            flpDanhSachMay.SuspendLayout();
            foreach (Control ctrl in flpDanhSachMay.Controls)
            {
                ctrl.Width = cardWidth;

                if (ctrl.Tag is string tag && tag == "FilterCard")
                {
                    ResizeFilterCard(ctrl, cardWidth);
                }
                else if (ctrl.Tag is FlowLayoutPanel zoneGrid)
                {
                    ResizeZoneCard(ctrl, zoneGrid, cardWidth);
                }

                ctrl.Invalidate();
            }
            flpDanhSachMay.ResumeLayout(true);
            flpDanhSachMay.PerformLayout();
        }

        private static void ResizeFilterCard(Control card, int cardWidth)
        {
            int innerW = Math.Max(300, cardWidth - 24);
            FlowLayoutPanel? flpFilter = null;
            Label? lblPromo = null;

            foreach (Control child in card.Controls)
            {
                if (child is FlowLayoutPanel flp) flpFilter = flp;
                else if (child is Label lbl) lblPromo = lbl;
            }

            int filterH = 34;
            if (flpFilter != null)
            {
                flpFilter.MaximumSize = new Size(innerW, 0);
                flpFilter.Width = innerW;
                flpFilter.PerformLayout();
                filterH = Math.Max(32, flpFilter.GetPreferredSize(new Size(innerW, 0)).Height);
                flpFilter.Height = filterH;
            }

            int promoH = 24;
            if (lblPromo != null)
            {
                lblPromo.MaximumSize = new Size(innerW, 0);
                lblPromo.Width = innerW;
                promoH = Math.Max(24, lblPromo.GetPreferredSize(new Size(innerW, 0)).Height + 6);
                lblPromo.Height = promoH;
            }

            card.Height = filterH + promoH + 22;
        }

        private static void ResizeZoneCard(Control card, FlowLayoutPanel zoneGrid, int cardWidth)
        {
            int stationCount = zoneGrid.Controls.OfType<Button>().Count();
            const int headerHeight = 54;
            const int tileOuterW = 128 + 12; // Width (128) + Margin.Left/Right (6+6)
            const int tileOuterH = 82 + 12;  // Height (82) + Margin.Top/Bottom (6+6)

            int innerGridW = Math.Max(tileOuterW, cardWidth - 24);
            int cols = Math.Max(1, innerGridW / tileOuterW);
            int rows = stationCount == 0 ? 1 : Math.Max(1, (int)Math.Ceiling(stationCount / (double)cols));
            int gridHeight = stationCount == 0 ? 44 : (rows * tileOuterH + 16);

            zoneGrid.Height = gridHeight;
            card.Height = headerHeight + gridHeight + 6;
        }

        private static int GetZoneDisplayOrder(string? zoneName)
        {
            string name = zoneName ?? string.Empty;
            if (name.Contains("Maximum", StringComparison.OrdinalIgnoreCase)) return 1;
            if (name.Contains("Master", StringComparison.OrdinalIgnoreCase)) return 2;
            if (name.Contains("Standard", StringComparison.OrdinalIgnoreCase)) return 3;
            return 4;
        }

        private Panel BuildFilterAndPromoCard(int width)
        {
            var card = new Panel
            {
                Width = width,
                Height = 82,
                Tag = "FilterCard",
                BackColor = UiThemeHelper.SurfaceWhite,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(12, 8, 12, 8)
            };

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(UiThemeHelper.BorderSubtle, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var flpFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            flpFilter.Controls.Add(CreateZoneFilterButton($"Tất cả ({_stations.Count})", null, UiThemeHelper.AccentBlue));

            foreach (var z in _zones.OrderBy(z => GetZoneDisplayOrder(z.ZoneName)).ThenBy(z => z.ZoneId))
            {
                int count = _stations.Count(s => s.ZoneId == z.ZoneId);
                var meta = UiThemeHelper.GetMasterGameZoneMeta(z.ZoneName, z.BasePricePerHour);
                string label = $"{z.ZoneName} ({z.BasePricePerHour / 1000m:0}K • {count})";
                flpFilter.Controls.Add(CreateZoneFilterButton(label, z.ZoneId, meta.Accent));
            }

            var lblPromo = new Label
            {
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 24,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 83, 9),
                BackColor = Color.FromArgb(255, 251, 235),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 2, 8, 2),
                Text = "⚡ Happy Hours 23h-11h (-40%): Standard 6K • Master 8K • Maximum 9K  |  🎁 Nạp 500K +20% • 1M +50% • 2M +100%"
            };

            card.Controls.Add(lblPromo);
            card.Controls.Add(flpFilter);
            ResizeFilterCard(card, width);
            return card;
        }

        private Button CreateZoneFilterButton(string text, int? zoneId, Color accent)
        {
            bool active = _selectedZoneFilterId == zoneId;
            var btn = new Button
            {
                AutoSize = true,
                MinimumSize = new Size(92, 28),
                Height = 28,
                Margin = new Padding(0, 0, 8, 4),
                Padding = new Padding(8, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Text = text,
                BackColor = active ? accent : Color.FromArgb(248, 250, 252),
                ForeColor = active ? Color.White : UiThemeHelper.TextPrimary
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = active ? accent : UiThemeHelper.BorderSubtle;
            btn.Click += (s, e) =>
            {
                _selectedZoneFilterId = zoneId;
                RenderStationGrid();
            };
            return btn;
        }

        private static Panel BuildReceptionDivider(int width)
        {
            var bar = new Panel
            {
                Width = width,
                Height = 32,
                BackColor = UiThemeHelper.SubHeaderDark,
                Margin = new Padding(0, 2, 0, 10)
            };

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240),
                Text = "🏪  QUẦY LỄ TÂN (RECEPTION) — KHU TRÁI: DÀN MAXIMUM & MASTER   |   KHU PHẢI: DÀN STANDARD"
            };
            bar.Controls.Add(lbl);
            return bar;
        }

        private Panel BuildZoneSectionCard(StationZone zone, List<ComputerStation> zoneStations, int width)
        {
            var meta = UiThemeHelper.GetMasterGameZoneMeta(zone.ZoneName, zone.BasePricePerHour);
            string specSummary = !string.IsNullOrWhiteSpace(zone.Description)
                ? zone.Description
                : meta.ShortSpec;

            int available = zoneStations.Count(s => string.Equals(s.Status, "Available", StringComparison.OrdinalIgnoreCase));
            int inUse = zoneStations.Count(s => string.Equals(s.Status, "InUse", StringComparison.OrdinalIgnoreCase));

            var card = new Panel
            {
                Width = width,
                BackColor = UiThemeHelper.SurfaceWhite,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0, 0, 0, 4)
            };

            card.Paint += (s, e) =>
            {
                using var borderPen = new Pen(UiThemeHelper.BorderSubtle, 1);
                e.Graphics.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
                using var accentBrush = new SolidBrush(meta.Accent);
                e.Graphics.FillRectangle(accentBrush, 0, 0, 5, card.Height);
            };

            // Header của Dàn Máy
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = meta.SoftBg,
                Padding = new Padding(14, 6, 14, 6)
            };

            var lblZoneHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = meta.Accent,
                Text = $"■  {zone.ZoneName.ToUpperInvariant()}  —  {zone.BasePricePerHour:N0}đ/h (Happy: {meta.HappyHourPriceText})  •  Trống {available}/{zoneStations.Count} | Đang chơi {inUse}"
            };

            var lblZoneSpec = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 18,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = UiThemeHelper.TextSecondary,
                AutoEllipsis = true,
                Text = $"⚙️ {specSummary}"
            };

            pnlTop.Controls.Add(lblZoneSpec);
            pnlTop.Controls.Add(lblZoneHeader);

            // Lưới nút máy trạm bên trong dàn máy (chiều cao tính chuẩn xác theo số cột/hàng khi resize)
            var flpZoneGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = UiThemeHelper.SurfaceWhite,
                Padding = new Padding(12, 8, 8, 4)
            };

            foreach (var station in zoneStations)
            {
                Button btn = CreateStationTileButton(station, zone, specSummary);
                _stationButtons[station.StationId] = btn;
                flpZoneGrid.Controls.Add(btn);
            }

            if (zoneStations.Count == 0)
            {
                flpZoneGrid.Controls.Add(new Label
                {
                    AutoSize = true,
                    Padding = new Padding(6, 10, 6, 10),
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = UiThemeHelper.TextSecondary,
                    Text = "Chưa có máy trạm nào trong dàn này."
                });
            }

            card.Tag = flpZoneGrid;
            ResizeZoneCard(card, flpZoneGrid, width);

            // Thứ tự add Dock.Top: add grid trước, header sau để header nằm trên cùng
            card.Controls.Add(flpZoneGrid);
            card.Controls.Add(pnlTop);
            return card;
        }

        private Button CreateStationTileButton(ComputerStation station, StationZone zone, string specSummary)
        {
            Button btn = new Button
            {
                Width = 128,
                Height = 82,
                Margin = new Padding(6),
                Tag = station,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = _stationButtonFont,
                ForeColor = UiThemeHelper.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var cms = new ContextMenuStrip();
            var itemRemote = new ToolStripMenuItem("🕹️ Điều khiển máy từ xa...", null, (s, e) => OpenRemoteControlDialog(station.StationId));
            var itemChat = new ToolStripMenuItem("💬 Chat với máy này", null, (s, e) => OpenChatHub(station.StationId));
            var itemForceLock = new ToolStripMenuItem("🔒 Khóa cưỡng chế máy này", null, (s, e) =>
            {
                string op = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                StationRemoteControlService.SendForceLock(station.StationId, station.StationName, "Khóa nhanh từ sơ đồ quầy", op);
                LoadDashboardData();
            });
            var itemUnlock = new ToolStripMenuItem("🔓 Mở khóa máy", null, (s, e) =>
            {
                string op = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                StationRemoteControlService.SendUnlock(station.StationId, station.StationName, op);
                LoadDashboardData();
            });
            var itemKillApp = new ToolStripMenuItem("❌ Tắt ứng dụng / game", null, (s, e) =>
            {
                string op = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                StationRemoteControlService.SendKillApp(station.StationId, station.StationName, "all", op);
                ShowNotification($"Đã gửi lệnh tắt ứng dụng đến {station.StationName}!", "Lệnh hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            cms.Items.AddRange(new ToolStripItem[] { itemRemote, itemChat, new ToolStripSeparator(), itemForceLock, itemUnlock, itemKillApp });
            btn.ContextMenuStrip = cms;

            ApplyStationTileVisual(btn, station, zone.ZoneName, isSelected: _selectedStation?.StationId == station.StationId);

            string tooltipText = $"{station.StationName} ({zone.ZoneName})\nIP: {station.IpAddress} | Giá: {zone.BasePricePerHour:N0}đ/h\nCấu hình: {specSummary}\n(Bấm chuột phải để mở menu Điều Khiển Máy Từ Xa)";
            _stationToolTip.SetToolTip(btn, tooltipText);

            btn.Click += StationButton_Click;
            return btn;
        }

        private void ApplyStationTileVisual(Button btn, ComputerStation station, string zoneName, bool isSelected)
        {
            btn.FlatAppearance.BorderSize = isSelected ? 3 : 2;

            if (StationRemoteControlService.IsStationLocked(station.StationId, out string? _))
            {
                btn.BackColor = isSelected ? Color.FromArgb(254, 226, 226) : Color.FromArgb(254, 242, 242);
                btn.FlatAppearance.BorderColor = isSelected ? UiThemeHelper.AccentBlue : Color.FromArgb(220, 38, 38);
                btn.ForeColor = Color.FromArgb(185, 28, 28);
                btn.Text = $"⛔ {station.StationName}\n{zoneName}\n[BỊ KHÓA]";
                return;
            }

            switch (station.Status)
            {
                case "InUse":
                    btn.BackColor = isSelected ? Color.FromArgb(255, 237, 213) : Color.FromArgb(255, 247, 237);
                    btn.FlatAppearance.BorderColor = isSelected ? UiThemeHelper.AccentBlue : Color.FromArgb(234, 88, 12);
                    btn.ForeColor = Color.FromArgb(154, 52, 18);
                    btn.Text = BuildInUseButtonText(station, zoneName);
                    break;
                case "Maintenance":
                    btn.BackColor = Color.FromArgb(254, 242, 242);
                    btn.FlatAppearance.BorderColor = isSelected ? UiThemeHelper.AccentBlue : Color.FromArgb(220, 38, 38);
                    btn.ForeColor = Color.FromArgb(153, 27, 27);
                    btn.Text = $"🛠️ {station.StationName}\n{zoneName}\n[BẢO TRÌ]";
                    break;
                case "TournamentLocked":
                    btn.BackColor = Color.FromArgb(254, 252, 232);
                    btn.FlatAppearance.BorderColor = isSelected ? UiThemeHelper.AccentBlue : Color.FromArgb(202, 138, 4);
                    btn.ForeColor = Color.FromArgb(133, 77, 14);
                    btn.Text = $"🏆 {station.StationName}\n{zoneName}\n[GIẢI ĐẤU]";
                    break;
                case "Available":
                default:
                    btn.BackColor = isSelected ? Color.FromArgb(239, 246, 255) : UiThemeHelper.SurfaceWhite;
                    btn.FlatAppearance.BorderColor = isSelected ? UiThemeHelper.AccentBlue : Color.FromArgb(16, 185, 129);
                    btn.ForeColor = Color.FromArgb(15, 23, 42);
                    btn.Text = $"🟢 {station.StationName}\n{zoneName}\n[SẴN SÀNG]";
                    break;
            }
        }

        private string BuildInUseButtonText(ComputerStation station, string zoneName)
        {
            if (_activeSessions.TryGetValue(station.StationId, out var active))
            {
                TimeSpan elapsed = DateTime.Now - active.StartTime;
                if (elapsed < TimeSpan.Zero)
                    elapsed = TimeSpan.Zero;

                string elapsedText = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                return $"🖥️ {station.StationName}\n⏱️ {elapsedText}\n[ĐANG CHƠI]";
            }

            return $"🖥️ {station.StationName}\n{zoneName}\n[ĐANG CHƠI]";
        }

        private string BuildActiveSessionRealtimeText(ComputerStation station, GamingSession active, decimal basePrice)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - active.StartTime;
            if (elapsed < TimeSpan.Zero)
                elapsed = TimeSpan.Zero;

            string cust = "Khách vãng lai";
            string memberTier = "Standard";
            if (active.UserId.HasValue)
            {
                var u = _users.FirstOrDefault(x => x.UserId == active.UserId.Value);
                if (u != null)
                {
                    memberTier = u.MemberTier;
                    cust = (CurrentUserSession.IsMember && active.UserId != CurrentUserSession.UserId)
                        ? "Đang có khách sử dụng"
                        : $"{u.Username} ({u.MemberTier} | Ví: {u.Balance:N0}đ)";
                }
                else if (CurrentUserSession.IsMember && active.UserId != CurrentUserSession.UserId)
                {
                    cust = "Đang có khách sử dụng";
                }
            }

            TariffCalculatorBase calculator = DBBilling.CreateTariffCalculator(active.TariffType, memberTier);
            decimal estimatedPlay = calculator.CalculateCost(active.StartTime, now, basePrice);
            decimal estimatedTotal = estimatedPlay + active.ServiceCost;

            return $"🖥️ Phiên đang chạy | Bắt đầu: {active.StartTime:HH:mm:ss}\n" +
                   $"⏱️ Thời lượng: {(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2} | Gói: {active.TariffType}\n" +
                   $"👤 Khách: {cust}\n" +
                   $"💵 Tạm tính: Giờ {estimatedPlay:N0}đ + DV {active.ServiceCost:N0}đ = {estimatedTotal:N0}đ";
        }

        private void UpdateRealtimeDisplay()
        {
            if (IsDisposed)
                return;

            int availableCount = _stations.Count(s => string.Equals(s.Status, "Available", StringComparison.OrdinalIgnoreCase));
            int inUseCount = _stations.Count(s => string.Equals(s.Status, "InUse", StringComparison.OrdinalIgnoreCase));
            lblRealtimeSummary.Text = $"🟢 Trống: {availableCount} | 🟠 Đang chơi: {inUseCount} | 🕒 {DateTime.Now:HH:mm:ss}";

            foreach (var station in _stations)
            {
                if (!string.Equals(station.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (_stationButtons.TryGetValue(station.StationId, out Button? btn))
                {
                    var zone = _zones.FirstOrDefault(z => z.ZoneId == station.ZoneId);
                    string zoneName = zone?.ZoneName ?? $"Khu {station.ZoneId}";
                    string updatedText = BuildInUseButtonText(station, zoneName);
                    if (btn.Text != updatedText)
                    {
                        btn.Text = updatedText;
                    }
                }
            }

            if (_selectedStation != null &&
                string.Equals(_selectedStation.Status, "InUse", StringComparison.OrdinalIgnoreCase) &&
                _activeSessions.TryGetValue(_selectedStation.StationId, out var selectedActive))
            {
                var zone = _zones.FirstOrDefault(z => z.ZoneId == _selectedStation.ZoneId);
                decimal price = zone?.BasePricePerHour ?? 8000m;
                string nextText = BuildActiveSessionRealtimeText(_selectedStation, selectedActive, price);
                if (lblKetQuaTinhTien.Text != nextText)
                {
                    lblKetQuaTinhTien.Text = nextText;
                }
            }
        }

        private void StationButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is ComputerStation station)
            {
                SelectStation(station);
            }
        }

        private void SelectStation(ComputerStation station)
        {
            var previousStation = _selectedStation;
            _selectedStation = station;
            var zone = _zones.FirstOrDefault(z => z.ZoneId == station.ZoneId);
            decimal price = zone?.BasePricePerHour ?? 10000m;
            var meta = UiThemeHelper.GetMasterGameZoneMeta(zone?.ZoneName, price);
            bool canOperateStations = !CurrentUserSession.IsLoggedIn || CurrentUserSession.CanManageOperations;

            // Cập nhật viền chọn nổi bật trên sơ đồ máy
            if (previousStation != null && previousStation.StationId != station.StationId &&
                _stationButtons.TryGetValue(previousStation.StationId, out var prevBtn))
            {
                var prevZone = _zones.FirstOrDefault(z => z.ZoneId == previousStation.ZoneId);
                ApplyStationTileVisual(prevBtn, previousStation, prevZone?.ZoneName ?? $"Khu {previousStation.ZoneId}", isSelected: false);
            }

            if (_stationButtons.TryGetValue(station.StationId, out var curBtn))
            {
                ApplyStationTileVisual(curBtn, station, zone?.ZoneName ?? $"Khu {station.ZoneId}", isSelected: true);
            }

            lblMayDangChon.Text = $"🖥️ Máy Trạm: {station.StationName}";
            lblTrangThaiMay.Text = $"Trạng thái: {station.Status}";
            lblKhuVucMay.Text = $"Dàn máy: {zone?.ZoneName ?? "Mặc định"}";
            lblGiaGio.Text = $"Đơn giá: {price:N0}đ/h  (Happy Hours: {meta.HappyHourPriceText})";
            string specText = !string.IsNullOrWhiteSpace(zone?.Description) ? zone!.Description! : meta.ShortSpec;
            lblCauHinhMay.Text = string.IsNullOrWhiteSpace(specText)
                ? $"⚙️ IP: {station.IpAddress}"
                : $"⚙️ {specText}";

            switch (station.Status)
            {
                case "InUse":
                    lblTrangThaiMay.ForeColor = Color.OrangeRed;
                    btnMoMay.Enabled = false;
                    btnGoiMon.Enabled = canOperateStations;
                    btnDongMay.Enabled = canOperateStations;
                    btnBaoTri.Enabled = false;

                    if (_activeSessions.TryGetValue(station.StationId, out var active))
                    {
                        lblKetQuaTinhTien.Text = BuildActiveSessionRealtimeText(station, active, price);

                        if (active.UserId.HasValue && canOperateStations)
                        {
                            int uIdx = _selectableUsers.FindIndex(u => u.UserId == active.UserId.Value);
                            if (uIdx >= 0 && cboKhachHang.Items.Count > uIdx + 1)
                            {
                                cboKhachHang.SelectedIndex = uIdx + 1;
                            }
                        }
                    }
                    else
                    {
                        lblKetQuaTinhTien.Text = $"Máy {station.StationName} đang ở trạng thái ĐANG CHƠI.";
                    }
                    break;

                case "Maintenance":
                    lblTrangThaiMay.ForeColor = Color.Crimson;
                    btnMoMay.Enabled = false;
                    btnGoiMon.Enabled = false;
                    btnDongMay.Enabled = false;
                    btnBaoTri.Enabled = canOperateStations;
                    btnBaoTri.Text = "🟢 MỞ LẠI SẴN SÀNG";
                    lblKetQuaTinhTien.Text = $"Máy {station.StationName} đang trong chế độ BẢO TRÌ kỹ thuật.";
                    break;

                case "TournamentLocked":
                    lblTrangThaiMay.ForeColor = Color.DarkGoldenrod;
                    btnMoMay.Enabled = false;
                    btnGoiMon.Enabled = false;
                    btnDongMay.Enabled = false;
                    btnBaoTri.Enabled = false;
                    lblKetQuaTinhTien.Text = $"Máy {station.StationName} đang KHÓA phục vụ giải đấu phòng máy.";
                    break;

                case "Available":
                default:
                    lblTrangThaiMay.ForeColor = Color.Green;
                    btnMoMay.Enabled = canOperateStations;
                    btnGoiMon.Enabled = false;
                    btnDongMay.Enabled = false;
                    btnBaoTri.Enabled = canOperateStations;
                    btnBaoTri.Text = "🛠️ CHUYỂN BẢO TRÌ";
                    lblKetQuaTinhTien.Text = canOperateStations
                        ? $"Máy {station.StationName} đang SẴN SÀNG đón khách. Chọn khách hàng & gói cước rồi bấm MỞ MÁY."
                        : $"Máy {station.StationName} đang SẴN SÀNG. Vui lòng liên hệ nhân viên quầy để mở máy.";
                    break;
            }
        }

        private string ResolveSelectedTariffType() => cboGoiCuoc.SelectedIndex switch
        {
            1 => "NightCombo",
            2 => "Tournament",
            3 => "MemberTier",
            _ => "Hourly"
        };

        private AppUser? GetSelectedCustomer()
        {
            if (CurrentUserSession.IsMember)
                return _selectableUsers.FirstOrDefault();

            if (cboKhachHang.SelectedIndex > 0 && _selectableUsers.Count >= cboKhachHang.SelectedIndex)
                return _selectableUsers[cboKhachHang.SelectedIndex - 1];

            return null;
        }

        private void BtnMoMay_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;
            if (!EnsureStaffOrAdminAccess("Mở máy cho khách")) return;

            var member = GetSelectedCustomer();
            int? userId = member?.UserId;
            string customerName = member != null ? $"{member.Username} - {member.FullName}" : "Khách vãng lai";

            string plan = cboGoiCuoc.SelectedItem?.ToString() ?? "Hourly";
            string tariffType = ResolveSelectedTariffType();

            if (string.Equals(tariffType, "MemberTier", StringComparison.OrdinalIgnoreCase) && !userId.HasValue)
            {
                ShowNotification(
                    "Gói cước Ưu đãi hội viên (MemberTier) chỉ áp dụng khi chọn tài khoản hội viên!",
                    "Chọn tài khoản hội viên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var zone = _zones.FirstOrDefault(z => z.ZoneId == _selectedStation.ZoneId);
            decimal basePrice = zone?.BasePricePerHour ?? 8000m;
            decimal minimumRequired = Math.Round(basePrice * 0.1m, 0);
            string stationName = _selectedStation.StationName;

            try
            {
                var session = _dbBilling.OpenStationSession(
                    _selectedStation.StationId,
                    userId,
                    tariffType,
                    minimumRequiredBalance: minimumRequired);

                LoadDashboardData();

                lblKetQuaTinhTien.Text = $"Phiên chơi bắt đầu lúc: {session.StartTime:HH:mm:ss}\n" +
                                         $"Máy: {stationName} | Gói: {plan}\n" +
                                         $"Khách hàng: {customerName}\n" +
                                         $"Trạng thái: Đang tính giờ theo thời gian thực.";

                ShowNotification($"Đã mở máy {stationName} thành công cho khách ({customerName})!", "Mở máy thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (StationInUseException ex)
            {
                ShowNotification(ex.Message, "Máy đang được sử dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (InsufficientBalanceException ex)
            {
                ShowNotification(
                    $"{ex.Message}\nVui lòng nạp thêm tiền vào ví tại màn hình Quản lý hội viên trước khi mở máy.",
                    "Số dư ví không đủ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                ShowNotification(ex.Message, "Không thể mở máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                ShowNotification($"Lỗi mở máy: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDongMay_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;
            if (!EnsureStaffOrAdminAccess("Đóng máy & thanh toán")) return;

            int? fallbackUserId = GetSelectedCustomer()?.UserId;

            string fallbackTariffType = ResolveSelectedTariffType();
            DateTime endTime = DateTime.Now;
            DateTime fallbackStartTime = endTime.AddHours(-1.5);

            var zone = _zones.FirstOrDefault(z => z.ZoneId == _selectedStation.ZoneId);
            decimal basePrice = zone?.BasePricePerHour ?? 8000m;
            int? processedByUserId = CurrentUserSession.IsLoggedIn ? CurrentUserSession.UserId : null;

            try
            {
                SessionCloseResult result;
                try
                {
                    result = _dbBilling.CloseStationSession(
                        _selectedStation.StationId,
                        basePrice,
                        endTime,
                        allowCashSupplement: false,
                        fallbackUserId: fallbackUserId,
                        fallbackTariffType: fallbackTariffType,
                        fallbackStartTime: fallbackStartTime,
                        processedByUserId: processedByUserId);
                }
                catch (InsufficientBalanceException ex)
                {
                    decimal shortfall = ex.RequiredAmount - ex.CurrentBalance;
                    var confirmCash = ShowNotification(
                        $"{ex.Message}\nSố tiền cần thu thêm bằng tiền mặt tại quầy: {shortfall:N0} VNĐ.\n\n" +
                        "Chọn YES để trừ toàn bộ số dư ví còn lại và thu phần thiếu bằng tiền mặt.\n" +
                        "Chọn NO để giữ nguyên phiên chơi (khách nạp thêm tiền vào ví trước).",
                        "Số dư ví không đủ",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmCash != DialogResult.Yes)
                        return;

                    result = _dbBilling.CloseStationSession(
                        _selectedStation.StationId,
                        basePrice,
                        endTime,
                        allowCashSupplement: true,
                        fallbackUserId: fallbackUserId,
                        fallbackTariffType: fallbackTariffType,
                        fallbackStartTime: fallbackStartTime,
                        processedByUserId: processedByUserId);
                }

                string paymentDetail;
                if (result.UserId.HasValue)
                {
                    string acctLabel = string.IsNullOrEmpty(result.Username) ? $"#{result.UserId.Value}" : result.Username;
                    if (result.CashDue > 0m)
                    {
                        paymentDetail = $"\nVí ({acctLabel}): Trừ {result.WalletDeducted:N0}đ. Thu thêm tiền mặt: {result.CashDue:N0} VNĐ!";
                    }
                    else
                    {
                        paymentDetail = $"\nVí ({acctLabel}): Đã trừ {result.WalletDeducted:N0}đ. Còn lại: {result.RemainingBalance ?? 0m:N0} VNĐ.";
                    }
                }
                else
                {
                    paymentDetail = "\nHình thức thanh toán: Tiền mặt tại quầy lễ tân.";
                }

                string receiptText = $"=== HÓA ĐƠN THANH TOÁN (ĐA HÌNH) ===\n" +
                                     $"Máy: {result.StationName} | Gói: {result.TariffDisplayName}\n" +
                                     $"Thời gian: {result.StartTime:HH:mm} - {result.EndTime:HH:mm}\n" +
                                     $"Tiền giờ: {result.PlayCost:N0}đ | F&B: {result.ServiceCost:N0}đ | TỔNG: {result.TotalCost:N0} VNĐ" +
                                     paymentDetail;

                LoadDashboardData();
                lblKetQuaTinhTien.Text = receiptText;

                ShowNotification(
                    $"Kết thúc phiên máy {result.StationName}!\n" +
                    $"Tổng tiền thanh toán: {result.TotalCost:N0} VNĐ.{paymentDetail}",
                    "Thanh toán hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (InvalidOperationException ex)
            {
                ShowNotification(ex.Message, "Không thể đóng máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                ShowNotification($"Lỗi chốt phiên & thanh toán: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBaoTri_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;
            if (!EnsureStaffOrAdminAccess("Chuyển trạng thái bảo trì")) return;

            try
            {
                string nextStatus = _dbBilling.ToggleStationMaintenance(_selectedStation.StationId);
                _selectedStation.Status = nextStatus;
                LoadDashboardData();
            }
            catch (StationInUseException ex)
            {
                ShowNotification(ex.Message, "Máy đang có khách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (InvalidOperationException ex)
            {
                ShowNotification(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                ShowNotification($"Lỗi cập nhật trạng thái bảo trì: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Func<DBBilling, int, string, DialogResult>? _orderDialogLauncher;
        internal void SetOrderDialogLauncher(Func<DBBilling, int, string, DialogResult>? launcher)
        {
            _orderDialogLauncher = launcher;
        }

        public Button GetBtnGoiMon() => btnGoiMon;
        public Label GetLblKetQuaTinhTien() => lblKetQuaTinhTien;
        public ComputerStation? GetSelectedStation() => _selectedStation;
        public string GetBillingResultText() => lblKetQuaTinhTien.Text;

        public void SelectStationById(int stationId)
        {
            var st = _stations.FirstOrDefault(s => s.StationId == stationId);
            if (st != null)
            {
                SelectStation(st);
            }
        }

        public GamingSession? GetActiveSession(int stationId)
        {
            return _activeSessions.TryGetValue(stationId, out var session) ? session : null;
        }

        public bool OrderFoodForStation(int stationId, int serviceId, int quantity, string? note = null)
        {
            if (!_activeSessions.TryGetValue(stationId, out var session))
            {
                throw new InvalidOperationException($"Máy {stationId} hiện không có phiên chơi đang hoạt động để gọi món.");
            }

            bool result = _dbBilling.OrderServiceForSession(session.SessionId, serviceId, quantity, note);
            if (result)
            {
                LoadDashboardData();
                var reloaded = _stations.FirstOrDefault(s => s.StationId == stationId);
                if (reloaded != null && _selectedStation?.StationId == stationId)
                {
                    SelectStation(reloaded);
                }
            }
            return result;
        }

        public void PerformGoiMonClick() => BtnGoiMon_Click(btnGoiMon, EventArgs.Empty);

        private void BtnGoiMon_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;
            if (!EnsureStaffOrAdminAccess("Gọi món F&B cho máy")) return;

            if (!_activeSessions.TryGetValue(_selectedStation.StationId, out var session))
            {
                ShowNotification(
                    $"Máy {_selectedStation.StationName} chưa có phiên chơi đang hoạt động để gọi món!",
                    "Chưa mở máy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult res;
            if (_orderDialogLauncher != null)
            {
                res = _orderDialogLauncher(_dbBilling, session.SessionId, _selectedStation.StationName);
            }
            else
            {
                using var dlg = new FormOrderServiceDialog(_dbBilling, session.SessionId, _selectedStation.StationName);
                res = dlg.ShowDialog(this);
            }

            if (res == DialogResult.OK)
            {
                int stationId = _selectedStation.StationId;
                LoadDashboardData();
                var reloaded = _stations.FirstOrDefault(s => s.StationId == stationId);
                if (reloaded != null)
                {
                    SelectStation(reloaded);
                }
            }
        }
    }
}
