using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormOrderServiceDialog : Form
    {
        private readonly DBBilling _dbBilling;
        private readonly int _sessionId;
        private readonly string _stationName;
        private List<ServiceItem> _availableItems = new();
        private ServiceItem? _selectedItem = null;

        public ServiceItem? SelectedItem => _selectedItem;
        public int SelectedQuantity => (int)nudQuantity.Value;
        public decimal CalculatedTotal => (_selectedItem?.UnitPrice ?? 0m) * nudQuantity.Value;

        public FormOrderServiceDialog(DBBilling dbBilling, int sessionId, string stationName)
        {
            ArgumentNullException.ThrowIfNull(dbBilling);
            if (sessionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(sessionId), "Mã phiên chơi không hợp lệ (phải lớn hơn 0).");
            _dbBilling = dbBilling;
            _sessionId = sessionId;
            _stationName = string.IsNullOrWhiteSpace(stationName) ? "Máy" : stationName;

            InitializeComponent();
            SetupForm();
            LoadFoodItems();
        }

        public FormOrderServiceDialog(DBBilling dbBilling, GamingSession session, ComputerStation station)
            : this(dbBilling, session?.SessionId ?? throw new ArgumentNullException(nameof(session)), station?.StationName ?? "Máy")
        {
        }

        private void SetupForm()
        {
            lblTitle.Text = $"🛒 GỌI MÓN F&B - MÁY {_stationName.ToUpperInvariant()}";
            lblSubtitle.Text = $"Phiên chơi #{_sessionId} | Giám sát & Quản trị trực tiếp trên Dashboard";

            cboFood.SelectedIndexChanged += CboFood_SelectedIndexChanged;
            nudQuantity.ValueChanged += (s, e) => UpdateCalculations();
            btnConfirm.Click += BtnConfirm_Click;
        }

        public void LoadFoodItems()
        {
            try
            {
                var all = _dbBilling.GetAllServiceItems(activeOnly: true);
                _availableItems = all.Where(x => x.StockQuantity > 0).ToList();

                cboFood.Items.Clear();
                if (_availableItems.Count == 0)
                {
                    cboFood.Items.Add("⚠️ Hiện không còn món nào trong kho!");
                    cboFood.SelectedIndex = 0;
                    cboFood.Enabled = false;
                    nudQuantity.Enabled = false;
                    txtNote.Enabled = false;
                    btnConfirm.Enabled = false;
                    return;
                }

                cboFood.Enabled = true;
                nudQuantity.Enabled = true;
                txtNote.Enabled = true;
                btnConfirm.Enabled = true;

                foreach (var item in _availableItems)
                {
                    cboFood.Items.Add($"{item.ServiceName} ({item.Category}) - {item.UnitPrice:N0}đ [Còn: {item.StockQuantity}]");
                }

                cboFood.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục dịch vụ: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboFood_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int idx = cboFood.SelectedIndex;
            if (idx >= 0 && idx < _availableItems.Count)
            {
                _selectedItem = _availableItems[idx];
                nudQuantity.Maximum = Math.Max(1, _selectedItem.StockQuantity);
                nudQuantity.Value = 1;
                lblItemCategory.Text = $"Danh mục: {_selectedItem.Category} | Tồn kho: {_selectedItem.StockQuantity}";
                lblUnitPrice.Text = $"Đơn giá: {_selectedItem.UnitPrice:N0} VNĐ";
                UpdateCalculations();
            }
            else
            {
                _selectedItem = null;
                lblItemCategory.Text = "Danh mục: -- | Tồn kho: --";
                lblUnitPrice.Text = "Đơn giá: 0 VNĐ";
                lblTotalAmount.Text = "0 VNĐ";
            }
        }

        private void UpdateCalculations()
        {
            if (_selectedItem != null)
            {
                decimal total = _selectedItem.UnitPrice * nudQuantity.Value;
                lblTotalAmount.Text = $"{total:N0} VNĐ";
            }
            else
            {
                lblTotalAmount.Text = "0 VNĐ";
            }
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn món F&B cần gọi!", "Chưa chọn món", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int qty = (int)nudQuantity.Value;
            if (qty <= 0)
            {
                MessageBox.Show("Số lượng gọi phải lớn hơn 0!", "Số lượng không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (qty > _selectedItem.StockQuantity)
            {
                MessageBox.Show($"Món '{_selectedItem.ServiceName}' chỉ còn {_selectedItem.StockQuantity} phần trong kho!", "Không đủ tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? note = string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text.Trim();

            try
            {
                bool success = _dbBilling.OrderServiceForSession(_sessionId, _selectedItem.ServiceId, qty, note);
                if (success)
                {
                    decimal total = _selectedItem.UnitPrice * qty;
                    MessageBox.Show(
                        $"Đã gọi thành công {qty}x {_selectedItem.ServiceName} cho máy {_stationName}!\n" +
                        $"Tổng tiền dịch vụ: {total:N0} VNĐ\n" +
                        (string.IsNullOrEmpty(note) ? string.Empty : $"Ghi chú: {note}\n") +
                        "Dữ liệu phiên chơi đã được cập nhật trực tiếp trên màn hình!",
                        "Gọi món thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Không thể gọi món vào phiên chơi này.", "Gọi món thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gọi món: {ex.Message}", "Lỗi gọi món", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
