using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using QRCoder;

namespace GamingCenterManagementSystem.Models
{
    /// <summary>
    /// Helper sinh chuỗi chuẩn VietQR (Napas247 / EMVCo) và hình ảnh mã QR động
    /// phục vụ nạp tiền ví hội viên và thanh toán dịch vụ F&B tại máy trạm.
    /// </summary>
    public static class VietQrHelper
    {
        public const string DefaultBankBin = "970422"; // MBBank (Ngân hàng TMCP Quân đội)
        public const string DefaultAccountNumber = "0868888888";
        public const string DefaultAccountName = "GAMING CENTER CYBER MATRIX";

        /// <summary>
        /// Tạo cú pháp chuyển khoản nạp tiền chuẩn Cyber Lounge: "NAP <STATION> <USERNAME>"
        /// Ví dụ: "NAP PC02 TOWIE1206"
        /// </summary>
        public static string BuildTransferSyntax(string stationName, string username)
        {
            string cleanStation = NormalizeCode(stationName);
            string cleanUser = NormalizeCode(username);

            if (string.IsNullOrWhiteSpace(cleanStation))
                cleanStation = "PC";
            if (string.IsNullOrWhiteSpace(cleanUser))
                cleanUser = "GUEST";

            return $"NAP {cleanStation} {cleanUser}";
        }

        /// <summary>
        /// Tạo cú pháp chuyển khoản thanh toán gọi món F&B tại máy: "ORDER <STATION> <ORDER_ID>"
        /// </summary>
        public static string BuildFnBOrderSyntax(string stationName, int orderId)
        {
            string cleanStation = NormalizeCode(stationName);
            if (string.IsNullOrWhiteSpace(cleanStation))
                cleanStation = "PC";

            return $"ORDER {cleanStation} #{orderId}";
        }

        /// <summary>
        /// Chuẩn hóa chuỗi mã trạm/tài khoản (bỏ ký tự đặc biệt, viết hoa, không dấu).
        /// </summary>
        public static string NormalizeCode(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string normalized = RemoveDiacritics(input.Trim()).ToUpperInvariant();
            // Cho phép chữ cái, chữ số và gạch ngang
            normalized = Regex.Replace(normalized, @"[^A-Z0-9\-]", "");
            return normalized;
        }

        private static string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(normalizedString.Length);

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace("Đ", "D").Replace("đ", "d");
        }

        /// <summary>
        /// Đóng gói TLV (Tag-Length-Value) theo chuẩn EMVCo.
        /// </summary>
        public static string FormatTlv(string tag, string value)
        {
            ArgumentNullException.ThrowIfNull(tag);
            ArgumentNullException.ThrowIfNull(value);
            return $"{tag}{value.Length:D2}{value}";
        }

        /// <summary>
        /// Tính mã kiểm tra CRC16-CCITT (poly 0x1021, init 0xFFFF) theo đặc tả EMVCo QR.
        /// </summary>
        public static string CalculateCrc16(string input)
        {
            ArgumentNullException.ThrowIfNull(input);

            int crc = 0xFFFF;
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            foreach (byte b in bytes)
            {
                crc ^= (b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = ((crc << 1) ^ 0x1021) & 0xFFFF;
                    else
                        crc = (crc << 1) & 0xFFFF;
                }
            }

            return crc.ToString("X4");
        }

