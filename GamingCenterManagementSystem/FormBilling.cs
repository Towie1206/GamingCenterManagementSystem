using System;
using System.Collections.Generic;
using System.Globalization;
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
        private List<ServiceItem> _serviceItems = new();
        private bool _loadingFnB;
        private int? _selectedServiceId;

        private sealed class ServiceOption
        {
            public int ServiceId { get; init; }
            public string Display { get; init; } = string.Empty;
            public override string ToString() => Display;
        }

        private sealed class SessionOption
        {
            public int SessionId { get; init; }
            public string Display { get; init; } = string.Empty;
            public override string ToString() => Display;
        }

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
            cboServiceCategory.SelectedIndex = 0;
            chkServiceActive.Checked = true;
            nudStartHour.Enabled = chkApDungKhungGio.Checked;
            nudEndHour.Enabled = chkApDungKhungGio.Checked;
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
            btnXoaGoiCuoc.Click += BtnXoaGoiCuoc_Click;
            chkApDungKhungGio.CheckedChanged += (s, e) =>
            {
                nudStartHour.Enabled = chkApDungKhungGio.Checked;
                nudEndHour.Enabled = chkApDungKhungGio.Checked;
            };

            btnThemMon.Click += BtnThemMon_Click;
            btnCapNhatMon.Click += BtnCapNhatMon_Click;
            btnBatTatMon.Click += BtnBatTatMon_Click;
            btnNhapKho.Click += BtnNhapKho_Click;
            btnLamMoiMon.Click += (s, e) => SafeLoadFnBData();
            btnGoiMonVaoPhien.Click += BtnGoiMonVaoPhien_Click;
            dgvDanhMucDichVu.SelectionChanged += DgvDanhMucDichVu_SelectionChanged;
            cboPhienDangChoi.SelectedIndexChanged += (s, e) =>
            {
                if (!_loadingFnB) RefreshSessionServiceGrid();
            };
            tabControlBilling.SelectedIndexChanged += (s, e) =>
            {
                if (tabControlBilling.SelectedTab == tabDichVuFnB) SafeLoadFnBData();
            };

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
                LoadFnBData();
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

            // Phase 2: three summaries always follow the same filtered sessions.
            dgvDoanhThuTheoMay.DataSource = byStation.Select(x => new
            {
                May = $"PC-{x.StationId:D2}", SoPhien = x.SessionCount,
                DoanhThu = x.Revenue
            }).ToList();
            dgvDoanhThuTheoNgay.DataSource = DBBilling.GroupRevenueByDay(sessions).Select(x => new
            {
                Ngay = x.Date.ToString("dd/MM/yyyy"), SoPhien = x.SessionCount,
                TienGio = x.PlayRevenue, TienDichVu = x.ServiceRevenue, DoanhThu = x.Revenue
            }).ToList();
            dgvDoanhThuTheoGoiCuoc.DataSource = DBBilling.GroupRevenueByTariffType(sessions).Select(x => new
            {
                GoiCuoc = x.TariffType, SoPhien = x.SessionCount,
                TienGio = x.PlayRevenue, TienDichVu = x.ServiceRevenue, DoanhThu = x.Revenue
            }).ToList();
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
                        ThongKeTheoMay = byStation,
                        ThongKeTheoNgay = DBBilling.GroupRevenueByDay(_filteredRevenueSessions),
                        ThongKeTheoGoiCuoc = DBBilling.GroupRevenueByTariffType(_filteredRevenueSessions),
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

        // Shared input parser: Vietnamese comma decimals and English dot decimals.
        // A short fractional part is treated as a decimal separator even when the
        // machine culture would otherwise interpret it as a thousands separator.
        private static bool TryParseDecimalInput(string text, out decimal value)
        {
            value = 0m;
            text = text.Trim();
            if (text.Length == 0) return false;

            bool dotOnly = text.Contains('.') && !text.Contains(',');
            bool commaOnly = text.Contains(',') && !text.Contains('.');
            if (dotOnly && text.LastIndexOf('.') == text.Length - 2 ||
                dotOnly && text.LastIndexOf('.') == text.Length - 3)
                return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            if (commaOnly && text.LastIndexOf(',') == text.Length - 2 ||
                commaOnly && text.LastIndexOf(',') == text.Length - 3)
                return decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out value);

            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
                || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                || decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out value);
        }

        private bool TryBuildPlanFromInputs(out TariffPlan? plan)
        {
            plan = null;
            string name = txtPlanName.Text.Trim();
            if (name.Length == 0 || name.Length > 100)
            {
                MessageBox.Show("Tên gói cước phải có từ 1 đến 100 ký tự.");
                return false;
            }

            decimal? fixedPrice = null;
            if (!string.IsNullOrWhiteSpace(txtFixedPrice.Text))
            {
                if (!TryParseDecimalInput(txtFixedPrice.Text, out decimal parsedPrice) || parsedPrice < 0)
                {
                    MessageBox.Show("Giá trọn gói phải là số hợp lệ, không được âm.");
                    return false;
                }
                fixedPrice = parsedPrice;
            }
            if (!TryParseDecimalInput(txtDiscountRate.Text, out decimal discount) || discount < 0m || discount > 1m)
            {
                MessageBox.Show("Chiết khấu phải nằm trong khoảng 0 đến 1 (ví dụ: 0.15).");
                return false;
            }
            int? start = chkApDungKhungGio.Checked ? (int)nudStartHour.Value : null;
            int? end = chkApDungKhungGio.Checked ? (int)nudEndHour.Value : null;
            if (start.HasValue && start == end)
            {
                MessageBox.Show("Giờ bắt đầu và giờ kết thúc không được trùng nhau.");
                return false;
            }
            try
            {
                plan = new TariffPlan
                {
                    PlanName = name,
                    TariffType = DBBilling.NormalizeTariffType(cboTariffType.SelectedItem?.ToString()),
                    FixedPrice = fixedPrice,
                    DiscountRate = (double)discount,
                    StartHour = start,
                    EndHour = end
                };
                return true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu gói cước");
                return false;
            }
        }

        private void DgvGoiCuoc_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvGoiCuoc.CurrentRow?.DataBoundItem == null) return;
            int id = Convert.ToInt32(dgvGoiCuoc.CurrentRow.Cells["MaGoi"].Value);
            TariffPlan? plan = _planList.FirstOrDefault(x => x.PlanId == id);
            if (plan == null) return;
            txtPlanName.Text = plan.PlanName;
            cboTariffType.SelectedItem = plan.TariffType;
            txtFixedPrice.Text = plan.FixedPrice?.ToString("F0") ?? string.Empty;
            txtDiscountRate.Text = plan.DiscountRate.ToString("F2");
            chkApDungKhungGio.Checked = plan.StartHour.HasValue && plan.EndHour.HasValue;
            if (plan.StartHour.HasValue && plan.EndHour.HasValue)
            {
                nudStartHour.Value = Math.Clamp(plan.StartHour.Value, 0, 23);
                nudEndHour.Value = Math.Clamp(plan.EndHour.Value, 0, 23);
            }
        }

        private void BtnThemGoiCuoc_Click(object? sender, EventArgs e)
        {
            if (!TryBuildPlanFromInputs(out TariffPlan? plan) || plan == null) return;
            try
            {
                if (_dbBilling.AddPlan(plan))
                {
                    LoadData();
                    OnBillingNotification?.Invoke($"Đã thêm gói cước: {plan.PlanName}");
                    MessageBox.Show("Thêm gói cước thành công.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể thêm gói cước: {ex.Message}");
            }
        }

        private void BtnCapNhatGoiCuoc_Click(object? sender, EventArgs e)
        {
            if (dgvGoiCuoc.CurrentRow?.DataBoundItem == null) return;
            if (!TryBuildPlanFromInputs(out TariffPlan? plan) || plan == null) return;
            plan.PlanId = Convert.ToInt32(dgvGoiCuoc.CurrentRow.Cells["MaGoi"].Value);
            try
            {
                if (_dbBilling.UpdatePlan(plan))
                {
                    LoadData();
                    OnBillingNotification?.Invoke($"Đã cập nhật gói cước: {plan.PlanName}");
                    MessageBox.Show("Cập nhật gói cước thành công.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật gói cước: {ex.Message}");
            }
        }

        private void BtnXoaGoiCuoc_Click(object? sender, EventArgs e)
        {
            if (dgvGoiCuoc.CurrentRow?.DataBoundItem == null) return;
            int id = Convert.ToInt32(dgvGoiCuoc.CurrentRow.Cells["MaGoi"].Value);
            string name = dgvGoiCuoc.CurrentRow.Cells["TenGoi"].Value?.ToString() ?? "";
            if (MessageBox.Show($"Xóa gói cước '{name}'?", "Xác nhận xóa",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (_dbBilling.DeletePlan(id))
                {
                    LoadData();
                    OnBillingNotification?.Invoke($"Đã xóa gói cước: {name}");
                    MessageBox.Show("Đã xóa gói cước.");
                }
                else MessageBox.Show("Không tìm thấy gói cước để xóa.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa gói cước: {ex.Message}");
            }
        }

        private void SafeLoadFnBData()
        {
            try { LoadFnBData(); }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải danh mục F&B: {ex.Message}"); }
        }

        // Load both the menu and active sessions whenever the form is refreshed.
        private void LoadFnBData()
        {
            if (_loadingFnB) return;
            int? previousSession = (cboPhienDangChoi.SelectedItem as SessionOption)?.SessionId;
            int? previousService = (cboMonGoi.SelectedItem as ServiceOption)?.ServiceId;
            _loadingFnB = true;
            try
            {
                _serviceItems = _dbBilling.GetAllServiceItems();
                var active = _dbBilling.GetActiveSessions();
                var stations = new DBStation().GetAll()
                    .GroupBy(x => x.StationId).ToDictionary(x => x.Key, x => x.First().StationName);

                dgvDanhMucDichVu.DataSource = _serviceItems.Select(item => new
                {
                    MaDichVu = item.ServiceId,
                    TenDichVu = item.ServiceName,
                    PhanLoai = item.Category,
                    DonGia = item.UnitPrice,
                    TonKho = item.StockQuantity,
                    TrangThai = item.IsActive ? "Đang bán" : "Ngừng bán"
                }).ToList();

                cboMonGoi.Items.Clear();
                foreach (ServiceItem item in _serviceItems.Where(x => x.IsActive && x.StockQuantity > 0))
                    cboMonGoi.Items.Add(new ServiceOption
                    {
                        ServiceId = item.ServiceId,
                        Display = $"{item.ServiceName} - {item.UnitPrice:N0}đ (Còn: {item.StockQuantity})"
                    });
                if (cboMonGoi.Items.Count > 0)
                {
                    int pos = cboMonGoi.Items.Cast<ServiceOption>().ToList()
                        .FindIndex(x => x.ServiceId == previousService);
                    cboMonGoi.SelectedIndex = pos >= 0 ? pos : 0;
                }

                cboPhienDangChoi.Items.Clear();
                foreach (GamingSession session in active)
                {
                    string station = stations.TryGetValue(session.StationId, out string? n)
                        ? n : $"PC-{session.StationId:D2}";
                    cboPhienDangChoi.Items.Add(new SessionOption
                    {
                        SessionId = session.SessionId,
                        Display = $"Phiên #{session.SessionId} - {station} (Bắt đầu: {session.StartTime:HH:mm} | DV: {session.ServiceCost:N0}đ)"
                    });
                }
                if (cboPhienDangChoi.Items.Count > 0)
                {
                    int pos = cboPhienDangChoi.Items.Cast<SessionOption>().ToList()
                        .FindIndex(x => x.SessionId == previousSession);
                    cboPhienDangChoi.SelectedIndex = pos >= 0 ? pos : 0;
                }

                if (_selectedServiceId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvDanhMucDichVu.Rows)
                        if (Convert.ToInt32(row.Cells["MaDichVu"].Value) == _selectedServiceId.Value)
                        {
                            row.Selected = true;
                            dgvDanhMucDichVu.CurrentCell = row.Cells[0];
                            break;
                        }
                }
                btnGoiMonVaoPhien.Enabled = cboPhienDangChoi.Items.Count > 0 &&
                    cboMonGoi.Items.Count > 0;
            }
            finally { _loadingFnB = false; }

            DgvDanhMucDichVu_SelectionChanged(null, EventArgs.Empty);
            RefreshSessionServiceGrid();
        }

        private void DgvDanhMucDichVu_SelectionChanged(object? sender, EventArgs e)
        {
            if (_loadingFnB || dgvDanhMucDichVu.CurrentRow?.DataBoundItem == null) return;
            int id = Convert.ToInt32(dgvDanhMucDichVu.CurrentRow.Cells["MaDichVu"].Value);
            ServiceItem? item = _serviceItems.FirstOrDefault(x => x.ServiceId == id);
            if (item == null) return;
            _selectedServiceId = id;
            txtServiceName.Text = item.ServiceName;
            cboServiceCategory.SelectedItem = item.Category;
            if (cboServiceCategory.SelectedIndex < 0) cboServiceCategory.SelectedIndex = 0;
            txtServicePrice.Text = item.UnitPrice.ToString("0.##", CultureInfo.InvariantCulture);
            txtServiceStock.Text = item.StockQuantity.ToString(CultureInfo.InvariantCulture);
            chkServiceActive.Checked = item.IsActive;
        }

        private void RefreshSessionServiceGrid()
        {
            if (_loadingFnB) return;
            if (cboPhienDangChoi.SelectedItem is not SessionOption session)
            {
                dgvDichVuTheoPhien.DataSource = null;
                lblTongDichVuPhien.Text = "Chưa có máy nào đang chơi";
                btnGoiMonVaoPhien.Enabled = false;
                return;
            }
            try
            {
                List<SessionServiceDetail> orders = _dbBilling.GetSessionServices(session.SessionId);
                dgvDichVuTheoPhien.DataSource = orders.Select(x => new
                {
                    MaDon = x.SessionServiceId, TenMon = x.ServiceName,
                    SoLuong = x.Quantity, DonGia = x.UnitPrice,
                    ThanhTien = x.TotalPrice,
                    ThoiGian = x.OrderedAt.ToString("dd/MM/yyyy HH:mm"),
                    GhiChu = x.Note
                }).ToList();
                lblTongDichVuPhien.Text = $"Tổng F&B phiên: {orders.Sum(x => x.TotalPrice):N0} VNĐ";
                btnGoiMonVaoPhien.Enabled = cboMonGoi.SelectedItem is ServiceOption;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được món của phiên: {ex.Message}");
            }
        }

        private bool TryBuildServiceItemFromInputs(out ServiceItem? item)
        {
            item = null;
            string name = txtServiceName.Text.Trim();
            string category = cboServiceCategory.SelectedItem?.ToString() ?? "Đồ uống";
            if (name.Length == 0 || name.Length > 100 || category.Length > 50)
            {
                MessageBox.Show("Tên món phải có từ 1 đến 100 ký tự và phân loại tối đa 50 ký tự.");
                return false;
            }
            if (!TryParseDecimalInput(txtServicePrice.Text, out decimal price) || price < 0m)
            {
                MessageBox.Show("Đơn giá phải là số hợp lệ và không được âm.");
                return false;
            }
            if (!int.TryParse(txtServiceStock.Text.Trim(), out int stock) || stock < 0)
            {
                MessageBox.Show("Tồn kho phải là số nguyên không âm.");
                return false;
            }
            item = new ServiceItem
            {
                ServiceName = name, Category = category,
                UnitPrice = price, StockQuantity = stock, IsActive = chkServiceActive.Checked
            };
            return true;
        }

        private void BtnThemMon_Click(object? sender, EventArgs e)
        {
            if (!TryBuildServiceItemFromInputs(out ServiceItem? item) || item == null) return;
            try
            {
                if (_dbBilling.AddServiceItem(item))
                {
                    _selectedServiceId = item.ServiceId;
                    LoadFnBData();
                    OnBillingNotification?.Invoke($"Đã thêm món mới: {item.ServiceName}");
                    MessageBox.Show("Thêm món thành công.");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Không thể thêm món: {ex.Message}"); }
        }

        private void BtnCapNhatMon_Click(object? sender, EventArgs e)
        {
            if (dgvDanhMucDichVu.CurrentRow?.DataBoundItem == null) return;
            if (!TryBuildServiceItemFromInputs(out ServiceItem? item) || item == null) return;
            item.ServiceId = Convert.ToInt32(dgvDanhMucDichVu.CurrentRow.Cells["MaDichVu"].Value);
            try
            {
                if (_dbBilling.UpdateServiceItem(item))
                {
                    _selectedServiceId = item.ServiceId;
                    LoadFnBData();
                    OnBillingNotification?.Invoke($"Đã cập nhật món: {item.ServiceName}");
                    MessageBox.Show("Cập nhật món thành công.");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Không thể sửa món: {ex.Message}"); }
        }

        private void BtnBatTatMon_Click(object? sender, EventArgs e)
        {
            if (dgvDanhMucDichVu.CurrentRow?.DataBoundItem == null) return;
            int id = Convert.ToInt32(dgvDanhMucDichVu.CurrentRow.Cells["MaDichVu"].Value);
            try
            {
                if (_dbBilling.ToggleServiceItemActive(id))
                {
                    _selectedServiceId = id;
                    LoadFnBData();
                    OnBillingNotification?.Invoke("Đã đổi trạng thái kinh doanh món F&B");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Không thể đổi trạng thái món: {ex.Message}"); }
        }

        private void BtnNhapKho_Click(object? sender, EventArgs e)
        {
            if (dgvDanhMucDichVu.CurrentRow?.DataBoundItem == null) return;
            int id = Convert.ToInt32(dgvDanhMucDichVu.CurrentRow.Cells["MaDichVu"].Value);
            try
            {
                if (_dbBilling.RestockServiceItem(id, (int)nudRestockQty.Value))
                {
                    _selectedServiceId = id;
                    LoadFnBData();
                    OnBillingNotification?.Invoke("Đã nhập thêm hàng F&B");
                    MessageBox.Show("Nhập kho thành công.");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Không thể nhập kho: {ex.Message}"); }
        }

        private void BtnGoiMonVaoPhien_Click(object? sender, EventArgs e)
        {
            if (cboPhienDangChoi.SelectedItem is not SessionOption session)
            {
                MessageBox.Show("Chưa có máy nào đang chơi. Hãy mở máy từ màn hình chính trước.");
                return;
            }
            if (cboMonGoi.SelectedItem is not ServiceOption selected)
            {
                MessageBox.Show("Vui lòng chọn món còn hàng.");
                return;
            }

            try
            {
                if (_dbBilling.OrderServiceForSession(session.SessionId, selected.ServiceId,
                    (int)nudSoLuongGoi.Value, txtGhiChuGoiMon.Text))
                {
                    LoadFnBData();
                    _sessionList = _dbBilling.GetAllSessions();
                    ApplyRevenueFilter(reloadFromDatabase: false);
                    OnBillingNotification?.Invoke($"Đã gọi món vào phiên #{session.SessionId}");
                    MessageBox.Show("Đã thêm món vào phiên đang chơi.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gọi món: {ex.Message}", "Lỗi gọi món",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
