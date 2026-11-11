using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GamingCenterManagementSystem.Exceptions;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem.DAL
{
    public class DBUser : IRepository<AppUser>
    {
        public const decimal MinimumMemberInitialBalance = 10000m;
        public const decimal MaximumTopUpAmount = 50000000m;
        public const int Pbkdf2Iterations = 100000;

        private static readonly string[] AllowedRoles = { "Member", "Staff", "Admin" };
        private static readonly string[] AllowedTiers = { "Standard", "Silver", "Gold" };

        private readonly Func<SqlConnection> _connectionFactory;

        // Mock fallback data in case database is offline
        private static readonly List<AppUser> _fallbackUsers = new()
        {
            new AppUser { UserId = 1, Username = "admin", PasswordHash = HashPassword("123456"), FullName = "Quản Lý Cyber", Role = "Admin", Balance = 0, MemberTier = "Standard", CreatedAt = DateTime.Now.AddMonths(-3) },
            new AppUser { UserId = 2, Username = "gamer01", PasswordHash = HashPassword("123456"), FullName = "Nguyễn Văn A", Role = "Member", Balance = 50000, MemberTier = "Standard", CreatedAt = DateTime.Now.AddDays(-20) },
            new AppUser { UserId = 3, Username = "gamer02", PasswordHash = HashPassword("123456"), FullName = "Trần Thị B", Role = "Member", Balance = 120000, MemberTier = "Silver", CreatedAt = DateTime.Now.AddDays(-10) },
            new AppUser { UserId = 4, Username = "gamer03", PasswordHash = HashPassword("123456"), FullName = "Lê Hoàng C", Role = "Member", Balance = 300000, MemberTier = "Gold", CreatedAt = DateTime.Now.AddDays(-5) }
        };

        public DBUser() : this(Database.GetConnection)
        {
        }

        public DBUser(Func<SqlConnection> connectionFactory)
        {
            ArgumentNullException.ThrowIfNull(connectionFactory);
            _connectionFactory = connectionFactory;
        }

        // ==========================================
        // PASSWORD HASHING & VERIFICATION (PBKDF2-SHA256)
        // ==========================================

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                32);

            return $"PBKDF2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedPassword))
                return false;

            // Backward compatibility with legacy plaintext seed accounts (e.g. Database.sql '123456')
            if (!storedPassword.StartsWith("PBKDF2$", StringComparison.Ordinal))
                return string.Equals(password, storedPassword, StringComparison.Ordinal);

            try
            {
                string[] parts = storedPassword.Split('$');
                if (parts.Length != 4)
                    return false;

                if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iterations)
                    || iterations <= 0
                    || iterations > 1_000_000)
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] storedHash = Convert.FromBase64String(parts[3]);
                if (salt.Length == 0 || storedHash.Length == 0)
                    return false;

                byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    storedHash.Length);

                return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
            }
            catch
            {
                return false;
            }
        }

        public static string EnsurePasswordHash(string? passwordOrHash)
        {
            if (string.IsNullOrEmpty(passwordOrHash))
                return HashPassword("123456");

            if (passwordOrHash.StartsWith("PBKDF2$", StringComparison.Ordinal))
                return passwordOrHash;

            return HashPassword(passwordOrHash);
        }

        // ==========================================
        // DATA MAPPING & PARAMETER HELPERS
        // ==========================================

        private static AppUser MapUser(SqlDataReader reader)
        {
            return new AppUser
            {
                UserId = reader.GetInt32(0),
                Username = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                PasswordHash = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                FullName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Role = reader.IsDBNull(4) ? "Member" : reader.GetString(4),
                Balance = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                MemberTier = reader.IsDBNull(6) ? "Standard" : reader.GetString(6),
                CreatedAt = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7)
            };
        }

        private static SqlParameter CreateDecimalParameter(string name, decimal value)
        {
            return new SqlParameter(name, SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = value
            };
        }

        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Member";

            string trimmed = role.Trim();
            string? match = AllowedRoles.FirstOrDefault(r => string.Equals(r, trimmed, StringComparison.OrdinalIgnoreCase));
            return match ?? string.Empty;
        }

        private static string NormalizeTier(string? tier)
        {
            if (string.IsNullOrWhiteSpace(tier))
                return "Standard";

            string trimmed = tier.Trim();
            string? match = AllowedTiers.FirstOrDefault(t => string.Equals(t, trimmed, StringComparison.OrdinalIgnoreCase));
            return match ?? "Standard";
        }

        // ==========================================
        // CRUD OPERATIONS (IRepository<AppUser>)
        // ==========================================

        public List<AppUser> GetAll()
        {
            var list = new List<AppUser>();
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"SELECT UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt
                                     FROM AppUser
                                     ORDER BY UserId";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapUser(reader));
                }
                return list;
            }
            catch
            {
                return CloneFallbackUsers();
            }
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            var list = new List<AppUser>();
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                const string sql = @"SELECT UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt
                                     FROM AppUser
                                     ORDER BY UserId";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    list.Add(MapUser(reader));
                }
                return list;
            }
            catch
            {
                return CloneFallbackUsers();
            }
        }

        public AppUser? GetById(int id)
        {
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"SELECT UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt
                                     FROM AppUser
                                     WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = id;
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapUser(reader);
                }
                return null;
            }
            catch
            {
                var found = _fallbackUsers.Find(u => u.UserId == id);
                return found == null ? null : CloneUser(found);
            }
        }

        public bool Add(AppUser entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            string username = entity.Username?.Trim() ?? string.Empty;
            string fullName = entity.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50 || username.Any(char.IsWhiteSpace))
                throw new ArgumentException("Tên đăng nhập không hợp lệ.", nameof(entity));
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
                throw new ArgumentException("Họ và tên không hợp lệ.", nameof(entity));
            if (entity.Balance < 0)
                throw new ArgumentException("Số dư tài khoản không được âm.", nameof(entity));

            string role = NormalizeRole(entity.Role);
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Vai trò tài khoản không hợp lệ.", nameof(entity));

            string tier = AppUser.ResolveUpgradedTier(NormalizeTier(entity.MemberTier), entity.Balance);
            string passwordHash = EnsurePasswordHash(entity.PasswordHash);

            entity.Username = username;
            entity.FullName = fullName;
            entity.Role = role;
            entity.MemberTier = tier;
            entity.PasswordHash = passwordHash;

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt)
                                     OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                                     VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE())";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;
                cmd.Parameters.Add(CreateDecimalParameter("@Balance", entity.Balance));
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = tier;

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    entity.UserId = reader.GetInt32(0);
                    entity.CreatedAt = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1);
                    return true;
                }
                return false;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại trong hệ thống.", ex);
            }
            catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
            {
                if (_fallbackUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Tên đăng nhập đã tồn tại trong hệ thống.", ex);

                entity.UserId = _fallbackUsers.Count > 0 ? _fallbackUsers.Max(u => u.UserId) + 1 : 1;
                entity.CreatedAt = DateTime.Now;
                _fallbackUsers.Add(CloneUser(entity));
                return true;
            }
        }

        public bool Update(AppUser entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            string username = entity.Username?.Trim() ?? string.Empty;
            string fullName = entity.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50 || username.Any(char.IsWhiteSpace))
                throw new ArgumentException("Tên đăng nhập không hợp lệ.", nameof(entity));
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
                throw new ArgumentException("Họ và tên không hợp lệ.", nameof(entity));

            string role = NormalizeRole(entity.Role);
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Vai trò tài khoản không hợp lệ.", nameof(entity));

            string tier = NormalizeTier(entity.MemberTier);

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"UPDATE AppUser
                                     SET Username = @Username, FullName = @FullName, Role = @Role, MemberTier = @MemberTier
                                     WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = tier;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = entity.UserId;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new InvalidOperationException("Tên đăng nhập mới đã thuộc về một tài khoản khác.", ex);
            }
            catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
            {
                if (_fallbackUsers.Any(u => u.UserId != entity.UserId && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Tên đăng nhập mới đã thuộc về một tài khoản khác.", ex);

                var idx = _fallbackUsers.FindIndex(u => u.UserId == entity.UserId);
                if (idx >= 0)
                {
                    _fallbackUsers[idx].Username = username;
                    _fallbackUsers[idx].FullName = fullName;
                    _fallbackUsers[idx].Role = role;
                    _fallbackUsers[idx].MemberTier = tier;
                    return true;
                }
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = "DELETE FROM AppUser WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = id;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Không thể xóa hội viên đã có lịch sử phiên chơi trong hệ thống.", ex);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                return _fallbackUsers.RemoveAll(u => u.UserId == id) > 0;
            }
        }

        // ==========================================
        // LOGIC 1: CHANGE PASSWORD (SECURE PBKDF2)
        // ==========================================

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(oldPassword) ||
                string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Vui lòng nhập đầy đủ tên đăng nhập, mật khẩu cũ và mật khẩu mới.");
            }

            if (newPassword.Length < 6 || newPassword.Length > 100)
            {
                throw new ArgumentException("Mật khẩu mới phải có độ dài từ 6 đến 100 ký tự.", nameof(newPassword));
            }

            return ChangePassword(username, oldPassword, newPassword, out _);
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(oldPassword) ||
                string.IsNullOrEmpty(newPassword))
            {
                errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập, mật khẩu hiện tại và mật khẩu mới.";
                return false;
            }

            if (newPassword.Length < 6 || newPassword.Length > 100)
            {
                errorMessage = "Mật khẩu mới phải có độ dài từ 6 đến 100 ký tự.";
                return false;
            }

            if (string.Equals(oldPassword, newPassword, StringComparison.Ordinal))
            {
                errorMessage = "Mật khẩu mới phải khác mật khẩu hiện tại.";
                return false;
            }

            string trimmedUsername = username.Trim();

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                using var trans = conn.BeginTransaction();

                int userId;
                string storedPassword;

                const string selectQuery = @"SELECT UserId, PasswordHash
                                             FROM AppUser WITH (UPDLOCK, HOLDLOCK)
                                             WHERE Username = @Username";
                using (var cmd = new SqlCommand(selectQuery, conn, trans))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        errorMessage = "Tên đăng nhập hoặc mật khẩu hiện tại không chính xác.";
                        return false;
                    }

                    userId = reader.GetInt32(0);
                    storedPassword = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                }

                if (!VerifyPassword(oldPassword, storedPassword))
                {
                    errorMessage = "Tên đăng nhập hoặc mật khẩu hiện tại không chính xác.";
                    return false;
                }

                string newHash = HashPassword(newPassword);

                const string updateQuery = @"UPDATE AppUser
                                             SET PasswordHash = @NewPassword
                                             WHERE UserId = @UserId";
                using (var cmd = new SqlCommand(updateQuery, conn, trans))
                {
                    cmd.Parameters.Add("@NewPassword", SqlDbType.NVarChar, 255).Value = newHash;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    bool updated = cmd.ExecuteNonQuery() > 0;
                    trans.Commit();
                    if (!updated)
                    {
                        errorMessage = "Không thể cập nhật mật khẩu.";
                    }
                    return updated;
                }
            }
            catch
            {
                var user = _fallbackUsers.Find(u => u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase));
                if (user == null || !VerifyPassword(oldPassword, user.PasswordHash))
                {
                    errorMessage = "Tên đăng nhập hoặc mật khẩu hiện tại không chính xác.";
                    return false;
                }

                user.PasswordHash = HashPassword(newPassword);
                return true;
            }
        }

        public Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(
            string username,
            string oldPassword,
            string newPassword)
        {
            return Task.Run(() =>
            {
                bool ok = ChangePassword(username, oldPassword, newPassword, out string err);
                return (ok, err);
            });
        }

        // ==========================================
        // LOGIC 2: INSTANT LINQ SEARCH & FILTERING
        // ==========================================

        public static List<AppUser> SearchUsers(
            IEnumerable<AppUser> users,
            string? keyword,
            string? roleFilter = null,
            string? tierFilter = null)
        {
            ArgumentNullException.ThrowIfNull(users);

            string trimmedKeyword = keyword?.Trim() ?? string.Empty;
            IEnumerable<AppUser> query = users;

            if (!string.IsNullOrEmpty(trimmedKeyword))
            {
                query = query.Where(u =>
                    u.Username.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase) ||
                    u.Role.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase) ||
                    u.MemberTier.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter) &&
                !string.Equals(roleFilter, "Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => string.Equals(u.Role, roleFilter.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(tierFilter) &&
                !string.Equals(tierFilter, "Tất cả hạng", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(tierFilter, "Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => string.Equals(u.MemberTier, tierFilter.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderBy(u => u.UserId).ToList();
        }

        // ==========================================
        // LOGIC 3: REGISTRATION VALIDATION & CREATION
        // ==========================================

        public static bool ValidateMemberInput(
            string? username,
            string? fullName,
            string? password,
            decimal initialBalance,
            string? role,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            string trimmedUsername = username?.Trim() ?? string.Empty;
            string trimmedFullName = fullName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmedUsername))
            {
                errorMessage = "Tên đăng nhập không được để trống.";
                return false;
            }

            if (trimmedUsername.Any(char.IsWhiteSpace))
            {
                errorMessage = "Tên đăng nhập không được chứa khoảng trắng.";
                return false;
            }

            if (trimmedUsername.Length > 50)
            {
                errorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(trimmedFullName))
            {
                errorMessage = "Họ và tên không được để trống.";
                return false;
            }

            if (trimmedFullName.Length > 100)
            {
                errorMessage = "Họ và tên không được vượt quá 100 ký tự.";
                return false;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                errorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return false;
            }

            if (password.Length > 100)
            {
                errorMessage = "Mật khẩu không được vượt quá 100 ký tự.";
                return false;
            }

            string normalizedRole = NormalizeRole(role);
            if (string.IsNullOrEmpty(normalizedRole))
            {
                errorMessage = "Vai trò tài khoản phải là Member, Staff hoặc Admin.";
                return false;
            }

            if (initialBalance < 0)
            {
                errorMessage = "Số dư ban đầu không được âm.";
                return false;
            }

            if (string.Equals(normalizedRole, "Member", StringComparison.OrdinalIgnoreCase) &&
                initialBalance < MinimumMemberInitialBalance)
            {
                errorMessage = "Số dư ban đầu của hội viên phải từ 10.000 VNĐ trở lên.";
                return false;
            }

            if (initialBalance > MaximumTopUpAmount)
            {
                errorMessage = "Số dư ban đầu không được vượt quá 50.000.000 VNĐ.";
                return false;
            }

            if (decimal.Round(initialBalance, 2) != initialBalance)
            {
                errorMessage = "Số dư chỉ được có tối đa 2 chữ số thập phân.";
                return false;
            }

            return true;
        }

        public bool IsUsernameExists(string username, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            string trimmedUsername = username.Trim();

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                string sql = excludeUserId.HasValue
                    ? "SELECT COUNT(1) FROM AppUser WHERE Username = @Username AND UserId <> @ExcludeUserId"
                    : "SELECT COUNT(1) FROM AppUser WHERE Username = @Username";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                if (excludeUserId.HasValue)
                    cmd.Parameters.Add("@ExcludeUserId", SqlDbType.Int).Value = excludeUserId.Value;

                object? result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value && Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
            }
            catch
            {
                return _fallbackUsers.Any(u =>
                    u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase) &&
                    (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
            }
        }

        public bool ValidateNewMember(
            string username,
            string fullName,
            string password,
            decimal initialBalance,
            out string errorMessage,
            string role = "Member")
        {
            if (!ValidateMemberInput(username, fullName, password, initialBalance, role, out errorMessage))
                return false;

            if (IsUsernameExists(username.Trim()))
            {
                errorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                return false;
            }

            return true;
        }

        public bool CreateMember(
            string username,
            string fullName,
            string password,
            decimal initialBalance,
            out string errorMessage,
            string role = "Member",
            string? requestedTier = null)
        {
            var user = new AppUser
            {
                Username = username?.Trim() ?? string.Empty,
                FullName = fullName?.Trim() ?? string.Empty,
                Role = string.IsNullOrWhiteSpace(role) ? "Member" : role.Trim(),
                MemberTier = NormalizeTier(requestedTier),
                Balance = Math.Max(0m, initialBalance)
            };

            if (initialBalance < 0)
            {
                errorMessage = "Số dư ban đầu không được âm.";
                return false;
            }

            return CreateMember(user, password, out errorMessage);
        }

        public bool CreateMember(AppUser entity, string rawPassword, out string errorMessage)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (!ValidateNewMember(entity.Username, entity.FullName, rawPassword, entity.Balance, out errorMessage, entity.Role))
                return false;

            string trimmedUsername = entity.Username.Trim();
            string trimmedFullName = entity.FullName.Trim();
            string normalizedRole = NormalizeRole(entity.Role);
            string resolvedTier = AppUser.ResolveUpgradedTier(NormalizeTier(entity.MemberTier), entity.Balance);
            string passwordHash = HashPassword(rawPassword);

            entity.Username = trimmedUsername;
            entity.FullName = trimmedFullName;
            entity.Role = normalizedRole;
            entity.MemberTier = resolvedTier;
            entity.PasswordHash = passwordHash;

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt)
                                     OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                                     VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE())";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = trimmedFullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = normalizedRole;
                cmd.Parameters.Add(CreateDecimalParameter("@Balance", entity.Balance));
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = resolvedTier;

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    entity.UserId = reader.GetInt32(0);
                    entity.CreatedAt = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1);
                    return true;
                }

                errorMessage = "Không thể tạo tài khoản hội viên.";
                return false;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                errorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                return false;
            }
            catch
            {
                if (_fallbackUsers.Any(u => u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                    return false;
                }

                entity.UserId = _fallbackUsers.Count > 0 ? _fallbackUsers.Max(u => u.UserId) + 1 : 1;
                entity.CreatedAt = DateTime.Now;
                _fallbackUsers.Add(CloneUser(entity));
                return true;
            }
        }

        public Task<(bool Success, string ErrorMessage)> CreateMemberAsync(
            AppUser entity,
            string rawPassword)
        {
            return Task.Run(() =>
            {
                bool ok = CreateMember(entity, rawPassword, out string err);
                return (ok, err);
            });
        }

        // ==========================================
        // WALLET TOP-UP & SAFE DEDUCTION
        // ==========================================

        public bool TopUpBalance(int userId, decimal amount)
        {
            if (amount <= 0 || amount > MaximumTopUpAmount)
                return false;

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"UPDATE AppUser
                                     SET Balance = Balance + @Amount,
                                         MemberTier = CASE
                                             WHEN (Balance + @Amount) >= 500000 THEN N'Gold'
                                             WHEN (Balance + @Amount) >= 200000 AND MemberTier <> N'Gold' THEN N'Silver'
                                             ELSE MemberTier
                                         END
                                     WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(CreateDecimalParameter("@Amount", amount));
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                var user = _fallbackUsers.Find(u => u.UserId == userId);
                if (user == null)
                    return false;

                user.Balance += amount;
                user.MemberTier = AppUser.ResolveUpgradedTier(user.MemberTier, user.Balance);
                return true;
            }
        }

        public bool DeductBalance(int userId, decimal amount)
        {
            if (amount <= 0)
                return false;

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                const string sql = @"UPDATE AppUser
                                     SET Balance = Balance - @Amount
                                     WHERE UserId = @UserId AND Balance >= @Amount";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(CreateDecimalParameter("@Amount", amount));
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                var user = _fallbackUsers.Find(u => u.UserId == userId);
                if (user == null || user.Balance < amount)
                    return false;

                user.Balance -= amount;
                return true;
            }
        }

        public void DeductBalanceOrThrow(int userId, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.", nameof(amount));

            var user = GetById(userId) ?? throw new InvalidOperationException("Không tìm thấy tài khoản hội viên.");
            if (!user.HasSufficientBalance(amount))
                throw new InsufficientBalanceException(user.Balance, amount);

            if (!DeductBalance(userId, amount))
            {
                var refreshed = GetById(userId);
                throw new InsufficientBalanceException(refreshed?.Balance ?? 0m, amount);
            }
        }

        private static List<AppUser> CloneFallbackUsers() => _fallbackUsers.Select(CloneUser).ToList();

        private static AppUser CloneUser(AppUser source)
        {
            return new AppUser
            {
                UserId = source.UserId,
                Username = source.Username,
                PasswordHash = source.PasswordHash,
                FullName = source.FullName,
                Role = source.Role,
                Balance = source.Balance,
                MemberTier = source.MemberTier,
                CreatedAt = source.CreatedAt
            };
        }
    }
}
