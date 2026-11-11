
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace GamingCenterManagementSystem
{
    public partial class Form1 : Form
    {
        // CYBER GAMING COLOR PALETTE
        private readonly Color bg = Color.FromArgb(13, 15, 25);
        private readonly Color sidebar = Color.FromArgb(18, 21, 34);
        private readonly Color card = Color.FromArgb(25, 29, 45);
        private readonly Color accent = Color.FromArgb(0, 220, 200);
        private readonly Color purple = Color.FromArgb(155, 105, 255);
        private readonly Color white = Color.FromArgb(240, 243, 250);
        private readonly Color muted = Color.FromArgb(145, 155, 175);

        private Panel sidebarPanel;
        private Panel contentPanel;
        private Panel headerPanel;
        private DataGridView dgvMembers;

        private TextBox txtSearch;
        private TextBox txtUsername;
        private TextBox txtFullName;
        private TextBox txtPassword;
        private TextBox txtBalance;

        private TextBox txtChangeUsername;
        private TextBox txtOldPassword;
        private TextBox txtNewPassword;

        private Label lblTotalMembers;
        private Label lblTotalBalance;
        private Label lblStatus;

        private Panel dashboardPage;
        private Panel memberPage;
        private Panel createPage;
        private Panel passwordPage;

        private string currentPage = "Dashboard";

        public Form1()
        {
            InitializeComponent();

            Controls.Clear();

            BuildInterface();
            LoadMembers();
        }

        // ==========================================
        // MAIN INTERFACE
        // ==========================================

        private void BuildInterface()
        {
            Text = "NEXUS GAMING | MANAGEMENT SYSTEM";
            Size = new Size(1250, 800);
            MinimumSize = new Size(1050, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bg;
            ForeColor = white;
            Font = new Font("Segoe UI", 10);

            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 225,
                BackColor = sidebar
            };

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg
            };

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = bg
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg,
                Padding = new Padding(25)
            };

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(headerPanel);

            Controls.Add(mainPanel);
            Controls.Add(sidebarPanel);

            BuildSidebar();
            BuildHeader();
            BuildPages();

            ShowPage("Dashboard");
        }

        // ==========================================
        // SIDEBAR
        // ==========================================

        private void BuildSidebar()
        {
            Label logo = new Label
            {
                Text = "NEXUS",
                Font = new Font("Segoe UI", 23, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(25, 25),
                Size = new Size(180, 45)
            };

            Label subtitle = new Label
            {
                Text = "GAMING CENTER",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = muted,
                Location = new Point(28, 70),
                Size = new Size(180, 25)
            };

            Label menuLabel = new Label
            {
                Text = "MAIN MENU",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = muted,
                Location = new Point(25, 125),
                Size = new Size(180, 25)
            };

            sidebarPanel.Controls.Add(logo);
            sidebarPanel.Controls.Add(subtitle);
            sidebarPanel.Controls.Add(menuLabel);

            AddNavButton("Dashboard", "Dashboard", 165);
            AddNavButton("Members", "Members", 220);
            AddNavButton("Create Member", "Create", 275);
            AddNavButton("Change Password", "Password", 330);

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = sidebar
            };

            Label version = new Label
            {
                Text = "NEXUS MANAGEMENT SYSTEM\nVersion 1.0.0",
                ForeColor = muted,
                Font = new Font("Segoe UI", 8),
                Location = new Point(23, 20),
                Size = new Size(190, 45)
            };

            bottomPanel.Controls.Add(version);
            sidebarPanel.Controls.Add(bottomPanel);
        }

        private void AddNavButton(string text, string page, int y)
        {
            Button btn = new Button
            {
                Text = "    " + text,
                Location = new Point(12, y),
                Size = new Size(200, 43),
                FlatStyle = FlatStyle.Flat,
                BackColor = sidebar,
                ForeColor = muted,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(36, 43, 63);

            btn.Click += (s, e) => ShowPage(page);

            sidebarPanel.Controls.Add(btn);
        }

        // ==========================================
        // HEADER
        // ==========================================

        private void BuildHeader()
        {
            Label title = new Label
            {
                Name = "lblPageTitle",
                Text = "Dashboard Overview",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = white,
                Location = new Point(25, 22),
                Size = new Size(450, 35)
            };

            Label status = new Label
            {
                Text = "● SYSTEM ONLINE",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(850, 30)
            };

            Panel line = new Panel
            {
                Height = 1,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(42, 48, 65)
            };

            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(status);
            headerPanel.Controls.Add(line);
        }

        // ==========================================
        // PAGE CREATION
        // ==========================================

        private void BuildPages()
        {
            dashboardPage = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg
            };

            memberPage = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg
            };

            createPage = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg
            };

            passwordPage = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg
            };

            contentPanel.Controls.Add(dashboardPage);
            contentPanel.Controls.Add(memberPage);
            contentPanel.Controls.Add(createPage);
            contentPanel.Controls.Add(passwordPage);

            BuildDashboard();
            BuildMemberPage();
            BuildCreatePage();
            BuildPasswordPage();
        }

        private void ShowPage(string page)
        {
            currentPage = page;

            dashboardPage.Visible = page == "Dashboard";
            memberPage.Visible = page == "Members";
            createPage.Visible = page == "Create";
            passwordPage.Visible = page == "Password";

            foreach (Control c in headerPanel.Controls)
            {
                if (c is Label label && label.Name == "lblPageTitle")
                {
                    switch (page)
                    {
                        case "Dashboard":
                            label.Text = "Dashboard Overview";
                            break;
                        case "Members":
                            label.Text = "Member Management";
                            break;
                        case "Create":
                            label.Text = "Create New Member";
                            break;
                        case "Password":
                            label.Text = "Security Settings";
                            break;
                    }
                }
            }

            if (page == "Dashboard" || page == "Members")
                LoadMembers();
        }

        // ==========================================
        // REUSABLE UI COMPONENTS
        // ==========================================

        private Label MakeLabel(
            string text, int x, int y, int width,
            int height, float size = 10,
            Color? color = null, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                ForeColor = color ?? white,
                Font = new Font(
                    "Segoe UI", size,
                    bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private Panel MakeCard(
            Control parent, int x, int y,
            int width, int height)
        {
            Panel p = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = card
            };

            parent.Controls.Add(p);
            return p;
        }

        private TextBox MakeTextBox(
            Control parent, string placeholder,
            int x, int y, bool password = false)
        {
            Panel wrapper = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(390, 45),
                BackColor = Color.FromArgb(35, 41, 60)
            };

            TextBox tb = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(35, 41, 60),
                ForeColor = white,
                Font = new Font("Segoe UI", 10),
                Location = new Point(12, 13),
                Size = new Size(360, 23),
                PlaceholderText = placeholder,
                UseSystemPasswordChar = password
            };

            wrapper.Controls.Add(tb);
            parent.Controls.Add(wrapper);

            return tb;
        }

        private Button MakeButton(
            Control parent, string text,
            int x, int y, int width,
            EventHandler click, bool primary = true)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 43),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? accent : card,
                ForeColor = primary ? bg : white,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;

            btn.Click += click;

            parent.Controls.Add(btn);
            return btn;
        }

        // ==========================================
        // DASHBOARD
        // ==========================================

        private void BuildDashboard()
        {
            Label welcome = MakeLabel(
                "Welcome back, Administrator",
                5, 5, 600, 35, 17, white, true);

            Label description = MakeLabel(
                "Manage your gaming center members and accounts.",
                5, 42, 650, 25, 9, muted);

            dashboardPage.Controls.Add(welcome);
            dashboardPage.Controls.Add(description);

            Panel c1 = MakeCard(dashboardPage, 5, 95, 280, 130);
            Panel c2 = MakeCard(dashboardPage, 305, 95, 280, 130);
            Panel c3 = MakeCard(dashboardPage, 605, 95, 280, 130);

            c1.Controls.Add(MakeLabel(
                "TOTAL MEMBERS", 20, 20, 240, 25, 9, muted, true));

            lblTotalMembers = MakeLabel(
                "0", 20, 52, 240, 50, 26, accent, true);

            c1.Controls.Add(lblTotalMembers);

            c2.Controls.Add(MakeLabel(
                "TOTAL BALANCE", 20, 20, 240, 25, 9, muted, true));

            lblTotalBalance = MakeLabel(
                "0 VND", 20, 52, 240, 50, 22, purple, true);

            c2.Controls.Add(lblTotalBalance);

            c3.Controls.Add(MakeLabel(
                "SYSTEM STATUS", 20, 20, 240, 25, 9, muted, true));

            lblStatus = MakeLabel(
                "ONLINE", 20, 52, 240, 50, 24, accent, true);

            c3.Controls.Add(lblStatus);

            Panel bottom = MakeCard(
                dashboardPage, 5, 250, 880, 240);

            bottom.Controls.Add(MakeLabel(
                "QUICK ACTIONS", 20, 18, 300, 30, 12, white, true));

            MakeButton(bottom, "Manage Members",
                20, 80, 190,
                (s, e) => ShowPage("Members"));

            MakeButton(bottom, "Create Member",
                230, 80, 190,
                (s, e) => ShowPage("Create"), false);

            MakeButton(bottom, "Change Password",
                440, 80, 190,
                (s, e) => ShowPage("Password"), false);

            bottom.Controls.Add(MakeLabel(
                "NEXUS GAMING MANAGEMENT SYSTEM",
                20, 175, 600, 25, 9, muted));
        }

        // ==========================================
        // MEMBER MANAGEMENT
        // ==========================================

        private void BuildMemberPage()
        {
            Label title = MakeLabel(
                "Member Directory", 5, 5, 400, 35,
                16, white, true);

            Label desc = MakeLabel(
                "Search and manage registered members.",
                5, 40, 500, 25, 9, muted);

            memberPage.Controls.Add(title);
            memberPage.Controls.Add(desc);

            txtSearch = MakeTextBox(
                memberPage, "Search username or full name...",
                5, 85);

            

            txtSearch.TextChanged += TxtSearch_TextChanged;

            MakeButton(memberPage, "Refresh",
                455, 85, 120,
                (s, e) => LoadMembers(), false);

            dgvMembers = new DataGridView
            {
                Location = new Point(5, 155),
                Size = new Size(880, 440),
                BackgroundColor = card,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(44, 51, 70),
                ForeColor = Color.FromArgb(30, 35, 48),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            dgvMembers.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(34, 40, 59);

            dgvMembers.ColumnHeadersDefaultCellStyle.ForeColor = accent;

            dgvMembers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);

            dgvMembers.ColumnHeadersHeight = 45;

            dgvMembers.DefaultCellStyle.BackColor = card;
            dgvMembers.DefaultCellStyle.ForeColor = white;
            dgvMembers.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(49, 62, 83);

            dgvMembers.DefaultCellStyle.SelectionForeColor = accent;

            dgvMembers.DefaultCellStyle.Font =
                new Font("Segoe UI", 9);

            dgvMembers.RowTemplate.Height = 38;

            dgvMembers.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(29, 34, 51);

            dgvMembers.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 &&
                    dgvMembers.Rows[e.RowIndex].DataBoundItem
                    is Member member)
                {
                    txtChangeUsername.Text = member.Username;
                }
            };

            memberPage.Controls.Add(dgvMembers);
        }

        // ==========================================
        // CREATE MEMBER
        // ==========================================

        private void BuildCreatePage()
        {
            Label title = MakeLabel(
                "Register New Member", 5, 5, 500, 35,
                17, white, true);

            Label desc = MakeLabel(
                "Create a new gaming center member account.",
                5, 42, 600, 25, 9, muted);

            createPage.Controls.Add(title);
            createPage.Controls.Add(desc);

            Panel formCard = MakeCard(
                createPage, 5, 90, 700, 460);

            formCard.Controls.Add(MakeLabel(
                "ACCOUNT INFORMATION",
                30, 25, 400, 30, 11, accent, true));

            formCard.Controls.Add(MakeLabel(
                "USERNAME", 30, 80, 300, 25, 9, muted, true));

            txtUsername = MakeTextBox(
                formCard, "Enter username", 30, 110);

            formCard.Controls.Add(MakeLabel(
                "FULL NAME", 30, 175, 300, 25, 9, muted, true));

            txtFullName = MakeTextBox(
                formCard, "Enter full name", 30, 205);

            formCard.Controls.Add(MakeLabel(
                "PASSWORD", 30, 270, 300, 25, 9, muted, true));

            txtPassword = MakeTextBox(
                formCard, "Enter password", 30, 300, true);

            formCard.Controls.Add(MakeLabel(
                "INITIAL BALANCE (VND)",
                30, 365, 300, 25, 9, muted, true));

            txtBalance = MakeTextBox(
                formCard, "Minimum 10,000 VND", 30, 395);

            txtBalance.Text = "10000";

            MakeButton(formCard, "CREATE ACCOUNT",
                450, 395, 200,
                BtnCreateMember_Click);
        }

        // ==========================================
        // CHANGE PASSWORD
        // ==========================================

        private void BuildPasswordPage()
        {
            Label title = MakeLabel(
                "Account Security", 5, 5, 500, 35,
                17, white, true);

            Label desc = MakeLabel(
                "Update a member's login credentials.",
                5, 42, 600, 25, 9, muted);

            passwordPage.Controls.Add(title);
            passwordPage.Controls.Add(desc);

            Panel formCard = MakeCard(
                passwordPage, 5, 90, 700, 420);

            formCard.Controls.Add(MakeLabel(
                "CHANGE PASSWORD",
                30, 25, 400, 30, 11, accent, true));

            formCard.Controls.Add(MakeLabel(
                "USERNAME", 30, 80, 300, 25, 9, muted, true));

            txtChangeUsername = MakeTextBox(
                formCard, "Enter member username",
                30, 110);

            formCard.Controls.Add(MakeLabel(
                "CURRENT PASSWORD",
                30, 175, 300, 25, 9, muted, true));

            txtOldPassword = MakeTextBox(
                formCard, "Enter current password",
                30, 205, true);

            formCard.Controls.Add(MakeLabel(
                "NEW PASSWORD",
                30, 270, 300, 25, 9, muted, true));

            txtNewPassword = MakeTextBox(
                formCard, "Enter new password",
                30, 300, true);

            MakeButton(formCard, "UPDATE PASSWORD",
                450, 300, 200,
                BtnChangePassword_Click);
        }

        // ==========================================
        // LOAD MEMBERS
        // ==========================================

        private void LoadMembers()
        {
            try
            {
                var members = DBUser.GetAllMembers();

                if (dgvMembers != null)
                {
                    dgvMembers.DataSource = null;
                    dgvMembers.DataSource = members;

                    if (dgvMembers.Columns["UserId"] != null)
                        dgvMembers.Columns["UserId"].HeaderText = "ID";

                    if (dgvMembers.Columns["Username"] != null)
                        dgvMembers.Columns["Username"].HeaderText = "USERNAME";

                    if (dgvMembers.Columns["FullName"] != null)
                        dgvMembers.Columns["FullName"].HeaderText = "FULL NAME";

                    if (dgvMembers.Columns["Role"] != null)
                        dgvMembers.Columns["Role"].HeaderText = "ROLE";

                    if (dgvMembers.Columns["Balance"] != null)
                    {
                        dgvMembers.Columns["Balance"].HeaderText = "BALANCE";
                        dgvMembers.Columns["Balance"].DefaultCellStyle.Format = "N0";
                    }

                    if (dgvMembers.Columns["MemberTier"] != null)
                        dgvMembers.Columns["MemberTier"].HeaderText = "TIER";

                    if (dgvMembers.Columns["CreatedAt"] != null)
                    {
                        dgvMembers.Columns["CreatedAt"].HeaderText = "CREATED AT";
                        dgvMembers.Columns["CreatedAt"].DefaultCellStyle.Format =
                            "dd/MM/yyyy";
                    }
                }

                if (lblTotalMembers != null)
                    lblTotalMembers.Text = members.Count.ToString();

                if (lblTotalBalance != null)
                {
                    decimal total = members.Sum(m => m.Balance);

                    lblTotalBalance.Text =
                        total.ToString("N0") + " VND";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load members:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // LOGIC 2: INSTANT LINQ SEARCH
        // ==========================================

        private void TxtSearch_TextChanged(
            object sender, EventArgs e)
        {
            try
            {
                string keyword = txtSearch.Text.Trim();

                var members = DBUser.GetAllMembers();

                var result = members.Where(u =>
                    u.Username.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    u.FullName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)
                ).ToList();

                dgvMembers.DataSource = null;
                dgvMembers.DataSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Search error: " + ex.Message);
            }
        }

        // ==========================================
        // LOGIC 3: CREATE MEMBER
        // ==========================================

        private void BtnCreateMember_Click(
            object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string password = txtPassword.Text;

            decimal balance;

            if (!decimal.TryParse(
                txtBalance.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out balance)
                &&
                !decimal.TryParse(
                    txtBalance.Text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out balance))
            {
                MessageBox.Show(
                    "Invalid balance.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string error;

                bool success = DBUser.CreateMember(
                    username, fullName, password,
                    balance, out error);

                if (success)
                {
                    MessageBox.Show(
                        "Member created successfully!",
                        "SUCCESS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    txtUsername.Clear();
                    txtFullName.Clear();
                    txtPassword.Clear();
                    txtBalance.Text = "10000";

                    LoadMembers();
                }
                else
                {
                    MessageBox.Show(
                        error,
                        "Cannot Create Member",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // LOGIC 1: CHANGE PASSWORD
        // ==========================================

        private void BtnChangePassword_Click(
            object sender, EventArgs e)
        {
            string username = txtChangeUsername.Text.Trim();
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(oldPassword) ||
                string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show(
                    "Please complete all fields.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = DBUser.ChangePassword(
                    username, oldPassword, newPassword);

                if (success)
                {
                    MessageBox.Show(
                        "Password updated successfully!",
                        "SUCCESS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    txtOldPassword.Clear();
                    txtNewPassword.Clear();
                }
                else
                {
                    MessageBox.Show(
                        "Incorrect username or current password.",
                        "Update Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
