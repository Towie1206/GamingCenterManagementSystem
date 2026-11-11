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
    }
}
