using System.Drawing;
using System.Windows.Forms;

namespace GamingCenterManagementSystem
{
    partial class FormRevenueReport
    {
        private DateTimePicker from = null!, to = null!;
        private ComboBox shift = null!, combo = null!, food = null!, drink = null!, sessions = null!;
        private NumericUpDown quantity = null!;
        private Button refresh = null!, order = null!;
        private DataGridView revenue = null!;
        private Label deposits = null!, ingredients = null!;

        private void InitializeComponent()
        {
            Text = "Combo F&B và doanh thu theo ca";
            Size = new Size(1000, 700);
            MinimumSize = new Size(850, 600);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10);
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var report = new TabPage("Doanh thu theo ca");
            var bundles = new TabPage("Gọi combo F&B") { AutoScroll = true };
            tabs.TabPages.AddRange(new[] { report, bundles });
            Controls.Add(tabs);
            var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, AutoScroll = true };
            from = new DateTimePicker { Value = System.DateTime.Today.AddDays(-7), Width = 180 };
            to = new DateTimePicker { Width = 180 };
            shift = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
            shift.Items.AddRange(new object[] { "Tất cả", "Happy Hours", "Giờ tiêu chuẩn" });
            shift.SelectedIndex = 0;
            refresh = new Button { Text = "Lọc / làm mới", AutoSize = true };
            filters.Controls.AddRange(new Control[] { new Label { Text = "Từ ngày", AutoSize = true }, from,
                new Label { Text = "Đến ngày", AutoSize = true }, to, shift, refresh });
            revenue = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false };
            deposits = new Label { Dock = DockStyle.Bottom, Height = 65 };
            var rule = new Label { Dock = DockStyle.Bottom, Height = 65,
                Text = "Doanh thu phiên được xếp ca theo lúc thanh toán: Happy Hours [23:00–11:00), giờ thường [11:00–23:00).\nTiền nạp và tiền thưởng được thống kê riêng theo thời điểm nạp." };
            report.Controls.AddRange(new Control[] { revenue, filters, deposits, rule });
            var inputs = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, Padding = new Padding(16) };
            combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            foreach (var option in ServiceComboManager.Combos) combo.Items.Add(option.Name);
            combo.SelectedIndex = 0;
            sessions = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            food = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            drink = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            quantity = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 1 };
            ingredients = new Label { Width = 740, Height = 65 };
            order = new Button { Text = "Gọi combo vào phiên", AutoSize = true };
            inputs.Controls.AddRange(new Control[] { new Label { Text = "Combo", AutoSize = true }, combo, ingredients,
                new Label { Text = "Mã phiên đang chơi", AutoSize = true }, sessions,
                new Label { Text = "Món ăn", AutoSize = true }, food, new Label { Text = "Đồ uống", AutoSize = true }, drink,
                new Label { Text = "Số combo", AutoSize = true }, quantity, order });
            bundles.Controls.Add(inputs);
        }
    }
}
