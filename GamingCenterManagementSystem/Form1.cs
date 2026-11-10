namespace GamingCenterManagementSystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Text = "Quản lý trung tâm Gaming";
            var tournamentButton = new Button
            {
                Text = "Quản lý giải đấu",
                AutoSize = true,
                Location = new Point(30, 30),
                Padding = new Padding(12)
            };
            tournamentButton.Click += (_, _) =>
            {
                using var tournamentForm = new FormTournament();
                tournamentForm.ShowDialog(this);
            };
            Controls.Add(tournamentButton);
        }
    }
}
