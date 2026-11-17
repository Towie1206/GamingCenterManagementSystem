using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GamingCenterManagementSystem.Models
{
    public sealed class ReceiptItem
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public static class ThermalReceiptFormatter
    {
        private const int Width = 40;
        private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

        private static string Money(decimal amount) => amount.ToString("#,0.############################", Vietnamese);

        public static string FormatReceipt(string? cyberName, string? invoiceNumber, string? stationOrCustomer,
            string? cashierName, List<ReceiptItem> items, decimal vatPercent = 8m)
        {
            ArgumentNullException.ThrowIfNull(items);
            if (vatPercent < 0 || vatPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(vatPercent));

            string safeCyberName = string.IsNullOrWhiteSpace(cyberName) ? "CYBER GAME" : cyberName.Trim();
            string safeInvoiceNumber = invoiceNumber ?? string.Empty;
            string safeStationOrCustomer = stationOrCustomer ?? string.Empty;
            string safeCashierName = cashierName ?? string.Empty;

            var receipt = new StringBuilder();
            string rule = new string('-', Width);
            string border = new string('=', Width);
            receipt.AppendLine(border);
            WriteLine(receipt, safeCyberName.ToUpper(Vietnamese));
            receipt.AppendLine("DỊCH VỤ PHÒNG GAME CHUYÊN NGHIỆP");
            receipt.AppendLine(border);
            WriteLine(receipt, $"Số HĐ: {safeInvoiceNumber}");
            WriteLine(receipt, $"Ngày: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", Vietnamese)}");
            WriteLine(receipt, $"Khách: {safeStationOrCustomer}");
            WriteLine(receipt, $"Thu ngân: {safeCashierName}");
            receipt.AppendLine(rule);
            receipt.AppendLine("Tên món          SL    Đơn giá    T.Tiền");

            decimal subtotal = 0m;
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(items), "Mục thanh toán không được rỗng.");
                if (item.Quantity <= 0 || item.UnitPrice < 0)
                    throw new ArgumentException("Số lượng phải dương và đơn giá phải không âm.", nameof(items));

                string itemName = item.ItemName ?? string.Empty;
                decimal lineTotal = item.Subtotal;
                string amounts = string.Format(Vietnamese, "{0,3} {1,9} {2,9}", item.Quantity, Money(item.UnitPrice), Money(lineTotal));
                if (itemName.Length + 1 + amounts.Length <= Width)
                    receipt.AppendLine(itemName.PadRight(Width - amounts.Length) + amounts);
                else
                {
                    WriteLine(receipt, itemName);
                    WriteLine(receipt, amounts.Length < Width ? amounts.PadLeft(Width) : amounts);
                }
                subtotal += lineTotal;
            }

            decimal vat = Math.Round(subtotal * (vatPercent / 100m), 0, MidpointRounding.AwayFromZero);
            receipt.AppendLine(rule);
            WriteLine(receipt, "Cộng tiền hàng: " + Money(subtotal) + " đ");
            WriteLine(receipt, "Thuế GTGT (" + vatPercent.ToString("G29", Vietnamese) + "%): " + Money(vat) + " đ");
            WriteLine(receipt, "TỔNG CỘNG: " + Money(subtotal + vat) + " đ");
            receipt.AppendLine(border);
            receipt.AppendLine("CẢM ƠN QUÝ KHÁCH - HẸN GẶP LẠI!");
            receipt.AppendLine("Wifi pass: cybergamevip2026");
            return receipt.ToString();
        }

        private static void WriteLine(StringBuilder receipt, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                receipt.AppendLine();
                return;
            }

            for (int start = 0; start < text.Length; start += Width)
                receipt.AppendLine(text.Substring(start, Math.Min(Width, text.Length - start)));
        }
    }
}
