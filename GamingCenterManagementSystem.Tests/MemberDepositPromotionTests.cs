using System.Globalization;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class MemberDepositPromotionTests
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        // ---------- Mốc phần trăm ----------

        [Theory]
        [InlineData(-1000, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(100_000, 0)]
        [InlineData(499_999, 0)]
        [InlineData(500_000, 20)]
        [InlineData(750_000, 20)]
        [InlineData(999_999, 20)]
        [InlineData(1_000_000, 50)]
        [InlineData(1_500_000, 50)]
        [InlineData(1_999_999, 50)]
        [InlineData(2_000_000, 100)]
        [InlineData(5_000_000, 100)]
        [InlineData(50_000_000, 100)]
        public void GetBonusPercent_ReturnsExpectedTier(int cashAmount, int expectedPercent)
        {
            Assert.Equal((decimal)expectedPercent, DepositPromotionEngine.GetBonusPercent(cashAmount));
        }

        // ---------- Tính tiền thưởng ----------

        [Theory]
        [InlineData(500_000, 20, 100_000, 600_000)]
        [InlineData(1_000_000, 50, 500_000, 1_500_000)]
        [InlineData(2_000_000, 100, 2_000_000, 4_000_000)]
        [InlineData(999_999, 20, 199_999.8, 1_199_998.8)]
        [InlineData(1_999_999, 50, 999_999.5, 2_999_998.5)]
        public void Calculate_AppliesBonusPerTier(double cash, int percent, double bonus, double total)
        {
            var result = DepositPromotionEngine.Calculate((decimal)cash);

            Assert.Equal((decimal)cash, result.CashAmount);
            Assert.Equal((decimal)percent, result.BonusPercent);
            Assert.Equal((decimal)bonus, result.BonusAmount);
            Assert.Equal((decimal)total, result.TotalCredited);
            Assert.True(result.HasBonus);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50_000)]
        [InlineData(499_999)]
        public void Calculate_BelowFirstTier_GivesNoBonus(int cash)
        {
            var result = DepositPromotionEngine.Calculate(cash);

            Assert.Equal((decimal)cash, result.CashAmount);
            Assert.Equal(0m, result.BonusPercent);
            Assert.Equal(0m, result.BonusAmount);
            Assert.Equal((decimal)cash, result.TotalCredited);
            Assert.False(result.HasBonus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-500_000)]
        public void Calculate_NonPositiveAmount_ReturnsAllZero(int cash)
        {
            var result = DepositPromotionEngine.Calculate(cash);

            Assert.Equal(0m, result.CashAmount);
            Assert.Equal(0m, result.BonusPercent);
            Assert.Equal(0m, result.BonusAmount);
            Assert.Equal(0m, result.TotalCredited);
            Assert.False(result.HasBonus);
        }

        [Fact]
        public void Calculate_TotalAlwaysEqualsCashPlusBonus()
        {
            foreach (decimal cash in new[] { 20_000m, 499_999m, 500_000m, 999_999m, 1_000_000m, 1_999_999m, 2_000_000m, 12_345_678m })
            {
                var result = DepositPromotionEngine.Calculate(cash);
                Assert.Equal(result.CashAmount + result.BonusAmount, result.TotalCredited);
            }
        }

        [Fact]
        public void Calculate_KeepsTwoDecimalPlaces()
        {
            var result = DepositPromotionEngine.Calculate(500_000.55m);

            Assert.Equal(20m, result.BonusPercent);
            Assert.Equal(100_000.11m, result.BonusAmount);
            Assert.Equal(600_000.66m, result.TotalCredited);
        }

        // ---------- Nhãn preview ----------

        [Fact]
        public void BuildPreviewText_WithBonus_UsesExpectedFormat()
        {
            string text = DepositPromotionEngine.BuildPreviewText(500_000m, Invariant);

            Assert.Equal("Thưởng khuyến mãi: +100,000 đ (Tỷ lệ: 20%) => Tổng tiền vào ví: 600,000 đ", text);
        }

        [Fact]
        public void BuildPreviewText_TopTier_ShowsDoubleMoney()
        {
            string text = DepositPromotionEngine.BuildPreviewText(2_000_000m, Invariant);

            Assert.Equal("Thưởng khuyến mãi: +2,000,000 đ (Tỷ lệ: 100%) => Tổng tiền vào ví: 4,000,000 đ", text);
        }

        [Fact]
        public void BuildPreviewText_WithoutBonus_ShowsZeroBonus()
        {
            string text = DepositPromotionEngine.BuildPreviewText(200_000m, Invariant);

            Assert.Equal("Thưởng khuyến mãi: +0 đ (Tỷ lệ: 0%) => Tổng tiền vào ví: 200,000 đ", text);
        }

        // ---------- Gợi ý mốc kế tiếp ----------

        [Theory]
        [InlineData(300_000, "Nạp thêm 200,000 đ để nhận thưởng 20%")]
        [InlineData(600_000, "Nạp thêm 400,000 đ để nhận thưởng 50%")]
        [InlineData(1_500_000, "Nạp thêm 500,000 đ để nhận thưởng 100%")]
        [InlineData(2_000_000, "Đã đạt mốc thưởng cao nhất (100%)")]
        [InlineData(10_000_000, "Đã đạt mốc thưởng cao nhất (100%)")]
        public void BuildNextTierHint_ReturnsExpectedText(int cash, string expected)
        {
            Assert.Equal(expected, DepositPromotionEngine.BuildNextTierHint(cash, Invariant));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void BuildNextTierHint_InvalidAmount_ReturnsEmpty(int cash)
        {
            Assert.Equal(string.Empty, DepositPromotionEngine.BuildNextTierHint(cash, Invariant));
        }
    }
}