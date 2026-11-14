using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem
{
    public partial class FormClientDashboard : Form
    {
        private readonly DBStation _dbStation;
        private readonly DBBilling _dbBilling;
        private readonly DBUser _dbUser;
        private Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? _promptHandler;

        private ServiceItem? _selectedFoodItem;
        private string _currentGameCategory = "Tất cả";
        private string _currentFoodCategory = "Tất cả";

        public bool IsLogoutRequested { get; private set; }
        public bool IsScreenLocked { get; private set; }
        public bool IsPreviewMode { get; private set; }
        public bool IsGameRunning => RunningGame != null;
        public GamingSession? CurrentSession { get; private set; }
        public ComputerStation? CurrentStation { get; private set; }
        public StationZone? CurrentZone { get; private set; }
        public decimal BasePricePerHour { get; private set; } = 10000m;
        public decimal CurrentBalance { get; private set; } = 0m;
        public GameItem? RunningGame { get; private set; }
        public string? LastStaffCallReason { get; private set; }

        public int MemberUserId { get; private set; } = 2;
        public string MemberUsername { get; private set; } = string.Empty;
        public string MemberFullName { get; private set; } = string.Empty;
        public string MemberTier { get; private set; } = "Standard";

        public List<GameItem> AvailableGames { get; } = new();
        public List<ServiceItem> AvailableServiceItems { get; private set; } = new();
        public List<SessionServiceDetail> SessionServices { get; private set; } = new();
        private readonly List<SessionServiceDetail> _inMemorySessionOrders = new();
        private bool _isOutOfTimeLocked = false;

        public FormClientDashboard()
            : this(new DBStation(), new DBBilling(), new DBUser(), null)
        {
        }

        public FormClientDashboard(DBStation dbStation, DBBilling dbBilling, DBUser dbUser, int? stationId = null)
        {
            ArgumentNullException.ThrowIfNull(dbStation);
            ArgumentNullException.ThrowIfNull(dbBilling);
            ArgumentNullException.ThrowIfNull(dbUser);

            _dbStation = dbStation;
            _dbBilling = dbBilling;
            _dbUser = dbUser;

            InitializeComponent();
            InitializeDefaultSessionAndStation(stationId);
            InitializeGameCatalog();
            SetupEventHandlers();
            LoadFoodCatalog();
            LoadSessionOrders();
            UpdateRealtimeHud();

            tmrRealtime.Tick += (s, e) => UpdateRealtimeHud();
            tmrRealtime.Start();

            FormClosing += (s, e) =>
            {
                tmrRealtime.Stop();

                if (IsScreenLocked && !IsLogoutRequested)
                {
                    e.Cancel = true;
                    ShowNotification(
                        "Máy trạm đang bị khóa màn hình. Vui lòng nhập mật khẩu mở khóa trước khi thao tác!",
                        "Máy đang khóa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (CurrentUserSession.IsMember && !IsLogoutRequested && e.CloseReason == CloseReason.UserClosing)
                {
                    var confirm = ShowNotification(
                        $"Bạn có muốn kết thúc phiên chơi và đăng xuất khỏi máy {CurrentStation?.StationName ?? "PC"}?",
                        "Xác nhận kết thúc phiên",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm == DialogResult.Yes)
                    {
                        EndSessionAndLogout();
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        internal void SetPromptHandler(Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>? handler)
        {
            _promptHandler = handler;
        }

        private DialogResult ShowNotification(
            string text,
            string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if (_promptHandler != null)
                return _promptHandler(text, caption, buttons, icon);

            return MessageBox.Show(this, text, caption, buttons, icon);
        }

        private void InitializeDefaultSessionAndStation(int? requestedStationId)
        {
            // 1. Ensure current user session has default if not logged in
            if (!CurrentUserSession.IsLoggedIn)
            {
                CurrentUserSession.SetSession(
                    userId: 2,
                    username: "gamer01",
                    role: "Member",
                    fullName: "Nguyễn Văn A",
                    balance: 100000m,
                    tier: "Standard",
                    citizenId: "001099000002");
            }

            // 2. Resolve Active Session & Station
            GamingSession? activeSession = null;
            if (requestedStationId.HasValue)
            {
                try
                {
                    activeSession = _dbBilling.GetActiveSession(requestedStationId.Value);
                }
                catch { }

                if (activeSession != null)
                {
                    CurrentStation = _dbStation.GetById(requestedStationId.Value);
                }
            }

            if (activeSession == null && CurrentUserSession.IsMember)
            {
                try
                {
                    var userSessions = _dbBilling.GetActiveSessions();
                    activeSession = userSessions.FirstOrDefault(s => s.UserId == CurrentUserSession.UserId);
                    if (activeSession != null)
                    {
                        CurrentStation = _dbStation.GetById(activeSession.StationId);
                    }
                }
                catch { }
            }

            // 3. If no active session, handle based on role
            bool isAdminOrStaff = CurrentUserSession.IsAdmin || CurrentUserSession.IsStaff;
            if (activeSession == null)
            {
                var stations = _dbStation.GetAll();
                int targetStationId = requestedStationId
                    ?? stations.FirstOrDefault(s => s.IsAvailable())?.StationId
                    ?? stations.FirstOrDefault()?.StationId
                    ?? 1;

                CurrentStation = _dbStation.GetById(targetStationId) ?? new ComputerStation
                {
                    StationId = targetStationId,
                    StationName = $"PC-{targetStationId:D2}",
                    ZoneId = 1,
                    IpAddress = $"192.168.1.{100 + targetStationId}",
                    Status = "Available"
                };

                if (isAdminOrStaff)
                {
                    // Admin / Staff preview mode on empty station: simulated preview session without polluting DB
                    IsPreviewMode = true;
                    activeSession = new GamingSession
                    {
                        SessionId = 0,
                        StationId = CurrentStation.StationId,
                        UserId = 2,
                        StartTime = DateTime.Now.AddMinutes(-30),
                        TariffType = "MemberTier",
                        Status = "Active",
                        PlayCost = 4000m,
                        ServiceCost = 0m,
                        TotalCost = 4000m
                    };
                }
                else
                {
                    // Member workstation login: auto-open station
                    try
                    {
                        activeSession = _dbBilling.OpenStationSession(
                            CurrentStation.StationId,
                            CurrentUserSession.UserId,
                            "MemberTier",
                            DateTime.Now,
                            1000m);
                    }
                    catch
                    {
                        // Fallback in-memory active session for disconnected tests
                        activeSession = new GamingSession
                        {
                            SessionId = 999,
                            StationId = CurrentStation.StationId,
                            UserId = CurrentUserSession.UserId,
                            StartTime = DateTime.Now,
                            TariffType = "MemberTier",
                            Status = "Active",
                            PlayCost = 0m,
                            ServiceCost = 0m,
                            TotalCost = 0m
                        };
                    }
                }
            }

            CurrentSession = activeSession;

            // 4. Resolve Active User Context
            if (CurrentUserSession.IsMember)
            {
                MemberUserId = CurrentUserSession.UserId;
                MemberUsername = CurrentUserSession.Username;
                MemberFullName = CurrentUserSession.FullName;
                MemberTier = CurrentUserSession.MemberTier;
                CurrentBalance = CurrentUserSession.Balance;
            }
            else
            {
                // Admin / Staff previewing station
                AppUser? activeUser = null;
                if (activeSession != null && activeSession.UserId.HasValue)
                {
                    try
                    {
                        activeUser = _dbUser.GetById(activeSession.UserId.Value);
                    }
                    catch { }
                }

                if (activeUser != null)
                {
                    MemberUserId = activeUser.UserId;
                    MemberUsername = activeUser.Username;
                    MemberFullName = string.IsNullOrWhiteSpace(activeUser.FullName) ? activeUser.Username : activeUser.FullName;
                    MemberTier = string.IsNullOrWhiteSpace(activeUser.MemberTier) ? "Standard" : activeUser.MemberTier;
                    CurrentBalance = activeUser.Balance;
                }
                else
                {
                    // Admin preview without existing active member
                    MemberUserId = 2;
                    MemberUsername = "gamer01";
                    MemberFullName = "Nguyễn Văn A (Mô phỏng)";
                    MemberTier = "Gold";
                    CurrentBalance = 150000m;
                }
            }

            // 5. Resolve Zone & Pricing
            var zones = _dbStation.GetAllZones();
            CurrentZone = zones.FirstOrDefault(z => z.ZoneId == (CurrentStation?.ZoneId ?? 1));
            BasePricePerHour = CurrentZone?.BasePricePerHour ?? 10000m;

            // Update Station Info Card
            string stName = CurrentStation?.StationName ?? "PC-01";
            string zName = CurrentZone?.ZoneName ?? "Khu Tiêu Chuẩn";
            string ip = string.IsNullOrWhiteSpace(CurrentStation?.IpAddress) ? "192.168.1.101" : CurrentStation.IpAddress;
            string previewTag = IsPreviewMode ? " [Xem thử / Preview]" : "";
            lblStationInfo.Text = $"🖥️ Máy: {stName}{previewTag}\nKhu: {zName} ({BasePricePerHour:N0}đ/h)\nIP: {ip}";

            // Update Account Info
            lblAccountInfo.Text = $"👤 {MemberFullName} ({MemberUsername})";
            string tier = MemberTier;
            decimal discount = MemberTierTariff.GetDefaultDiscountRate(tier);
            lblMemberTier.Text = tier switch
            {
                "Gold" => $"🥇 Vàng (-{discount * 100m:0.##}%)",
                "Silver" => $"🥈 Bạc (-{discount * 100m:0.##}%)",
                _ => "🥉 Tiêu chuẩn (0%)"
            };
            lblMemberTier.ForeColor = tier switch
            {
                "Gold" => Color.FromArgb(255, 215, 0),
                "Silver" => Color.FromArgb(0, 210, 255),
                _ => Color.FromArgb(200, 215, 230)
            };
            lblWalletBalance.Text = $"💵 Ví: {CurrentBalance:N0} VNĐ";
        }

        private void InitializeGameCatalog()
        {
            AvailableGames.Clear();
            AvailableGames.AddRange(new[]
            {
                new GameItem
                {
                    GameId = "lol",
                    Name = "Liên Minh Huyền Thoại",
                    Category = "Esports / MOBA",
                    Publisher = "Riot Games / VNG",
                    Version = "v14.22",
                    Description = "Đấu trường công lý MOBA 5v5 đỉnh cao, giao tranh đồng đội rực lửa.",
                    IconEmoji = "⚔️",
                    Aliases = "lol lmht league of legends"
                },
                new GameItem
                {
                    GameId = "dota2",
                    Name = "Dota 2",
                    Category = "Esports / MOBA",
                    Publisher = "Valve Corporation",
                    Version = "v7.37",
                    Description = "Chiến thuật thời gian thực MOBA chuyên nghiệp với hơn 120 vị tướng.",
                    IconEmoji = "🛡️",
                    Aliases = "dota dota2 defense of the ancients"
                },
                new GameItem
                {
                    GameId = "cs2",
                    Name = "Counter-Strike 2",
                    Category = "Bắn súng FPS",
                    Publisher = "Valve Corporation",
                    Version = "Source 2",
                    Description = "Tựa game bắn súng chiến thuật góc nhìn thứ nhất số 1 thế giới.",
                    IconEmoji = "🎯",
                    Aliases = "cs csgo cs2 counter strike shooter"
                },
                new GameItem
                {
                    GameId = "valorant",
                    Name = "Valorant",
                    Category = "Bắn súng FPS",
                    Publisher = "Riot Games / VNG",
                    Version = "Episode 9",
                    Description = "Bắn súng đặc vụ 5v5 phối hợp kỹ năng và kỹ thuật ngắm bắn tinh chuẩn.",
                    IconEmoji = "💥",
                    Aliases = "valo val valorant riot shooter"
                },
                new GameItem
                {
                    GameId = "pubg",
                    Name = "PUBG: Battlegrounds",
                    Category = "Sinh tồn / Nhập vai",
                    Publisher = "KRAFTON",
                    Version = "Season 32",
                    Description = "Đấu trường sinh tồn 100 người nghẹt thở, nhảy dù lượn nhặt đồ súng đạn.",
                    IconEmoji = "🪂",
                    Aliases = "pubg battlegrounds nhay du ban sung sinh ton"
                },
                new GameItem
                {
                    GameId = "gtav",
                    Name = "Grand Theft Auto V",
                    Category = "Sinh tồn / Nhập vai",
                    Publisher = "Rockstar Games",
                    Version = "v1.68 Online",
                    Description = "Thế giới mở Los Santos tự do, cướp ngân hàng và đua xe cùng chiến hữu.",
                    IconEmoji = "🚗",
                    Aliases = "gta gtav gta5 grand theft auto cuop duong pho"
                },
                new GameItem
                {
                    GameId = "wukong",
                    Name = "Black Myth: Wukong",
                    Category = "Sinh tồn / Nhập vai",
                    Publisher = "Game Science",
                    Version = "v1.0.9",
                    Description = "Siêu phẩm hành động nhập vai Tôn Ngộ Không đồ họa Unreal Engine 5 đỉnh cao.",
                    IconEmoji = "🐒",
                    Aliases = "wukong ngo khong black myth hac ngo khong ton ngo khong"
                },
                new GameItem
                {
                    GameId = "fconline",
                    Name = "EA SPORTS FC Online",
                    Category = "Thể thao",
                    Publisher = "Nexon / Garena",
                    Version = "Update 2024",
                    Description = "Bóng đá điện tử đỉnh cao với hàng ngàn cầu thủ và giải đấu bản quyền.",
                    IconEmoji = "⚽",
                    Aliases = "fco fconline fo4 fifa ea fc bong da"
                }
            });

            RenderGameCards();
        }

        public List<GameItem> GetFilteredGames(string category = "Tất cả", string searchQuery = "")
        {
            var query = AvailableGames.AsEnumerable();

            if (!string.Equals(category, "Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(g => g.Category.Contains(category, StringComparison.OrdinalIgnoreCase) ||
                                         category.Contains(g.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string kw = searchQuery.Trim();
                query = query.Where(g => g.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                                         g.Description.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                                         g.Publisher.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                                         g.GameId.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                                         (!string.IsNullOrEmpty(g.Aliases) && g.Aliases.Contains(kw, StringComparison.OrdinalIgnoreCase)));
            }

            return query.ToList();
        }

        private void RenderGameCards()
        {
            flpGameCards.SuspendLayout();
            flpGameCards.Controls.Clear();

            var filtered = GetFilteredGames(_currentGameCategory, txtSearchGame.Text);
            foreach (var game in filtered)
            {
                var card = CreateGameCardControl(game);
                flpGameCards.Controls.Add(card);
            }

            flpGameCards.ResumeLayout(true);
        }

        private Panel CreateGameCardControl(GameItem game)
        {
            var pnl = new Panel
            {
                Size = new Size(295, 230),
                BackColor = Color.FromArgb(24, 32, 47),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.None
            };

            var lblHeader = new Label
            {
                Text = $"{game.IconEmoji} {game.Name}",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10),
                Size = new Size(270, 26)
            };

            var lblCat = new Label
            {
                Text = $"Thể loại: {game.Category}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 255),
                Location = new Point(12, 38),
                Size = new Size(270, 20)
            };

            var lblPub = new Label
            {
                Text = $"{game.Publisher} | {game.Version}",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(160, 180, 200),
                Location = new Point(12, 60),
                Size = new Size(270, 18)
            };

            var lblDesc = new Label
            {
                Text = game.Description,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(200, 215, 230),
                Location = new Point(12, 85),
                Size = new Size(270, 75)
            };

            var btnPlay = new Button
            {
                Text = (RunningGame?.GameId == game.GameId) ? "🎮 ĐANG CHƠI" : "▶ CHƠI NGAY",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = (RunningGame?.GameId == game.GameId)
                    ? Color.FromArgb(16, 185, 129)
                    : Color.FromArgb(0, 180, 136),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(12, 172),
                Size = new Size(270, 42),
                Cursor = Cursors.Hand
            };

            btnPlay.Click += (s, e) =>
            {
                LaunchGame(game.GameId);
            };

            pnl.Controls.Add(lblHeader);
            pnl.Controls.Add(lblCat);
            pnl.Controls.Add(lblPub);
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(btnPlay);

            return pnl;
        }

        public bool LaunchGame(string gameId)
        {
            var game = AvailableGames.FirstOrDefault(g => g.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase));
            if (game == null) return false;

            if (RunningGame != null && !RunningGame.GameId.Equals(game.GameId, StringComparison.OrdinalIgnoreCase))
            {
                RunningGame.Status = "Sẵn sàng";
            }

            RunningGame = game;
            game.Status = "Đang chạy";
            game.LastPlayed = DateTime.Now;

            pnlActiveGame.Visible = true;
            lblActiveGameTitle.Text = $"🎮 Đang chạy: {game.Name} ({game.Version}) | Khởi chạy máy trạm thành công!";
            lblLauncherStatus.Text = $"🚀 [Thành công] Đã khởi chạy tựa game {game.Name}! Tối ưu hóa cấu hình đồ họa 144Hz & đồng bộ tài nguyên máy trạm.";

            RenderGameCards();
            return true;
        }

        public bool StopGame()
        {
            if (RunningGame == null) return false;

            string stoppedGameName = RunningGame.Name;
            RunningGame.Status = "Sẵn sàng";
            RunningGame = null;

            pnlActiveGame.Visible = false;
            lblLauncherStatus.Text = $"⏹️ Đã đóng tựa game {stoppedGameName}. Sẵn sàng cho lượt chơi tiếp theo.";

            RenderGameCards();
            return true;
        }

        private void SetupEventHandlers()
        {
            // Game filter buttons
            btnCatAll.Click += (s, e) => SwitchGameCategory("Tất cả", btnCatAll);
            btnCatMoba.Click += (s, e) => SwitchGameCategory("Esports / MOBA", btnCatMoba);
            btnCatFps.Click += (s, e) => SwitchGameCategory("Bắn súng FPS", btnCatFps);
            btnCatRpg.Click += (s, e) => SwitchGameCategory("Sinh tồn / Nhập vai", btnCatRpg);
            btnCatSports.Click += (s, e) => SwitchGameCategory("Thể thao", btnCatSports);
            txtSearchGame.TextChanged += (s, e) => RenderGameCards();
            btnStopGame.Click += (s, e) => StopGame();

            // Food filter buttons
            btnFoodAll.Click += (s, e) => SwitchFoodCategory("Tất cả", btnFoodAll);
            btnFoodDrink.Click += (s, e) => SwitchFoodCategory("Đồ uống", btnFoodDrink);
            btnFoodMeal.Click += (s, e) => SwitchFoodCategory("Món ăn", btnFoodMeal);
            btnFoodSnack.Click += (s, e) => SwitchFoodCategory("Ăn vặt", btnFoodSnack);
            txtSearchFood.TextChanged += (s, e) => FilterFoodGrid();

            dgvFoodItems.SelectionChanged += DgvFoodItems_SelectionChanged;
            nudFoodQty.ValueChanged += (s, e) => UpdateOrderTotalCalculation();
            btnOrderFood.Click += BtnOrderFood_Click;

            // Utilities
            btnChangePassword.Click += (s, e) => OpenChangePasswordDialog();
            btnLockScreen.Click += (s, e) => LockStation();
            btnUnlock.Click += (s, e) => TryUnlockStationFromUi();
            txtUnlockPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    TryUnlockStationFromUi();
                }
            };
            btnCallStaff.Click += (s, e) => PromptCallStaff();
            btnLogoutSession.Click += (s, e) => ConfirmEndSessionAndLogout();
        }

        private void SwitchGameCategory(string category, Button activeBtn)
        {
            _currentGameCategory = category;
            foreach (var b in new[] { btnCatAll, btnCatMoba, btnCatFps, btnCatRpg, btnCatSports })
            {
                b.BackColor = Color.FromArgb(30, 38, 52);
            }
            activeBtn.BackColor = Color.FromArgb(0, 180, 136);
            RenderGameCards();
        }

        private void SwitchFoodCategory(string category, Button activeBtn)
        {
            _currentFoodCategory = category;
            foreach (var b in new[] { btnFoodAll, btnFoodDrink, btnFoodMeal, btnFoodSnack })
            {
                b.BackColor = Color.FromArgb(30, 38, 52);
            }
            activeBtn.BackColor = Color.FromArgb(0, 180, 136);
            FilterFoodGrid();
        }

        public void LoadFoodCatalog()
        {
            try
            {
                var allItems = _dbBilling.GetAllServiceItems(activeOnly: true);
                AvailableServiceItems = allItems.Where(i => i.IsActive).ToList();
            }
            catch
            {
                // Fallback in-memory items
                AvailableServiceItems = new List<ServiceItem>
                {
                    new() { ServiceId = 1, ServiceName = "Sting dâu tây lon 330ml", Category = "Đồ uống", UnitPrice = 15000m, StockQuantity = 50, IsActive = true },
                    new() { ServiceId = 2, ServiceName = "Red Bull Thái lon 250ml", Category = "Đồ uống", UnitPrice = 20000m, StockQuantity = 40, IsActive = true },
                    new() { ServiceId = 3, ServiceName = "Mì tôm xào trứng xúc xích", Category = "Món ăn", UnitPrice = 30000m, StockQuantity = 25, IsActive = true },
                    new() { ServiceId = 4, ServiceName = "Cơm đùi gà chiên mắm", Category = "Món ăn", UnitPrice = 45000m, StockQuantity = 20, IsActive = true },
                    new() { ServiceId = 5, ServiceName = "Khoai tây chiên lắc phô mai", Category = "Ăn vặt", UnitPrice = 25000m, StockQuantity = 30, IsActive = true }
                };
            }

            SetupFoodGridColumns();
            FilterFoodGrid();
        }

        private void SetupFoodGridColumns()
        {
            dgvFoodItems.Columns.Clear();
            dgvFoodItems.AutoGenerateColumns = false;

            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ServiceId",
                HeaderText = "Mã",
                DataPropertyName = "ServiceId",
                FillWeight = 30
            });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ServiceName",
                HeaderText = "Tên món / đồ uống",
                DataPropertyName = "ServiceName",
                FillWeight = 140
            });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                HeaderText = "Thể loại",
                DataPropertyName = "Category",
                FillWeight = 60
            });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitPrice",
                HeaderText = "Đơn giá (VNĐ)",
                DataPropertyName = "UnitPrice",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" },
                FillWeight = 60
            });
            dgvFoodItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockQuantity",
                HeaderText = "Tồn kho",
                DataPropertyName = "StockQuantity",
                FillWeight = 40
            });
        }

        private void FilterFoodGrid()
        {
            var query = AvailableServiceItems.AsEnumerable();

            if (!string.Equals(_currentFoodCategory, "Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => i.Category.Contains(_currentFoodCategory, StringComparison.OrdinalIgnoreCase));
            }

            string search = txtSearchFood.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.ServiceName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         i.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var list = query.ToList();
            dgvFoodItems.DataSource = null;
            dgvFoodItems.DataSource = list;

            if (list.Count > 0)
            {
                dgvFoodItems.Rows[0].Selected = true;
                SelectFoodItem(list[0]);
            }
            else
            {
                SelectFoodItem(null);
            }
        }

        private void DgvFoodItems_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvFoodItems.CurrentRow?.DataBoundItem is ServiceItem item)
            {
                SelectFoodItem(item);
            }
        }

        private void SelectFoodItem(ServiceItem? item)
        {
            _selectedFoodItem = item;
            if (item == null)
            {
                lblSelectedFoodName.Text = "Chưa chọn món nào";
                lblSelectedFoodPrice.Text = "Đơn giá: 0 VNĐ | Còn kho: 0";
                nudFoodQty.Maximum = 1;
                nudFoodQty.Value = 1;
                nudFoodQty.Enabled = false;
                btnOrderFood.Enabled = false;
                lblOrderTotal.Text = "TỔNG TIỀN TÍNH: 0 VNĐ";
                return;
            }

            lblSelectedFoodName.Text = item.ServiceName;
            lblSelectedFoodPrice.Text = $"Đơn giá: {item.UnitPrice:N0} VNĐ | Danh mục: {item.Category} | Còn kho: {item.StockQuantity}";
            nudFoodQty.Enabled = item.StockQuantity > 0;
            nudFoodQty.Maximum = Math.Max(1, item.StockQuantity);
            nudFoodQty.Value = 1;
            btnOrderFood.Enabled = item.StockQuantity > 0;
            UpdateOrderTotalCalculation();
        }

        private void UpdateOrderTotalCalculation()
        {
            if (_selectedFoodItem == null)
            {
                lblOrderTotal.Text = "TỔNG TIỀN TÍNH: 0 VNĐ";
                return;
            }

            decimal total = _selectedFoodItem.UnitPrice * nudFoodQty.Value;
            lblOrderTotal.Text = $"TỔNG TIỀN TÍNH: {total:N0} VNĐ";
        }

        private void BtnOrderFood_Click(object? sender, EventArgs e)
        {
            if (_selectedFoodItem == null)
            {
                ShowNotification("Vui lòng chọn món F&B cần gọi từ danh mục!", "Chưa chọn món", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int qty = (int)nudFoodQty.Value;
            string? note = string.IsNullOrWhiteSpace(txtFoodNote.Text) ? null : txtFoodNote.Text.Trim();

            if (TryOrderFood(_selectedFoodItem.ServiceId, qty, note, out string message))
            {
                ShowNotification(message, "Gọi món F&B thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtFoodNote.Clear();
            }
            else
            {
                ShowNotification(message, "Không thể gọi món", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public decimal CalculateCurrentRemainingBalance()
        {
            if (CurrentSession == null) return CurrentBalance;

            DateTime now = DateTime.Now;
            DateTime start = CurrentSession.StartTime;
            if (now < start) now = start;

            TariffCalculatorBase calculator = DBBilling.CreateTariffCalculator(CurrentSession.TariffType, MemberTier);
            decimal estimatedPlayCost = calculator.CalculateCost(start, now, BasePricePerHour);
            decimal totalCost = estimatedPlayCost + CurrentSession.ServiceCost;

            return Math.Max(0m, CurrentBalance - totalCost);
        }

        public bool TryOrderFood(int serviceId, int quantity, string? note, out string message)
        {
            if (CurrentSession == null)
            {
                message = "Phiên chơi chưa được khởi tạo.";
                return false;
            }

            var item = AvailableServiceItems.FirstOrDefault(i => i.ServiceId == serviceId);
            if (item == null)
            {
                message = "Món F&B không tồn tại.";
                return false;
            }

            if (quantity <= 0)
            {
                message = "Số lượng gọi phải lớn hơn 0.";
                return false;
            }

            if (quantity > item.StockQuantity)
            {
                message = $"Món '{item.ServiceName}' chỉ còn {item.StockQuantity} trong kho.";
                return false;
            }

            decimal lineTotal = item.UnitPrice * quantity;
            decimal remainingBal = CalculateCurrentRemainingBalance();
            if (CurrentBalance > 0m && lineTotal > remainingBal)
            {
                message = $"Số dư ví không đủ để gọi món (cần thêm {(lineTotal - remainingBal):N0} VNĐ). Vui lòng nạp thêm tiền tại quầy thu ngân!";
                return false;
            }

            try
            {
                bool success = false;
                if (!IsPreviewMode)
                {
                    success = _dbBilling.OrderServiceForSession(CurrentSession.SessionId, serviceId, quantity, note);
                }
                else
                {
                    success = true;
                }

                if (success)
                {
                    var updatedSession = _dbBilling.GetActiveSession(CurrentStation?.StationId ?? CurrentSession.StationId);
                    if (updatedSession != null)
                    {
                        CurrentSession.ServiceCost = updatedSession.ServiceCost;
                        CurrentSession.TotalCost = updatedSession.TotalCost;
                    }
                    else
                    {
                        CurrentSession.ServiceCost += lineTotal;
                        CurrentSession.TotalCost += lineTotal;
                    }

                    item.StockQuantity = Math.Max(0, item.StockQuantity - quantity);

                    _inMemorySessionOrders.Add(new SessionServiceDetail
                    {
                        SessionServiceId = _inMemorySessionOrders.Count + 1,
                        SessionId = CurrentSession.SessionId,
                        ServiceId = item.ServiceId,
                        ServiceName = item.ServiceName,
                        Category = item.Category,
                        Quantity = quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = lineTotal,
                        OrderedAt = DateTime.Now,
                        Note = note
                    });

                    // Reload Session Orders & Refresh UI
                    LoadSessionOrders();
                    UpdateRealtimeHud();
                    FilterFoodGrid();
                    SelectFoodItem(item);

                    string noteText = string.IsNullOrEmpty(note) ? string.Empty : $"\nGhi chú: {note}";
                    message = $"Đã gọi thành công {quantity}x {item.ServiceName}!\nTổng tiền F&B: {lineTotal:N0} VNĐ.{noteText}\nMón đang được chuẩn bị tại quầy!";
                    return true;
                }

                message = "Hệ thống quầy không chấp nhận đơn gọi món này.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Lỗi gọi món: {ex.Message}";
                return false;
            }
        }

        public void LoadSessionOrders()
        {
            if (CurrentSession == null) return;

            try
            {
                SessionServices = _dbBilling.GetSessionServices(CurrentSession.SessionId);
            }
            catch
            {
                SessionServices = new List<SessionServiceDetail>();
            }

            if ((SessionServices == null || SessionServices.Count == 0) && _inMemorySessionOrders.Count > 0)
            {
                SessionServices = new List<SessionServiceDetail>(_inMemorySessionOrders);
            }

            dgvSessionOrders.DataSource = null;
            dgvSessionOrders.AutoGenerateColumns = false;
            dgvSessionOrders.Columns.Clear();

            dgvSessionOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ServiceName",
                HeaderText = "Tên món",
                DataPropertyName = "ServiceName",
                FillWeight = 110
            });
            dgvSessionOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "SL",
                DataPropertyName = "Quantity",
                FillWeight = 30
            });
            dgvSessionOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalPrice",
                HeaderText = "Tổng (VNĐ)",
                DataPropertyName = "TotalPrice",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" },
                FillWeight = 60
            });
            dgvSessionOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderedAt",
                HeaderText = "Thời gian",
                DataPropertyName = "OrderedAt",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm" },
                FillWeight = 50
            });
            dgvSessionOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "Ghi chú",
                DataPropertyName = "Notes",
                FillWeight = 80
            });

            dgvSessionOrders.DataSource = SessionServices;
        }

        public void UpdateRealtimeHud()
        {
            if (CurrentSession == null) return;

            DateTime now = DateTime.Now;
            DateTime start = CurrentSession.StartTime;
            if (now < start) now = start;

            TimeSpan elapsed = now - start;
            lblPlayedTime.Text = $"⏱️ Đã chơi: {(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

            // Calculate Play Cost
            string memberTier = MemberTier;
            TariffCalculatorBase calculator = DBBilling.CreateTariffCalculator(CurrentSession.TariffType, memberTier);
            decimal estimatedPlayCost = calculator.CalculateCost(start, now, BasePricePerHour);

            lblPlayCost.Text = $"💰 Tiền giờ: {estimatedPlayCost:N0} VNĐ";
            lblServiceCost.Text = $"🥤 Dịch vụ F&B: {CurrentSession.ServiceCost:N0} VNĐ";

            decimal totalCost = estimatedPlayCost + CurrentSession.ServiceCost;
            decimal remainingBalance = Math.Max(0m, CurrentBalance - totalCost);
            lblRemainingBalance.Text = $"💵 Số dư ví tạm tính: {remainingBalance:N0} VNĐ";
            lblWalletBalance.Text = $"💵 Ví: {CurrentBalance:N0} VNĐ";

            // Estimate Remaining Time
            decimal discount = MemberTierTariff.GetDefaultDiscountRate(memberTier);
            decimal hourlyRate = BasePricePerHour * (1m - discount);
            if (hourlyRate <= 0m) hourlyRate = 8000m;

            if (remainingBalance > 0m)
            {
                _isOutOfTimeLocked = false;
                double remainingHours = (double)(remainingBalance / hourlyRate);
                TimeSpan remainingTime = TimeSpan.FromHours(remainingHours);
                lblRemainingTime.Text = $"⏳ Còn lại: {(int)remainingTime.TotalHours:D2}:{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
                lblRemainingTime.ForeColor = Color.FromArgb(0, 220, 255);
            }
            else
            {
                lblRemainingTime.Text = "⏳ Còn lại: 00:00:00 (Hết giờ / Nạp ví)";
                lblRemainingTime.ForeColor = Color.FromArgb(255, 99, 99);

                // Enforce out-of-time protection
                if (IsGameRunning)
                {
                    StopGame();
                }

                if (!IsScreenLocked && !_isOutOfTimeLocked)
                {
                    _isOutOfTimeLocked = true;
                    LockStation();
                    lblLockSubtitle.Text = $"⚠️ HẾT THỜI GIAN CHƠI!\nKhách hàng: {MemberFullName} ({MemberUsername}) | Máy: {CurrentStation?.StationName ?? "PC-01"}\nSố dư tài khoản đã hết (0 VNĐ). Vui lòng nạp thêm tiền tại quầy thu ngân để tiếp tục sử dụng!";
                    lblUnlockError.Text = "Hết thời gian chơi! Vui lòng nạp thêm tiền tại quầy thu ngân.";
                    lblUnlockError.Visible = true;
                }
            }
        }

        public void LockStation()
        {
            IsScreenLocked = true;
            pnlLockOverlay.Visible = true;
            pnlLockOverlay.BringToFront();
            lblLockSubtitle.Text = $"Khách hàng: {MemberFullName} ({MemberUsername}) | Máy: {CurrentStation?.StationName ?? "PC-01"}\nPhiên chơi vẫn đang được tính giờ và dữ liệu tài khoản được bảo vệ an toàn.";
            lblUnlockError.Visible = false;
            txtUnlockPassword.Clear();
            txtUnlockPassword.Focus();
        }

        public bool TryUnlockStation(string password, out string message)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Vui lòng nhập mật khẩu mở khóa.";
                return false;
            }

            try
            {
                // Allow member password, admin master unlock, or current user session
                bool valid = _dbUser.TryAuthenticate(MemberUsername, password, out var authUser, out string authError)
                             || _dbUser.TryAuthenticate("admin", password, out authUser, out _)
                             || _dbUser.TryAuthenticate(CurrentUserSession.Username, password, out authUser, out _);

                if (valid)
                {
                    decimal remaining = CalculateCurrentRemainingBalance();
                    if (_isOutOfTimeLocked && remaining <= 0m && authUser?.Role != "Admin" && authUser?.Role != "Staff")
                    {
                        var freshUser = _dbUser.GetById(MemberUserId);
                        if (freshUser != null && freshUser.Balance > CurrentBalance)
                        {
                            CurrentBalance = freshUser.Balance;
                            _isOutOfTimeLocked = false;
                        }
                        else
                        {
                            message = "Số dư ví vẫn là 0 VNĐ. Vui lòng nạp thêm tiền tại quầy thu ngân trước khi mở khóa máy!";
                            return false;
                        }
                    }

                    IsScreenLocked = false;
                    _isOutOfTimeLocked = false;
                    pnlLockOverlay.Visible = false;
                    txtUnlockPassword.Clear();
                    lblUnlockError.Visible = false;
                    message = "Mở khóa máy thành công!";
                    UpdateRealtimeHud();
                    return true;
                }

                message = string.IsNullOrWhiteSpace(authError) ? "Mật khẩu không chính xác." : authError;
                return false;
            }
            catch (Exception ex)
            {
                message = $"Lỗi xác thực: {ex.Message}";
                return false;
            }
        }

        private void TryUnlockStationFromUi()
        {
            if (TryUnlockStation(txtUnlockPassword.Text, out string message))
            {
                lblUnlockError.Visible = false;
            }
            else
            {
                lblUnlockError.Text = message;
                lblUnlockError.Visible = true;
                txtUnlockPassword.SelectAll();
                txtUnlockPassword.Focus();
            }
        }

        public void OpenChangePasswordDialog()
        {
            using var dlg = new FormChangePasswordDialog(_dbUser, MemberUsername);
            dlg.ShowDialog(this);
        }

        public void CallStaff(string reason)
        {
            LastStaffCallReason = reason;
            string stName = CurrentStation?.StationName ?? "PC-01";

            StaffCallQueue.Enqueue(new StaffCallNotification
            {
                StationId = CurrentStation?.StationId ?? 1,
                StationName = stName,
                MemberUsername = MemberUsername,
                MemberFullName = MemberFullName,
                Reason = reason,
                Timestamp = DateTime.Now
            });

            ShowNotification(
                $"🔔 Đã gửi yêu cầu hỗ trợ tới quầy thu ngân cho máy {stName}!\nNội dung: {reason}\nNhân viên quầy sẽ hỗ trợ bạn trong giây lát.",
                "Đã gọi nhân viên quầy",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void PromptCallStaff()
        {
            using var dlg = new Form();
            dlg.Text = "🔔 Gọi nhân viên / Hỗ trợ quầy";
            dlg.Size = new Size(420, 260);
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.BackColor = Color.FromArgb(20, 26, 38);
            dlg.ForeColor = Color.White;

            var lblPrompt = new Label
            {
                Text = "Chọn nội dung yêu cầu hỗ trợ gửi tới quầy thu ngân:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                Size = new Size(360, 25)
            };

            var cboReason = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 48),
                Size = new Size(360, 30),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White
            };
            cboReason.Items.AddRange(new object[]
            {
                "💵 Nạp thêm tiền ví hội viên tại máy",
                "🥤 Hỗ trợ kiểm tra đơn gọi món F&B",
                "🎧 Lỗi phần cứng (Chuột / Phím / Tai nghe / Màn hình)",
                "🖥️ Lỗi game / Cần cập nhật bản vá game",
                "❓ Hỗ trợ dịch vụ khác"
            });
            cboReason.SelectedIndex = 0;

            var txtCustom = new TextBox
            {
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(20, 90),
                Size = new Size(360, 26),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White,
                PlaceholderText = "Ghi chú thêm (không bắt buộc)..."
            };

            var btnSend = new Button
            {
                Text = "🔔 GỬI YÊU CẦU",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(217, 119, 6),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(190, 150),
                Size = new Size(190, 42),
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "HỦY",
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(50, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(70, 150),
                Size = new Size(100, 42),
                DialogResult = DialogResult.Cancel
            };

            dlg.Controls.Add(lblPrompt);
            dlg.Controls.Add(cboReason);
            dlg.Controls.Add(txtCustom);
            dlg.Controls.Add(btnSend);
            dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnSend;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string baseReason = cboReason.SelectedItem?.ToString() ?? "Hỗ trợ khách hàng tại máy";
                string extra = txtCustom.Text.Trim();
                string finalReason = string.IsNullOrEmpty(extra) ? baseReason : $"{baseReason} - {extra}";
                CallStaff(finalReason);
            }
        }

        public bool EndSessionAndLogout()
        {
            if (CurrentStation == null)
            {
                IsLogoutRequested = true;
                Close();
                return true;
            }

            if (IsPreviewMode)
            {
                ShowNotification("Đã đóng màn hình xem thử giao diện máy trạm.", "Kết thúc xem thử", MessageBoxButtons.OK, MessageBoxIcon.Information);
                IsLogoutRequested = true;
                Close();
                return true;
            }

            DateTime endTime = DateTime.Now;
            DateTime start = CurrentSession?.StartTime ?? endTime.AddHours(-1);
            TimeSpan elapsed = endTime - start;
            string memberTier = MemberTier;
            var calc = DBBilling.CreateTariffCalculator(CurrentSession?.TariffType ?? "MemberTier", memberTier);
            decimal playCost = calc.CalculateCost(start, endTime, BasePricePerHour);
            decimal serviceCost = CurrentSession?.ServiceCost ?? 0m;
            decimal total = playCost + serviceCost;

            try
            {
                var result = _dbBilling.CloseStationSession(
                    CurrentStation.StationId,
                    BasePricePerHour,
                    endTime,
                    allowCashSupplement: false,
                    fallbackUserId: MemberUserId,
                    fallbackTariffType: CurrentSession?.TariffType,
                    fallbackStartTime: start,
                    processedByUserId: MemberUserId);

                string msg =
                    $"🧾 HÓA ĐƠN KẾT THÚC PHIÊN CHƠI - MÁY {result.StationName}\n" +
                    $"────────────────────────────────────────\n" +
                    $"• Khách hàng: {MemberFullName} ({MemberUsername})\n" +
                    $"• Thời gian chơi: {result.StartTime:HH:mm:ss} -> {result.EndTime:HH:mm:ss} ({(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2})\n" +
                    $"• Tiền giờ: {result.PlayCost:N0} VNĐ\n" +
                    $"• Tiền F&B ăn uống: {result.ServiceCost:N0} VNĐ\n" +
                    $"• TỔNG KHẤU TRỪ VÍ: {result.TotalCost:N0} VNĐ\n" +
                    $"• SỐ DƯ VÍ CÒN LẠI: {(result.RemainingBalance.HasValue ? result.RemainingBalance.Value.ToString("N0") : "0")} VNĐ\n" +
                    $"────────────────────────────────────────\n" +
                    $"Cảm ơn bạn đã trải nghiệm tại Cyber Gaming Center!";

                ShowNotification(msg, "Kết thúc phiên chơi thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowNotification($"Đóng phiên chơi với số dư ước tính: {ex.Message}", "Kết thúc phiên", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            IsLogoutRequested = true;
            Close();
            return true;
        }

        private void ConfirmEndSessionAndLogout()
        {
            var confirm = ShowNotification(
                $"Bạn có chắc chắn muốn kết thúc phiên chơi và đăng xuất khỏi máy {CurrentStation?.StationName ?? "PC"}?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                EndSessionAndLogout();
            }
        }
    }
}
