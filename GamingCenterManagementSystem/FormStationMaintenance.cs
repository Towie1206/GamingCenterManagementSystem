using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public class StationGearAuditRecord
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string MouseGear { get; set; } = "Razer DeathAdder V3";
        public string KeyboardGear { get; set; } = "DrunkDeer A75";
        public string HeadsetGear { get; set; } = "HyperX Cloud II";
        public string ConditionStatus { get; set; } = "Bình thường";
        public string Notes { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Hộp thoại quản lý bảo trì & tình trạng thiết bị ngoại vi (Gear) cho các máy trạm PC-01 đến PC-30.
    /// </summary>
    public class FormStationMaintenance : Form
    {
        private readonly DBStation _dbStation;
        private readonly HappyHourPricingEngine _pricingEngine = new();
        private readonly List<ComputerStation> _stations = new();
        private readonly Dictionary<int, StationGearAuditRecord> _gearLogs = new();

        private readonly ComboBox _cboStation = new();
        private readonly ComboBox _cboMouse = new();
        private readonly ComboBox _cboKeyboard = new();
        private readonly ComboBox _cboHeadset = new();
        private readonly ComboBox _cboCondition = new();
        private readonly TextBox _txtNote = new();
        private readonly Label _lblHappyHourBanner = new();
        private readonly DataGridView _dgvLogs = new();

        public IReadOnlyDictionary<int, StationGearAuditRecord> GearLogs => _gearLogs;

        public FormStationMaintenance() : this(new DBStation())
        {
        }

        public FormStationMaintenance(DBStation dbStation)
        {
            _dbStation = dbStation ?? throw new ArgumentNullException(nameof(dbStation));

            Text = "🛠️ Quản Lý Bảo Trì & Nhật Ký Linh Kiện Gear (PC-01 - PC-30)";
            Size = new Size(920, 580);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);

            InitializeStations();
            InitializeLayout();
            BindStationCombo();
            RefreshGrid();
            UpdateHappyHourBanner();
        }

        private void InitializeStations()
        {
            var dbList = _dbStation.GetAll();
            if (dbList.Count >= 30)
            {
                _stations.AddRange(dbList.Take(30));
            }
            else
            {
                _stations.AddRange(dbList);
                for (int i = _stations.Count + 1; i <= 30; i++)
                {
                    int zoneId = i <= 10 ? 1 : (i <= 20 ? 2 : 3);
                    _stations.Add(new ComputerStation
                    {
                        StationId = i,
                        StationName = $"PC-{i:D2}",
                        ZoneId = zoneId,
                        IpAddress = $"192.168.1.{100 + i}",
                        Status = "Available"
                    });
                }
            }

            foreach (var st in _stations)
            {
                _gearLogs[st.StationId] = new StationGearAuditRecord
                {
                    StationId = st.StationId,
                    StationName = st.StationName,
                    MouseGear = st.ZoneId == 3 ? "Razer DeathAdder V3" : (st.ZoneId == 2 ? "Endgame Gear XM1r" : "Logitech G102"),
                    KeyboardGear = st.ZoneId == 3 ? "DrunkDeer A75 Rapid Trigger" : (st.ZoneId == 2 ? "Dareu EK87 Pro" : "Dareu EK87"),
                    HeadsetGear = st.ZoneId == 3 ? "HyperX Cloud II" : "Somic G941",
                    ConditionStatus = st.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase) ? "Đang bảo trì" : "Bình thường"
                };
            }
        }

        private void InitializeLayout()
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.FromArgb(24, 30, 54), Padding = new Padding(16, 10, 16, 8) };
            var lblTitle = new Label
            {
                Text = "NHẬT KÝ KIỂM TRA GEAR & KHÓA BẢO TRÌ MÁY TRẠM MASTER GAME",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 10)
            };
            _lblHappyHourBanner.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _lblHappyHourBanner.ForeColor = Color.FromArgb(56, 189, 248);
            _lblHappyHourBanner.AutoSize = true;
            _lblHappyHourBanner.Location = new Point(16, 39);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(_lblHappyHourBanner);

            var grpForm = new GroupBox
            {
                Text = "Cập Nhật Tình Trạng Gear & Trạng Thái Máy",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.Left,
                Width = 330,
                Padding = new Padding(10)
            };

            AddFieldLabel(grpForm, "Chọn máy trạm:", 32);
            _cboStation.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboStation.Font = new Font("Segoe UI", 9.5f);
            _cboStation.Location = new Point(130, 28);
            _cboStation.Width = 180;
            _cboStation.SelectedIndexChanged += (s, e) => LoadSelectedStationGear();

            AddFieldLabel(grpForm, "Chuột (Mouse):", 72);
            _cboMouse.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboMouse.Font = new Font("Segoe UI", 9.5f);
            _cboMouse.Items.AddRange(new object[] { "Razer DeathAdder V3", "Endgame Gear XM1r", "Logitech G Pro X", "Logitech G102" });
            _cboMouse.Location = new Point(130, 68);
            _cboMouse.Width = 180;

            AddFieldLabel(grpForm, "Bàn phím:", 112);
            _cboKeyboard.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboKeyboard.Font = new Font("Segoe UI", 9.5f);
            _cboKeyboard.Items.AddRange(new object[] { "DrunkDeer A75 Rapid Trigger", "Dareu EK87 Pro", "Dareu EK87", "Akko 3068B" });
            _cboKeyboard.Location = new Point(130, 108);
            _cboKeyboard.Width = 180;

            AddFieldLabel(grpForm, "Tai nghe:", 152);
            _cboHeadset.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboHeadset.Font = new Font("Segoe UI", 9.5f);
            _cboHeadset.Items.AddRange(new object[] { "HyperX Cloud II", "HyperX Cloud Alpha", "Somic G941", "Dareu EH722X" });
            _cboHeadset.Location = new Point(130, 148);
            _cboHeadset.Width = 180;

            AddFieldLabel(grpForm, "Trạng thái máy:", 192);
            _cboCondition.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboCondition.Font = new Font("Segoe UI", 9.5f);
            _cboCondition.Items.AddRange(new object[] { "Bình thường", "Cần vệ sinh", "Hỏng gear chờ thay", "Đang bảo trì" });
            _cboCondition.Location = new Point(130, 188);
            _cboCondition.Width = 180;

            AddFieldLabel(grpForm, "Ghi chú lỗi:", 232);
            _txtNote.Font = new Font("Segoe UI", 9.5f);
            _txtNote.Location = new Point(130, 228);
            _txtNote.Width = 180;

            var btnSave = new Button
            {
                Text = "💾 Lưu Tình Trạng Gear",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 144, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(16, 280),
                Size = new Size(294, 42)
            };
            btnSave.Click += (s, e) => SaveCurrentStationGear();

            grpForm.Controls.AddRange(new Control[]
            {
                _cboStation, _cboMouse, _cboKeyboard, _cboHeadset, _cboCondition, _txtNote, btnSave
            });

            _dgvLogs.Dock = DockStyle.Fill;
            _dgvLogs.BackgroundColor = Color.WhiteSmoke;
            _dgvLogs.ReadOnly = true;
            _dgvLogs.AllowUserToAddRows = false;
            _dgvLogs.AllowUserToDeleteRows = false;
            _dgvLogs.AllowUserToResizeColumns = false;
            _dgvLogs.AllowUserToResizeRows = false;
            _dgvLogs.RowHeadersVisible = false;
            _dgvLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(_dgvLogs);
            Controls.Add(grpForm);
            Controls.Add(pnlHeader);
        }

        private static void AddFieldLabel(Control parent, string text, int top)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(16, top)
            });
        }

        private void BindStationCombo()
        {
            _cboStation.DisplayMember = "StationName";
            _cboStation.ValueMember = "StationId";
            _cboStation.DataSource = _stations.ToList();
        }

        private void LoadSelectedStationGear()
        {
            if (_cboStation.SelectedValue is not int stationId || !_gearLogs.TryGetValue(stationId, out var log))
            {
                return;
            }

            _cboMouse.SelectedItem = log.MouseGear;
            if (_cboMouse.SelectedIndex < 0) _cboMouse.SelectedIndex = 0;

            _cboKeyboard.SelectedItem = log.KeyboardGear;
            if (_cboKeyboard.SelectedIndex < 0) _cboKeyboard.SelectedIndex = 0;

            _cboHeadset.SelectedItem = log.HeadsetGear;
            if (_cboHeadset.SelectedIndex < 0) _cboHeadset.SelectedIndex = 0;

            _cboCondition.SelectedItem = log.ConditionStatus;
            if (_cboCondition.SelectedIndex < 0) _cboCondition.SelectedIndex = 0;

            _txtNote.Text = log.Notes;
        }

        public bool ApplyGearMaintenanceStatus(int stationId, string mouse, string keyboard, string headset, string conditionStatus, string notes, out string errorMessage)
        {
            errorMessage = string.Empty;
            var station = _stations.FirstOrDefault(s => s.StationId == stationId);
            if (station == null)
            {
                errorMessage = "Không tìm thấy máy trạm cần cập nhật!";
                return false;
            }

            if (conditionStatus == "Đang bảo trì" && station.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"Máy {station.StationName} đang có khách sử dụng (InUse), không thể khóa bảo trì!";
                return false;
            }

            string targetDbStatus = conditionStatus == "Đang bảo trì" ? "Maintenance" : "Available";
            if (!station.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
            {
                _dbStation.UpdateStatus(stationId, targetDbStatus);
                station.Status = targetDbStatus;
            }

            _gearLogs[stationId] = new StationGearAuditRecord
            {
                StationId = stationId,
                StationName = station.StationName,
                MouseGear = mouse,
                KeyboardGear = keyboard,
                HeadsetGear = headset,
                ConditionStatus = conditionStatus,
                Notes = notes.Trim(),
                UpdatedAt = DateTime.Now
            };

            return true;
        }

        private void SaveCurrentStationGear()
        {
            if (_cboStation.SelectedValue is not int stationId) return;

            string mouse = _cboMouse.SelectedItem?.ToString() ?? "Razer DeathAdder V3";
            string keyboard = _cboKeyboard.SelectedItem?.ToString() ?? "DrunkDeer A75 Rapid Trigger";
            string headset = _cboHeadset.SelectedItem?.ToString() ?? "HyperX Cloud II";
            string condition = _cboCondition.SelectedItem?.ToString() ?? "Bình thường";

            if (!ApplyGearMaintenanceStatus(stationId, mouse, keyboard, headset, condition, _txtNote.Text, out string error))
            {
                MessageBox.Show(error, "Cảnh báo bảo trì", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RefreshGrid();
            string lockNotice = condition == "Đang bảo trì" ? " Máy đã tự động khóa màn hình bảo trì (Maintenance)." : "";
            MessageBox.Show($"Đã cập nhật tình trạng Gear cho {_gearLogs[stationId].StationName}!{lockNotice}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshGrid()
        {
            _dgvLogs.DataSource = _gearLogs.Values
                .OrderBy(x => x.StationId)
                .Select(x => new
                {
                    May = x.StationName,
                    Chuot = x.MouseGear,
                    Phim = x.KeyboardGear,
                    TaiNghe = x.HeadsetGear,
                    TinhTrang = x.ConditionStatus,
                    GhiChu = x.Notes
                })
                .ToList();
        }

        private void UpdateHappyHourBanner()
        {
            var sampleZone = new StationZone { ZoneId = 1, ZoneName = "Standard", BasePricePerHour = 10000m };
            var quote = _pricingEngine.EvaluateZoneQuote(sampleZone, DateTime.Now);
            _lblHappyHourBanner.Text = quote.BadgeText;
        }
    }
}