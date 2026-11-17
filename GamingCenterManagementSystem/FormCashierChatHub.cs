using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GamingCenterManagementSystem.DAL;
using GamingCenterManagementSystem.Models;

namespace GamingCenterManagementSystem
{
    public partial class FormCashierChatHub : Form
    {
        private readonly DBStation _dbStation;
        private readonly Action<ChatMessage> _onMessageSentHandler;
        private readonly Action<int> _onUnreadChangedHandler;
        private int _currentStationId = 1;

        public class StationComboItem
        {
            public int StationId { get; set; }
            public string StationName { get; set; } = string.Empty;
            public int UnreadCount { get; set; }

            public override string ToString()
            {
                return UnreadCount > 0 ? $"{StationName} (🔔 {UnreadCount} mới)" : StationName;
            }
        }

        public int SelectedStationId => _currentStationId;
        public int RenderedMessageBubbleCount => flpChatHistory.Controls.Count;

        public FormCashierChatHub() : this(new DBStation(), null)
        {
        }

        public FormCashierChatHub(DBStation dbStation, int? initialStationId = null)
        {
            _dbStation = dbStation ?? new DBStation();

            InitializeComponent();

            _onMessageSentHandler = HandleMessageSent;
            _onUnreadChangedHandler = HandleUnreadChanged;

            StationChatService.OnMessageSent += _onMessageSentHandler;
            StationChatService.OnUnreadCountChanged += _onUnreadChangedHandler;

            FormClosed += (s, e) =>
            {
                StationChatService.OnMessageSent -= _onMessageSentHandler;
                StationChatService.OnUnreadCountChanged -= _onUnreadChangedHandler;
            };

            SetupEventHandlers();
            LoadStations(initialStationId);
            UpdateUnreadHeader();
        }

        private void SetupEventHandlers()
        {
            cboStation.SelectedIndexChanged += (s, e) =>
            {
                if (cboStation.SelectedItem is StationComboItem item)
                {
                    _currentStationId = item.StationId;
                    LoadChatHistory();
                    StationChatService.MarkAsReadByStaff(_currentStationId);
                    UpdateUnreadHeader();
                    RefreshStationDropdownUnreadCounts();
                    UpdateStationDetails(item);
                }
            };

            flpChatHistory.SizeChanged += (s, e) =>
            {
                int targetWidth = Math.Max(280, flpChatHistory.ClientSize.Width - 30);
                int contentMax = Math.Max(240, flpChatHistory.ClientSize.Width - 50);
                flpChatHistory.SuspendLayout();
                foreach (Control c in flpChatHistory.Controls)
                {
                    if (c is Panel p)
                    {
                        p.Width = targetWidth;
                        foreach (Control child in p.Controls)
                        {
                            if (child is Label lbl && lbl.Font.Size >= 10F)
                            {
                                lbl.MaximumSize = new Size(contentMax, 0);
                            }
                        }
                    }
                }
                flpChatHistory.ResumeLayout(true);
            };

            btnRefresh.Click += (s, e) =>
            {
                LoadStations(_currentStationId);
                LoadChatHistory();
            };

            btnSend.Click += (s, e) => SendTypedMessage();

            txtMessage.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    SendTypedMessage();
                }
            };

            // Cashier quick reply templates
            btnQuickBring.Click += (s, e) => SendStaffReply("Nhân viên đang mang qua ngay", "Bring");
            btnQuickDeposit.Click += (s, e) => SendStaffReply("Đã nạp tiền thành công nhé", "Deposit");
            btnQuickTech.Click += (s, e) => SendStaffReply("Kỹ thuật đang đến hỗ trợ", "Tech");
            btnQuickKitchen.Click += (s, e) => SendStaffReply("Bếp đang làm đồ, đợi 2 phút nhé", "Kitchen");

