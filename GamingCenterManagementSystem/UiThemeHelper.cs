using System.Drawing;
using System.Windows.Forms;

namespace GamingCenterManagementSystem
{
    /// <summary>
    /// Bảng màu và chuẩn hóa giao diện hiện đại (phong cách phần mềm quản lý phòng máy thương mại CSM / FNet / Gcafe+).
    /// </summary>
    public static class UiThemeHelper
    {
        public static readonly Color HeaderDark = Color.FromArgb(15, 23, 42);        // Slate 900
        public static readonly Color SubHeaderDark = Color.FromArgb(30, 41, 59);    // Slate 800
        public static readonly Color CanvasLight = Color.FromArgb(241, 245, 249);   // Slate 100
        public static readonly Color SurfaceWhite = Color.FromArgb(255, 255, 255);  // White
        public static readonly Color BorderSubtle = Color.FromArgb(203, 213, 225);  // Slate 300
        public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);      // Slate 900
        public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);   // Slate 600
        public static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);      // Blue 600
        public static readonly Color AccentSky = Color.FromArgb(56, 189, 248);      // Sky 400

        // Màu đặc trưng 3 dàn máy Master Game (theo sơ đồ thực tế)
        public static readonly Color ZoneStandardAccent = Color.FromArgb(220, 38, 38);   // Đỏ Standard (10K/h)
        public static readonly Color ZoneStandardBg = Color.FromArgb(254, 242, 242);
        public static readonly Color ZoneMasterAccent = Color.FromArgb(22, 163, 74);     // Xanh lá Master (13K/h)
        public static readonly Color ZoneMasterBg = Color.FromArgb(240, 253, 244);
        public static readonly Color ZoneMaximumAccent = Color.FromArgb(202, 138, 4);    // Vàng Gold Maximum (15K/h)
        public static readonly Color ZoneMaximumBg = Color.FromArgb(254, 252, 232);

        public static void StyleDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;

            dgv.BackgroundColor = SurfaceWhite;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(226, 232, 240);
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowTemplate.Height = 32;

            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = SubHeaderDark;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            dgv.DefaultCellStyle.BackColor = SurfaceWhite;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(6, 2, 6, 2);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimary;
        }

        public static void StyleSecondaryForm(Form form)
        {
            if (form == null) return;
            form.BackColor = CanvasLight;
            ApplyRecursiveStyles(form);
        }

        private static void ApplyRecursiveStyles(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is DataGridView dgv)
                {
                    StyleDataGridView(dgv);
                }
                else if (ctrl is GroupBox grp)
                {
                    grp.BackColor = SurfaceWhite;
                    grp.ForeColor = TextPrimary;
                }
                else if (ctrl is Button btn)
                {
                    btn.Cursor = Cursors.Hand;
                    if (btn.FlatStyle == FlatStyle.Flat && btn.FlatAppearance.BorderSize > 0)
                    {
                        btn.FlatAppearance.BorderColor = BorderSubtle;
                    }
                }

                if (ctrl.HasChildren)
                {
                    ApplyRecursiveStyles(ctrl);
                }
            }
        }

        public static (Color Accent, Color SoftBg, string HappyHourPriceText, string ShortSpec) GetMasterGameZoneMeta(string? zoneName, decimal basePrice)
        {
            string name = zoneName ?? string.Empty;
            decimal happyPrice = decimal.Round(basePrice * 0.6m / 1000m, 0) * 1000m;
            string happyText = $"{happyPrice / 1000m:0}K/h";

            if (name.Contains("Maximum", System.StringComparison.OrdinalIgnoreCase))
            {
                return (ZoneMaximumAccent, ZoneMaximumBg, "9K/h",
                    "AMD 9800X3D • ASUS TUF B850 • 64GB DDR5 • RTX 4070Ti Super 16GB • Acer 24\" 540Hz • Razer DA V3 8K • DrunkDeer A75 Pro");
            }

            if (name.Contains("Master", System.StringComparison.OrdinalIgnoreCase))
            {
                return (ZoneMasterAccent, ZoneMasterBg, "8K/h",
                    "i7-14700 • ASUS TUF B760 • 64GB DDR5 • RTX 4070Ti Super 16GB • AOC 27\" 2K 400Hz • EndgameGear OP1 • DareU EK108RT");
            }

            if (name.Contains("Standard", System.StringComparison.OrdinalIgnoreCase))
            {
                return (ZoneStandardAccent, ZoneStandardBg, "6K/h",
                    "i5-14400 • ASUS TUF B760 • 32GB DDR5 • RTX 4060Ti 8GB • Acer 27\" 2K 240Hz • EndgameGear XM1R • FL-Esport GP 108");
            }

            return (AccentBlue, Color.FromArgb(239, 246, 255), happyText, string.Empty);
        }
    }
}
