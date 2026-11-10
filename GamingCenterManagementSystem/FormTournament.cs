using System.Data;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem;

public sealed class FormTournament : Form
{
    private readonly DBTournament database = new();
    private readonly TextBox titleInput = new() { Width = 220, MaxLength = 150 };
    private readonly TextBox gameInput = new() { Width = 160, MaxLength = 100 };
    private readonly NumericUpDown prizeInput = new()
    {
        Width = 160, DecimalPlaces = 2, ThousandsSeparator = true,
        Maximum = 9999999999999999.99m
    };
    private readonly DateTimePicker startInput = new()
    {
        Width = 130, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = DateTime.Today
    };
    private readonly DataGridView tournamentsGrid = CreateGrid();
    private readonly DataGridView matchesGrid = CreateGrid();
    private readonly TextBox team1Input = new() { Width = 160, MaxLength = 100 };
    private readonly TextBox team2Input = new() { Width = 160, MaxLength = 100 };
    private readonly NumericUpDown roundInput = new() { Width = 65, Minimum = 1, Maximum = int.MaxValue, Value = 1 };
    private readonly NumericUpDown team1ScoreInput = new() { Width = 90, Maximum = 999999999 };
    private readonly NumericUpDown team2ScoreInput = new() { Width = 90, Maximum = 999999999 };
    private readonly ComboBox winnerInput = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label tournamentLabel = new() { AutoSize = true, Padding = new Padding(4, 10, 4, 4) };
    private readonly Label matchLabel = new() { AutoSize = true, MaximumSize = new Size(700, 0) };
    private readonly Label stationCountLabel = new() { AutoSize = true, Text = "Đã chọn: 0/5 máy" };
    private readonly CheckedListBox stationsList = new() { Dock = DockStyle.Fill, CheckOnClick = true };
    private readonly Button createMatchButton = new() { Text = "Thêm trận", AutoSize = true };
    private readonly Button saveScoreButton = new() { Text = "Lưu kết quả", AutoSize = true };
    private readonly Button lockButton = new() { Text = "Khóa 5 máy cho giải đấu", AutoSize = true };
    private bool loading;

    public FormTournament()
    {
        Text = "Quản lý giải đấu";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1280, 760);
        MinimumSize = new Size(1100, 700);
        AutoScaleMode = AutoScaleMode.Font;

        AddColumn(tournamentsGrid, "Title", "Tên giải", 160);
        AddColumn(tournamentsGrid, "GameTitle", "Trò chơi", 110);
        AddColumn(tournamentsGrid, "PrizePool", "Giải thưởng", 110, "N2");
        AddColumn(tournamentsGrid, "StartDate", "Ngày bắt đầu", 110, "dd/MM/yyyy");
        AddColumn(tournamentsGrid, "Status", "Trạng thái", 100);
        AddColumn(matchesGrid, "RoundNumber", "Vòng", 50);
        AddColumn(matchesGrid, "Team1Name", "Đội 1", 130);
        AddColumn(matchesGrid, "Team2Name", "Đội 2", 130);
        AddColumn(matchesGrid, "Score", "Tỉ số", 70);
        AddColumn(matchesGrid, "WinnerTeamName", "Đội thắng", 130);
        AddColumn(matchesGrid, "Status", "Trạng thái", 100);

        var createTournamentButton = new Button { Text = "Tạo giải đấu", AutoSize = true };
        var refreshButton = new Button { Text = "Tải lại", AutoSize = true };
        var createPanel = CreateFlow();
        createPanel.Controls.AddRange([
            Field("Tên giải đấu", titleInput), Field("Trò chơi", gameInput),
            Field("Tổng giải thưởng", prizeInput), Field("Ngày bắt đầu", startInput),
            createTournamentButton, refreshButton
        ]);
        var createGroup = new GroupBox { Text = "Tạo giải đấu", Dock = DockStyle.Fill };
        createGroup.Controls.Add(createPanel);