            btnQuickQr.Click += (s, e) => ShowVietQrGeneratorDialog();
        }

        public void LoadStations(int? preferredStationId = null)
        {
            cboStation.Items.Clear();

            List<ComputerStation> stations;
            try
            {
                stations = _dbStation.GetAll();
            }
            catch
            {
                stations = new List<ComputerStation>();
            }

            if (stations.Count == 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    stations.Add(new ComputerStation
                    {
                        StationId = i,
                        StationName = $"PC-{i:D2}",
                        IpAddress = $"192.168.1.{100 + i}",
                        Status = "Available"
                    });
                }
            }

            StationComboItem? selectedItem = null;
            foreach (var st in stations)
            {
                int unread = StationChatService.GetUnreadCountForStaff(st.StationId);
                var item = new StationComboItem
                {
                    StationId = st.StationId,
                    StationName = st.StationName,
                    UnreadCount = unread
                };
                cboStation.Items.Add(item);

                if (preferredStationId.HasValue && st.StationId == preferredStationId.Value)
                {
                    selectedItem = item;
                }
                else if (selectedItem == null && unread > 0)
                {
                    selectedItem = item;
                }
            }

            if (selectedItem != null)
            {
                cboStation.SelectedItem = selectedItem;
            }
            else if (cboStation.Items.Count > 0)
            {
                cboStation.SelectedIndex = 0;
            }
        }

        public void SelectStation(int stationId)
        {
            for (int i = 0; i < cboStation.Items.Count; i++)
            {
                if (cboStation.Items[i] is StationComboItem item && item.StationId == stationId)
                {
                    cboStation.SelectedIndex = i;
                    return;
                }
            }
        }

        private void UpdateStationDetails(StationComboItem item)
        {
            lblSelectedStationDetails.Text = $"🖥️ Đang trao đổi với máy: {item.StationName} (Mã #{item.StationId}) | Thu ngân: {CurrentUserSession.Username}";
        }

        public void LoadChatHistory()
        {
            flpChatHistory.SuspendLayout();
            flpChatHistory.Controls.Clear();

            var messages = StationChatService.GetMessages(_currentStationId);
            foreach (var msg in messages)
            {
                var bubble = CreateMessageBubble(msg);
                flpChatHistory.Controls.Add(bubble);
            }

            flpChatHistory.ResumeLayout(true);

            if (flpChatHistory.Controls.Count > 0)
            {
                flpChatHistory.ScrollControlIntoView(flpChatHistory.Controls[^1]);
            }
        }

        private Control CreateMessageBubble(ChatMessage msg)
        {
            int bubbleWidth = Math.Max(280, flpChatHistory.ClientSize.Width - 30);
            int contentMax = Math.Max(240, flpChatHistory.ClientSize.Width - 50);

            var pnlBubble = new Panel
            {
                Width = bubbleWidth,
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 8),
                Padding = new Padding(10, 8, 10, 8)
            };

            bool isStaff = msg.IsSentByStaff;

            pnlBubble.BackColor = isStaff
                ? Color.FromArgb(20, 45, 75)
                : Color.FromArgb(18, 54, 45);

            var lblHeader = new Label
            {
                Text = msg.GetDisplayHeader(),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = isStaff ? Color.FromArgb(0, 210, 255) : Color.FromArgb(0, 230, 160),
                Dock = DockStyle.Top,
                AutoSize = true
            };

            var lblContent = new Label
            {
                Text = msg.MessageText,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(contentMax, 0),
                Margin = new Padding(0, 4, 0, 0)
            };

            pnlBubble.Controls.Add(lblContent);
            pnlBubble.Controls.Add(lblHeader);

            return pnlBubble;
        }

        public void SendTypedMessage()
        {
            string text = txtMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            SendStaffReply(text, null);
            txtMessage.Clear();
            txtMessage.Focus();
        }

        public ChatMessage SendStaffReply(string messageText, string? quickTag = null)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                throw new ArgumentException("Nội dung tin nhắn không được để trống.", nameof(messageText));

            string staffName = string.IsNullOrWhiteSpace(CurrentUserSession.FullName)
                ? (string.IsNullOrWhiteSpace(CurrentUserSession.Username) ? "Thu Ngân" : CurrentUserSession.Username)
                : CurrentUserSession.FullName;

            string stationName = cboStation.SelectedItem is StationComboItem item
                ? item.StationName
                : $"PC-{_currentStationId:D2}";

            var sent = StationChatService.SendMessage(
                stationId: _currentStationId,
                stationName: stationName,
                senderRole: "Staff",
                senderUsername: CurrentUserSession.Username,
                senderDisplayName: staffName,
                messageText: messageText,
                quickTag: quickTag);

            return sent;
        }

        private void HandleMessageSent(ChatMessage msg)
        {
            if (IsDisposed || !IsHandleCreated) return;

            Action renderAction = () =>
            {
                if (msg.StationId == _currentStationId)
                {
                    var bubble = CreateMessageBubble(msg);
                    flpChatHistory.Controls.Add(bubble);
                    flpChatHistory.ScrollControlIntoView(bubble);
                    if (msg.IsSentByMember)
                    {
                        StationChatService.MarkAsReadByStaff(_currentStationId);
                    }
                }
                UpdateUnreadHeader();
                RefreshStationDropdownUnreadCounts();
            };

            if (InvokeRequired)
            {
                try { BeginInvoke(renderAction); } catch { }
            }
            else
            {
                renderAction();
            }
        }

        private void HandleUnreadChanged(int stationId)
        {
            if (IsDisposed || !IsHandleCreated) return;

            Action updateAction = () =>
            {
                UpdateUnreadHeader();
                RefreshStationDropdownUnreadCounts();
            };

            if (InvokeRequired)
            {
                try { BeginInvoke(updateAction); } catch { }
            }
            else
            {
                updateAction();
            }
        }

        public void RefreshStationDropdownUnreadCounts()
        {
            if (IsDisposed || !IsHandleCreated) return;

            for (int i = 0; i < cboStation.Items.Count; i++)
            {
                if (cboStation.Items[i] is StationComboItem item)
                {
                    int unread = StationChatService.GetUnreadCountForStaff(item.StationId);
                    if (item.UnreadCount != unread)
                    {
                        item.UnreadCount = unread;
                        cboStation.Items[i] = item;
                    }
                }
            }
        }

        public string ResolveMemberUsernameForStation(int stationId)
        {
            var recentMemberMsg = StationChatService.GetMessages(stationId)
                .LastOrDefault(m => m.IsSentByMember && !string.IsNullOrWhiteSpace(m.SenderUsername));
            if (recentMemberMsg != null)
            {
                return recentMemberMsg.SenderUsername;
            }

            try
            {
                var activeSession = new DBBilling().GetActiveSession(stationId);
                if (activeSession?.UserId.HasValue == true)
                {
                    var user = new DBUser().GetById(activeSession.UserId.Value);
                    if (user != null && !string.IsNullOrWhiteSpace(user.Username))
                    {
                        return user.Username;
                    }
                }
            }
            catch { }

            return "GUEST";
        }

        public void UpdateUnreadHeader()
        {
            int total = StationChatService.GetUnreadCountForStaff();
            lblUnreadTotal.Text = total > 0 ? $"🔔 {total} tin chưa đọc" : "✅ Không có tin mới";
            lblUnreadTotal.BackColor = total > 0 ? Color.FromArgb(220, 53, 69) : Color.FromArgb(39, 174, 96);
        }

        public void ShowVietQrGeneratorDialog()
        {
            string stationName = cboStation.SelectedItem is StationComboItem item
                ? item.StationName
                : $"PC-{_currentStationId:D2}";
            string resolvedUser = ResolveMemberUsernameForStation(_currentStationId);

            using var dlg = new Form();
            dlg.Text = $"⚡ Sinh mã VietQR nạp ví cho máy {stationName}";
            dlg.Size = new Size(440, 530);
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.BackColor = Color.FromArgb(20, 26, 38);
            dlg.ForeColor = Color.White;

            var lblPrompt = new Label
            {
                Text = $"Nhập số tiền nạp ví (VNĐ):",
                Location = new Point(20, 15),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var txtAmount = new TextBox
            {
                Text = "50000",
                Location = new Point(20, 38),
                Size = new Size(180, 25),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White
            };

            var lblUserPrompt = new Label
            {
                Text = "Tên tài khoản:",
                Location = new Point(220, 15),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var txtUsername = new TextBox
            {
                Text = resolvedUser,
                Location = new Point(220, 38),
                Size = new Size(180, 25),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White
            };

            var picQr = new PictureBox
            {
                Location = new Point(90, 80),
                Size = new Size(240, 240),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var lblSyntax = new Label
            {
                Location = new Point(20, 335),
                Size = new Size(380, 45),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255),
                TextAlign = ContentAlignment.MiddleCenter
            };

            void RefreshQr()
            {
                if (decimal.TryParse(txtAmount.Text, out decimal amt) && amt > 0)
                {
                    string user = string.IsNullOrWhiteSpace(txtUsername.Text) ? "GUEST" : txtUsername.Text.Trim();
                    string syntax = VietQrHelper.BuildTransferSyntax(stationName, user);
                    lblSyntax.Text = $"Cú pháp CK: {syntax}\nSố tiền: {amt:N0} VNĐ";
                    string payload = VietQrHelper.GenerateWalletTopUpPayload(stationName, user, amt);
                    var oldImg = picQr.Image;
                    picQr.Image = VietQrHelper.GenerateQrBitmap(payload, 6);
                    oldImg?.Dispose();
                }
            }

            txtAmount.TextChanged += (s, e) => RefreshQr();
            txtUsername.TextChanged += (s, e) => RefreshQr();
            RefreshQr();

            var btnClose = new Button
            {
                Text = "Đóng",
                Location = new Point(160, 430),
                Size = new Size(100, 34),
                BackColor = Color.FromArgb(50, 65, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            dlg.Controls.Add(lblPrompt);
            dlg.Controls.Add(txtAmount);
            dlg.Controls.Add(lblUserPrompt);
            dlg.Controls.Add(txtUsername);
            dlg.Controls.Add(picQr);
            dlg.Controls.Add(lblSyntax);
            dlg.Controls.Add(btnClose);

            dlg.ShowDialog(this);
        }
    }
}
