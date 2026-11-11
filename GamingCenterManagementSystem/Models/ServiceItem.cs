using System;

namespace GamingCenterManagementSystem.Models
{
    public class ServiceItem
    {
        private decimal _unitPrice;
        private int _stockQuantity;

        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = "Đồ uống";

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

        public int StockQuantity
        {
            get => _stockQuantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số lượng tồn kho không được âm.", nameof(value));
                _stockQuantity = value;
            }
        }

        public bool IsActive { get; set; } = true;

        public bool IsAvailableForOrder(int quantity = 1) =>
            IsActive && quantity > 0 && StockQuantity >= quantity;
    }
}
