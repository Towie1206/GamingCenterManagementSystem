using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormTournament : Form
    {
        private readonly DBTournament _dbTournament = new();
        private List<Tournament> _tournamentList = new();
        private List<TournamentMatch> _matchList = new();
        private Tournament? _selectedTournament = null;
        private TournamentMatch? _selectedMatch = null;
        private bool _loadingTournaments;

        private sealed class StationCheckItem
        {
            public int StationId { get; init; }
            public string StationName { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public bool IsOccupied { get; init; }
            public bool CanLock => !IsOccupied && string.Equals(Status, "Available", StringComparison.OrdinalIgnoreCase);

            public override string ToString() => $"{StationName} [{(IsOccupied ? "Đang có khách (Active)" : Status)}]";
        }

        // Delegate / Action for tournament events (Chapter 4 Delegate requirement)
        public Action<string>? OnTournamentEvent;

        public FormTournament()
        {
            InitializeComponent();
            SetupEvents();
            LoadTournaments();
            LoadStationsChecklist();
        }

        private void SetupEvents()
        {
            Load += (s, e) => RefreshTournamentData();
            btnTaoGiai.Click += BtnTaoGiai_Click;
            btnCapNhatGiai.Click += BtnCapNhatGiai_Click;
            btnXoaGiai.Click += BtnXoaGiai_Click;
            btnLamMoi.Click += (s, e) => RefreshTournamentData();

            btnKhoaMayGiaiDau.Click += BtnKhoaMayGiaiDau_Click;
            btnMoKhoaMay.Click += BtnMoKhoaMay_Click;

            btnThemTranDau.Click += BtnThemTranDau_Click;
            btnCapNhatKetQua.Click += BtnCapNhatKetQua_Click;
            btnTaoVongSau.Click += BtnTaoVongSau_Click;
            clbDanMay.ItemCheck += ClbDanMay_ItemCheck;

            dgvGiaiDau.SelectionChanged += DgvGiaiDau_SelectionChanged;
            dgvTranDau.SelectionChanged += DgvTranDau_SelectionChanged;
        }

        private void RefreshTournamentData()
        {
            LoadTournaments(_selectedTournament?.TournamentId);
            LoadStationsChecklist();
        }

        private void LoadTournaments(int? preserveTournamentId = null)
        {
            try
            {
                _tournamentList = _dbTournament.GetAll();
                _loadingTournaments = true;
                try
                {
                    BindTournamentGrid(_tournamentList);
                    if (preserveTournamentId.HasValue)
                    {
                        foreach (DataGridViewRow row in dgvGiaiDau.Rows)
                        {
                            if (Convert.ToInt32(row.Cells["MaGiai"].Value) != preserveTournamentId.Value) continue;
                            dgvGiaiDau.CurrentCell = row.Cells["MaGiai"];
                            row.Selected = true;
                            break;
                        }
                    }
                }
                finally
                {
                    _loadingTournaments = false;
                }

                if (_tournamentList.Count == 0)
                {
                    _selectedTournament = null;
                    _selectedMatch = null;
                    _matchList.Clear();
                    dgvTranDau.DataSource = null;
                    ClearMatchInputs();
                    txtTournamentId.Clear();
                }
                else
                {
                    DgvGiaiDau_SelectionChanged(dgvGiaiDau, EventArgs.Empty);
                }
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách giải đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindTournamentGrid(List<Tournament> tournaments)
        {
            // LINQ projection for tournaments
            var displayData = tournaments.Select(t => new
            {
                MaGiai = t.TournamentId,
                TenGiaiDau = t.Title,
                TuaGame = t.GameTitle,
                TheThuc = t.FormatType,
                TienThuong = $"{t.PrizePool:N0} VNĐ",
                TrangThai = t.Status,
                NgayKhoiTranh = t.StartDate.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            dgvGiaiDau.DataSource = displayData;

            if (dgvGiaiDau.Columns["MaGiai"] is DataGridViewColumn col1) col1.HeaderText = "Mã";
            if (dgvGiaiDau.Columns["TenGiaiDau"] is DataGridViewColumn col2) col2.HeaderText = "Tên Giải Đấu";
            if (dgvGiaiDau.Columns["TuaGame"] is DataGridViewColumn col3) col3.HeaderText = "Tựa Game";
            if (dgvGiaiDau.Columns["TheThuc"] is DataGridViewColumn col4) col4.HeaderText = "Thể Thức";
            if (dgvGiaiDau.Columns["TienThuong"] is DataGridViewColumn col5) col5.HeaderText = "Tổng Thưởng";
            if (dgvGiaiDau.Columns["TrangThai"] is DataGridViewColumn col6) col6.HeaderText = "Trạng Thái";
            if (dgvGiaiDau.Columns["NgayKhoiTranh"] is DataGridViewColumn col7) col7.HeaderText = "Ngày Khởi Tranh";
        }

        private void LoadMatchesForSelectedTournament(int tournamentId)
        {
            _selectedMatch = null;
            ClearMatchInputs();
            _matchList.Clear();
            dgvTranDau.DataSource = null;
            try
            {
                _matchList = _dbTournament.GetMatchesByTournament(tournamentId);

                var displayMatches = _matchList.Select(m => new
                {
                    MaTran = m.MatchId,
                    VongDau = $"Vòng {m.RoundNumber}",
                    Doi1 = m.Team1Name,
                    Doi2 = m.Team2Name,
                    DoiThang = m.WinnerTeamName ?? "-- Chưa rõ --",
                    TiSo = m.Score ?? "0 - 0",
                    MayThiDau = m.AssignedStationIds ?? "Chưa gán",
                    TrangThai = m.Status
                }).ToList();

                dgvTranDau.DataSource = displayMatches;

                if (dgvTranDau.Columns["MaTran"] is DataGridViewColumn mCol1) mCol1.HeaderText = "Mã";
                if (dgvTranDau.Columns["VongDau"] is DataGridViewColumn mCol2) mCol2.HeaderText = "Vòng";
                if (dgvTranDau.Columns["Doi1"] is DataGridViewColumn mCol3) mCol3.HeaderText = "Đội 1";
                if (dgvTranDau.Columns["Doi2"] is DataGridViewColumn mCol4) mCol4.HeaderText = "Đội 2";
                if (dgvTranDau.Columns["DoiThang"] is DataGridViewColumn mCol5) mCol5.HeaderText = "Đội Thắng";
                if (dgvTranDau.Columns["TiSo"] is DataGridViewColumn mCol6) mCol6.HeaderText = "Tỉ Số";
                if (dgvTranDau.Columns["MayThiDau"] is DataGridViewColumn mCol7) mCol7.HeaderText = "Dàn Máy Gán";
                if (dgvTranDau.Columns["TrangThai"] is DataGridViewColumn mCol8) mCol8.HeaderText = "Trạng Thái";

                DgvTranDau_SelectionChanged(dgvTranDau, EventArgs.Empty);
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách trận đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearMatchInputs()
        {
            txtRoundNumber.Text = "1";
            txtTeam1.Clear();
            txtTeam2.Clear();
            txtScore.Text = "0 - 0";
            cboWinnerTeam.Items.Clear();
            cboWinnerTeam.Items.Add("-- Chưa có --");
            cboWinnerTeam.SelectedIndex = 0;
            cboMatchStatus.SelectedIndex = 0;
        }

        private List<StationCheckItem> ReadStationCheckItems()
        {
            using DataTable stations = _dbTournament.GetStations();
            if (stations.Rows.Count > 0)
            {
                return stations.AsEnumerable().Select(row => new StationCheckItem
                {
                    StationId = row.Field<int>("StationId"),
                    StationName = row.Field<string>("StationName") ?? string.Empty,
                    Status = row.Field<string>("Status") ?? string.Empty,
                    IsOccupied = row.Field<bool>("IsOccupied")
                }).ToList();
            }

            return new DBStation().GetAll().Select(station => new StationCheckItem
            {
                StationId = station.StationId,
                StationName = station.StationName,
                Status = station.Status,
                IsOccupied = string.Equals(station.Status, "InUse", StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        private void LoadStationsChecklist()
        {
            var checkedIds = clbDanMay.CheckedItems.Cast<StationCheckItem>().Select(item => item.StationId).ToHashSet();
            var stations = ReadStationCheckItems();
            clbDanMay.BeginUpdate();
            try
            {
                clbDanMay.Items.Clear();
                foreach (var station in stations)
                    clbDanMay.Items.Add(station, checkedIds.Contains(station.StationId));
            }
            finally
            {
                clbDanMay.EndUpdate();
            }
            UpdateSelectedStationCounter();
        }

        private void ClbDanMay_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // ItemCheck runs before CheckedItems reflects the click.
            var selected = clbDanMay.CheckedItems.Cast<StationCheckItem>().ToList();
            var changed = (StationCheckItem)clbDanMay.Items[e.Index];
            selected.Remove(changed);
            if (e.NewValue == CheckState.Checked) selected.Add(changed);
            SetSelectedStationCounter(selected);
        }

        private void UpdateSelectedStationCounter() =>
            SetSelectedStationCounter(clbDanMay.CheckedItems.Cast<StationCheckItem>().ToList());

        private void SetSelectedStationCounter(IReadOnlyCollection<StationCheckItem> selected)
        {
            int readyCount = selected.Count(item => item.CanLock);
            lblSoMayDaChon.Text = $"Đã chọn: {selected.Count}/5 máy (Sẵn sàng khóa: {readyCount}/5)";
            lblSoMayDaChon.ForeColor = selected.Count == 5 && readyCount == 5
                ? Color.Green : Color.FromArgb(139, 69, 19);
        }

        private void UpdateStatistics()
        {
            // LINQ Count
            int totalTourneys = _tournamentList.Count;
            int ongoingTourneys = _tournamentList.Count(t => t.Status.Equals("Ongoing", StringComparison.OrdinalIgnoreCase));
            int totalMatches = _matchList.Count;

            lblTongSoGiai.Text = $"🏆 Tổng số giải: {totalTourneys}";
            lblGiaiDangDienRa.Text = $"🔥 Đang diễn ra: {ongoingTourneys}";
            lblSoTranDau.Text = $"⚔️ Tổng số trận đấu: {totalMatches}";
        }

        private void DgvGiaiDau_SelectionChanged(object? sender, EventArgs e)
        {
            if (_loadingTournaments) return;
            if (dgvGiaiDau.CurrentRow != null && dgvGiaiDau.CurrentRow.DataBoundItem != null)
            {
                int tournamentId = Convert.ToInt32(dgvGiaiDau.CurrentRow.Cells["MaGiai"].Value);
                _selectedTournament = _tournamentList.FirstOrDefault(t => t.TournamentId == tournamentId);
                if (_selectedTournament != null)
                {
                    txtTournamentId.Text = _selectedTournament.TournamentId.ToString();
                    txtTenGiai.Text = _selectedTournament.Title;
                    txtGameTitle.Text = _selectedTournament.GameTitle;
                    cboFormatType.SelectedItem = _selectedTournament.FormatType;
                    txtPrizePool.Text = _selectedTournament.PrizePool.ToString("F0");
                    dtpStartDate.Value = _selectedTournament.StartDate;
                    cboTournamentStatus.SelectedItem = _selectedTournament.Status;

                    LoadMatchesForSelectedTournament(_selectedTournament.TournamentId);
                }
            }
        }

        private void DgvTranDau_SelectionChanged(object? sender, EventArgs e)
        {
            _selectedMatch = null;
            if (dgvTranDau.CurrentRow != null && dgvTranDau.CurrentRow.DataBoundItem != null)
            {
                int matchId = Convert.ToInt32(dgvTranDau.CurrentRow.Cells["MaTran"].Value);
                _selectedMatch = _matchList.FirstOrDefault(m => m.MatchId == matchId && m.TournamentId == _selectedTournament?.TournamentId);
                if (_selectedMatch != null)
                {
                    txtRoundNumber.Text = _selectedMatch.RoundNumber.ToString();
                    txtTeam1.Text = _selectedMatch.Team1Name;
                    txtTeam2.Text = _selectedMatch.Team2Name;
                    txtScore.Text = _selectedMatch.Score ?? "0 - 0";
                    cboMatchStatus.SelectedItem = _selectedMatch.Status;

                    cboWinnerTeam.Items.Clear();
                    cboWinnerTeam.Items.Add("-- Chưa có --");
                    if (!string.IsNullOrWhiteSpace(_selectedMatch.Team1Name)) cboWinnerTeam.Items.Add(_selectedMatch.Team1Name);
                    if (!string.IsNullOrWhiteSpace(_selectedMatch.Team2Name)) cboWinnerTeam.Items.Add(_selectedMatch.Team2Name);

                    if (!string.IsNullOrEmpty(_selectedMatch.WinnerTeamName))
                    {
                        cboWinnerTeam.SelectedItem = _selectedMatch.WinnerTeamName;
                    }
                    else
                    {
                        cboWinnerTeam.SelectedIndex = 0;
                    }
                }
            }
            if (_selectedMatch == null) ClearMatchInputs();
        }

        private void BtnTaoGiai_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtPrizePool.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal prizePool) &&
                    !decimal.TryParse(txtPrizePool.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out prizePool))
                {
                    MessageBox.Show("Tổng giải thưởng không hợp lệ. Vui lòng nhập số hợp lệ.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DBTournament.ValidateTournament(txtTenGiai.Text, prizePool, dtpStartDate.Value);
                DBTournament.ValidateText(txtGameTitle.Text, 100, "Tên trò chơi");

                var newTourney = new Tournament
                {
                    Title = txtTenGiai.Text.Trim(),
                    GameTitle = txtGameTitle.Text.Trim(),
                    FormatType = cboFormatType.SelectedItem?.ToString() ?? "SingleElimination",
                    PrizePool = prizePool,
                    Status = cboTournamentStatus.SelectedItem?.ToString() ?? "Upcoming",
                    StartDate = dtpStartDate.Value
                };

                if (_dbTournament.Add(newTourney))
                {
                    MessageBox.Show($"Tạo giải đấu '{newTourney.Title}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnTournamentEvent?.Invoke($"Đã tạo giải đấu mới: {newTourney.Title}");
                    LoadTournaments();
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo giải đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCapNhatGiai_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTournamentId.Text))
            {
                MessageBox.Show("Vui lòng chọn giải đấu cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int id = Convert.ToInt32(txtTournamentId.Text);
                if (!decimal.TryParse(txtPrizePool.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal prizePool) &&
                    !decimal.TryParse(txtPrizePool.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out prizePool))
                {
                    MessageBox.Show("Tổng giải thưởng không hợp lệ. Vui lòng nhập số hợp lệ.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string status = cboTournamentStatus.SelectedItem?.ToString() ?? "Upcoming";
                if (dtpStartDate.Value.Date < DateTime.Today && (status != "Ongoing" && status != "Finished"))
                {
                    throw new ArgumentException("Ngày bắt đầu phải từ ngày hiện tại trở đi.", nameof(dtpStartDate));
                }

                if (dtpStartDate.Value.Date < DateTime.Today && (status == "Ongoing" || status == "Finished"))
                {
                    DBTournament.ValidateText(txtTenGiai.Text, 150, "Tên giải đấu");
                    if (prizePool <= 0 || prizePool > DBTournament.MaximumPrizePool)
                        throw new ArgumentException("Tổng giải thưởng phải lớn hơn 0 và không vượt quá 9.999.999.999.999.999,99.", nameof(prizePool));
                    if (decimal.Round(prizePool, 2) != prizePool)
                        throw new ArgumentException("Tổng giải thưởng chỉ được có tối đa 2 chữ số thập phân.", nameof(prizePool));
                }
                else
                {
                    DBTournament.ValidateTournament(txtTenGiai.Text, prizePool, dtpStartDate.Value);
                }
                DBTournament.ValidateText(txtGameTitle.Text, 100, "Tên trò chơi");

                var tourney = new Tournament
                {
                    TournamentId = id,
                    Title = txtTenGiai.Text.Trim(),
                    GameTitle = txtGameTitle.Text.Trim(),
                    FormatType = cboFormatType.SelectedItem?.ToString() ?? "SingleElimination",
                    PrizePool = prizePool,
                    Status = status,
                    StartDate = dtpStartDate.Value
                };

                if (_dbTournament.Update(tourney))
                {
                    MessageBox.Show($"Cập nhật thông tin giải '{tourney.Title}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTournaments();
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật giải đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoaGiai_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTournamentId.Text)) return;
            int id = Convert.ToInt32(txtTournamentId.Text);

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa giải đấu '{txtTenGiai.Text}' cùng toàn bộ các trận đấu?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                if (_dbTournament.Delete(id))
                {
                    MessageBox.Show("Xóa giải đấu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTournaments();
                }
            }
        }

        private void BtnThemTranDau_Click(object? sender, EventArgs e)
        {
            if (_selectedTournament == null)
            {
                MessageBox.Show("Vui lòng chọn một giải đấu ở bảng trên trước khi xếp cặp đấu!", "Chưa chọn giải", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedTournament.Status == "Finished")
            {
                MessageBox.Show("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!int.TryParse(txtRoundNumber.Text.Trim(), out int round) || round <= 0)
                {
                    throw new ArgumentException("Vòng đấu phải lớn hơn 0.", nameof(round));
                }

                DBTournament.ValidateMatch(txtTeam1.Text, txtTeam2.Text, round);

                var newMatch = new TournamentMatch
                {
                    TournamentId = _selectedTournament.TournamentId,
                    RoundNumber = round,
                    Team1Name = txtTeam1.Text.Trim(),
                    Team2Name = txtTeam2.Text.Trim(),
                    Score = "0 - 0",
                    AssignedStationIds = null,
                    Status = "Pending"
                };

                if (_dbTournament.AddMatch(newMatch))
                {
                    MessageBox.Show($"Xếp cặp đấu '{newMatch.Team1Name}' VS '{newMatch.Team2Name}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMatchesForSelectedTournament(_selectedTournament.TournamentId);
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm trận đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCapNhatKetQua_Click(object? sender, EventArgs e)
        {
            if (_selectedMatch == null || _selectedTournament == null)
            {
                MessageBox.Show("Vui lòng chọn trận đấu từ bảng dưới!", "Chưa chọn trận đấu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedTournament.Status == "Finished")
            {
                MessageBox.Show("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedMatch.Status == "Finished")
            {
                MessageBox.Show("Trận đấu đã kết thúc và đã có kết quả. Vui lòng tải lại danh sách.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string scoreText = txtScore.Text.Trim();
                string winner = cboWinnerTeam.SelectedItem?.ToString() ?? "";
                string winnerName = (winner == "-- Chưa có --") ? "" : winner;
                string status = cboMatchStatus.SelectedItem?.ToString() ?? "Pending";

                if (status == "Finished" || !string.IsNullOrEmpty(winnerName))
                {
                    if (!DBTournament.TryParseScore(scoreText, out int s1, out int s2))
                    {
                        throw new ArgumentException("Tỉ số không hợp lệ. Vui lòng nhập theo định dạng 'Đội1 - Đội2' (ví dụ: 2-1).");
                    }

                    DBTournament.ValidateMatchScore(_selectedMatch.Team1Name, _selectedMatch.Team2Name, s1, s2, winnerName);
                    _selectedMatch.Score = DBTournament.FormatScore(s1, s2);
                    _selectedMatch.WinnerTeamName = (s1 > s2 ? _selectedMatch.Team1Name : _selectedMatch.Team2Name).Trim();
                    _selectedMatch.Status = "Finished";
                }
                else
                {
                    if (DBTournament.TryParseScore(scoreText, out int s1, out int s2))
                    {
                        if (s1 < 0 || s2 < 0)
                            throw new ArgumentException("Tỉ số không được âm.");
                        if (DBTournament.FormatScore(s1, s2).Length > 20)
                            throw new ArgumentException("Tỉ số vượt quá độ dài lưu trữ cho phép (20 ký tự).");
                        _selectedMatch.Score = DBTournament.FormatScore(s1, s2);
                    }
                    else
                    {
                        _selectedMatch.Score = scoreText;
                    }
                    _selectedMatch.WinnerTeamName = null;
                    _selectedMatch.Status = status;
                }

                if (_dbTournament.UpdateMatch(_selectedMatch))
                {
                    MessageBox.Show("Cập nhật kết quả trận đấu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMatchesForSelectedTournament(_selectedTournament.TournamentId);
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật trận đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKhoaMayGiaiDau_Click(object? sender, EventArgs e)
        {
            if (_selectedTournament == null)
            {
                MessageBox.Show("Vui lòng chọn một giải đấu trước khi khóa máy!", "Chưa chọn giải", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedTournament.Status == "Finished")
            {
                MessageBox.Show("Giải đấu đã kết thúc, không thể thay đổi trận đấu hoặc khóa máy.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedMatch == null || _selectedMatch.TournamentId != _selectedTournament.TournamentId)
            {
                MessageBox.Show("Vui lòng chọn một trận đấu ở bảng dưới để gán & khóa 5 máy thi đấu!", "Chưa chọn trận đấu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var checkedItems = clbDanMay.CheckedItems.Cast<StationCheckItem>().ToList();
                if (checkedItems.Count != 5)
                    throw new ArgumentException($"Vui lòng tích chọn ĐÚNG 5 máy thi đấu (hiện đang chọn {checkedItems.Count} máy).");

                var unavailable = checkedItems.Where(item => !item.CanLock).ToList();
                if (unavailable.Count > 0)
                    throw new InvalidOperationException($"Máy không sẵn sàng: {string.Join(", ", unavailable)}. Vui lòng chọn máy Available, không có khách.");

                int tournamentId = _selectedTournament.TournamentId;
                int[] stationIds = checkedItems.Select(item => item.StationId).ToArray();
                _dbTournament.LockTournament(tournamentId, _selectedMatch.MatchId, stationIds);
                MessageBox.Show("Đã KHÓA thành công 5 máy thi đấu cho trận đã chọn!", "Khóa máy giải đấu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTournaments(tournamentId);
                LoadStationsChecklist();
                OnTournamentEvent?.Invoke($"Đã khóa dàn máy thi đấu: {string.Join(", ", checkedItems.Select(item => item.StationName))}");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khóa máy thi đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMoKhoaMay_Click(object? sender, EventArgs e)
        {
            try
            {
                int[] stationIds = GetLockedStationIds();
                if (stationIds.Length == 0)
                    throw new InvalidOperationException("Không có máy nào ở trạng thái 'TournamentLocked' được chọn để giải phóng.");

                if (_dbTournament.UnlockTournamentStations(stationIds))
                {
                    MessageBox.Show($"Đã GIẢI PHÓNG thành công {stationIds.Length} máy trở về trạng thái 'Available' cho khách thường!", "Giải phóng dàn máy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStationsChecklist();
                    OnTournamentEvent?.Invoke($"Đã mở lại dàn máy thi đấu sang Available.");
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở khóa dàn máy: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int[] GetLockedStationIds()
        {
            // Re-read statuses so stale checklist entries cannot release a customer's machine.
            var stations = ReadStationCheckItems();
            IEnumerable<StationCheckItem> selected;
            if (clbDanMay.CheckedItems.Count > 0)
            {
                var checkedIds = clbDanMay.CheckedItems.Cast<StationCheckItem>().Select(item => item.StationId).ToHashSet();
                selected = stations.Where(item => checkedIds.Contains(item.StationId));
            }
            else
            {
                if (_selectedMatch == null || _selectedMatch.TournamentId != _selectedTournament?.TournamentId)
                    return Array.Empty<int>();

                var assignments = (_selectedMatch.AssignedStationIds ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var assignedIds = assignments.Where(value => int.TryParse(value, out _))
                    .Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToHashSet();
                var assignedNames = assignments.ToHashSet(StringComparer.OrdinalIgnoreCase);
                selected = stations.Where(item => assignedIds.Contains(item.StationId) || assignedNames.Contains(item.StationName));
            }

            return selected.Where(item => !item.IsOccupied && string.Equals(item.Status, "TournamentLocked", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.StationId).Distinct().ToArray();
        }

        private void BtnTaoVongSau_Click(object? sender, EventArgs e)
        {
            if (_selectedTournament == null)
            {
                MessageBox.Show("Vui lòng chọn một giải đấu trước khi tạo vòng sau!", "Chưa chọn giải", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int tournamentId = _selectedTournament.TournamentId;
                var result = _dbTournament.AutoAdvanceRound(tournamentId);
                string message;
                if (result.IsTournamentFinished)
                {
                    message = $"🏆 CHÚC MỪNG ĐỘI '{result.ChampionTeamName}' ĐÃ VÔ ĐỊCH GIẢI ĐẤU (Vòng {result.CompletedRound})!\nTrạng thái giải đấu đã được chuyển sang 'Finished'.";
                }
                else
                {
                    int pairCount = result.CreatedMatches.Count(match => match.Status == "Pending");
                    message = $"Đã tạo {pairCount} cặp đấu ở Vòng {result.NextRound}.";
                    if (!string.IsNullOrWhiteSpace(result.ByeTeamName))
                        message += $"\nĐội '{result.ByeTeamName}' được miễn đấu (BYE) và tự động đi tiếp.";
                }

                MessageBox.Show(message, "Tạo cặp đấu vòng sau", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTournaments(tournamentId);
                OnTournamentEvent?.Invoke(result.IsTournamentFinished
                    ? $"Giải đấu đã kết thúc. Nhà vô địch: {result.ChampionTeamName}"
                    : $"Đã tạo Vòng {result.NextRound} cho giải đấu.");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo vòng đấu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
