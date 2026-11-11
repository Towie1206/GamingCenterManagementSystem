using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem
{
    public partial class FormBilling : Form
    {
        private readonly DBBilling _dbBilling = new();
        private List<GamingSession> _sessionList = new();
        private List<GamingSession> _filteredRevenueSessions = new();
        private readonly Label _lblTopRevenueStation = new();
        private List<TariffPlan> _planList = new();

        // Delegate / Action for billing events (Chapter 4 Delegate requirement)
        public Action<string>? OnBillingNotification;

        public FormBilling()
        {
            InitializeComponent();
            SetupEvents();
            InitDefaults();
            LoadData();
        }

        private void InitDefaults()
        {
            dtTuNgayInit();
            cboThuatToanDaHinh.SelectedIndex = 0;
            cboLocGoiCuoc.SelectedIndex = 0;
            // Báo cáo doanh thu chỉ ghi nhận phiên đã thanh toán/hoàn thành.
            cboLocTrangThai.SelectedItem = "Completed";
            cboLocTrangThai.Enabled = false;

            // Display the best earning station without changing the leader's Designer.
            pnlStats.Height = 72;
            _lblTopRevenueStation.Name = "lblTopRevenueStation";
            _lblTopRevenueStation.AutoSize = true;
            _lblTopRevenueStation.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblTopRevenueStation.ForeColor = Color.FromArgb(135, 210, 255);
            _lblTopRevenueStation.Location = new Point(20, 44);
            pnlStats.Controls.Add(_lblTopRevenueStation);
            cboTariffType.SelectedIndex = 0;
        }

        private void dtTuNgayInit()
        {
            dtpTuNgay.Value = DateTime.Today.AddDays(-7);
            dtpDenNgay.Value = DateTime.Today;
        }

        private void SetupEvents()
        {
            Load += (s, e) => LoadData();
            btnLocBaoCao.Click += BtnLocBaoCao_Click;
            btnLamMoi.Click += (s, e) => { dtTuNgayInit(); LoadData(); };
            btnXuatBaoCao.Click += BtnXuatBaoCao_Click;

            btnTinhThuTien.Click += BtnTinhThuTien_Click;

            btnThemGoiCuoc.Click += BtnThemGoiCuoc_Click;
            btnCapNhatGoiCuoc.Click += BtnCapNhatGoiCuoc_Click;

            dgvGoiCuoc.SelectionChanged += DgvGoiCuoc_SelectionChanged;
        }

        private void LoadData()
        {
            try
            {
                _planList = _dbBilling.GetAllPlans();
                _sessionList = _dbBilling.GetAllSessions();

                BindPlanGrid(_planList);
                ApplyRevenueFilter(reloadFromDatabase: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu cước & doanh thu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindPlanGrid(List<TariffPlan> plans)
        {
            var displayPlans = plans.Select(p => new
            {
                MaGoi = p.PlanId,
                TenGoi = p.PlanName,
                LoaiCuoc = p.TariffType,
                GiaTronGoi = p.FixedPrice.HasValue ? $"{p.FixedPrice.Value:N0} VNĐ" : "--",
                ChietKhau = $"{p.DiscountRate * 100:0}%",
                GioApDung = (p.StartHour.HasValue && p.EndHour.HasValue) ? $"{p.StartHour}h - {p.EndHour}h" : "Toàn thời gian"
            }).ToList();

            dgvGoiCuoc.DataSource = displayPlans;

            if (dgvGoiCuoc.Columns["MaGoi"] is DataGridViewColumn col1) col1.HeaderText = "Mã";
            if (dgvGoiCuoc.Columns["TenGoi"] is DataGridViewColumn col2) col2.HeaderText = "Tên Gói Cước";
            if (dgvGoiCuoc.Columns["LoaiCuoc"] is DataGridViewColumn col3) col3.HeaderText = "Loại Thuật Toán";
            if (dgvGoiCuoc.Columns["GiaTronGoi"] is DataGridViewColumn col4) col4.HeaderText = "Giá Trọn Gói";
            if (dgvGoiCuoc.Columns["ChietKhau"] is DataGridViewColumn col5) col5.HeaderText = "Chiết Khấu";
            if (dgvGoiCuoc.Columns["GioApDung"] is DataGridViewColumn col6) col6.HeaderText = "Khung Giờ";
        }

        private void BindSessionGrid(List<GamingSession> sessions)
        {
            // LINQ projection for Sessions & Revenue
            var displaySessions = sessions.Select(s => new
            {
                MaPhien = s.SessionId,
                MayTram = $"PC-{s.StationId:D2}",
                MaNguoiDung = s.UserId.HasValue ? $"User #{s.UserId}" : "Khách vãng lai",
                BatDau = s.StartTime.ToString("dd/MM/yyyy HH:mm"),
                KetThuc = s.EndTime.HasValue ? s.EndTime.Value.ToString("dd/MM/yyyy HH:mm") : "Đang chơi...",
                GoiCuoc = s.TariffType,
                TienGio = $"{s.PlayCost:N0} VNĐ",
                TienDichVu = $"{s.ServiceCost:N0} VNĐ",
                TongCong = $"{s.TotalCost:N0} VNĐ",
                TrangThai = s.Status
            }).ToList();

            dgvDoanhThu.DataSource = displaySessions;

            if (dgvDoanhThu.Columns["MaPhien"] is DataGridViewColumn sCol1) sCol1.HeaderText = "Mã Phiên";
            if (dgvDoanhThu.Columns["MayTram"] is DataGridViewColumn sCol2) sCol2.HeaderText = "Máy Trạm";
            if (dgvDoanhThu.Columns["MaNguoiDung"] is DataGridViewColumn sCol3) sCol3.HeaderText = "Hội Viên";
            if (dgvDoanhThu.Columns["BatDau"] is DataGridViewColumn sCol4) sCol4.HeaderText = "Bắt Đầu";
            if (dgvDoanhThu.Columns["KetThuc"] is DataGridViewColumn sCol5) sCol5.HeaderText = "Kết Thúc";
            if (dgvDoanhThu.Columns["GoiCuoc"] is DataGridViewColumn sCol6) sCol6.HeaderText = "Gói Cước";
            if (dgvDoanhThu.Columns["TienGio"] is DataGridViewColumn sCol7) sCol7.HeaderText = "Tiền Giờ";
            if (dgvDoanhThu.Columns["TienDichVu"] is DataGridViewColumn sCol8) sCol8.HeaderText = "Dịch Vụ F&B";
            if (dgvDoanhThu.Columns["TongCong"] is DataGridViewColumn sCol9) sCol9.HeaderText = "Tổng Cộng";
            if (dgvDoanhThu.Columns["TrangThai"] is DataGridViewColumn sCol10) sCol10.HeaderText = "Trạng Thái";
        }

        private void UpdateStatistics(List<GamingSession> sessions)
        {
            // LINQ Sum and Count calculations (Applying LINQ to Objects)
            decimal totalRevenue = sessions.Sum(s => s.TotalCost);
            decimal totalPlay = sessions.Sum(s => s.PlayCost);
            decimal totalService = sessions.Sum(s => s.ServiceCost);
            int sessionCount = sessions.Count;

            lblTongDoanhThu.Text = $"💵 Tổng doanh thu: {totalRevenue:N0} VNĐ";
            lblTienGio.Text = $"⏱️ Tiền giờ chơi: {totalPlay:N0} VNĐ";
            lblTienDichVu.Text = $"🍔 Dịch vụ F&B: {totalService:N0} VNĐ";
            lblTongSoPhien.Text = $"🎮 Tổng phiên: {sessionCount}";

            // Logic 2: station income ranking for the current filtered date range.
            List<StationRevenueSummary> byStation = DBBilling.GroupRevenueByStation(sessions);
            _lblTopRevenueStation.Text = byStation.Count == 0
                ? "🏆 Máy doanh thu cao nhất: Chưa có phiên hoàn thành"
                : $"🏆 Máy doanh thu cao nhất: PC-{byStation[0].StationId:D2} - {byStation[0].Revenue:N0} VNĐ ({byStation[0].SessionCount} phiên)";
        }

        // Khôi - Logic 1: refresh from SQL/fallback and LINQ-filter Completed sessions.
        // Both the grid and all revenue labels use the same filtered list.
        private bool ApplyRevenueFilter(bool reloadFromDatabase)
        {
            DateTime fromDate = dtpTuNgay.Value.Date;
            DateTime toDate = dtpDenNgay.Value.Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("Từ ngày không được sau Đến ngày.",
                    "Khoảng ngày không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                if (reloadFromDatabase)
                    _sessionList = _dbBilling.GetAllSessions();

                string? tariff = cboLocGoiCuoc.SelectedItem?.ToString();
                List<GamingSession> filtered = DBBilling.FilterCompletedRevenueSessions(
                    _sessionList, fromDate, toDate, tariff);

                _filteredRevenueSessions = filtered;
                BindSessionGrid(filtered);
                UpdateStatistics(filtered); // includes Sum(s => s.TotalCost) and station GroupBy
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lọc doanh thu: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void BtnLocBaoCao_Click(object? sender, EventArgs e)
        {
            ApplyRevenueFilter(reloadFromDatabase: true);
        }

        // Xuất báo cáo doanh thu ca trực: Hỗ trợ linh hoạt cả định dạng JSON lẫn Text (TXT)
        private void BtnXuatBaoCao_Click(object? sender, EventArgs e)
        {
            if (!ApplyRevenueFilter(reloadFromDatabase: true))
                return;

            if (_filteredRevenueSessions == null || _filteredRevenueSessions.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu phiên máy hoàn thành để xuất báo cáo!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                DefaultExt = "json",
                AddExtension = true,
                OverwritePrompt = true,
                Title = "Xuất Báo Cáo Doanh Thu (JSON / TXT)"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                decimal total = _filteredRevenueSessions.Sum(s => s.TotalCost);
                string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();

                if (ext == ".txt")
                {
                    string reportText = DBBilling.BuildRevenueShiftReport(
                        _filteredRevenueSessions, dtpTuNgay.Value.Date, dtpDenNgay.Value.Date);
                    File.WriteAllText(sfd.FileName, reportText, new UTF8Encoding(true));
                }
                else
                {
                    List<StationRevenueSummary> byStation = DBBilling.GroupRevenueByStation(_filteredRevenueSessions);
                    var reportData = new
                    {
                        TieuDe = "BÁO CÁO DOANH THU CA TRỰC - CYBER GAME",
                        NgayXuat = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        TuNgay = dtpTuNgay.Value.ToString("dd/MM/yyyy"),
                        DenNgay = dtpDenNgay.Value.ToString("dd/MM/yyyy"),
                        TongSoPhien = _filteredRevenueSessions.Count,
                        TongTienGio = _filteredRevenueSessions.Sum(s => s.PlayCost),
                        TongTienDichVu = _filteredRevenueSessions.Sum(s => s.ServiceCost),
                        TongDoanhThu = total,
                        MayDoanhThuCaoNhat = byStation.Count > 0 ? $"PC-{byStation[0].StationId:D2} ({byStation[0].Revenue:N0} VNĐ)" : "N/A",
                        DanhSachPhien = _filteredRevenueSessions.Select(s => new
                        {
                            MaPhien = s.SessionId,
                            MayTram = $"PC-{s.StationId:D2}",
                            HoiVien = s.UserId.HasValue ? $"User #{s.UserId}" : "Khách vãng lai",
                            GoiCuoc = s.TariffType,
                            ThoiGianBatDau = s.StartTime.ToString("dd/MM/yyyy HH:mm"),
                            ThoiGianKetThuc = s.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "Đang chơi",
                            TienGio = s.PlayCost,
                            TienDichVu = s.ServiceCost,
                            TongTien = s.TotalCost,
                            TrangThai = s.Status
                        }).ToList()
                    };

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    string jsonString = JsonSerializer.Serialize(reportData, options);
                    File.WriteAllText(sfd.FileName, jsonString, new UTF8Encoding(true));
                }

                OnBillingNotification?.Invoke($"Đã xuất báo cáo doanh thu: {Path.GetFileName(sfd.FileName)}");
                MessageBox.Show(
                    $"Xuất báo cáo thành công!\nTổng doanh thu: {total:N0} VNĐ ({_filteredRevenueSessions.Count} phiên)\nĐường dẫn: {sfd.FileName}",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xuất báo cáo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTinhThuTien_Click(object? sender, EventArgs e)
        {
            if (!decimal.TryParse(txtDonGiaGoc.Text.Trim(), out decimal basePrice) || basePrice <= 0)
            {
                MessageBox.Show("Vui lòng nhập đơn giá gốc hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtSoGioChoi.Text.Trim(), out double hours) || hours <= 0)
            {
                MessageBox.Show("Vui lòng nhập số giờ chơi hợp lệ (> 0)!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(hours);

            // POLYMORPHISM IN ACTION (Tính đa hình OOP):
            // Reference to abstract base class TariffCalculatorBase
            TariffCalculatorBase calculator;
            int choice = cboThuatToanDaHinh.SelectedIndex;

            switch (choice)
            {
                case 1:
                    calculator = new NightComboTariff();
                    break;
                case 2:
                    calculator = new TournamentTariff();
                    break;
                case 0:
                default:
                    calculator = new HourlyTariff();
                    break;
            }

            // Polymorphic method invocation: CalculateCost()
            decimal cost = calculator.CalculateCost(startTime, endTime, basePrice);

            lblKetQuaTinh.Text = $"Gói: {calculator.TariffName}\n" +
                                 $"Thời lượng: {hours:F1} giờ | Đơn giá: {basePrice:N0}đ/h\n" +
                                 $"Thành tiền: {cost:N0} VNĐ";
        }

        private void DgvGoiCuoc_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvGoiCuoc.CurrentRow != null && dgvGoiCuoc.CurrentRow.DataBoundItem != null)
            {
                int planId = Convert.ToInt32(dgvGoiCuoc.CurrentRow.Cells["MaGoi"].Value);
                var plan = _planList.FirstOrDefault(p => p.PlanId == planId);
                if (plan != null)
                {
                    txtPlanName.Text = plan.PlanName;
                    cboTariffType.SelectedItem = plan.TariffType;
                    txtFixedPrice.Text = plan.FixedPrice.HasValue ? plan.FixedPrice.Value.ToString("F0") : "";
                    txtDiscountRate.Text = plan.DiscountRate.ToString("F2");
                }
            }
        }

        private void BtnThemGoiCuoc_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlanName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên gói cước!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal? fixedPrice = null;
            if (decimal.TryParse(txtFixedPrice.Text.Trim(), out decimal fp)) fixedPrice = fp;

            double.TryParse(txtDiscountRate.Text.Trim(), out double discount);

            var newPlan = new TariffPlan
            {
                PlanName = txtPlanName.Text.Trim(),
                TariffType = cboTariffType.SelectedItem?.ToString() ?? "Hourly",
                FixedPrice = fixedPrice,
                DiscountRate = discount
            };

            if (_dbBilling.AddPlan(newPlan))
            {
                MessageBox.Show($"Thêm gói cước '{newPlan.PlanName}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnBillingNotification?.Invoke($"Đã thêm gói cước mới: {newPlan.PlanName}");
                LoadData();
            }
        }

        private void BtnCapNhatGoiCuoc_Click(object? sender, EventArgs e)
        {
            if (dgvGoiCuoc.CurrentRow == null) return;
            int planId = Convert.ToInt32(dgvGoiCuoc.CurrentRow.Cells["MaGoi"].Value);

            decimal? fixedPrice = null;
            if (decimal.TryParse(txtFixedPrice.Text.Trim(), out decimal fp)) fixedPrice = fp;
            double.TryParse(txtDiscountRate.Text.Trim(), out double discount);

            var plan = new TariffPlan
            {
                PlanId = planId,
                PlanName = txtPlanName.Text.Trim(),
                TariffType = cboTariffType.SelectedItem?.ToString() ?? "Hourly",
                FixedPrice = fixedPrice,
                DiscountRate = discount
            };

            if (_dbBilling.UpdatePlan(plan))
            {
                MessageBox.Show($"Cập nhật gói cước '{plan.PlanName}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnBillingNotification?.Invoke($"Đã cập nhật gói cước: {plan.PlanName}");
                LoadData();
            }
        }
    }
}
