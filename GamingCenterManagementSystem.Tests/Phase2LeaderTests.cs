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
                                         DELETE FROM dbo.WalletTransaction WHERE UserId = @UserId OR CreatedByUserId = @UserId;
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
            string inactiveUsername = $"inact_{Guid.NewGuid():N}"[..15];
            var user = new AppUser
            {
                Username = uniqueUsername,
                FullName = "Auth Test User",
                Role = "Staff",
                Balance = 0m,
                MemberTier = "Standard"
            };
            var inactiveUser = new AppUser
            {
                Username = inactiveUsername,
                FullName = "Initially Inactive User",
                Role = "Staff",
                Balance = 0m,
                MemberTier = "Standard",
                IsActive = false
            };

            try
            {
                Assert.True(db.CreateMember(user, "Secret#2026", out string err), err);
                Assert.True(db.CreateMember(inactiveUser, "Secret#2026", out string err2), err2);

                var reloadedInactive = db.GetById(inactiveUser.UserId);
                Assert.NotNull(reloadedInactive);
                Assert.False(reloadedInactive!.IsActive);
                Assert.False(db.TryAuthenticate(inactiveUsername, "Secret#2026", out _, out string initLockedErr));
                Assert.Contains("khóa", initLockedErr, StringComparison.OrdinalIgnoreCase);

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
                if (inactiveUser.UserId > 0)
                    db.Delete(inactiveUser.UserId);
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

                // Case-insensitive tariff factory & validation
                Assert.IsType<MemberTierTariff>(DBBilling.CreateTariffCalculator("membertier", "Gold"));
                Assert.Throws<InvalidOperationException>(() => dbBilling.OpenStationSession(stationId, null, "MemberTier"));
                Assert.Throws<ArgumentException>(() => dbBilling.OpenStationSession(stationId, fundedUser.UserId, "InvalidPlan"));

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

                // Live GetActiveSession reflects SessionService sum before close
                var liveActive = dbBilling.GetActiveSession(stationId);
                Assert.NotNull(liveActive);
                Assert.Equal(15000m, liveActive!.ServiceCost);

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
        public void Form1_RealtimeTimer_TicksViaMessagePumpAndMasksOtherMemberBalance()
        {
            var dbUser = new DBUser();
            var dbBilling = new DBBilling();
            int stationId = CreateTemporaryStation("TMR");
            var playingMember = new AppUser
            {
                Username = $"ply_{Guid.NewGuid():N}"[..15],
                FullName = "Playing Member",
                Role = "Member",
                Balance = 60000m,
                MemberTier = "Gold"
            };
            var viewingMember = new AppUser
            {
                Username = $"viw_{Guid.NewGuid():N}"[..15],
                FullName = "Viewing Member",
                Role = "Member",
                Balance = 40000m,
                MemberTier = "Silver"
            };

            Exception? threadException = null;

            try
            {
                Assert.True(dbUser.CreateMember(playingMember, "pass1234", out string err1), err1);
                Assert.True(dbUser.CreateMember(viewingMember, "pass1234", out string err2), err2);
                dbBilling.OpenStationSession(stationId, playingMember.UserId, "MemberTier", startTime: DateTime.Now.AddMinutes(-30));

                Thread staThread = new(() =>
                {
                    try
                    {
                        CurrentUserSession.SetUser(viewingMember);

                        using var form1 = new Form1
                        {
                            ShowInTaskbar = false,
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(-2000, -2000)
                        };

                        var stationButtons = typeof(Form1).GetField("_stationButtons", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1)
                            as System.Collections.Generic.Dictionary<int, Button>;
                        var stations = typeof(Form1).GetField("_stations", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1)
                            as System.Collections.Generic.List<ComputerStation>;
                        var selectMethod = typeof(Form1).GetMethod("SelectStation", BindingFlags.NonPublic | BindingFlags.Instance);
                        var lblResult = typeof(Form1).GetField("lblKetQuaTinhTien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                        var lblRealtimeSummary = typeof(Form1).GetField("lblRealtimeSummary", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                        var cboKhachHang = typeof(Form1).GetField("cboKhachHang", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as ComboBox;

                        Assert.NotNull(stationButtons);
                        Assert.NotNull(stations);
                        Assert.NotNull(selectMethod);
                        Assert.NotNull(lblResult);
                        Assert.NotNull(lblRealtimeSummary);
                        Assert.NotNull(cboKhachHang);

                        // Member must not see other users' balances in cboKhachHang
                        Assert.Single(cboKhachHang!.Items);
                        Assert.Contains(viewingMember.Username, cboKhachHang.Items[0]?.ToString());

                        var tempInUseStation = stations!.FirstOrDefault(s => s.StationId == stationId);
                        Assert.NotNull(tempInUseStation);
                        Assert.True(stationButtons!.TryGetValue(stationId, out Button? inUseBtn));

                        selectMethod!.Invoke(form1, new object[] { tempInUseStation! });
                        string initialSummary = lblRealtimeSummary!.Text;
                        string initialBtnText = inUseBtn!.Text;

                        form1.Show();

                        // Pump real WinForms message loop until _realtimeTimer ticks at least once (>1s)
                        DateTime deadline = DateTime.UtcNow.AddMilliseconds(2300);
                        while (DateTime.UtcNow < deadline)
                        {
                            Application.DoEvents();
                            Thread.Sleep(50);
                            if (lblRealtimeSummary.Text != initialSummary && inUseBtn.Text != initialBtnText)
                                break;
                        }

                        Assert.NotEqual(initialSummary, lblRealtimeSummary.Text);
                        Assert.NotEqual(initialBtnText, inUseBtn.Text);
                        Assert.Contains("⏱️", inUseBtn.Text);
                        Assert.Contains("[ĐANG CHƠI]", inUseBtn.Text);
                        Assert.Contains("Thời lượng:", lblResult!.Text);
                        Assert.DoesNotContain(playingMember.Username, lblResult.Text);
                        Assert.DoesNotContain("60,000", lblResult.Text);

                        form1.Close();
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
            finally
            {
                CleanupTemporaryArtifacts(stationId, playingMember.UserId);
                CleanupTemporaryArtifacts(0, viewingMember.UserId);
            }
        }

        [Fact]
        public void Form1_And_FormLogin_Layout_HaveNoOverlapOrClippingAtDefaultAndMinimumSize()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    var dbUser = new DBUser();
                    var admin = dbUser.GetByUsername("admin");
                    Assert.NotNull(admin);
                    CurrentUserSession.SetUser(admin!);

                    using (var loginForm = new FormLogin
                    {
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        Location = new System.Drawing.Point(-2000, -2000)
                    })
                    {
                        loginForm.Show();
                        Application.DoEvents();

                        var pnlCard = typeof(FormLogin).GetField("pnlCard", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as Panel;
                        var btnLogin = typeof(FormLogin).GetField("btnLogin", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as Button;
                        var btnExit = typeof(FormLogin).GetField("btnExit", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as Button;
                        var lblRoleHint = typeof(FormLogin).GetField("lblRoleHint", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as Label;

                        Assert.NotNull(pnlCard);
                        Assert.NotNull(btnLogin);
                        Assert.NotNull(btnExit);
                        Assert.NotNull(lblRoleHint);
                        Assert.True(btnLogin!.Right < btnExit!.Left);
                        Assert.True(btnExit.Right <= pnlCard!.ClientSize.Width);
                        Assert.True(lblRoleHint!.Bottom <= pnlCard.ClientSize.Height);
                        loginForm.Close();
                    }

                    using (var form1 = new Form1
                    {
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        Location = new System.Drawing.Point(-2000, -2000)
                    })
                    {
                        form1.Show();
                        Application.DoEvents();

                        void AssertLayoutClean()
                        {
                            var lblMainTitle = typeof(Form1).GetField("lblMainTitle", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                            var lblLeaderRole = typeof(Form1).GetField("lblLeaderRole", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                            var lblCurrentUser = typeof(Form1).GetField("lblCurrentUser", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                            var btnDangXuat = typeof(Form1).GetField("btnDangXuat", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var btnNavBilling = typeof(Form1).GetField("btnNavBilling", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var btnLamMoiSoDo = typeof(Form1).GetField("btnLamMoiSoDo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var lblLegendTournament = typeof(Form1).GetField("lblLegendTournament", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                            var lblRealtimeSummary = typeof(Form1).GetField("lblRealtimeSummary", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;
                            var grpDieuKhienPhien = typeof(Form1).GetField("grpDieuKhienPhien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as GroupBox;
                            var lblKetQuaTinhTien = typeof(Form1).GetField("lblKetQuaTinhTien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Label;

                            Assert.True(lblMainTitle!.Right < lblCurrentUser!.Left, $"lblMainTitle.Right ({lblMainTitle.Right}) >= lblCurrentUser.Left ({lblCurrentUser.Left})");
                            Assert.True(lblLeaderRole!.Right < lblCurrentUser.Left, $"lblLeaderRole.Right ({lblLeaderRole.Right}) >= lblCurrentUser.Left ({lblCurrentUser.Left})");
                            Assert.True(lblCurrentUser.Right < btnDangXuat!.Left);
                            Assert.True(btnDangXuat.Right <= form1.ClientSize.Width);
                            Assert.True(btnNavBilling!.Right < btnLamMoiSoDo!.Left, $"btnNavBilling.Right ({btnNavBilling.Right}) >= btnLamMoiSoDo.Left ({btnLamMoiSoDo.Left})");
                            Assert.True(btnLamMoiSoDo.Right <= form1.ClientSize.Width, $"btnLamMoiSoDo.Right ({btnLamMoiSoDo.Right}) > ClientWidth ({form1.ClientSize.Width})");
                            Assert.True(lblLegendTournament!.Right < lblRealtimeSummary!.Left, $"lblLegendTournament.Right ({lblLegendTournament.Right}) >= lblRealtimeSummary.Left ({lblRealtimeSummary.Left})");
                            Assert.True(lblRealtimeSummary.Height >= lblRealtimeSummary.PreferredSize.Height, $"lblRealtimeSummary.Height ({lblRealtimeSummary.Height}) < Pref ({lblRealtimeSummary.PreferredSize.Height})");
                            Assert.True(lblKetQuaTinhTien!.Bottom <= grpDieuKhienPhien!.Height, $"lblKetQuaTinhTien.Bottom ({lblKetQuaTinhTien.Bottom}) > grpHeight ({grpDieuKhienPhien.Height})");
                        }

                        AssertLayoutClean();

                        form1.Size = form1.MinimumSize;
                        Application.DoEvents();
                        AssertLayoutClean();

                        form1.Close();
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

        [Fact]
        public void FormLogin_Click_And_Form1_StationClickHandlers_ExecuteEndToEnd()
        {
            var dbUser = new DBUser();
            var dbStation = new DBStation();
            int stationId = CreateTemporaryStation("UIC");
            var operatorUser = new AppUser
            {
                Username = $"opr_{Guid.NewGuid():N}"[..15],
                FullName = "Operator Staff",
                Role = "Staff",
                Balance = 0m,
                MemberTier = "Standard"
            };
            var gamerUser = new AppUser
            {
                Username = $"gmr_{Guid.NewGuid():N}"[..15],
                FullName = "UI Flow Gamer",
                Role = "Member",
                Balance = 10000m,
                MemberTier = "Standard"
            };

            Exception? threadException = null;

            try
            {
                Assert.True(dbUser.CreateMember(operatorUser, "OpPass#1", out string err1), err1);
                Assert.True(dbUser.CreateMember(gamerUser, "GmPass#1", out string err2), err2);

                Thread staThread = new(() =>
                {
                    try
                    {
                        CurrentUserSession.Clear();

                        // 1. Real FormLogin btnLogin click flow
                        using (var loginForm = new FormLogin
                        {
                            ShowInTaskbar = false,
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(-2000, -2000)
                        })
                        {
                            var userBox = typeof(FormLogin).GetField("txtUsername", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as TextBox;
                            var passBox = typeof(FormLogin).GetField("txtPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as TextBox;
                            var btnLogin = typeof(FormLogin).GetField("btnLogin", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(loginForm) as Button;

                            loginForm.Show();
                            userBox!.Text = operatorUser.Username;
                            passBox!.Text = "OpPass#1";
                            btnLogin!.PerformClick();

                            Assert.Equal(DialogResult.OK, loginForm.DialogResult);
                            Assert.True(CurrentUserSession.IsLoggedIn);
                            Assert.Equal(operatorUser.UserId, CurrentUserSession.UserId);
                        }

                        // 2. Real Form1 station open / maintenance / close / logout click flows
                        using (var form1 = new Form1
                        {
                            ShowInTaskbar = false,
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(-2000, -2000)
                        })
                        {
                            DialogResult nextConfirmResult = DialogResult.Yes;
                            string lastCaption = string.Empty;
                            form1.SetPromptHandler((text, caption, buttons, icon) =>
                            {
                                lastCaption = caption;
                                return buttons == MessageBoxButtons.YesNo ? nextConfirmResult : DialogResult.OK;
                            });

                            var stationButtons = typeof(Form1).GetField("_stationButtons", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1)
                                as System.Collections.Generic.Dictionary<int, Button>;
                            var cboKhachHang = typeof(Form1).GetField("cboKhachHang", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as ComboBox;
                            var cboGoiCuoc = typeof(Form1).GetField("cboGoiCuoc", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as ComboBox;
                            var btnMoMay = typeof(Form1).GetField("btnMoMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var btnDongMay = typeof(Form1).GetField("btnDongMay", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var btnBaoTri = typeof(Form1).GetField("btnBaoTri", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var btnNavBilling = typeof(Form1).GetField("btnNavBilling", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;
                            var btnDangXuat = typeof(Form1).GetField("btnDangXuat", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form1) as Button;

                            form1.Show();
                            Application.DoEvents();

                            // Staff cannot access Admin-only Billing module even if handler invoked
                            Assert.False(btnNavBilling!.Enabled);
                            var navClickMethod = typeof(Control).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance);
                            navClickMethod!.Invoke(btnNavBilling, new object[] { EventArgs.Empty });
                            Assert.Equal("Giới hạn phân quyền", lastCaption);

                            Assert.True(stationButtons!.TryGetValue(stationId, out Button? tempStationBtn));
                            tempStationBtn!.PerformClick();

                            // Toggle maintenance on Available -> Maintenance -> Available
                            btnBaoTri!.PerformClick();
                            Assert.Equal("Maintenance", dbStation.GetById(stationId)?.Status);
                            btnBaoTri.PerformClick();
                            Assert.Equal("Available", dbStation.GetById(stationId)?.Status);

                            // Select gamerUser and NightCombo (45,000 VND > 10,000 balance)
                            int itemIdx = -1;
                            for (int i = 0; i < cboKhachHang!.Items.Count; i++)
                            {
                                if (cboKhachHang.Items[i]?.ToString()?.Contains(gamerUser.Username, StringComparison.Ordinal) == true)
                                {
                                    itemIdx = i;
                                    break;
                                }
                            }
                            Assert.True(itemIdx > 0);
                            cboKhachHang.SelectedIndex = itemIdx;
                            cboGoiCuoc!.SelectedIndex = 1; // NightCombo

                            btnMoMay!.PerformClick();
                            Assert.Equal("Mở máy thành công", lastCaption);
                            Assert.Equal("InUse", dbStation.GetById(stationId)?.Status);

                            // Close with No -> keeps session Active and preserves balance
                            nextConfirmResult = DialogResult.No;
                            btnDongMay!.PerformClick();
                            Assert.Equal("Số dư ví không đủ", lastCaption);
                            Assert.Equal("InUse", dbStation.GetById(stationId)?.Status);
                            Assert.Equal(10000m, dbUser.GetById(gamerUser.UserId)?.Balance);

                            // Close with Yes -> drains wallet to 0 + cash supplement
                            nextConfirmResult = DialogResult.Yes;
                            btnDongMay.PerformClick();
                            Assert.Equal("Thanh toán hoàn tất", lastCaption);
                            Assert.Equal("Available", dbStation.GetById(stationId)?.Status);
                            Assert.Equal(0m, dbUser.GetById(gamerUser.UserId)?.Balance);

                            // Logout click sets IsLogoutRequested
                            nextConfirmResult = DialogResult.Yes;
                            btnDangXuat!.PerformClick();
                            Assert.True(form1.IsLogoutRequested);
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
            finally
            {
                CleanupTemporaryArtifacts(stationId, gamerUser.UserId);
                CleanupTemporaryArtifacts(0, operatorUser.UserId);
            }
        }
    }
}
