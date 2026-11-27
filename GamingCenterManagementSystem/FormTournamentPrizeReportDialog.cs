using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Hộp thoại hiển thị và xuất biên bản cơ cấu giải thưởng phục vụ lễ trao giải giải đấu Cyber.
    /// </summary>
    public class FormTournamentPrizeReportDialog : Form
    {
        private readonly DBTournament _dbTournament;
        private readonly int _tournamentId;
        private readonly decimal _prizePool;
        private readonly TextBox _txtReport = new();
        private readonly Button _btnSaveFile = new();
        private readonly Button _btnClose = new();

        public FormTournamentPrizeReportDialog(DBTournament dbTournament, int tournamentId, decimal prizePool)
        {
            _dbTournament = dbTournament ?? throw new ArgumentNullException(nameof(dbTournament));
            _tournamentId = tournamentId;
            _prizePool = prizePool;

            Text = $"🏆 Biên Bản Trao Giải Đấu Cyber - Giải đấu #{tournamentId}";
            Size = new Size(600, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;

            InitializeLayout();
            LoadReport();
        }

        private void InitializeLayout()
        {
            var lblTitle = new Label
            {
                Text = "BIÊN BẢN CƠ CẤU GIẢI THƯỞNG GIẢI ĐẤU CYBER GAME",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 204, 21),
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _txtReport.Dock = DockStyle.Fill;
            _txtReport.Multiline = true;
            _txtReport.ReadOnly = true;
            _txtReport.ScrollBars = ScrollBars.Vertical;
            _txtReport.BackColor = Color.FromArgb(30, 41, 59);
            _txtReport.ForeColor = Color.FromArgb(241, 245, 249);
            _txtReport.Font = new Font("Consolas", 10f);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(16, 10, 16, 10) };
            _btnSaveFile.Text = "💾 Lưu Tệp Văn Bản (.txt)";
            _btnSaveFile.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnSaveFile.BackColor = Color.FromArgb(16, 185, 129);
            _btnSaveFile.ForeColor = Color.White;
            _btnSaveFile.FlatStyle = FlatStyle.Flat;
            _btnSaveFile.Size = new Size(180, 36);
            _btnSaveFile.Location = new Point(16, 10);
            _btnSaveFile.Click += BtnSaveFile_Click;

            _btnClose.Text = "Đóng";
            _btnClose.Font = new Font("Segoe UI", 9.5f);
            _btnClose.BackColor = Color.FromArgb(51, 65, 85);
            _btnClose.ForeColor = Color.White;
            _btnClose.FlatStyle = FlatStyle.Flat;
            _btnClose.Size = new Size(90, 36);
            _btnClose.Location = new Point(480, 10);
            _btnClose.Click += (s, e) => Close();

            pnlBottom.Controls.Add(_btnSaveFile);
            pnlBottom.Controls.Add(_btnClose);

            Controls.Add(_txtReport);
            Controls.Add(lblTitle);
            Controls.Add(pnlBottom);
        }

        private void LoadReport()
        {
            try
            {
                _txtReport.Text = _dbTournament.GeneratePrizeReport(_tournamentId, _prizePool);
            }
            catch (Exception ex)
            {
                _txtReport.Text = $"Lỗi tạo báo cáo giải thưởng: {ex.Message}";
            }
        }

        private void BtnSaveFile_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                FileName = $"BienBanTraoGiai_Tournament_{_tournamentId}.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, _txtReport.Text);
                MessageBox.Show("Lưu biên bản giải đấu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
