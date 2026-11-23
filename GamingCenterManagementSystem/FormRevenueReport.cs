using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public sealed record ShiftRevenue(string Period, int Sessions, decimal PlayRevenue, decimal ServiceRevenue, decimal Revenue);
    public sealed record DepositRevenue(DateTime CreatedAt, decimal Cash, decimal Bonus);

    public static class HappyHourRevenueAnalysis
    {
        // Attribute receipts to the shift when the session was settled.
        public static bool IsHappyHour(DateTime time) => time.Hour >= 23 || time.Hour < 11;

        public static List<ShiftRevenue> Summarize(IEnumerable<GamingSession> sessions, DateTime from, DateTime to)
        {
            ArgumentNullException.ThrowIfNull(sessions);
            if (from.Date > to.Date) throw new ArgumentException("Khoảng ngày không hợp lệ.");
            return sessions.Where(s => s.EndTime.HasValue && s.EndTime.Value.Date >= from.Date
                    && s.EndTime.Value.Date <= to.Date && string.Equals(s.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .GroupBy(s => IsHappyHour(s.EndTime!.Value))
                .Select(g => new ShiftRevenue(g.Key ? "Happy Hours" : "Giờ tiêu chuẩn", g.Count(),
                    g.Sum(s => s.PlayCost), g.Sum(s => s.ServiceCost), g.Sum(s => s.TotalCost)))
                .OrderBy(s => s.Period).ToList();
        }

        public static (decimal Cash, decimal Bonus) SumDeposits(IEnumerable<DepositRevenue> deposits,
            DateTime from, DateTime to, bool? happyHour)
        {
            ArgumentNullException.ThrowIfNull(deposits);
            if (from.Date > to.Date) throw new ArgumentException("Khoảng ngày không hợp lệ.");
            var selected = deposits.Where(d => d.CreatedAt.Date >= from.Date && d.CreatedAt.Date <= to.Date
                && (!happyHour.HasValue || IsHappyHour(d.CreatedAt) == happyHour.Value)).ToList();
            return (selected.Sum(d => d.Cash), selected.Sum(d => d.Bonus));
        }
    }

    public partial class FormRevenueReport : Form
    {
        private readonly DBBilling billing = new();
        public event Action? ComboOrdered;

        public FormRevenueReport()
        {
            InitializeComponent();
            Shown += (_, _) => Reload();
            refresh.Click += (_, _) => Reload();
            combo.SelectedIndexChanged += (_, _) => LoadIngredients();
            order.Click += (_, _) => OrderCombo();
        }

        private void Reload()
        {
            try
            {
                if (from.Value.Date > to.Value.Date) throw new ArgumentException("Từ ngày phải trước hoặc bằng đến ngày.");
                var summaries = HappyHourRevenueAnalysis.Summarize(billing.GetAllSessions(), from.Value, to.Value);
                string? period = shift.SelectedIndex == 1 ? "Happy Hours" : shift.SelectedIndex == 2 ? "Giờ tiêu chuẩn" : null;
                revenue.DataSource = summaries.Where(s => period == null || s.Period == period).ToList();
                LoadDeposits();
                sessions.DataSource = billing.GetActiveSessions();
                sessions.DisplayMember = "SessionId";
                sessions.ValueMember = "SessionId";
                LoadIngredients();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Không tải được dữ liệu"); }
        }

        private void LoadIngredients()
        {
            try
            {
                if (combo.SelectedIndex < 0) return;
                var items = billing.GetAllServiceItems(true);
                food.DataSource = items.Where(i => ServiceComboManager.MatchesIngredient(combo.SelectedIndex, true, i.ServiceName)).ToList();
                drink.DataSource = items.Where(i => ServiceComboManager.MatchesIngredient(combo.SelectedIndex, false, i.ServiceName)).ToList();
                food.DisplayMember = drink.DisplayMember = "ServiceName";
                food.ValueMember = drink.ValueMember = "ServiceId";
                order.Enabled = food.Items.Count > 0 && drink.Items.Count > 0 && sessions.Items.Count > 0;
                var selected = ServiceComboManager.Combos[combo.SelectedIndex];
                ingredients.Text = $"{selected.Food} + {selected.Drink} — {selected.Price:N0}đ. Thêm món còn thiếu tại danh mục F&B.";
            }
            catch (Exception ex) { order.Enabled = false; MessageBox.Show(this, ex.Message, "Không tải được món"); }
        }

        private void OrderCombo()
        {
            if (sessions.SelectedItem is not GamingSession session || food.SelectedItem is not ServiceItem meal
                || drink.SelectedItem is not ServiceItem beverage) return;
            order.Enabled = false;
            try
            {
                new ServiceComboManager().Order(combo.SelectedIndex, session.SessionId, meal.ServiceId,
                    beverage.ServiceId, (int)quantity.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chưa lưu combo");
                Reload();
                return;
            }
            MessageBox.Show(this, "Đã lưu combo và trừ kho các món thành phần.");
            Reload();
            try { ComboOrdered?.Invoke(); }
            catch (Exception ex) { MessageBox.Show(this, "Combo đã lưu. Cần làm mới màn hình chính: " + ex.Message); }
        }

        private void LoadDeposits()
        {
            deposits.Text = "Tiền nạp thực thu / thưởng: chưa có dữ liệu tách riêng.";
            using var connection = Database.GetConnection();
            connection.Open();
            using var schema = new SqlCommand(@"SELECT COUNT(*) FROM sys.columns
                WHERE object_id=OBJECT_ID('dbo.WalletTransaction') AND name IN ('GiaTriNap','TienKhuyenMai')", connection);
            if (Convert.ToInt32(schema.ExecuteScalar()) != 2) return;
            using var command = new SqlCommand(@"SELECT CreatedAt,GiaTriNap,TienKhuyenMai
                FROM dbo.WalletTransaction WHERE TransactionType=N'TopUp'
                AND CreatedAt>=@From AND CreatedAt<@Until", connection);
            command.Parameters.Add("@From", SqlDbType.DateTime2).Value = from.Value.Date;
            command.Parameters.Add("@Until", SqlDbType.DateTime2).Value = to.Value.Date.AddDays(1);
            var rows = new List<DepositRevenue>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(1) || reader.IsDBNull(2))
                {
                    deposits.Text = "Tiền nạp thực thu / thưởng: có giao dịch chưa được tách số liệu.";
                    return;
                }
                rows.Add(new DepositRevenue(reader.GetDateTime(0), reader.GetDecimal(1), reader.GetDecimal(2)));
            }
            bool? happy = shift.SelectedIndex == 0 ? null : shift.SelectedIndex == 1;
            var totals = HappyHourRevenueAnalysis.SumDeposits(rows, from.Value, to.Value, happy);
            deposits.Text = $"Tiền nạp thực thu: {totals.Cash:N0}đ | Thưởng vào ví: {totals.Bonus:N0}đ";
        }
    }
}
