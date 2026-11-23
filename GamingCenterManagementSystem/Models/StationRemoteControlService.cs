using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GamingCenterManagementSystem.Models
{
    public enum RemoteCommandType
    {
        ForceLock,
        Unlock,
        TransferStation,
        AlertPopup,
        BroadcastPopup,
        KillRunningApp,
        Shutdown,
        Restart,
        CaptureScreenshot
    }

    public sealed class RunningAppInfo
    {
        public string ProcessName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public int MemoryMb { get; set; } = 512;
        public bool IsGame { get; set; } = true;

        public RunningAppInfo() { }

        public RunningAppInfo(string processName, string displayName, int memoryMb = 512, bool isGame = true)
        {
            ProcessName = processName;
            DisplayName = displayName;
            MemoryMb = memoryMb;
            IsGame = isGame;
            StartTime = DateTime.Now;
        }

        public override string ToString() => $"{DisplayName} ({ProcessName}.exe) - {MemoryMb} MB";
    }

    public sealed class RemoteCommand
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public int TargetStationId { get; set; } // 0 = Broadcast toàn phòng máy
        public string TargetStationName { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = "Admin";
        public RemoteCommandType CommandType { get; set; }
        public string Payload { get; set; } = string.Empty;
        public string AdditionalInfo { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsExecuted { get; set; }

        public RemoteCommand() { }

        public RemoteCommand(
            int targetStationId,
            string targetStationName,
            string senderUsername,
            RemoteCommandType commandType,
            string payload,
            string additionalInfo = "")
        {
            TargetStationId = targetStationId;
            TargetStationName = targetStationName;
            SenderUsername = senderUsername;
            CommandType = commandType;
            Payload = payload;
            AdditionalInfo = additionalInfo;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Service quản lý hệ thống điều khiển & cưỡng chế máy trạm từ xa thời gian thực giữa Máy Tổng (Cashier/Admin) và Máy Con (Member/Client).
    /// Hỗ trợ khóa/mở khóa cưỡng chế, chuyển phiên máy, gửi popup cảnh báo, đóng game từ xa, tắt/reboot và chụp màn hình anti-cheat.
    /// </summary>
    public static class StationRemoteControlService
    {
        private static readonly object _syncLock = new();
        private static readonly Dictionary<int, string> _lockedStations = new();
        private static readonly Dictionary<int, List<RunningAppInfo>> _runningApps = new();
        private static readonly Dictionary<int, Image> _latestScreenshots = new();
        private static readonly List<RemoteCommand> _commandHistory = new();

        public static event Action<RemoteCommand>? OnCommandDispatched;
        public static event Action<int, bool, string?>? OnStationLockStateChanged;
        public static event Action<int, Image>? OnScreenshotReceived;
        public static event Action<int, IReadOnlyList<RunningAppInfo>>? OnRunningAppsUpdated;

        public static readonly IReadOnlyList<string> QuickAlertTemplates = new List<string>
        {
            "Yêu cầu giữ trật tự và không nói tục chửi bậy trong phòng máy!",
            "Nghiêm cấm hút thuốc lá trong phòng điều hòa (Khu vực cấm hút thuốc)!",
            "Tài khoản của bạn sắp hết tiền, vui lòng nạp thêm để không bị gián đoạn!",
            "Nhân viên đang mang đồ ăn / nước uống đến máy của bạn, vui lòng đợi 1 phút!",
            "Phòng máy sẽ tạm bảo trì đường truyền trong 15 phút tới, quý khách vui lòng lưu game!",
            "Phát hiện tiến trình bất thường nghi vấn gian lận (Cheat/Hack). Vui lòng dừng ngay!"
        }.AsReadOnly();

        public static RemoteCommand DispatchCommand(RemoteCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            lock (_syncLock)
            {
                _commandHistory.Add(command);

                if (command.CommandType == RemoteCommandType.ForceLock)
                {
                    _lockedStations[command.TargetStationId] = string.IsNullOrWhiteSpace(command.Payload)
                        ? "Khóa cưỡng chế từ Máy Chủ Quầy Thu Ngân"
                        : command.Payload;
                }
                else if (command.CommandType == RemoteCommandType.Unlock)
                {
                    _lockedStations.Remove(command.TargetStationId);
                }
            }

            var invocationList = OnCommandDispatched?.GetInvocationList();
            if (invocationList != null)
            {
                foreach (var handler in invocationList)
                {
                    try
                    {
                        ((Action<RemoteCommand>)handler)(command);
                    }
                    catch
                    {
                        // Suppress individual subscriber errors
                    }
                }
            }

            if (command.CommandType == RemoteCommandType.ForceLock || command.CommandType == RemoteCommandType.Unlock)
            {
                try
                {
                    bool isLocked = command.CommandType == RemoteCommandType.ForceLock;
                    string? reason = isLocked ? command.Payload : null;
                    OnStationLockStateChanged?.Invoke(command.TargetStationId, isLocked, reason);
                }
                catch
                {
                    // Suppress subscriber errors
                }
            }

            return command;
        }

        public static RemoteCommand SendForceLock(int stationId, string stationName, string reason, string operatorName)
        {
            var cmd = new RemoteCommand(
                stationId,
                stationName,
                operatorName,
                RemoteCommandType.ForceLock,
                string.IsNullOrWhiteSpace(reason) ? "Vi phạm quy định phòng máy" : reason);

            return DispatchCommand(cmd);
        }

        public static RemoteCommand SendUnlock(int stationId, string stationName, string operatorName)
        {
            var cmd = new RemoteCommand(
                stationId,
                stationName,
                operatorName,
                RemoteCommandType.Unlock,
                "Mở khóa từ Máy Tổng");

            return DispatchCommand(cmd);
        }

        public static RemoteCommand SendTransferStation(int sourceStationId, string sourceStationName, int targetStationId, string targetStationName, string operatorName)
        {
            var cmd = new RemoteCommand(
                sourceStationId,
                sourceStationName,
                operatorName,
                RemoteCommandType.TransferStation,
                targetStationId.ToString(),
                targetStationName);

            return DispatchCommand(cmd);
        }

        public static RemoteCommand SendAlertPopup(int stationId, string stationName, string message, string title, string operatorName, bool isBroadcast = false)
        {
            var cmd = new RemoteCommand(
                stationId,
                stationName,
                operatorName,
                isBroadcast ? RemoteCommandType.BroadcastPopup : RemoteCommandType.AlertPopup,
                message,
                string.IsNullOrWhiteSpace(title) ? "CẢNH BÁO TỪ QUẦY THU NGÂN" : title);

            return DispatchCommand(cmd);
        }

        public static RemoteCommand SendKillApp(int stationId, string stationName, string appOrProcessName, string operatorName)
        {
            var cmd = new RemoteCommand(
                stationId,
                stationName,
                operatorName,
                RemoteCommandType.KillRunningApp,
                appOrProcessName);

            return DispatchCommand(cmd);
        }

        public static RemoteCommand SendShutdownOrRestart(int stationId, string stationName, bool isRestart, int countdownSeconds, string operatorName)
        {
            var cmd = new RemoteCommand(
                stationId,
                stationName,
                operatorName,
                isRestart ? RemoteCommandType.Restart : RemoteCommandType.Shutdown,
                countdownSeconds.ToString());

            return DispatchCommand(cmd);
        }

        public static RemoteCommand SendCaptureScreenshotRequest(int stationId, string stationName, string operatorName)
        {
            var cmd = new RemoteCommand(
                stationId,
                stationName,
                operatorName,
                RemoteCommandType.CaptureScreenshot,
                "ScreenshotRequest");

            return DispatchCommand(cmd);
        }

        public static void ReportRunningApps(int stationId, IEnumerable<RunningAppInfo> apps)
        {
            List<RunningAppInfo> list;
            lock (_syncLock)
            {
                list = apps.ToList();
                _runningApps[stationId] = list;
            }

            try
            {
                OnRunningAppsUpdated?.Invoke(stationId, list.AsReadOnly());
            }
            catch
            {
                // Suppress subscriber errors
            }
        }

        public static void ReportScreenshot(int stationId, Image screenshot)
        {
            ArgumentNullException.ThrowIfNull(screenshot);

            lock (_syncLock)
            {
                if (_latestScreenshots.TryGetValue(stationId, out var oldImg))
                {
                    try { oldImg.Dispose(); } catch { }
                }
                _latestScreenshots[stationId] = screenshot;
            }

            try
            {
                OnScreenshotReceived?.Invoke(stationId, screenshot);
            }
            catch
            {
                // Suppress subscriber errors
            }
        }

        public static bool IsStationLocked(int stationId, out string? reason)
        {
            lock (_syncLock)
            {
                return _lockedStations.TryGetValue(stationId, out reason);
            }
        }

        public static IReadOnlyList<RunningAppInfo> GetRunningApps(int stationId)
        {
            lock (_syncLock)
            {
                if (_runningApps.TryGetValue(stationId, out var list))
                    return list.ToList().AsReadOnly();

                return new List<RunningAppInfo>
                {
                    new("LeagueClient", "Liên Minh Huyền Thoại (LoL)", 1850, true),
                    new("Valorant", "Valorant", 2200, true),
                    new("chrome", "Google Chrome (YouTube Music)", 680, false),
                    new("Discord", "Discord", 340, false)
                }.AsReadOnly();
            }
        }

        public static Image? GetLatestScreenshot(int stationId)
        {
            lock (_syncLock)
            {
                if (_latestScreenshots.TryGetValue(stationId, out var img))
                    return (Image)img.Clone();

                return GenerateSimulatedScreenshot(stationId);
            }
        }

        public static IReadOnlyList<RemoteCommand> GetCommandHistory(int? stationId = null)
        {
            lock (_syncLock)
            {
                if (stationId.HasValue)
                    return _commandHistory.Where(c => c.TargetStationId == stationId.Value || c.TargetStationId == 0).ToList().AsReadOnly();

                return _commandHistory.ToList().AsReadOnly();
            }
        }

        public static void ResetForTesting()
        {
            lock (_syncLock)
            {
                _lockedStations.Clear();
                _runningApps.Clear();
                foreach (var img in _latestScreenshots.Values)
                {
                    try { img.Dispose(); } catch { }
                }
                _latestScreenshots.Clear();
                _commandHistory.Clear();
            }
        }

        public static Image GenerateSimulatedScreenshot(int stationId)
        {
            var bmp = new Bitmap(640, 360);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(15, 23, 42)); // Slate dark background

            using var pen = new Pen(Color.FromArgb(30, 41, 59), 2);
            g.DrawRectangle(pen, 10, 10, 620, 340);

            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 10F, FontStyle.Regular);
            using var brushTitle = new SolidBrush(Color.FromArgb(56, 189, 248));
            using var brushWhite = new SolidBrush(Color.White);
            using var brushSub = new SolidBrush(Color.FromArgb(148, 163, 184));

            g.DrawString($"MASTER GAME 165 HOÀNG CÔNG CHẤT - PC-{stationId:00}", titleFont, brushTitle, 25, 25);
            g.DrawString($"Ảnh chụp màn hình từ xa (Remote Live Capture) • Thời gian: {DateTime.Now:HH:mm:ss dd/MM/yyyy}", subFont, brushSub, 25, 60);

            // Giả lập giao diện game đang chạy
            using var gameBrush = new SolidBrush(Color.FromArgb(30, 58, 138));
            g.FillRectangle(gameBrush, 30, 95, 580, 200);

            using var gameFont = new Font("Segoe UI", 12F, FontStyle.Bold);
            g.DrawString("🎮 RIOT GAMES - LEAGUE OF LEGENDS (IN-GAME 5v5 RANKED)", gameFont, brushWhite, 45, 115);

            using var hudFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            g.DrawString("Ping: 9ms | FPS: 360 FPS | Anti-Cheat Vanguard: Active (Normal)", hudFont, brushSub, 45, 145);
            g.DrawString("Khách chơi: Member | Trạng thái hệ thống: Ổn định", hudFont, brushSub, 45, 170);

            // Watermark
            using var wmBrush = new SolidBrush(Color.FromArgb(234, 88, 12));
            g.DrawString("MASTER SERVER SPY MONITOR - NO CHEAT DETECTED", new Font("Segoe UI", 9F, FontStyle.Bold), wmBrush, 30, 310);

            return bmp;
        }
    }
}