        var matchesPage = new TabPage("Trận đấu") { Padding = new Padding(8) };
        var matchLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        matchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        matchLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        matchLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        matchLayout.Controls.Add(matchesGrid, 0, 0);
        var newMatchPanel = CreateFlow();
        newMatchPanel.Controls.AddRange([
            Field("Đội 1", team1Input), Field("Đội 2", team2Input),
            Field("Vòng đấu", roundInput), createMatchButton
        ]);
        var newMatchGroup = new GroupBox { Text = "Thêm trận vào giải đang chọn", Dock = DockStyle.Fill };
        newMatchGroup.Controls.Add(newMatchPanel);
        matchLayout.Controls.Add(newMatchGroup, 0, 1);
        var scorePanel = CreateFlow();
        scorePanel.Controls.AddRange([
            Field("Tỉ số đội 1", team1ScoreInput), Field("Tỉ số đội 2", team2ScoreInput),
            Field("Đội thắng", winnerInput), saveScoreButton
        ]);
        var scoreGroup = new GroupBox { Text = "Kết quả trận đang chọn (tỉ số theo thứ tự đội 1 - đội 2)", Dock = DockStyle.Fill };
        scoreGroup.Controls.Add(scorePanel);
        matchLayout.Controls.Add(scoreGroup, 0, 2);
        matchesPage.Controls.Add(matchLayout);

        var stationsPage = new TabPage("Khóa máy thi đấu") { Padding = new Padding(12) };
        var stationLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        stationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        stationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stationLayout.Controls.Add(matchLabel, 0, 0);
        stationLayout.Controls.Add(new Label
        {
            AutoSize = true, MaximumSize = new Size(700, 0), Margin = new Padding(3, 12, 3, 12),
            Text = "Chọn trận ở tab Trận đấu, rồi chọn đúng 5 máy. Nếu máy đang có khách, cần chuyển khách sang máy khác trước khi khóa."
        }, 0, 1);
        stationLayout.Controls.Add(stationsList, 0, 2);
        stationLayout.Controls.Add(stationCountLabel, 0, 3);
        stationLayout.Controls.Add(lockButton, 0, 4);
        stationsPage.Controls.Add(stationLayout);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.AddRange([matchesPage, stationsPage]);

