using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    public class DBUser : IRepository<AppUser>
    {
        // Mock fallback data in case database is offline
        private static readonly List<AppUser> _fallbackUsers = new()
        {
            new AppUser { UserId = 1, Username = "admin", FullName = "Quản Lý Cyber", Role = "Admin", Balance = 0, MemberTier = "Standard", CreatedAt = DateTime.Now.AddMonths(-3) },
            new AppUser { UserId = 2, Username = "gamer01", FullName = "Nguyễn Văn A", Role = "Member", Balance = 50000, MemberTier = "Standard", CreatedAt = DateTime.Now.AddDays(-20) },
            new AppUser { UserId = 3, Username = "gamer02", FullName = "Trần Thị B", Role = "Member", Balance = 120000, MemberTier = "Silver", CreatedAt = DateTime.Now.AddDays(-10) },
            new AppUser { UserId = 4, Username = "gamer03", FullName = "Lê Hoàng C", Role = "Member", Balance = 300000, MemberTier = "Gold", CreatedAt = DateTime.Now.AddDays(-5) }
        };

        public List<AppUser> GetAll()
        {
            List<AppUser> list = new List<AppUser>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt FROM AppUser ORDER BY UserId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new AppUser
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Username = reader["Username"].ToString() ?? "",
                                PasswordHash = reader["PasswordHash"].ToString() ?? "",
                                FullName = reader["FullName"].ToString() ?? "",
                                Role = reader["Role"].ToString() ?? "Member",
                                Balance = Convert.ToDecimal(reader["Balance"]),
                                MemberTier = reader["MemberTier"].ToString() ?? "Standard",
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
                return list;
            }
            catch
            {
                // Return fallback data if database is not reachable
                return new List<AppUser>(_fallbackUsers);
            }
        }

        public AppUser? GetById(int id)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT UserId, Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt FROM AppUser WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new AppUser
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Username = reader["Username"].ToString() ?? "",
                                    PasswordHash = reader["PasswordHash"].ToString() ?? "",
                                    FullName = reader["FullName"].ToString() ?? "",
                                    Role = reader["Role"].ToString() ?? "Member",
                                    Balance = Convert.ToDecimal(reader["Balance"]),
                                    MemberTier = reader["MemberTier"].ToString() ?? "Standard",
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };
                            }
                        }
                    }
                }
            }
            catch
            {
                return _fallbackUsers.Find(u => u.UserId == id);
            }
            return null;
        }

        public bool Add(AppUser entity)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CreatedAt)
                                   VALUES (@Username, @PasswordHash, @FullName, @Role, @Balance, @MemberTier, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", entity.Username);
                        cmd.Parameters.AddWithValue("@PasswordHash", string.IsNullOrEmpty(entity.PasswordHash) ? "123456" : entity.PasswordHash);
                        cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                        cmd.Parameters.AddWithValue("@Role", entity.Role);
                        cmd.Parameters.AddWithValue("@Balance", entity.Balance);
                        cmd.Parameters.AddWithValue("@MemberTier", entity.MemberTier);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                entity.UserId = _fallbackUsers.Count > 0 ? _fallbackUsers[^1].UserId + 1 : 1;
                entity.CreatedAt = DateTime.Now;
                _fallbackUsers.Add(entity);
                return true;
            }
        }

        public bool Update(AppUser entity)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE AppUser 
                                   SET Username = @Username, FullName = @FullName, Role = @Role, MemberTier = @MemberTier
                                   WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", entity.Username);
                        cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                        cmd.Parameters.AddWithValue("@Role", entity.Role);
                        cmd.Parameters.AddWithValue("@MemberTier", entity.MemberTier);
                        cmd.Parameters.AddWithValue("@UserId", entity.UserId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var idx = _fallbackUsers.FindIndex(u => u.UserId == entity.UserId);
                if (idx >= 0)
                {
                    _fallbackUsers[idx].Username = entity.Username;
                    _fallbackUsers[idx].FullName = entity.FullName;
                    _fallbackUsers[idx].Role = entity.Role;
                    _fallbackUsers[idx].MemberTier = entity.MemberTier;
                    return true;
                }
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "DELETE FROM AppUser WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                return _fallbackUsers.RemoveAll(u => u.UserId == id) > 0;
            }
        }

        public bool TopUpBalance(int userId, decimal amount)
        {
            if (amount <= 0 || amount > 50000000) return false;

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE AppUser 
                                   SET Balance = Balance + @Amount,
                                       MemberTier = CASE 
                                           WHEN (Balance + @Amount) >= 500000 THEN 'Gold'
                                           WHEN (Balance + @Amount) >= 200000 THEN 'Silver'
                                           ELSE MemberTier
                                       END
                                   WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var user = _fallbackUsers.Find(u => u.UserId == userId);
                if (user != null)
                {
                    user.Balance += amount;
                    // Auto tier upgrade based on balance
                    if (user.Balance >= 500000) user.MemberTier = "Gold";
                    else if (user.Balance >= 200000) user.MemberTier = "Silver";
                    return true;
                }
                return false;
            }
        }

        public bool DeductBalance(int userId, decimal amount)
        {
            if (amount <= 0) return false;

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE AppUser 
                                   SET Balance = CASE WHEN Balance >= @Amount THEN Balance - @Amount ELSE 0 END
                                   WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                var user = _fallbackUsers.Find(u => u.UserId == userId);
                if (user != null)
                {
                    user.Balance = Math.Max(0, user.Balance - amount);
                    return true;
                }
                return false;
            }
        }
    }
}
