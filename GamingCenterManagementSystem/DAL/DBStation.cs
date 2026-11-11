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
        private static readonly List<StationZone> _fallbackZones = new()
        {
            new StationZone { ZoneId = 1, ZoneName = "Khu Thường", BasePricePerHour = 8000, Description = "Cấu hình i5, GTX 1660, Màn 144Hz" },
            new StationZone { ZoneId = 2, ZoneName = "Khu VIP", BasePricePerHour = 12000, Description = "Cấu hình i7, RTX 3060, Màn 240Hz" },
            new StationZone { ZoneId = 3, ZoneName = "Đấu Trường LAN", BasePricePerHour = 18000, Description = "Dàn thi đấu 10 máy khép kín, RTX 4070, Màn 360Hz" }
        };

        private static readonly List<ComputerStation> _fallbackStations = new()
        {
            new ComputerStation { StationId = 1, StationName = "PC-01", ZoneId = 1, IpAddress = "192.168.1.101", Status = "Available" },
            new ComputerStation { StationId = 2, StationName = "PC-02", ZoneId = 1, IpAddress = "192.168.1.102", Status = "Available" },
            new ComputerStation { StationId = 3, StationName = "PC-03", ZoneId = 1, IpAddress = "192.168.1.103", Status = "Available" },
            new ComputerStation { StationId = 4, StationName = "PC-04", ZoneId = 2, IpAddress = "192.168.1.104", Status = "Available" },
            new ComputerStation { StationId = 5, StationName = "PC-05", ZoneId = 2, IpAddress = "192.168.1.105", Status = "Available" },
            new ComputerStation { StationId = 6, StationName = "PC-06", ZoneId = 3, IpAddress = "192.168.1.106", Status = "Available" },
            new ComputerStation { StationId = 7, StationName = "PC-07", ZoneId = 3, IpAddress = "192.168.1.107", Status = "Available" },
            new ComputerStation { StationId = 8, StationName = "PC-08", ZoneId = 3, IpAddress = "192.168.1.108", Status = "Available" },
            new ComputerStation { StationId = 9, StationName = "PC-09", ZoneId = 3, IpAddress = "192.168.1.109", Status = "Available" },
            new ComputerStation { StationId = 10, StationName = "PC-10", ZoneId = 3, IpAddress = "192.168.1.110", Status = "Available" }
        };

        public List<ComputerStation> GetAll()
        {
            List<ComputerStation> list = new List<ComputerStation>();

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT StationId, StationName, ZoneId, IpAddress, Status FROM ComputerStation ORDER BY StationId";

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
            catch
            {
                return new List<ComputerStation>(_fallbackStations);
            }
        }

        public List<StationZone> GetAllZones()
        {
            List<StationZone> list = new List<StationZone>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT ZoneId, ZoneName, BasePricePerHour, Description FROM StationZone ORDER BY ZoneId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new StationZone
                            {
                                ZoneId = Convert.ToInt32(reader["ZoneId"]),
                                ZoneName = reader["ZoneName"].ToString() ?? "",
                                BasePricePerHour = Convert.ToDecimal(reader["BasePricePerHour"]),
                                Description = reader["Description"]?.ToString()
                            });
                        }
                    }
                }
                return list;
            }
            catch
            {
                return new List<StationZone>(_fallbackZones);
            }
        }

        public ComputerStation? GetById(int id)
        {
            try
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
            }
            catch
            {
                return _fallbackStations.Find(s => s.StationId == id);
            }
            return null;
        }

        public bool Add(ComputerStation entity)
        {
            try
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
            catch
            {
                entity.StationId = _fallbackStations.Count > 0 ? _fallbackStations[^1].StationId + 1 : 1;
                _fallbackStations.Add(entity);
                return true;
            }
        }

        public bool Update(ComputerStation entity)
        {
            try
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
            catch
            {
                var idx = _fallbackStations.FindIndex(s => s.StationId == entity.StationId);
                if (idx >= 0)
                {
                    _fallbackStations[idx].StationName = entity.StationName;
                    _fallbackStations[idx].ZoneId = entity.ZoneId;
                    _fallbackStations[idx].IpAddress = entity.IpAddress;
                    _fallbackStations[idx].Status = entity.Status;
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
                    string sql = "DELETE FROM ComputerStation WHERE StationId = @Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                return _fallbackStations.RemoveAll(s => s.StationId == id) > 0;
            }
        }

        public bool UpdateStatus(int stationId, string status)
        {
            try
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
            catch
            {
                var station = _fallbackStations.Find(s => s.StationId == stationId);
                if (station != null)
                {
                    station.Status = status;
                    return true;
                }
                return false;
            }
        }

        public static void UpdateMultipleStatusesFallback(IEnumerable<int> stationIds, string status)
        {
            var idSet = new HashSet<int>(stationIds);
            foreach (var s in _fallbackStations)
            {
                if (idSet.Contains(s.StationId))
                {
                    s.Status = status;
                }
            }
        }
    }
}
