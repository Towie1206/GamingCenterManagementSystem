using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;

namespace GamingCenterManagementSystem.DAL
{
    public class DBStation : IRepository<ComputerStation>
    {
        private const decimal MaxZonePrice = 9999999999999999.99m;

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
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = "SELECT StationId, StationName, ZoneId, IpAddress, Status FROM dbo.ComputerStation ORDER BY StationId";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ComputerStation
                    {
                        StationId = reader.GetInt32(0),
                        StationName = reader.GetString(1),
                        ZoneId = reader.GetInt32(2),
                        IpAddress = reader.GetString(3),
                        Status = reader.GetString(4)
                    });
                }
                return list;
            }
            catch when (!opened)
            {
                return new List<ComputerStation>(_fallbackStations);
            }
        }

        public List<StationZone> GetAllZones()
        {
            List<StationZone> list = new List<StationZone>();
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = "SELECT ZoneId, ZoneName, BasePricePerHour, Description FROM dbo.StationZone ORDER BY ZoneId";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new StationZone
                    {
                        ZoneId = reader.GetInt32(0),
                        ZoneName = reader.GetString(1),
                        BasePricePerHour = reader.GetDecimal(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
                return list;
            }
            catch when (!opened)
            {
                return new List<StationZone>(_fallbackZones);
            }
        }

        public ComputerStation? GetById(int id)
        {
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = "SELECT StationId, StationName, ZoneId, IpAddress, Status FROM dbo.ComputerStation WHERE StationId = @StationId";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StationId", SqlDbType.Int).Value = id;

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new ComputerStation
                    {
                        StationId = reader.GetInt32(0),
                        StationName = reader.GetString(1),
                        ZoneId = reader.GetInt32(2),
                        IpAddress = reader.GetString(3),
                        Status = reader.GetString(4)
                    };
                }
                return null;
            }
            catch when (!opened)
            {
                return _fallbackStations.Find(s => s.StationId == id);
            }
        }

        public bool IsStationNameExists(string stationName, int excludeStationId = 0)
        {
            string trimmedName = stationName.Trim();
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = @"SELECT COUNT(1)
                               FROM dbo.ComputerStation
                               WHERE LOWER(LTRIM(RTRIM(StationName))) = LOWER(LTRIM(RTRIM(@StationName)))
                                 AND StationId <> @ExcludeId";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StationName", SqlDbType.NVarChar, 50).Value = trimmedName;
                cmd.Parameters.Add("@ExcludeId", SqlDbType.Int).Value = excludeStationId;

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch when (!opened)
            {
                return _fallbackStations.Any(s =>
                    s.StationId != excludeStationId &&
                    s.StationName.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public bool Add(ComputerStation entity)
        {
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = @"INSERT INTO dbo.ComputerStation (StationName, ZoneId, IpAddress, Status)
                               VALUES (@Name, @ZoneId, @Ip, @Status)";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = entity.StationName.Trim();
                cmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = entity.ZoneId;
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 45).Value = entity.IpAddress.Trim();
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = entity.Status;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new InvalidOperationException("Tên máy trạm đã tồn tại trong CSDL.", ex);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Khu vực được chọn không hợp lệ hoặc không tồn tại.", ex);
            }
            catch when (!opened)
            {
                entity.StationId = _fallbackStations.Count > 0 ? _fallbackStations[^1].StationId + 1 : 1;
                _fallbackStations.Add(entity);
                return true;
            }
        }

        public bool Update(ComputerStation entity)
        {
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = @"UPDATE dbo.ComputerStation
                               SET StationName = @Name, ZoneId = @ZoneId, IpAddress = @Ip, Status = @Status
                               WHERE StationId = @Id";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = entity.StationName.Trim();
                cmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = entity.ZoneId;
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 45).Value = entity.IpAddress.Trim();
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = entity.Status;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = entity.StationId;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new InvalidOperationException("Tên máy trạm đã bị trùng với một máy khác trong CSDL.", ex);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Dữ liệu khu vực hoặc trạng thái không hợp lệ.", ex);
            }
            catch when (!opened)
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
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = "DELETE FROM dbo.ComputerStation WHERE StationId = @Id";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Không thể xóa máy trạm này vì đã có lịch sử phiên chơi hoặc dữ liệu giải đấu tham chiếu.", ex);
            }
            catch when (!opened)
            {
                return _fallbackStations.RemoveAll(s => s.StationId == id) > 0;
            }
        }

        public bool UpdateStatus(int stationId, string status)
        {
            bool opened = false;

            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string sql = "UPDATE dbo.ComputerStation SET Status = @Status WHERE StationId = @Id";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = stationId;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch when (!opened)
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

        // =========================================================================
        // NHIỆM VỤ 1: CRUD QUẢN LÝ KHU VỰC (StationZone)
        // =========================================================================
        private static bool ValidateZoneData(StationZone zone, out string cleanName, out string? cleanDescription, out string errorMessage)
        {
            cleanName = zone.ZoneName?.Trim() ?? string.Empty;
            cleanDescription = string.IsNullOrWhiteSpace(zone.Description) ? null : zone.Description.Trim();
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(cleanName) || cleanName.Length > 50)
            {
                errorMessage = "Tên khu vực không được để trống và tối đa 50 ký tự!";
                return false;
            }

            if (zone.BasePricePerHour <= 0m || zone.BasePricePerHour > MaxZonePrice || decimal.Round(zone.BasePricePerHour, 2) != zone.BasePricePerHour)
            {
                errorMessage = "Đơn giá/giờ phải lớn hơn 0, không vượt quá giới hạn hệ thống và tối đa 2 chữ số thập phân!";
                return false;
            }

            if (cleanDescription != null && cleanDescription.Length > 255)
            {
                errorMessage = "Mô tả khu vực không được vượt quá 255 ký tự!";
                return false;
            }

            return true;
        }

        public bool AddZone(StationZone zone, out string errorMessage)
        {
            if (!ValidateZoneData(zone, out string cleanName, out string? cleanDescription, out errorMessage))
            {
                return false;
            }

            bool opened = false;
            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string checkSql = @"SELECT COUNT(1)
                                    FROM dbo.StationZone
                                    WHERE LOWER(LTRIM(RTRIM(ZoneName))) = LOWER(LTRIM(RTRIM(@ZoneName)))";

                using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.Add("@ZoneName", SqlDbType.NVarChar, 50).Value = cleanName;
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Tên khu vực đã tồn tại!";
                        return false;
                    }
                }

                string insertSql = @"INSERT INTO dbo.StationZone (ZoneName, BasePricePerHour, Description)
                                     OUTPUT INSERTED.ZoneId
                                     VALUES (@ZoneName, @BasePricePerHour, @Description)";

                using SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                insertCmd.Parameters.Add("@ZoneName", SqlDbType.NVarChar, 50).Value = cleanName;

                var pPrice = insertCmd.Parameters.Add("@BasePricePerHour", SqlDbType.Decimal);
                pPrice.Precision = 18;
                pPrice.Scale = 2;
                pPrice.Value = zone.BasePricePerHour;

                insertCmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)cleanDescription ?? DBNull.Value;

                object? newId = insertCmd.ExecuteScalar();
                if (newId != null && newId != DBNull.Value)
                {
                    zone.ZoneId = Convert.ToInt32(newId);
                    zone.ZoneName = cleanName;
                    zone.Description = cleanDescription;
                    return true;
                }

                errorMessage = "Không thể thêm khu vực mới vào cơ sở dữ liệu.";
                return false;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                errorMessage = "Tên khu vực đã tồn tại!";
                return false;
            }
            catch (Exception ex) when (opened)
            {
                errorMessage = $"Lỗi CSDL khi thêm khu vực: {ex.Message}";
                return false;
            }
            catch when (!opened)
            {
                if (_fallbackZones.Any(z => z.ZoneName.Trim().Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = "Tên khu vực đã tồn tại!";
                    return false;
                }

                zone.ZoneId = _fallbackZones.Count > 0 ? _fallbackZones.Max(z => z.ZoneId) + 1 : 1;
                zone.ZoneName = cleanName;
                zone.Description = cleanDescription;
                _fallbackZones.Add(zone);
                return true;
            }
        }

        public bool UpdateZone(StationZone zone, out string errorMessage)
        {
            if (zone.ZoneId <= 0)
            {
                errorMessage = "Mã khu vực không hợp lệ!";
                return false;
            }

            if (!ValidateZoneData(zone, out string cleanName, out string? cleanDescription, out errorMessage))
            {
                return false;
            }

            bool opened = false;
            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string checkSql = @"SELECT COUNT(1)
                                    FROM dbo.StationZone
                                    WHERE LOWER(LTRIM(RTRIM(ZoneName))) = LOWER(LTRIM(RTRIM(@ZoneName)))
                                      AND ZoneId <> @ZoneId";

                using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.Add("@ZoneName", SqlDbType.NVarChar, 50).Value = cleanName;
                    checkCmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = zone.ZoneId;
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Tên khu vực này đã được dùng bởi khu khác!";
                        return false;
                    }
                }

                string updateSql = @"UPDATE dbo.StationZone
                                     SET ZoneName = @ZoneName,
                                         BasePricePerHour = @BasePricePerHour,
                                         Description = @Description
                                     WHERE ZoneId = @ZoneId";

                using SqlCommand updateCmd = new SqlCommand(updateSql, conn);
                updateCmd.Parameters.Add("@ZoneName", SqlDbType.NVarChar, 50).Value = cleanName;

                var pPrice = updateCmd.Parameters.Add("@BasePricePerHour", SqlDbType.Decimal);
                pPrice.Precision = 18;
                pPrice.Scale = 2;
                pPrice.Value = zone.BasePricePerHour;

                updateCmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)cleanDescription ?? DBNull.Value;
                updateCmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = zone.ZoneId;

                if (updateCmd.ExecuteNonQuery() > 0)
                {
                    zone.ZoneName = cleanName;
                    zone.Description = cleanDescription;
                    return true;
                }

                errorMessage = "Không tìm thấy khu vực cần cập nhật!";
                return false;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                errorMessage = "Tên khu vực này đã được dùng bởi khu khác!";
                return false;
            }
            catch (Exception ex) when (opened)
            {
                errorMessage = $"Lỗi CSDL khi cập nhật khu vực: {ex.Message}";
                return false;
            }
            catch when (!opened)
            {
                if (_fallbackZones.Any(z => z.ZoneId != zone.ZoneId && z.ZoneName.Trim().Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = "Tên khu vực này đã được dùng bởi khu khác!";
                    return false;
                }

                var existing = _fallbackZones.FirstOrDefault(z => z.ZoneId == zone.ZoneId);
                if (existing == null)
                {
                    errorMessage = "Không tìm thấy khu vực cần cập nhật!";
                    return false;
                }

                existing.ZoneName = cleanName;
                existing.BasePricePerHour = zone.BasePricePerHour;
                existing.Description = cleanDescription;
                return true;
            }
        }

        public bool DeleteZone(int zoneId, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (zoneId <= 0)
            {
                errorMessage = "Mã khu vực không hợp lệ!";
                return false;
            }

            bool opened = false;
            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                string checkSql = "SELECT COUNT(1) FROM dbo.ComputerStation WHERE ZoneId = @ZoneId";
                using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = zoneId;
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Không thể xóa khu vực đang có máy trạm! Vui lòng chuyển các máy sang khu khác trước.";
                        return false;
                    }
                }

                string deleteSql = "DELETE FROM dbo.StationZone WHERE ZoneId = @ZoneId";
                using SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
                deleteCmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = zoneId;

                if (deleteCmd.ExecuteNonQuery() > 0)
                {
                    return true;
                }

                errorMessage = "Không tìm thấy khu vực để xóa!";
                return false;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                errorMessage = "Không thể xóa khu vực đang có máy trạm! Vui lòng chuyển các máy sang khu khác trước.";
                return false;
            }
            catch (Exception ex) when (opened)
            {
                errorMessage = $"Lỗi CSDL khi xóa khu vực: {ex.Message}";
                return false;
            }
            catch when (!opened)
            {
                if (_fallbackStations.Any(s => s.ZoneId == zoneId))
                {
                    errorMessage = "Không thể xóa khu vực đang có máy trạm! Vui lòng chuyển các máy sang khu khác trước.";
                    return false;
                }

                if (_fallbackZones.RemoveAll(z => z.ZoneId == zoneId) > 0)
                {
                    return true;
                }

                errorMessage = "Không tìm thấy khu vực để xóa!";
                return false;
            }
        }

        // =========================================================================
        // NHIỆM VỤ 3: CHUYỂN MÁY ĐANG CHƠI AN TOÀN VỚI SQL TRANSACTION
        // =========================================================================
        public bool TransferStation(int sourceStationId, int targetStationId, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (sourceStationId <= 0 || targetStationId <= 0 || sourceStationId == targetStationId)
            {
                errorMessage = "Máy nguồn và máy đích không hợp lệ hoặc trùng nhau!";
                return false;
            }

            bool opened = false;
            try
            {
                using SqlConnection conn = Database.GetConnection();
                conn.Open();
                opened = true;

                using SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 1. Khóa và kiểm tra máy nguồn
                    string sourceStatus;
                    string sourceName;
                    string sqlSource = @"SELECT StationName, Status
                                         FROM dbo.ComputerStation WITH (UPDLOCK, HOLDLOCK)
                                         WHERE StationId = @SourceId";

                    using (SqlCommand cmdSource = new SqlCommand(sqlSource, conn, trans))
                    {
                        cmdSource.Parameters.Add("@SourceId", SqlDbType.Int).Value = sourceStationId;
                        using SqlDataReader reader = cmdSource.ExecuteReader();
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("Không tìm thấy máy nguồn trong hệ thống!");
                        }
                        sourceName = reader.GetString(0);
                        sourceStatus = reader.GetString(1);
                    }

                    if (!sourceStatus.Equals("InUse", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Máy nguồn [{sourceName}] không ở trạng thái đang chơi (InUse) nên không thể chuyển!");
                    }

                    // 2. Khóa và kiểm tra máy đích
                    string targetStatus;
                    string targetName;
                    string sqlTarget = @"SELECT StationName, Status
                                         FROM dbo.ComputerStation WITH (UPDLOCK, HOLDLOCK)
                                         WHERE StationId = @TargetId";

                    using (SqlCommand cmdTarget = new SqlCommand(sqlTarget, conn, trans))
                    {
                        cmdTarget.Parameters.Add("@TargetId", SqlDbType.Int).Value = targetStationId;
                        using SqlDataReader reader = cmdTarget.ExecuteReader();
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("Không tìm thấy máy đích trong hệ thống!");
                        }
                        targetName = reader.GetString(0);
                        targetStatus = reader.GetString(1);
                    }

                    if (!targetStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Máy đích [{targetName}] hiện không sẵn sàng (Trạng thái: {targetStatus})!");
                    }

                    // 3. Kiểm tra máy đích không có phiên đang mở hoặc chưa kết thúc
                    string sqlCheckTargetSession = @"SELECT COUNT(1)
                                                     FROM dbo.GamingSession WITH (UPDLOCK, HOLDLOCK)
                                                     WHERE StationId = @TargetId
                                                       AND (Status = N'Active' OR EndTime IS NULL)";

                    using (SqlCommand cmdCheckTarget = new SqlCommand(sqlCheckTargetSession, conn, trans))
                    {
                        cmdCheckTarget.Parameters.Add("@TargetId", SqlDbType.Int).Value = targetStationId;
                        if (Convert.ToInt32(cmdCheckTarget.ExecuteScalar()) > 0)
                        {
                            throw new InvalidOperationException($"Máy đích [{targetName}] đang còn phiên chơi chưa đóng trong lịch sử!");
                        }
                    }

                    // 4. Tìm phiên chơi Active mới nhất của máy nguồn
                    int activeSessionId;
                    string sqlFindSession = @"SELECT TOP 1 SessionId
                                              FROM dbo.GamingSession WITH (UPDLOCK, HOLDLOCK)
                                              WHERE StationId = @SourceId
                                                AND Status = N'Active'
                                              ORDER BY SessionId DESC";

                    using (SqlCommand cmdFindSession = new SqlCommand(sqlFindSession, conn, trans))
                    {
                        cmdFindSession.Parameters.Add("@SourceId", SqlDbType.Int).Value = sourceStationId;
                        object? sessionObj = cmdFindSession.ExecuteScalar();
                        if (sessionObj == null || sessionObj == DBNull.Value)
                        {
                            throw new InvalidOperationException($"Không tìm thấy phiên chơi Active của máy nguồn [{sourceName}]!");
                        }
                        activeSessionId = Convert.ToInt32(sessionObj);
                    }

                    // 5. Cập nhật phiên chơi sang máy đích
                    string sqlUpdateSession = @"UPDATE dbo.GamingSession
                                                SET StationId = @TargetId
                                                WHERE SessionId = @SessionId
                                                  AND Status = N'Active'";

                    using (SqlCommand cmdUpdateSession = new SqlCommand(sqlUpdateSession, conn, trans))
                    {
                        cmdUpdateSession.Parameters.Add("@TargetId", SqlDbType.Int).Value = targetStationId;
                        cmdUpdateSession.Parameters.Add("@SessionId", SqlDbType.Int).Value = activeSessionId;
                        if (cmdUpdateSession.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("Không thể cập nhật phiên chơi sang máy đích!");
                        }
                    }

                    // 6. Trả máy nguồn về Available
                    string sqlFreeSource = @"UPDATE dbo.ComputerStation
                                             SET Status = N'Available'
                                             WHERE StationId = @SourceId
                                               AND Status = N'InUse'";

                    using (SqlCommand cmdFreeSource = new SqlCommand(sqlFreeSource, conn, trans))
                    {
                        cmdFreeSource.Parameters.Add("@SourceId", SqlDbType.Int).Value = sourceStationId;
                        if (cmdFreeSource.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("Không thể giải phóng trạng thái máy nguồn!");
                        }
                    }

                    // 7. Chuyển máy đích sang InUse
                    string sqlOccupyTarget = @"UPDATE dbo.ComputerStation
                                               SET Status = N'InUse'
                                               WHERE StationId = @TargetId
                                                 AND Status = N'Available'";

                    using (SqlCommand cmdOccupyTarget = new SqlCommand(sqlOccupyTarget, conn, trans))
                    {
                        cmdOccupyTarget.Parameters.Add("@TargetId", SqlDbType.Int).Value = targetStationId;
                        if (cmdOccupyTarget.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("Không thể cập nhật trạng thái đang chơi cho máy đích!");
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    try { trans.Rollback(); } catch { }
                    errorMessage = ex.Message;
                    return false;
                }
            }
            catch when (!opened)
            {
                errorMessage = "Không thể kết nối SQL Server để đồng bộ phiên chơi (GamingSession). Vui lòng kiểm tra kết nối CSDL!";
                return false;
            }
        }
    }
}