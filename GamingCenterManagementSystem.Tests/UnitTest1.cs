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
    [Collection("GamingCenterSequential")]
    public class SystemTests
    {
        private static AppUser CreateTemporaryUser(DBUser db, decimal initialBalance = 50000m, string tier = "Standard")
        {
            string uniqueUsername = $"ut_{Guid.NewGuid():N}"[..16];
            var user = new AppUser
            {
                Username = uniqueUsername,
                FullName = "Temp Test User",
                Role = "Member",
                Balance = initialBalance,
                MemberTier = tier
            };

            bool ok = db.CreateMember(user, "testPass123", out string error);
            Assert.True(ok, error);
            Assert.True(user.UserId > 0);
            return user;
        }

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
            var tempUser = CreateTemporaryUser(db, initialBalance: 50000m);

            try
            {
                decimal initialBalance = tempUser.Balance;
                bool success = db.TopUpBalance(tempUser.UserId, 50000m);

                Assert.True(success);
                var updated = db.GetById(tempUser.UserId);
                Assert.NotNull(updated);
                Assert.Equal(initialBalance + 50000m, updated.Balance);
            }
            finally
            {
                db.Delete(tempUser.UserId);
            }
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
            var dbStation = new DBStation();
            int[] testStations = new int[] { 6, 7 };
            string? status6 = dbStation.GetById(6)?.Status;
            string? status7 = dbStation.GetById(7)?.Status;

            try
            {
                bool lockResult = db.LockTournamentStations(testStations);
                Assert.True(lockResult);
                Assert.Equal("TournamentLocked", dbStation.GetById(6)?.Status);
                Assert.Equal("TournamentLocked", dbStation.GetById(7)?.Status);

                bool unlockResult = db.UnlockTournamentStations(testStations);
                Assert.True(unlockResult);
                Assert.Equal("Available", dbStation.GetById(6)?.Status);
                Assert.Equal("Available", dbStation.GetById(7)?.Status);
            }
            finally
            {
                if (!string.IsNullOrEmpty(status6)) dbStation.UpdateStatus(6, status6);
                if (!string.IsNullOrEmpty(status7)) dbStation.UpdateStatus(7, status7);
            }
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

            try
            {
                bool added = db.AddSession(session);
                Assert.True(added);
                Assert.True(session.SessionId > 0);

                var sessions = db.GetAllSessions();
                var addedSession = sessions.FirstOrDefault(s => s.SessionId == session.SessionId);
                Assert.NotNull(addedSession);
                Assert.NotNull(addedSession.EndTime);
                Assert.Equal(31000m, addedSession.TotalCost);
            }
            finally
            {
                if (session.SessionId > 0)
                {
                    db.DeleteSession(session.SessionId);
                }
            }
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

            try
            {
                bool started = db.StartSession(session);
                Assert.True(started);
                Assert.True(session.SessionId > 0);

                var active = db.GetActiveSession(testStationId);
                Assert.NotNull(active);
                Assert.Equal(session.SessionId, active.SessionId);
                Assert.Equal("Active", active.Status);
                Assert.Equal(3, active.UserId);

                // Complete session
                DateTime end = DateTime.Now;
                bool completed = db.CompleteSession(active.SessionId, end, 12000, 10000, 22000);
                Assert.True(completed);

                var checkActive = db.GetActiveSession(testStationId);
                Assert.Null(checkActive);
            }
            finally
            {
                if (session.SessionId > 0)
                {
                    db.DeleteSession(session.SessionId);
                }
            }
        }

        [Fact]
        public void DBUser_DeductBalance_ReducesBalanceAccurately()
        {
            var db = new DBUser();
            var tempUser = CreateTemporaryUser(db, initialBalance: 50000m);

            try
            {
                db.TopUpBalance(tempUser.UserId, 50000m);
                var reloaded = db.GetById(tempUser.UserId);
                Assert.NotNull(reloaded);
                decimal initialBalance = reloaded.Balance;

                bool deducted = db.DeductBalance(tempUser.UserId, 20000m);
                Assert.True(deducted);

                var afterDeduct = db.GetById(tempUser.UserId);
                Assert.NotNull(afterDeduct);
                Assert.Equal(initialBalance - 20000m, afterDeduct.Balance);
            }
            finally
            {
                db.Delete(tempUser.UserId);
            }
        }

        [Fact]
        public void DBUser_TopUpBalance_UpgradesMemberTierToGold()
        {
            var db = new DBUser();
            var tempUser = CreateTemporaryUser(db, initialBalance: 10000m, tier: "Standard");

            try
            {
                // Top up enough to exceed 500k
                bool toppedUp = db.TopUpBalance(tempUser.UserId, 600000m);
                Assert.True(toppedUp);

                var upgradedUser = db.GetById(tempUser.UserId);
                Assert.NotNull(upgradedUser);
                Assert.Equal("Gold", upgradedUser.MemberTier);
            }
            finally
            {
                db.Delete(tempUser.UserId);
            }
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
            Exception? threadException = null;

            Thread staThread = new(() =>
            {
                try
                {
                    CurrentUserSession.Clear();

                    using var formLogin = new FormLogin();
                    Assert.NotNull(formLogin);
                    Assert.Contains("Cyber Matrix", formLogin.Text);

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
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    CurrentUserSession.Clear();
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Assert.Null(threadException);
        }
    }
}
