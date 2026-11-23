using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormStationRemoteControl : Form
    {
        private readonly ComputerStation _station;
        private readonly IReadOnlyList<ComputerStation> _allStations;
        private readonly IReadOnlyList<StationZone> _zones;
        private readonly DBBilling _dbBilling;
        private readonly DBStation _dbStation;

        private Action<int, bool, string?>? _lockChangedHandler;
        private Action<int, Image>? _screenshotHandler;
        private Action<int, IReadOnlyList<RunningAppInfo>>? _appsUpdatedHandler;

        public bool StationTransferred { get; private set; }
        public int? TargetStationId { get; private set; }

        public FormStationRemoteControl()
            : this(
                new ComputerStation { StationId = 1, StationName = "PC-01", Status = "InUse", ZoneId = 1 },
                new List<ComputerStation>
                {
                    new() { StationId = 1, StationName = "PC-01", Status = "InUse", ZoneId = 1 },
                    new() { StationId = 2, StationName = "PC-02", Status = "Available", ZoneId = 1 },
                    new() { StationId = 11, StationName = "PC-11", Status = "Available", ZoneId = 2 }
                },
                new List<StationZone>
                {
                    new() { ZoneId = 1, ZoneName = "Dàn Máy Standard", BasePricePerHour = 10000m },
                    new() { ZoneId = 2, ZoneName = "Dàn Máy Master", BasePricePerHour = 13000m },
                    new() { ZoneId = 3, ZoneName = "Dàn Máy Maximum", BasePricePerHour = 15000m }
                },
                new DBBilling(),
                new DBStation())
        {
        }

        public FormStationRemoteControl(
            ComputerStation station,
            IReadOnlyList<ComputerStation> allStations,
            IReadOnlyList<StationZone> zones,
            DBBilling dbBilling,
            DBStation dbStation)
        {
            InitializeComponent();

            _station = station ?? throw new ArgumentNullException(nameof(station));
            _allStations = allStations ?? new List<ComputerStation>();
            _zones = zones ?? new List<StationZone>();
            _dbBilling = dbBilling ?? new DBBilling();
            _dbStation = dbStation ?? new DBStation();

            Load += FormStationRemoteControl_Load;
            FormClosed += FormStationRemoteControl_FormClosed;
        }

        private void FormStationRemoteControl_Load(object? sender, EventArgs e)
        {
            SetupStationHeader();
            SetupLockTab();
            SetupPowerAndAlert();
            SetupTransferTab();
            SetupProcessManagerTab();
            SetupScreenshotTab();
            SubscribeRemoteEvents();

            btnClose.Click += (s, ev) => Close();
        }

        private void SetupStationHeader()
        {
            var zone = _zones.FirstOrDefault(z => z.ZoneId == _station.ZoneId);
            string zoneName = zone?.ZoneName ?? "Standard";
            decimal price = zone?.BasePricePerHour ?? 10000m;

            string statusText = _station.Status switch
            {
                "InUse" => "Đang có khách chơi",
                "Available" => "Đang trống",
                "Maintenance" => "Đang bảo trì",
                _ => _station.Status
            };

            lblStationInfo.Text = $"Máy trạm: {_station.StationName} • Khu vực: {zoneName} ({price:N0} đ/h) • Trạng thái: {statusText}";
            lblSourceInfo.Text = $"Máy nguồn: {_station.StationName} (Khu vực: {zoneName} • {price:N0} đ/h)";
            Text = $"Trung Tâm Điều Khiển & Quản Trị Máy Trạm Từ Xa - {_station.StationName} - Master Game";

            UpdateLockStatusBadge();
        }

        private void UpdateLockStatusBadge()
        {
            bool isLocked = StationRemoteControlService.IsStationLocked(_station.StationId, out string? reason);
            if (isLocked)
            {
                lblStationStatusBadge.Text = "⛔ ĐANG BỊ KHÓA BỞI MÁY TỔNG";
                lblStationStatusBadge.BackColor = Color.FromArgb(220, 38, 38);
                lblStatusFooter.Text = $"Máy {_station.StationName} đang bị máy tổng khóa cưỡng chế. Lý do: {reason}";
                lblStatusFooter.ForeColor = Color.FromArgb(220, 38, 38);
                btnForceLock.Enabled = false;
                btnUnlock.Enabled = true;
            }
            else
            {
                lblStationStatusBadge.Text = "TRẠNG THÁI: BÌNH THƯỜNG";
                lblStationStatusBadge.BackColor = Color.FromArgb(16, 185, 129);
                lblStatusFooter.Text = $"Máy {_station.StationName} hoạt động bình thường.";
                lblStatusFooter.ForeColor = Color.FromArgb(30, 41, 59);
                btnForceLock.Enabled = true;
                btnUnlock.Enabled = false;
            }
        }

        private void SetupLockTab()
        {
            cboLockTemplates.Items.Clear();
            cboLockTemplates.Items.Add("-- Chọn mẫu lý do vi phạm hoặc gõ tay bên dưới --");
            cboLockTemplates.Items.Add("Vi phạm quy định: Hút thuốc lá trong phòng máy lạnh");
            cboLockTemplates.Items.Add("Vi phạm quy định: Gây mất trật tự / chửi bậy / đập phá linh kiện");
            cboLockTemplates.Items.Add("Tài khoản nghi vấn sử dụng phần mềm gian lận (Hack / Cheat)");
            cboLockTemplates.Items.Add("Hết tiền trong tài khoản nhưng chưa thanh toán dịch vụ");
            cboLockTemplates.Items.Add("Tạm khóa theo yêu cầu kiểm tra kỹ thuật phòng máy");
            cboLockTemplates.SelectedIndex = 0;

            cboLockTemplates.SelectedIndexChanged += (s, e) =>
            {
                if (cboLockTemplates.SelectedIndex > 0)
                {
                    txtLockReason.Text = cboLockTemplates.SelectedItem?.ToString() ?? string.Empty;
                }
            };

            btnForceLock.Click += BtnForceLock_Click;
            btnUnlock.Click += BtnUnlock_Click;
        }

        private void BtnForceLock_Click(object? sender, EventArgs e)
        {
            string reason = txtLockReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Vi phạm quy định phòng máy Master Game";
            }

            string operatorName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
            StationRemoteControlService.SendForceLock(_station.StationId, _station.StationName, reason, operatorName);

            UpdateLockStatusBadge();
            MessageBox.Show(
                $"Đã gửi lệnh KHÓA CƯỠNG CHẾ đến máy {_station.StationName} thành công!\nLý do: {reason}",
                "Đã khóa máy con",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void BtnUnlock_Click(object? sender, EventArgs e)
        {
            string operatorName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
            StationRemoteControlService.SendUnlock(_station.StationId, _station.StationName, operatorName);

            UpdateLockStatusBadge();
            MessageBox.Show(
                $"Đã gửi lệnh MỞ KHÓA MÁY đến {_station.StationName} thành công!",
                "Đã mở khóa máy con",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SetupPowerAndAlert()
        {
            btnShutdown.Click += (s, e) =>
            {
                var dr = MessageBox.Show(
                    $"Bạn có chắc chắn muốn TẮT MÁY {_station.StationName} từ xa?\n(Máy con sẽ đếm ngược 10 giây lưu dữ liệu rồi tự tắt)",
                    "Xác nhận tắt máy từ xa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    string operatorName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                    StationRemoteControlService.SendShutdownOrRestart(_station.StationId, _station.StationName, false, 10, operatorName);
                    MessageBox.Show($"Đã gửi lệnh tắt máy đến {_station.StationName}!", "Lệnh hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnRestart.Click += (s, e) =>
            {
                var dr = MessageBox.Show(
                    $"Bạn có chắc chắn muốn KHỞI ĐỘNG LẠI máy {_station.StationName} từ xa?",
                    "Xác nhận khởi động lại",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    string operatorName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                    StationRemoteControlService.SendShutdownOrRestart(_station.StationId, _station.StationName, true, 10, operatorName);
                    MessageBox.Show($"Đã gửi lệnh khởi động lại đến {_station.StationName}!", "Lệnh hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            cboAlertTemplates.Items.Clear();
            cboAlertTemplates.Items.Add("-- Chọn thông báo mẫu nhanh hoặc nhập nội dung bên dưới --");
            foreach (var template in StationRemoteControlService.QuickAlertTemplates)
            {
                cboAlertTemplates.Items.Add(template);
            }
            cboAlertTemplates.SelectedIndex = 0;

            cboAlertTemplates.SelectedIndexChanged += (s, e) =>
            {
                if (cboAlertTemplates.SelectedIndex > 0)
                {
                    txtAlertMessage.Text = cboAlertTemplates.SelectedItem?.ToString() ?? string.Empty;
                }
            };

            btnSendAlert.Click += (s, e) =>
            {
                string message = txtAlertMessage.Text.Trim();
                if (string.IsNullOrWhiteSpace(message))
                {
                    MessageBox.Show("Vui lòng nhập nội dung cảnh báo / thông báo trước khi gửi!", "Thiếu nội dung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool isBroadcast = chkBroadcast.Checked;
                string operatorName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                int targetId = isBroadcast ? 0 : _station.StationId;
                string targetName = isBroadcast ? "Toàn bộ phòng máy" : _station.StationName;

                StationRemoteControlService.SendAlertPopup(targetId, targetName, message, "THÔNG BÁO TỪ QUẦY THU NGÂN", operatorName, isBroadcast);

                string destStr = isBroadcast ? "toàn bộ 30 máy" : $"máy {_station.StationName}";
                MessageBox.Show($"Đã phát thông báo pop-up trực tiếp tới {destStr}!", "Gửi thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void SetupTransferTab()
        {
            cboTargetStation.Items.Clear();
            var availableStations = _allStations
                .Where(s => s.StationId != _station.StationId && string.Equals(s.Status, "Available", StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.StationId)
                .ToList();

            if (availableStations.Count == 0)
            {
                cboTargetStation.Items.Add("-- Hiện không có máy nào khác đang trống --");
                cboTargetStation.SelectedIndex = 0;
                btnTransferStation.Enabled = false;
            }
            else
            {
                foreach (var st in availableStations)
                {
                    var zone = _zones.FirstOrDefault(z => z.ZoneId == st.ZoneId);
                    string zName = zone?.ZoneName ?? "Standard";
                    decimal pr = zone?.BasePricePerHour ?? 10000m;
                    cboTargetStation.Items.Add($"{st.StationName} — {zName} ({pr:N0} đ/h)");
                }
                cboTargetStation.SelectedIndex = 0;
                btnTransferStation.Enabled = true;
            }

            btnTransferStation.Click += (s, e) =>
            {
                if (cboTargetStation.SelectedIndex < 0 || availableStations.Count == 0)
                    return;

                var target = availableStations[cboTargetStation.SelectedIndex];

                var dr = MessageBox.Show(
                    $"Bạn có chắc chắn muốn CHUYỂN TOÀN BỘ PHIÊN CHƠI từ [{_station.StationName}] sang [{target.StationName}] không?\n" +
                    $"• Máy {_station.StationName} sẽ tự động đóng lại.\n" +
                    $"• Máy {target.StationName} sẽ mở phiên chơi mới với đầy đủ số dư và thời gian của khách.",
                    "Xác nhận chuyển máy",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        string opName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                        StationRemoteControlService.SendTransferStation(_station.StationId, _station.StationName, target.StationId, target.StationName, opName);

                        StationTransferred = true;
                        TargetStationId = target.StationId;

                        MessageBox.Show(
                            $"Chuyển phiên chơi từ {_station.StationName} sang {target.StationName} thành công!",
                            "Chuyển máy hoàn tất",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi chuyển máy: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
        }

        private void SetupProcessManagerTab()
        {
            dgvRunningApps.Columns.Clear();
            dgvRunningApps.Columns.Add("DisplayName", "Tên Ứng Dụng / Game");
            dgvRunningApps.Columns.Add("ProcessName", "Tên Tiến Trình (.exe)");
            dgvRunningApps.Columns.Add("MemoryMb", "Bộ Nhớ RAM (MB)");
            dgvRunningApps.Columns.Add("StartTime", "Thời Gian Chạy");
            dgvRunningApps.Columns.Add("AppType", "Phân Loại");

            var colDisplay = dgvRunningApps.Columns["DisplayName"];
            if (colDisplay != null) colDisplay.Width = 240;
            var colProcess = dgvRunningApps.Columns["ProcessName"];
            if (colProcess != null) colProcess.Width = 160;
            var colMemory = dgvRunningApps.Columns["MemoryMb"];
            if (colMemory != null) colMemory.Width = 120;
            var colStart = dgvRunningApps.Columns["StartTime"];
            if (colStart != null) colStart.Width = 140;
            var colType = dgvRunningApps.Columns["AppType"];
            if (colType != null) colType.Width = 110;

            LoadRunningApps();

            btnRefreshApps.Click += (s, e) => LoadRunningApps();

            btnKillApp.Click += (s, e) =>
            {
                if (dgvRunningApps.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một ứng dụng / game trong danh sách để tắt!", "Chưa chọn ứng dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var row = dgvRunningApps.SelectedRows[0];
                string displayName = row.Cells["DisplayName"].Value?.ToString() ?? "Ứng dụng";
                string processName = row.Cells["ProcessName"].Value?.ToString() ?? string.Empty;

                var dr = MessageBox.Show(
                    $"Bạn có chắc chắn muốn CƯỠNG CHẾ TẮT ứng dụng [{displayName}] ({processName}.exe) trên máy {_station.StationName} từ xa?\n" +
                    "Ứng dụng sẽ bị đóng ngay lập tức trên máy con!",
                    "Xác nhận tắt ứng dụng từ xa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dr == DialogResult.Yes)
                {
                    string opName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                    StationRemoteControlService.SendKillApp(_station.StationId, _station.StationName, processName, opName);

                    // Remove from grid
                    dgvRunningApps.Rows.Remove(row);
                    MessageBox.Show($"Đã gửi lệnh ép đóng tiến trình [{processName}.exe] đến máy {_station.StationName}!", "Lệnh hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
        }

        private void LoadRunningApps()
        {
            dgvRunningApps.Rows.Clear();
            var apps = StationRemoteControlService.GetRunningApps(_station.StationId);
            foreach (var app in apps)
            {
                dgvRunningApps.Rows.Add(
                    app.DisplayName,
                    app.ProcessName,
                    $"{app.MemoryMb:N0} MB",
                    app.StartTime.ToString("HH:mm:ss"),
                    app.IsGame ? "🎮 Trò chơi" : "💻 Tiện ích");
            }
        }

        private void SetupScreenshotTab()
        {
            RefreshScreenshotDisplay();

            btnCaptureScreen.Click += (s, e) =>
            {
                string opName = CurrentUserSession.IsLoggedIn ? CurrentUserSession.Username : "Admin";
                StationRemoteControlService.SendCaptureScreenshotRequest(_station.StationId, _station.StationName, opName);
                RefreshScreenshotDisplay();
                MessageBox.Show($"Đã chụp màn hình máy {_station.StationName} thời gian thực!", "Chụp thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnSaveScreenshot.Click += (s, e) =>
            {
                if (picScreenshot.Image == null)
                {
                    MessageBox.Show("Chưa có ảnh chụp màn hình để lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                    FileName = $"Screenshot_{_station.StationName}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    picScreenshot.Image.Save(sfd.FileName, ImageFormat.Png);
                    MessageBox.Show($"Đã lưu ảnh chụp màn hình vào:\n{sfd.FileName}", "Lưu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
        }

        private void RefreshScreenshotDisplay()
        {
            var img = StationRemoteControlService.GetLatestScreenshot(_station.StationId);
            if (img != null)
            {
                picScreenshot.Image?.Dispose();
                picScreenshot.Image = img;
                lblScreenshotMeta.Text = $"Chụp lúc: {DateTime.Now:HH:mm:ss} • 1920x1080 • Vanguard Anti-Cheat: Active";
            }
        }

        private void SubscribeRemoteEvents()
        {
            _lockChangedHandler = (stId, isLocked, reason) =>
            {
                if (stId == _station.StationId && !IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(UpdateLockStatusBadge));
                }
            };
            StationRemoteControlService.OnStationLockStateChanged += _lockChangedHandler;

            _screenshotHandler = (stId, img) =>
            {
                if (stId == _station.StationId && !IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(RefreshScreenshotDisplay));
                }
            };
            StationRemoteControlService.OnScreenshotReceived += _screenshotHandler;

            _appsUpdatedHandler = (stId, apps) =>
            {
                if (stId == _station.StationId && !IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(LoadRunningApps));
                }
            };
            StationRemoteControlService.OnRunningAppsUpdated += _appsUpdatedHandler;
        }

        private void FormStationRemoteControl_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_lockChangedHandler != null)
                StationRemoteControlService.OnStationLockStateChanged -= _lockChangedHandler;

            if (_screenshotHandler != null)
                StationRemoteControlService.OnScreenshotReceived -= _screenshotHandler;

            if (_appsUpdatedHandler != null)
                StationRemoteControlService.OnRunningAppsUpdated -= _appsUpdatedHandler;

            picScreenshot.Image?.Dispose();
        }
    }
}
