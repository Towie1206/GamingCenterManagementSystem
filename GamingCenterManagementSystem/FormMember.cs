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

        // Delegate / Action notifications for parent Dashboard sync
        public Action<string, decimal>? OnBalanceTopUpSuccess;
        public Action<string>? OnMemberChanged;

        public FormMember()
        {
            InitializeComponent();
            BuildPhase2Tabs();
            InitDefaults();
            SetupEvents();
            LoadData();
            _skipInitialLoadEvent = true;
        }

        private void InitDefaults()
        {
            cboLocHangThe.SelectedIndex = 0;
            cboLocVaiTro.SelectedIndex = 0;
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
                    await LoadTopMembersAsync();
                    return;
                }
                await LoadDataAsync();
                await LoadTopMembersAsync();
            };

            btnThemHoiVien.Click += async (s, e) => await BtnThemHoiVien_ClickAsync();
            btnSuaHoiVien.Click += async (s, e) => await BtnSuaHoiVien_ClickAsync();
            btnXoaHoiVien.Click += async (s, e) => await BtnKhoaMoKhoa_ClickAsync();
            btnLamMoi.Click += async (s, e) =>
            {
                txtTimKiem.Clear();
                cboLocHangThe.SelectedIndex = 0;
                cboLocVaiTro.SelectedIndex = 0;
                await LoadDataAsync();
                ResetEditorToCreateMode();
            };

            // Logic 2 (Nam): Instant LINQ search on cached _userList + tier filter
            txtTimKiem.TextChanged += (s, e) => SearchUsers();
            cboLocHangThe.SelectedIndexChanged += (s, e) => SearchUsers();
            cboLocVaiTro.SelectedIndexChanged += (s, e) => SearchUsers();
            btnTimKiem.Click += (s, e) => SearchUsers();
            btnDatLaiTimKiem.Click += async (s, e) =>
            {
                txtTimKiem.Clear();
                cboLocHangThe.SelectedIndex = 0;
                cboLocVaiTro.SelectedIndex = 0;
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
            btnTaiLaiLichSu.Click += async (s, e) => await LoadWalletTransactionsForSelectedUserAsync();
            tabMain.SelectedIndexChanged += async (s, e) =>
            {
                if (tabMain.SelectedTab == tabLichSuVi)
                    await LoadWalletTransactionsForSelectedUserAsync();
                else if (tabMain.SelectedTab == tabTopHoiVien)
                    await LoadTopMembersAsync();
            };
        }

        private void BuildPhase2Tabs()
        {
            pnlSearch.SuspendLayout();
            pnlSearch.Controls.Clear();
            pnlSearch.Size = new Size(770, 78);

            lblTimKiem.Location = new Point(10, 14);
            txtTimKiem.Location = new Point(135, 10);
            txtTimKiem.Size = new Size(240, 24);
            btnTimKiem.Location = new Point(385, 8);
            btnTimKiem.Size = new Size(95, 28);
            btnDatLaiTimKiem.Location = new Point(488, 8);
            btnDatLaiTimKiem.Size = new Size(88, 28);

            lblLocHangThe = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(10, 47),
                Text = "Hạng thẻ:"
            };
            cboLocHangThe.Location = new Point(82, 44);
            cboLocHangThe.Size = new Size(130, 23);
            lblLocVaiTro = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(230, 47),
                Text = "Vai trò:"
            };
            cboLocVaiTro = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(285, 44),
                Size = new Size(130, 23)
            };
            cboLocVaiTro.Items.AddRange(new object[] { "Tất cả", "Member", "Staff", "Admin" });
            pnlSearch.Controls.AddRange(new Control[]
            {
                lblTimKiem, txtTimKiem, btnTimKiem, btnDatLaiTimKiem,
                lblLocHangThe, cboLocHangThe, lblLocVaiTro, cboLocVaiTro
            });
            pnlSearch.ResumeLayout(true);

            tabMain = new TabControl { Dock = DockStyle.Fill, Name = "tabMain" };
            tabDanhSach = new TabPage("📋 Danh Sách Hội Viên") { Name = "tabDanhSach" };
            tabLichSuVi = new TabPage("📜 Lịch Sử Giao Dịch Ví") { Name = "tabLichSuVi" };
            tabTopHoiVien = new TabPage("🏆 Top 5 Hội Viên") { Name = "tabTopHoiVien" };

            tabDanhSach.Controls.Add(dgvHoiVien);
            tabDanhSach.Controls.Add(pnlSearch);

            dgvLichSuVi = CreateReadOnlyGrid("dgvLichSuVi");
            pnlWalletHeader = new Panel { Dock = DockStyle.Top, Height = 44, Name = "pnlWalletHeader" };
            lblLichSuUser = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(10, 14),
                Text = "Lịch sử giao dịch ví: Chưa chọn hội viên",
                Name = "lblLichSuUser"
            };
            btnTaiLaiLichSu = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(615, 8),
                Size = new Size(135, 28),
                Text = "🔄 Tải lại lịch sử",
                Name = "btnTaiLaiLichSu",
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            pnlWalletHeader.Controls.Add(lblLichSuUser);
            pnlWalletHeader.Controls.Add(btnTaiLaiLichSu);
            tabLichSuVi.Controls.Add(dgvLichSuVi);
            tabLichSuVi.Controls.Add(pnlWalletHeader);

            lblTopSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Padding(10, 12, 0, 0),
                Text = "Bảng xếp hạng Top 5 hội viên (LINQ)",
                Name = "lblTopSummary"
            };
            splTopHoiVien = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Size = new Size(760, 600),
                SplitterDistance = 280,
                Name = "splTopHoiVien"
            };
            grpTopSoDu = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "💰 Top 5 Hội Viên Có Số Dư Ví Cao Nhất",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            grpTopNapTien = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "🔥 Top 5 Hội Viên Nạp Tiền Nhiều Nhất",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            dgvTopSoDu = CreateReadOnlyGrid("dgvTopSoDu");
            dgvTopNapTien = CreateReadOnlyGrid("dgvTopNapTien");
            grpTopSoDu.Controls.Add(dgvTopSoDu);
            grpTopNapTien.Controls.Add(dgvTopNapTien);
            splTopHoiVien.Panel1.Controls.Add(grpTopSoDu);
            splTopHoiVien.Panel2.Controls.Add(grpTopNapTien);
            tabTopHoiVien.Controls.Add(splTopHoiVien);
            tabTopHoiVien.Controls.Add(lblTopSummary);

            tabMain.TabPages.AddRange(new[] { tabDanhSach, tabLichSuVi, tabTopHoiVien });
            pnlMain.Controls.Clear();
            pnlMain.Controls.Add(tabMain);
            btnXoaHoiVien.Text = "🔒 Khóa / Mở khóa";
        }

        private static DataGridView CreateReadOnlyGrid(string name)
        {
            return new DataGridView
            {
                Name = name,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.WhiteSmoke
            };
        }

        private void UpdateLockButtonState(AppUser? user)
        {
            if (user == null)
            {
                btnXoaHoiVien.Text = "🔒 Khóa / Mở khóa";
                btnXoaHoiVien.BackColor = Color.FromArgb(178, 34, 34);
                return;
            }

            if (user.IsActive)
            {
                btnXoaHoiVien.Text = "🔒 Khóa Tài Khoản";
                btnXoaHoiVien.BackColor = Color.FromArgb(178, 34, 34);
            }
            else
            {
                btnXoaHoiVien.Text = "🔓 Mở Khóa TK";
                btnXoaHoiVien.BackColor = Color.FromArgb(46, 125, 50);
            }
        }

        private void BindTopBalanceGrid()
        {
            if (dgvTopSoDu == null || dgvTopSoDu.IsDisposed)
                return;

            var top = DBUser.GetTopMembersByBalance(_userList, 5);
            dgvTopSoDu.DataSource = top.Select((u, index) => new
            {
                Hang = $"#{index + 1}",
                TenDangNhap = u.Username,
                HoVaTen = u.FullName,
                HangThe = u.MemberTier,
                SoDuVi = $"{u.Balance:N0} VNĐ",
                TrangThai = u.IsActive ? "Hoạt động" : "Đã khóa"
            }).ToList();
        }

        private async Task LoadTopMembersAsync()
        {
            if (IsDisposed || Disposing)
                return;

            BindTopBalanceGrid();
            var topTopUp = await _dbUser.GetTopMembersByTopUpAsync(_userList, 5);
            if (IsDisposed || Disposing)
                return;

            dgvTopNapTien.DataSource = topTopUp.Select((u, index) => new
            {
                Hang = $"#{index + 1}",
                TenDangNhap = u.Username,
                HoVaTen = u.FullName,
                SoLanNap = u.TopUpCount,
                TongTienNap = $"{u.TotalTopUpAmount:N0} VNĐ",
                HangThe = u.MemberTier
            }).ToList();

            string topBalanceName = DBUser.GetTopMembersByBalance(_userList, 1)
                .FirstOrDefault()?.Username ?? "Chưa có";
            string topTopUpName = topTopUp.FirstOrDefault()?.Username ?? "Chưa có";
            lblTopSummary.Text = $"Top số dư: {topBalanceName}  |  Top nạp tiền: {topTopUpName}";
        }

        private async Task LoadWalletTransactionsForSelectedUserAsync()
        {
            if (_selectedUser == null)
            {
                if (!IsDisposed && !Disposing)
                {
                    dgvLichSuVi.DataSource = null;
                    lblLichSuUser.Text = "Lịch sử giao dịch ví: Chưa chọn hội viên";
                }
                return;
            }

            int requestedUserId = _selectedUser.UserId;
            string requestedUsername = _selectedUser.Username;
            try
            {
                var transactions = await _dbUser.GetWalletTransactionsByUserIdAsync(requestedUserId);
                if (IsDisposed || Disposing || _selectedUser == null || _selectedUser.UserId != requestedUserId)
                    return;

                lblLichSuUser.Text = $"Lịch sử giao dịch ví: {requestedUsername}";
                dgvLichSuVi.DataSource = transactions.Select(t =>
                {
                    string type = t.TransactionType switch
                    {
                        "TopUp" => "Nạp tiền",
                        "SessionPayment" => "Thanh toán giờ chơi",
                        "Refund" => "Hoàn tiền",
                        "Adjustment" => "Điều chỉnh",
                        _ => t.TransactionType
                    };
                    bool positive = t.TransactionType == "TopUp" || t.TransactionType == "Refund";
                    string amount = $"{(positive ? "+" : "-")}{Math.Abs(t.Amount):N0} VNĐ";
                    return new
                    {
                        MaGD = t.TransactionId,
                        LoaiGiaoDich = type,
                        SoTien = amount,
                        SoDuTruoc = $"{t.BalanceBefore:N0} VNĐ",
                        SoDuSau = $"{t.BalanceAfter:N0} VNĐ",
                        HinhThuc = t.PaymentMethod ?? "--",
                        MoTa = t.Description ?? string.Empty,
                        ThoiGian = t.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show($"Không thể tải lịch sử giao dịch ví: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            BindTopBalanceGrid();
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
                    VaiTro = u.Role,
                    SoDuVi = $"{u.Balance:N0} VNĐ",
                    HangThe = u.MemberTier,
                    NgayTao = u.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    TrangThai = u.IsActive ? "Hoạt động" : "Đã khóa"
                }).ToList();

                dgvHoiVien.DataSource = displayData;

                if (dgvHoiVien.Columns["MaHoiVien"] is DataGridViewColumn col1) col1.HeaderText = "Mã";
                if (dgvHoiVien.Columns["TenDangNhap"] is DataGridViewColumn col2) col2.HeaderText = "Tên Đăng Nhập";
                if (dgvHoiVien.Columns["HoVaTen"] is DataGridViewColumn col3) col3.HeaderText = "Họ và Tên";
                if (dgvHoiVien.Columns["VaiTro"] is DataGridViewColumn col4) col4.HeaderText = "Vai Trò";
                if (dgvHoiVien.Columns["SoDuVi"] is DataGridViewColumn col5) col5.HeaderText = "Số Dư Ví";
                if (dgvHoiVien.Columns["HangThe"] is DataGridViewColumn col6) col6.HeaderText = "Hạng Thẻ";
                if (dgvHoiVien.Columns["NgayTao"] is DataGridViewColumn col7) col7.HeaderText = "Ngày Đăng Ký";
                if (dgvHoiVien.Columns["TrangThai"] is DataGridViewColumn col8) col8.HeaderText = "Trạng Thái";

                foreach (DataGridViewRow row in dgvHoiVien.Rows)
                {
                    if (string.Equals(row.Cells["TrangThai"].Value?.ToString(), "Đã khóa", StringComparison.Ordinal))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }

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
            string? roleFilter = cboLocVaiTro.SelectedItem?.ToString();
            string? tierFilter = cboLocHangThe.SelectedItem?.ToString();
            var filtered = DBUser.SearchUsers(_userList, keyword, roleFilter: roleFilter, tierFilter: tierFilter);
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
            cboRole.SelectedItem = "Member";
            cboMemberTier.SelectedItem = "Standard";
            SetBalanceEditable(true, DBUser.MinimumMemberInitialBalance.ToString("F0", CultureInfo.InvariantCulture));
            txtDoiMatKhauUser.Clear();
            lblDangChon.Text = "Tài khoản nhận: Chưa chọn tài khoản";
            UpdateLockButtonState(null);
            dgvLichSuVi.DataSource = null;
            lblLichSuUser.Text = "Lịch sử giao dịch ví: Chưa chọn hội viên";
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
                cboMemberTier.SelectedItem = _selectedUser.MemberTier;
                cboRole.SelectedItem = _selectedUser.Role;
                SetBalanceEditable(false, $"{_selectedUser.Balance:N0} VNĐ");

                txtDoiMatKhauUser.Text = _selectedUser.Username;
                lblDangChon.Text = $"Tài khoản nhận: {_selectedUser.Username} ({_selectedUser.FullName})";
                UpdateLockButtonState(_selectedUser);
                lblLichSuUser.Text = $"Lịch sử giao dịch ví: {_selectedUser.Username} ({_selectedUser.FullName})";
                if (IsHandleCreated && tabMain.SelectedTab == tabLichSuVi)
                    _ = LoadWalletTransactionsForSelectedUserAsync();
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

            if (!DBUser.ValidateMemberInput(txtUsername.Text, txtFullName.Text, txtPassword.Text, initialBalance, role, out string validationError))
            {
                MessageBox.Show(validationError, "Không thể tạo tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                Role = role,
                MemberTier = requestedTier,
                Balance = initialBalance
            };

            try
            {
                var (success, errorMessage) = await _dbUser.CreateMemberAsync(newUser, txtPassword.Text);
                if (!success)
                {
                    MessageBox.Show(errorMessage, "Không thể tạo tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (_selectedUser == null || !int.TryParse(txtUserId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn hội viên cần sửa từ bảng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show(
                    "Nút '✏️ Cập Nhật' không dùng để đổi mật khẩu. Vui lòng xóa ô Mật khẩu hoặc dùng khung 'Đổi Mật Khẩu Hội Viên' bên dưới.",
                    "Lưu ý bảo mật",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtPassword.Clear();
                return;
            }

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
                bool updated = await _dbUser.UpdateAsync(user);
                if (updated)
                {
                    MessageBox.Show($"Cập nhật hội viên {user.Username} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnMemberChanged?.Invoke($"Đã cập nhật tài khoản: {user.Username}");
                    await LoadDataAsync(id);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tài khoản cần cập nhật (có thể đã bị xóa).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private async Task BtnKhoaMoKhoa_ClickAsync()
        {
            if (_selectedUser == null || !int.TryParse(txtUserId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần khóa hoặc mở khóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedUser.IsActive && CurrentUserSession.IsLoggedIn && _selectedUser.UserId == CurrentUserSession.UserId)
            {
                MessageBox.Show("Không thể tự khóa tài khoản đang đăng nhập!", "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedUser.IsActive &&
                string.Equals(_selectedUser.Username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể khóa tài khoản quản trị mặc định (admin)!", "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CurrentUserSession.IsLoggedIn && CurrentUserSession.IsStaff &&
                string.Equals(_selectedUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Nhân viên (Staff) không có quyền khóa/mở khóa tài khoản Quản trị viên (Admin)!", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int targetUserId = _selectedUser.UserId;
            string targetUsername = _selectedUser.Username;
            bool nextActiveState = !_selectedUser.IsActive;
            string action = nextActiveState ? "mở khóa" : "khóa";
            string detail = nextActiveState
                ? "Tài khoản sẽ được phép đăng nhập và sử dụng máy trở lại."
                : "Tài khoản sẽ không thể đăng nhập hoặc mở máy cho đến khi được mở khóa.";
            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn {action} tài khoản '{targetUsername}'?\n{detail}",
                "Xác nhận khóa / mở khóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                bool ok = await _dbUser.SetUserActiveStatusAsync(targetUserId, nextActiveState);
                if (!ok)
                {
                    MessageBox.Show("Không thể cập nhật trạng thái tài khoản. Vui lòng kiểm tra cơ sở dữ liệu đã chạy Database.sql và có cột IsActive.", "Cập nhật thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string resultMessage = nextActiveState ? "đã được mở khóa" : "đã bị khóa";
                MessageBox.Show($"Tài khoản '{targetUsername}' {resultMessage} thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnMemberChanged?.Invoke($"Tài khoản {targetUsername} {resultMessage}");
                await LoadDataAsync(targetUserId);
                await LoadTopMembersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật trạng thái tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BtnNapTien_ClickAsync()
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản hội viên từ danh sách để nạp tiền!", "Chưa chọn tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_selectedUser.IsActive)
            {
                MessageBox.Show("Tài khoản này đang bị khóa. Vui lòng mở khóa tài khoản trước khi nạp tiền!", "Tài khoản bị khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    await LoadTopMembersAsync();
                    if (tabMain.SelectedTab == tabLichSuVi)
                        await LoadWalletTransactionsForSelectedUserAsync();
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
