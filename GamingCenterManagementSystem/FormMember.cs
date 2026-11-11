using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormMember : Form
    {
        private readonly DBUser _dbUser = new();
        private List<AppUser> _userList = new();
        private AppUser? _selectedUser = null;

        // Delegate / Action notifications for parent Dashboard sync
        public Action<string, decimal>? OnBalanceTopUpSuccess;
        public Action<string>? OnMemberChanged;

        public FormMember()
        {
            InitializeComponent();
            InitDefaults();
            SetupEvents();
            LoadData();
        }

        private void InitDefaults()
        {
            cboLocHangThe.SelectedIndex = 0;
            cboHinhThucNap.SelectedIndex = 0;
            cboRole.SelectedItem = "Member";
            cboMemberTier.SelectedItem = "Standard";
        }

        private void SetupEvents()
        {
            Load += async (s, e) => await LoadDataAsync();
            btnThemHoiVien.Click += async (s, e) => await BtnThemHoiVien_ClickAsync();
            btnSuaHoiVien.Click += async (s, e) => await BtnSuaHoiVien_ClickAsync();
            btnXoaHoiVien.Click += async (s, e) => await BtnXoaHoiVien_ClickAsync();
            btnLamMoi.Click += async (s, e) =>
            {
                txtTimKiem.Clear();
                cboLocHangThe.SelectedIndex = 0;
                await LoadDataAsync();
                ResetEditorToCreateMode();
            };

            // Logic 2 (Nam): Instant LINQ search on cached _userList + tier filter
            txtTimKiem.TextChanged += (s, e) => SearchUsers();
            cboLocHangThe.SelectedIndexChanged += (s, e) => SearchUsers();
            btnTimKiem.Click += (s, e) => SearchUsers();
            btnDatLaiTimKiem.Click += async (s, e) =>
            {
                txtTimKiem.Clear();
                cboLocHangThe.SelectedIndex = 0;
                await LoadDataAsync();
            };
            txtTimKiem.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SearchUsers();
                }
            };

            dgvHoiVien.SelectionChanged += DgvHoiVien_SelectionChanged;

            // Quick amount buttons
            btnCong20k.Click += (s, e) => SetQuickAmount(20000);
            btnCong50k.Click += (s, e) => SetQuickAmount(50000);
            btnCong100k.Click += (s, e) => SetQuickAmount(100000);
            btnCong200k.Click += (s, e) => SetQuickAmount(200000);

            btnNapTien.Click += async (s, e) => await BtnNapTien_ClickAsync();
            btnDoiMatKhau.Click += async (s, e) => await BtnDoiMatKhau_ClickAsync();

            OnBalanceTopUpSuccess += (username, amount) =>
            {
                // Can be wired to logging or parent dashboard
            };
        }

        private void SetQuickAmount(decimal amount)
        {
            string clean = txtSoTienNap.Text.Replace(".", "").Replace(",", "").Trim();
            if (decimal.TryParse(clean, out decimal current))
            {
                txtSoTienNap.Text = (current + amount).ToString("F0");
            }
            else
            {
                txtSoTienNap.Text = amount.ToString("F0");
            }
        }

        private void LoadData()
        {
            try
            {
                _userList = _dbUser.GetAll();
                ApplyCurrentSearchAndStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _userList = await _dbUser.GetAllAsync();
                ApplyCurrentSearchAndStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyCurrentSearchAndStatistics()
        {
            SearchUsers();
            UpdateStatistics();
        }

        private void BindGrid(List<AppUser> users)
        {
            // LINQ Projection for DataGridView display
            var displayData = users.Select(u => new
            {
                MaHoiVien = u.UserId,
                TenDangNhap = u.Username,
                HoVaTen = u.FullName,
                VaiTro = u.Role,
                SoDuVi = $"{u.Balance:N0} VNĐ",
                HangThe = u.MemberTier,
                NgayTao = u.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            dgvHoiVien.DataSource = displayData;

            if (dgvHoiVien.Columns["MaHoiVien"] is DataGridViewColumn col1) col1.HeaderText = "Mã";
            if (dgvHoiVien.Columns["TenDangNhap"] is DataGridViewColumn col2) col2.HeaderText = "Tên Đăng Nhập";
            if (dgvHoiVien.Columns["HoVaTen"] is DataGridViewColumn col3) col3.HeaderText = "Họ và Tên";
            if (dgvHoiVien.Columns["VaiTro"] is DataGridViewColumn col4) col4.HeaderText = "Vai Trò";
            if (dgvHoiVien.Columns["SoDuVi"] is DataGridViewColumn col5) col5.HeaderText = "Số Dư Ví";
            if (dgvHoiVien.Columns["HangThe"] is DataGridViewColumn col6) col6.HeaderText = "Hạng Thẻ";
            if (dgvHoiVien.Columns["NgayTao"] is DataGridViewColumn col7) col7.HeaderText = "Ngày Đăng Ký";
        }

        private void UpdateStatistics()
        {
            int total = _userList.Count;
            decimal totalBalance = _userList.Sum(u => u.Balance);
            int vipCount = _userList.Count(u =>
                u.MemberTier.Equals("Silver", StringComparison.OrdinalIgnoreCase) ||
                u.MemberTier.Equals("Gold", StringComparison.OrdinalIgnoreCase));

            lblTongHoiVien.Text = $"👥 Tổng hội viên: {total}";
            lblTongSoDuVi.Text = $"💰 Tổng quỹ ví: {totalBalance:N0} VNĐ";
            lblHoiVienVip.Text = $"⭐ Hội viên VIP (Gold/Silver): {vipCount}";
        }

        private void SearchUsers()
        {
            string keyword = txtTimKiem.Text.Trim();
            string? tierFilter = cboLocHangThe.SelectedItem?.ToString();
            var filtered = DBUser.SearchUsers(_userList, keyword, tierFilter: tierFilter);
            BindGrid(filtered);
        }

        private void ResetEditorToCreateMode()
        {
            dgvHoiVien.ClearSelection();
            _selectedUser = null;
            txtUserId.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtPassword.PlaceholderText = "Tối thiểu 6 ký tự";
            txtFullName.Clear();
            cboRole.SelectedItem = "Member";
            cboMemberTier.SelectedItem = "Standard";
            txtBalance.ReadOnly = false;
            txtBalance.Text = DBUser.MinimumMemberInitialBalance.ToString("F0", CultureInfo.InvariantCulture);
            lblDangChon.Text = "Tài khoản nhận: Chưa chọn tài khoản";
            txtUsername.Focus();
        }

        private void DgvHoiVien_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvHoiVien.CurrentRow == null ||
                dgvHoiVien.CurrentRow.DataBoundItem == null ||
                !dgvHoiVien.Columns.Contains("MaHoiVien") ||
                dgvHoiVien.CurrentRow.Cells["MaHoiVien"].Value == null)
            {
                _selectedUser = null;
                return;
            }

            int userId = Convert.ToInt32(dgvHoiVien.CurrentRow.Cells["MaHoiVien"].Value);
            _selectedUser = _userList.FirstOrDefault(u => u.UserId == userId);
            if (_selectedUser != null)
            {
                txtUserId.Text = _selectedUser.UserId.ToString();
                txtUsername.Text = _selectedUser.Username;
                txtPassword.Clear();
                txtPassword.PlaceholderText = "Để trống (dùng khung Đổi Mật Khẩu)";
                txtFullName.Text = _selectedUser.FullName;
                cboMemberTier.SelectedItem = _selectedUser.MemberTier;
                cboRole.SelectedItem = _selectedUser.Role;
                txtBalance.ReadOnly = true;
                txtBalance.Text = $"{_selectedUser.Balance:N0} VNĐ";

                txtDoiMatKhauUser.Text = _selectedUser.Username;
                lblDangChon.Text = $"Tài khoản nhận: {_selectedUser.Username} ({_selectedUser.FullName})";
            }
        }

        private static bool TryParseBalanceText(string? rawText, out decimal balance)
        {
            balance = 0m;
            if (string.IsNullOrWhiteSpace(rawText))
                return false;

            string cleaned = rawText
                .Replace("VNĐ", "", StringComparison.OrdinalIgnoreCase)
                .Replace("VND", "", StringComparison.OrdinalIgnoreCase)
                .Replace("đ", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d{1,3}(\.\d{3})+$"))
                cleaned = cleaned.Replace(".", "");
            else if (System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d{1,3}(,\d{3})+$"))
                cleaned = cleaned.Replace(",", "");

            if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out balance))
                return true;

            if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.CurrentCulture, out balance))
                return true;

            return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out balance);
        }

        private async Task BtnThemHoiVien_ClickAsync()
        {
            string role = cboRole.SelectedItem?.ToString() ?? "Member";
            string requestedTier = cboMemberTier.SelectedItem?.ToString() ?? "Standard";

            if (!TryParseBalanceText(txtBalance.Text, out decimal initialBalance))
            {
                MessageBox.Show(
                    "Số dư ban đầu không hợp lệ! Vui lòng nhập số hợp lệ (hội viên tối thiểu 10.000 VNĐ).",
                    "Kiểm tra dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtBalance.ReadOnly = false;
                txtBalance.Focus();
                return;
            }

            var newUser = new AppUser
            {
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Role = role,
                MemberTier = requestedTier,
                Balance = Math.Max(0m, initialBalance)
            };

            if (initialBalance < 0)
            {
                MessageBox.Show("Số dư ban đầu không được âm.", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBalance.ReadOnly = false;
                txtBalance.Focus();
                return;
            }

            try
            {
                var (success, errorMessage) = await _dbUser.CreateMemberAsync(newUser, txtPassword.Text);
                if (!success)
                {
                    MessageBox.Show(errorMessage, "Không thể tạo tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (errorMessage.Contains("Số dư", StringComparison.OrdinalIgnoreCase))
                    {
                        txtBalance.ReadOnly = false;
                        txtBalance.Text = DBUser.MinimumMemberInitialBalance.ToString("F0", CultureInfo.InvariantCulture);
                        txtBalance.Focus();
                        txtBalance.SelectAll();
                    }
                    return;
                }

                MessageBox.Show(
                    $"Tạo tài khoản '{newUser.Username}' thành công!\nHạng thẻ: {newUser.MemberTier} | Số dư: {newUser.Balance:N0} VNĐ",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                OnMemberChanged?.Invoke($"Đã thêm tài khoản: {newUser.Username}");
                await LoadDataAsync();
                ResetEditorToCreateMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnSuaHoiVien_ClickAsync()
        {
            if (string.IsNullOrEmpty(txtUserId.Text))
            {
                MessageBox.Show("Vui lòng chọn hội viên cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtUserId.Text);
            var user = new AppUser
            {
                UserId = id,
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Role = cboRole.SelectedItem?.ToString() ?? "Member",
                MemberTier = cboMemberTier.SelectedItem?.ToString() ?? "Standard"
            };

            try
            {
                bool updated = await Task.Run(() => _dbUser.Update(user));
                if (updated)
                {
                    MessageBox.Show($"Cập nhật hội viên {user.Username} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnMemberChanged?.Invoke($"Đã cập nhật tài khoản: {user.Username}");
                    await LoadDataAsync();
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnXoaHoiVien_ClickAsync()
        {
            if (string.IsNullOrEmpty(txtUserId.Text))
            {
                MessageBox.Show("Vui lòng chọn hội viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtUserId.Text);
            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa hội viên {txtUsername.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                bool deleted = await Task.Run(() => _dbUser.Delete(id));
                if (deleted)
                {
                    MessageBox.Show("Xóa tài khoản hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnMemberChanged?.Invoke($"Đã xóa tài khoản ID: {id}");
                    await LoadDataAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnNapTien_ClickAsync()
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản hội viên từ danh sách để nạp tiền!", "Chưa chọn tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cleanText = txtSoTienNap.Text.Replace(".", "").Replace(",", "").Trim();
            if (!decimal.TryParse(cleanText, out decimal amount) || amount <= 0 || amount > DBUser.MaximumTopUpAmount)
            {
                MessageBox.Show("Số tiền nạp không hợp lệ! Vui lòng nhập số tiền từ 1 VNĐ đến 50,000,000 VNĐ.", "Lỗi nhập tiền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hinhThuc = cboHinhThucNap.SelectedItem?.ToString() ?? "Tiền mặt tại quầy";

            var confirm = MessageBox.Show(
                $"Xác nhận nạp {amount:N0} VNĐ vào ví của hội viên: {_selectedUser.Username}?\nHình thức: {hinhThuc}",
                "Xác nhận giao dịch ví",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            string targetUsername = _selectedUser.Username;
            int targetUserId = _selectedUser.UserId;

            if (await Task.Run(() => _dbUser.TopUpBalance(targetUserId, amount)))
            {
                MessageBox.Show($"Nạp thành công {amount:N0} VNĐ cho tài khoản {targetUsername}!", "Giao dịch thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnBalanceTopUpSuccess?.Invoke(targetUsername, amount);
                OnMemberChanged?.Invoke($"Đã nạp {amount:N0} VNĐ cho {targetUsername}");
                await LoadDataAsync();
            }
        }

        private async Task BtnDoiMatKhau_ClickAsync()
        {
            string username = txtDoiMatKhauUser.Text.Trim();
            string oldPassword = txtMatKhauCu.Text;
            string newPassword = txtMatKhauMoi.Text;

            try
            {
                var (success, errorMessage) = await _dbUser.ChangePasswordAsync(username, oldPassword, newPassword);
                if (success)
                {
                    MessageBox.Show(
                        $"Đổi mật khẩu cho tài khoản '{username}' thành công (đã mã hóa PBKDF2)!",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    txtMatKhauCu.Clear();
                    txtMatKhauMoi.Clear();
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Đổi mật khẩu thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đổi mật khẩu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
