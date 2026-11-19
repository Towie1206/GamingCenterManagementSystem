using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    /// <summary>Xem trước và lưu mẫu hóa đơn nhiệt POS 80 mm của quầy dịch vụ.</summary>
    public class FormPosThermalReceiptDialog : Form
    {
        private readonly TextBox _txtReceipt;
        private readonly string _defaultFileName;

        public FormPosThermalReceiptDialog(
            string invoiceNumber, string customerOrStation, string cashierName,
            List<ReceiptItem> items, decimal vatPercent = 8m)
        {
            string receipt = ThermalReceiptFormatter.FormatReceipt(
                "CYBER GAMING LOUNGE", invoiceNumber, customerOrStation, cashierName, items, vatPercent);
            string fileNumber = string.Join("_", (invoiceNumber ?? string.Empty).Split(Path.GetInvalidFileNameChars()));
            _defaultFileName = $"HoaDon_{fileNumber}.txt";

            Text = $"Hóa đơn nhiệt POS 80 mm - {invoiceNumber}";
            ClientSize = new Size(460, 540);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;

            var title = new Label
            {
                Text = "MẪU IN HÓA ĐƠN NHIỆT POS 80 MM",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _txtReceipt = new TextBox
            {
                Name = "txtReceipt",
                AccessibleName = "Nội dung hóa đơn",
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                WordWrap = false,
                ScrollBars = ScrollBars.Both,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Consolas", 9.5f),
                Text = "MẪU HÓA ĐƠN F&B (TẠM TÍNH)" + Environment.NewLine + receipt
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8)
            };
            var close = new Button
            {
                Text = "Đóng",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(51, 65, 85),
                DialogResult = DialogResult.Cancel
            };
            var save = new Button
            {
                Text = "Lưu hóa đơn (.txt)",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            save.Click += SaveReceipt;
            close.Click += (_, _) => Close();
            CancelButton = close;
            actions.Controls.Add(close);
            actions.Controls.Add(save);
            Controls.Add(_txtReceipt);
            Controls.Add(title);
            Controls.Add(actions);
        }

        private void SaveReceipt(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Lưu hóa đơn nhiệt",
                Filter = "Tệp văn bản (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = _defaultFileName
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                File.WriteAllText(dialog.FileName, _txtReceipt.Text, Encoding.UTF8);
                MessageBox.Show(this, "Đã lưu hóa đơn.", "Hóa đơn POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"Không thể lưu hóa đơn. {ex.Message}", "Hóa đơn POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
