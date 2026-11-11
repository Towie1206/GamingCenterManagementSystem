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

                using var mainForm = new Form1();
                Application.Run(mainForm);

                if (!mainForm.IsLogoutRequested)
                {
                    CurrentUserSession.Clear();
                    break;
                }

                CurrentUserSession.Clear();
            }
        }
    }
}
