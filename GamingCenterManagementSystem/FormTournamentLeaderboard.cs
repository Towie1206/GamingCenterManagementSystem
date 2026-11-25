using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Bản ghi thành tích tuyển thủ xuất sắc nhất (MVP) qua các giải đấu tại phòng máy Master Game.
    /// </summary>
    public sealed class TournamentMvpEntry
    {
        public int EntryId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string TournamentTitle { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public string Venue { get; set; } = TournamentLeaderboardService.DefaultVenue;
        public int MvpScore { get; set; }
        public decimal PrizeAmount { get; set; }
        public string AchievementNote { get; set; } = string.Empty;
        public DateTime AwardedDate { get; set; } = DateTime.Today;

        public TournamentMvpEntry Clone() => new()
        {
            EntryId = EntryId,
            PlayerName = PlayerName,
            TeamName = TeamName,
            TournamentTitle = TournamentTitle,
            GameTitle = GameTitle,
            Venue = Venue,
            MvpScore = MvpScore,
            PrizeAmount = PrizeAmount,
            AchievementNote = AchievementNote,
            AwardedDate = AwardedDate
        };
    }

    /// <summary>
    /// Một dòng xếp hạng trên Bảng Vinh Danh Top MVP.
    /// </summary>
    public sealed class TournamentLeaderboardRankEntry
    {
        public int Rank { get; init; }
        public string MedalLabel { get; init; } = string.Empty;
        public TournamentMvpEntry Entry { get; init; } = new();
    }

    /// <summary>
    /// Dịch vụ quản lý, xếp hạng và xuất/in Bảng Vinh Danh Top MVP & Thành Tích Giải Đấu Master Game.
    /// </summary>
    public class TournamentLeaderboardService
    {
        public const string DefaultVenue = "Master Game - 165 Hoàng Công Chất, Phường Phú Diễn, Quận Bắc Từ Liêm, Hà Nội";

        private readonly List<TournamentMvpEntry> _entries = new();
        private int _nextEntryId = 1;

        public TournamentLeaderboardService(bool loadSampleData = true)
        {
            if (loadSampleData)
            {
                SeedDefaultMasterGameMvps();
            }
        }

        private void SeedDefaultMasterGameMvps()
        {
            var seed = new[]
            {
                new TournamentMvpEntry
                {
                    PlayerName = "Nguyễn Minh Quân (QuanV)",
                    TeamName = "Master Dragon",
                    TournamentTitle = "Master Game Valorant Community Cup Mùa 1",
                    GameTitle = "Valorant",
                    MvpScore = 985,
                    PrizeAmount = 5000000m,
                    AchievementNote = "MVP Chung kết — ACS 312 | KDA 26/7/8",
                    AwardedDate = new DateTime(2026, 10, 18)
                },
                new TournamentMvpEntry
                {
                    PlayerName = "Trần Đức Anh (DuckA)",
                    TeamName = "Hoàng Công Chất Gaming",
                    TournamentTitle = "Master Game LoL Championship 2026",
                    GameTitle = "League of Legends",
                    MvpScore = 940,
                    PrizeAmount = 3000000m,
                    AchievementNote = "MVP Vòng Playoffs — 2 Pentakill tại dàn Maximum",
                    AwardedDate = new DateTime(2026, 10, 25)
                },
                new TournamentMvpEntry
                {
                    PlayerName = "Lê Quang Huy (HuyCS)",
                    TeamName = "Phú Diễn Esports",
                    TournamentTitle = "Master Game CS2 Arena Open",
                    GameTitle = "CS2",
                    MvpScore = 910,
                    PrizeAmount = 2000000m,
                    AchievementNote = "MVP Giải đấu — Rating 1.48 | 4 Clutch 1v3",
                    AwardedDate = new DateTime(2026, 11, 2)
                },
                new TournamentMvpEntry
                {
                    PlayerName = "Phạm Tuấn Kiệt (KietFC)",
                    TeamName = "Bắc Từ Liêm FC",
                    TournamentTitle = "Master Game FC Online Weekend Cup",
                    GameTitle = "FC Online",
                    MvpScore = 865,
                    PrizeAmount = 1500000m,
                    AchievementNote = "Bất bại 6 trận liên tiếp tại dàn Master",
                    AwardedDate = new DateTime(2026, 11, 10)
                },
                new TournamentMvpEntry
                {
                    PlayerName = "Vũ Hoàng Long (LongSniper)",
                    TeamName = "Phoenix Cyber",
                    TournamentTitle = "Master Game Valorant Community Cup Mùa 1",
                    GameTitle = "Valorant",
                    MvpScore = 840,
                    PrizeAmount = 1000000m,
                    AchievementNote = "Top Frag Tứ kết & Bán kết",
                    AwardedDate = new DateTime(2026, 10, 18)
                }
            };

            foreach (var item in seed)
            {
                AddEntry(item);
            }
        }

        public IReadOnlyList<TournamentMvpEntry> GetAllEntries() =>
            _entries.Select(e => e.Clone()).ToList();

        public TournamentMvpEntry AddEntry(TournamentMvpEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            ValidateEntry(entry);

            var stored = new TournamentMvpEntry
            {
                EntryId = _nextEntryId++,
                PlayerName = entry.PlayerName.Trim(),
                TeamName = entry.TeamName.Trim(),
                TournamentTitle = entry.TournamentTitle.Trim(),
                GameTitle = entry.GameTitle.Trim(),
                Venue = string.IsNullOrWhiteSpace(entry.Venue) ? DefaultVenue : entry.Venue.Trim(),
                MvpScore = entry.MvpScore,
                PrizeAmount = decimal.Round(entry.PrizeAmount, 0),
                AchievementNote = entry.AchievementNote?.Trim() ?? string.Empty,
                AwardedDate = entry.AwardedDate == default ? DateTime.Today : entry.AwardedDate.Date
            };

            _entries.Add(stored);
            return stored.Clone();
        }

        public bool RemoveEntry(int entryId) =>
            _entries.RemoveAll(e => e.EntryId == entryId) > 0;

        public static void ValidateEntry(TournamentMvpEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.PlayerName))
                throw new ArgumentException("Tên tuyển thủ MVP không được để trống.", nameof(entry));
            if (entry.PlayerName.Trim().Length > 100)
                throw new ArgumentException("Tên tuyển thủ MVP không được vượt quá 100 ký tự.", nameof(entry));

            if (string.IsNullOrWhiteSpace(entry.TeamName))
                throw new ArgumentException("Tên đội tuyển không được để trống.", nameof(entry));
            if (entry.TeamName.Trim().Length > 100)
                throw new ArgumentException("Tên đội tuyển không được vượt quá 100 ký tự.", nameof(entry));

            if (string.IsNullOrWhiteSpace(entry.TournamentTitle))
                throw new ArgumentException("Tên giải đấu không được để trống.", nameof(entry));
            if (entry.TournamentTitle.Trim().Length > 150)
                throw new ArgumentException("Tên giải đấu không được vượt quá 150 ký tự.", nameof(entry));

            if (string.IsNullOrWhiteSpace(entry.GameTitle))
                throw new ArgumentException("Tựa game không được để trống.", nameof(entry));
            if (entry.GameTitle.Trim().Length > 100)
                throw new ArgumentException("Tựa game không được vượt quá 100 ký tự.", nameof(entry));

            if (entry.MvpScore <= 0)
                throw new ArgumentException("Điểm vinh danh MVP phải lớn hơn 0.", nameof(entry));

            if (entry.PrizeAmount < 0)
                throw new ArgumentException("Tiền thưởng không được là số âm.", nameof(entry));
        }

        public IReadOnlyList<string> GetDistinctGames() =>
            _entries.Select(e => e.GameTitle)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
                .ToList();

        public IReadOnlyList<TournamentLeaderboardRankEntry> GetRankedEntries(
            string? gameFilter = null,
            string? searchKeyword = null)
        {
            IEnumerable<TournamentMvpEntry> query = _entries;

            if (!string.IsNullOrWhiteSpace(gameFilter) &&
                !string.Equals(gameFilter.Trim(), "Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                string filter = gameFilter.Trim();
                query = query.Where(e => string.Equals(e.GameTitle, filter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string keyword = searchKeyword.Trim();
                query = query.Where(e =>
                    e.PlayerName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.TeamName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.TournamentTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.AchievementNote.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = query
                .OrderByDescending(e => e.MvpScore)
                .ThenByDescending(e => e.PrizeAmount)
                .ThenByDescending(e => e.AwardedDate)
                .ThenBy(e => e.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var ranked = new List<TournamentLeaderboardRankEntry>(ordered.Count);
            for (int i = 0; i < ordered.Count; i++)
            {
                int rank = i + 1;
                ranked.Add(new TournamentLeaderboardRankEntry
                {
                    Rank = rank,
                    MedalLabel = FormatRankLabel(rank),
                    Entry = ordered[i].Clone()
                });
            }

            return ranked;
        }

        public IReadOnlyList<TournamentLeaderboardRankEntry> GetTopPodium(int topCount = 3, string? gameFilter = null)
        {
            if (topCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(topCount), "Số lượng vinh danh phải lớn hơn 0.");

            return GetRankedEntries(gameFilter).Take(topCount).ToList();
        }

        public static string FormatRankLabel(int rank) => rank switch
        {
            1 => "🥇 Top 1",
            2 => "🥈 Top 2",
            3 => "🥉 Top 3",
            _ => $"#{rank}"
        };

        /// <summary>
        /// Tạo văn bản bảng vinh danh Top 1, Top 2, Top 3 kèm tiền thưởng và điểm vinh danh để in hoặc xuất file .txt.
        /// </summary>
        public string BuildHallOfFameExportText(string? gameFilter = null, int topCount = 3, DateTime? generatedAt = null)
        {
            var topEntries = GetTopPodium(topCount, gameFilter);
            DateTime timestamp = generatedAt ?? DateTime.Now;
            string scope = string.IsNullOrWhiteSpace(gameFilter) || string.Equals(gameFilter, "Tất cả", StringComparison.OrdinalIgnoreCase)
                ? "Tất cả các bộ môn giải đấu"
                : $"Bộ môn: {gameFilter.Trim()}";

            var sb = new StringBuilder();
            sb.AppendLine("========================================================================");
            sb.AppendLine("        BẢNG VINH DANH TOP MVP & THÀNH TÍCH GIẢI ĐẤU - MASTER GAME      ");
            sb.AppendLine("========================================================================");
            sb.AppendLine($"Địa điểm : {DefaultVenue}");
            sb.AppendLine($"Phạm vi  : {scope}");
            sb.AppendLine($"Ngày xuất: {timestamp:dd/MM/yyyy HH:mm}");
            sb.AppendLine("------------------------------------------------------------------------");

            if (topEntries.Count == 0)
            {
                sb.AppendLine("Chưa có dữ liệu tuyển thủ MVP nào trong danh sách vinh danh.");
            }
            else
            {
                foreach (var item in topEntries)
                {
                    var e = item.Entry;
                    sb.AppendLine($"{item.MedalLabel} | Tuyển thủ: {e.PlayerName} — Đội: {e.TeamName}");
                    sb.AppendLine($"   Giải đấu       : {e.TournamentTitle} ({e.GameTitle})");
                    sb.AppendLine($"   Điểm vinh danh : {e.MvpScore:N0} điểm");
                    sb.AppendLine($"   Tiền thưởng    : {e.PrizeAmount:N0} VNĐ");
                    if (!string.IsNullOrWhiteSpace(e.AchievementNote))
                    {
                        sb.AppendLine($"   Thành tích     : {e.AchievementNote}");
                    }
                    sb.AppendLine($"   Ngày vinh danh : {e.AwardedDate:dd/MM/yyyy}");
                    sb.AppendLine("------------------------------------------------------------------------");
                }

                decimal totalPrize = topEntries.Sum(x => x.Entry.PrizeAmount);
                int totalScore = topEntries.Sum(x => x.Entry.MvpScore);
                sb.AppendLine($"Tổng tiền thưởng Top {topEntries.Count}: {totalPrize:N0} VNĐ");
                sb.AppendLine($"Tổng điểm vinh danh Top {topEntries.Count}: {totalScore:N0} điểm");
            }

            sb.AppendLine("========================================================================");
            return sb.ToString();
        }

        /// <summary>
        /// Tạo nội dung CSV chuẩn UTF-8 phục vụ xuất bảng vinh danh Top 1, Top 2, Top 3 sang Excel.
        /// </summary>
        public string BuildCsvExportText(string? gameFilter = null, int topCount = 3)
        {
            var topEntries = GetTopPodium(topCount, gameFilter);
            var sb = new StringBuilder();
            sb.AppendLine("Hang,TuyenThuMVP,DoiTuyen,GiaiDau,TuaGame,DiemVinhDanh,TienThuongVND,ThanhTich,NgayVinhDanh,DiaDiem");

            foreach (var item in topEntries)
            {
                var e = item.Entry;
                sb.AppendLine(string.Join(",",
                    EscapeCsv($"Top {item.Rank}"),
                    EscapeCsv(e.PlayerName),
                    EscapeCsv(e.TeamName),
                    EscapeCsv(e.TournamentTitle),
                    EscapeCsv(e.GameTitle),
                    e.MvpScore.ToString(CultureInfo.InvariantCulture),
                    e.PrizeAmount.ToString("F0", CultureInfo.InvariantCulture),
                    EscapeCsv(e.AchievementNote),
                    e.AwardedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EscapeCsv(e.Venue)));
            }

            return sb.ToString();
        }

        public void ExportLeaderboardToFile(string filePath, string? gameFilter = null, int topCount = 3)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Đường dẫn tệp xuất không được để trống.", nameof(filePath));

            string fullPath = Path.GetFullPath(filePath);
            string? dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string content = string.Equals(Path.GetExtension(fullPath), ".csv", StringComparison.OrdinalIgnoreCase)
                ? BuildCsvExportText(gameFilter, topCount)
                : BuildHallOfFameExportText(gameFilter, topCount);

            File.WriteAllText(fullPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        private static string EscapeCsv(string? value)
        {
            string text = value ?? string.Empty;
            if (text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
            {
                return $"\"{text.Replace("\"", "\"\"")}\"";
            }
            return text;
        }
    }

    /// <summary>
    /// Hộp thoại Bảng Vinh Danh Top MVP & Thành Tích Giải Đấu tại phòng máy Master Game (165 Hoàng Công Chất).
    /// </summary>
    public class FormTournamentLeaderboard : Form
    {
        private readonly TournamentLeaderboardService _service;
        private Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? _promptHandler;

        private readonly Label _lblTop1Player = new();
        private readonly Label _lblTop1Details = new();
        private readonly Label _lblTop2Player = new();
        private readonly Label _lblTop2Details = new();
        private readonly Label _lblTop3Player = new();
        private readonly Label _lblTop3Details = new();

        private readonly TextBox _txtPlayerName = new();
        private readonly TextBox _txtTeamName = new();
        private readonly TextBox _txtTournamentTitle = new();
        private readonly ComboBox _cboGameTitle = new();
        private readonly NumericUpDown _numMvpScore = new();
        private readonly NumericUpDown _numPrizeAmount = new();
        private readonly TextBox _txtAchievementNote = new();
        private readonly DateTimePicker _dtpAwardedDate = new();
        private readonly Button _btnAddMvp = new();
        private readonly Button _btnDeleteMvp = new();

        private readonly ComboBox _cboFilterGame = new();
        private readonly TextBox _txtSearch = new();
        private readonly Label _lblTotalSummary = new();
        private readonly DataGridView _dgvLeaderboard = new();

        private readonly Button _btnExportTxt = new();
        private readonly Button _btnExportCsv = new();
        private readonly Button _btnPrintTop3 = new();
        private readonly Button _btnClose = new();

        private string _pendingPrintContent = string.Empty;
        private int _pendingPrintCharIndex;

        public TournamentLeaderboardService Service => _service;

        public string CurrentGameFilter => _cboFilterGame.SelectedItem?.ToString() ?? "Tất cả";

        public FormTournamentLeaderboard() : this(new TournamentLeaderboardService(loadSampleData: true))
        {
        }

        public FormTournamentLeaderboard(TournamentLeaderboardService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            Text = "🏆 Bảng Vinh Danh Top MVP & Thành Tích Giải Đấu — Master Game";
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(1120, 740);
            MinimumSize = new Size(960, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 250, 252); // Slate-50 Light theme
            ForeColor = Color.FromArgb(15, 23, 42);    // Slate-900
            Font = new Font("Segoe UI", 9.5f);

            InitializeLayout();
            PopulateGameFilters();
            RefreshLeaderboard();
        }

        internal void SetPromptHandler(Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? handler)
        {
            _promptHandler = handler;
        }

        private DialogResult ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (_promptHandler != null)
                return _promptHandler(text, caption, buttons, icon);

            return MessageBox.Show(this, text, caption, buttons, icon);
        }

        private void InitializeLayout()
        {
            // 1. Header Slate Dark + Gold accent
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20, 12, 20, 10)
            };

            var lblTitle = new Label
            {
                Text = "🏆 BẢNG VINH DANH TOP MVP & THÀNH TÍCH GIẢI ĐẤU",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 204, 21),
                AutoSize = true,
                Location = new Point(20, 12)
            };

            var lblSubtitle = new Label
            {
                Text = TournamentLeaderboardService.DefaultVenue,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                AutoSize = true,
                Location = new Point(22, 44)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Top 3 Podium Cards
            var tblPodium = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 128,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(16, 12, 16, 8),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            tblPodium.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tblPodium.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tblPodium.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            tblPodium.Controls.Add(
                CreatePodiumCard("🥇 TOP 1 — QUÁN QUÂN MVP", Color.FromArgb(255, 251, 235), Color.FromArgb(217, 119, 6), _lblTop1Player, _lblTop1Details),
                0, 0);
            tblPodium.Controls.Add(
                CreatePodiumCard("🥈 TOP 2 — Á QUÂN MVP", Color.FromArgb(248, 250, 252), Color.FromArgb(71, 85, 105), _lblTop2Player, _lblTop2Details),
                1, 0);
            tblPodium.Controls.Add(
                CreatePodiumCard("🥉 TOP 3 — HẠNG BA MVP", Color.FromArgb(255, 247, 237), Color.FromArgb(180, 83, 9), _lblTop3Player, _lblTop3Details),
                2, 0);

            // 3. Footer Action Bar
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Size = new Size(1100, 60),
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            ConfigureActionButton(_btnExportTxt, "💾 Xuất Vinh Danh Top 3 (.txt)", Color.FromArgb(16, 185, 129), Color.White, 220);
            _btnExportTxt.Name = "btnExportTxt";
            _btnExportTxt.Location = new Point(16, 12);
            _btnExportTxt.Click += BtnExportTxt_Click;

            ConfigureActionButton(_btnExportCsv, "📊 Xuất Excel (.csv)", Color.FromArgb(14, 116, 144), Color.White, 165);
            _btnExportCsv.Name = "btnExportCsv";
            _btnExportCsv.Location = new Point(248, 12);
            _btnExportCsv.Click += BtnExportCsv_Click;

            ConfigureActionButton(_btnPrintTop3, "🖨️ In Bảng Vinh Danh Top 3", Color.FromArgb(124, 58, 237), Color.White, 210);
            _btnPrintTop3.Name = "btnPrintTop3";
            _btnPrintTop3.Location = new Point(425, 12);
            _btnPrintTop3.Click += BtnPrintTop3_Click;

            ConfigureActionButton(_btnClose, "Đóng", Color.FromArgb(71, 85, 105), Color.White, 95);
            _btnClose.Name = "btnClose";
            _btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnClose.Location = new Point(pnlFooter.Width - 115, 12);
            _btnClose.Click += (_, _) => Close();

            pnlFooter.Controls.Add(_btnExportTxt);
            pnlFooter.Controls.Add(_btnExportCsv);
            pnlFooter.Controls.Add(_btnPrintTop3);
            pnlFooter.Controls.Add(_btnClose);

            // 4. Main Content (Left Input Form + Right Leaderboard Grid)
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = Color.FromArgb(248, 250, 252)
            };

            var grpInput = BuildInputGroup();
            grpInput.Dock = DockStyle.Left;
            grpInput.Width = 340;

            var pnlSpacer = new Panel { Dock = DockStyle.Left, Width = 14 };

            var grpGrid = BuildLeaderboardGroup();
            grpGrid.Dock = DockStyle.Fill;

            pnlBody.Controls.Add(grpGrid);
            pnlBody.Controls.Add(pnlSpacer);
            pnlBody.Controls.Add(grpInput);

            Controls.Add(pnlBody);
            Controls.Add(tblPodium);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFooter);
        }

        private static Panel CreatePodiumCard(string badgeTitle, Color bgColor, Color accentColor, Label lblPlayer, Label lblDetails)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 6, 0),
                BackColor = bgColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblHeader = new Label
            {
                Text = badgeTitle,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Top,
                Height = 22
            };

            lblPlayer.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblPlayer.ForeColor = Color.FromArgb(15, 23, 42);
            lblPlayer.Dock = DockStyle.Top;
            lblPlayer.Height = 26;
            lblPlayer.Text = "-- Chưa có --";

            lblDetails.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblDetails.ForeColor = Color.FromArgb(51, 65, 85);
            lblDetails.Dock = DockStyle.Fill;
            lblDetails.Text = "Điểm vinh danh: 0 | Thưởng: 0 VNĐ";

            card.Controls.Add(lblDetails);
            card.Controls.Add(lblPlayer);
            card.Controls.Add(lblHeader);
            return card;
        }

        private GroupBox BuildInputGroup()
        {
            var grp = new GroupBox
            {
                Text = "Ghi Nhận Tuyển Thủ MVP Mới",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var pnlScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            int y = 8;

            y = AddLabeledControl(pnlScroll, "Tên tuyển thủ (MVP):", _txtPlayerName, "txtPlayerName", y);
            y = AddLabeledControl(pnlScroll, "Đội tuyển:", _txtTeamName, "txtTeamName", y);
            y = AddLabeledControl(pnlScroll, "Tên giải đấu:", _txtTournamentTitle, "txtTournamentTitle", y);

            _cboGameTitle.DropDownStyle = ComboBoxStyle.DropDown;
            _cboGameTitle.Items.AddRange(new object[] { "Valorant", "League of Legends", "CS2", "FC Online", "Dota 2" });
            _cboGameTitle.SelectedIndex = 0;
            y = AddLabeledControl(pnlScroll, "Tựa game:", _cboGameTitle, "cboGameTitle", y);

            _numMvpScore.Minimum = 1;
            _numMvpScore.Maximum = 100000;
            _numMvpScore.Value = 850;
            _numMvpScore.Increment = 10;
            y = AddLabeledControl(pnlScroll, "Điểm vinh danh (MVP):", _numMvpScore, "numMvpScore", y);

            _numPrizeAmount.Minimum = 0;
            _numPrizeAmount.Maximum = 1000000000;
            _numPrizeAmount.Value = 2000000;
            _numPrizeAmount.Increment = 100000;
            _numPrizeAmount.ThousandsSeparator = true;
            y = AddLabeledControl(pnlScroll, "Tiền thưởng (VNĐ):", _numPrizeAmount, "numPrizeAmount", y);

            _dtpAwardedDate.Format = DateTimePickerFormat.Custom;
            _dtpAwardedDate.CustomFormat = "dd/MM/yyyy";
            _dtpAwardedDate.Value = DateTime.Today;
            y = AddLabeledControl(pnlScroll, "Ngày vinh danh:", _dtpAwardedDate, "dtpAwardedDate", y);

            y = AddLabeledControl(pnlScroll, "Thành tích / Ghi chú:", _txtAchievementNote, "txtAchievementNote", y);

            ConfigureActionButton(_btnAddMvp, "➕ Ghi Nhận MVP", Color.FromArgb(37, 99, 235), Color.White, 148);
            _btnAddMvp.Name = "btnAddMvp";
            _btnAddMvp.Location = new Point(4, y + 8);
            _btnAddMvp.Click += BtnAddMvp_Click;

            ConfigureActionButton(_btnDeleteMvp, "🗑️ Xóa Mục Chọn", Color.FromArgb(220, 38, 38), Color.White, 148);
            _btnDeleteMvp.Name = "btnDeleteMvp";
            _btnDeleteMvp.Location = new Point(160, y + 8);
            _btnDeleteMvp.Click += BtnDeleteMvp_Click;

            pnlScroll.Controls.Add(_btnAddMvp);
            pnlScroll.Controls.Add(_btnDeleteMvp);
            grp.Controls.Add(pnlScroll);
            return grp;
        }

        private GroupBox BuildLeaderboardGroup()
        {
            var grp = new GroupBox
            {
                Text = "Danh Sách Vinh Danh Tuyển Thủ Xuất Sắc Nhất",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(0, 4, 0, 4)
            };

            var lblGame = new Label
            {
                Text = "Bộ môn:",
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(4, 11)
            };

            _cboFilterGame.Name = "cboFilterGame";
            _cboFilterGame.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboFilterGame.Font = new Font("Segoe UI", 9f);
            _cboFilterGame.Location = new Point(62, 8);
            _cboFilterGame.Width = 155;
            _cboFilterGame.SelectedIndexChanged += (_, _) => RefreshLeaderboard();

            var lblSearch = new Label
            {
                Text = "Tìm kiếm:",
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(230, 11)
            };

            _txtSearch.Name = "txtSearch";
            _txtSearch.Font = new Font("Segoe UI", 9f);
            _txtSearch.Location = new Point(296, 8);
            _txtSearch.Width = 190;
            _txtSearch.PlaceholderText = "Tuyển thủ, đội, giải...";
            _txtSearch.TextChanged += (_, _) => RefreshLeaderboard();

            _lblTotalSummary.Name = "lblTotalSummary";
            _lblTotalSummary.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblTotalSummary.ForeColor = Color.FromArgb(30, 64, 175);
            _lblTotalSummary.AutoSize = true;
            _lblTotalSummary.Location = new Point(498, 11);

            pnlFilter.Controls.Add(lblGame);
            pnlFilter.Controls.Add(_cboFilterGame);
            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(_txtSearch);
            pnlFilter.Controls.Add(_lblTotalSummary);

            _dgvLeaderboard.Name = "dgvLeaderboard";
            _dgvLeaderboard.Dock = DockStyle.Fill;
            _dgvLeaderboard.ReadOnly = true;
            _dgvLeaderboard.AllowUserToAddRows = false;
            _dgvLeaderboard.AllowUserToDeleteRows = false;
            _dgvLeaderboard.MultiSelect = false;
            _dgvLeaderboard.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvLeaderboard.RowHeadersVisible = false;
            _dgvLeaderboard.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvLeaderboard.BackgroundColor = Color.FromArgb(248, 250, 252);
            _dgvLeaderboard.BorderStyle = BorderStyle.FixedSingle;
            _dgvLeaderboard.RowTemplate.Height = 30;
            _dgvLeaderboard.EnableHeadersVisualStyles = false;
            _dgvLeaderboard.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            _dgvLeaderboard.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvLeaderboard.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _dgvLeaderboard.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            _dgvLeaderboard.CellFormatting += DgvLeaderboard_CellFormatting;

            grp.Controls.Add(_dgvLeaderboard);
            grp.Controls.Add(pnlFilter);
            return grp;
        }

        private static int AddLabeledControl(Panel container, string labelText, Control control, string controlName, int startY)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 8.75f, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(4, startY)
            };

            control.Name = controlName;
            control.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            control.Location = new Point(4, startY + 20);
            control.Width = 304;

            container.Controls.Add(lbl);
            container.Controls.Add(control);
            return startY + 52;
        }

        private static void ConfigureActionButton(Button btn, string text, Color backColor, Color foreColor, int width)
        {
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btn.BackColor = backColor;
            btn.ForeColor = foreColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Size = new Size(width, 36);
            btn.UseVisualStyleBackColor = false;
        }

        private void PopulateGameFilters()
        {
            string? previous = _cboFilterGame.SelectedItem?.ToString();
            _cboFilterGame.Items.Clear();
            _cboFilterGame.Items.Add("Tất cả");
            foreach (string game in _service.GetDistinctGames())
            {
                _cboFilterGame.Items.Add(game);
            }

            if (!string.IsNullOrWhiteSpace(previous) && _cboFilterGame.Items.Contains(previous))
                _cboFilterGame.SelectedItem = previous;
            else
                _cboFilterGame.SelectedIndex = 0;
        }

        public void RefreshLeaderboard()
        {
            string filterGame = CurrentGameFilter;
            string keyword = _txtSearch.Text;

            var ranked = _service.GetRankedEntries(filterGame, keyword);

            _dgvLeaderboard.DataSource = ranked.Select(r => new
            {
                MaVinhDanh = r.Entry.EntryId,
                Hang = r.MedalLabel,
                TuyenThu = r.Entry.PlayerName,
                DoiTuyen = r.Entry.TeamName,
                GiaiDau = r.Entry.TournamentTitle,
                TuaGame = r.Entry.GameTitle,
                DiemVinhDanh = $"{r.Entry.MvpScore:N0}",
                TienThuong = $"{r.Entry.PrizeAmount:N0} VNĐ",
                NgayVinhDanh = r.Entry.AwardedDate.ToString("dd/MM/yyyy"),
                ThanhTich = r.Entry.AchievementNote
            }).ToList();

            SetColumnHeader("MaVinhDanh", "ID", 42);
            SetColumnHeader("Hang", "Hạng", 78);
            SetColumnHeader("TuyenThu", "Tuyển Thủ MVP", 150);
            SetColumnHeader("DoiTuyen", "Đội Tuyển", 125);
            SetColumnHeader("GiaiDau", "Giải Đấu", 165);
            SetColumnHeader("TuaGame", "Tựa Game", 95);
            SetColumnHeader("DiemVinhDanh", "Điểm", 68);
            SetColumnHeader("TienThuong", "Tiền Thưởng", 110);
            SetColumnHeader("NgayVinhDanh", "Ngày", 82);
            SetColumnHeader("ThanhTich", "Thành Tích", 165);

            var podium = _service.GetTopPodium(3, filterGame);
            UpdatePodiumCard(podium.ElementAtOrDefault(0), _lblTop1Player, _lblTop1Details);
            UpdatePodiumCard(podium.ElementAtOrDefault(1), _lblTop2Player, _lblTop2Details);
            UpdatePodiumCard(podium.ElementAtOrDefault(2), _lblTop3Player, _lblTop3Details);

            decimal totalPrize = ranked.Sum(r => r.Entry.PrizeAmount);
            _lblTotalSummary.Text = $"Tổng: {ranked.Count} MVP | Quỹ thưởng: {totalPrize:N0} VNĐ";
        }

        private void SetColumnHeader(string columnName, string headerText, int fillWeight)
        {
            if (_dgvLeaderboard.Columns[columnName] is DataGridViewColumn col)
            {
                col.HeaderText = headerText;
                col.FillWeight = fillWeight;
            }
        }

        private static void UpdatePodiumCard(TournamentLeaderboardRankEntry? rankEntry, Label lblPlayer, Label lblDetails)
        {
            if (rankEntry is null)
            {
                lblPlayer.Text = "-- Chưa có tuyển thủ --";
                lblDetails.Text = "Điểm vinh danh: 0 điểm\nTiền thưởng: 0 VNĐ";
                return;
            }

            var e = rankEntry.Entry;
            lblPlayer.Text = $"{e.PlayerName} ({e.TeamName})";
            lblDetails.Text = $"{e.GameTitle} • Điểm vinh danh: {e.MvpScore:N0} điểm\nTiền thưởng: {e.PrizeAmount:N0} VNĐ";
        }

        private void DgvLeaderboard_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || !_dgvLeaderboard.Columns.Contains("Hang"))
                return;

            string? rankText = _dgvLeaderboard.Rows[e.RowIndex].Cells["Hang"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(rankText))
                return;

            if (rankText.Contains("Top 1", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(254, 249, 195);
                e.CellStyle.ForeColor = Color.FromArgb(113, 63, 18);
            }
            else if (rankText.Contains("Top 2", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(241, 245, 249);
                e.CellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            }
            else if (rankText.Contains("Top 3", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 237, 213);
                e.CellStyle.ForeColor = Color.FromArgb(124, 45, 18);
            }
        }

        public string BuildCurrentExportText(int topCount = 3) =>
            _service.BuildHallOfFameExportText(CurrentGameFilter, topCount);

        public string BuildCurrentCsvText(int topCount = 3) =>
            _service.BuildCsvExportText(CurrentGameFilter, topCount);

        public void ExportTopLeaderboard(string filePath, int topCount = 3) =>
            _service.ExportLeaderboardToFile(filePath, CurrentGameFilter, topCount);

        private void BtnAddMvp_Click(object? sender, EventArgs e)
        {
            try
            {
                var newEntry = new TournamentMvpEntry
                {
                    PlayerName = _txtPlayerName.Text,
                    TeamName = _txtTeamName.Text,
                    TournamentTitle = _txtTournamentTitle.Text,
                    GameTitle = _cboGameTitle.Text,
                    MvpScore = (int)_numMvpScore.Value,
                    PrizeAmount = _numPrizeAmount.Value,
                    AchievementNote = _txtAchievementNote.Text,
                    AwardedDate = _dtpAwardedDate.Value.Date
                };

                _service.AddEntry(newEntry);
                PopulateGameFilters();
                RefreshLeaderboard();
                ClearInputFields();
                ShowMessage("Đã ghi nhận tuyển thủ MVP vào Bảng Vinh Danh thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                ShowMessage(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDeleteMvp_Click(object? sender, EventArgs e)
        {
            if (_dgvLeaderboard.CurrentRow == null || !_dgvLeaderboard.Columns.Contains("MaVinhDanh"))
            {
                ShowMessage("Vui lòng chọn một tuyển thủ cần xóa khỏi bảng vinh danh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int entryId = Convert.ToInt32(_dgvLeaderboard.CurrentRow.Cells["MaVinhDanh"].Value, CultureInfo.InvariantCulture);
            if (_service.RemoveEntry(entryId))
            {
                PopulateGameFilters();
                RefreshLeaderboard();
            }
        }

        private void ClearInputFields()
        {
            _txtPlayerName.Clear();
            _txtTeamName.Clear();
            _txtTournamentTitle.Clear();
            _txtAchievementNote.Clear();
            _numMvpScore.Value = 850;
            _numPrizeAmount.Value = 2000000;
            _dtpAwardedDate.Value = DateTime.Today;
        }

        private void BtnExportTxt_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Xuất Bảng Vinh Danh Top 3 MVP (.txt)",
                Filter = "Text Files (*.txt)|*.txt",
                FileName = $"BangVinhDanh_TopMVP_MasterGame_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                ExportTopLeaderboard(sfd.FileName, topCount: 3);
                ShowMessage("Đã xuất Bảng Vinh Danh Top 1, Top 2, Top 3 thành công!", "Xuất tệp", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportCsv_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Xuất Bảng Vinh Danh Top 3 MVP (.csv)",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"BangVinhDanh_TopMVP_MasterGame_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                ExportTopLeaderboard(sfd.FileName, topCount: 3);
                ShowMessage("Đã xuất Bảng Vinh Danh Top 1, Top 2, Top 3 sang CSV thành công!", "Xuất tệp", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnPrintTop3_Click(object? sender, EventArgs e)
        {
            try
            {
                _pendingPrintContent = BuildCurrentExportText(topCount: 3);
                _pendingPrintCharIndex = 0;

                using var printDoc = new PrintDocument
                {
                    DocumentName = "Bang Vinh Danh Top MVP Master Game"
                };
                printDoc.PrintPage += PrintDocument_PrintPage;

                using var preview = new PrintPreviewDialog
                {
                    Document = printDoc,
                    Width = 820,
                    Height = 620,
                    StartPosition = FormStartPosition.CenterParent
                };
                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ShowMessage($"Không thể mở xem trước bản in: {ex.Message}", "Lỗi in ấn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null)
                return;

            using var printFont = new Font("Consolas", 10f);
            string remaining = _pendingPrintContent[_pendingPrintCharIndex..];

            e.Graphics.MeasureString(
                remaining,
                printFont,
                e.MarginBounds.Size,
                StringFormat.GenericTypographic,
                out int charsFitted,
                out _);

            string pageText = remaining[..charsFitted];
            e.Graphics.DrawString(pageText, printFont, Brushes.Black, e.MarginBounds, StringFormat.GenericTypographic);

            _pendingPrintCharIndex += charsFitted;
            e.HasMorePages = _pendingPrintCharIndex < _pendingPrintContent.Length;
        }
    }
}
