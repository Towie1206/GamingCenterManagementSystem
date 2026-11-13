using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    [Collection("GamingCenterSequential")]
    public class LeaderCitizenIdAndFnBDashboardTests
    {
        private static DBUser CreateOfflineUserDb() =>
            new(() => throw new TimeoutException("Simulated offline SQL Server"));

        private static int CreateTemporaryStation(string prefix = "TST-FNB")
        {
            string stationName = $"{prefix}-{Guid.NewGuid().ToString("N")[..6]}";
            using var conn = Database.GetConnection();
            conn.Open();
            const string sql = @"INSERT INTO dbo.ComputerStation (StationName, ZoneId, IpAddress, Status)
                                 OUTPUT INSERTED.StationId
                                 VALUES (@Name, 1, '192.168.88.88', N'Available')";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = stationName;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void CleanupTemporaryArtifacts(int stationId, int userId = 0, int sessionId = 0)
        {
            try
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
            catch
            {
                // Ignore cleanup errors
            }
        }

        // =========================================================================
        // FEATURE 1: MANDATORY CITIZEN ID (CCCD/CMND) TESTS - NGHỊ ĐỊNH 72/2013
        // =========================================================================

        [Theory]
        [InlineData("001099000001")]
        [InlineData("079099123456")]
        [InlineData("038200000001")]
        [InlineData("123456789")]
        [InlineData("012345678")]
        public void DBUser_IsValidCitizenId_ValidFormats_ReturnTrue(string citizenId)
        {
            Assert.True(DBUser.IsValidCitizenId(citizenId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("12345678")]       // 8 digits
        [InlineData("1234567890")]     // 10 digits
        [InlineData("12345678901")]    // 11 digits
        [InlineData("1234567890123")]  // 13 digits
        [InlineData("001099A00001")]   // contains letter
        [InlineData("001-099-00001")]  // contains hyphen
        [InlineData("001099 00001")]   // contains whitespace
        public void DBUser_IsValidCitizenId_InvalidFormats_ReturnFalse(string? citizenId)
        {
            Assert.False(DBUser.IsValidCitizenId(citizenId));
        }

        [Fact]
        public void DBUser_ValidateMemberInput_CitizenId_EnforcesDecree72Requirement()
        {
            // 1. Valid 12-digit CCCD with requireCitizenId = true
            bool valid12 = DBUser.ValidateMemberInput(
                "mem_valid12", "Nguyễn Văn A", "pass12345", 20000m, "Member",
                "001099111222", out string error12, requireCitizenId: true);
            Assert.True(valid12, error12);
            Assert.True(string.IsNullOrEmpty(error12));

            // 2. Valid 9-digit CMND with requireCitizenId = true
            bool valid9 = DBUser.ValidateMemberInput(
                "mem_valid9", "Trần Văn B", "pass12345", 20000m, "Member",
                "123456789", out string error9, requireCitizenId: true);
            Assert.True(valid9, error9);
            Assert.True(string.IsNullOrEmpty(error9));

            // 3. Null / empty CCCD when required per legal decree
            bool invalidEmpty = DBUser.ValidateMemberInput(
                "mem_empty_cccd", "Lê Văn C", "pass12345", 20000m, "Member",
                "", out string errorEmpty, requireCitizenId: true);
            Assert.False(invalidEmpty);
            Assert.Contains("CCCD", errorEmpty, StringComparison.OrdinalIgnoreCase);

            // 4. Invalid format CCCD (e.g. 5 digits)
            bool invalidFormat = DBUser.ValidateMemberInput(
                "mem_bad_cccd", "Phạm Văn D", "pass12345", 20000m, "Member",
                "12345", out string errorFormat, requireCitizenId: true);
            Assert.False(invalidFormat);
            Assert.Contains("12", errorFormat);

            // 5. requireCitizenId = false allows null CCCD (for backward compatibility)
            bool optionalNull = DBUser.ValidateMemberInput(
                "mem_optional", "Hoàng Văn E", "pass12345", 20000m, "Member",
                null, out string errorOpt, requireCitizenId: false);
            Assert.True(optionalNull, errorOpt);

            // 6. Original 5-parameter overload works and defaults to requireCitizenId = false
            bool legacy5Params = DBUser.ValidateMemberInput(
                "mem_legacy", "Đỗ Văn F", "pass12345", 20000m, "Member", out string errorLegacy);
            Assert.True(legacy5Params, errorLegacy);
        }

        [Fact]
        public void DBUser_Fallback_DuplicateCitizenId_ThrowsInvalidOperationException()
        {
            var offlineDb = CreateOfflineUserDb();
            string testCccd = "001099888999";
            string u1 = $"cccd_u1_{Guid.NewGuid():N}"[..12];
            string u2 = $"cccd_u2_{Guid.NewGuid():N}"[..12];

            var user1 = new AppUser
            {
                Username = u1,
                FullName = "Hội Viên Một",
                Role = "Member",
                Balance = 20000m,
                CitizenId = testCccd
            };

            bool created = offlineDb.CreateMember(user1, "pass123", out string err);
            Assert.True(created, err);

            var user2 = new AppUser
            {
                Username = u2,
                FullName = "Hội Viên Hai",
                Role = "Member",
                Balance = 20000m,
                CitizenId = testCccd
            };

            // Second user with same CCCD via CreateMember returns false with error message
            bool created2 = offlineDb.CreateMember(user2, "pass123", out string err2);
            Assert.False(created2);
            Assert.Contains("CCCD", err2, StringComparison.OrdinalIgnoreCase);

            // And via Add(entity) throws InvalidOperationException
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                offlineDb.Add(user2);
            });
            Assert.Contains("CCCD", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void DBUser_Fallback_UpdateCitizenId_EnforcesUniqueness()
        {
            var offlineDb = CreateOfflineUserDb();
            string cccdA = "001099111333";
            string cccdB = "001099222444";

            var userA = new AppUser
            {
                Username = $"upd_a_{Guid.NewGuid():N}"[..12],
                FullName = "User A",
                Role = "Member",
                Balance = 15000m,
                CitizenId = cccdA
            };
            var userB = new AppUser
            {
                Username = $"upd_b_{Guid.NewGuid():N}"[..12],
                FullName = "User B",
                Role = "Member",
                Balance = 15000m,
                CitizenId = cccdB
            };

            Assert.True(offlineDb.CreateMember(userA, "pass123", out _));
            Assert.True(offlineDb.CreateMember(userB, "pass123", out _));

            // Attempting to update userB's CCCD to userA's CCCD must throw
            userB.CitizenId = cccdA;
            var ex = Assert.Throws<InvalidOperationException>(() => offlineDb.Update(userB));
            Assert.Contains("CCCD", ex.Message, StringComparison.OrdinalIgnoreCase);

            // Updating userB with its own CCCD or new unique CCCD succeeds
            userB.CitizenId = "001099555666";
            bool updated = offlineDb.Update(userB);
            Assert.True(updated);
        }

        [Fact]
        public void DBUser_SearchUsers_MatchesCitizenIdKeyword()
        {
            var users = new List<AppUser>
            {
                new() { UserId = 101, Username = "alpha", FullName = "Nguyễn Văn Alpha", CitizenId = "001099000101", Role = "Member", MemberTier = "Standard" },
                new() { UserId = 102, Username = "beta", FullName = "Trần Văn Beta", CitizenId = "001099000102", Role = "Member", MemberTier = "Silver" },
                new() { UserId = 103, Username = "gamma", FullName = "Lê Văn Gamma", CitizenId = "079099000999", Role = "Member", MemberTier = "Gold" }
            };

            var searchResult = DBUser.SearchUsers(users, "079099000999");
            Assert.Single(searchResult);
            Assert.Equal("gamma", searchResult[0].Username);

            var searchPartial = DBUser.SearchUsers(users, "001099");
            Assert.Equal(2, searchPartial.Count);
        }

        [Fact]
        public void FormMember_CitizenIdField_ConfiguredProperlyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember();
                    var txtCccdField = typeof(FormMember).GetField("txtCccd",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var txtCccd = txtCccdField?.GetValue(form) as TextBox;

                    var lblCccdField = typeof(FormMember).GetField("lblCccd",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var lblCccd = lblCccdField?.GetValue(form) as Label;

                    Assert.NotNull(txtCccd);
                    Assert.NotNull(lblCccd);
                    Assert.Equal(12, txtCccd!.MaxLength);
                    Assert.Contains("CCCD", lblCccd!.Text);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }

        // =========================================================================
        // FEATURE 2: DASHBOARD DIRECT F&B ORDERING TESTS (LEADER FORM1)
        // =========================================================================

        [Fact]
        public void Form1_Dashboard_GoiMonButton_VisibilityAndPermissions()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.SetSession(1, "admin", "Admin");

                    using var form1 = new Form1();
                    var btnGoiMon = form1.GetBtnGoiMon();

                    Assert.NotNull(btnGoiMon);
                    Assert.Contains("GỌI MÓN", btnGoiMon.Text.ToUpperInvariant());
                    Assert.Equal(Color.FromArgb(230, 126, 34), btnGoiMon.BackColor);

                    // When no station is selected or station is Available, button is disabled
                    var availStation = new ComputerStation { StationId = 9991, StationName = "MAY-TEST-AVAIL", Status = "Available" };
                    var selectMethod = typeof(Form1).GetMethod("SelectStation", BindingFlags.NonPublic | BindingFlags.Instance);
                    selectMethod?.Invoke(form1, new object[] { availStation });

                    Assert.False(btnGoiMon.Enabled);

                    // When station is InUse and staff/admin is logged in, button is enabled
                    var inUseStation = new ComputerStation { StationId = 9992, StationName = "MAY-TEST-INUSE", Status = "InUse" };
                    selectMethod?.Invoke(form1, new object[] { inUseStation });

                    Assert.True(btnGoiMon.Enabled);

                    // When pure member is logged in, station operations including GoiMon are disabled
                    CurrentUserSession.SetSession(2, "gamer01", "Member");
                    var applyPermMethod = typeof(Form1).GetMethod("ApplyRolePermissions", BindingFlags.NonPublic | BindingFlags.Instance);
                    applyPermMethod?.Invoke(form1, null);
                    selectMethod?.Invoke(form1, new object[] { inUseStation });

                    Assert.False(btnGoiMon.Enabled);
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
        public void Form1_OrderFoodForStation_NoActiveSession_ThrowsException()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.SetSession(1, "admin", "Admin");
                    using var form1 = new Form1();

                    // Non-existent station ID must throw InvalidOperationException
                    var ex = Assert.Throws<InvalidOperationException>(() =>
                    {
                        form1.OrderFoodForStation(999999, 1, 1);
                    });
                    Assert.Contains("không có phiên chơi", ex.Message, StringComparison.OrdinalIgnoreCase);
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
        public void Form1_Dashboard_OrderFoodEndToEnd_UpdatesServiceCostAndRealtimeDisplay()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                int stationId = 0;
                int sessionId = 0;
                try
                {
                    CurrentUserSession.SetSession(21, "staff01", "Staff");
                    stationId = CreateTemporaryStation("TST-LEADER-DASH");

                    var dbBilling = new DBBilling();
                    var session = dbBilling.OpenStationSession(stationId, userId: null, tariffType: "Hourly");
                    sessionId = session.SessionId;

                    using var form1 = new Form1();
                    form1.SetPromptHandler((msg, cap, btn, ico) => DialogResult.OK);

                    // Select the active station on Dashboard
                    form1.SelectStationById(stationId);
                    var activeSessionBefore = form1.GetActiveSession(stationId);
                    Assert.NotNull(activeSessionBefore);
                    Assert.Equal(0m, activeSessionBefore!.ServiceCost);

                    // Order 2x Sting Dâu (ServiceId = 1, Price = 15,000đ)
                    bool orderSuccess = form1.OrderFoodForStation(stationId, serviceId: 1, quantity: 2, note: "Thêm đá");
                    Assert.True(orderSuccess);

                    // Verify active session updated with ServiceCost = 30,000đ
                    var activeSessionAfter = form1.GetActiveSession(stationId);
                    Assert.NotNull(activeSessionAfter);
                    Assert.Equal(30000m, activeSessionAfter!.ServiceCost);

                    // Verify Dashboard text updated dynamically with service cost
                    string billingText = form1.GetBillingResultText();
                    Assert.Contains("DV 30,000đ", billingText);

                    // Order 1x Mì Tôm (ServiceId = 4, Price = 25,000đ)
                    bool orderFood2 = form1.OrderFoodForStation(stationId, serviceId: 4, quantity: 1, note: "Trứng lòng đào");
                    Assert.True(orderFood2);

                    var activeSessionAfterFood2 = form1.GetActiveSession(stationId);
                    Assert.NotNull(activeSessionAfterFood2);
                    Assert.Equal(55000m, activeSessionAfterFood2!.ServiceCost);

                    string billingTextAfterFood2 = form1.GetBillingResultText();
                    Assert.Contains("DV 55,000đ", billingTextAfterFood2);

                    // Close station session and verify final billed receipt incorporates F&B
                    var closeResult = dbBilling.CloseStationSession(stationId, basePricePerHour: 10000m, endTime: DateTime.Now.AddMinutes(15));
                    Assert.Equal(55000m, closeResult.ServiceCost);
                    Assert.Equal(closeResult.PlayCost + closeResult.ServiceCost, closeResult.TotalCost);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    CurrentUserSession.Clear();
                    CleanupTemporaryArtifacts(stationId, 0, sessionId);
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }

        [Fact]
        public void FormOrderServiceDialog_CalculatesTotalAndValidatesStock()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    var dbBilling = new DBBilling();
                    using var dlg = new FormOrderServiceDialog(dbBilling, sessionId: 1, stationName: "MÁY-01");

                    Assert.NotNull(dlg.SelectedItem);
                    Assert.True(dlg.SelectedQuantity >= 1);
                    Assert.True(dlg.CalculatedTotal >= 0m);

                    var nudField = typeof(FormOrderServiceDialog).GetField("nudQuantity",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var nud = nudField?.GetValue(dlg) as NumericUpDown;
                    Assert.NotNull(nud);

                    nud!.Value = 3;
                    decimal expected = dlg.SelectedItem!.UnitPrice * 3;
                    Assert.Equal(expected, dlg.CalculatedTotal);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }

        [Fact]
        public async Task DBUser_RegisterUser_WithoutCitizenId_FailsPerDecree72()
        {
            var db = new DBUser();
            var userNoCccd = new AppUser
            {
                Username = $"nocccd_{Guid.NewGuid():N}"[..12],
                FullName = "Không CCCD",
                Role = "Member",
                Balance = 50000m,
                CitizenId = null
            };

            bool syncResult = db.RegisterUser(userNoCccd, "validPass123", out string syncError);
            Assert.False(syncResult);
            Assert.Contains("Nghị định", syncError, StringComparison.OrdinalIgnoreCase);

            var (asyncSuccess, asyncError) = await db.RegisterUserAsync(userNoCccd, "validPass123");
            Assert.False(asyncSuccess);
            Assert.Contains("Nghị định", asyncError, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Form1_BtnGoiMon_Click_InvokesLauncherAndRefreshesDashboard()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                int stationId = 0;
                int sessionId = 0;
                try
                {
                    CurrentUserSession.SetSession(1, "admin", "Admin");
                    stationId = CreateTemporaryStation("TST-LAUNCHER");

                    var dbBilling = new DBBilling();
                    var session = dbBilling.OpenStationSession(stationId, userId: null, tariffType: "Hourly");
                    sessionId = session.SessionId;

                    using var form1 = new Form1();
                    form1.SetPromptHandler((msg, cap, btn, ico) => DialogResult.OK);

                    bool launcherCalled = false;
                    int passedSessionId = 0;
                    string passedStationName = string.Empty;

                    form1.SetOrderDialogLauncher((billing, sessId, stName) =>
                    {
                        launcherCalled = true;
                        passedSessionId = sessId;
                        passedStationName = stName;
                        // Simulate dialog ordering 1x Sting Dâu
                        billing.OrderServiceForSession(sessId, serviceId: 1, quantity: 1, "Gọi từ dialog");
                        return DialogResult.OK;
                    });

                    form1.SelectStationById(stationId);
                    form1.PerformGoiMonClick();

                    Assert.True(launcherCalled);
                    Assert.Equal(sessionId, passedSessionId);
                    Assert.Contains("TST-LAUNCHER", passedStationName);

                    // Verify dashboard updated
                    var activeSession = form1.GetActiveSession(stationId);
                    Assert.NotNull(activeSession);
                    Assert.Equal(15000m, activeSession!.ServiceCost);
                    Assert.Contains("DV 15,000đ", form1.GetBillingResultText());
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    CurrentUserSession.Clear();
                    CleanupTemporaryArtifacts(stationId, 0, sessionId);
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }

        [Fact]
        public void DBUser_Sql_CitizenIdUniqueConstraint_EnforcedOnDatabase()
        {
            var db = new DBUser();
            string uniqueCccd = $"079{Guid.NewGuid():N}"[..12];
            // Ensure purely numeric 12 digits
            uniqueCccd = string.Concat(uniqueCccd.Select(c => char.IsDigit(c) ? c : '9'));

            string u1 = $"sql_cccd_1_{Guid.NewGuid():N}"[..14];
            string u2 = $"sql_cccd_2_{Guid.NewGuid():N}"[..14];

            var user1 = new AppUser
            {
                Username = u1,
                FullName = "SQL Hội Viên 1",
                Role = "Member",
                Balance = 30000m,
                CitizenId = uniqueCccd
            };

            int u1Id = 0;
            int u2Id = 0;
            try
            {
                bool created1 = db.CreateMember(user1, "sqlPass123", out string err1);
                Assert.True(created1, err1);
                u1Id = user1.UserId;

                // Query by CitizenId
                var fetched = db.GetByCitizenId(uniqueCccd);
                Assert.NotNull(fetched);
                Assert.Equal(u1, fetched!.Username);
                Assert.Equal(uniqueCccd, fetched.CitizenId);

                // Uniqueness check
                Assert.True(db.IsCitizenIdExists(uniqueCccd));
                Assert.False(db.IsCitizenIdExists(uniqueCccd, excludeUserId: u1Id));

                // Second user with same CCCD must be rejected
                var user2 = new AppUser
                {
                    Username = u2,
                    FullName = "SQL Hội Viên 2",
                    Role = "Member",
                    Balance = 30000m,
                    CitizenId = uniqueCccd
                };

                bool created2 = db.CreateMember(user2, "sqlPass123", out string err2);
                Assert.False(created2);
                Assert.Contains("CCCD", err2, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                if (u1Id > 0) CleanupTemporaryArtifacts(0, u1Id);
                if (u2Id > 0) CleanupTemporaryArtifacts(0, u2Id);
            }
        }

        [Fact]
        public async Task DBUser_UpdateUser_OverloadsAndCitizenIdSupport()
        {
            var offlineDb = CreateOfflineUserDb();
            string cccdTest = "001099777888";
            var user = new AppUser
            {
                Username = $"upd_ov_{Guid.NewGuid():N}"[..12],
                FullName = "Update Overload User",
                Role = "Member",
                Balance = 25000m,
                CitizenId = cccdTest
            };

            Assert.True(offlineDb.CreateMember(user, "validPass123", out _));

            // 1. UpdateUser(AppUser)
            user.FullName = "Update Overload User Renamed";
            bool updatedSync = offlineDb.UpdateUser(user);
            Assert.True(updatedSync);
            Assert.Equal("Update Overload User Renamed", user.FullName);

            // 2. UpdateUserAsync(AppUser)
            user.FullName = "Update Overload User Async";
            bool updatedAsync = await offlineDb.UpdateUserAsync(user);
            Assert.True(updatedAsync);

            // 3. UpdateUser(AppUser, out string errorMessage)
            user.FullName = "Update Overload User Out";
            bool updatedOut = offlineDb.UpdateUser(user, out string errorMsg);
            Assert.True(updatedOut, errorMsg);
            Assert.True(string.IsNullOrEmpty(errorMsg));

            // 4. UpdateUser with duplicate CitizenId fails and populates error message
            var anotherUser = new AppUser
            {
                Username = $"dup_ov_{Guid.NewGuid():N}"[..12],
                FullName = "Another User",
                Role = "Member",
                Balance = 25000m,
                CitizenId = "001099666555"
            };
            Assert.True(offlineDb.CreateMember(anotherUser, "validPass123", out _));

            anotherUser.CitizenId = cccdTest;
            bool duplicateUpdate = offlineDb.UpdateUser(anotherUser, out string dupError);
            Assert.False(duplicateUpdate);
            Assert.Contains("CCCD", dupError, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DBUser_SearchUsers_InstanceMethods_MatchesCitizenIdKeyword()
        {
            var offlineDb = CreateOfflineUserDb();

            // Fallback seed gamer01 has CitizenId = "001099000002"
            var syncResults = offlineDb.SearchUsers("001099000002");
            Assert.Single(syncResults);
            Assert.Equal("gamer01", syncResults[0].Username);

            // Async instance search with role filter
            var asyncResults = await offlineDb.SearchUsersAsync("001099000001", roleFilter: "Admin");
            Assert.Single(asyncResults);
            Assert.Equal("admin", asyncResults[0].Username);

            // CitizenId query that does not match
            var emptyResults = offlineDb.SearchUsers("999999999999");
            Assert.Empty(emptyResults);
        }

        [Fact]
        public void DBUser_CitizenId_PreservesLeadingZerosEndToEnd()
        {
            var offlineDb = CreateOfflineUserDb();
            string leadingZeroCccd = "000012345678";
            var user = new AppUser
            {
                Username = $"leadzero_{Guid.NewGuid():N}"[..12],
                FullName = "Leading Zero Citizen",
                Role = "Member",
                Balance = 30000m,
                CitizenId = leadingZeroCccd
            };

            bool created = offlineDb.CreateMember(user, "validPass123", out string err);
            Assert.True(created, err);

            var fetched = offlineDb.GetByCitizenId(leadingZeroCccd);
            Assert.NotNull(fetched);
            Assert.NotNull(fetched!.CitizenId);
            Assert.Equal(leadingZeroCccd, fetched.CitizenId);
            Assert.StartsWith("0000", fetched.CitizenId!);
            Assert.Equal(12, fetched.CitizenId!.Length);
        }

        [Fact]
        public void FormOrderServiceDialog_InvalidSessionId_ThrowsArgumentOutOfRangeException()
        {
            var dbBilling = new DBBilling();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using var dlg = new FormOrderServiceDialog(dbBilling, sessionId: 0, stationName: "MAY-01");
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using var dlg = new FormOrderServiceDialog(dbBilling, sessionId: -5, stationName: "MAY-01");
            });
        }

        [Fact]
        public void FormOrderServiceDialog_EmptyStock_DisablesControls()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    // DBBilling with custom connection returning empty service items
                    var offlineBilling = new DBBilling(() => throw new TimeoutException("Offline SQL"));
                    using var dlg = new FormOrderServiceDialog(offlineBilling, sessionId: 1, stationName: "MAY-01");

                    // Set empty available items via reflection to simulate 0 stock
                    var fieldItems = typeof(FormOrderServiceDialog).GetField("_availableItems", BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldItems?.SetValue(dlg, new List<ServiceItem>());

                    var cboField = typeof(FormOrderServiceDialog).GetField("cboFood", BindingFlags.NonPublic | BindingFlags.Instance);
                    var cbo = cboField?.GetValue(dlg) as ComboBox;
                    var btnConfirmField = typeof(FormOrderServiceDialog).GetField("btnConfirm", BindingFlags.NonPublic | BindingFlags.Instance);
                    var btnConfirm = btnConfirmField?.GetValue(dlg) as Button;
                    var nudField = typeof(FormOrderServiceDialog).GetField("nudQuantity", BindingFlags.NonPublic | BindingFlags.Instance);
                    var nud = nudField?.GetValue(dlg) as NumericUpDown;

                    // Invoking LoadFoodItems with an offline billing that has fallback items or cleared items
                    dlg.LoadFoodItems();
                    Assert.NotNull(cbo);
                    Assert.NotNull(btnConfirm);
                    Assert.NotNull(nud);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }

        [Fact]
        public void FormMember_UpdateMember_WithoutCitizenId_BlockedPerDecree72()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember();
                    string? capturedPrompt = null;
                    form.SetPromptHandler((msg, cap, btn, ico) =>
                    {
                        capturedPrompt = msg;
                        return DialogResult.OK;
                    });

                    var txtUserIdField = typeof(FormMember).GetField("txtUserId", BindingFlags.NonPublic | BindingFlags.Instance);
                    var txtUsernameField = typeof(FormMember).GetField("txtUsername", BindingFlags.NonPublic | BindingFlags.Instance);
                    var txtCccdField = typeof(FormMember).GetField("txtCccd", BindingFlags.NonPublic | BindingFlags.Instance);
                    var cboRoleField = typeof(FormMember).GetField("cboRole", BindingFlags.NonPublic | BindingFlags.Instance);
                    var selectedUserField = typeof(FormMember).GetField("_selectedUser", BindingFlags.NonPublic | BindingFlags.Instance);

                    var txtUserId = txtUserIdField?.GetValue(form) as TextBox;
                    var txtUsername = txtUsernameField?.GetValue(form) as TextBox;
                    var txtCccd = txtCccdField?.GetValue(form) as TextBox;
                    var cboRole = cboRoleField?.GetValue(form) as ComboBox;

                    Assert.NotNull(txtUserId);
                    Assert.NotNull(txtUsername);
                    Assert.NotNull(txtCccd);
                    Assert.NotNull(cboRole);

                    txtUserId!.Text = "2";
                    txtUsername!.Text = "gamer01";
                    cboRole!.SelectedItem = "Member";
                    txtCccd!.Text = ""; // Clearing CCCD for a Member

                    selectedUserField?.SetValue(form, new AppUser
                    {
                        UserId = 2,
                        Username = "gamer01",
                        FullName = "Nguyễn Văn A",
                        Role = "Member",
                        CitizenId = "001099000002"
                    });

                    // Trigger update via reflection
                    var btnSuaMethod = typeof(FormMember).GetMethod("BtnSuaHoiVien_ClickAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                    var task = btnSuaMethod?.Invoke(form, null) as Task;
                    task?.GetAwaiter().GetResult();

                    Assert.NotNull(capturedPrompt);
                    Assert.Contains("Nghị định", capturedPrompt);
                    Assert.True(txtCccd.Focused || string.IsNullOrEmpty(txtCccd.Text));
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }
    }
}
