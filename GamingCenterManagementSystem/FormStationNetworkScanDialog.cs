using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Hộp thoại hiển thị kết quả kiểm tra độ trễ (Ping) và sức khỏe mạng toàn bộ máy trạm.
    /// </summary>
    public class FormStationNetworkScanDialog : Form
    {
        private readonly DBStation _dbStation;
        private readonly DataGridView _dgvHealth = new();
        private readonly Label _lblSummary = new();
        private readonly Button _btnScanAgain = new();
        private readonly Button _btnClose = new();

        public FormStationNetworkScanDialog(DBStation dbStation)
        {
            _dbStation = dbStation ?? throw new ArgumentNullException(nameof(dbStation));

            Text = "📡 Giám Sát Kết Nối Mạng & Độ Trễ Máy Trạm (Network Ping Monitor)";
            Size = new Size(720, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;

            InitializeLayout();
            RunScan();
        }

        private void InitializeLayout()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(16, 12, 16, 8) };
            var lblTitle = new Label
            {
                Text = "BẢNG ĐÁNH GIÁ CHẤT LƯỢNG MẠNG VÀ ĐỘ TRỄ MÁY TRẠM CYBER",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Dock = DockStyle.Top,
                Height = 26
            };
            _lblSummary.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            _lblSummary.ForeColor = Color.FromArgb(148, 163, 184);
            _lblSummary.Dock = DockStyle.Bottom;
            _lblSummary.Height = 20;

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(_lblSummary);

            _dgvHealth.Dock = DockStyle.Fill;
            _dgvHealth.BackgroundColor = Color.FromArgb(30, 41, 59);
            _dgvHealth.ForeColor = Color.White;
            _dgvHealth.GridColor = Color.FromArgb(51, 65, 85);
            _dgvHealth.RowHeadersVisible = false;
            _dgvHealth.AllowUserToAddRows = false;
            _dgvHealth.ReadOnly = true;
            _dgvHealth.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvHealth.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            _dgvHealth.DefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248);
            _dgvHealth.DefaultCellStyle.SelectionForeColor = Color.Black;
            _dgvHealth.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            _dgvHealth.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            _dgvHealth.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            _dgvHealth.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgvHealth.EnableHeadersVisualStyles = false;

            _dgvHealth.Columns.Add("StationName", "Tên Máy");
            _dgvHealth.Columns.Add("IpAddress", "Địa Chỉ IP");
            _dgvHealth.Columns.Add("LatencyMs", "Độ Trễ (Ping)");
            _dgvHealth.Columns.Add("PacketLoss", "Rớt Gói (%)");
            _dgvHealth.Columns.Add("StatusBadge", "Tình Trạng Mạng");

            _dgvHealth.Columns["StationName"]!.Width = 100;
            _dgvHealth.Columns["IpAddress"]!.Width = 130;
            _dgvHealth.Columns["LatencyMs"]!.Width = 110;
            _dgvHealth.Columns["PacketLoss"]!.Width = 110;
            _dgvHealth.Columns["StatusBadge"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(16, 10, 16, 10) };
            _btnScanAgain.Text = "🔄 Quét Lại";
            _btnScanAgain.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnScanAgain.BackColor = Color.FromArgb(37, 99, 235);
            _btnScanAgain.ForeColor = Color.White;
            _btnScanAgain.FlatStyle = FlatStyle.Flat;
            _btnScanAgain.Size = new Size(110, 36);
            _btnScanAgain.Location = new Point(16, 10);
            _btnScanAgain.Click += (s, e) => RunScan();

            _btnClose.Text = "Đóng";
            _btnClose.Font = new Font("Segoe UI", 9.5f);
            _btnClose.BackColor = Color.FromArgb(51, 65, 85);
            _btnClose.ForeColor = Color.White;
            _btnClose.FlatStyle = FlatStyle.Flat;
            _btnClose.Size = new Size(90, 36);
            _btnClose.Location = new Point(600, 10);
            _btnClose.Click += (s, e) => Close();

            pnlBottom.Controls.Add(_btnScanAgain);
            pnlBottom.Controls.Add(_btnClose);

            Controls.Add(_dgvHealth);
            Controls.Add(pnlTop);
            Controls.Add(pnlBottom);
        }

        public void RunScan()
        {
            _dgvHealth.Rows.Clear();
            var results = _dbStation.CheckAllStationsNetworkHealth();

            int goodCount = 0;
            foreach (var r in results)
            {
                if (r.Status == NetworkHealthStatus.Good) goodCount++;
                string latencyText = r.LatencyMs >= 0 ? $"{r.LatencyMs} ms" : "Timeout";
                _dgvHealth.Rows.Add(r.StationName, r.IpAddress, latencyText, $"{r.PacketLossPercent:0.#}%", r.GetStatusBadge());
            }

            _lblSummary.Text = $"Quét thành công {results.Count} máy trạm | {goodCount}/{results.Count} máy kết nối tối ưu cho Game FPS (<30ms).";
        }
    }
}