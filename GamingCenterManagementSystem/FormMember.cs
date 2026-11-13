using System;
using System.Collections.Generic;
using System.Drawing;
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
        private bool _skipInitialLoadEvent = false;
        private bool _isBindingGrid = false;
        private Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> _messageBox = MessageBox.Show;

        internal void SetPromptHandler(Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? handler)
        {
            _messageBox = handler ?? MessageBox.Show;
        }

        // Delegate / Action notifications for parent Dashboard sync
        public Action<string, decimal>? OnBalanceTopUpSuccess;
        public Action<string>? OnMemberChanged;

        public FormMember()
        {
            InitializeComponent();
            InitDefaults();
            SetupEvents();
            LoadData();
            _skipInitialLoadEvent = true;
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
            Load += async (s, e) =>
            {
                if (_skipInitialLoadEvent)
                {
                    _skipInitialLoadEvent = false;
                    return;
                }
                await LoadDataAsync();
            };

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
        }

        private void SetQuickAmount(decimal amount)
        {
            if (DBUser.TryParseCurrencyAmount(txtSoTienNap.Text, out decimal current) && current >= 0)
            {
                txtSoTienNap.Text = (current + amount).ToString("F0", CultureInfo.InvariantCulture);
            }
            else
            {
                txtSoTienNap.Text = amount.ToString("F0", CultureInfo.InvariantCulture);
            }
        }

        private void LoadData(int? selectUserId = null)
        {
            try
            {
                _userList = _dbUser.GetAll();
                ApplyCurrentSearchAndStatistics(selectUserId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync(int? selectUserId = null)
        {
            try
            {
                _userList = await _dbUser.GetAllAsync();
                ApplyCurrentSearchAndStatistics(selectUserId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyCurrentSearchAndStatistics(int? selectUserId = null)
        {
            SearchUsers(selectUserId);
            UpdateStatistics();
        }

        private void BindGrid(List<AppUser> users, int? selectUserId = null)
        {
            _isBindingGrid = true;
            try
            {
                // LINQ Projection for DataGridView display
                var displayData = users.Select(u => new
                {
                    MaHoiVien = u.UserId,
                    TenDangNhap = u.Username,
                    HoVaTen = u.FullName,
                    SoCCCD = string.IsNullOrWhiteSpace(u.CitizenId) ? "Chưa khai báo" : u.CitizenId,
                    VaiTro = u.Role,
                    SoDuVi = $"{u.Balance:N0} VNĐ",
                    HangThe = u.MemberTier,
                    NgayTao = u.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

                dgvHoiVien.DataSource = displayData;

                if (dgvHoiVien.Columns["MaHoiVien"] is DataGridViewColumn col1) col1.HeaderText = "Mã";
                if (dgvHoiVien.Columns["TenDangNhap"] is DataGridViewColumn col2) col2.HeaderText = "Tên Đăng Nhập";
                if (dgvHoiVien.Columns["HoVaTen"] is DataGridViewColumn col3) col3.HeaderText = "Họ và Tên";
                if (dgvHoiVien.Columns["SoCCCD"] is DataGridViewColumn colCccd) colCccd.HeaderText = "Số CCCD/CMND";
                if (dgvHoiVien.Columns["VaiTro"] is DataGridViewColumn col4) col4.HeaderText = "Vai Trò";
                if (dgvHoiVien.Columns["SoDuVi"] is DataGridViewColumn col5) col5.HeaderText = "Số Dư Ví";
                if (dgvHoiVien.Columns["HangThe"] is DataGridViewColumn col6) col6.HeaderText = "Hạng Thẻ";
                if (dgvHoiVien.Columns["NgayTao"] is DataGridViewColumn col7) col7.HeaderText = "Ngày Đăng Ký";

                if (selectUserId.HasValue && dgvHoiVien.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgvHoiVien.Rows)
                    {
                        if (row.Cells["MaHoiVien"].Value != null &&
                            Convert.ToInt32(row.Cells["MaHoiVien"].Value) == selectUserId.Value)
                        {
                            dgvHoiVien.CurrentCell = row.Cells[0];
                            row.Selected = true;
                            break;
                        }
                    }
                }
            }
            finally
            {
                _isBindingGrid = false;
            }

            DgvHoiVien_SelectionChanged(dgvHoiVien, EventArgs.Empty);
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

        private void SearchUsers(int? selectUserId = null)
        {
            string keyword = txtTimKiem.Text.Trim();
            string? tierFilter = cboLocHangThe.SelectedItem?.ToString();
            var filtered = DBUser.SearchUsers(_userList, keyword, tierFilter: tierFilter);
            BindGrid(filtered, selectUserId);
        }

        private void SetBalanceEditable(bool editable, string? text = null)
        {
            txtBalance.ReadOnly = !editable;
            txtBalance.BackColor = editable ? Color.White : Color.FromArgb(236, 240, 246);
            if (text != null)
            {
                txtBalance.Text = text;
            }
        }

        private void ClearSelectionEditorFields()
        {
            _selectedUser = null;
            txtUserId.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtPassword.PlaceholderText = "Tối thiểu 6 ký tự";
            txtFullName.Clear();
            txtCccd.Clear();
            cboRole.SelectedItem = "Member";
            cboMemberTier.SelectedItem = "Standard";
            SetBalanceEditable(true, DBUser.MinimumMemberInitialBalance.ToString("F0", CultureInfo.InvariantCulture));
            txtDoiMatKhauUser.Clear();
            lblDangChon.Text = "Tài khoản nhận: Chưa chọn tài khoản";
        }

        private void ResetEditorToCreateMode()
        {
            _isBindingGrid = true;
            try
            {
                if (dgvHoiVien.Rows.Count > 0)
                {
                    dgvHoiVien.CurrentCell = null;
                    dgvHoiVien.ClearSelection();
                }
            }
            finally
            {
                _isBindingGrid = false;
            }

            ClearSelectionEditorFields();
            txtUsername.Focus();
        }

        private void DgvHoiVien_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isBindingGrid)
                return;

            if (dgvHoiVien.CurrentRow == null ||
                dgvHoiVien.CurrentRow.DataBoundItem == null ||
                !dgvHoiVien.Columns.Contains("MaHoiVien") ||
                dgvHoiVien.CurrentRow.Cells["MaHoiVien"].Value == null)
            {
                ClearSelectionEditorFields();
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
                txtCccd.Text = _selectedUser.CitizenId ?? string.Empty;
                cboMemberTier.SelectedItem = _selectedUser.MemberTier;
                cboRole.SelectedItem = _selectedUser.Role;
                SetBalanceEditable(false, $"{_selectedUser.Balance:N0} VNĐ");

                txtDoiMatKhauUser.Text = _selectedUser.Username;
                lblDangChon.Text = $"Tài khoản nhận: {_selectedUser.Username} ({_selectedUser.FullName})";
            }
            else
            {
                ClearSelectionEditorFields();
            }
        }

        internal static bool TryParseBalanceText(string? rawText, out decimal balance) =>
            DBUser.TryParseCurrencyAmount(rawText, out balance);

        private async Task BtnThemHoiVien_ClickAsync()
        {
            if (txtBalance.ReadOnly || _selectedUser != null || !string.IsNullOrEmpty(txtUserId.Text))
            {
                string previousUsername = _selectedUser?.Username ?? txtUsername.Text.Trim();
                bool sameUsername = _selectedUser != null &&
                    string.Equals(txtUsername.Text.Trim(), _selectedUser.Username, StringComparison.OrdinalIgnoreCase);

                _selectedUser = null;
                txtUserId.Clear();
                if (dgvHoiVien.Rows.Count > 0)
                {
                    _isBindingGrid = true;
                    try
                    {
                        dgvHoiVien.CurrentCell = null;
                        dgvHoiVien.ClearSelection();
                    }
                    finally
                    {
                        _isBindingGrid = false;
                    }
                }
                SetBalanceEditable(true, DBUser.MinimumMemberInitialBalance.ToString("F0", CultureInfo.InvariantCulture));
                lblDangChon.Text = "Tài khoản nhận: Chưa chọn tài khoản";

                string hint = sameUsername
                    ? $"Đang chọn tài khoản '{previousUsername}'. Hệ thống đã chuyển sang chế độ tạo mới — vui lòng nhập Username mới, mật khẩu và Số dư ban đầu rồi bấm '➕ Thêm Mới'."
                    : "Ô Số dư ví trước đó đang khóa theo dòng đã chọn. Hệ thống đã mở khóa về mặc định 10.000 VNĐ — vui lòng kiểm tra lại số dư ban đầu rồi bấm '➕ Thêm Mới' lần nữa.";
                MessageBox.Show(hint, "Chế độ tạo tài khoản mới", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (sameUsername)
                    txtUsername.Focus();
                else
                {
                    txtBalance.Focus();
                    txtBalance.SelectAll();
                }
                return;
            }

            string role = cboRole.SelectedItem?.ToString() ?? "Member";
            string requestedTier = cboMemberTier.SelectedItem?.ToString() ?? "Standard";

            if (!TryParseBalanceText(txtBalance.Text, out decimal initialBalance))
            {
                MessageBox.Show(
                    "Số dư ban đầu không hợp lệ! Vui lòng nhập số hợp lệ (hội viên tối thiểu 10.000 VNĐ).",
                    "Kiểm tra dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                SetBalanceEditable(true);
                txtBalance.Focus();
                return;
            }

            string cccd = txtCccd.Text.Trim();
            if (string.IsNullOrWhiteSpace(cccd))
            {
                _messageBox(
                    "Theo quy định Nghị định 72/2013/NĐ-CP & 27/2018/NĐ-CP, bắt buộc phải khai báo CCCD/CMND khi tạo tài khoản hội viên!",
                    "Thiếu số CCCD/CMND",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCccd.Focus();
                return;
            }

            if (!DBUser.IsValidCitizenId(cccd))
            {
                _messageBox(
                    "Số CCCD/CMND không đúng định dạng! Vui lòng nhập đúng 12 chữ số (CCCD) hoặc 9 chữ số (CMND cũ).",
                    "CCCD/CMND không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCccd.Focus();
                txtCccd.SelectAll();
                return;
            }

            if (_dbUser.IsCitizenIdExists(cccd))
            {
                _messageBox(
                    "Số CCCD/CMND này đã được đăng ký cho một tài khoản khác trong hệ thống!",
                    "Trùng lặp CCCD/CMND",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCccd.Focus();
                txtCccd.SelectAll();
                return;
            }

            if (!DBUser.ValidateMemberInput(txtUsername.Text, txtFullName.Text, txtPassword.Text, initialBalance, role, cccd, out string validationError, requireCitizenId: true))
            {
                _messageBox(validationError, "Không thể tạo tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (validationError.Contains("Số dư", StringComparison.OrdinalIgnoreCase))
                {
                    SetBalanceEditable(true, DBUser.MinimumMemberInitialBalance.ToString("F0", CultureInfo.InvariantCulture));
                    txtBalance.Focus();
                    txtBalance.SelectAll();
                }
                return;
            }

            var newUser = new AppUser
            {
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                CitizenId = cccd,
                Role = role,
                MemberTier = requestedTier,
                Balance = initialBalance
            };

            try
            {
                var (success, errorMessage) = await _dbUser.CreateMemberAsync(newUser, txtPassword.Text);
                if (!success)
                {
                    _messageBox(errorMessage, "Không thể tạo tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _messageBox(
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
                _messageBox($"Lỗi tạo tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnSuaHoiVien_ClickAsync()
        {
            if (_selectedUser == null || !int.TryParse(txtUserId.Text, out int id))
            {
                _messageBox("Vui lòng chọn hội viên cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(txtPassword.Text))
            {
                _messageBox(
                    "Nút '✏️ Cập Nhật' không dùng để đổi mật khẩu. Vui lòng xóa ô Mật khẩu hoặc dùng khung 'Đổi Mật Khẩu Hội Viên' bên dưới.",
                    "Lưu ý bảo mật",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtPassword.Clear();
                return;
            }

            string role = cboRole.SelectedItem?.ToString() ?? "Member";
            string cccd = txtCccd.Text.Trim();

            if (string.Equals(role, "Member", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(cccd))
            {
                _messageBox(
                    "Theo quy định Nghị định 72/2013/NĐ-CP & 27/2018/NĐ-CP, bắt buộc phải khai báo CCCD/CMND cho tài khoản hội viên!",
                    "Thiếu số CCCD/CMND",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCccd.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(cccd))
            {
                if (!DBUser.IsValidCitizenId(cccd))
                {
                    _messageBox(
                        "Số CCCD/CMND không đúng định dạng! Vui lòng nhập đúng 12 chữ số (CCCD) hoặc 9 chữ số (CMND cũ).",
                        "CCCD/CMND không hợp lệ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtCccd.Focus();
                    return;
                }

                if (_dbUser.IsCitizenIdExists(cccd, id))
                {
                    _messageBox(
                        "Số CCCD/CMND này đã thuộc về tài khoản khác trong hệ thống!",
                        "Trùng lặp CCCD/CMND",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtCccd.Focus();
                    return;
                }
            }

            var user = new AppUser
            {
                UserId = id,
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                CitizenId = string.IsNullOrWhiteSpace(cccd) ? null : cccd,
                Role = role,
                MemberTier = cboMemberTier.SelectedItem?.ToString() ?? "Standard"
            };

            try
            {
                bool updated = await _dbUser.UpdateUserAsync(user);
                if (updated)
                {
                    _messageBox($"Cập nhật hội viên {user.Username} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnMemberChanged?.Invoke($"Đã cập nhật tài khoản: {user.Username}");
                    await LoadDataAsync(id);
                }
                else
                {
                    _messageBox("Không tìm thấy tài khoản cần cập nhật (có thể đã bị xóa).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadDataAsync();
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                _messageBox(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _messageBox($"Lỗi cập nhật hội viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnXoaHoiVien_ClickAsync()
        {
            if (_selectedUser == null || !int.TryParse(txtUserId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn hội viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa hội viên {txtUsername.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                bool deleted = await _dbUser.DeleteAsync(id);
                if (deleted)
                {
                    MessageBox.Show("Xóa tài khoản hội viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnMemberChanged?.Invoke($"Đã xóa tài khoản ID: {id}");
                    await LoadDataAsync();
                    if (_userList.Count == 0)
                        ResetEditorToCreateMode();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tài khoản cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            if (!TryParseBalanceText(txtSoTienNap.Text, out decimal amount) ||
                amount <= 0 ||
                amount > DBUser.MaximumTopUpAmount ||
                decimal.Round(amount, 2) != amount)
            {
                MessageBox.Show("Số tiền nạp không hợp lệ! Vui lòng nhập số tiền từ 1 VNĐ đến 50,000,000 VNĐ (tối đa 2 chữ số thập phân).", "Lỗi nhập tiền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            try
            {
                if (await _dbUser.TopUpBalanceAsync(targetUserId, amount, hinhThuc))
                {
                    MessageBox.Show($"Nạp thành công {amount:N0} VNĐ cho tài khoản {targetUsername}!", "Giao dịch thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnBalanceTopUpSuccess?.Invoke(targetUsername, amount);
                    OnMemberChanged?.Invoke($"Đã nạp {amount:N0} VNĐ cho {targetUsername}");
                    await LoadDataAsync(targetUserId);
                }
                else
                {
                    MessageBox.Show("Không thể nạp tiền cho tài khoản này (tài khoản không tồn tại hoặc số tiền không hợp lệ).", "Nạp tiền thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp tiền: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    int? targetId = _userList.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))?.UserId;
                    await LoadDataAsync(targetId);
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
