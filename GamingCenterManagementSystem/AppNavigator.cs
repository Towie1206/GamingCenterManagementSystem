using System;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;

namespace GamingCenterManagementSystem
{
    public static class AppNavigator
    {
        public static Form CreateMainFormForCurrentRole(
            DBStation? dbStation = null,
            DBBilling? dbBilling = null,
            DBUser? dbUser = null,
            int? stationId = null)
        {
            return CreateFormForRole(
                CurrentUserSession.Role,
                dbStation,
                dbBilling,
                dbUser,
                stationId);
        }

        public static Form CreateFormForRole(
            string? role,
            DBStation? dbStation = null,
            DBBilling? dbBilling = null,
            DBUser? dbUser = null,
            int? stationId = null)
        {
            bool isMember = string.Equals(role, "Member", StringComparison.OrdinalIgnoreCase);

            if (isMember)
            {
                return (dbStation != null && dbBilling != null && dbUser != null)
                    ? new FormClientDashboard(dbStation, dbBilling, dbUser, stationId)
                    : new FormClientDashboard();
            }

            return (dbStation != null && dbBilling != null && dbUser != null)
                ? new Form1(dbStation, dbUser, dbBilling)
                : new Form1();
        }
    }
}
