using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem
{
    // Model representing a member
    public class Member
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public decimal Balance { get; set; }
        public string MemberTier { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class DBUser
    {
        private const decimal MinimumBalance = 10000m;
        private const int Iterations = 100000;

        // ==========================================
        // PASSWORD HASHING
        // ==========================================

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return "PBKDF2$" + Iterations + "$" +
                    Convert.ToBase64String(salt) + "$" +
                    Convert.ToBase64String(hash);
            }
        }

        private static bool VerifyPassword(
            string password, string storedPassword)
        {
            // Compatibility with old sample accounts
            if (!storedPassword.StartsWith("PBKDF2$"))
            {
                return string.Equals(
                    password,
                    storedPassword,
                    StringComparison.Ordinal);
            }

            try
            {
                string[] parts = storedPassword.Split('$');

                int iterations = int.Parse(parts[1]);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] storedHash = Convert.FromBase64String(parts[3]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] inputHash = pbkdf2.GetBytes(storedHash.Length);

                    return CryptographicOperations.FixedTimeEquals(
                        inputHash, storedHash);
                }
            }
            catch
            {
                return false;
            }
        }

        // ==========================================
        // GET ALL MEMBERS
        // ==========================================

        public static List<Member> GetAllMembers()
        {
            List<Member> members = new List<Member>();

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT UserId, Username, FullName,
                           Role, Balance, MemberTier, CreatedAt
                    FROM AppUser
                    WHERE Role = 'Member'
                    ORDER BY UserId DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        members.Add(new Member
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString() ?? "",
                            FullName = reader["FullName"].ToString() ?? "",
                            Role = reader["Role"].ToString() ?? "",
                            Balance = Convert.ToDecimal(reader["Balance"]),
                            MemberTier = reader["MemberTier"].ToString() ?? "",
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return members;
        }

        // ==========================================
        // LOGIC 1: CHANGE PASSWORD
        // ==========================================

        public static bool ChangePassword(
            string username,
            string oldPassword,
            string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(oldPassword) ||
                string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException(
                    "Vui lòng nhập đầy đủ thông tin.");
            }

            if (newPassword.Length < 6)
            {
                throw new ArgumentException(
                    "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                string selectQuery = @"
                    SELECT PasswordHash
                    FROM AppUser
                    WHERE Username = @Username AND Role = 'Member'";

                string? storedPassword;

                using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                {
                    cmd.Parameters.Add(
                        "@Username", SqlDbType.NVarChar, 50).Value = username;

                    storedPassword = cmd.ExecuteScalar() as string;
                }

                if (storedPassword == null)
                {
                    return false;
                }

                if (!VerifyPassword(oldPassword, storedPassword))
                {
                    return false;
                }

                string newHash = HashPassword(newPassword);

                string updateQuery = @"
                    UPDATE AppUser
                    SET PasswordHash = @NewPassword
                    WHERE Username = @Username AND Role = 'Member'";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.Add(
                        "@NewPassword", SqlDbType.NVarChar, 255).Value = newHash;

                    cmd.Parameters.Add(
                        "@Username", SqlDbType.NVarChar, 50).Value = username;

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ==========================================
        // LOGIC 3: CHECK DUPLICATE USERNAME
        // ==========================================

        public static bool IsUsernameExists(string username)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT COUNT(*)
                    FROM AppUser
                    WHERE Username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add(
                        "@Username", SqlDbType.NVarChar, 50).Value = username;

                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // ==========================================
        // LOGIC 3: VALIDATE NEW MEMBER
        // ==========================================

        public static bool ValidateNewMember(
            string username,
            string fullName,
            string password,
            decimal initialBalance,
            out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Username không được để trống.";
                return false;
            }

            if (username.Length > 50)
            {
                errorMessage = "Username không được vượt quá 50 ký tự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "Họ tên không được để trống.";
                return false;
            }

            if (fullName.Length > 100)
            {
                errorMessage = "Họ tên không được vượt quá 100 ký tự.";
                return false;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                errorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return false;
            }

            if (initialBalance < MinimumBalance)
            {
                errorMessage = "Số dư ban đầu phải từ 10.000 VNĐ.";
                return false;
            }

            if (initialBalance < 0)
            {
                errorMessage = "Số dư không hợp lệ.";
                return false;
            }

            if (IsUsernameExists(username))
            {
                errorMessage = "Username đã tồn tại trong hệ thống.";
                return false;
            }

            return true;
        }

        // ==========================================
        // LOGIC 3: CREATE NEW MEMBER
        // ==========================================

        public static bool CreateMember(
            string username,
            string fullName,
            string password,
            decimal initialBalance,
            out string errorMessage)
        {
            if (!ValidateNewMember(
                username, fullName, password,
                initialBalance, out errorMessage))
            {
                return false;
            }

            string passwordHash = HashPassword(password);

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO AppUser
                    (Username, PasswordHash, FullName,
                     Role, Balance, MemberTier)
                    VALUES
                    (@Username, @PasswordHash, @FullName,
                     'Member', @Balance, 'Standard')";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add(
                        "@Username", SqlDbType.NVarChar, 50).Value = username;

                    cmd.Parameters.Add(
                        "@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;

                    cmd.Parameters.Add(
                        "@FullName", SqlDbType.NVarChar, 100).Value = fullName;

                    SqlParameter balanceParam =
                        cmd.Parameters.Add("@Balance", SqlDbType.Decimal);

                    balanceParam.Precision = 18;
                    balanceParam.Scale = 2;
                    balanceParam.Value = initialBalance;

                    try
                    {
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2601 || ex.Number == 2627)
                        {
                            errorMessage = "Username đã tồn tại.";
                            return false;
                        }

                        throw;
                    }
                }
            }
        }
    }
}