        var detailLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        detailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detailLayout.Controls.Add(tournamentLabel, 0, 0);
        detailLayout.Controls.Add(tabs, 0, 1);
        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
        split.Panel1.Controls.Add(tournamentsGrid);
        split.Panel2.Controls.Add(detailLayout);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 115));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(createGroup, 0, 0);
        layout.Controls.Add(split, 0, 1);
        Controls.Add(layout);
        // Set splitter limits after layout gives the container its actual width.
        Load += (_, _) => { split.SplitterDistance = 440; split.Panel1MinSize = 300; split.Panel2MinSize = 600; };

        Shown += (_, _) => RunAction(() => ReloadAll());
        refreshButton.Click += (_, _) => RunAction(() => ReloadAll());
        createTournamentButton.Click += (_, _) => RunAction(CreateTournament);
        createMatchButton.Click += (_, _) => RunAction(CreateMatch);
        saveScoreButton.Click += (_, _) => RunAction(SaveScore);
        lockButton.Click += (_, _) => RunAction(LockStations);
        tournamentsGrid.CurrentCellChanged += (_, _) =>
        {
            if (!loading) RunAction(() => LoadMatches());
        };
        matchesGrid.CurrentCellChanged += (_, _) => { if (!loading) UpdateMatchEditor(); };
        stationsList.ItemCheck += (_, e) =>
        {
            int count = stationsList.CheckedItems.Count + (e.NewValue == CheckState.Checked ? 1 : -1);
            stationCountLabel.Text = $"Đã chọn: {count}/5 máy";
        };
        UpdateMatchEditor();
    }

    private void CreateTournament()
    {
        DBTournament.ValidateTournament(titleInput.Text, prizeInput.Value, startInput.Value);
        int id = database.CreateTournament(titleInput.Text, gameInput.Text, prizeInput.Value, startInput.Value);
        titleInput.Clear();
        gameInput.Clear();
        prizeInput.Value = 0;
        RefreshAfterSave(() => ReloadAll(id), "Đã tạo giải đấu.");
    }

    private void CreateMatch()
    {
        int tournamentId = SelectedId(tournamentsGrid, "TournamentId")
            ?? throw new InvalidOperationException("Vui lòng chọn giải đấu trước khi thêm trận.");
        int id = database.CreateMatch(tournamentId, team1Input.Text, team2Input.Text, (int)roundInput.Value);
        team1Input.Clear();
        team2Input.Clear();
        RefreshAfterSave(() => LoadMatches(id), "Đã thêm trận đấu.");
    }

    private void SaveScore()
    {
        DataRowView? match = SelectedRow(matchesGrid);
        if (match == null || Convert.ToInt32(match["TournamentId"]) != SelectedId(tournamentsGrid, "TournamentId"))
            throw new InvalidOperationException("Vui lòng chọn trận đấu thuộc giải đấu hiện tại.");
        int matchId = SelectedId(matchesGrid, "MatchId")
            ?? throw new InvalidOperationException("Vui lòng chọn trận đấu cần cập nhật tỉ số.");
        string winner = winnerInput.SelectedItem as string
            ?? throw new ArgumentException("Vui lòng chọn đội thắng cuộc.");
        database.UpdateMatchScore(matchId, (int)team1ScoreInput.Value, (int)team2ScoreInput.Value, winner);
        RefreshAfterSave(() => LoadMatches(matchId), "Đã lưu tỉ số, đội thắng và trạng thái Finished.");
    }

    private void LockStations()
    {
        int tournamentId = SelectedId(tournamentsGrid, "TournamentId")
            ?? throw new InvalidOperationException("Vui lòng chọn giải đấu.");
        int matchId = SelectedId(matchesGrid, "MatchId")
            ?? throw new InvalidOperationException("Vui lòng chọn trận đấu cần bố trí máy ở tab Trận đấu.");
        int[] stationIds = stationsList.CheckedItems.Cast<StationOption>().Select(station => station.Id).ToArray();
        if (stationIds.Length != 5)
            throw new ArgumentException("Vui lòng chọn đúng 5 máy thi đấu trước khi khóa giải đấu.");
        database.LockTournament(tournamentId, matchId, stationIds);
        RefreshAfterSave(() => ReloadAll(tournamentId, matchId), "Đã khóa 5 máy thi đấu và chuyển giải đấu sang Ongoing.");
    }

    private void ReloadAll(int? tournamentId = null, int? matchId = null)
    {
        tournamentId ??= SelectedId(tournamentsGrid, "TournamentId");
        DataTable tournaments = database.GetTournaments();
        loading = true;
        try
        {
            ReplaceData(tournamentsGrid, tournaments);
            SelectRow(tournamentsGrid, "TournamentId", tournamentId);
        }
        finally { loading = false; }
        LoadMatches(matchId);
        LoadStations();
    }

    private void LoadMatches(int? matchId = null)
    {
        int? tournamentId = SelectedId(tournamentsGrid, "TournamentId");
        DataRowView? tournament = SelectedRow(tournamentsGrid);
        tournamentLabel.Text = tournament == null ? "Chưa có giải đấu. Hãy tạo giải đấu đầu tiên." : $"Giải đấu: {tournament["Title"]}";
        createMatchButton.Enabled = tournament != null && Convert.ToString(tournament["Status"]) != "Finished";
        // A failed reload must not leave the previous tournament's matches editable.
        loading = true;
        try { ReplaceData(matchesGrid, null); }
        finally { loading = false; }
        UpdateMatchEditor();
        DataTable? matches = tournamentId.HasValue ? database.GetMatches(tournamentId.Value) : null;
        loading = true;
        try
        {
            ReplaceData(matchesGrid, matches);
            SelectRow(matchesGrid, "MatchId", matchId);
        }
        finally { loading = false; }
        UpdateMatchEditor();
    }

    private void LoadStations()
    {
        int[] checkedIds = stationsList.CheckedItems.Cast<StationOption>().Select(station => station.Id).ToArray();
        using DataTable stations = database.GetStations();
        stationsList.Items.Clear();
        foreach (DataRow station in stations.Rows)
        {
            int id = Convert.ToInt32(station["StationId"]);
            string status = Convert.ToBoolean(station["IsOccupied"]) ? "Có khách - cần chuyển máy" : Convert.ToString(station["Status"]) switch
            {
                "Available" => "Sẵn sàng", "Maintenance" => "Bảo trì",
                "TournamentLocked" => "Đã khóa thi đấu", var value => value ?? "Không rõ trạng thái"
            };
            stationsList.Items.Add(new StationOption(id, $"{station["StationName"]} - {status}"), checkedIds.Contains(id));
        }
        stationCountLabel.Text = $"Đã chọn: {stationsList.CheckedItems.Count}/5 máy";
    }

    private void UpdateMatchEditor()
    {
        DataRowView? match = SelectedRow(matchesGrid);
        winnerInput.Items.Clear();
        team1ScoreInput.Value = 0;
        team2ScoreInput.Value = 0;
        bool tournamentOpen = SelectedRow(tournamentsGrid) is { } tournament && Convert.ToString(tournament["Status"]) != "Finished";
        saveScoreButton.Enabled = tournamentOpen && match != null && Convert.ToString(match["Status"]) != "Finished";
        lockButton.Enabled = tournamentOpen && match != null && Convert.ToString(match["Status"]) == "Pending"
            && string.IsNullOrWhiteSpace(Convert.ToString(match["AssignedStationIds"]));
        if (match == null)
        {
            matchLabel.Text = "Chưa chọn trận đấu.";
            return;
        }

        string team1 = Convert.ToString(match["Team1Name"]) ?? "";
        string team2 = Convert.ToString(match["Team2Name"]) ?? "";
        matchLabel.Text = $"Trận được chọn: {team1} - {team2} (vòng {match["RoundNumber"]}, {match["Status"]})";
        winnerInput.Items.AddRange([team1, team2]);
        string? previousWinner = Convert.ToString(match["WinnerTeamName"]);
        if (!string.IsNullOrEmpty(previousWinner)) winnerInput.SelectedItem = previousWinner;
        string[] scores = (Convert.ToString(match["Score"]) ?? "").Split('-');
        if (scores.Length == 2 && int.TryParse(scores[0], out int first) && int.TryParse(scores[1], out int second))
        {
            team1ScoreInput.Value = Math.Clamp(first, 0, team1ScoreInput.Maximum);
            team2ScoreInput.Value = Math.Clamp(second, 0, team2ScoreInput.Maximum);
        }
    }

    private void RunAction(Action action)
    {
        try
        {
            UseWaitCursor = true;
            action();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, $"Không thể thao tác với cơ sở dữ liệu.\n{ex.Message}", "Lỗi cơ sở dữ liệu",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    private void RefreshAfterSave(Action refresh, string successMessage)
    {
        try { refresh(); }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            saveScoreButton.Enabled = false;
            lockButton.Enabled = false;
            MessageBox.Show(this, $"{successMessage}\nDữ liệu đã được lưu nhưng không thể tải lại danh sách. Bấm Tải lại để cập nhật.\n{ex.Message}",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        MessageBox.Show(this, successMessage, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AutoGenerateColumns = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private static void AddColumn(DataGridView grid, string property, string header, int weight, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = property, HeaderText = header, FillWeight = weight, MinimumWidth = 50
        };
        if (format != null) column.DefaultCellStyle.Format = format;
        grid.Columns.Add(column);
    }

    private static FlowLayoutPanel CreateFlow() => new()
    {
        Dock = DockStyle.Fill, Padding = new Padding(8), AutoScroll = true, WrapContents = true
    };

    private static Control Field(string caption, Control input)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(3, 0, 8, 3)
        };
        panel.Controls.Add(new Label { Text = caption, AutoSize = true });
        panel.Controls.Add(input);
        return panel;
    }

    private static DataRowView? SelectedRow(DataGridView grid) => grid.CurrentRow?.DataBoundItem as DataRowView;

    private static int? SelectedId(DataGridView grid, string column) =>
        SelectedRow(grid) is { } row ? Convert.ToInt32(row[column]) : null;

    private static void SelectRow(DataGridView grid, string column, int? id)
    {
        if (grid.Rows.Count == 0) return;
        DataGridViewRow target = grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row =>
            row.DataBoundItem is DataRowView data && Convert.ToInt32(data[column]) == id) ?? grid.Rows[0];
        grid.CurrentCell = target.Cells[0];
        target.Selected = true;
    }

    private static void ReplaceData(DataGridView grid, DataTable? data)
    {
        var previous = grid.DataSource as DataTable;
        grid.DataSource = data;
        previous?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            (tournamentsGrid.DataSource as DataTable)?.Dispose();
            (matchesGrid.DataSource as DataTable)?.Dispose();
        }
        base.Dispose(disposing);
    }

    private sealed record StationOption(int Id, string Display)
    {
        public override string ToString() => Display;
    }
}
