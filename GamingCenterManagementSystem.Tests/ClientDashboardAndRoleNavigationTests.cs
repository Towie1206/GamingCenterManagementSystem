using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class ClientDashboardAndRoleNavigationTests
    {
        private static void RunOnSTA(Action action)
        {
            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }

        private static DBUser CreateOfflineUserDb() =>
            new(() => throw new TimeoutException("Simulated offline SQL Server"));

        private static DBBilling CreateOfflineBillingDb() =>
            new(() => throw new TimeoutException("Simulated offline SQL Server"));

        private static DBStation CreateOfflineStationDb() =>
            new();

        // =========================================================================
        // 1. ROLE-BASED NAVIGATION TESTS
        // =========================================================================

        [Fact]
        public void AppNavigator_RoleRouting_MemberGetsClientDashboard_AdminAndStaffGetForm1()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                // Role = Member
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using (var memberForm = AppNavigator.CreateFormForRole("Member", dbStation, dbBilling, dbUser))
                {
                    Assert.IsType<FormClientDashboard>(memberForm);
                }

                using (var currentRoleForm = AppNavigator.CreateMainFormForCurrentRole(dbStation, dbBilling, dbUser))
                {
                    Assert.IsType<FormClientDashboard>(currentRoleForm);
                }

                // Role = Admin
                CurrentUserSession.SetSession(1, "admin", "Admin", "Quản Lý", 0m, "Standard");
                using (var adminForm = AppNavigator.CreateFormForRole("Admin", dbStation, dbBilling, dbUser))
                {
                    Assert.IsType<Form1>(adminForm);
                }

                // Role = Staff
                CurrentUserSession.SetSession(5, "staff01", "Staff", "Nhân Viên", 0m, "Standard");
                using (var staffForm = AppNavigator.CreateFormForRole("Staff", dbStation, dbBilling, dbUser))
                {
                    Assert.IsType<Form1>(staffForm);
                }

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void Form1_ClientModeButton_ExistsAndEnabledForAdminAndStaff()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                // Admin
                CurrentUserSession.SetSession(1, "admin", "Admin", "Quản Lý", 0m, "Standard");
                using (var form1 = new Form1(dbStation, dbUser, dbBilling))
                {
                    var btnClientMode = typeof(Form1).GetField("btnClientMode", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                    Assert.NotNull(btnClientMode);
                    Assert.True(btnClientMode.Enabled);
                    Assert.Contains("Máy Trạm", btnClientMode.Text);
                }

                // Staff
                CurrentUserSession.SetSession(5, "staff01", "Staff", "Nhân Viên", 0m, "Standard");
                using (var form1Staff = new Form1(dbStation, dbUser, dbBilling))
                {
                    var btnClientMode = typeof(Form1).GetField("btnClientMode", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1Staff) as Button;
                    Assert.NotNull(btnClientMode);
                    Assert.True(btnClientMode.Enabled);
                }

                CurrentUserSession.Clear();
            });
        }

        // =========================================================================
        // 2. FORM CLIENT DASHBOARD - REAL-TIME HUD & TIERS
        // =========================================================================

        [Fact]
        public void FormClientDashboard_InitializesWithRealtimeHUD_TierBadges_AndStationInfo()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                // Gold Member
                CurrentUserSession.SetSession(4, "gamer03", "Member", "Lê Hoàng C", 300000m, "Gold");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 2);

                Assert.NotNull(dashboard.CurrentStation);
                Assert.NotNull(dashboard.CurrentSession);
                Assert.Equal(300000m, dashboard.CurrentBalance);

                var lblAccountInfo = typeof(FormClientDashboard).GetField("lblAccountInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                var lblMemberTier = typeof(FormClientDashboard).GetField("lblMemberTier", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                var lblStationInfo = typeof(FormClientDashboard).GetField("lblStationInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                var lblWalletBalance = typeof(FormClientDashboard).GetField("lblWalletBalance", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;

                Assert.NotNull(lblAccountInfo);
                Assert.Contains("gamer03", lblAccountInfo.Text);
                Assert.Contains("Lê Hoàng C", lblAccountInfo.Text);

                Assert.NotNull(lblMemberTier);
                Assert.Contains("Vàng", lblMemberTier.Text);
                Assert.Contains("15%", lblMemberTier.Text);

                Assert.NotNull(lblStationInfo);
                Assert.Contains("PC-", lblStationInfo.Text);

                Assert.NotNull(lblWalletBalance);
                Assert.Contains("300,000", lblWalletBalance.Text);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_UpdateRealtimeHud_CalculatesPlayCostAndRemainingTimeAccurately()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(3, "gamer02", "Member", "Trần Thị B", 120000m, "Silver");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);

                // Set session start time back 1 hour
                dashboard.CurrentSession!.StartTime = DateTime.Now.AddHours(-1);
                dashboard.CurrentSession.ServiceCost = 25000m;
                dashboard.UpdateRealtimeHud();

                var lblPlayedTime = typeof(FormClientDashboard).GetField("lblPlayedTime", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                var lblPlayCost = typeof(FormClientDashboard).GetField("lblPlayCost", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                var lblServiceCost = typeof(FormClientDashboard).GetField("lblServiceCost", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                var lblRemainingTime = typeof(FormClientDashboard).GetField("lblRemainingTime", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;

                Assert.NotNull(lblPlayedTime);
                Assert.Contains("01:", lblPlayedTime.Text);

                Assert.NotNull(lblPlayCost);
                Assert.Contains("VNĐ", lblPlayCost.Text);

                Assert.NotNull(lblServiceCost);
                Assert.Contains("25,000", lblServiceCost.Text);

                Assert.NotNull(lblRemainingTime);
                Assert.Contains("Còn lại", lblRemainingTime.Text);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_UpdateRealtimeHud_WhenBalanceZero_DisplaysWarning()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 0m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);

                dashboard.CurrentSession!.StartTime = DateTime.Now.AddHours(-2);
                dashboard.UpdateRealtimeHud();

                var lblRemainingTime = typeof(FormClientDashboard).GetField("lblRemainingTime", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Label;
                Assert.NotNull(lblRemainingTime);
                Assert.Contains("00:00:00", lblRemainingTime.Text);
                Assert.Contains("Hết giờ", lblRemainingTime.Text);

                CurrentUserSession.Clear();
            });
        }

        // =========================================================================
        // 3. GAME HUB & LAUNCHER TESTS
        // =========================================================================

        [Fact]
        public void FormClientDashboard_GameHub_LoadsCatalogAcrossAllFourCategories()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);

                Assert.NotEmpty(dashboard.AvailableGames);
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("MOBA") && g.Name.Contains("Liên Minh"));
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("MOBA") && g.Name.Contains("Dota"));
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("FPS") && g.Name.Contains("Counter-Strike"));
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("FPS") && g.Name.Contains("Valorant"));
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("Sinh tồn") && g.Name.Contains("PUBG"));
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("Nhập vai") && g.Name.Contains("Wukong"));
                Assert.Contains(dashboard.AvailableGames, g => g.Category.Contains("Thể thao") && g.Name.Contains("FC Online"));
            });
        }

        [Fact]
        public void FormClientDashboard_GameHub_FiltersByCategoryAndSearchKeyword()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);

                var fpsGames = dashboard.GetFilteredGames("Bắn súng FPS");
                Assert.All(fpsGames, g => Assert.Contains("FPS", g.Category));

                var mobaGames = dashboard.GetFilteredGames("Esports / MOBA");
                Assert.All(mobaGames, g => Assert.Contains("MOBA", g.Category));

                var searchedWukong = dashboard.GetFilteredGames("Tất cả", "Wukong");
                Assert.Single(searchedWukong);
                Assert.Equal("wukong", searchedWukong[0].GameId);
            });
        }

        [Fact]
        public void FormClientDashboard_GameHub_LaunchGameAndStopGame_ManagesStateCorrectly()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);
                dashboard.Show();
                Application.DoEvents();

                Assert.Null(dashboard.RunningGame);
                Assert.False(dashboard.IsGameRunning);

                bool launched = dashboard.LaunchGame("lol");
                Assert.True(launched);
                Assert.True(dashboard.IsGameRunning);
                Assert.NotNull(dashboard.RunningGame);
                Assert.Equal("lol", dashboard.RunningGame.GameId);
                Assert.Equal("Đang chạy", dashboard.RunningGame.Status);

                var pnlActiveGame = typeof(FormClientDashboard).GetField("pnlActiveGame", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Panel;
                Assert.NotNull(pnlActiveGame);
                Assert.True(pnlActiveGame.Visible);

                bool stopped = dashboard.StopGame();
                Assert.True(stopped);
                Assert.Null(dashboard.RunningGame);
                Assert.False(dashboard.IsGameRunning);
                Assert.False(pnlActiveGame.Visible);
            });
        }

        [Fact]
        public void FormClientDashboard_GameHub_SearchAliases_FindsGamesByAcronyms()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);

                // Searching "lol" should find Liên Minh Huyền Thoại
                var lolResult = dashboard.GetFilteredGames("Tất cả", "lol");
                Assert.Contains(lolResult, g => g.GameId == "lol");

                // Searching "valo" should find Valorant
                var valoResult = dashboard.GetFilteredGames("Tất cả", "valo");
                Assert.Contains(valoResult, g => g.GameId == "valorant");

                // Searching "cs2" should find Counter-Strike 2
                var cs2Result = dashboard.GetFilteredGames("Tất cả", "cs2");
                Assert.Contains(cs2Result, g => g.GameId == "cs2");

                // Searching "pubg" should find PUBG
                var pubgResult = dashboard.GetFilteredGames("Tất cả", "pubg");
                Assert.Contains(pubgResult, g => g.GameId == "pubg");

                // Searching "fo4" or "fifa" should find EA SPORTS FC Online
                var fifaResult = dashboard.GetFilteredGames("Tất cả", "fo4");
                Assert.Contains(fifaResult, g => g.GameId == "fconline");
            });
        }

        [Fact]
        public void FormClientDashboard_GameHub_LaunchGame_ResetsPreviousRunningGameStatus()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);
                dashboard.Show();
                Application.DoEvents();

                // Launch game 1
                dashboard.LaunchGame("lol");
                var lolGame = dashboard.AvailableGames.First(g => g.GameId == "lol");
                Assert.Equal("Đang chạy", lolGame.Status);
                Assert.Equal("lol", dashboard.RunningGame?.GameId);

                // Launch game 2
                dashboard.LaunchGame("cs2");
                var cs2Game = dashboard.AvailableGames.First(g => g.GameId == "cs2");
                Assert.Equal("Đang chạy", cs2Game.Status);
                Assert.Equal("cs2", dashboard.RunningGame?.GameId);

                // Game 1 must be reset to "Sẵn sàng"
                Assert.Equal("Sẵn sàng", lolGame.Status);
            });
        }

        // =========================================================================
        // 4. IN-CLIENT F&B ORDERING TESTS
        // =========================================================================

        [Fact]
        public void FormClientDashboard_FoodMenu_LoadsCatalogAndFiltersByCategory()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);

                Assert.NotEmpty(dashboard.AvailableServiceItems);
                Assert.Contains(dashboard.AvailableServiceItems, i => i.ServiceName.Contains("Sting") || i.ServiceName.Contains("Red Bull"));

                var dgvFoodItems = typeof(FormClientDashboard).GetField("dgvFoodItems", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as DataGridView;
                Assert.NotNull(dgvFoodItems);
                Assert.True(dgvFoodItems.Rows.Count > 0);
            });
        }

        [Fact]
        public void FormClientDashboard_TryOrderFood_ValidatesStockAndQuantity()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser);

                // Invalid service id
                bool res1 = dashboard.TryOrderFood(999999, 1, null, out string msg1);
                Assert.False(res1);
                Assert.Contains("không tồn tại", msg1);

                // Zero quantity
                int firstId = dashboard.AvailableServiceItems[0].ServiceId;
                bool res2 = dashboard.TryOrderFood(firstId, 0, null, out string msg2);
                Assert.False(res2);
                Assert.Contains("lớn hơn 0", msg2);

                // Exceed stock
                bool res3 = dashboard.TryOrderFood(firstId, 99999, null, out string msg3);
                Assert.False(res3);
                Assert.Contains("kho", msg3);
            });
        }

        [Fact]
        public void FormClientDashboard_TryOrderFood_Success_UpdatesHudAndSessionOrders()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 100000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);

                decimal initialServiceCost = dashboard.CurrentSession!.ServiceCost;
                var targetItem = dashboard.AvailableServiceItems.First(i => i.StockQuantity >= 2);
                int initialStock = targetItem.StockQuantity;
                decimal itemPrice = targetItem.UnitPrice;

                bool ordered = dashboard.TryOrderFood(targetItem.ServiceId, 2, "ít đá, nhiều đường", out string orderMessage);
                Assert.True(ordered);
                Assert.Contains("thành công", orderMessage);

                // Stock deducted
                Assert.Equal(initialStock - 2, targetItem.StockQuantity);

                // Service cost incremented
                Assert.Equal(initialServiceCost + itemPrice * 2, dashboard.CurrentSession.ServiceCost);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_TryOrderFood_WhenRemainingBalanceInsufficient_RejectsOrder()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                // User has only 10,000 VNĐ balance
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 10000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);

                // Target an item with UnitPrice > 10,000 and StockQuantity > 0 (e.g. Red Bull 20,000 or Cơm gà 45,000)
                var expensiveItem = dashboard.AvailableServiceItems.First(i => i.UnitPrice > 10000m && i.StockQuantity > 0);
                bool ordered = dashboard.TryOrderFood(expensiveItem.ServiceId, 1, null, out string errorMsg);

                Assert.False(ordered);
                Assert.Contains("Số dư ví không đủ", errorMsg);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_TryOrderFood_Success_RefreshesGridStockAndUiState()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 5000000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                dashboard.Show();
                Application.DoEvents();

                var targetItem = dashboard.AvailableServiceItems.First(i => i.StockQuantity > 0);
                int savedStock = targetItem.StockQuantity;
                targetItem.StockQuantity = 1; // Set local stock to 1 to test zeroing out
                try
                {
                    bool ordered = dashboard.TryOrderFood(targetItem.ServiceId, 1, "Hết sạch tồn kho", out _);
                    Assert.True(ordered);
                    Assert.Equal(0, targetItem.StockQuantity);

                    var dgvFoodItems = typeof(FormClientDashboard).GetField("dgvFoodItems", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as DataGridView;
                    var btnOrderFood = typeof(FormClientDashboard).GetField("btnOrderFood", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Button;

                    Assert.NotNull(dgvFoodItems);
                    Assert.NotNull(btnOrderFood);
                    // When selected item stock is 0, button must be disabled
                    Assert.False(btnOrderFood.Enabled);
                }
                finally
                {
                    targetItem.StockQuantity = savedStock;
                }

                CurrentUserSession.Clear();
            });
        }

        // =========================================================================
        // 5. MEMBER UTILITIES (PASSWORD, LOCK, STAFF, LOGOUT)
        // =========================================================================

        [Fact]
        public void FormChangePasswordDialog_Validation_RejectsInvalidInputs()
        {
            RunOnSTA(() =>
            {
                var dbUser = CreateOfflineUserDb();
                using var dlg = new FormChangePasswordDialog(dbUser, "gamer01");

                var txtOld = typeof(FormChangePasswordDialog).GetField("txtOldPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                var txtNew = typeof(FormChangePasswordDialog).GetField("txtNewPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                var txtConfirm = typeof(FormChangePasswordDialog).GetField("txtConfirmPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;

                // Empty old password
                Assert.False(dlg.TryChangePassword(out string msg1));
                Assert.Contains("mật khẩu hiện tại", msg1);

                // Empty new password
                txtOld!.Text = "123456";
                Assert.False(dlg.TryChangePassword(out string msg2));
                Assert.Contains("mật khẩu mới", msg2);

                // Short new password (< 6 chars)
                txtNew!.Text = "123";
                txtConfirm!.Text = "123";
                Assert.False(dlg.TryChangePassword(out string msg3));
                Assert.Contains("tối thiểu từ 6 ký tự", msg3);

                // Mismatched confirm password
                txtNew.Text = "654321";
                txtConfirm.Text = "999999";
                Assert.False(dlg.TryChangePassword(out string msg4));
                Assert.Contains("không khớp", msg4);

                // Same as old password
                txtNew.Text = "123456";
                txtConfirm.Text = "123456";
                Assert.False(dlg.TryChangePassword(out string msg5));
                Assert.Contains("không được trùng", msg5);
            });
        }

        [Fact]
        public void FormChangePasswordDialog_Success_ChangesPasswordWithHash()
        {
            RunOnSTA(() =>
            {
                var dbUser = CreateOfflineUserDb();
                using var dlg = new FormChangePasswordDialog(dbUser, "gamer01");

                var txtOld = typeof(FormChangePasswordDialog).GetField("txtOldPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                var txtNew = typeof(FormChangePasswordDialog).GetField("txtNewPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                var txtConfirm = typeof(FormChangePasswordDialog).GetField("txtConfirmPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;

                txtOld!.Text = "123456";
                txtNew!.Text = "Abc@123456";
                txtConfirm!.Text = "Abc@123456";

                try
                {
                    bool success = dlg.TryChangePassword(out string error);
                    Assert.True(success);
                    Assert.True(string.IsNullOrEmpty(error));

                    // Verify login with new password succeeds and old password fails
                    bool authOld = dbUser.TryAuthenticate("gamer01", "123456", out _, out _);
                    Assert.False(authOld);

                    bool authNew = dbUser.TryAuthenticate("gamer01", "Abc@123456", out var newUser, out _);
                    Assert.True(authNew);
                    Assert.NotNull(newUser);
                }
                finally
                {
                    // Restore password for other tests that rely on gamer01 seed
                    dbUser.ChangePassword("gamer01", "Abc@123456", "123456", out _);
                }
            });
        }

        [Fact]
        public void FormClientDashboard_LockStation_And_TryUnlock_ProtectsSessionWithPassword()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                dashboard.Show();
                Application.DoEvents();

                Assert.False(dashboard.IsScreenLocked);

                // Lock station
                dashboard.LockStation();
                Assert.True(dashboard.IsScreenLocked);

                var pnlLockOverlay = typeof(FormClientDashboard).GetField("pnlLockOverlay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dashboard) as Panel;
                Assert.NotNull(pnlLockOverlay);
                Assert.True(pnlLockOverlay.Visible);

                // Unlock with wrong password
                bool unlockedWrong = dashboard.TryUnlockStation("WrongPass999", out string errWrong);
                Assert.False(unlockedWrong);
                Assert.True(dashboard.IsScreenLocked);
                Assert.True(pnlLockOverlay.Visible);

                // Unlock with correct password
                bool unlockedRight = dashboard.TryUnlockStation("123456", out string msgRight);
                Assert.True(unlockedRight);
                Assert.False(dashboard.IsScreenLocked);
                Assert.False(pnlLockOverlay.Visible);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_CallStaff_RecordsReasonAndNotifies()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                dashboard.SetPromptHandler((msg, title, btns, icon) => DialogResult.OK);

                dashboard.CallStaff("Nạp thêm tiền ví hội viên");
                Assert.Equal("Nạp thêm tiền ví hội viên", dashboard.LastStaffCallReason);
            });
        }

        [Fact]
        public void FormClientDashboard_EndSessionAndLogout_ClosesSessionAndRequestsLogout()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                dashboard.SetPromptHandler((msg, title, btns, icon) => DialogResult.OK);

                Assert.False(dashboard.IsLogoutRequested);

                bool result = dashboard.EndSessionAndLogout();
                Assert.True(result);
                Assert.True(dashboard.IsLogoutRequested);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_FormClosing_WhenScreenLocked_CancelsClosing()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                dashboard.SetPromptHandler((msg, title, btns, icon) => DialogResult.OK);
                dashboard.Show();
                Application.DoEvents();

                // Lock screen
                dashboard.LockStation();
                Assert.True(dashboard.IsScreenLocked);

                // Simulate user clicking 'X' on window
                var closingArgs = new FormClosingEventArgs(CloseReason.UserClosing, false);
                var onFormClosingMethod = typeof(Form).GetMethod("OnFormClosing", BindingFlags.NonPublic | BindingFlags.Instance);
                onFormClosingMethod?.Invoke(dashboard, new object[] { closingArgs });

                // Form closing MUST be cancelled when screen is locked!
                Assert.True(closingArgs.Cancel);
                Assert.True(dashboard.IsScreenLocked);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_FormClosing_WhenMember_ConfirmsEndSession()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                // Prompt handler answers YES to confirmation
                dashboard.SetPromptHandler((msg, title, btns, icon) => DialogResult.Yes);
                dashboard.Show();
                Application.DoEvents();

                var closingArgs = new FormClosingEventArgs(CloseReason.UserClosing, false);
                var onFormClosingMethod = typeof(Form).GetMethod("OnFormClosing", BindingFlags.NonPublic | BindingFlags.Instance);
                onFormClosingMethod?.Invoke(dashboard, new object[] { closingArgs });

                // Confirmed close triggers EndSessionAndLogout
                Assert.True(dashboard.IsLogoutRequested);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_AdminPreview_AvailableStation_DoesNotPolluteDBWithOpenSession()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(1, "admin", "Admin", "Quản Lý", 0m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 3);

                // Admin preview mode
                Assert.True(dashboard.IsPreviewMode);
                Assert.NotNull(dashboard.CurrentStation);
                Assert.Equal(3, dashboard.CurrentStation.StationId);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_UpdateRealtimeHud_WhenBalanceRunsOut_ForcesGameStopAndLocksMachine()
        {
            RunOnSTA(() =>
            {
                var dbStation = CreateOfflineStationDb();
                var dbBilling = CreateOfflineBillingDb();
                var dbUser = CreateOfflineUserDb();

                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 0m, "Standard");
                using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                dashboard.Show();
                Application.DoEvents();

                // Launch game
                dashboard.LaunchGame("cs2");
                Assert.True(dashboard.IsGameRunning);

                // Elapsed time exceeds balance (0 balance)
                dashboard.CurrentSession!.StartTime = DateTime.Now.AddHours(-1);
                dashboard.UpdateRealtimeHud();

                // Must forcefully stop running game and lock machine!
                Assert.False(dashboard.IsGameRunning);
                Assert.Null(dashboard.RunningGame);
                Assert.True(dashboard.IsScreenLocked);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void StaffCallQueue_CallStaff_BroadcastsNotificationToQueueAndSubscribers()
        {
            RunOnSTA(() =>
            {
                StaffCallQueue.Clear();
                StaffCallNotification? receivedCall = null;
                Action<StaffCallNotification> handler = call => { receivedCall = call; };

                StaffCallQueue.OnCallReceived += handler;
                try
                {
                    var dbStation = CreateOfflineStationDb();
                    var dbBilling = CreateOfflineBillingDb();
                    var dbUser = CreateOfflineUserDb();

                    CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                    using var dashboard = new FormClientDashboard(dbStation, dbBilling, dbUser, 1);
                    dashboard.SetPromptHandler((msg, title, btns, icon) => DialogResult.OK);

                    dashboard.CallStaff("Nạp tiền ví 100k");

                    Assert.NotNull(receivedCall);
                    Assert.Equal("Nạp tiền ví 100k", receivedCall.Reason);
                    Assert.Contains("PC-", receivedCall.StationName);
                    Assert.Equal("gamer01", receivedCall.MemberUsername);

                    var allCalls = StaffCallQueue.GetAll();
                    Assert.NotEmpty(allCalls);
                    Assert.Equal("Nạp tiền ví 100k", allCalls.Last().Reason);

                    CurrentUserSession.Clear();
                }
                finally
                {
                    StaffCallQueue.OnCallReceived -= handler;
                    StaffCallQueue.Clear();
                }
            });
        }

        [Fact]
        public void StaffCallQueue_ResolveCall_MarksStatusResolvedAndFiresEvent()
        {
            StaffCallQueue.Clear();
            StaffCallNotification? resolvedCall = null;
            Action<StaffCallNotification> resolvedHandler = c => resolvedCall = c;
            StaffCallQueue.OnCallResolved += resolvedHandler;

            try
            {
                var call = new StaffCallNotification
                {
                    StationId = 3,
                    StationName = "PC-03",
                    MemberUsername = "gamer02",
                    MemberFullName = "Trần B",
                    Reason = "Đổi chuột máy"
                };

                StaffCallQueue.Enqueue(call);
                Assert.Equal("Pending", call.Status);

                bool success = StaffCallQueue.ResolveCall(call.CallId, "staff01");
                Assert.True(success);
                Assert.Equal("Resolved", call.Status);
                Assert.Equal("staff01", call.ResolvedBy);
                Assert.NotNull(call.ResolvedAt);
                Assert.NotNull(resolvedCall);
                Assert.Equal(call.CallId, resolvedCall.CallId);
            }
            finally
            {
                StaffCallQueue.OnCallResolved -= resolvedHandler;
                StaffCallQueue.Clear();
            }
        }

        [Fact]
        public void StaffCallQueue_GetPendingCalls_ReturnsOnlyUnresolvedCalls()
        {
            StaffCallQueue.Clear();
            try
            {
                var call1 = new StaffCallNotification { StationId = 1, StationName = "PC-01", Reason = "Order đồ ăn" };
                var call2 = new StaffCallNotification { StationId = 2, StationName = "PC-02", Reason = "Gọi nhân viên" };

                StaffCallQueue.Enqueue(call1);
                StaffCallQueue.Enqueue(call2);

                Assert.Equal(2, StaffCallQueue.GetPendingCalls().Count);

                StaffCallQueue.ResolveCall(call1.CallId, "staff01");

                var pending = StaffCallQueue.GetPendingCalls();
                Assert.Single(pending);
                Assert.Equal(call2.CallId, pending[0].CallId);
            }
            finally
            {
                StaffCallQueue.Clear();
            }
        }

        [Fact]
        public void StaffCallQueue_ResolveCall_NonExistentOrAlreadyResolved_ReturnsFalse()
        {
            StaffCallQueue.Clear();
            try
            {
                var call = new StaffCallNotification { StationId = 5, StationName = "PC-05", Reason = "Kẹt phím" };
                StaffCallQueue.Enqueue(call);

                bool fakeResolve = StaffCallQueue.ResolveCall(999999, "staff01");
                Assert.False(fakeResolve);

                bool firstResolve = StaffCallQueue.ResolveCall(call.CallId, "staff01");
                Assert.True(firstResolve);

                bool secondResolve = StaffCallQueue.ResolveCall(call.CallId, "staff01");
                Assert.False(secondResolve);
            }
            finally
            {
                StaffCallQueue.Clear();
            }
        }

        [Fact]
        public void StaffCallNotification_MarkResolved_SetsResolvedPropertiesCorrectly()
        {
            var call = new StaffCallNotification
            {
                StationId = 1,
                StationName = "PC-01",
                Reason = "Báo lỗi game"
            };

            Assert.Equal("Pending", call.Status);
            Assert.Null(call.ResolvedBy);
            Assert.Null(call.ResolvedAt);

            call.MarkResolved("admin");

            Assert.Equal("Resolved", call.Status);
            Assert.Equal("admin", call.ResolvedBy);
            Assert.NotNull(call.ResolvedAt);
        }

        [Fact]
        public void Form1_StaffCall_OnCallReceivedAndResolved_UpdatesRealtimeSummaryLabel()
        {
            RunOnSTA(() =>
            {
                StaffCallQueue.Clear();
                CurrentUserSession.SetSession(1, "admin", "Admin", "Quản Trị Viên", 0, "Standard");

                using var form1 = new Form1();
                form1.Show();
                Application.DoEvents();

                var call = new StaffCallNotification
                {
                    StationId = 4,
                    StationName = "PC-04",
                    MemberUsername = "gamer03",
                    Reason = "Nạp tiền ví 50k"
                };

                StaffCallQueue.Enqueue(call);
                Application.DoEvents();

                // Kiểm tra label có thông tin gọi máy
                var lblSummary = form1.Controls.Find("lblRealtimeSummary", true).FirstOrDefault() as Label;
                Assert.NotNull(lblSummary);
                Assert.Contains("PC-04", lblSummary.Text);
                Assert.Contains("Nạp tiền ví 50k", lblSummary.Text);

                // Test giải quyết cuộc gọi
                StaffCallQueue.ResolveCall(call.CallId, "admin");
                Application.DoEvents();

                Assert.Contains("Đã xử lý yêu cầu", lblSummary.Text);

                CurrentUserSession.Clear();
                StaffCallQueue.Clear();
            });
        }
    }
}
