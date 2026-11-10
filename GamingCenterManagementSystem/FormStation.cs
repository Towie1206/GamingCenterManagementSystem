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

        // Delegate / Action for station status notification (conforming to Chapter 4 Delegate requirements)
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

            // Optional notification handler
            OnStationNotification += msg =>
            {
                // Can be wired to status strip or parent dashboard
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

                // Populate Filter Combos
                var filterZones = new List<string> { "Tất cả khu vực" };
                filterZones.AddRange(_zoneList.Select(z => z.ZoneName));
                cboLocKhuVuc.DataSource = filterZones;

                cboLocTrangThai.DataSource = new List<string>
                {
                    "Tất cả trạng thái",
                    "Available",
                    "InUse",
                    "Maintenance",
                    "TournamentLocked"
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
            // LINQ Join / Projection to format display (Applying LINQ to Objects from PPTX)
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
            // Applying LINQ Count queries
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
            string selectedStatus = cboLocTrangThai.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            // LINQ query filtering (Applying LINQ Where clauses)
            var query = _stationList.AsEnumerable();

            if (selectedZone != "Tất cả khu vực")
            {
                var zone = _zoneList.FirstOrDefault(z => z.ZoneName == selectedZone);
                if (zone != null)
                {
                    query = query.Where(s => s.ZoneId == zone.ZoneId);
                }
            }

            if (selectedStatus != "Tất cả trạng thái")
            {
                query = query.Where(s => s.Status.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase));
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
                }
            }
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

        private void BtnSuaMay_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStationId.Text))
            {
                MessageBox.Show("Vui lòng chọn máy trạm cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtStationId.Text);
            var station = new ComputerStation
            {
                StationId = id,
                StationName = txtStationName.Text.Trim(),
                ZoneId = Convert.ToInt32(cboKhuVuc.SelectedValue ?? 1),
                IpAddress = txtIpAddress.Text.Trim(),
                Status = cboTrangThai.SelectedItem?.ToString() ?? "Available"
            };

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
