using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    // Lớp mẫu truy vấn CSDL cho thành viên tham khảo
    public class DBStation : IRepository<ComputerStation>
    {
        public List<ComputerStation> GetAll()
        {
            List<ComputerStation> list = new List<ComputerStation>();

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT StationId, StationName, ZoneId, IpAddress, Status FROM ComputerStation";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ComputerStation
                        {
                            StationId = Convert.ToInt32(reader["StationId"]),
                            StationName = reader["StationName"].ToString() ?? "",
                            ZoneId = Convert.ToInt32(reader["ZoneId"]),
                            IpAddress = reader["IpAddress"].ToString() ?? "",
                            Status = reader["Status"].ToString() ?? "Available"
                        });
                    }
                }
            }
            return list;
        }

        public ComputerStation? GetById(int id)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT StationId, StationName, ZoneId, IpAddress, Status FROM ComputerStation WHERE StationId = @StationId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StationId", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ComputerStation
                            {
                                StationId = Convert.ToInt32(reader["StationId"]),
                                StationName = reader["StationName"].ToString() ?? "",
                                ZoneId = Convert.ToInt32(reader["ZoneId"]),
                                IpAddress = reader["IpAddress"].ToString() ?? "",
                                Status = reader["Status"].ToString() ?? "Available"
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool Add(ComputerStation entity)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO ComputerStation (StationName, ZoneId, IpAddress, Status) VALUES (@Name, @ZoneId, @Ip, @Status)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", entity.StationName);
                    cmd.Parameters.AddWithValue("@ZoneId", entity.ZoneId);
                    cmd.Parameters.AddWithValue("@Ip", entity.IpAddress);
                    cmd.Parameters.AddWithValue("@Status", entity.Status);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Update(ComputerStation entity)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE ComputerStation SET StationName = @Name, ZoneId = @ZoneId, IpAddress = @Ip, Status = @Status WHERE StationId = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", entity.StationName);
                    cmd.Parameters.AddWithValue("@ZoneId", entity.ZoneId);
                    cmd.Parameters.AddWithValue("@Ip", entity.IpAddress);
                    cmd.Parameters.AddWithValue("@Status", entity.Status);
                    cmd.Parameters.AddWithValue("@Id", entity.StationId);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int id)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM ComputerStation WHERE StationId = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateStatus(int stationId, string status)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE ComputerStation SET Status = @Status WHERE StationId = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Id", stationId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
