using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormStation : Form
    {
        private readonly DBStation _dbStation = new();
        private List<ComputerStation> _stationList = new();
        private List<StationZone> _zoneList = new();

        private static readonly Regex Ipv4Regex = new(
            @"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$",
            RegexOptions.Compiled);

        // Delegate / Action notification (được Form1 lắng nghe tự động cập nhật Dashboard)
        public Action<string>? OnStationNotification;

        public FormStation()
        {
            InitializeComponent();
            SetupEvents();
            LoadData();
        }

        private void SetupEvents()
        {
            Load += (s, e) => LoadData();

            // Sự kiện quản lý Máy trạm
            btnThemMay.Click += BtnThemMay_Click;
            btnSuaMay.Click += BtnSuaMay_Click;
            btnXoaMay.Click += BtnXoaMay_Click;
            btnBaoTri.Click += BtnBaoTri_Click;
            btnMoMay.Click += BtnMoMay_Click;
            btnLamMoi.Click += BtnLamMoi_Click;
            btnApDungLoc.Click += (s, e) => ApplyFilter();
            dgvMayTram.SelectionChanged += DgvMayTram_SelectionChanged;

            // Sự kiện Chuyển máy
            btnChuyenMay.Click += BtnChuyenMay_Click;

            // Sự kiện quản lý Khu vực
            btnThemZone.Click += BtnThemZone_Click;
            btnSuaZone.Click += BtnSuaZone_Click;
            btnXoaZone.Click += BtnXoaZone_Click;
            btnLamMoiZone.Click += BtnLamMoiZone_Click;
            dgvKhuVuc.SelectionChanged += DgvKhuVuc_SelectionChanged;
        }

        private void LoadData()
        {
            try
            {
                _zoneList = _dbStation.GetAllZones();
                _stationList = _dbStation.GetAll();

                // Nạp ComboBox Khu vực (set DisplayMember/ValueMember TRƯỚC DataSource)
                cboKhuVuc.DisplayMember = "ZoneName";
                cboKhuVuc.ValueMember = "ZoneId";
                cboKhuVuc.DataSource = _zoneList.ToList();

                // Nạp ComboBox Lọc Khu vực
                var filterZones = new List<string> { "Tất cả khu vực" };
                filterZones.AddRange(_zoneList.Select(z => z.ZoneName));
                cboLocKhuVuc.DataSource = filterZones;

                // Nạp ComboBox Lọc Trạng thái
                cboLocTrangThai.DataSource = new List<string>
                {
                    "Tất cả trạng thái",
                    "Máy trống (Available)",
                    "Đang chơi (InUse)",
                    "Bảo trì (Maintenance)",
                    "Đấu trường (TournamentLocked)"
                };

                BindGrid(_stationList);
                BindZoneGrid();
                BindTransferTargets();
                UpdateStatistics();

                if (dgvMayTram.Rows.Count == 0)
                {
                    ResetStationInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu máy trạm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindGrid(List<ComputerStation> stations)
        {
            var displayData = (from s in stations
                               join z in _zoneList on s.ZoneId equals z.ZoneId into zoneGroup
                               from zg in zoneGroup.DefaultIfEmpty()
                               select new
                               {
                                   MaMay = s.StationId,
                                   TenMay = s.StationName,
                                   KhuVuc = zg?.ZoneName ?? $"Zone {s.ZoneId}",
                                   GiaMoiGio = zg != null ? $"{zg.BasePricePerHour:N0} VNĐ" : "N/A",
                                   DiaChiIP = s.IpAddress,
                                   TrangThai = s.Status
                               }).ToList();

            dgvMayTram.DataSource = displayData;

            if (dgvMayTram.Columns["MaMay"] is DataGridViewColumn col1) col1.HeaderText = "Mã Máy";
            if (dgvMayTram.Columns["TenMay"] is DataGridViewColumn col2) col2.HeaderText = "Tên Máy Trạm";
            if (dgvMayTram.Columns["KhuVuc"] is DataGridViewColumn col3) col3.HeaderText = "Khu Vực";
            if (dgvMayTram.Columns["GiaMoiGio"] is DataGridViewColumn col4) col4.HeaderText = "Giá / Giờ";
            if (dgvMayTram.Columns["DiaChiIP"] is DataGridViewColumn col5) col5.HeaderText = "Địa Chỉ IP";
            if (dgvMayTram.Columns["TrangThai"] is DataGridViewColumn col6) col6.HeaderText = "Trạng Thái";

            if (displayData.Count == 0)
            {
                ResetStationInputs();
            }
        }

        private void UpdateStatistics()
        {
            int total = _stationList.Count;
            int available = _stationList.Count(s => s.Status.Equals("Available", StringComparison.OrdinalIgnoreCase));
            int inUse = _stationList.Count(s => s.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase));
            int maintenance = _stationList.Count(s => s.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase));
            int tournament = _stationList.Count(s => s.Status.Equals("TournamentLocked", StringComparison.OrdinalIgnoreCase));

            lblTongSoMay.Text = $"🖥️ Tổng: {total}";
            lblSoMayTrong.Text = $"🟢 Sẵn sàng: {available}";
            lblSoMayDangChoi.Text = $"🎮 Đang chơi: {inUse}";
            lblSoMayBaoTri.Text = $"🛠️ Bảo trì: {maintenance} | 🏆 Giải: {tournament}";
        }

        private void ApplyFilter()
        {
            string selectedZone = cboLocKhuVuc.SelectedItem?.ToString() ?? "Tất cả khu vực";
            string selectedStatusText = cboLocTrangThai.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            string targetStatus = selectedStatusText switch
            {
                "Máy trống (Available)" => "Available",
                "Đang chơi (InUse)" => "InUse",
                "Bảo trì (Maintenance)" => "Maintenance",
                "Đấu trường (TournamentLocked)" => "TournamentLocked",
                _ => "ALL"
            };

            var query = _stationList.AsEnumerable();

            if (selectedZone != "Tất cả khu vực")
            {
                var zone = _zoneList.FirstOrDefault(z => z.ZoneName == selectedZone);
                if (zone != null)
                {
                    query = query.Where(s => s.ZoneId == zone.ZoneId);
                }
            }

            if (targetStatus != "ALL")
            {
                query = query.Where(s => s.Status.Equals(targetStatus, StringComparison.OrdinalIgnoreCase));
            }

            BindGrid(query.ToList());
        }

        // =========================================================================
        // AN TOÀN TRẠNG THÁI UI & RESET Ô NHẬP MÁY TRẠM
        // =========================================================================
        private void ResetStationInputs()
        {
            txtStationId.Clear();
            txtStationName.Clear();
            txtIpAddress.Clear();

            cboKhuVuc.Enabled = true;
            cboTrangThai.Enabled = true;

            if (cboKhuVuc.Items.Count > 0)
            {
                cboKhuVuc.SelectedIndex = 0;
            }
            cboTrangThai.SelectedItem = "Available";

            UpdateTransferControlsState(null);
        }

        private void BtnLamMoi_Click(object? sender, EventArgs e)
        {
            LoadData();
            dgvMayTram.ClearSelection();
            ResetStationInputs();
        }

        private void DgvMayTram_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvMayTram.CurrentRow == null ||
                dgvMayTram.CurrentRow.DataBoundItem == null ||
                dgvMayTram.CurrentRow.Cells["MaMay"]?.Value == null)
            {
                ResetStationInputs();
                return;
            }

            int stationId = Convert.ToInt32(dgvMayTram.CurrentRow.Cells["MaMay"].Value);
            var station = _stationList.FirstOrDefault(s => s.StationId == stationId);
            if (station == null)
            {
                ResetStationInputs();
                return;
            }

            txtStationId.Text = station.StationId.ToString();
            txtStationName.Text = station.StationName;
            txtIpAddress.Text = station.IpAddress;
            cboKhuVuc.SelectedValue = station.ZoneId;
            cboTrangThai.SelectedItem = station.Status;

            bool isBusy = station.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase);
            cboKhuVuc.Enabled = !isBusy;
            cboTrangThai.Enabled = !isBusy;

            UpdateTransferControlsState(station);
        }

        // =========================================================================
        // VALIDATE TÊN MÁY, ĐỊA CHỈ IPv4 VÀ CHẶN TRẠNG THÁI BẬN (InUse / TournamentLocked)
        // =========================================================================
        private bool CheckStationCanBeModified(int stationId, string actionName)
        {
            var station = _stationList.FirstOrDefault(s => s.StationId == stationId);
            if (station == null) return true;

            if (station.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
            {
                var ex = new StationInUseException(station.StationId, station.StationName);
                MessageBox.Show(
                    $"🚫 KHÔNG THỂ {actionName.ToUpper()}!\n\n{ex.Message}\nVui lòng kết thúc phiên chơi của khách trước khi thực hiện thao tác này.",
                    "Cảnh báo hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            if (station.Status.Equals("TournamentLocked", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"🚫 KHÔNG THỂ {actionName.ToUpper()}!\n\nMáy trạm [{station.StationName}] đang bị khóa cho giải đấu LAN (TournamentLocked).\nVui lòng giải phóng máy tại màn hình Giải Đấu LAN!",
                    "Cảnh báo hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool ValidateStationInput(int excludeStationId, out string stationName, out string ipAddress, out int zoneId, out string status)
        {
            stationName = txtStationName.Text.Trim();
            ipAddress = txtIpAddress.Text.Trim();
            zoneId = cboKhuVuc.SelectedValue is int zId ? zId : 0;
            status = cboTrangThai.SelectedItem?.ToString() ?? "Available";

            if (string.IsNullOrEmpty(stationName) || stationName.Length > 50)
            {
                MessageBox.Show("Tên máy trạm không được để trống và tối đa 50 ký tự!", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStationName.Focus();
                return false;
            }

            if (_dbStation.IsStationNameExists(stationName, excludeStationId))
            {
                MessageBox.Show($"Tên máy trạm [{stationName}] đã tồn tại! Vui lòng đặt tên khác.", "Trùng tên máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStationName.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(ipAddress) || !Ipv4Regex.IsMatch(ipAddress))
            {
                MessageBox.Show("Địa chỉ IP không đúng định dạng IPv4 (ví dụ: 192.168.1.105, mỗi số từ 0 đến 255)!", "Sai định dạng IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIpAddress.Focus();
                return false;
            }

            if (zoneId <= 0)
            {
                MessageBox.Show("Vui lòng chọn khu vực hợp lệ cho máy trạm!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể đặt trực tiếp trạng thái InUse tại đây! Vui lòng mở máy cho khách tại Bảng điều khiển chính để tạo phiên chơi.", "Trạng thái không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnThemMay_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateStationInput(0, out string stationName, out string ipAddress, out int zoneId, out string status))
                {
                    return;
                }

                var newStation = new ComputerStation
                {
                    StationName = stationName,
                    ZoneId = zoneId,
                    IpAddress = ipAddress,
                    Status = status
                };

                if (_dbStation.Add(newStation))
                {
                    MessageBox.Show($"Thêm máy trạm {newStation.StationName} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Đã thêm máy: {newStation.StationName}");
                    LoadData();
                    ResetStationInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi thêm máy trạm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSuaMay_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtStationId.Text))
                {
                    MessageBox.Show("Vui lòng chọn máy trạm cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = Convert.ToInt32(txtStationId.Text);

                if (!CheckStationCanBeModified(id, "chỉnh sửa / chuyển khu vực"))
                {
                    return;
                }

                if (!ValidateStationInput(id, out string stationName, out string ipAddress, out int zoneId, out string status))
                {
                    return;
                }

                var station = new ComputerStation
                {
                    StationId = id,
                    StationName = stationName,
                    ZoneId = zoneId,
                    IpAddress = ipAddress,
                    Status = status
                };

                if (_dbStation.Update(station))
                {
                    MessageBox.Show($"Cập nhật máy trạm {station.StationName} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Đã cập nhật máy: {station.StationName}");
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi cập nhật máy trạm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoaMay_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtStationId.Text))
                {
                    MessageBox.Show("Vui lòng chọn máy trạm cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = Convert.ToInt32(txtStationId.Text);

                if (!CheckStationCanBeModified(id, "xóa máy"))
                {
                    return;
                }

                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa máy {txtStationName.Text}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    if (_dbStation.Delete(id))
                    {
                        MessageBox.Show("Xóa máy trạm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OnStationNotification?.Invoke($"Đã xóa máy ID: {id}");
                        LoadData();
                        ResetStationInputs();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi xóa máy trạm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnBaoTri_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtStationId.Text))
                {
                    MessageBox.Show("Vui lòng chọn máy trạm cần chuyển bảo trì!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = Convert.ToInt32(txtStationId.Text);

                if (!CheckStationCanBeModified(id, "chuyển bảo trì"))
                {
                    return;
                }

                if (_dbStation.UpdateStatus(id, "Maintenance"))
                {
                    MessageBox.Show($"Đã chuyển máy {txtStationName.Text} sang trạng thái BẢO TRÌ!", "Bảo trì", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Máy {txtStationName.Text} chuyển sang Bảo trì");
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi chuyển bảo trì", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnMoMay_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtStationId.Text))
                {
                    MessageBox.Show("Vui lòng chọn máy trạm cần mở sẵn sàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = Convert.ToInt32(txtStationId.Text);

                if (!CheckStationCanBeModified(id, "mở sẵn sàng"))
                {
                    return;
                }

                if (_dbStation.UpdateStatus(id, "Available"))
                {
                    MessageBox.Show($"Máy {txtStationName.Text} đã chuyển sang trạng thái SẴN SÀNG (Available)!", "Mở máy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Máy {txtStationName.Text} đã sẵn sàng");
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi mở sẵn sàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =========================================================================
        // NHIỆM VỤ 3: CHỨC NĂNG CHUYỂN MÁY ĐANG CHƠI (InUse -> Available)
        // =========================================================================
        private void BindTransferTargets()
        {
            var availableTargets = (from s in _stationList
                                    where s.Status.Equals("Available", StringComparison.OrdinalIgnoreCase)
                                    join z in _zoneList on s.ZoneId equals z.ZoneId into zg
                                    from zone in zg.DefaultIfEmpty()
                                    select new
                                    {
                                        s.StationId,
                                        DisplayText = zone != null
                                            ? $"{s.StationName} - {zone.ZoneName} ({zone.BasePricePerHour:N0}đ/h)"
                                            : $"{s.StationName} - Zone {s.ZoneId}"
                                    }).ToList();

            cboMayDich.DisplayMember = "DisplayText";
            cboMayDich.ValueMember = "StationId";
            cboMayDich.DataSource = availableTargets;
        }

        private void UpdateTransferControlsState(ComputerStation? selectedStation)
        {
            bool canTransfer = selectedStation != null
                               && selectedStation.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase)
                               && cboMayDich.Items.Count > 0;

            cboMayDich.Enabled = canTransfer;
            btnChuyenMay.Enabled = canTransfer;

            if (selectedStation == null)
            {
                lblMayNguon.Text = "Máy nguồn: Chọn máy đang InUse để chuyển";
            }
            else if (selectedStation.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
            {
                var srcZone = _zoneList.FirstOrDefault(z => z.ZoneId == selectedStation.ZoneId);
                string zoneInfo = srcZone != null ? $"{srcZone.ZoneName} ({srcZone.BasePricePerHour:N0}đ/h)" : $"Zone {selectedStation.ZoneId}";
                lblMayNguon.Text = cboMayDich.Items.Count > 0
                    ? $"Máy nguồn: {selectedStation.StationName} [{zoneInfo}]"
                    : $"Máy nguồn: {selectedStation.StationName} (Hết máy trống để chuyển!)";
            }
            else
            {
                lblMayNguon.Text = $"Máy nguồn: {selectedStation.StationName} ({selectedStation.Status} - Chỉ chuyển được máy InUse)";
            }
        }

        private void BtnChuyenMay_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtStationId.Text))
                {
                    MessageBox.Show("Vui lòng chọn máy nguồn đang chơi (InUse)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int sourceId = Convert.ToInt32(txtStationId.Text);
                var sourceStation = _stationList.FirstOrDefault(s => s.StationId == sourceId);
                if (sourceStation == null || !sourceStation.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Chỉ có thể chuyển máy đang ở trạng thái InUse!", "Không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cboMayDich.SelectedValue is not int targetId || targetId <= 0)
                {
                    MessageBox.Show("Vui lòng chọn máy trống đích để chuyển sang!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var targetStation = _stationList.FirstOrDefault(s => s.StationId == targetId);
                if (targetStation == null) return;

                string confirmMessage = $"Xác nhận chuyển phiên chơi từ [{sourceStation.StationName}] sang [{targetStation.StationName}]?";

                if (sourceStation.ZoneId != targetStation.ZoneId)
                {
                    var srcZone = _zoneList.FirstOrDefault(z => z.ZoneId == sourceStation.ZoneId);
                    var dstZone = _zoneList.FirstOrDefault(z => z.ZoneId == targetStation.ZoneId);
                    confirmMessage += $"\n\n⚠️ LƯU Ý CHÊNH LỆCH ĐƠN GIÁ KHU VỰC:\n" +
                                      $"• Khu cũ ({srcZone?.ZoneName}): {srcZone?.BasePricePerHour:N0} VNĐ/giờ\n" +
                                      $"• Khu mới ({dstZone?.ZoneName}): {dstZone?.BasePricePerHour:N0} VNĐ/giờ\n" +
                                      $"Khi tính tiền đóng máy, hệ thống sẽ áp dụng đơn giá của khu vực mới.";
                }

                if (MessageBox.Show(confirmMessage, "Xác nhận chuyển máy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (_dbStation.TransferStation(sourceId, targetId, out string errorMessage))
                {
                    MessageBox.Show($"Đã chuyển khách từ [{sourceStation.StationName}] sang [{targetStation.StationName}] thành công!", "Chuyển máy thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Đã chuyển máy {sourceStation.StationName} -> {targetStation.StationName}");
                    LoadData();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Không thể chuyển máy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi chuyển máy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================================
        // NHIỆM VỤ 1: UI + CRUD QUẢN LÝ KHU VỰC & BẢNG GIÁ (StationZone)
        // =========================================================================
        private void BindZoneGrid()
        {
            var zoneGridData = _zoneList.Select(z => new
            {
                MaKhuVuc = z.ZoneId,
                TenKhuVuc = z.ZoneName,
                GiaMoiGio = $"{z.BasePricePerHour:N0} VNĐ",
                SoMay = _stationList.Count(s => s.ZoneId == z.ZoneId),
                MoTa = z.Description ?? string.Empty
            }).ToList();

            dgvKhuVuc.DataSource = zoneGridData;

            if (dgvKhuVuc.Columns["MaKhuVuc"] is DataGridViewColumn c1) c1.HeaderText = "Mã Khu";
            if (dgvKhuVuc.Columns["TenKhuVuc"] is DataGridViewColumn c2) c2.HeaderText = "Tên Khu Vực";
            if (dgvKhuVuc.Columns["GiaMoiGio"] is DataGridViewColumn c3) c3.HeaderText = "Đơn Giá / Giờ";
            if (dgvKhuVuc.Columns["SoMay"] is DataGridViewColumn c4) c4.HeaderText = "Số Máy";
            if (dgvKhuVuc.Columns["MoTa"] is DataGridViewColumn c5) c5.HeaderText = "Mô Tả Cấu Hình";
        }

        private void DgvKhuVuc_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvKhuVuc.CurrentRow == null ||
                dgvKhuVuc.CurrentRow.DataBoundItem == null ||
                dgvKhuVuc.CurrentRow.Cells["MaKhuVuc"]?.Value == null)
            {
                return;
            }

            int zoneId = Convert.ToInt32(dgvKhuVuc.CurrentRow.Cells["MaKhuVuc"].Value);
            var zone = _zoneList.FirstOrDefault(z => z.ZoneId == zoneId);
            if (zone != null)
            {
                txtZoneId.Text = zone.ZoneId.ToString();
                txtZoneName.Text = zone.ZoneName;
                txtZonePrice.Text = zone.BasePricePerHour.ToString("0.##", CultureInfo.InvariantCulture);
                txtZoneDescription.Text = zone.Description ?? string.Empty;
            }
        }

        private void ClearZoneInputs()
        {
            txtZoneId.Clear();
            txtZoneName.Clear();
            txtZonePrice.Clear();
            txtZoneDescription.Clear();
        }

        private bool TryParseZonePriceInput(out decimal price)
        {
            string raw = txtZonePrice.Text.Trim();
            if (DBUser.TryParseCurrencyAmount(raw, out price))
            {
                return true;
            }

            return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out price) ||
                   decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out price);
        }

        private void BtnThemZone_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!TryParseZonePriceInput(out decimal price))
                {
                    MessageBox.Show("Đơn giá/giờ không hợp lệ! Vui lòng nhập số dương (ví dụ: 15000).", "Sai định dạng giá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtZonePrice.Focus();
                    return;
                }

                var zone = new StationZone
                {
                    ZoneName = txtZoneName.Text.Trim(),
                    BasePricePerHour = price,
                    Description = string.IsNullOrWhiteSpace(txtZoneDescription.Text) ? null : txtZoneDescription.Text.Trim()
                };

                if (_dbStation.AddZone(zone, out string errorMessage))
                {
                    MessageBox.Show($"Thêm khu vực [{zone.ZoneName}] thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Đã thêm khu vực: {zone.ZoneName}");
                    LoadData();
                    ClearZoneInputs();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Không thể thêm khu vực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi thêm khu vực", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSuaZone_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtZoneId.Text))
                {
                    MessageBox.Show("Vui lòng chọn khu vực cần cập nhật từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!TryParseZonePriceInput(out decimal price))
                {
                    MessageBox.Show("Đơn giá/giờ không hợp lệ! Vui lòng nhập số dương (ví dụ: 15000).", "Sai định dạng giá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtZonePrice.Focus();
                    return;
                }

                var zone = new StationZone
                {
                    ZoneId = Convert.ToInt32(txtZoneId.Text),
                    ZoneName = txtZoneName.Text.Trim(),
                    BasePricePerHour = price,
                    Description = string.IsNullOrWhiteSpace(txtZoneDescription.Text) ? null : txtZoneDescription.Text.Trim()
                };

                if (_dbStation.UpdateZone(zone, out string errorMessage))
                {
                    MessageBox.Show($"Cập nhật khu vực [{zone.ZoneName}] thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Đã cập nhật khu vực: {zone.ZoneName}");
                    LoadData();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Không thể cập nhật khu vực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi cập nhật khu vực", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoaZone_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtZoneId.Text))
                {
                    MessageBox.Show("Vui lòng chọn khu vực cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int zoneId = Convert.ToInt32(txtZoneId.Text);
                string zoneName = txtZoneName.Text;

                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa khu vực [{zoneName}]?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (_dbStation.DeleteZone(zoneId, out string errorMessage))
                {
                    MessageBox.Show($"Đã xóa khu vực [{zoneName}] thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnStationNotification?.Invoke($"Đã xóa khu vực: {zoneName}");
                    LoadData();
                    ClearZoneInputs();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Không thể xóa khu vực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi xóa khu vực", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLamMoiZone_Click(object? sender, EventArgs e)
        {
            dgvKhuVuc.ClearSelection();
            ClearZoneInputs();
            LoadData();
        }
    }
}