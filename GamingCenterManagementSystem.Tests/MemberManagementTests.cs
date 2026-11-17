using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    [Collection("GamingCenterSequential")]
    public class MemberManagementTests
    {
        private static DBUser CreateOfflineDb() =>
            new(() => throw new TimeoutException("Simulated offline SQL Server"));

        [Fact]
        public void AppUser_NegativeBalance_ThrowsArgumentException()
        {
            var user = new AppUser();
            Assert.Throws<ArgumentException>(() => user.Balance = -1000m);
        }

        [Theory]
        [InlineData("Standard", 50000, "Standard")]
        [InlineData("Standard", 200000, "Silver")]
        [InlineData("Standard", 500000, "Gold")]
        [InlineData("Gold", 100000, "Gold")]
        public void AppUser_ResolveUpgradedTier_RespectsThresholdsWithoutDowngrade(
            string currentTier, decimal balance, string expectedTier)
        {
            Assert.Equal(expectedTier, AppUser.ResolveUpgradedTier(currentTier, balance));
        }

        [Fact]
        public void DBUser_HashPassword_ProducesUniqueSaltsAndVerifiesCorrectly()
        {
            string hash1 = DBUser.HashPassword("CyberPass123");
            string hash2 = DBUser.HashPassword("CyberPass123");

            Assert.StartsWith("PBKDF2$100000$", hash1);
            Assert.NotEqual(hash1, hash2);
            Assert.True(DBUser.VerifyPassword("CyberPass123", hash1));
            Assert.True(DBUser.VerifyPassword("CyberPass123", hash2));
            Assert.False(DBUser.VerifyPassword("WrongPass", hash1));
            Assert.False(DBUser.VerifyPassword("", hash1));
        }

        [Fact]
        public void DBUser_VerifyPassword_SupportsLegacyPlaintextAndRejectsMalformedHash()
        {
            Assert.True(DBUser.VerifyPassword("123456", "123456"));
            Assert.False(DBUser.VerifyPassword("654321", "123456"));
            Assert.False(DBUser.VerifyPassword("123456", "PBKDF2$notanumber$abc$def"));
            Assert.False(DBUser.VerifyPassword("123456", "PBKDF2$100000$invalidbase64$abc"));
        }

        [Fact]
        public void DBUser_SearchUsers_FiltersByKeywordAndTierCaseInsensitively()
        {
            var sample = new List<AppUser>
            {
                new() { UserId = 1, Username = "admin", FullName = "Quản Lý Cyber", Role = "Admin", MemberTier = "Standard" },
                new() { UserId = 2, Username = "nam_pro", FullName = "Nguyễn Thành Nam", Role = "Member", MemberTier = "Silver" },
                new() { UserId = 3, Username = "van_vip", FullName = "Hoàng Văn", Role = "Member", MemberTier = "Gold" }
            };

            var byName = DBUser.SearchUsers(sample, "thành nam");
            Assert.Single(byName);
            Assert.Equal("nam_pro", byName[0].Username);

            var byTier = DBUser.SearchUsers(sample, "", tierFilter: "Gold");
            Assert.Single(byTier);
            Assert.Equal("van_vip", byTier[0].Username);

            var all = DBUser.SearchUsers(sample, "   ", tierFilter: "Tất cả hạng");
            Assert.Equal(3, all.Count);
        }

        [Theory]
        [InlineData("", "Nguyen Van A", "secret123", 10000, "Member")]
        [InlineData("bad user", "Nguyen Van A", "secret123", 10000, "Member")]
        [InlineData("validuser", "", "secret123", 10000, "Member")]
        [InlineData("validuser", "Nguyen Van A", "12345", 10000, "Member")]
        [InlineData("validuser", "Nguyen Van A", "secret123", 9999, "Member")]
        [InlineData("validuser", "Nguyen Van A", "secret123", -1, "Member")]
        [InlineData("validuser", "Nguyen Van A", "secret123", 50000001, "Member")]
        [InlineData("validuser", "Nguyen Van A", "secret123", 10000, "SuperAdmin")]
        public void DBUser_ValidateMemberInput_RejectsInvalidMemberData(
            string username, string fullName, string password, decimal balance, string role)
        {
            bool valid = DBUser.ValidateMemberInput(username, fullName, password, balance, role, out string error);
            Assert.False(valid);
            Assert.False(string.IsNullOrWhiteSpace(error));
        }

        [Fact]
        public void DBUser_ValidateMemberInput_AllowsZeroBalanceForStaffAdmin_RequiresTenThousandForMember()
        {
            Assert.True(DBUser.ValidateMemberInput("staff01", "Nhân Viên Quầy", "staffpass", 0m, "Staff", out _));
            Assert.True(DBUser.ValidateMemberInput("admin02", "Quản Lý", "adminpass", 0m, "Admin", out _));
            Assert.False(DBUser.ValidateMemberInput("mem01", "Hội Viên", "mempass", 5000m, "Member", out _));
            Assert.True(DBUser.ValidateMemberInput("mem01", "Hội Viên", "mempass", 10000m, "Member", out _));
        }

        [Fact]
        public void DBUser_EndToEndMemberLifecycle_CreateDuplicateChangePasswordAndSafeDeduct()
        {
            var db = new DBUser();
            string uniqueUsername = $"test_{Guid.NewGuid():N}"[..16];
            var created = new AppUser
            {
                Username = uniqueUsername,
                FullName = "Nguyễn Test Hội Viên",
                Role = "Member",
                Balance = 250000m,
                MemberTier = "Standard"
            };

            try
            {
                // 1. Create member with auto Silver promotion & PBKDF2 hashing
                bool ok = db.CreateMember(created, "initPass123", out string createError);
                Assert.True(ok, createError);
                Assert.True(created.UserId > 0);
                Assert.Equal("Silver", created.MemberTier);
                Assert.StartsWith("PBKDF2$100000$", created.PasswordHash);

                // 2. Reject duplicate username
                bool duplicate = db.CreateMember(uniqueUsername, "Another Name", "initPass123", 20000m, out string dupError);
                Assert.False(duplicate);
                Assert.Contains("tồn tại", dupError, StringComparison.OrdinalIgnoreCase);

                // 3. Reject wrong old password, short password, or identical new password
                Assert.False(db.ChangePassword(uniqueUsername, "wrongPass", "newPass123", out _));
                Assert.False(db.ChangePassword(uniqueUsername, "initPass123", "123", out _));
                Assert.False(db.ChangePassword(uniqueUsername, "initPass123", "initPass123", out _));

                // 4. Succeed with valid password change and verify hash updated
                bool changed = db.ChangePassword(uniqueUsername, "initPass123", "updatedPass456", out string changeError);
                Assert.True(changed, changeError);

                var reloaded = db.GetById(created.UserId);
                Assert.NotNull(reloaded);
                Assert.False(DBUser.VerifyPassword("initPass123", reloaded.PasswordHash));
                Assert.True(DBUser.VerifyPassword("updatedPass456", reloaded.PasswordHash));

                // 5. DeductBalance fails safely when balance is insufficient and preserves exact balance
                Assert.False(db.DeductBalance(created.UserId, 999999m));
                var afterFailedDeduct = db.GetById(created.UserId);
                Assert.NotNull(afterFailedDeduct);
                Assert.Equal(250000m, afterFailedDeduct.Balance);

                // 6. DeductBalanceOrThrow throws InsufficientBalanceException when insufficient
                var ex = Assert.Throws<InsufficientBalanceException>(() => db.DeductBalanceOrThrow(created.UserId, 300000m));
                Assert.Equal(250000m, ex.CurrentBalance);
                Assert.Equal(300000m, ex.RequiredAmount);

                // 7. DeductBalance succeeds when sufficient
                Assert.True(db.DeductBalance(created.UserId, 50000m));
                var afterDeduct = db.GetById(created.UserId);
                Assert.NotNull(afterDeduct);
                Assert.Equal(200000m, afterDeduct.Balance);
            }
            finally
            {
                if (created.UserId > 0)
                {
                    db.Delete(created.UserId);
                }
            }
        }

        [Fact]
        public void DBUser_OfflineFallback_SupportsCreateChangePasswordAndSafeDeduct()
        {
            var offlineDb = CreateOfflineDb();
            string uniqueUsername = $"off_{Guid.NewGuid():N}"[..15];
            var user = new AppUser
            {
                Username = uniqueUsername,
                FullName = "Offline Gamer",
                Role = "Member",
                Balance = 30000m
            };

            try
            {
                Assert.True(offlineDb.CreateMember(user, "offline123", out string err), err);
                Assert.True(offlineDb.IsUsernameExists(uniqueUsername));
                Assert.True(offlineDb.ChangePassword(uniqueUsername, "offline123", "offline456", out err), err);
                Assert.False(offlineDb.DeductBalance(user.UserId, 50000m));
                Assert.True(offlineDb.DeductBalance(user.UserId, 10000m));
                Assert.Equal(20000m, offlineDb.GetById(user.UserId)?.Balance);
            }
            finally
            {
                if (user.UserId > 0)
                {
                    offlineDb.Delete(user.UserId);
                }
            }
        }

        [Fact]
        public void FormMember_MasksPasswordAndLoadsSafelyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember();
                    var pwdField = typeof(FormMember).GetField("txtPassword",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var pwdBox = pwdField?.GetValue(form) as System.Windows.Forms.TextBox;

                    Assert.NotNull(pwdBox);
                    Assert.True(pwdBox!.UseSystemPasswordChar);
                    Assert.DoesNotContain("PBKDF2$", pwdBox.Text);
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

        [Theory]
        [InlineData("50,000.00", true, 50000)]
        [InlineData("50000.5", true, 50000.5)]
        [InlineData("10.000", true, 10000)]
        [InlineData("10.000 VNĐ", true, 10000)]
        [InlineData("-10.000", true, -10000)]
        [InlineData("1.250.000 đ", true, 1250000)]
        [InlineData("", false, 0)]
        [InlineData("abc", false, 0)]
        public void DBUser_TryParseCurrencyAmount_HandlesFormattedAndDecimalInputsAccurately(
            string rawText, bool expectedSuccess, decimal expectedAmount)
        {
            bool ok = DBUser.TryParseCurrencyAmount(rawText, out decimal actual);
            Assert.Equal(expectedSuccess, ok);
            if (expectedSuccess)
            {
                Assert.Equal(expectedAmount, actual);
            }
        }

        [Fact]
        public void DBUser_VerifyPassword_RejectsSignedIterationsAndSubcentAmounts()
        {
            string validHash = DBUser.HashPassword("pass123");
            string signedIterationHash = validHash.Replace("PBKDF2$100000$", "PBKDF2$+100000$", StringComparison.Ordinal);

            Assert.False(DBUser.VerifyPassword("pass123", signedIterationHash));

            var offlineDb = CreateOfflineDb();
            Assert.False(offlineDb.TopUpBalance(2, 1000.123m));
            Assert.False(offlineDb.DeductBalance(2, 1000.123m));
            Assert.Throws<ArgumentException>(() => offlineDb.DeductBalanceOrThrow(2, 1000.123m));
        }

        [Fact]
        public void DBUser_SearchUsers_FiltersByRoleAndTierTogether()
        {
            var sample = new List<AppUser>
            {
                new() { UserId = 1, Username = "staff_std", FullName = "Staff A", Role = "Staff", MemberTier = "Standard" },
                new() { UserId = 2, Username = "mem_gold", FullName = "Member B", Role = "Member", MemberTier = "Gold" },
                new() { UserId = 3, Username = "staff_gold", FullName = "Staff Gold", Role = "Staff", MemberTier = "Gold" }
            };

            var result = DBUser.SearchUsers(sample, "", roleFilter: "Staff", tierFilter: "Gold");
            Assert.Single(result);
            Assert.Equal("staff_gold", result[0].Username);
        }

        [Fact]
        public void DBUser_CreateMember_DoesNotMutateInputEntityOnFailure_AndUpdateRejectsInvalidTier()
        {
            var offlineDb = CreateOfflineDb();
            var entity = new AppUser
            {
                Username = "admin",
                FullName = "Duplicate Admin",
                Role = "Member",
                Balance = 500000m,
                MemberTier = "Standard"
            };

            Assert.False(offlineDb.CreateMember(entity, "secret123", out _));
            Assert.Equal(0, entity.UserId);
            Assert.Equal(string.Empty, entity.PasswordHash);
            Assert.Equal("Standard", entity.MemberTier);

            var invalidTierUser = new AppUser
            {
                UserId = 2,
                Username = "gamer01",
                FullName = "Nguyễn Văn A",
                Role = "Member",
                MemberTier = "Platinum"
            };
            Assert.Throws<ArgumentException>(() => offlineDb.Update(invalidTierUser));
        }

        [Fact]
        public async Task DBUser_AsyncCrudAndTopUpLifecycle_WorksEndToEnd()
        {
            var db = new DBUser();
            string u1 = $"asy1_{Guid.NewGuid():N}"[..15];
            string u2 = $"asy2_{Guid.NewGuid():N}"[..15];
            var user1 = new AppUser { Username = u1, FullName = "Async User 1", Role = "Member", Balance = 20000m };
            var user2 = new AppUser { Username = u2, FullName = "Async User 2", Role = "Member", Balance = 20000m };

            try
            {
                var (ok1, err1) = await db.CreateMemberAsync(user1, "asyncPass1");
                Assert.True(ok1, err1);
                var (ok2, err2) = await db.CreateMemberAsync(user2, "asyncPass2");
                Assert.True(ok2, err2);

                Assert.True(await db.TopUpBalanceAsync(user1.UserId, 200000m));
                var afterTopUp = db.GetById(user1.UserId);
                Assert.NotNull(afterTopUp);
                Assert.Equal(220000m, afterTopUp.Balance);
                Assert.Equal("Silver", afterTopUp.MemberTier);

                user1.FullName = "Async User 1 Edited";
                Assert.True(await db.UpdateAsync(user1));

                user1.Username = u2;
                await Assert.ThrowsAsync<InvalidOperationException>(() => db.UpdateAsync(user1));

                var (pwdOk, pwdErr) = await db.ChangePasswordAsync(u2, "asyncPass2", "asyncPass2New");
                Assert.True(pwdOk, pwdErr);

                Assert.True(await db.DeleteAsync(user1.UserId));
                user1.UserId = 0;
                Assert.True(await db.DeleteAsync(user2.UserId));
                user2.UserId = 0;
            }
            finally
            {
                if (user1.UserId > 0) await db.DeleteAsync(user1.UserId);
                if (user2.UserId > 0) await db.DeleteAsync(user2.UserId);
            }
        }

        [Fact]
        public void FormMember_ClearsStaleEditorOnEmptySearch_AndSearchBarFitsMinimumWidth()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    var searchBox = typeof(FormMember).GetField("txtTimKiem", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TextBox;
                    var idBox = typeof(FormMember).GetField("txtUserId", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TextBox;
                    var pwdBox = typeof(FormMember).GetField("txtPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TextBox;
                    var changePwdUserBox = typeof(FormMember).GetField("txtDoiMatKhauUser", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TextBox;
                    var resetBtn = typeof(FormMember).GetField("btnDatLaiTimKiem", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var pnlLeft = typeof(FormMember).GetField("pnlLeft", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Panel;
                    var dgv = typeof(FormMember).GetField("dgvHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as DataGridView;

                    Assert.NotNull(searchBox);
                    Assert.NotNull(idBox);
                    Assert.NotNull(pwdBox);
                    Assert.NotNull(changePwdUserBox);
                    Assert.NotNull(resetBtn);
                    Assert.NotNull(pnlLeft);
                    Assert.NotNull(dgv);

                    // After Show(), row 0 is genuinely bound and selected: txtUserId and txtDoiMatKhauUser populate, password stays masked & empty
                    Assert.True(dgv!.Rows.Count > 0);
                    Assert.False(string.IsNullOrEmpty(idBox!.Text));
                    Assert.False(string.IsNullOrEmpty(changePwdUserBox!.Text));
                    Assert.True(pwdBox!.UseSystemPasswordChar);
                    Assert.Equal(string.Empty, pwdBox.Text);

                    // Searching for nonexistent user must clear stale txtUserId and txtDoiMatKhauUser so hidden user cannot be modified/deleted
                    searchBox!.Text = "__no_member_matches_this_999__";
                    Application.DoEvents();
                    Assert.Equal(0, dgv.Rows.Count);
                    Assert.Equal(string.Empty, idBox.Text);
                    Assert.Equal(string.Empty, changePwdUserBox.Text);

                    // Clearing search restores rows and selection
                    searchBox.Text = string.Empty;
                    Application.DoEvents();
                    Assert.True(dgv.Rows.Count > 0);
                    Assert.False(string.IsNullOrEmpty(idBox.Text));

                    // Search controls must fit inside MinimumSize width without clipping
                    int availableSearchWidth = form.MinimumSize.Width - 16 - pnlLeft!.Width - 20;
                    Assert.True(resetBtn!.Right <= availableSearchWidth,
                        $"btnDatLaiTimKiem.Right ({resetBtn.Right}) exceeds available width ({availableSearchWidth})");

                    form.Close();
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
        public void DBUser_GetTopMembersByBalance_And_BuildTopMembersByTopUp_RankProperlyUsingLINQ()
        {
            var users = new List<AppUser>
            {
                new() { UserId = 1, Username = "admin", Role = "Admin", Balance = 1000000m },
                new() { UserId = 2, Username = "gamer01", FullName = "A", Role = "Member", Balance = 50000m, MemberTier = "Standard" },
                new() { UserId = 3, Username = "gamer02", FullName = "B", Role = "Member", Balance = 120000m, MemberTier = "Silver" },
                new() { UserId = 4, Username = "gamer03", FullName = "C", Role = "Member", Balance = 300000m, MemberTier = "Gold" },
                new() { UserId = 5, Username = "staff01", Role = "Staff", Balance = 500000m },
                new() { UserId = 6, Username = "gamer04", FullName = "D", Role = "Member", Balance = 120000m, MemberTier = "Silver" }
            };

            // 1. Top by balance (excludes non-members, sorts descending, then by UserId)
            var topBalance = DBUser.GetTopMembersByBalance(users, 2);
            Assert.Equal(2, topBalance.Count);
            Assert.Equal("gamer03", topBalance[0].Username); // 300,000
            Assert.Equal("gamer02", topBalance[1].Username); // 120,000 (UserId 3 before UserId 6)

            // 2. Build top by top-up using LINQ aggregation
            var transactions = new List<WalletTransaction>
            {
                new() { TransactionId = 1, UserId = 2, TransactionType = "TopUp", Amount = 50000m },
                new() { TransactionId = 2, UserId = 2, TransactionType = "TopUp", Amount = 100000m },
                new() { TransactionId = 3, UserId = 3, TransactionType = "TopUp", Amount = 200000m },
                new() { TransactionId = 4, UserId = 3, TransactionType = "SessionPayment", Amount = 50000m },
                new() { TransactionId = 5, UserId = 4, TransactionType = "TopUp", Amount = 500000m },
                new() { TransactionId = 6, UserId = 5, TransactionType = "TopUp", Amount = 999999m } // Staff excluded
            };

            var topTopUp = DBUser.BuildTopMembersByTopUp(users, transactions, 2);
            Assert.Equal(2, topTopUp.Count);
            Assert.Equal("gamer03", topTopUp[0].Username); // 500,000
            Assert.Equal(500000m, topTopUp[0].TotalTopUpAmount);
            Assert.Equal(1, topTopUp[0].TopUpCount);

            Assert.Equal("gamer02", topTopUp[1].Username); // 200,000
            Assert.Equal(200000m, topTopUp[1].TotalTopUpAmount);
            Assert.Equal(1, topTopUp[1].TopUpCount);
        }

        [Fact]
        public async Task DBUser_OfflineWalletTransactions_And_LockStatusToggle_WorksCorrectly()
        {
            var offlineDb = CreateOfflineDb();

            // 1. Lock and unlock toggle in offline mode
            bool locked = await offlineDb.SetUserActiveStatusAsync(2, false);
            Assert.True(locked);
            var user = offlineDb.GetById(2);
            Assert.NotNull(user);
            Assert.False(user.IsActive);

            bool unlocked = await offlineDb.SetUserActiveStatusAsync(2, true);
            Assert.True(unlocked);
            user = offlineDb.GetById(2);
            Assert.NotNull(user);
            Assert.True(user.IsActive);

            // 2. Invalid userId returns false
            Assert.False(await offlineDb.SetUserActiveStatusAsync(0, false));
            Assert.False(await offlineDb.SetUserActiveStatusAsync(999999, false));

            // 3. Top-up & safe deduct append to wallet transaction history
            decimal balanceBefore = user.Balance;
            bool topUpOk = await offlineDb.TopUpBalanceAsync(2, 50000m, "Tiền mặt tại quầy");
            Assert.True(topUpOk);

            var transactions = await offlineDb.GetWalletTransactionsByUserIdAsync(2);
            Assert.NotEmpty(transactions);
            var latest = transactions[0];
            Assert.Equal(2, latest.UserId);
            Assert.Equal("TopUp", latest.TransactionType);
            Assert.Equal(50000m, latest.Amount);
            Assert.Equal(balanceBefore, latest.BalanceBefore);
            Assert.Equal(balanceBefore + 50000m, latest.BalanceAfter);
        }

        [Fact]
        public void FormMember_Phase2TabsAndControls_InitializedProperlyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    var tabMain = typeof(FormMember).GetField("tabMain", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TabControl;
                    var cboLocVaiTro = typeof(FormMember).GetField("cboLocVaiTro", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as ComboBox;
                    var dgvLichSuVi = typeof(FormMember).GetField("dgvLichSuVi", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as DataGridView;
                    var dgvTopSoDu = typeof(FormMember).GetField("dgvTopSoDu", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as DataGridView;
                    var dgvTopNapTien = typeof(FormMember).GetField("dgvTopNapTien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as DataGridView;
                    var btnLock = typeof(FormMember).GetField("btnXoaHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;

                    Assert.NotNull(tabMain);
                    Assert.Equal(3, tabMain!.TabPages.Count);
                    Assert.Contains("Danh Sách", tabMain.TabPages[0].Text);
                    Assert.Contains("Lịch Sử", tabMain.TabPages[1].Text);
                    Assert.Contains("Top 5", tabMain.TabPages[2].Text);

                    Assert.NotNull(cboLocVaiTro);
                    Assert.Contains("Tất cả", cboLocVaiTro!.Items.Cast<object>().Select(x => x.ToString()));
                    Assert.Contains("Member", cboLocVaiTro.Items.Cast<object>().Select(x => x.ToString()));

                    Assert.NotNull(dgvLichSuVi);
                    Assert.NotNull(dgvTopSoDu);
                    Assert.NotNull(dgvTopNapTien);

                    Assert.NotNull(btnLock);
                    Assert.Contains("Khóa", btnLock!.Text);

                    form.Close();
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
        public async Task DBUser_GetWalletTransactions_And_GetTopMembersByTopUp_AllOverloadsWork()
        {
            var offlineDb = CreateOfflineDb();

            // Synchronous and asynchronous GetWalletTransactions(int userId)
            var userTransSync = offlineDb.GetWalletTransactions(2);
            var userTransAsync = await offlineDb.GetWalletTransactionsAsync(2);
            Assert.NotEmpty(userTransSync);
            Assert.Equal(userTransSync.Count, userTransAsync.Count);
            Assert.Equal(2, userTransSync[0].UserId);

            // Parameterless GetWalletTransactions()
            var allTransSync = offlineDb.GetWalletTransactions();
            var allTransAsync = await offlineDb.GetWalletTransactionsAsync();
            Assert.NotEmpty(allTransSync);
            Assert.Equal(allTransSync.Count, allTransAsync.Count);

            // DeductBalance records Adjustment transaction
            decimal deductAmount = 10000m;
            bool deducted = offlineDb.DeductBalance(2, deductAmount);
            Assert.True(deducted);
            var transAfterDeduct = offlineDb.GetWalletTransactions(2);
            var adjustTrans = transAfterDeduct.FirstOrDefault(t => t.TransactionType == "Adjustment");
            Assert.NotNull(adjustTrans);
            Assert.Equal(deductAmount, adjustTrans!.Amount);

            // Synchronous GetTopMembersByTopUp and async parameterless
            var topSync = offlineDb.GetTopMembersByTopUp(topCount: 5);
            var topAsync = await offlineDb.GetTopMembersByTopUpAsync(topCount: 5);
            Assert.NotNull(topSync);
            Assert.NotNull(topAsync);
            Assert.Equal(topSync.Count, topAsync.Count);
        }

        [Fact]
        public void FormMember_AliasesAndControls_FullyExposedAndWired()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    // Check both original and alias control fields via reflection
                    var tabMain = typeof(FormMember).GetField("tabMain", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TabControl;
                    var tabLichSuVi = typeof(FormMember).GetField("tabLichSuVi", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TabPage;
                    var tabLichSuNap = typeof(FormMember).GetField("tabLichSuNap", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TabPage;
                    var cboLocVaiTro = typeof(FormMember).GetField("cboLocVaiTro", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as ComboBox;
                    var cboLocRole = typeof(FormMember).GetField("cboLocRole", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as ComboBox;
                    var btnTopHoiVien = typeof(FormMember).GetField("btnTopHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnKhoaThe = typeof(FormMember).GetField("btnKhoaThe", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnMoThe = typeof(FormMember).GetField("btnMoThe", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnLock = typeof(FormMember).GetField("btnXoaHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;

                    Assert.NotNull(tabMain);
                    Assert.NotNull(tabLichSuVi);
                    Assert.NotNull(tabLichSuNap);
                    Assert.Same(tabLichSuVi, tabLichSuNap);

                    Assert.NotNull(cboLocVaiTro);
                    Assert.NotNull(cboLocRole);
                    Assert.Same(cboLocVaiTro, cboLocRole);

                    Assert.NotNull(btnTopHoiVien);
                    Assert.NotNull(btnKhoaThe);
                    Assert.NotNull(btnMoThe);
                    Assert.Same(btnLock, btnKhoaThe);
                    Assert.Same(btnLock, btnMoThe);

                    // Test btnTopHoiVien switches to tabTopHoiVien
                    btnTopHoiVien!.PerformClick();
                    Application.DoEvents();
                    var tabTop = typeof(FormMember).GetField("tabTopHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TabPage;
                    Assert.Equal(tabTop, tabMain!.SelectedTab);

                    form.Close();
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
        public void FormMember_HeadlessLockUnlockAndTopUpFlow_RunsWithoutHanging()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };

                    var promptMessages = new List<string>();
                    form.SetPromptHandler((text, caption, buttons, icon) =>
                    {
                        promptMessages.Add(text);
                        return DialogResult.Yes;
                    });

                    form.Show();
                    Application.DoEvents();

                    var dgv = typeof(FormMember).GetField("dgvHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as DataGridView;
                    var btnLock = typeof(FormMember).GetField("btnXoaHoiVien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var btnNapTien = typeof(FormMember).GetField("btnNapTien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var txtSoTienNap = typeof(FormMember).GetField("txtSoTienNap", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TextBox;

                    Assert.NotNull(dgv);
                    Assert.NotNull(btnLock);
                    Assert.NotNull(btnNapTien);
                    Assert.NotNull(txtSoTienNap);

                    // Ensure gamer01 is active before starting UI flow
                    new DBUser().SetUserActiveStatus(2, true);

                    // Select member row (gamer01, UserId 2)
                    if (dgv!.Rows.Count > 1)
                    {
                        dgv.CurrentCell = dgv.Rows[1].Cells[0];
                        dgv.Rows[1].Selected = true;
                        typeof(FormMember).GetMethod("DgvHoiVien_SelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                            ?.Invoke(form, new object[] { dgv, EventArgs.Empty });
                        Application.DoEvents();

                        // Lock the member
                        var lockMethod = typeof(FormMember).GetMethod("BtnKhoaMoKhoa_ClickAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                        (lockMethod?.Invoke(form, null) as Task)?.GetAwaiter().GetResult();
                        Application.DoEvents();

                        // Attempt to top-up locked member
                        txtSoTienNap!.Text = "50000";
                        var napMethod = typeof(FormMember).GetMethod("BtnNapTien_ClickAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                        (napMethod?.Invoke(form, null) as Task)?.GetAwaiter().GetResult();
                        Application.DoEvents();

                        Assert.Contains(promptMessages, m => m.Contains("bị khóa", StringComparison.OrdinalIgnoreCase));

                        // Unlock the member
                        (lockMethod?.Invoke(form, null) as Task)?.GetAwaiter().GetResult();
                        Application.DoEvents();
                    }

                    form.Close();
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
        public async Task DBUser_TopUp_NegativeAmount_ReturnsFalse()
        {
            var dbUser = CreateOfflineDb();
            bool result = await dbUser.TopUpBalanceAsync(1, -50000m);
            Assert.False(result);
        }

        [Fact]
        public void AppUser_InitialStatus_DefaultsToActive()
        {
            var user = new AppUser { Username = "newuser", FullName = "Hội viên mới" };
            Assert.True(user.IsActive);
        }

        [Fact]
        public void DBUser_SearchUsers_EmptyKeyword_ReturnsFullList()
        {
            var users = new List<AppUser>
            {
                new AppUser { UserId = 1, Username = "gamer01", FullName = "Nguyễn Văn A" },
                new AppUser { UserId = 2, Username = "gamer02", FullName = "Trần Thị B" }
            };
            var result = DBUser.SearchUsers(users, "");
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FormMember_KeyboardShortcuts_And_ToolTips_WorkProperlyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    using var form = new FormMember
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    form.Show();
                    Application.DoEvents();

                    var toolTip = typeof(FormMember)
                        .GetField("_memberToolTip", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.GetValue(form) as ToolTip;
                    Assert.NotNull(toolTip);

                    var txtTimKiem = typeof(FormMember).GetField("txtTimKiem", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TextBox;
                    var btnNapTien = typeof(FormMember).GetField("btnNapTien", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Button;
                    var tabMain = typeof(FormMember).GetField("tabMain", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as TabControl;

                    Assert.NotNull(txtTimKiem);
                    Assert.NotNull(btnNapTien);
                    Assert.NotNull(tabMain);

                    Assert.False(string.IsNullOrEmpty(toolTip!.GetToolTip(txtTimKiem!)));
                    Assert.False(string.IsNullOrEmpty(toolTip.GetToolTip(btnNapTien!)));

                    var onKeyDown = typeof(FormMember).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance);
                    Assert.NotNull(onKeyDown);

                    // Test Ctrl+T -> tabTopHoiVien (tab index 2)
                    onKeyDown!.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.T) });
                    Assert.Equal(2, tabMain!.SelectedIndex);

                    // Test Ctrl+H -> tabLichSuVi (tab index 1)
                    onKeyDown.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.H) });
                    Assert.Equal(1, tabMain.SelectedIndex);

                    // Test Ctrl+D -> tabDanhSach (tab index 0)
                    onKeyDown.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.D) });
                    Assert.Equal(0, tabMain.SelectedIndex);

                    // Test Ctrl+T then Ctrl+F -> should switch to tabDanhSach (index 0)
                    onKeyDown.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.T) });
                    Assert.Equal(2, tabMain.SelectedIndex);
                    onKeyDown.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.F) });
                    Assert.Equal(0, tabMain.SelectedIndex);

                    // Test Ctrl+H then Ctrl+N -> should switch to tabDanhSach (index 0)
                    onKeyDown.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.H) });
                    Assert.Equal(1, tabMain.SelectedIndex);
                    onKeyDown.Invoke(form, new object[] { new KeyEventArgs(Keys.Control | Keys.N) });
                    Assert.Equal(0, tabMain.SelectedIndex);

                    form.Close();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            bool completed = staThread.Join(TimeSpan.FromSeconds(10));

            Assert.True(completed, "STA thread timed out");
            Assert.Null(threadException);
        }

        [Fact]
        public void LoyaltyReward_CalculatePoints_AppliesRoleMultiplier()
        {
            int memberPoints = LoyaltyReward.CalculatePoints(50000m, "Member");
            int vipPoints = LoyaltyReward.CalculatePoints(50000m, "VIP");

            Assert.Equal(50, memberPoints);
            Assert.Equal(75, vipPoints); // 50 * 1.5 = 75
        }

        [Fact]
        public void LoyaltyReward_RedeemPoints_RequiresMinimum50Points()
        {
            decimal cashUnderMin = LoyaltyReward.RedeemPointsToCash(40);
            decimal cashValid = LoyaltyReward.RedeemPointsToCash(100);

            Assert.Equal(0m, cashUnderMin);
            Assert.Equal(10000m, cashValid); // 100 * 100 = 10,000 VNĐ
        }

        [Fact]
        public void AccountRecoveryService_ValidateOtp_EnforcesLimitAndMatching()
        {
            var service = new AccountRecoveryService();
            string code = service.GenerateOtp(99);

            bool wrong = service.ValidateOtp(99, "000000", out string error);
            Assert.False(wrong);
            Assert.Contains("không chính xác", error);

            bool correct = service.ValidateOtp(99, code, out string okMsg);
            Assert.True(correct);
            Assert.Empty(okMsg);
        }

        [Fact]
        public void AccountRecoveryService_ValidateOtp_LockedAfterThreeFailedAttempts()
        {
            var service = new AccountRecoveryService();
            string code = service.GenerateOtp(101);

            service.ValidateOtp(101, "111111", out _); // Lan 1
            service.ValidateOtp(101, "222222", out _); // Lan 2
            bool thirdWrong = service.ValidateOtp(101, "333333", out string err3); // Lan 3
            Assert.False(thirdWrong);
            Assert.Contains("quá 3 lần", err3);

            // Nhập đúng mã sau 3 lần sai vẫn bị khóa
            bool fourthAttempt = service.ValidateOtp(101, code, out string err4);
            Assert.False(fourthAttempt);
            Assert.Contains("quá 3 lần", err4);
        }

        [Fact]
        public void AccountRecoveryService_ValidateOtp_ReusedOtpFails()
        {
            var service = new AccountRecoveryService();
            string code = service.GenerateOtp(102);

            Assert.True(service.ValidateOtp(102, code, out _));
            bool secondUse = service.ValidateOtp(102, code, out string reuseErr);
            Assert.False(secondUse);
            Assert.Contains("đã được sử dụng", reuseErr);
        }

        [Fact]
        public void AccountRecoveryService_GenerateOtp_ValidatesInputs()
        {
            var service = new AccountRecoveryService();
            Assert.Throws<ArgumentOutOfRangeException>(() => service.GenerateOtp(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.GenerateOtp(1, 0));
        }

        [Fact]
        public void LoyaltyReward_CalculatePoints_CaseInsensitive()
        {
            int vipLower = LoyaltyReward.CalculatePoints(50000m, "vip");
            int tournamentMixed = LoyaltyReward.CalculatePoints(50000m, "Tournament");
            int nonExistent = LoyaltyReward.CalculatePoints(50000m, "Other");

            Assert.Equal(75, vipLower);
            Assert.Equal(100, tournamentMixed);
            Assert.Equal(50, nonExistent);
        }

        [Fact]
        public void LoyaltyReward_RedeemPoints_InvalidRate_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LoyaltyReward.RedeemPointsToCash(100, 0m));
            Assert.Throws<ArgumentOutOfRangeException>(() => LoyaltyReward.RedeemPointsToCash(100, -5m));
        }

        // ==========================================
        // PHONE NUMBER & DUAL LOGIN & RECOVERY TESTS
        // ==========================================

        [Theory]
        [InlineData("0981000001", true)]
        [InlineData("0901234567", true)]
        [InlineData("0321234567", true)]
        [InlineData("0561234567", true)]
        [InlineData("0771234567", true)]
        [InlineData("0881234567", true)]
        [InlineData(" 0981000001 ", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("098123456", false)] // 9 digits
        [InlineData("09812345678", false)] // 11 digits
        [InlineData("0123456789", false)] // Prefix 01 invalid
        [InlineData("0243456789", false)] // Prefix 02 invalid
        [InlineData("0481234567", false)] // Prefix 04 invalid
        [InlineData("0681234567", false)] // Prefix 06 invalid
        [InlineData("098123456a", false)] // Contains letter
        [InlineData("098abc4567", false)] // Contains letters
        [InlineData("098 123 456", false)] // Contains space inside
        [InlineData("098-1234567", false)] // Contains hyphen
        [InlineData("0|12345678", false)] // Contains pipe symbol
        [InlineData("+8498100001", false)] // International prefix +84
        [InlineData("030312345678", false)] // 12 digits
        public void DBUser_IsValidPhoneNumber_ValidatesVietnamesePhoneNumbers(string? phone, bool expected)
        {
            Assert.Equal(expected, DBUser.IsValidPhoneNumber(phone));
        }

        [Fact]
        public async Task DBUser_DualLogin_AuthenticatesWithUsernameOrPhoneNumber()
        {
            var offlineDb = CreateOfflineDb();

            // 1. Login with Username (gamer01, pass: 123456)
            var userByUsername = offlineDb.Authenticate(usernameOrPhone: "gamer01", password: "123456");
            Assert.NotNull(userByUsername);
            Assert.Equal("gamer01", userByUsername!.Username);
            Assert.Equal("0981000002", userByUsername.PhoneNumber);

            // 2. Login with PhoneNumber (0981000002, pass: 123456)
            var userByPhone = offlineDb.Authenticate(usernameOrPhone: "0981000002", password: "123456");
            Assert.NotNull(userByPhone);
            Assert.Equal("gamer01", userByPhone!.Username);
            Assert.Equal(userByUsername.UserId, userByPhone.UserId);

            // 3. TryAuthenticate with PhoneNumber
            bool tryOk = offlineDb.TryAuthenticate(usernameOrPhone: "0981000002", password: "123456", out var tryUser, out string tryErr);
            Assert.True(tryOk);
            Assert.NotNull(tryUser);
            Assert.Empty(tryErr);
            Assert.Equal("gamer01", tryUser!.Username);

            // 4. AuthenticateAsync with PhoneNumber
            var (asyncOk, asyncUser, asyncErr) = await offlineDb.AuthenticateAsync(usernameOrPhone: "0981000002", password: "123456");
            Assert.True(asyncOk);
            Assert.NotNull(asyncUser);
            Assert.Empty(asyncErr);
            Assert.Equal("gamer01", asyncUser!.Username);

            // 5. Dual login with Admin phone
            var adminUser = offlineDb.Authenticate(usernameOrPhone: "0981000001", password: "123456");
            Assert.NotNull(adminUser);
            Assert.Equal("admin", adminUser!.Username);

            // 6. ChangePassword using PhoneNumber identifier
            bool changePassOk = offlineDb.ChangePassword("0981000002", "123456", "newPassViaPhone123", out string changeErr);
            Assert.True(changePassOk, changeErr);
            Assert.NotNull(offlineDb.Authenticate(usernameOrPhone: "0981000002", password: "newPassViaPhone123"));
            Assert.NotNull(offlineDb.Authenticate(usernameOrPhone: "gamer01", password: "newPassViaPhone123"));
            offlineDb.ChangePassword("gamer01", "newPassViaPhone123", "123456", out _);

            // 7. ChangePasswordAsync using PhoneNumber identifier
            var (asyncChangeOk, _) = await offlineDb.ChangePasswordAsync("0981000002", "123456", "asyncPassViaPhone123");
            Assert.True(asyncChangeOk);
            Assert.NotNull(offlineDb.Authenticate(usernameOrPhone: "0981000002", password: "asyncPassViaPhone123"));
            offlineDb.ChangePassword("0981000002", "asyncPassViaPhone123", "123456", out _);

            // 8. Wrong password with phone number fails
            Assert.Null(offlineDb.Authenticate("0981000002", "wrongpassword"));
            bool wrongPassOk = offlineDb.TryAuthenticate("0981000002", "wrongpassword", out _, out string wrongPassErr);
            Assert.False(wrongPassOk);
            Assert.NotEmpty(wrongPassErr);

            // 9. Non-existent phone number fails
            Assert.Null(offlineDb.Authenticate("0989999999", "123456"));
        }

        [Fact]
        public async Task DBUser_PhoneNumberUniqueCollision_RejectedOnAddAndUpdate()
        {
            var offlineDb = CreateOfflineDb();

            // 1. Phone number exists check
            Assert.True(offlineDb.IsPhoneNumberExists("0981000002"));
            Assert.True(await offlineDb.IsPhoneNumberExistsAsync("0981000001"));
            Assert.False(offlineDb.IsPhoneNumberExists("0989999999"));
            Assert.False(await offlineDb.IsPhoneNumberExistsAsync("0989999999"));

            // 2. CreateMember rejects duplicate phone number
            bool createDuplicate = offlineDb.CreateMember(
                "gamer_new", "Gamer Moi", "123456", 50000m, out string createErr,
                role: "Member", requestedTier: "Standard", citizenId: "001200000999", phoneNumber: "0981000002");
            Assert.False(createDuplicate);
            Assert.Contains("Số điện thoại", createErr);

            // 3. Add user rejects duplicate phone number
            var duplicateUser = new AppUser
            {
                Username = "gamer_new2",
                FullName = "Gamer Moi 2",
                Role = "Member",
                PhoneNumber = "0981000001",
                PasswordHash = DBUser.HashPassword("123456")
            };
            var exAdd = Assert.Throws<InvalidOperationException>(() => offlineDb.Add(duplicateUser));
            Assert.Contains("Số điện thoại", exAdd.Message);

            // 4. Update user rejects collision with another user's phone number
            var gamer02 = offlineDb.GetById(3);
            Assert.NotNull(gamer02);
            gamer02!.PhoneNumber = "0981000002"; // collision with gamer01
            bool updateDuplicate = offlineDb.UpdateUser(gamer02, out string updateErr);
            Assert.False(updateDuplicate);
            Assert.Contains("Số điện thoại", updateErr);

            // 5. Updating same user with their own phone number is allowed
            gamer02.PhoneNumber = "0981000003"; // own phone
            gamer02.FullName = "Tran Thi B Updated";
            bool updateSelfOk = offlineDb.UpdateUser(gamer02, out string selfErr);
            Assert.True(updateSelfOk, selfErr);
        }

        [Fact]
        public async Task DBUser_SearchUsers_MatchesPhoneNumber()
        {
            var offlineDb = CreateOfflineDb();

            // 1. Search in-memory list
            var sample = new List<AppUser>
            {
                new() { UserId = 1, Username = "admin", FullName = "Admin Cyber", PhoneNumber = "0981000001", Role = "Admin" },
                new() { UserId = 2, Username = "gamer01", FullName = "Le Van A", PhoneNumber = "0981000002", Role = "Member" },
                new() { UserId = 3, Username = "gamer02", FullName = "Tran Thi B", PhoneNumber = "0981000003", Role = "Member" }
            };

            var searchResult = DBUser.SearchUsers(sample, "0981000002");
            Assert.Single(searchResult);
            Assert.Equal("gamer01", searchResult[0].Username);

            // 2. Search via offline DB
            var dbSearchResults = offlineDb.SearchUsers("0981000003");
            Assert.Single(dbSearchResults);
            Assert.Equal("gamer02", dbSearchResults[0].Username);

            // 3. Search async via offline DB
            var dbAsyncResults = await offlineDb.SearchUsersAsync("0981000005");
            Assert.Single(dbAsyncResults);
            Assert.Equal("staff01", dbAsyncResults[0].Username);
        }

        [Fact]
        public async Task DBUser_GetByPhoneNumber_And_GetByUsernameOrPhone_Works()
        {
            var offlineDb = CreateOfflineDb();

            // 1. GetByPhoneNumber
            var userByPhone = offlineDb.GetByPhoneNumber("0981000002");
            Assert.NotNull(userByPhone);
            Assert.Equal("gamer01", userByPhone!.Username);

            var userByPhoneAsync = await offlineDb.GetByPhoneNumberAsync("0981000004");
            Assert.NotNull(userByPhoneAsync);
            Assert.Equal("gamer03", userByPhoneAsync!.Username);

            Assert.Null(offlineDb.GetByPhoneNumber("0980000000"));
            Assert.Null(await offlineDb.GetByPhoneNumberAsync(""));

            // 2. GetByUsernameOrPhone with Username
            var byUsername = offlineDb.GetByUsernameOrPhone("gamer01");
            Assert.NotNull(byUsername);
            Assert.Equal("gamer01", byUsername!.Username);

            // 3. GetByUsernameOrPhone with PhoneNumber
            var byPhone = offlineDb.GetByUsernameOrPhone("0981000002");
            Assert.NotNull(byPhone);
            Assert.Equal("gamer01", byPhone!.Username);

            // 4. GetByUsernameOrPhone with CitizenId
            var byCccd = offlineDb.GetByUsernameOrPhone("001099000001");
            Assert.NotNull(byCccd);
            Assert.Equal("admin", byCccd!.Username);

            // 5. GetByUsernameOrPhoneAsync
            var byPhoneAsync = await offlineDb.GetByUsernameOrPhoneAsync("0981000003");
            Assert.NotNull(byPhoneAsync);
            Assert.Equal("gamer02", byPhoneAsync!.Username);

            // 6. GetByCitizenId preserves PhoneNumber
            var byCccdOnly = offlineDb.GetByCitizenId("001099000002");
            Assert.NotNull(byCccdOnly);
            Assert.Equal("gamer01", byCccdOnly!.Username);
            Assert.Equal("0981000002", byCccdOnly.PhoneNumber);

            var byCccdOnlyAsync = await offlineDb.GetByCitizenIdAsync("001099000003");
            Assert.NotNull(byCccdOnlyAsync);
            Assert.Equal("gamer02", byCccdOnlyAsync!.Username);
            Assert.Equal("0981000003", byCccdOnlyAsync.PhoneNumber);

            Assert.Null(offlineDb.GetByUsernameOrPhone("nonexistent_val"));
            Assert.Null(await offlineDb.GetByUsernameOrPhoneAsync(""));
        }

        [Fact]
        public async Task DBUser_ResetPassword_ResetsPasswordAndAllowsDualLogin()
        {
            var offlineDb = CreateOfflineDb();

            try
            {
                // 1. Reset password for gamer01 (UserId = 2)
                bool resetOk = offlineDb.ResetPassword(2, "newPass789", out string resetErr);
                Assert.True(resetOk, resetErr);

                // 2. Authenticate with username and new password
                var userWithNewPass = offlineDb.Authenticate("gamer01", "newPass789");
                Assert.NotNull(userWithNewPass);

                // 3. Authenticate with phone number and new password
                var userWithPhoneNewPass = offlineDb.Authenticate("0981000002", "newPass789");
                Assert.NotNull(userWithPhoneNewPass);

                // 4. Old password no longer works
                Assert.Null(offlineDb.Authenticate("gamer01", "123456"));
                Assert.Null(offlineDb.Authenticate("0981000002", "123456"));

                // 5. ResetPasswordAsync (UserId = 3 for gamer02)
                var (asyncResetOk, _) = await offlineDb.ResetPasswordAsync(3, "gamer02NewPass");
                Assert.True(asyncResetOk);
                Assert.NotNull(offlineDb.Authenticate("0981000003", "gamer02NewPass"));

                // 6. Non-existent identifier fails
                Assert.False(offlineDb.ResetPassword(999999, "newPass123", out _));

                // 7. Password too short (<6 chars) fails
                Assert.False(offlineDb.ResetPassword(2, "123", out string shortErr));
                Assert.Contains("độ dài từ 6 đến 100 ký tự", shortErr);
            }
            finally
            {
                offlineDb.ResetPassword(2, "123456", out _);
                offlineDb.ResetPassword(3, "123456", out _);
            }
        }

        [Fact]
        public void DBUser_ValidateMemberInput_WithPhoneNumber_ValidatesCorrectly()
        {
            // 1. Valid member with valid phone number
            bool valid = DBUser.ValidateMemberInput(
                "gamer_valid", "Nguyen Van Hop Le", "secret123", 50000m, "Member",
                "001200000002", "0981234567", out string error);
            Assert.True(valid, error);
            Assert.Empty(error);

            // 2. Invalid phone format rejected
            bool invalidFormat = DBUser.ValidateMemberInput(
                "gamer_valid", "Nguyen Van Hop Le", "secret123", 50000m, "Member",
                "001200000002", "0123456789", out string formatErr);
            Assert.False(invalidFormat);
            Assert.Contains("Số điện thoại không đúng định dạng", formatErr);

            // 3. Required phone number missing rejected
            bool requiredMissing = DBUser.ValidateMemberInput(
                "gamer_valid", "Nguyen Van Hop Le", "secret123", 50000m, "Member",
                "001200000002", "", out string reqErr, requirePhoneNumber: true);
            Assert.False(requiredMissing);
            Assert.Contains("Số điện thoại không được để trống", reqErr);

            // 4. Optional phone number empty accepted when requirePhoneNumber is false
            bool optionalEmpty = DBUser.ValidateMemberInput(
                "gamer_valid", "Nguyen Van Hop Le", "secret123", 50000m, "Member",
                "001200000002", "", out string optErr, requirePhoneNumber: false);
            Assert.True(optionalEmpty, optErr);
        }

        [Fact]
        public void FormAccountRecoveryDialog_FormControls_InitializedProperlyOnSTAThread()
        {
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    var offlineDb = CreateOfflineDb();
                    using var dlg = new FormAccountRecoveryDialog(offlineDb)
                    {
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };
                    dlg.Show();
                    Application.DoEvents();

                    // Verify controls exist
                    var txtIdentifier = typeof(FormAccountRecoveryDialog).GetField("txtIdentifier", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                    var btnLookup = typeof(FormAccountRecoveryDialog).GetField("btnLookup", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as Button;
                    var btnSendOtp = typeof(FormAccountRecoveryDialog).GetField("btnSendOtp", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as Button;
                    var txtOtp = typeof(FormAccountRecoveryDialog).GetField("txtOtp", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                    var txtNewPassword = typeof(FormAccountRecoveryDialog).GetField("txtNewPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                    var txtConfirmPassword = typeof(FormAccountRecoveryDialog).GetField("txtConfirmPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as TextBox;
                    var btnReset = typeof(FormAccountRecoveryDialog).GetField("btnResetPassword", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dlg) as Button;

                    Assert.NotNull(txtIdentifier);
                    Assert.NotNull(btnLookup);
                    Assert.NotNull(btnSendOtp);
                    Assert.NotNull(txtOtp);
                    Assert.NotNull(txtNewPassword);
                    Assert.NotNull(txtConfirmPassword);
                    Assert.NotNull(btnReset);

                    // 1. Lookup by phone number
                    var foundByPhone = dlg.LookupUser("0981000002");
                    Assert.NotNull(foundByPhone);
                    Assert.Equal("gamer01", foundByPhone!.Username);
                    Assert.Equal("0981000002", foundByPhone.PhoneNumber);

                    // 2. Lookup by username
                    var foundByUsername = dlg.LookupUser("gamer02");
                    Assert.NotNull(foundByUsername);
                    Assert.Equal("0981000003", foundByUsername!.PhoneNumber);

                    // 3. Lookup by CCCD
                    var foundByCccd = dlg.LookupUser("001099000001");
                    Assert.NotNull(foundByCccd);
                    Assert.Equal("admin", foundByCccd!.Username);

                    // 4. Generate OTP and Reset Password flow
                    dlg.LookupUser("0981000002");
                    string otp = dlg.GenerateOtpForUser(foundByPhone.UserId);
                    Assert.NotEmpty(otp);
                    Assert.Equal(6, otp.Length);

                    // Reset password using OTP
                    bool resetResult = dlg.TryResetPassword("0981000002", otp, "recoveryNewPass123", out string resetErr);
                    Assert.True(resetResult, resetErr);

                    // Verify can authenticate with new password via phone
                    var verifiedUser = offlineDb.Authenticate("0981000002", "recoveryNewPass123");
                    Assert.NotNull(verifiedUser);
                    Assert.Equal("gamer01", verifiedUser!.Username);

                    // Restore password for other tests
                    offlineDb.ResetPassword(foundByPhone.UserId, "123456", out _);

                    dlg.Close();
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