using System;

namespace GamingCenterManagementSystem.Exceptions
{
    public class StationInUseException : Exception
    {
        public int StationId { get; }
        public string StationName { get; } = string.Empty;

        public StationInUseException(string message) : base(message) { }

        public StationInUseException(int stationId, string stationName)
            : base($"Máy trạm {stationName} (ID: {stationId}) hiện đang có khách chơi, không thể thực hiện thao tác!")
        {
            StationId = stationId;
            StationName = stationName;
        }
    }
}
