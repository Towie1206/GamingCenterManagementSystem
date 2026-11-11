using System;

namespace GamingCenterManagementSystem.Models
{
    public class SessionService
    {
        private int _quantity = 1;
        private decimal _unitPrice;

        public int SessionServiceId { get; set; }
        public int SessionId { get; set; }
        public int ServiceId { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Số lượng dịch vụ phải lớn hơn 0.", nameof(value));
                _quantity = value;
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Đơn giá dịch vụ không được âm.", nameof(value));
                _unitPrice = value;
            }
        }

        public decimal TotalPrice { get; set; }
        public DateTime OrderedAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }

        public decimal CalculateLineTotal() => Quantity * UnitPrice;
    }
}
