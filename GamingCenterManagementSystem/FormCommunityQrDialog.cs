using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QRCoder;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Hộp thoại QR cộng đồng Master Game: nhóm Zalo (nhận giftcode) và Fanpage.
    /// Dùng thư viện NuGet QRCoder để sinh mã QR trực tiếp từ link, không cần file ảnh.
    /// Mở bằng: using var dlg = new FormCommunityQrDialog(); dlg.ShowDialog(this);
    /// </summary>
    public class FormCommunityQrDialog : Form
    {
        // TODO(Nam): thay bằng link mời nhóm Zalo thật của quán (Zalo > Nhóm > Link tham gia nhóm).
        public const string ZaloGroupUrl = "https://zalo.me/g/REPLACE_WITH_REAL_GROUP_ID";
        public const string FanpageUrl = "https://fb.com/mastergame165";

        private readonly Image? _zaloQr;
        private readonly Image? _fanpageQr;

        public FormCommunityQrDialog()
        {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Cộng đồng Master Game - Quét mã tham gia";
            ClientSize = new Size(640, 470);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 247, 250);

            _zaloQr = TryCreateQrImage(ZaloGroupUrl);
            _fanpageQr = TryCreateQrImage(FanpageUrl);

            var lblTitle = new Label
            {
                Text = "KẾT NỐI CỘNG ĐỒNG MASTER GAME",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 30, 54),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 12),
                Size = new Size(640, 32)
            };

            var lblSub = new Label
            {
                Text = "Quét mã để vào nhóm Zalo nhận giftcode và theo dõi Fanpage\n165 Hoàng Công Chất, Phú Diễn, Bắc Từ Liêm, Hà Nội",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(70, 80, 110),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 44),
                Size = new Size(640, 40)
            };

            Controls.Add(lblTitle);
            Controls.Add(lblSub);
            Controls.Add(BuildQrCard(
                left: 25,
                caption: "💬 Nhóm Zalo Master Game",
                url: ZaloGroupUrl,
                qr: _zaloQr,
                warnIfPlaceholder: true));
            Controls.Add(BuildQrCard(
                left: 335,
                caption: "👍 Fanpage Master Game",
                url: FanpageUrl,
                qr: _fanpageQr,
                warnIfPlaceholder: false));

            var btnClose = new Button
            {
                Text = "Đóng",
                Size = new Size(110, 32),
                Location = new Point(265, 422),
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White
            };
            Controls.Add(btnClose);
            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        private Panel BuildQrCard(int left, string caption, string url, Image? qr, bool warnIfPlaceholder)
        {
            var card = new Panel
            {
                Location = new Point(left, 95),
                Size = new Size(280, 315),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblCaption = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 51, 102),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 8),
                Size = new Size(278, 26)
            };

            var picture = new PictureBox
            {
                Location = new Point(35, 40),
                Size = new Size(208, 208),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = qr,
                BackColor = Color.White
            };

            bool isPlaceholder = warnIfPlaceholder && url.Contains("REPLACE_WITH", StringComparison.Ordinal);

            var link = new LinkLabel
            {
                Text = isPlaceholder ? "(Chưa cấu hình link nhóm Zalo)" : url,
                Font = new Font("Segoe UI", 9F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 252),
                Size = new Size(268, 22),
                Enabled = !isPlaceholder
            };
            link.LinkClicked += (s, e) => OpenUrl(url);

            var btnCopy = new Button
            {
                Text = "📋 Sao chép link",
                Size = new Size(140, 28),
                Location = new Point(70, 279),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Enabled = !isPlaceholder
            };
            btnCopy.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(url);
                    btnCopy.Text = "✔ Đã sao chép";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể sao chép link: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            card.Controls.Add(lblCaption);
            card.Controls.Add(picture);
            card.Controls.Add(link);
            card.Controls.Add(btnCopy);
            return card;
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở liên kết: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static Image? TryCreateQrImage(string content)
        {
            try
            {
                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                var png = new PngByteQRCode(data);
                byte[] bytes = png.GetGraphic(10);
                using var stream = new MemoryStream(bytes);
                using var temp = Image.FromStream(stream);
                return new Bitmap(temp); // copy để có thể giải phóng stream
            }
            catch
            {
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _zaloQr?.Dispose();
                _fanpageQr?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}