        /// <summary>
        /// Sinh chuỗi payload EMVCo Napas247 chuẩn VietQR đầy đủ kèm CRC16.
        /// </summary>
        public static string GenerateNapas247Payload(
            string bankBin,
            string accountNumber,
            decimal amount,
            string transferContent)
        {
            if (string.IsNullOrWhiteSpace(bankBin))
                bankBin = DefaultBankBin;
            if (string.IsNullOrWhiteSpace(accountNumber))
                accountNumber = DefaultAccountNumber;
            if (string.IsNullOrWhiteSpace(transferContent))
                transferContent = "NAP TIEN GAMING";

            string cleanContent = RemoveDiacritics(transferContent).Trim();
            if (cleanContent.Length > 25)
                cleanContent = cleanContent.Substring(0, 25);

            var sb = new StringBuilder();

            // Tag 00: Payload Format Indicator ("01")
            sb.Append(FormatTlv("00", "01"));

            // Tag 01: Point of Initiation Method ("12" = Dynamic khi có số tiền, "11" = Static)
            sb.Append(FormatTlv("01", amount > 0m ? "12" : "11"));

            // Tag 38: Merchant Account Information (Napas247)
            // Sub-tag 00: Napas GUID = "A000000727"
            // Sub-tag 01: Beneficiary Bank (sub 00: BIN, sub 01: AccountNumber)
            // Sub-tag 02: Service Code = "QRIBFTTA"
            string sub01Bank = FormatTlv("00", bankBin.Trim()) + FormatTlv("01", accountNumber.Trim());
            string tag38Value = FormatTlv("00", "A000000727")
                              + FormatTlv("01", sub01Bank)
                              + FormatTlv("02", "QRIBFTTA");
            sb.Append(FormatTlv("38", tag38Value));

            // Tag 53: Transaction Currency (VND = 704)
            sb.Append(FormatTlv("53", "704"));

            // Tag 54: Transaction Amount (nếu > 0)
            if (amount > 0m)
            {
                long integerAmount = (long)Math.Round(amount);
                sb.Append(FormatTlv("54", integerAmount.ToString(CultureInfo.InvariantCulture)));
            }

            // Tag 58: Country Code ("VN")
            sb.Append(FormatTlv("58", "VN"));

            // Tag 62: Additional Data Field Template (sub 08: nội dung chuyển khoản)
            string tag62Value = FormatTlv("08", cleanContent);
            sb.Append(FormatTlv("62", tag62Value));

            // Tag 63: CRC16 Checksum (tính trên chuỗi kết thúc bằng "6304")
            sb.Append("6304");
            string payloadWithoutCrc = sb.ToString();
            string crc = CalculateCrc16(payloadWithoutCrc);

            return payloadWithoutCrc + crc;
        }

        /// <summary>
        /// Tạo payload nạp tiền ví nhanh: NAP <STATION> <USERNAME>
        /// </summary>
        public static string GenerateWalletTopUpPayload(
            string stationName,
            string username,
            decimal amount,
            string bankBin = DefaultBankBin,
            string accountNumber = DefaultAccountNumber)
        {
            string content = BuildTransferSyntax(stationName, username);
            return GenerateNapas247Payload(bankBin, accountNumber, amount, content);
        }

        /// <summary>
        /// Tạo URL gọi ảnh VietQR nhanh phục vụ preview trực tuyến.
        /// </summary>
        public static string GenerateVietQrUrl(
            string bankBin,
            string accountNumber,
            decimal amount,
            string transferContent,
            string accountName = DefaultAccountName)
        {
            if (string.IsNullOrWhiteSpace(bankBin)) bankBin = DefaultBankBin;
            if (string.IsNullOrWhiteSpace(accountNumber)) accountNumber = DefaultAccountNumber;
            long intAmount = (long)Math.Max(0, Math.Round(amount));
            string safeContent = Uri.EscapeDataString(transferContent ?? string.Empty);
            string safeName = Uri.EscapeDataString(accountName ?? string.Empty);

            return $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact2.png?amount={intAmount}&addInfo={safeContent}&accountName={safeName}";
        }

        /// <summary>
        /// Sinh đối tượng Bitmap trực quan cho mã QR Napas247 sử dụng QRCoder (hỗ trợ hiển thị trên WinForms PictureBox).
        /// </summary>
        public static Bitmap GenerateQrBitmap(string payload, int pixelsPerModule = 8)
        {
            if (string.IsNullOrWhiteSpace(payload))
                payload = "https://vietqr.io";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);

            var matrix = qrCodeData.ModuleMatrix;
            int moduleCount = matrix.Count;
            int pixelSize = Math.Max(2, pixelsPerModule);
            int quietZone = 2; // Vùng đệm trắng xung quanh chuẩn QR
            int totalModules = moduleCount + (quietZone * 2);
            int imageSize = totalModules * pixelSize;

            var bitmap = new Bitmap(imageSize, imageSize);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                using var brush = new SolidBrush(Color.Black);

                for (int y = 0; y < moduleCount; y++)
                {
                    for (int x = 0; x < moduleCount; x++)
                    {
                        if (matrix[y][x])
                        {
                            g.FillRectangle(
                                brush,
                                (x + quietZone) * pixelSize,
                                (y + quietZone) * pixelSize,
                                pixelSize,
                                pixelSize);
                        }
                    }
                }
            }

            return bitmap;
        }
    }
}
