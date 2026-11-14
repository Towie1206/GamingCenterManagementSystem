namespace GamingCenterManagementSystem
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            while (true)
            {
                using var loginForm = new FormLogin();
                if (loginForm.ShowDialog() != DialogResult.OK || !CurrentUserSession.IsLoggedIn)
                {
                    CurrentUserSession.Clear();
                    break;
                }

                bool isLogoutRequested = false;
                if (CurrentUserSession.IsMember)
                {
                    using var clientForm = new FormClientDashboard();
                    Application.Run(clientForm);
                    isLogoutRequested = clientForm.IsLogoutRequested;
                }
                else
                {
                    using var adminForm = new Form1();
                    Application.Run(adminForm);
                    isLogoutRequested = adminForm.IsLogoutRequested;
                }

                if (!isLogoutRequested)
                {
                    CurrentUserSession.Clear();
                    break;
                }

                CurrentUserSession.Clear();
            }
        }
    }
}
