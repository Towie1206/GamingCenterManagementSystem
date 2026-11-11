using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormStation : Form
    {
        private readonly DBStation _dbStation = new();
        private List<ComputerStation> _stationList = new();
        private List<StationZone> _zoneList = new();

        // Delegate / Action notification
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
            btnThemMay.Click += BtnThemMay_Click;
            btnSuaMay.Click += BtnSuaMay_Click;
            btnXoaMay.Click += BtnXoaMay_Click;
            btnBaoTri.Click += BtnBaoTri_Click;
            btnMoMay.Click += BtnMoMay_Click;
            btnLamMoi.Click += (s, e) => LoadData();
            btnApDungLoc.Click += (s, e) => ApplyFilter();
            dgvMayTram.SelectionChanged += DgvMayTram_SelectionChanged;

            OnStationNotification += msg =>
            {
                // Hook notification handler if needed
            };
        }

        private void LoadData()
        {
            try
            {
                _zoneList = _dbStation.GetAllZones();
                _stationList = _dbStation.GetAll();

                // Populate Zone combo
                cboKhuVuc.DataSource = _zoneList.ToList();
                cboKhuVuc.DisplayMember = "ZoneName";
                cboKhuVuc.ValueMember = "ZoneId";

                // Populate Filter Combos (Tiếng Việt đồng nhất UI)
                var filterZones = new List<string> { "Tất cả khu vực" };
                filterZones.AddRange(_zoneList.Select(z => z.ZoneName));
                cboLocKhuVuc.DataSource = filterZones;

                // LOGIC 2 UI: Hiển thị danh sách Tiếng Việt cho ComboBox Lọc
                cboLocTrangThai.DataSource = new List<string>
                {
                    "Tất cả trạng thái",
                    "Máy trống (Available)",
                    "Đang chơi (InUse)",
                    "Bảo trì (Maintenance)",
                    "Đấu trường (TournamentLocked)"
                };

                BindGrid(_stationList);
                UpdateStatistics();
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

        // =========================================================================
        // LOGIC 2: VIẾT CÂU LỆNH LINQ ĐỂ LỌC DANH SÁCH MÁY TRẠM THEO COMBOBOX
        // =========================================================================
        private void ApplyFilter()
        {
            string selectedZone = cboLocKhuVuc.SelectedItem?.ToString() ?? "Tất cả khu vực";
            string selectedStatusText = cboLocTrangThai.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            // Ánh xạ từ văn bản hiển thị trên UI ra giá trị lưu trong CSDL
            string targetStatus = selectedStatusText switch
            {
                "Máy trống (Available)" => "Available",
                "Đang chơi (InUse)" => "InUse",
                "Bảo trì (Maintenance)" => "Maintenance",
                "Đấu trường (TournamentLocked)" => "TournamentLocked",
                _ => "ALL"
            };

            // Truy vấn LINQ lọc theo danh sách
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

        private void DgvMayTram_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvMayTram.CurrentRow != null && dgvMayTram.CurrentRow.DataBoundItem != null)
            {
                int stationId = Convert.ToInt32(dgvMayTram.CurrentRow.Cells["MaMay"].Value);
                var station = _stationList.FirstOrDefault(s => s.StationId == stationId);
                if (station != null)
                {
                    txtStationId.Text = station.StationId.ToString();
                    txtStationName.Text = station.StationName;
                    txtIpAddress.Text = station.IpAddress;
                    cboKhuVuc.SelectedValue = station.ZoneId;
                    cboTrangThai.SelectedItem = station.Status;

                    // MỞ KHÓA HOÀN TOÀN: Cho phép đổi khu vực nếu máy KHÔNG PHẢI đang InUse
                    bool isBusy = station.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase);
                    cboKhuVuc.Enabled = !isBusy; // Nếu InUse thì khóa (false), nếu Available thì mở (true)
                    cboTrangThai.Enabled = !isBusy;
                }
            }
        }

        // =========================================================================
        // LOGIC 1: HÀM KIỂM TRA MÁY TRẠM (CHẶN NẾU MÁY ĐANG CÓ KHÁCH 'InUse')
        // =========================================================================
        private bool CheckStationCanBeModified(int stationId, string actionName)
        {
            var station = _stationList.FirstOrDefault(s => s.StationId == stationId);
            if (station == null) return true;

            // Nếu máy đang có khách chơi (InUse), chặn chỉnh sửa / chuyển vùng / bảo trì
            if (station.Status.Equals("InUse", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"🚫 KHÔNG THỂ {actionName.ToUpper()}!\n\n" +
                    $"Máy trạm [{station.StationName}] đang có khách sử dụng (Status: InUse).\n" +
                    $"Vui lòng kết thúc phiên chơi của khách trước khi thực hiện thao tác này.",
                    "Cảnh báo hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false; // Trả về false để ngắt thao tác
            }
            return true; // Hợp lệ
        }

        private void BtnThemMay_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStationName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên máy trạm!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newStation = new ComputerStation
            {
                StationName = txtStationName.Text.Trim(),
                ZoneId = Convert.ToInt32(cboKhuVuc.SelectedValue ?? 1),
                IpAddress = string.IsNullOrWhiteSpace(txtIpAddress.Text) ? "192.168.1.100" : txtIpAddress.Text.Trim(),
                Status = cboTrangThai.SelectedItem?.ToString() ?? "Available"
            };

            if (_dbStation.Add(newStation))
            {
                MessageBox.Show($"Thêm máy trạm {newStation.StationName} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnStationNotification?.Invoke($"Đã thêm máy: {newStation.StationName}");
                LoadData();
            }
        }

        // =========================================================================
        // LOGIC 3: CẬP NHẬT THÔNG TIN VÀ CẤU HÌNH MÁY TRẠM XUỐNG CSDL
        // =========================================================================
        private void BtnSuaMay_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStationId.Text))
            {
                MessageBox.Show("Vui lòng chọn máy trạm cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtStationId.Text);

            // LOGIC 1 APPLIED: Chặn sửa hoặc chuyển khu vực nếu máy đang InUse
            if (!CheckStationCanBeModified(id, "chỉnh sửa / chuyển khu vực"))
            {
                return;
            }

            var station = new ComputerStation
            {
                StationId = id,
                StationName = txtStationName.Text.Trim(),
                ZoneId = Convert.ToInt32(cboKhuVuc.SelectedValue ?? 1),
                IpAddress = txtIpAddress.Text.Trim(),
                Status = cboTrangThai.SelectedItem?.ToString() ?? "Available"
            };

            // Thực hiện cập nhật cấu hình/thông tin máy trạm
            if (_dbStation.Update(station))
            {
                MessageBox.Show($"Cập nhật máy trạm {station.StationName} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnStationNotification?.Invoke($"Đã cập nhật máy: {station.StationName}");
                LoadData();
            }
        }

        private void BtnXoaMay_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStationId.Text))
            {
                MessageBox.Show("Vui lòng chọn máy trạm cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtStationId.Text);

            // LOGIC 1 APPLIED: Chặn xóa máy nếu đang InUse
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
                }
            }
        }

        private void BtnBaoTri_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStationId.Text)) return;
            int id = Convert.ToInt32(txtStationId.Text);

            // LOGIC 1 APPLIED: Chặn chuyển sang bảo trì nếu máy đang có khách
            if (!CheckStationCanBeModified(id, "chuyển bảo trì"))
            {
                return;
            }

            if (_dbStation.UpdateStatus(id, "Maintenance"))
            {
                MessageBox.Show($"Đã chuyển máy {txtStationName.Text} sang trạng thái BẢO TRÌ!", "Bảo trì", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnMoMay_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStationId.Text)) return;
            int id = Convert.ToInt32(txtStationId.Text);

            if (_dbStation.UpdateStatus(id, "Available"))
            {
                MessageBox.Show($"Máy {txtStationName.Text} đã chuyển sang trạng thái SẴN SÀNG (Available)!", "Mở máy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }
    }
}