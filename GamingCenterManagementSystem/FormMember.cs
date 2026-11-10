using System;
using System.Collections.Generic;
using System.Linq;
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

        // Delegate / Action for top-up notification
        public Action<string, decimal>? OnBalanceTopUpSuccess;

        public FormMember()
        {
            InitializeComponent();
            SetupEvents();
            LoadData();
        }

        private void SetupEvents()
        {
            Load += (s, e) => LoadData();
            btnThemHoiVien.Click += BtnThemHoiVien_Click;
            btnSuaHoiVien.Click += BtnSuaHoiVien_Click;
            btnXoaHoiVien.Click += BtnXoaHoiVien_Click;
            btnLamMoi.Click += (s, e) => { txtTimKiem.Clear(); LoadData(); };
            btnTimKiem.Click += (s, e) => SearchUsers();
            btnDatLaiTimKiem.Click += (s, e) => { txtTimKiem.Clear(); LoadData(); };
            txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchUsers(); };

            dgvHoiVien.SelectionChanged += DgvHoiVien_SelectionChanged;

            // Quick amount buttons
            btnCong20k.Click += (s, e) => SetQuickAmount(20000);
            btnCong50k.Click += (s, e) => SetQuickAmount(50000);
            btnCong100k.Click += (s, e) => SetQuickAmount(100000);
            btnCong200k.Click += (s, e) => SetQuickAmount(200000);

            btnNapTien.Click += BtnNapTien_Click;

            // Delegate event
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
                BindGrid(_userList);
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindGrid(List<AppUser> users)
        {
            // LINQ Projection for DataGridView display (Applying LINQ to Objects)
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
            // Applying LINQ Count and Sum queries
            int total = _userList.Count;
            decimal totalBalance = _userList.Sum(u => u.Balance);
            int vipCount = _userList.Count(u => u.MemberTier == "Silver" || u.MemberTier == "Gold");

            lblTongHoiVien.Text = $"👥 Tổng hội viên: {total}";
            lblTongSoDuVi.Text = $"💰 Tổng quỹ ví: {totalBalance:N0} VNĐ";
            lblHoiVienVip.Text = $"⭐ Hội viên VIP (Gold/Silver): {vipCount}";
        }

        private void SearchUsers()
        {
            string keyword = txtTimKiem.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                BindGrid(_userList);
                return;
            }

            // LINQ Where search
            var filtered = _userList.Where(u =>
                u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                u.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                u.Role.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            BindGrid(filtered);
        }

        private void DgvHoiVien_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvHoiVien.CurrentRow != null && dgvHoiVien.CurrentRow.DataBoundItem != null)
            {
                int userId = Convert.ToInt32(dgvHoiVien.CurrentRow.Cells["MaHoiVien"].Value);
                _selectedUser = _userList.FirstOrDefault(u => u.UserId == userId);
                if (_selectedUser != null)
                {
                    txtUserId.Text = _selectedUser.UserId.ToString();
                    txtUsername.Text = _selectedUser.Username;
                    txtPassword.Text = _selectedUser.PasswordHash;
                    txtFullName.Text = _selectedUser.FullName;
                    cboMemberTier.SelectedItem = _selectedUser.MemberTier;
                    cboRole.SelectedItem = _selectedUser.Role;
                    txtBalance.Text = $"{_selectedUser.Balance:N0} VNĐ";

                    lblDangChon.Text = $"Tài khoản nhận: {_selectedUser.Username} ({_selectedUser.FullName})";
                }
            }
        }

        private void BtnThemHoiVien_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và họ tên hội viên!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check duplicate username with LINQ Any
            if (_userList.Any(u => u.Username.Equals(txtUsername.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại trong hệ thống!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newUser = new AppUser
            {
                Username = txtUsername.Text.Trim(),
                PasswordHash = string.IsNullOrWhiteSpace(txtPassword.Text) ? "123456" : txtPassword.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Role = cboRole.SelectedItem?.ToString() ?? "Member",
                MemberTier = cboMemberTier.SelectedItem?.ToString() ?? "Standard",
                Balance = 0
            };

            if (_dbUser.Add(newUser))
            {
                MessageBox.Show($"Tạo tài khoản hội viên {newUser.Username} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnSuaHoiVien_Click(object? sender, EventArgs e)
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

            if (_dbUser.Update(user))
            {
                MessageBox.Show($"Cập nhật hội viên {user.Username} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        private void BtnXoaHoiVien_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text))
            {
                MessageBox.Show("Vui lòng chọn hội viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtUserId.Text);
            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa hội viên {txtUsername.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                if (_dbUser.Delete(id))
                {
                    MessageBox.Show("Xóa tài khoản hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            }
        }

        private void BtnNapTien_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản hội viên từ danh sách để nạp tiền!", "Chưa chọn tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cleanText = txtSoTienNap.Text.Replace(".", "").Replace(",", "").Trim();
            if (!decimal.TryParse(cleanText, out decimal amount) || amount <= 0 || amount > 50000000)
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

            if (confirm == DialogResult.Yes)
            {
                if (_dbUser.TopUpBalance(_selectedUser.UserId, amount))
                {
                    MessageBox.Show($"Nạp thành công {amount:N0} VNĐ cho tài khoản {_selectedUser.Username}!", "Giao dịch thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnBalanceTopUpSuccess?.Invoke(_selectedUser.Username, amount);
                    LoadData();
                }
            }
        }
    }
}
