using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem
{
    public partial class Form1 : Form
    {
        private readonly DBStation _dbStation = new();
        private readonly DBUser _dbUser = new();
        private readonly DBBilling _dbBilling = new();

        private List<ComputerStation> _stations = new();
        private List<StationZone> _zones = new();
        private List<AppUser> _users = new();
        private ComputerStation? _selectedStation = null;
        private readonly Dictionary<int, GamingSession> _activeSessions = new();

        public Form1()
        {
            InitializeComponent();
            SetupNavigation();
            SetupStationControls();
            LoadDashboardData();
        }

        private void SetupNavigation()
        {
            // Navigation to 4 forms assigned to 4 members
            btnNavStation.Click += (s, e) =>
            {
                FormStation form = new FormStation();
                form.OnStationNotification += msg => LoadDashboardData();
                form.Show();
            };

            btnNavMember.Click += (s, e) =>
            {
                FormMember form = new FormMember();
                form.OnBalanceTopUpSuccess += (user, amt) => LoadDashboardData();
                form.OnMemberChanged += msg => LoadDashboardData();
                form.Show();
            };

            btnNavTournament.Click += (s, e) =>
            {
                FormTournament form = new FormTournament();
                form.OnTournamentEvent += msg => LoadDashboardData();
                form.Show();
            };

            btnNavBilling.Click += (s, e) =>
            {
                FormBilling form = new FormBilling();
                form.OnBillingNotification += msg => LoadDashboardData();
                form.Show();
            };

            btnLamMoiSoDo.Click += (s, e) => LoadDashboardData();
        }

        private void SetupStationControls()
        {
            btnMoMay.Click += BtnMoMay_Click;
            btnDongMay.Click += BtnDongMay_Click;
            btnBaoTri.Click += BtnBaoTri_Click;
            cboGoiCuoc.SelectedIndex = 0;
        }

        public void LoadDashboardData()
        {
            try
            {
                _zones = _dbStation.GetAllZones();
                _stations = _dbStation.GetAll();
                _users = _dbUser.GetAll();

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
                cboKhachHang.SelectedIndex = 0;

                RenderStationGrid();
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

            foreach (var station in _stations)
            {
                var zone = _zones.FirstOrDefault(z => z.ZoneId == station.ZoneId);
                string zoneName = zone?.ZoneName ?? $"Khu {station.ZoneId}";

                Button btn = new Button
                {
                    Width = 135,
                    Height = 110,
                    Margin = new Padding(10),
                    Tag = station,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                btn.FlatAppearance.BorderSize = 2;

                // Color coding according to project specification
                switch (station.Status)
                {
                    case "InUse":
                        btn.BackColor = Color.FromArgb(255, 178, 102); // Đang chơi - Cam nhạt
                        btn.FlatAppearance.BorderColor = Color.FromArgb(230, 81, 0);
                        btn.Text = $"🖥️ {station.StationName}\n({zoneName})\n\n[ĐANG CHƠI]";
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
                flpDanhSachMay.Controls.Add(btn);
            }

            flpDanhSachMay.ResumeLayout();

            // Reselect current station if still present
            if (_selectedStation != null)
            {
                var refreshed = _stations.FirstOrDefault(s => s.StationId == _selectedStation.StationId);
                if (refreshed != null) SelectStation(refreshed);
            }
            else if (_stations.Count > 0)
            {
                SelectStation(_stations[0]);
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
            decimal price = zone?.BasePricePerHour ?? 8000;

            lblMayDangChon.Text = $"🖥️ Máy Trạm: {station.StationName}";
            lblTrangThaiMay.Text = $"Trạng thái: {station.Status}";
            lblKhuVucMay.Text = $"Khu vực: {zone?.ZoneName ?? "Mặc định"}";
            lblGiaGio.Text = $"Đơn giá: {price:N0} VNĐ/h";

            // Visual indicator & Session state display
            switch (station.Status)
            {
                case "InUse":
                    lblTrangThaiMay.ForeColor = Color.OrangeRed;
                    btnMoMay.Enabled = false;
                    btnDongMay.Enabled = true;
                    btnBaoTri.Enabled = false;

                    if (_activeSessions.TryGetValue(station.StationId, out var active))
                    {
                        var elapsed = DateTime.Now - active.StartTime;
                        string cust = "Khách vãng lai";
                        if (active.UserId.HasValue)
                        {
                            var u = _users.FirstOrDefault(x => x.UserId == active.UserId.Value);
                            if (u != null) cust = $"{u.Username} - {u.FullName} (Số dư ví: {u.Balance:N0}đ)";
                        }

                        lblKetQuaTinhTien.Text = $"🖥️ Phiên chơi đang diễn ra:\n" +
                                                 $"Bắt đầu lúc: {active.StartTime:HH:mm:ss} (Đã chơi: {(int)elapsed.TotalHours}h {elapsed.Minutes}m)\n" +
                                                 $"Gói cước: {active.TariffType} | Đơn giá: {price:N0}đ/h\n" +
                                                 $"Khách hàng: {cust}\n" +
                                                 $"Trạng thái: Đang tính cước thời gian thực.";

                        // Select active user in combo if applicable
                        if (active.UserId.HasValue)
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
                    btnBaoTri.Enabled = true;
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
                    btnMoMay.Enabled = true;
                    btnDongMay.Enabled = false;
                    btnBaoTri.Enabled = true;
                    btnBaoTri.Text = "🛠️ CHUYỂN BẢO TRÌ";
                    lblKetQuaTinhTien.Text = $"Máy {station.StationName} đang SẴN SÀNG đón khách. Chọn khách hàng & gói cước rồi bấm MỞ MÁY.";
                    break;
            }
        }

        private void BtnMoMay_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;

            if (_selectedStation.Status != "Available")
            {
                MessageBox.Show("Máy hiện không ở trạng thái sẵn sàng để mở!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check member balance if member is selected
            int? userId = null;
            string customerName = "Khách vãng lai";
            if (cboKhachHang.SelectedIndex > 0 && _users.Count >= cboKhachHang.SelectedIndex)
            {
                var member = _users[cboKhachHang.SelectedIndex - 1];
                if (member.Balance <= 0)
                {
                    MessageBox.Show(
                        $"Hội viên {member.FullName} ({member.Username}) có số dư ví là 0 VNĐ!\n" +
                        $"Vui lòng nạp thêm tiền vào ví tại màn hình Quản lý hội viên trước khi mở máy.",
                        "Số dư ví không đủ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
                userId = member.UserId;
                customerName = $"{member.Username} - {member.FullName}";
            }

            string plan = cboGoiCuoc.SelectedItem?.ToString() ?? "Hourly";
            string tariffType = cboGoiCuoc.SelectedIndex switch
            {
                1 => "NightCombo",
                2 => "Tournament",
                _ => "Hourly"
            };

            var session = new GamingSession
            {
                StationId = _selectedStation.StationId,
                UserId = userId,
                StartTime = DateTime.Now,
                TariffType = tariffType,
                PlayCost = 0,
                ServiceCost = 0,
                TotalCost = 0,
                Status = "Active"
            };

            _dbBilling.StartSession(session);
            _activeSessions[_selectedStation.StationId] = session;
            _selectedStation.Status = "InUse";
            _dbStation.UpdateStatus(_selectedStation.StationId, "InUse");

            lblKetQuaTinhTien.Text = $"Phiên chơi bắt đầu lúc: {session.StartTime:HH:mm:ss}\n" +
                                     $"Máy: {_selectedStation.StationName} | Gói: {plan}\n" +
                                     $"Khách hàng: {customerName}\n" +
                                     $"Trạng thái: Đang tính giờ theo thời gian thực.";

            MessageBox.Show($"Đã mở máy {_selectedStation.StationName} thành công cho khách ({customerName})!", "Mở máy thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RenderStationGrid();
        }

        private void BtnDongMay_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;

            DateTime endTime = DateTime.Now;
            DateTime startTime;
            int? userId = null;
            string tariffType = "Hourly";
            int sessionId = 0;

            if (_activeSessions.TryGetValue(_selectedStation.StationId, out var active))
            {
                startTime = active.StartTime;
                userId = active.UserId;
                tariffType = active.TariffType;
                sessionId = active.SessionId;
            }
            else
            {
                var dbActive = _dbBilling.GetActiveSession(_selectedStation.StationId);
                if (dbActive != null)
                {
                    startTime = dbActive.StartTime;
                    userId = dbActive.UserId;
                    tariffType = dbActive.TariffType;
                    sessionId = dbActive.SessionId;
                }
                else
                {
                    startTime = endTime.AddHours(-1.5);
                    tariffType = cboGoiCuoc.SelectedIndex switch { 1 => "NightCombo", 2 => "Tournament", _ => "Hourly" };
                    if (cboKhachHang.SelectedIndex > 0 && _users.Count >= cboKhachHang.SelectedIndex)
                    {
                        userId = _users[cboKhachHang.SelectedIndex - 1].UserId;
                    }
                }
            }

            var zone = _zones.FirstOrDefault(z => z.ZoneId == _selectedStation.ZoneId);
            decimal basePrice = zone?.BasePricePerHour ?? 8000;

            // POLYMORPHIC BILLING CALCULATION (Engine Đa hình Chapter 3.3 & 6.2):
            TariffCalculatorBase calculator = tariffType switch
            {
                "NightCombo" => new NightComboTariff(),
                "Tournament" => new TournamentTariff(),
                _ => new HourlyTariff()
            };

            // Polymorphic method invocation:
            decimal playCost = calculator.CalculateCost(startTime, endTime, basePrice);
            decimal serviceCost = 15000; // Simulated F&B drink/snack
            decimal totalCost = playCost + serviceCost;

            if (sessionId > 0)
            {
                _dbBilling.CompleteSession(sessionId, endTime, playCost, serviceCost, totalCost);
            }
            else
            {
                _dbBilling.AddSession(new GamingSession
                {
                    StationId = _selectedStation.StationId,
                    UserId = userId,
                    StartTime = startTime,
                    EndTime = endTime,
                    TariffType = tariffType,
                    PlayCost = playCost,
                    ServiceCost = serviceCost,
                    TotalCost = totalCost,
                    Status = "Completed"
                });
            }

            _activeSessions.Remove(_selectedStation.StationId);
            _selectedStation.Status = "Available";
            _dbStation.UpdateStatus(_selectedStation.StationId, "Available");

            // Deduct from member balance if registered user
            string paymentDetail = "";
            if (userId.HasValue)
            {
                var member = _dbUser.GetById(userId.Value) ?? _users.FirstOrDefault(u => u.UserId == userId.Value);
                if (member != null)
                {
                    if (member.Balance >= totalCost && _dbUser.DeductBalance(member.UserId, totalCost))
                    {
                        paymentDetail = $"\nVí hội viên ({member.Username}): Đã trừ {totalCost:N0} VNĐ. Số dư ví còn lại: {(member.Balance - totalCost):N0} VNĐ.";
                    }
                    else
                    {
                        var latest = _dbUser.GetById(member.UserId) ?? member;
                        decimal walletUsed = latest.Balance;
                        if (walletUsed > 0 && !_dbUser.DeductBalance(latest.UserId, walletUsed))
                        {
                            walletUsed = 0m;
                        }
                        decimal cashDue = totalCost - walletUsed;
                        paymentDetail = $"\nVí hội viên ({latest.Username}): Đã trừ toàn bộ {walletUsed:N0} VNĐ trong ví. Cần thu thêm tiền mặt: {cashDue:N0} VNĐ!";
                    }
                }
            }
            else
            {
                paymentDetail = "\nHình thức thanh toán: Tiền mặt tại quầy lễ tân.";
            }

            lblKetQuaTinhTien.Text = $"=== HÓA ĐƠN THANH TOÁN (ĐA HÌNH) ===\n" +
                                     $"Máy: {_selectedStation.StationName} | Gói: {calculator.TariffName}\n" +
                                     $"Bắt đầu: {startTime:HH:mm} - Kết thúc: {endTime:HH:mm}\n" +
                                     $"Tiền giờ chơi: {playCost:N0} VNĐ\n" +
                                     $"Dịch vụ F&B: {serviceCost:N0} VNĐ\n" +
                                     $"TỔNG CỘNG: {totalCost:N0} VNĐ" + paymentDetail;

            MessageBox.Show(
                $"Kết thúc phiên máy {_selectedStation.StationName}!\n" +
                $"Tổng tiền thanh toán: {totalCost:N0} VNĐ.\n{paymentDetail}",
                "Thanh toán hoàn tất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            LoadDashboardData();
        }

        private void BtnBaoTri_Click(object? sender, EventArgs e)
        {
            if (_selectedStation == null) return;

            string targetStatus = (_selectedStation.Status == "Maintenance") ? "Available" : "Maintenance";
            _selectedStation.Status = targetStatus;
            _dbStation.UpdateStatus(_selectedStation.StationId, targetStatus);

            LoadDashboardData();
        }
    }
}
