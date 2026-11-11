using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;
using GamingCenterManagementSystem.OOP;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class SystemTests
    {
        [Fact]
        public void TariffCalculator_HourlyTariff_CalculatesCorrectCost()
        {
            TariffCalculatorBase calculator = new HourlyTariff();
            DateTime start = new DateTime(2026, 11, 10, 8, 0, 0);
            DateTime end = start.AddHours(2.5);
            decimal basePrice = 10000;

            decimal cost = calculator.CalculateCost(start, end, basePrice);

            Assert.Equal(25000, cost);
            Assert.Equal("Cước Theo Giờ Thường", calculator.TariffName);
        }

        [Fact]
        public void TariffCalculator_NightComboTariff_FixedPriceWithinNight()
        {
            TariffCalculatorBase calculator = new NightComboTariff();
            DateTime start = new DateTime(2026, 11, 10, 22, 0, 0);
            DateTime end = new DateTime(2026, 11, 11, 5, 0, 0); // 5h AM
            decimal basePrice = 12000;

            decimal cost = calculator.CalculateCost(start, end, basePrice);

            Assert.Equal(45000, cost);
            Assert.Equal("Gói Combo Đêm Trọn Gói", calculator.TariffName);
        }

        [Fact]
        public void TariffCalculator_TournamentTariff_Applies50PercentDiscount()
        {
            TariffCalculatorBase calculator = new TournamentTariff();
            DateTime start = new DateTime(2026, 11, 10, 14, 0, 0);
            DateTime end = start.AddHours(4); // 4h @ 18k = 72k -> 50% = 36k
            decimal basePrice = 18000;

            decimal cost = calculator.CalculateCost(start, end, basePrice);

            Assert.Equal(36000, cost);
            Assert.Equal("Gói Cước Ưu Đãi Giải Đấu", calculator.TariffName);
        }

        [Fact]
        public void DBStation_ReturnsStationsAndZones()
        {
            var db = new DBStation();
            var stations = db.GetAll();
            var zones = db.GetAllZones();

            Assert.NotEmpty(stations);
            Assert.NotEmpty(zones);
            Assert.Contains(stations, s => s.StationName == "PC-01");
            Assert.Contains(zones, z => z.ZoneName == "Khu Thường");
        }

        [Fact]
        public void DBUser_TopUpBalance_IncreasesBalanceCorrectly()
        {
            var db = new DBUser();
            var user = db.GetById(2); // gamer01
            Assert.NotNull(user);

            decimal initialBalance = user.Balance;
            bool success = db.TopUpBalance(user.UserId, 50000);

            Assert.True(success);
            var updated = db.GetById(user.UserId);
            Assert.NotNull(updated);
            Assert.Equal(initialBalance + 50000, updated.Balance);
        }

        [Fact]
        public void DBTournament_LoadsTournamentsAndMatches()
        {
            var db = new DBTournament();
            var tournaments = db.GetAll();
            Assert.NotEmpty(tournaments);

            var firstTourney = tournaments.First();
            var matches = db.GetMatchesByTournament(firstTourney.TournamentId);
            Assert.NotNull(matches);
        }

        [Fact]
        public void DBBilling_FiltersSessionsByDateAndType()
        {
            var db = new DBBilling();
            var sessions = db.GetAllSessions();
            Assert.NotEmpty(sessions);

            var filtered = db.GetSessionsByFilter(DateTime.Today.AddDays(-10), DateTime.Today.AddDays(1), "Hourly", "Tất cả");
            Assert.NotNull(filtered);
            Assert.All(filtered, s => Assert.Equal("Hourly", s.TariffType));
        }

        [Fact]
        public void TariffCalculator_HourlyTariff_ClampsToMinimumDuration()
        {
            TariffCalculatorBase calculator = new HourlyTariff();
            DateTime start = new DateTime(2026, 11, 10, 10, 0, 0);
            DateTime end = start.AddMinutes(2); // Less than 6 minutes (0.1h)

            decimal cost = calculator.CalculateCost(start, end, 10000);

            // Minimum 0.1 hour * 10,000 = 1,000
            Assert.Equal(1000, cost);
        }

        [Fact]
        public void TariffCalculator_NightComboTariff_WithDaytimeOvertime_ChargesExtra()
        {
            TariffCalculatorBase calculator = new NightComboTariff();
            DateTime start = new DateTime(2026, 11, 10, 22, 0, 0);
            DateTime end = new DateTime(2026, 11, 11, 8, 0, 0); // 8 AM (2 hours past 6 AM limit)
            decimal basePrice = 10000;

            decimal cost = calculator.CalculateCost(start, end, basePrice);

            // 45,000 fixed + (2 hours * 10,000) = 65,000
            Assert.Equal(65000, cost);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10000)]
        public void DBUser_TopUpBalance_RejectsInvalidAmounts(decimal invalidAmount)
        {
            var db = new DBUser();
            bool result = db.TopUpBalance(1, invalidAmount);
            Assert.False(result);
        }

        [Fact]
        public void DBStation_GetById_ReturnsNullForNonExistentId()
        {
            var db = new DBStation();
            var station = db.GetById(99999);
            Assert.Null(station);
        }

        [Fact]
        public void DBTournament_LockAndUnlockStations_Succeeds()
        {
            var db = new DBTournament();
            int[] testStations = new int[] { 6, 7 };

            bool lockResult = db.LockTournamentStations(testStations);
            Assert.True(lockResult);

            bool unlockResult = db.UnlockTournamentStations(testStations);
            Assert.True(unlockResult);
        }

        [Fact]
        public void TariffCalculator_NightComboTariff_WithOvertimeAt630AM_ChargesCorrectOvertime()
        {
            TariffCalculatorBase calculator = new NightComboTariff();
            DateTime start = new DateTime(2026, 11, 10, 22, 0, 0);
            DateTime end = new DateTime(2026, 11, 11, 6, 30, 0); // 30 mins (0.5 hour) past 6 AM
            decimal basePrice = 10000;

            decimal cost = calculator.CalculateCost(start, end, basePrice);

            // 45,000 fixed + (0.5 hour * 10,000) = 50,000
            Assert.Equal(50000, cost);
        }

        [Fact]
        public void DBBilling_AddSession_SavesAndRetrievesEndTimeProperly()
        {
            var db = new DBBilling();
            DateTime start = DateTime.Now.AddHours(-2);
            DateTime end = DateTime.Now;

            var session = new GamingSession
            {
                StationId = 3,
                UserId = 2,
                StartTime = start,
                EndTime = end,
                TariffType = "Hourly",
                PlayCost = 16000,
                ServiceCost = 15000,
                TotalCost = 31000,
                Status = "Completed"
            };

            bool added = db.AddSession(session);
            Assert.True(added);

            var sessions = db.GetAllSessions();
            var addedSession = sessions.FirstOrDefault(s => s.StationId == 3 && s.PlayCost == 16000 && s.Status == "Completed");
            Assert.NotNull(addedSession);
            Assert.NotNull(addedSession.EndTime);
        }

        [Fact]
        public void DBBilling_ActiveSessionLifecycle_StartsAndCompletesSuccessfully()
        {
            var db = new DBBilling();
            int testStationId = 9;

            var session = new GamingSession
            {
                StationId = testStationId,
                UserId = 3,
                StartTime = DateTime.Now.AddMinutes(-30),
                TariffType = "Hourly",
                Status = "Active"
            };

            bool started = db.StartSession(session);
            Assert.True(started);

            var active = db.GetActiveSession(testStationId);
            Assert.NotNull(active);
            Assert.Equal("Active", active.Status);
            Assert.Equal(3, active.UserId);

            // Complete session
            DateTime end = DateTime.Now;
            bool completed = db.CompleteSession(active.SessionId, end, 12000, 10000, 22000);
            Assert.True(completed);

            var checkActive = db.GetActiveSession(testStationId);
            Assert.Null(checkActive);
        }

        [Fact]
        public void DBUser_DeductBalance_ReducesBalanceAccurately()
        {
            var db = new DBUser();
            var user = db.GetById(2); // gamer01
            Assert.NotNull(user);

            // Top up first so we know there's enough balance
            db.TopUpBalance(user.UserId, 50000);
            var reloaded = db.GetById(user.UserId);
            Assert.NotNull(reloaded);
            decimal initialBalance = reloaded.Balance;

            bool deducted = db.DeductBalance(user.UserId, 20000);
            Assert.True(deducted);

            var afterDeduct = db.GetById(user.UserId);
            Assert.NotNull(afterDeduct);
            Assert.Equal(initialBalance - 20000, afterDeduct.Balance);
        }

        [Fact]
        public void DBUser_TopUpBalance_UpgradesMemberTierToGold()
        {
            var db = new DBUser();
            var user = db.GetById(2);
            Assert.NotNull(user);

            // Top up enough to exceed 500k
            bool toppedUp = db.TopUpBalance(user.UserId, 600000);
            Assert.True(toppedUp);

            var upgradedUser = db.GetById(user.UserId);
            Assert.NotNull(upgradedUser);
            Assert.Equal("Gold", upgradedUser.MemberTier);
        }

        [Fact]
        public void DBTournament_LockTournamentStations_RejectsNullOrEmptyArray()
        {
            var db = new DBTournament();
            Assert.False(db.LockTournamentStations(null!));
            Assert.False(db.LockTournamentStations(new int[0]));
            Assert.False(db.UnlockTournamentStations(null!));
            Assert.False(db.UnlockTournamentStations(new int[0]));
        }

        [Fact]
        public void WindowsForms_AllFiveFormsInstantiateSuccessfullyOnSTAThread()
        {
            // Windows Forms controls require STA Thread
            Thread staThread = new Thread(() =>
            {
                using var form1 = new Form1();
                Assert.NotNull(form1);
                Assert.Contains("Cyber Matrix", form1.Text);

                using var formStation = new FormStation();
                Assert.NotNull(formStation);
                Assert.Contains("Máy Trạm", formStation.Text);

                using var formMember = new FormMember();
                Assert.NotNull(formMember);
                Assert.Contains("Hội Viên", formMember.Text);

                using var formTournament = new FormTournament();
                Assert.NotNull(formTournament);
                Assert.Contains("Giải Đấu", formTournament.Text);

                using var formBilling = new FormBilling();
                Assert.NotNull(formBilling);
                Assert.Contains("Dịch Vụ", formBilling.Text);
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }
    }
}
