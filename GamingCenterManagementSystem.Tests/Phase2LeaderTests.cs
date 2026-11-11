using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;
using Microsoft.Data.SqlClient;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    [CollectionDefinition("GamingCenterSequential", DisableParallelization = true)]
    public sealed class GamingCenterSequentialCollection
    {
    }

    [Collection("GamingCenterSequential")]
    public class Phase2LeaderTests
    {
        private static DBUser CreateOfflineUserDb() =>
            new(() => throw new TimeoutException("Simulated offline SQL Server"));

        private static int CreateTemporaryStation(string prefix = "TST")
        {
            string stationName = $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";
            using var conn = Database.GetConnection();
            conn.Open();
            const string sql = @"INSERT INTO dbo.ComputerStation (StationName, ZoneId, IpAddress, Status)
                                 OUTPUT INSERTED.StationId
                                 VALUES (@Name, 1, '192.168.99.99', N'Available')";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = stationName;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void CleanupTemporaryArtifacts(int stationId, int userId, int sessionId = 0)
        {
            using var conn = Database.GetConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            const string sql = @"IF @SessionId > 0
                                 BEGIN
                                     IF OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL
                                         DELETE FROM dbo.SessionService WHERE SessionId = @SessionId;
                                     IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                         DELETE FROM dbo.WalletTransaction WHERE SessionId = @SessionId;
                                     DELETE FROM dbo.GamingSession WHERE SessionId = @SessionId;
                                 END
                                 IF @StationId > 0
                                 BEGIN
                                     IF OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL
                                         DELETE ss FROM dbo.SessionService ss INNER JOIN dbo.GamingSession gs ON ss.SessionId = gs.SessionId WHERE gs.StationId = @StationId;
                                     IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                         DELETE wt FROM dbo.WalletTransaction wt INNER JOIN dbo.GamingSession gs ON wt.SessionId = gs.SessionId WHERE gs.StationId = @StationId;
                                     DELETE FROM dbo.GamingSession WHERE StationId = @StationId;
                                     DELETE FROM dbo.ComputerStation WHERE StationId = @StationId;
                                 END
                                 IF @UserId > 0
                                 BEGIN
                                     IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                         DELETE FROM dbo.WalletTransaction WHERE UserId = @UserId;
                                     DELETE FROM dbo.AppUser WHERE UserId = @UserId;
                                 END";
            using var cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
            cmd.Parameters.Add("@StationId", SqlDbType.Int).Value = stationId;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.ExecuteNonQuery();
            trans.Commit();
        }

        // ==========================================
        // 1. MemberTierTariff TESTS (POLYMORPHISM)
        // ==========================================

        [Fact]
        public void MemberTierTariff_GoldDefault_Applies15PercentDiscount()
        {
            TariffCalculatorBase calculator = new MemberTierTariff();
            DateTime start = new(2026, 11, 11, 10, 0, 0);
            DateTime end = start.AddHours(2);

            decimal cost = calculator.CalculateCost(start, end, 10000m);

            Assert.Equal(17000m, cost);
            Assert.Contains("Gold", calculator.TariffName);
        }

        [Theory]
        [InlineData("Standard", 20000)]
        [InlineData("Silver", 18000)]
        [InlineData("Gold", 17000)]
        public void MemberTierTariff_TierDiscounts_CalculateAccurately(string tier, decimal expectedCost)
        {
            TariffCalculatorBase calculator = new MemberTierTariff(tier);
            DateTime start = new(2026, 11, 11, 14, 0, 0);
            DateTime end = start.AddHours(2);

            decimal cost = calculator.CalculateCost(start, end, 10000m);

            Assert.Equal(expectedCost, cost);
        }

        [Fact]
        public void MemberTierTariff_MinimumDuration_ClampsTo0Point1Hours()
        {
            TariffCalculatorBase calculator = new MemberTierTariff("Gold");
            DateTime start = new(2026, 11, 11, 10, 0, 0);
            DateTime end = start.AddMinutes(2);

            decimal cost = calculator.CalculateCost(start, end, 10000m);

            Assert.Equal(850m, cost);
        }

        [Fact]
        public void MemberTierTariff_RejectsInvalidTierDiscountOrNegativePrice()
        {
            Assert.Throws<ArgumentException>(() => new MemberTierTariff("Diamond"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MemberTierTariff("Gold", -0.1m));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MemberTierTariff("Gold", 1.2m));

            var tariff = new MemberTierTariff("Silver", 0.25m);
            Assert.Equal(15000m, tariff.CalculateCost(DateTime.Today, DateTime.Today.AddHours(2), 10000m));
            Assert.Throws<ArgumentOutOfRangeException>(() => tariff.CalculateCost(DateTime.Today, DateTime.Today.AddHours(1), -100m));
        }

        // ==========================================
        // 2. AUTHENTICATION & CurrentUserSession TESTS
        // ==========================================

        [Fact]
        public void DBUser_Authenticate_OfflineSupportsAllRolesAndRejectsWrongCredentials()
        {
            var offlineDb = CreateOfflineUserDb();

            Assert.True(offlineDb.TryAuthenticate("admin", "123456", out AppUser? admin, out string adminErr), adminErr);
            Assert.NotNull(admin);
            Assert.Equal("Admin", admin!.Role);
            Assert.Equal(string.Empty, admin.PasswordHash);

            Assert.True(offlineDb.TryAuthenticate("staff01", "123456", out AppUser? staff, out string staffErr), staffErr);
            Assert.NotNull(staff);
            Assert.Equal("Staff", staff!.Role);

            Assert.True(offlineDb.TryAuthenticate("gamer01", "123456", out AppUser? member, out string memErr), memErr);
            Assert.NotNull(member);
            Assert.Equal("Member", member!.Role);

            Assert.False(offlineDb.TryAuthenticate("admin", "wrongpass", out _, out string badPassErr));
            Assert.False(string.IsNullOrWhiteSpace(badPassErr));

            Assert.False(offlineDb.TryAuthenticate("", "123456", out _, out _));
            Assert.Null(offlineDb.Authenticate("nonexistent_user", "123456"));
        }

        [Fact]
        public async Task DBUser_Authenticate_LiveDatabasePbkdf2AndLockedAccountHandledCorrectly()
        {
            var db = new DBUser();
            string uniqueUsername = $"auth_{Guid.NewGuid():N}"[..15];
            var user = new AppUser
            {
                Username = uniqueUsername,
                FullName = "Auth Test User",
                Role = "Staff",
                Balance = 0m,
                MemberTier = "Standard"
            };

            try
            {
                Assert.True(db.CreateMember(user, "Secret#2026", out string err), err);

                var (ok, authUser, authErr) = await db.AuthenticateAsync(uniqueUsername, "Secret#2026");
                Assert.True(ok, authErr);
                Assert.NotNull(authUser);
                Assert.Equal("Staff", authUser!.Role);
                Assert.Equal(string.Empty, authUser.PasswordHash);

                Assert.False(db.TryAuthenticate(uniqueUsername, "WrongPass", out _, out _));

                Assert.True(db.SetUserActiveStatus(user.UserId, false));
                Assert.False(db.TryAuthenticate(uniqueUsername, "Secret#2026", out _, out string lockedErr));
                Assert.Contains("khóa", lockedErr, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                if (user.UserId > 0)
                    db.Delete(user.UserId);
            }
        }

        [Fact]
        public void CurrentUserSession_SetUserRefreshAndClear_ManagesSessionSafely()
        {
            try
            {
                CurrentUserSession.Clear();
                Assert.False(CurrentUserSession.IsLoggedIn);
                Assert.Equal(0, CurrentUserSession.UserId);

                var user = new AppUser
                {
                    UserId = 99,
                    Username = "staff_test",
                    PasswordHash = "PBKDF2$secret",
                    FullName = "Nhân Viên A",
                    Role = "Staff",
                    Balance = 150000m,
                    MemberTier = "Silver",
                    IsActive = true
                };

                CurrentUserSession.SetUser(user);
                Assert.True(CurrentUserSession.IsLoggedIn);
                Assert.Equal(99, CurrentUserSession.UserId);
                Assert.Equal("staff_test", CurrentUserSession.Username);
                Assert.True(CurrentUserSession.IsStaff);
                Assert.False(CurrentUserSession.IsAdmin);
                Assert.False(CurrentUserSession.IsMember);
                Assert.True(CurrentUserSession.CanManageOperations);
                Assert.Equal(string.Empty, CurrentUserSession.CurrentUser!.PasswordHash);
                Assert.NotNull(CurrentUserSession.LoginTime);

                user.Balance = 250000m;
                user.MemberTier = "Gold";
                CurrentUserSession.RefreshUser(user);
                Assert.Equal(250000m, CurrentUserSession.Balance);
                Assert.Equal("Gold", CurrentUserSession.MemberTier);
            }
            finally
            {
                CurrentUserSession.Clear();
                Assert.False(CurrentUserSession.IsLoggedIn);
            }
        }

        [Fact]
        public void FormLogin_And_Form1_RolePermissions_WorkAcrossAllThreeRoles()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.Clear();
                    var offlineDb = CreateOfflineUserDb();

                    using (var loginForm = new FormLogin(offlineDb))
                    {
                        var userBox = typeof(FormLogin).GetField("txtUsername", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as TextBox;
                        var passBox = typeof(FormLogin).GetField("txtPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as TextBox;
                        Assert.NotNull(userBox);
                        Assert.NotNull(passBox);
                        Assert.True(passBox!.UseSystemPasswordChar);

                        userBox!.Text = "admin";
                        passBox.Text = "wrongpass";
                        Assert.False(loginForm.TryPerformLogin(out string badErr));
                        Assert.False(string.IsNullOrWhiteSpace(badErr));
                        Assert.False(CurrentUserSession.IsLoggedIn);

                        passBox.Text = "123456";
                        Assert.True(loginForm.TryPerformLogin(out string okErr), okErr);
                        Assert.True(CurrentUserSession.IsLoggedIn);
                        Assert.True(CurrentUserSession.IsAdmin);
                    }

                    using (var form1 = new Form1())
                    {
                        var btnStation = typeof(Form1).GetField("btnNavStation", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var btnMember = typeof(Form1).GetField("btnNavMember", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var btnTournament = typeof(Form1).GetField("btnNavTournament", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var btnBilling = typeof(Form1).GetField("btnNavBilling", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var btnMoMay = typeof(Form1).GetField("btnMoMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var btnDongMay = typeof(Form1).GetField("btnDongMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var btnBaoTri = typeof(Form1).GetField("btnBaoTri", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                        var lblCurrentUser = typeof(Form1).GetField("lblCurrentUser", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                        var lblRealtimeSummary = typeof(Form1).GetField("lblRealtimeSummary", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                        var timer = typeof(Form1).GetField("_realtimeTimer", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as System.Windows.Forms.Timer;

                        Assert.NotNull(btnStation);
                        Assert.NotNull(btnMember);
                        Assert.NotNull(btnTournament);
                        Assert.NotNull(btnBilling);
                        Assert.NotNull(btnMoMay);
                        Assert.NotNull(btnDongMay);
                        Assert.NotNull(btnBaoTri);
                        Assert.NotNull(lblCurrentUser);
                        Assert.NotNull(lblRealtimeSummary);
                        Assert.NotNull(timer);
                        Assert.True(timer!.Enabled);
                        Assert.Equal(1000, timer.Interval);
                        Assert.Contains("Trống:", lblRealtimeSummary!.Text);

                        // Admin check
                        Assert.True(btnStation!.Enabled);
                        Assert.True(btnMember!.Enabled);
                        Assert.True(btnTournament!.Enabled);
                        Assert.True(btnBilling!.Enabled);
                        Assert.Contains("Admin", lblCurrentUser!.Text);

                        // Staff check
                        Assert.True(offlineDb.TryAuthenticate("staff01", "123456", out AppUser? staff, out _));
                        CurrentUserSession.SetUser(staff!);
                        form1.ApplyRolePermissions();
                        Assert.True(btnStation.Enabled);
                        Assert.True(btnMember.Enabled);
                        Assert.True(btnTournament.Enabled);
                        Assert.False(btnBilling.Enabled);
                        Assert.Contains("Staff", lblCurrentUser.Text);

                        // Member check (must keep station action buttons disabled even on Available/InUse stations)
                        Assert.True(offlineDb.TryAuthenticate("gamer01", "123456", out AppUser? member, out _));
                        CurrentUserSession.SetUser(member!);
                        form1.ApplyRolePermissions();
                        Assert.False(btnStation.Enabled);
                        Assert.False(btnMember.Enabled);
                        Assert.False(btnTournament.Enabled);
                        Assert.False(btnBilling.Enabled);
                        Assert.False(btnMoMay!.Enabled);
                        Assert.False(btnDongMay!.Enabled);
                        Assert.False(btnBaoTri!.Enabled);
                        Assert.Contains("gamer01", lblCurrentUser.Text);
                    }
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    CurrentUserSession.Clear();
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }

        // ==========================================
        // 3. SCHEMA & MODELS TESTS
        // ==========================================

        [Fact]
        public void DatabaseSchema_ContainsPhase2TablesAndAppUserIsActive()
        {
            using var conn = Database.GetConnection();
            conn.Open();
            const string sql = @"SELECT
                                     CASE WHEN OBJECT_ID('dbo.ServiceItem', 'U') IS NOT NULL THEN 1 ELSE 0 END,
                                     CASE WHEN OBJECT_ID('dbo.SessionService', 'U') IS NOT NULL THEN 1 ELSE 0 END,
                                     CASE WHEN OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL THEN 1 ELSE 0 END,
                                     CASE WHEN COL_LENGTH('dbo.AppUser', 'IsActive') IS NOT NULL THEN 1 ELSE 0 END";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal(1, reader.GetInt32(0));
            Assert.Equal(1, reader.GetInt32(1));
            Assert.Equal(1, reader.GetInt32(2));
            Assert.Equal(1, reader.GetInt32(3));
        }

        [Fact]
        public void Phase2Models_EnforceEncapsulationAndHelpers()
        {
            var item = new ServiceItem { ServiceId = 1, ServiceName = "Sting", UnitPrice = 15000m, StockQuantity = 5, IsActive = true };
            Assert.True(item.IsAvailableForOrder(3));
            Assert.False(item.IsAvailableForOrder(6));
            Assert.Throws<ArgumentException>(() => item.UnitPrice = -1m);
            Assert.Throws<ArgumentException>(() => item.StockQuantity = -1);

            var line = new SessionService { SessionId = 1, ServiceId = 1, Quantity = 3, UnitPrice = 15000m };
            Assert.Equal(45000m, line.CalculateLineTotal());
            Assert.Throws<ArgumentException>(() => line.Quantity = 0);
            Assert.Throws<ArgumentException>(() => line.UnitPrice = -100m);

            var tx = new WalletTransaction { TransactionType = "TopUp", Amount = 50000m, BalanceBefore = 0m, BalanceAfter = 50000m };
            Assert.Equal(50000m, tx.BalanceAfter);
        }

        // ==========================================
        // 4. SQL TRANSACTION & EXCEPTION TESTS
        // ==========================================

        [Fact]
        public void DBBilling_OpenStationSession_RejectsZeroBalanceAndDoubleOpenAtomically()
        {
            var dbUser = new DBUser();
            var dbBilling = new DBBilling();
            var dbStation = new DBStation();

            int stationId = CreateTemporaryStation("OPN");
            var zeroBalanceUser = new AppUser
            {
                Username = $"zero_{Guid.NewGuid():N}"[..15],
                FullName = "Zero Balance User",
                Role = "Staff", // Staff allowed 0 initial balance
                Balance = 0m,
                MemberTier = "Standard"
            };
            var fundedUser = new AppUser
            {
                Username = $"fund_{Guid.NewGuid():N}"[..15],
                FullName = "Funded Gamer",
                Role = "Member",
                Balance = 50000m,
                MemberTier = "Standard"
            };

            try
            {
                Assert.True(dbUser.CreateMember(zeroBalanceUser, "pass1234", out string err1), err1);
                Assert.True(dbUser.CreateMember(fundedUser, "pass1234", out string err2), err2);

                // 1. Zero balance throws InsufficientBalanceException and leaves station Available
                Assert.Throws<InsufficientBalanceException>(() =>
                    dbBilling.OpenStationSession(stationId, zeroBalanceUser.UserId, "Hourly", minimumRequiredBalance: 800m));
                Assert.Equal("Available", dbStation.GetById(stationId)?.Status);
                Assert.Null(dbBilling.GetActiveSession(stationId));

                // 2. Funded user opens station atomically
                var activeSession = dbBilling.OpenStationSession(stationId, fundedUser.UserId, "Hourly", minimumRequiredBalance: 800m);
                Assert.True(activeSession.SessionId > 0);
                Assert.Equal("InUse", dbStation.GetById(stationId)?.Status);

                // 3. Re-opening same InUse station throws StationInUseException
                var ex = Assert.Throws<StationInUseException>(() =>
                    dbBilling.OpenStationSession(stationId, fundedUser.UserId, "Hourly"));
                Assert.Equal(stationId, ex.StationId);

                // 4. Maintenance toggle while InUse throws StationInUseException
                Assert.Throws<StationInUseException>(() => dbBilling.ToggleStationMaintenance(stationId));
            }
            finally
            {
                CleanupTemporaryArtifacts(stationId, zeroBalanceUser.UserId);
                CleanupTemporaryArtifacts(0, fundedUser.UserId);
            }
        }

        [Fact]
        public void DBBilling_CloseStationSession_MemberTierAndSessionService_DeductsWalletAndLogsTransaction()
        {
            var dbUser = new DBUser();
            var dbBilling = new DBBilling();
            var dbStation = new DBStation();

            int stationId = CreateTemporaryStation("CLS");
            var goldMember = new AppUser
            {
                Username = $"gold_{Guid.NewGuid():N}"[..15],
                FullName = "Gold Gamer",
                Role = "Member",
                Balance = 100000m,
                MemberTier = "Gold"
            };

            try
            {
                Assert.True(dbUser.CreateMember(goldMember, "goldPass1", out string err), err);

                DateTime start = new(2026, 11, 11, 10, 0, 0);
                DateTime end = start.AddHours(2);

                var session = dbBilling.OpenStationSession(stationId, goldMember.UserId, "MemberTier", startTime: start);

                // Insert a real SessionService order row (15,000 VND)
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string svcSql = @"INSERT INTO dbo.SessionService (SessionId, ServiceId, Quantity, UnitPrice, TotalPrice, Note)
                                            SELECT @SessionId, TOP_SVC.ServiceId, 1, 15000, 15000, N'Test F&B'
                                            FROM (SELECT TOP 1 ServiceId FROM dbo.ServiceItem ORDER BY ServiceId) AS TOP_SVC";
                    using var svcCmd = new SqlCommand(svcSql, conn);
                    svcCmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = session.SessionId;
                    svcCmd.ExecuteNonQuery();
                }

                // 2h @ 10,000 with Gold (15% off) = 17,000 + 15,000 F&B = 32,000
                var result = dbBilling.CloseStationSession(stationId, 10000m, endTime: end, allowCashSupplement: false);

                Assert.Equal(17000m, result.PlayCost);
                Assert.Equal(15000m, result.ServiceCost);
                Assert.Equal(32000m, result.TotalCost);
                Assert.Equal(32000m, result.WalletDeducted);
                Assert.Equal(0m, result.CashDue);
                Assert.Equal(68000m, result.RemainingBalance);

                Assert.Equal("Available", dbStation.GetById(stationId)?.Status);
                Assert.Null(dbBilling.GetActiveSession(stationId));
                Assert.Equal(68000m, dbUser.GetById(goldMember.UserId)?.Balance);

                // Verify WalletTransaction row recorded
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    const string txSql = @"SELECT TransactionType, Amount, BalanceBefore, BalanceAfter
                                           FROM dbo.WalletTransaction
                                           WHERE SessionId = @SessionId";
                    using var txCmd = new SqlCommand(txSql, conn);
                    txCmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = session.SessionId;
                    using var reader = txCmd.ExecuteReader();
                    Assert.True(reader.Read());
                    Assert.Equal("SessionPayment", reader.GetString(0));
                    Assert.Equal(32000m, reader.GetDecimal(1));
                    Assert.Equal(100000m, reader.GetDecimal(2));
                    Assert.Equal(68000m, reader.GetDecimal(3));
                }
            }
            finally
            {
                CleanupTemporaryArtifacts(stationId, goldMember.UserId);
            }
        }

        [Fact]
        public void DBBilling_CloseStationSession_InsufficientBalanceRollsBack_ThenSucceedsWithCashSupplement()
        {
            var dbUser = new DBUser();
            var dbBilling = new DBBilling();
            var dbStation = new DBStation();

            int stationId = CreateTemporaryStation("RBK");
            var member = new AppUser
            {
                Username = $"low_{Guid.NewGuid():N}"[..15],
                FullName = "Low Balance User",
                Role = "Member",
                Balance = 10000m,
                MemberTier = "Standard"
            };

            try
            {
                Assert.True(dbUser.CreateMember(member, "pass1234", out string err), err);

                DateTime start = new(2026, 11, 11, 10, 0, 0);
                DateTime end = start.AddHours(2); // 2h @ 10,000 = 20,000 > 10,000 balance
                var session = dbBilling.OpenStationSession(stationId, member.UserId, "Hourly", startTime: start);

                // 1. Strict close must throw InsufficientBalanceException AND roll back completely
                var ex = Assert.Throws<InsufficientBalanceException>(() =>
                    dbBilling.CloseStationSession(stationId, 10000m, endTime: end, allowCashSupplement: false));

                Assert.Equal(10000m, ex.CurrentBalance);
                Assert.Equal(20000m, ex.RequiredAmount);

                // Verify complete rollback in DB
                Assert.Equal("InUse", dbStation.GetById(stationId)?.Status);
                Assert.NotNull(dbBilling.GetActiveSession(stationId));
                Assert.Equal(10000m, dbUser.GetById(member.UserId)?.Balance);

                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using var countCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.WalletTransaction WHERE SessionId = @SessionId", conn);
                    countCmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = session.SessionId;
                    Assert.Equal(0, Convert.ToInt32(countCmd.ExecuteScalar()));
                }

                // 2. Retry with allowCashSupplement: true drains wallet to 0 and collects 10,000 cash
                var cashResult = dbBilling.CloseStationSession(stationId, 10000m, endTime: end, allowCashSupplement: true);
                Assert.Equal(20000m, cashResult.TotalCost);
                Assert.Equal(10000m, cashResult.WalletDeducted);
                Assert.Equal(10000m, cashResult.CashDue);
                Assert.Equal(0m, cashResult.RemainingBalance);
                Assert.Equal("Available", dbStation.GetById(stationId)?.Status);
                Assert.Equal(0m, dbUser.GetById(member.UserId)?.Balance);

                // 3. Maintenance toggle cycle works on Available station
                Assert.Equal("Maintenance", dbBilling.ToggleStationMaintenance(stationId));
                Assert.Equal("Maintenance", dbStation.GetById(stationId)?.Status);
                Assert.Equal("Available", dbBilling.ToggleStationMaintenance(stationId));
                Assert.Equal("Available", dbStation.GetById(stationId)?.Status);
            }
            finally
            {
                CleanupTemporaryArtifacts(stationId, member.UserId);
            }
        }

        [Fact]
        public void Form1_RealtimeTimer_UpdatesInUseStationButtonAndMasksOtherMemberBalance()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.Clear();
                    var offlineDb = CreateOfflineUserDb();
                    Assert.True(offlineDb.TryAuthenticate("gamer03", "123456", out AppUser? gamer03, out _));

                    using var form1 = new Form1();
                    var stationButtonsField = typeof(Form1).GetField("_stationButtons", BindingFlags.NonPublic | BindingFlags.Instance);
                    var stationsField = typeof(Form1).GetField("_stations", BindingFlags.NonPublic | BindingFlags.Instance);
                    var updateMethod = typeof(Form1).GetMethod("UpdateRealtimeDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
                    var selectMethod = typeof(Form1).GetMethod("SelectStation", BindingFlags.NonPublic | BindingFlags.Instance);
                    var lblResult = typeof(Form1).GetField("lblKetQuaTinhTien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;

                    Assert.NotNull(stationButtonsField);
                    Assert.NotNull(stationsField);
                    Assert.NotNull(updateMethod);
                    Assert.NotNull(selectMethod);
                    Assert.NotNull(lblResult);

                    var stationButtons = stationButtonsField!.GetValue(form1) as System.Collections.Generic.Dictionary<int, Button>;
                    var stations = stationsField!.GetValue(form1) as System.Collections.Generic.List<ComputerStation>;
                    Assert.NotNull(stationButtons);
                    Assert.NotNull(stations);

                    var inUseStation = stations!.FirstOrDefault(s => s.Status == "InUse");
                    if (inUseStation != null && stationButtons!.TryGetValue(inUseStation.StationId, out Button? inUseBtn))
                    {
                        CurrentUserSession.SetUser(gamer03!);
                        form1.ApplyRolePermissions();
                        selectMethod!.Invoke(form1, new object[] { inUseStation });
                        updateMethod!.Invoke(form1, null);

                        Assert.Contains("⏱️", inUseBtn.Text);
                        Assert.Contains("[ĐANG CHƠI]", inUseBtn.Text);
                        Assert.Contains("Thời lượng:", lblResult!.Text);
                        Assert.DoesNotContain("gamer01", lblResult.Text);
                    }
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    CurrentUserSession.Clear();
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }
    }
}
