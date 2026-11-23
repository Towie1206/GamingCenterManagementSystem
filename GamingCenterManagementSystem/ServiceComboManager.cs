using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem
{
    public sealed record ServiceCombo(string Name, decimal Price, string Food, string Drink);

    public sealed class ServiceComboManager
    {
        public static IReadOnlyList<ServiceCombo> Combos { get; } = Array.AsReadOnly(new[]
        {
            new ServiceCombo("Combo Cày Đêm", 40000m, "Mì tôm bò trứng", "Bò húc / Sting"),
            new ServiceCombo("Combo Năng Lượng", 50000m, "Cơm chiên dưa bò", "Cà phê sữa đá")
        });

        public static bool MatchesIngredient(int comboIndex, bool food, string name)
        {
            string[] names = (comboIndex, food) switch
            {
                (0, true) => new[] { "Mì tôm bò trứng" },
                (0, false) => new[] { "Bò húc", "Red Bull", "Sting", "Sting Dâu" },
                (1, true) => new[] { "Cơm chiên dưa bò" },
                (1, false) => new[] { "Cà phê sữa đá" },
                _ => Array.Empty<string>()
            };
            return names.Any(n => string.Equals(n, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private readonly Func<SqlConnection> connectionFactory;
        public ServiceComboManager() : this(Database.GetConnection) { }
        public ServiceComboManager(Func<SqlConnection> connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        // Allocate the fixed bundle price across two inventory lines, conserving cents.
        public static (decimal Food, decimal Drink) AllocatePrice(decimal price, decimal foodPrice, decimal drinkPrice)
        {
            if (price < 0 || foodPrice < 0 || drinkPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(price));
            decimal total = foodPrice + drinkPrice;
            decimal food = Math.Round(total == 0 ? price / 2 : price * (foodPrice / total), 2,
                MidpointRounding.AwayFromZero);
            return (food, price - food);
        }

        public void Order(int comboIndex, int sessionId, int foodId, int drinkId, int quantity)
        {
            if (comboIndex < 0 || comboIndex >= Combos.Count || quantity < 1 || quantity > 100
                || sessionId <= 0 || foodId <= 0 || drinkId <= 0 || foodId == drinkId)
                throw new ArgumentException("Chọn combo, phiên và hai món khác nhau; số lượng từ 1 đến 100.");

            ServiceCombo combo = Combos[comboIndex];
            using var connection = connectionFactory();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using (var command = new SqlCommand(
                "SELECT Status FROM dbo.GamingSession WITH (UPDLOCK,HOLDLOCK) WHERE SessionId=@Id",
                connection, transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = sessionId;
                if (!string.Equals(command.ExecuteScalar() as string, "Active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Phiên đã đóng hoặc không tồn tại.");
            }

            var prices = new Dictionary<int, decimal>();
            foreach (int id in new[] { foodId, drinkId }.OrderBy(id => id))
            {
                using var command = new SqlCommand(
                    "SELECT UnitPrice,StockQuantity,IsActive,ServiceName FROM dbo.ServiceItem WITH (UPDLOCK,HOLDLOCK) WHERE ServiceId=@Id",
                    connection, transaction);
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                using var reader = command.ExecuteReader();
                if (!reader.Read() || !reader.GetBoolean(2) || reader.GetInt32(1) < quantity)
                    throw new InvalidOperationException("Một thành phần đã ngừng bán hoặc không đủ tồn kho.");
                if (!MatchesIngredient(comboIndex, id == foodId, reader.GetString(3)))
                    throw new InvalidOperationException("Món được chọn không thuộc combo.");
                prices.Add(id, reader.GetDecimal(0));
            }

            var allocation = AllocatePrice(combo.Price, prices[foodId], prices[drinkId]);
            foreach (var line in new[] { (Id: foodId, Price: allocation.Food), (Id: drinkId, Price: allocation.Drink) })
            {
                using var command = new SqlCommand(@"
                    UPDATE dbo.ServiceItem SET StockQuantity=StockQuantity-@Quantity WHERE ServiceId=@Id;
                    INSERT dbo.SessionService(SessionId,ServiceId,Quantity,UnitPrice,TotalPrice,OrderedAt,Note)
                    VALUES(@SessionId,@Id,@Quantity,@Price,@Total,GETDATE(),@Note);", connection, transaction);
                command.Parameters.Add("@Id", SqlDbType.Int).Value = line.Id;
                command.Parameters.Add("@SessionId", SqlDbType.Int).Value = sessionId;
                command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                command.Parameters.Add(Money("@Price", line.Price));
                command.Parameters.Add(Money("@Total", line.Price * quantity));
                command.Parameters.Add("@Note", SqlDbType.NVarChar, 255).Value = combo.Name;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand(@"
                UPDATE gs SET ServiceCost=totals.Amount,TotalCost=gs.PlayCost+totals.Amount
                FROM dbo.GamingSession gs
                CROSS APPLY (SELECT SUM(TotalPrice) AS Itemized FROM dbo.SessionService WHERE SessionId=gs.SessionId) items
                CROSS APPLY (SELECT CASE WHEN gs.ServiceCost+@Charge>items.Itemized
                    THEN gs.ServiceCost+@Charge ELSE items.Itemized END AS Amount) totals
                WHERE gs.SessionId=@Id;", connection, transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = sessionId;
                command.Parameters.Add(Money("@Charge", combo.Price * quantity));
                if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không cập nhật được phiên.");
            }
            transaction.Commit();
        }

        private static SqlParameter Money(string name, decimal value) =>
            new(name, SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = value };
    }
}
