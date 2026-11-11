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
        private ComputerStation? _selectedStation = null;
        private readonly Dictionary<int, GamingSession> _activeSessions = new();
        private readonly Dictionary<int, Button> _stationButtons = new();
        private readonly System.Windows.Forms.Timer _realtimeTimer = new();

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
            SetupNavigation();
            SetupStationControls();
            SetupRealtimeTimer();
            LoadDashboardData();
            ApplyRolePermissions();
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
                if (!EnsureStaffOrAdminAccess("Quản lý giải đấu LAN"))
                    return;

                FormTournament form = new FormTournament();
                form.OnTournamentEvent += msg => LoadDashboardData();
                form.Show(this);
            };

            btnNavBilling.Click += (s, e) =>
            {
                if (CurrentUserSession.IsLoggedIn && !CurrentUserSession.IsAdmin)
                {
                    MessageBox.Show(
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

            btnDangXuat.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
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

        private void SetupStationControls()
        {
            btnMoMay.Click += BtnMoMay_Click;
            btnDongMay.Click += BtnDongMay_Click;
            btnBaoTri.Click += BtnBaoTri_Click;
            cboGoiCuoc.SelectedIndex = 0;
        }

        private void SetupRealtimeTimer()
        {
            _realtimeTimer.Interval = 1000;
            _realtimeTimer.Tick += (s, e) => UpdateRealtimeDisplay();
            _realtimeTimer.Start();

            FormClosed += (s, e) =>
            {
                _realtimeTimer.Stop();
                _realtimeTimer.Dispose();
            };
        }

        private bool EnsureStaffOrAdminAccess(string moduleName)
        {
            if (!CurrentUserSession.IsLoggedIn || CurrentUserSession.CanManageOperations)
                return true;

            MessageBox.Show(
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
                btnMoMay.Enabled = false;
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

                // Populate users in combo
                cboKhachHang.Items.Clear();
                cboKhachHang.Items.Add("Khách vãng lai (Tiền mặt)");
                foreach (var user in _users)
                {
                    cboKhachHang.Items.Add($"{user.Username} - {user.FullName} ({user.Balance:N0}đ)");
                }

                if (CurrentUserSession.IsMember)
                {
                    int memberIdx = _users.FindIndex(u => u.UserId == CurrentUserSession.UserId);
                    cboKhachHang.SelectedIndex = memberIdx >= 0 ? memberIdx + 1 : 0;
                }
                else
                {
                    cboKhachHang.SelectedIndex = 0;
                }

                RenderStationGrid();
                ApplyRolePermissions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu phòng máy: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Section 4.2 in Project Roadmap: Sơ đồ phòng máy trực quan đổi màu Realtime
        private void RenderStationGrid()
        {
            flpDanhSachMay.SuspendLayout();
            flpDanhSachMay.Controls.Clear();
            _stationButtons.Clear();

            foreach (var station in _stations)
            {
                var zone = _zones.FirstOrDefault(z => z.ZoneId == station.ZoneId);
                string zoneName = zone?.ZoneName ?? $"Khu {station.ZoneId}";

                Button btn = new Button
                {
                    Width = 145,
                    Height = 118,
                    Margin = new Padding(10),
                    Tag = station,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                btn.FlatAppearance.BorderSize = 2;

                switch (station.Status)
                {
                    case "InUse":
                        btn.BackColor = Color.FromArgb(255, 178, 102); // Đang chơi - Cam nhạt
                        btn.FlatAppearance.BorderColor = Color.FromArgb(230, 81, 0);
                        btn.Text = BuildInUseButtonText(station, zoneName);
                        break;
                    case "Maintenance":
                        btn.BackColor = Color.FromArgb(255, 153, 153); // Bảo trì - Đỏ nhạt
                        btn.FlatAppearance.BorderColor = Color.FromArgb(178, 34, 34);
                        btn.Text = $"🛠️ {station.StationName}\n({zoneName})\n\n[BẢO TRÌ]";
                        break;
                    case "TournamentLocked":
                        btn.BackColor = Color.FromArgb(255, 230, 128); // Khóa giải - Vàng kim
                        btn.FlatAppearance.BorderColor = Color.FromArgb(184, 134, 11);
                        btn.Text = $"🏆 {station.StationName}\n({zoneName})\n\n[GIẢI ĐẤU]";
                        break;
                    case "Available":
                    default:
                        btn.BackColor = Color.FromArgb(178, 236, 178); // Sẵn sàng - Xanh lá tươi
                        btn.FlatAppearance.BorderColor = Color.FromArgb(34, 139, 34);
                        btn.Text = $"🟢 {station.StationName}\n({zoneName})\n\n[SẴN SÀNG]";
                        break;
                }

                btn.Click += StationButton_Click;
                _stationButtons[station.StationId] = btn;
                flpDanhSachMay.Controls.Add(btn);
            }

            flpDanhSachMay.ResumeLayout();

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

        private string BuildInUseButtonText(ComputerStation station, string zoneName)
        {
            if (_activeSessions.TryGetValue(station.StationId, out var active))
            {
                TimeSpan elapsed = DateTime.Now - active.StartTime;
                if (elapsed < TimeSpan.Zero)
                    elapsed = TimeSpan.Zero;

                string elapsedText = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                return $"🖥️ {station.StationName}\n({zoneName})\n⏱️ {elapsedText}\n[ĐANG CHƠI]";
            }

            return $"🖥️ {station.StationName}\n({zoneName})\n\n[ĐANG CHƠI]";
        }

        private string BuildActiveSessionRealtimeText(ComputerStation station, GamingSession active, decimal basePrice)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - active.StartTime;
            if (elapsed < TimeSpan.Zero)
                elapsed = TimeSpan.Zero;

            string cust = "Khách vãng lai";
            string memberTier = "Standard";
            if (CurrentUserSession.IsMember && active.UserId != CurrentUserSession.UserId)
            {
                cust = "Đang có khách sử dụng";
            }
            else if (active.UserId.HasValue)
            {
                var u = _users.FirstOrDefault(x => x.UserId == active.UserId.Value);
                if (u != null)
                {
                    cust = $"{u.Username} ({u.MemberTier} | Ví: {u.Balance:N0}đ)";
                    memberTier = u.MemberTier;
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
                lblKetQuaTinhTien.Text = BuildActiveSessionRealtimeText(_selectedStation, selectedActive, price);
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
            _selectedStation = station;
            var zone = _zones.FirstOrDefault(z => z.ZoneId == station.ZoneId);
            decimal price = zone?.BasePricePerHour ?? 8000m;
            bool canOperateStations = !CurrentUserSession.IsLoggedIn || CurrentUserSession.CanManageOperations;

            lblMayDangChon.Text = $"🖥️ Máy Trạm: {station.StationName}";
            lblTrangThaiMay.Text = $"Trạng thái: {station.Status}";
            lblKhuVucMay.Text = $"Khu vực: {zone?.ZoneName ?? "Mặc định"}";
            lblGiaGio.Text = $"Đơn giá: {price:N0} VNĐ/h";

            switch (station.Status)
            {
                case "InUse":
                    lblTrangThaiMay.ForeColor = Color.OrangeRed;
                    btnMoMay.Enabled = false;
                    btnDongMay.Enabled = canOperateStations;
                    btnBaoTri.Enabled = false;

                    if (_activeSessions.TryGetValue(station.StationId, out var active))
                    {
                        lblKetQuaTinhTien.Text = BuildActiveSessionRealtimeText(station, active, price);

                        if (active.UserId.HasValue && canOperateStations)
                        {
                            int uIdx = _users.FindIndex(u => u.UserId == active.UserId.Value);
                            if (uIdx >= 0) cboKhachHang.SelectedIndex = uIdx + 1;
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
                    btnDongMay.Enabled = false;
                    btnBaoTri.Enabled = canOperateStations;
                    btnBaoTri.Text = "🟢 MỞ LẠI SẴN SÀNG";
                    lblKetQuaTinhTien.Text = $"Máy {station.StationName} đang trong chế độ BẢO TRÌ kỹ thuật.";
                    break;

                case "TournamentLocked":
                    lblTrangThaiMay.ForeColor = Color.DarkGoldenrod;
                    btnMoMay.Enabled = false;
                    btnDongMay.Enabled = false;
                    btnBaoTri.Enabled = false;
                    lblKetQuaTinhTien.Text = $"Máy {station.StationName} đang KHÓA phục vụ giải đấu LAN Party.";
                    break;

                case "Available":
                default:
                    lblTrangThaiMay.ForeColor = Color.Green;
                    btnMoMay.Enabled = canOperateStations;
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

        private void BtnMoMay_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;
            if (!EnsureStaffOrAdminAccess("Mở máy cho khách")) return;

            int? userId = null;
            string customerName = "Khách vãng lai";
            if (cboKhachHang.SelectedIndex > 0 && _users.Count >= cboKhachHang.SelectedIndex)
            {
                var member = _users[cboKhachHang.SelectedIndex - 1];
                userId = member.UserId;
                customerName = $"{member.Username} - {member.FullName}";
            }

            string plan = cboGoiCuoc.SelectedItem?.ToString() ?? "Hourly";
            string tariffType = ResolveSelectedTariffType();

            if (string.Equals(tariffType, "MemberTier", StringComparison.OrdinalIgnoreCase) && !userId.HasValue)
            {
                MessageBox.Show(
                    "Gói cước Ưu đãi hội viên (MemberTier) chỉ áp dụng khi chọn tài khoản hội viên!",
                    "Chọn tài khoản hội viên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var zone = _zones.FirstOrDefault(z => z.ZoneId == _selectedStation.ZoneId);
            decimal basePrice = zone?.BasePricePerHour ?? 8000m;
            decimal minimumRequired = Math.Round(basePrice * 0.1m, 0);

            try
            {
                var session = _dbBilling.OpenStationSession(
                    _selectedStation.StationId,
                    userId,
                    tariffType,
                    minimumRequiredBalance: minimumRequired);

                _activeSessions[_selectedStation.StationId] = session;
                _selectedStation.Status = "InUse";

                lblKetQuaTinhTien.Text = $"Phiên chơi bắt đầu lúc: {session.StartTime:HH:mm:ss}\n" +
                                         $"Máy: {_selectedStation.StationName} | Gói: {plan}\n" +
                                         $"Khách hàng: {customerName}\n" +
                                         $"Trạng thái: Đang tính giờ theo thời gian thực.";

                MessageBox.Show($"Đã mở máy {_selectedStation.StationName} thành công cho khách ({customerName})!", "Mở máy thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDashboardData();
            }
            catch (StationInUseException ex)
            {
                MessageBox.Show(ex.Message, "Máy đang được sử dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (InsufficientBalanceException ex)
            {
                MessageBox.Show(
                    $"{ex.Message}\nVui lòng nạp thêm tiền vào ví tại màn hình Quản lý hội viên trước khi mở máy.",
                    "Số dư ví không đủ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể mở máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở máy: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDongMay_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;
            if (!EnsureStaffOrAdminAccess("Đóng máy & thanh toán")) return;

            int? fallbackUserId = null;
            if (cboKhachHang.SelectedIndex > 0 && _users.Count >= cboKhachHang.SelectedIndex)
            {
                fallbackUserId = _users[cboKhachHang.SelectedIndex - 1].UserId;
            }

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
                    var confirmCash = MessageBox.Show(
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

                _activeSessions.Remove(_selectedStation.StationId);
                _selectedStation.Status = "Available";

                lblKetQuaTinhTien.Text = receiptText;

                MessageBox.Show(
                    $"Kết thúc phiên máy {result.StationName}!\n" +
                    $"Tổng tiền thanh toán: {result.TotalCost:N0} VNĐ.{paymentDetail}",
                    "Thanh toán hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadDashboardData();
                lblKetQuaTinhTien.Text = receiptText;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể đóng máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chốt phiên & thanh toán: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(ex.Message, "Máy đang có khách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật trạng thái bảo trì: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
