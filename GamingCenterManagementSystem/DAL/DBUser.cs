using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
        private static readonly object _fallbackLock = new();

        private readonly Func<SqlConnection> _connectionFactory;

        // Mock fallback data in case database is offline
        private static readonly List<AppUser> _fallbackUsers = new()
        {
            new AppUser { UserId = 1, Username = "admin", PasswordHash = HashPassword("123456"), FullName = "Quản Lý Cyber", Role = "Admin", Balance = 0, MemberTier = "Standard", CreatedAt = DateTime.Now.AddMonths(-3), IsActive = true },
            new AppUser { UserId = 2, Username = "gamer01", PasswordHash = HashPassword("123456"), FullName = "Nguyễn Văn A", Role = "Member", Balance = 50000, MemberTier = "Standard", CreatedAt = DateTime.Now.AddDays(-20), IsActive = true },
            new AppUser { UserId = 3, Username = "gamer02", PasswordHash = HashPassword("123456"), FullName = "Trần Thị B", Role = "Member", Balance = 120000, MemberTier = "Silver", CreatedAt = DateTime.Now.AddDays(-10), IsActive = true },
            new AppUser { UserId = 4, Username = "gamer03", PasswordHash = HashPassword("123456"), FullName = "Lê Hoàng C", Role = "Member", Balance = 300000, MemberTier = "Gold", CreatedAt = DateTime.Now.AddDays(-5), IsActive = true },
            new AppUser { UserId = 5, Username = "staff01", PasswordHash = HashPassword("123456"), FullName = "Nhân Viên Quầy", Role = "Staff", Balance = 0, MemberTier = "Standard", CreatedAt = DateTime.Now.AddDays(-2), IsActive = true }
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

                if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int iterations)
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
        // DATA MAPPING, PARSING & PARAMETER HELPERS
        // ==========================================

        public static bool TryParseCurrencyAmount(string? rawText, out decimal amount)
        {
            amount = 0m;
            if (string.IsNullOrWhiteSpace(rawText))
                return false;

            string cleaned = rawText
                .Replace("VNĐ", "", StringComparison.OrdinalIgnoreCase)
                .Replace("VND", "", StringComparison.OrdinalIgnoreCase)
                .Replace("đ", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (Regex.IsMatch(cleaned, @"^[+-]?\d{1,3}(\.\d{3})+$"))
                cleaned = cleaned.Replace(".", "");
            else if (Regex.IsMatch(cleaned, @"^[+-]?\d{1,3}(,\d{3})+$"))
                cleaned = cleaned.Replace(",", "");

            const NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;

            if (decimal.TryParse(cleaned, style, CultureInfo.InvariantCulture, out amount))
                return true;

            if (decimal.TryParse(cleaned, style, CultureInfo.CurrentCulture, out amount))
                return true;

            return decimal.TryParse(cleaned, style, CultureInfo.GetCultureInfo("vi-VN"), out amount);
        }

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
                CreatedAt = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7),
                IsActive = reader.FieldCount > 8 && !reader.IsDBNull(8) ? reader.GetBoolean(8) : true
            };
        }

        private static bool HasIsActiveColumn(SqlConnection conn, SqlTransaction? trans = null)
        {
            const string sql = "SELECT CASE WHEN COL_LENGTH('dbo.AppUser', 'IsActive') IS NOT NULL THEN 1 ELSE 0 END";
            using var cmd = new SqlCommand(sql, conn, trans);
            object? result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value && Convert.ToInt32(result, CultureInfo.InvariantCulture) == 1;
        }

        private static async Task<bool> HasIsActiveColumnAsync(SqlConnection conn, SqlTransaction? trans = null)
        {
            const string sql = "SELECT CASE WHEN COL_LENGTH('dbo.AppUser', 'IsActive') IS NOT NULL THEN 1 ELSE 0 END";
            using var cmd = new SqlCommand(sql, conn, trans);
            object? result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            return result != null && result != DBNull.Value && Convert.ToInt32(result, CultureInfo.InvariantCulture) == 1;
        }

        private static string GetUserSelectColumns(bool includeIsActive) =>
            includeIsActive
                ? "UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt, IsActive"
                : "UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt";

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

        private static bool TryNormalizeTier(string? tier, out string normalizedTier)
        {
            if (string.IsNullOrWhiteSpace(tier))
            {
                normalizedTier = "Standard";
                return true;
            }

            string trimmed = tier.Trim();
            string? match = AllowedTiers.FirstOrDefault(t => string.Equals(t, trimmed, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                normalizedTier = string.Empty;
                return false;
            }

            normalizedTier = match;
            return true;
        }

        // ==========================================
        // CRUD OPERATIONS (IRepository<AppUser>)
        // ==========================================

        public List<AppUser> GetAll()
        {
            var list = new List<AppUser>();
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                string cols = GetUserSelectColumns(HasIsActiveColumn(conn));
                string sql = $@"SELECT {cols}
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
            catch when (!opened)
            {
                return CloneFallbackUsers();
            }
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            var list = new List<AppUser>();
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;
                string cols = GetUserSelectColumns(await HasIsActiveColumnAsync(conn).ConfigureAwait(false));
                string sql = $@"SELECT {cols}
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
            catch when (!opened)
            {
                return CloneFallbackUsers();
            }
        }

        public AppUser? GetById(int id)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                string cols = GetUserSelectColumns(HasIsActiveColumn(conn));
                string sql = $@"SELECT {cols}
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
            catch when (!opened)
            {
                lock (_fallbackLock)
                {
                    var found = _fallbackUsers.Find(u => u.UserId == id);
                    return found == null ? null : CloneUser(found);
                }
            }
        }

        public AppUser? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            string trimmedUsername = username.Trim();
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                string cols = GetUserSelectColumns(HasIsActiveColumn(conn));
                string sql = $@"SELECT {cols}
                                FROM AppUser
                                WHERE Username = @Username";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapUser(reader);
                }
                return null;
            }
            catch when (!opened)
            {
                lock (_fallbackLock)
                {
                    var found = _fallbackUsers.Find(u => u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase));
                    return found == null ? null : CloneUser(found);
                }
            }
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            string trimmedUsername = username.Trim();
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;
                string cols = GetUserSelectColumns(await HasIsActiveColumnAsync(conn).ConfigureAwait(false));
                string sql = $@"SELECT {cols}
                                FROM AppUser
                                WHERE Username = @Username";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return MapUser(reader);
                }
                return null;
            }
            catch when (!opened)
            {
                lock (_fallbackLock)
                {
                    var found = _fallbackUsers.Find(u => u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase));
                    return found == null ? null : CloneUser(found);
                }
            }
        }

        // ==========================================
        // AUTHENTICATION (LOGIN & SESSION)
        // ==========================================

        public bool TryAuthenticate(string? username, string? password, out AppUser? authenticatedUser, out string errorMessage)
        {
            authenticatedUser = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return false;
            }

            AppUser? found = GetByUsername(username.Trim());
            if (found == null || !VerifyPassword(password, found.PasswordHash))
            {
                errorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return false;
            }

            if (!found.IsActive)
            {
                errorMessage = "Tài khoản này đã bị khóa. Vui lòng liên hệ Quản lý.";
                return false;
            }

            found.PasswordHash = string.Empty;
            authenticatedUser = found;
            return true;
        }

        public async Task<(bool Success, AppUser? User, string ErrorMessage)> AuthenticateAsync(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return (false, null, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
            }

            AppUser? found = await GetByUsernameAsync(username.Trim()).ConfigureAwait(false);
            if (found == null || !VerifyPassword(password, found.PasswordHash))
            {
                return (false, null, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            if (!found.IsActive)
            {
                return (false, null, "Tài khoản này đã bị khóa. Vui lòng liên hệ Quản lý.");
            }

            found.PasswordHash = string.Empty;
            return (true, found, string.Empty);
        }

        public AppUser? Authenticate(string? username, string? password) =>
            TryAuthenticate(username, password, out AppUser? user, out _) ? user : null;

        public bool SetUserActiveStatus(int userId, bool isActive)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                if (!HasIsActiveColumn(conn))
                    return false;

                const string sql = "UPDATE AppUser SET IsActive = @IsActive WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch when (!opened)
            {
                lock (_fallbackLock)
                {
                    var found = _fallbackUsers.Find(u => u.UserId == userId);
                    if (found == null)
                        return false;

                    found.IsActive = isActive;
                    return true;
                }
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
            if (entity.Balance < 0 || decimal.Round(entity.Balance, 2) != entity.Balance)
                throw new ArgumentException("Số dư tài khoản không hợp lệ.", nameof(entity));

            string role = NormalizeRole(entity.Role);
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Vai trò tài khoản không hợp lệ.", nameof(entity));

            if (!TryNormalizeTier(entity.MemberTier, out string normalizedTier))
                throw new ArgumentException("Hạng thẻ không hợp lệ (Standard, Silver, Gold).", nameof(entity));

            string tier = AppUser.ResolveUpgradedTier(normalizedTier, entity.Balance);
            string passwordHash = EnsurePasswordHash(entity.PasswordHash);

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                bool hasIsActive = HasIsActiveColumn(conn);
                string sql = hasIsActive
                    ? @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt, IsActive)
                        OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                        VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE(), @IsActive)"
                    : @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt)
                        OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                        VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE())";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;
                cmd.Parameters.Add(CreateDecimalParameter("@Balance", entity.Balance));
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = tier;
                if (hasIsActive)
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = entity.IsActive;

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    entity.UserId = reader.GetInt32(0);
                    entity.CreatedAt = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1);
                    entity.Username = username;
                    entity.FullName = fullName;
                    entity.Role = role;
                    entity.MemberTier = tier;
                    entity.PasswordHash = passwordHash;
                    return true;
                }
                return false;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại trong hệ thống.", ex);
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                lock (_fallbackLock)
                {
                    if (_fallbackUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("Tên đăng nhập đã tồn tại trong hệ thống.", ex);

                    entity.UserId = _fallbackUsers.Count > 0 ? _fallbackUsers.Max(u => u.UserId) + 1 : 1;
                    entity.CreatedAt = DateTime.Now;
                    entity.Username = username;
                    entity.FullName = fullName;
                    entity.Role = role;
                    entity.MemberTier = tier;
                    entity.PasswordHash = passwordHash;
                    _fallbackUsers.Add(CloneUser(entity));
                    return true;
                }
            }
        }

        public bool Update(AppUser entity)
        {
            ValidateUpdateEntity(entity, out string username, out string fullName, out string role, out string tier);

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
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
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                return UpdateFallback(entity.UserId, username, fullName, role, tier, ex);
            }
        }

        public async Task<bool> UpdateAsync(AppUser entity)
        {
            ValidateUpdateEntity(entity, out string username, out string fullName, out string role, out string tier);

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;
                const string sql = @"UPDATE AppUser
                                     SET Username = @Username, FullName = @FullName, Role = @Role, MemberTier = @MemberTier
                                     WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = tier;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = entity.UserId;

                return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new InvalidOperationException("Tên đăng nhập mới đã thuộc về một tài khoản khác.", ex);
            }
            catch (Exception ex) when (!opened && ex is not (ArgumentException or InvalidOperationException))
            {
                return UpdateFallback(entity.UserId, username, fullName, role, tier, ex);
            }
        }

        private static void ValidateUpdateEntity(
            AppUser entity,
            out string username,
            out string fullName,
            out string role,
            out string tier)
        {
            ArgumentNullException.ThrowIfNull(entity);

            username = entity.Username?.Trim() ?? string.Empty;
            fullName = entity.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50 || username.Any(char.IsWhiteSpace))
                throw new ArgumentException("Tên đăng nhập không hợp lệ.", nameof(entity));
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
                throw new ArgumentException("Họ và tên không hợp lệ.", nameof(entity));

            role = NormalizeRole(entity.Role);
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Vai trò tài khoản không hợp lệ.", nameof(entity));

            if (!TryNormalizeTier(entity.MemberTier, out tier))
                throw new ArgumentException("Hạng thẻ không hợp lệ (Standard, Silver, Gold).", nameof(entity));
        }

        private static bool UpdateFallback(
            int userId,
            string username,
            string fullName,
            string role,
            string tier,
            Exception inner)
        {
            lock (_fallbackLock)
            {
                if (_fallbackUsers.Any(u => u.UserId != userId && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Tên đăng nhập mới đã thuộc về một tài khoản khác.", inner);

                var idx = _fallbackUsers.FindIndex(u => u.UserId == userId);
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
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                const string sql = "DELETE FROM AppUser WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = id;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Không thể xóa hội viên đã có lịch sử phiên chơi trong hệ thống.", ex);
            }
            catch (Exception ex) when (!opened && ex is not InvalidOperationException)
            {
                lock (_fallbackLock)
                {
                    return _fallbackUsers.RemoveAll(u => u.UserId == id) > 0;
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;
                const string sql = "DELETE FROM AppUser WHERE UserId = @UserId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = id;
                return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Không thể xóa hội viên đã có lịch sử phiên chơi trong hệ thống.", ex);
            }
            catch (Exception ex) when (!opened && ex is not InvalidOperationException)
            {
                lock (_fallbackLock)
                {
                    return _fallbackUsers.RemoveAll(u => u.UserId == id) > 0;
                }
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
            if (!ValidatePasswordChangeInput(username, oldPassword, newPassword, out string trimmedUsername, out errorMessage))
                return false;

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
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
            catch when (!opened)
            {
                return ChangePasswordFallback(trimmedUsername, oldPassword, newPassword, out errorMessage);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(
            string username,
            string oldPassword,
            string newPassword)
        {
            if (!ValidatePasswordChangeInput(username, oldPassword, newPassword, out string trimmedUsername, out string errorMessage))
                return (false, errorMessage);

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;
                using var trans = conn.BeginTransaction();

                int userId;
                string storedPassword;

                const string selectQuery = @"SELECT UserId, PasswordHash
                                             FROM AppUser WITH (UPDLOCK, HOLDLOCK)
                                             WHERE Username = @Username";
                using (var cmd = new SqlCommand(selectQuery, conn, trans))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                    using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    if (!await reader.ReadAsync().ConfigureAwait(false))
                    {
                        return (false, "Tên đăng nhập hoặc mật khẩu hiện tại không chính xác.");
                    }

                    userId = reader.GetInt32(0);
                    storedPassword = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                }

                if (!VerifyPassword(oldPassword, storedPassword))
                {
                    return (false, "Tên đăng nhập hoặc mật khẩu hiện tại không chính xác.");
                }

                string newHash = HashPassword(newPassword);

                const string updateQuery = @"UPDATE AppUser
                                             SET PasswordHash = @NewPassword
                                             WHERE UserId = @UserId";
                using (var cmd = new SqlCommand(updateQuery, conn, trans))
                {
                    cmd.Parameters.Add("@NewPassword", SqlDbType.NVarChar, 255).Value = newHash;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    bool updated = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
                    await trans.CommitAsync().ConfigureAwait(false);
                    return updated
                        ? (true, string.Empty)
                        : (false, "Không thể cập nhật mật khẩu.");
                }
            }
            catch when (!opened)
            {
                bool ok = ChangePasswordFallback(trimmedUsername, oldPassword, newPassword, out string fallbackError);
                return (ok, fallbackError);
            }
        }

        private static bool ValidatePasswordChangeInput(
            string username,
            string oldPassword,
            string newPassword,
            out string trimmedUsername,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            trimmedUsername = username?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmedUsername) ||
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

            return true;
        }

        private static bool ChangePasswordFallback(
            string trimmedUsername,
            string oldPassword,
            string newPassword,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            lock (_fallbackLock)
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
            bool opened = false;

            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
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
            catch when (!opened)
            {
                lock (_fallbackLock)
                {
                    return _fallbackUsers.Any(u =>
                        u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase) &&
                        (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
                }
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
            if (!ValidateMemberInput(username, fullName, password, initialBalance, role, out errorMessage))
                return false;

            if (!TryNormalizeTier(requestedTier, out string normalizedTier))
            {
                errorMessage = "Hạng thẻ không hợp lệ (Standard, Silver, Gold).";
                return false;
            }

            var user = new AppUser
            {
                Username = username.Trim(),
                FullName = fullName.Trim(),
                Role = NormalizeRole(role),
                MemberTier = normalizedTier,
                Balance = initialBalance
            };

            return CreateMember(user, password, out errorMessage);
        }

        public bool CreateMember(AppUser entity, string rawPassword, out string errorMessage)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (!TryPrepareNewMember(entity, rawPassword, out string trimmedUsername, out string trimmedFullName,
                out string normalizedRole, out string resolvedTier, out string passwordHash, out errorMessage))
            {
                return false;
            }

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;

                if (IsUsernameExistsOnConnection(conn, trimmedUsername))
                {
                    errorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                    return false;
                }

                bool hasIsActive = HasIsActiveColumn(conn);
                string sql = hasIsActive
                    ? @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt, IsActive)
                        OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                        VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE(), @IsActive)"
                    : @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt)
                        OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                        VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE())";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = trimmedFullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = normalizedRole;
                cmd.Parameters.Add(CreateDecimalParameter("@Balance", entity.Balance));
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = resolvedTier;
                if (hasIsActive)
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = entity.IsActive;

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    entity.UserId = reader.GetInt32(0);
                    entity.CreatedAt = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1);
                    entity.Username = trimmedUsername;
                    entity.FullName = trimmedFullName;
                    entity.Role = normalizedRole;
                    entity.MemberTier = resolvedTier;
                    entity.PasswordHash = passwordHash;
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
            catch when (!opened)
            {
                return CreateMemberFallback(entity, trimmedUsername, trimmedFullName, normalizedRole, resolvedTier, passwordHash, out errorMessage);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> CreateMemberAsync(
            AppUser entity,
            string rawPassword)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (!TryPrepareNewMember(entity, rawPassword, out string trimmedUsername, out string trimmedFullName,
                out string normalizedRole, out string resolvedTier, out string passwordHash, out string errorMessage))
            {
                return (false, errorMessage);
            }

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;

                if (await IsUsernameExistsOnConnectionAsync(conn, trimmedUsername).ConfigureAwait(false))
                {
                    return (false, "Tên đăng nhập đã tồn tại trong hệ thống.");
                }

                bool hasIsActive = await HasIsActiveColumnAsync(conn).ConfigureAwait(false);
                string sql = hasIsActive
                    ? @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt, IsActive)
                        OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                        VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE(), @IsActive)"
                    : @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt)
                        OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                        VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE())";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = trimmedFullName;
                cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = normalizedRole;
                cmd.Parameters.Add(CreateDecimalParameter("@Balance", entity.Balance));
                cmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = resolvedTier;
                if (hasIsActive)
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = entity.IsActive;

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    entity.UserId = reader.GetInt32(0);
                    entity.CreatedAt = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1);
                    entity.Username = trimmedUsername;
                    entity.FullName = trimmedFullName;
                    entity.Role = normalizedRole;
                    entity.MemberTier = resolvedTier;
                    entity.PasswordHash = passwordHash;
                    return (true, string.Empty);
                }

                return (false, "Không thể tạo tài khoản hội viên.");
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return (false, "Tên đăng nhập đã tồn tại trong hệ thống.");
            }
            catch when (!opened)
            {
                bool ok = CreateMemberFallback(entity, trimmedUsername, trimmedFullName, normalizedRole, resolvedTier, passwordHash, out string err);
                return (ok, err);
            }
        }

        private static bool TryPrepareNewMember(
            AppUser entity,
            string rawPassword,
            out string trimmedUsername,
            out string trimmedFullName,
            out string normalizedRole,
            out string resolvedTier,
            out string passwordHash,
            out string errorMessage)
        {
            trimmedUsername = string.Empty;
            trimmedFullName = string.Empty;
            normalizedRole = string.Empty;
            resolvedTier = string.Empty;
            passwordHash = string.Empty;

            if (!ValidateMemberInput(entity.Username, entity.FullName, rawPassword, entity.Balance, entity.Role, out errorMessage))
                return false;

            if (!TryNormalizeTier(entity.MemberTier, out string normalizedTier))
            {
                errorMessage = "Hạng thẻ không hợp lệ (Standard, Silver, Gold).";
                return false;
            }

            trimmedUsername = entity.Username.Trim();
            trimmedFullName = entity.FullName.Trim();
            normalizedRole = NormalizeRole(entity.Role);
            resolvedTier = AppUser.ResolveUpgradedTier(normalizedTier, entity.Balance);
            passwordHash = HashPassword(rawPassword);
            return true;
        }

        private static bool IsUsernameExistsOnConnection(SqlConnection conn, string trimmedUsername)
        {
            const string sql = "SELECT COUNT(1) FROM AppUser WHERE Username = @Username";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
            object? result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value && Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
        }

        private static async Task<bool> IsUsernameExistsOnConnectionAsync(SqlConnection conn, string trimmedUsername)
        {
            const string sql = "SELECT COUNT(1) FROM AppUser WHERE Username = @Username";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = trimmedUsername;
            object? result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            return result != null && result != DBNull.Value && Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
        }

        private static bool CreateMemberFallback(
            AppUser entity,
            string trimmedUsername,
            string trimmedFullName,
            string normalizedRole,
            string resolvedTier,
            string passwordHash,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            lock (_fallbackLock)
            {
                if (_fallbackUsers.Any(u => u.Username.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                    return false;
                }

                entity.UserId = _fallbackUsers.Count > 0 ? _fallbackUsers.Max(u => u.UserId) + 1 : 1;
                entity.CreatedAt = DateTime.Now;
                entity.Username = trimmedUsername;
                entity.FullName = trimmedFullName;
                entity.Role = normalizedRole;
                entity.MemberTier = resolvedTier;
                entity.PasswordHash = passwordHash;
                _fallbackUsers.Add(CloneUser(entity));
                return true;
            }
        }

        // ==========================================
        // WALLET TOP-UP & SAFE DEDUCTION
        // ==========================================

        public bool TopUpBalance(int userId, decimal amount, string? paymentMethod = "Tiền mặt tại quầy")
        {
            if (amount <= 0 || amount > MaximumTopUpAmount || decimal.Round(amount, 2) != amount)
                return false;

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                decimal balanceBefore;
                string currentTier;
                const string selectSql = @"SELECT Balance, MemberTier
                                           FROM dbo.AppUser WITH (UPDLOCK, HOLDLOCK)
                                           WHERE UserId = @UserId";
                using (var selectCmd = new SqlCommand(selectSql, conn, trans))
                {
                    selectCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    using var reader = selectCmd.ExecuteReader();
                    if (!reader.Read())
                        return false;

                    balanceBefore = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                    currentTier = reader.IsDBNull(1) ? "Standard" : reader.GetString(1);
                }

                decimal balanceAfter = balanceBefore + amount;
                string nextTier = AppUser.ResolveUpgradedTier(currentTier, balanceAfter);

                const string updateSql = @"UPDATE dbo.AppUser
                                           SET Balance = @BalanceAfter, MemberTier = @MemberTier
                                           WHERE UserId = @UserId";
                using (var updateCmd = new SqlCommand(updateSql, conn, trans))
                {
                    updateCmd.Parameters.Add(CreateDecimalParameter("@BalanceAfter", balanceAfter));
                    updateCmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = nextTier;
                    updateCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    if (updateCmd.ExecuteNonQuery() <= 0)
                        return false;
                }

                InsertWalletTransactionIfTableExists(
                    conn,
                    trans,
                    userId,
                    sessionId: null,
                    transactionType: "TopUp",
                    amount: amount,
                    balanceBefore: balanceBefore,
                    balanceAfter: balanceAfter,
                    paymentMethod: string.IsNullOrWhiteSpace(paymentMethod) ? "Tiền mặt tại quầy" : paymentMethod.Trim(),
                    description: $"Nạp tiền vào ví (+{amount:N0} VNĐ)",
                    createdByUserId: CurrentUserSession.IsLoggedIn ? CurrentUserSession.UserId : null);

                trans.Commit();
                return true;
            }
            catch when (!opened)
            {
                return TopUpFallback(userId, amount);
            }
        }

        public async Task<bool> TopUpBalanceAsync(int userId, decimal amount, string? paymentMethod = "Tiền mặt tại quầy")
        {
            if (amount <= 0 || amount > MaximumTopUpAmount || decimal.Round(amount, 2) != amount)
                return false;

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                await conn.OpenAsync().ConfigureAwait(false);
                opened = true;
                using var trans = conn.BeginTransaction();

                decimal balanceBefore;
                string currentTier;
                const string selectSql = @"SELECT Balance, MemberTier
                                           FROM dbo.AppUser WITH (UPDLOCK, HOLDLOCK)
                                           WHERE UserId = @UserId";
                using (var selectCmd = new SqlCommand(selectSql, conn, trans))
                {
                    selectCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    using var reader = await selectCmd.ExecuteReaderAsync().ConfigureAwait(false);
                    if (!await reader.ReadAsync().ConfigureAwait(false))
                        return false;

                    balanceBefore = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                    currentTier = reader.IsDBNull(1) ? "Standard" : reader.GetString(1);
                }

                decimal balanceAfter = balanceBefore + amount;
                string nextTier = AppUser.ResolveUpgradedTier(currentTier, balanceAfter);

                const string updateSql = @"UPDATE dbo.AppUser
                                           SET Balance = @BalanceAfter, MemberTier = @MemberTier
                                           WHERE UserId = @UserId";
                using (var updateCmd = new SqlCommand(updateSql, conn, trans))
                {
                    updateCmd.Parameters.Add(CreateDecimalParameter("@BalanceAfter", balanceAfter));
                    updateCmd.Parameters.Add("@MemberTier", SqlDbType.NVarChar, 20).Value = nextTier;
                    updateCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    if (await updateCmd.ExecuteNonQueryAsync().ConfigureAwait(false) <= 0)
                        return false;
                }

                await InsertWalletTransactionIfTableExistsAsync(
                    conn,
                    trans,
                    userId,
                    sessionId: null,
                    transactionType: "TopUp",
                    amount: amount,
                    balanceBefore: balanceBefore,
                    balanceAfter: balanceAfter,
                    paymentMethod: string.IsNullOrWhiteSpace(paymentMethod) ? "Tiền mặt tại quầy" : paymentMethod.Trim(),
                    description: $"Nạp tiền vào ví (+{amount:N0} VNĐ)",
                    createdByUserId: CurrentUserSession.IsLoggedIn ? CurrentUserSession.UserId : null).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
                return true;
            }
            catch when (!opened)
            {
                return TopUpFallback(userId, amount);
            }
        }

        private static bool TopUpFallback(int userId, decimal amount)
        {
            lock (_fallbackLock)
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
            if (amount <= 0 || decimal.Round(amount, 2) != amount)
                return false;

            bool opened = false;
            try
            {
                using var conn = _connectionFactory();
                conn.Open();
                opened = true;
                using var trans = conn.BeginTransaction();

                decimal balanceBefore;
                const string selectSql = @"SELECT Balance
                                           FROM dbo.AppUser WITH (UPDLOCK, HOLDLOCK)
                                           WHERE UserId = @UserId";
                using (var selectCmd = new SqlCommand(selectSql, conn, trans))
                {
                    selectCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    using var reader = selectCmd.ExecuteReader();
                    if (!reader.Read())
                        return false;

                    balanceBefore = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                }

                if (balanceBefore < amount)
                    return false;

                decimal balanceAfter = balanceBefore - amount;
                const string updateSql = @"UPDATE dbo.AppUser
                                           SET Balance = Balance - @Amount
                                           WHERE UserId = @UserId AND Balance >= @Amount";
                using (var updateCmd = new SqlCommand(updateSql, conn, trans))
                {
                    updateCmd.Parameters.Add(CreateDecimalParameter("@Amount", amount));
                    updateCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    if (updateCmd.ExecuteNonQuery() <= 0)
                        return false;
                }

                InsertWalletTransactionIfTableExists(
                    conn,
                    trans,
                    userId,
                    sessionId: null,
                    transactionType: "Adjustment",
                    amount: amount,
                    balanceBefore: balanceBefore,
                    balanceAfter: balanceAfter,
                    paymentMethod: "Ví hội viên",
                    description: $"Trừ số dư ví (-{amount:N0} VNĐ)",
                    createdByUserId: CurrentUserSession.IsLoggedIn ? CurrentUserSession.UserId : null);

                trans.Commit();
                return true;
            }
            catch when (!opened)
            {
                lock (_fallbackLock)
                {
                    var user = _fallbackUsers.Find(u => u.UserId == userId);
                    if (user == null || user.Balance < amount)
                        return false;

                    user.Balance -= amount;
                    return true;
                }
            }
        }

        private static void InsertWalletTransactionIfTableExists(
            SqlConnection conn,
            SqlTransaction trans,
            int userId,
            int? sessionId,
            string transactionType,
            decimal amount,
            decimal balanceBefore,
            decimal balanceAfter,
            string? paymentMethod,
            string? description,
            int? createdByUserId)
        {
            const string sql = @"IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                 BEGIN
                                     DECLARE @SafeCreatedBy INT = CASE
                                         WHEN @CreatedByUserId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.AppUser WHERE UserId = @CreatedByUserId)
                                         THEN @CreatedByUserId
                                         ELSE NULL
                                     END;
                                     INSERT INTO dbo.WalletTransaction
                                         (UserId, SessionId, TransactionType, Amount, BalanceBefore, BalanceAfter, PaymentMethod, Description, CreatedByUserId)
                                     VALUES
                                         (@UserId, @SessionId, @TransactionType, @Amount, @BalanceBefore, @BalanceAfter, @PaymentMethod, @Description, @SafeCreatedBy);
                                 END";
            using var cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = (object?)sessionId ?? DBNull.Value;
            cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar, 30).Value = transactionType;
            cmd.Parameters.Add(CreateDecimalParameter("@Amount", amount));
            cmd.Parameters.Add(CreateDecimalParameter("@BalanceBefore", balanceBefore));
            cmd.Parameters.Add(CreateDecimalParameter("@BalanceAfter", balanceAfter));
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = (object?)paymentMethod ?? DBNull.Value;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)description ?? DBNull.Value;
            cmd.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = (object?)createdByUserId ?? DBNull.Value;
            cmd.ExecuteNonQuery();
        }

        private static async Task InsertWalletTransactionIfTableExistsAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int userId,
            int? sessionId,
            string transactionType,
            decimal amount,
            decimal balanceBefore,
            decimal balanceAfter,
            string? paymentMethod,
            string? description,
            int? createdByUserId)
        {
            const string sql = @"IF OBJECT_ID('dbo.WalletTransaction', 'U') IS NOT NULL
                                 BEGIN
                                     DECLARE @SafeCreatedBy INT = CASE
                                         WHEN @CreatedByUserId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.AppUser WHERE UserId = @CreatedByUserId)
                                         THEN @CreatedByUserId
                                         ELSE NULL
                                     END;
                                     INSERT INTO dbo.WalletTransaction
                                         (UserId, SessionId, TransactionType, Amount, BalanceBefore, BalanceAfter, PaymentMethod, Description, CreatedByUserId)
                                     VALUES
                                         (@UserId, @SessionId, @TransactionType, @Amount, @BalanceBefore, @BalanceAfter, @PaymentMethod, @Description, @SafeCreatedBy);
                                 END";
            using var cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@SessionId", SqlDbType.Int).Value = (object?)sessionId ?? DBNull.Value;
            cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar, 30).Value = transactionType;
            cmd.Parameters.Add(CreateDecimalParameter("@Amount", amount));
            cmd.Parameters.Add(CreateDecimalParameter("@BalanceBefore", balanceBefore));
            cmd.Parameters.Add(CreateDecimalParameter("@BalanceAfter", balanceAfter));
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = (object?)paymentMethod ?? DBNull.Value;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)description ?? DBNull.Value;
            cmd.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = (object?)createdByUserId ?? DBNull.Value;
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public void DeductBalanceOrThrow(int userId, decimal amount)
        {
            if (amount <= 0 || decimal.Round(amount, 2) != amount)
                throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0 và tối đa 2 chữ số thập phân.", nameof(amount));

            var user = GetById(userId) ?? throw new InvalidOperationException("Không tìm thấy tài khoản hội viên.");
            if (!user.HasSufficientBalance(amount))
                throw new InsufficientBalanceException(user.Balance, amount);

            if (!DeductBalance(userId, amount))
            {
                var refreshed = GetById(userId);
                throw new InsufficientBalanceException(refreshed?.Balance ?? 0m, amount);
            }
        }

        private static List<AppUser> CloneFallbackUsers()
        {
            lock (_fallbackLock)
            {
                return _fallbackUsers.Select(CloneUser).ToList();
            }
        }

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
                CreatedAt = source.CreatedAt,
                IsActive = source.IsActive
            };
        }
    }
}
