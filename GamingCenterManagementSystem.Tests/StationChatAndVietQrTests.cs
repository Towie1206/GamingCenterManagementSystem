using System;
using System.Drawing;
using System.Linq;
using GamingCenterManagementSystem.Models;
using Xunit;

namespace GamingCenterManagementSystem.Tests
{
    public class StationChatAndVietQrTests
    {
        public StationChatAndVietQrTests()
        {
            StationChatService.Clear();
        }

        // =========================================================================
        // 1. CHAT MESSAGE TESTS
        // =========================================================================

        [Fact]
        public void ChatMessage_DefaultConstructor_IncrementsMessageIdAndSetsDefaults()
        {
            StationChatService.Clear();
            var msg1 = new ChatMessage();
            var msg2 = new ChatMessage();

            Assert.True(msg2.MessageId > msg1.MessageId);
            Assert.Equal("Member", msg1.SenderRole);
            Assert.False(msg1.IsReadByStaff);
            Assert.False(msg1.IsReadByMember);
        }

        [Fact]
        public void ChatMessage_ParameterizedConstructor_ValidatesStationIdAndMessageText()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ChatMessage(0, "PC-01", "Member", "gamer01", "Gamer 1", "Xin chào"));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ChatMessage(-5, "PC-01", "Member", "gamer01", "Gamer 1", "Xin chào"));

            Assert.Throws<ArgumentException>(() =>
                new ChatMessage(1, "PC-01", "Member", "gamer01", "Gamer 1", ""));

            Assert.Throws<ArgumentException>(() =>
                new ChatMessage(1, "PC-01", "Member", "gamer01", "Gamer 1", "   \t\n  "));
        }

        [Fact]
        public void ChatMessage_MemberRole_SetsProperReadFlagsAndHeader()
        {
            var msg = new ChatMessage(2, "PC-02", "Member", "gamer02", "Gamer Hai", "Nạp 50k ví", "Deposit");

            Assert.True(msg.IsSentByMember);
            Assert.False(msg.IsSentByStaff);
            Assert.True(msg.IsReadByMember);
            Assert.False(msg.IsReadByStaff);
            Assert.Equal("Deposit", msg.QuickTag);
            Assert.Contains("PC-02", msg.GetDisplayHeader());
            Assert.Contains("Gamer Hai", msg.GetDisplayHeader());
        }

        [Fact]
        public void ChatMessage_StaffRole_SetsProperReadFlagsAndHeader()
        {
            var msg = new ChatMessage(2, "PC-02", "Staff", "staff01", "Thu Ngân Ly", "Đã nạp tiền thành công nhé");

            Assert.True(msg.IsSentByStaff);
            Assert.False(msg.IsSentByMember);
            Assert.True(msg.IsReadByStaff);
            Assert.False(msg.IsReadByMember);
            Assert.Contains("Thu Ngân - Thu Ngân Ly", msg.GetDisplayHeader());
        }

        // =========================================================================
        // 2. STATION CHAT SERVICE TESTS
        // =========================================================================

        [Fact]
        public void StationChatService_SendMessage_TriggersEventsAndStoresMessage()
        {
            ChatMessage? receivedMsg = null;
            int eventStationId = 0;

            Action<ChatMessage> msgHandler = m => receivedMsg = m;
            Action<int> unreadHandler = s => eventStationId = s;

            StationChatService.OnMessageSent += msgHandler;
            StationChatService.OnUnreadCountChanged += unreadHandler;

            try
            {
                var sent = StationChatService.SendMessage(
                    stationId: 5,
                    stationName: "PC-05",
                    senderRole: "Member",
                    senderUsername: "player05",
                    senderDisplayName: "Nguyễn Văn B",
                    messageText: "Xin thêm nước lọc/đá",
                    quickTag: "Water");

                Assert.NotNull(receivedMsg);
                Assert.Equal(sent.MessageId, receivedMsg.MessageId);
                Assert.Equal(5, eventStationId);
                Assert.Equal("Xin thêm nước lọc/đá", receivedMsg.MessageText);

                var history = StationChatService.GetMessages(5);
                Assert.Single(history);
                Assert.Equal(sent.MessageId, history[0].MessageId);
            }
            finally
            {
                StationChatService.OnMessageSent -= msgHandler;
                StationChatService.OnUnreadCountChanged -= unreadHandler;
            }
        }

        [Fact]
        public void StationChatService_UnreadCountsAndMarkAsRead_WorkAccurately()
        {
            // Station 1: 2 unread messages from member
            StationChatService.SendMessage(1, "PC-01", "Member", "u1", "User 1", "Tin nhắn 1");
            StationChatService.SendMessage(1, "PC-01", "Member", "u1", "User 1", "Tin nhắn 2");

            // Station 2: 1 unread message from member
            StationChatService.SendMessage(2, "PC-02", "Member", "u2", "User 2", "Kẹt phím/chuột");

            // Station 2: 1 unread message from staff to member
            StationChatService.SendMessage(2, "PC-02", "Staff", "staff01", "Thu Ngân", "Kỹ thuật đang đến hỗ trợ");

            // Check staff unread count
            Assert.Equal(3, StationChatService.GetUnreadCountForStaff());
            Assert.Equal(2, StationChatService.GetUnreadCountForStaff(1));
            Assert.Equal(1, StationChatService.GetUnreadCountForStaff(2));

            // Check station unread count for member
            Assert.Equal(0, StationChatService.GetUnreadCountForStation(1));
            Assert.Equal(1, StationChatService.GetUnreadCountForStation(2));

            // Staff reads Station 1
            int markedForStaff = StationChatService.MarkAsReadByStaff(1);
            Assert.Equal(2, markedForStaff);
            Assert.Equal(0, StationChatService.GetUnreadCountForStaff(1));
            Assert.Equal(1, StationChatService.GetUnreadCountForStaff());

            // Member reads Station 2
            int markedForMember = StationChatService.MarkAsReadByMember(2);
            Assert.Equal(1, markedForMember);
            Assert.Equal(0, StationChatService.GetUnreadCountForStation(2));
        }

        [Fact]
        public void StationChatService_GetActiveChatStations_ReturnsOrderedStations()
        {
            StationChatService.SendMessage(3, "PC-03", "Member", "u3", "User 3", "Msg A");
            StationChatService.SendMessage(7, "PC-07", "Member", "u7", "User 7", "Msg B");
            StationChatService.SendMessage(3, "PC-03", "Staff", "staff", "Thu Ngân", "Msg C");

            var active = StationChatService.GetActiveChatStations();
            Assert.Equal(2, active.Count);
            Assert.Equal(3, active[0]); // Station 3 had latest message
            Assert.Equal(7, active[1]);
        }

        [Fact]
        public void StationChatService_QuickTemplates_ContainExpectedPresets()
        {
            Assert.Contains("Xin thêm nước lọc/đá", StationChatService.MemberQuickTemplates);
            Assert.Contains("Nạp 50k ví", StationChatService.MemberQuickTemplates);
            Assert.Contains("Kẹt phím/chuột", StationChatService.MemberQuickTemplates);
            Assert.Contains("Chỉnh điều hòa", StationChatService.MemberQuickTemplates);
            Assert.Contains("Bao lâu nữa có đồ ăn ạ?", StationChatService.MemberQuickTemplates);

            Assert.Contains("Nhân viên đang mang qua ngay", StationChatService.StaffQuickTemplates);
            Assert.Contains("Đã nạp tiền thành công nhé", StationChatService.StaffQuickTemplates);
            Assert.Contains("Kỹ thuật đang đến hỗ trợ", StationChatService.StaffQuickTemplates);
            Assert.Contains("Bếp đang làm đồ, đợi 2 phút nhé", StationChatService.StaffQuickTemplates);
        }

        // =========================================================================
        // 3. VIETQR HELPER TESTS
        // =========================================================================

        [Fact]
        public void VietQrHelper_BuildTransferSyntax_AdheresToCyberLoungeFormat()
        {
            string syntax1 = VietQrHelper.BuildTransferSyntax("PC-02", "Towie1206");
            Assert.Equal("NAP PC-02 TOWIE1206", syntax1);

            string syntax2 = VietQrHelper.BuildTransferSyntax("Máy VIP 01", "nguyễn công bằng");
            Assert.Equal("NAP MAYVIP01 NGUYENCONGBANG", syntax2);

            string syntax3 = VietQrHelper.BuildTransferSyntax("", "");
            Assert.Equal("NAP PC GUEST", syntax3);
        }

        [Fact]
        public void VietQrHelper_BuildFnBOrderSyntax_GeneratesCorrectFormat()
        {
            string syntax = VietQrHelper.BuildFnBOrderSyntax("PC-05", 142);
            Assert.Equal("ORDER PC-05 #142", syntax);
        }

        [Fact]
        public void VietQrHelper_FormatTlv_FormatsTwoDigitLength()
        {
            Assert.Equal("000201", VietQrHelper.FormatTlv("00", "01"));
            Assert.Equal("5303704", VietQrHelper.FormatTlv("53", "704"));
            Assert.Equal("5802VN", VietQrHelper.FormatTlv("58", "VN"));
        }

        [Fact]
        public void VietQrHelper_CalculateCrc16_ProducesFourHexDigits()
        {
            string crc = VietQrHelper.CalculateCrc16("0002010102126304");
            Assert.Equal(4, crc.Length);
            Assert.Matches("^[0-9A-F]{4}$", crc);
        }

        [Fact]
        public void VietQrHelper_GenerateNapas247Payload_ContainsStandardTags()
        {
            string payload = VietQrHelper.GenerateNapas247Payload("970422", "0868888888", 50000m, "NAP PC02 BANGNC");

            Assert.StartsWith("000201", payload); // Tag 00
            Assert.Contains("010212", payload);   // Tag 01 Dynamic
            Assert.Contains("A000000727", payload); // Napas GUID
            Assert.Contains("QRIBFTTA", payload);   // Napas Service
            Assert.Contains("970422", payload);     // MB Bank BIN
            Assert.Contains("0868888888", payload); // Account number
            Assert.Contains("5303704", payload);    // Currency VND
            Assert.Contains("540550000", payload);  // 50,000 VND
            Assert.Contains("5802VN", payload);     // Country VN
            Assert.Contains("NAP PC02 BANGNC", payload); // Transfer Content
            Assert.Contains("6304", payload);       // Tag 63 Checksum

            // Verify CRC integrity
            string payloadWithoutCrc = payload.Substring(0, payload.Length - 4);
            string crc = payload.Substring(payload.Length - 4);
            string calculatedCrc = VietQrHelper.CalculateCrc16(payloadWithoutCrc);
            Assert.Equal(calculatedCrc, crc);
        }

        [Fact]
        public void VietQrHelper_GenerateWalletTopUpPayload_EmbedsNapSyntax()
        {
            string payload = VietQrHelper.GenerateWalletTopUpPayload("PC-02", "towie1206", 100000m);
            Assert.Contains("NAP PC-02 TOWIE1206", payload);
            Assert.Contains("5406100000", payload);
        }

        [Fact]
        public void VietQrHelper_GenerateQrBitmap_ProducesValidBitmapImage()
        {
            string payload = VietQrHelper.GenerateWalletTopUpPayload("PC-02", "bangnc", 50000m);
            using (var bmp = VietQrHelper.GenerateQrBitmap(payload, pixelsPerModule: 4))
            {
                Assert.NotNull(bmp);
                Assert.True(bmp.Width > 50);
                Assert.True(bmp.Height > 50);
                Assert.Equal(bmp.Width, bmp.Height);

                // Check pixel color: top-left quiet zone pixel must be white
                Color cornerColor = bmp.GetPixel(0, 0);
                Assert.Equal(255, cornerColor.R);
                Assert.Equal(255, cornerColor.G);
                Assert.Equal(255, cornerColor.B);
            }
        }

        // =========================================================================
        // 4. UI INTEGRATION TESTS (STA THREAD)
        // =========================================================================

        private static void RunOnSTA(Action action)
        {
            Exception? ex = null;
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }

        [Fact]
        public void FormClientDashboard_HardwareSpecsAndNetworkLatencyHud_RendersExpectedHardwareString()
        {
            RunOnSTA(() =>
            {
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 100000m, "Gold");
                using var dashboard = new FormClientDashboard(new DAL.DBStation(), new DAL.DBBilling(), new DAL.DBUser(), stationId: 2)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                dashboard.Show();
                System.Windows.Forms.Application.DoEvents();

                dashboard.UpdateRealtimeHud();

                // Initial verify matching exact prompt specs
                Assert.Contains("i7-14700K | 32GB DDR5 | RTX 4070 Ti | 240Hz Fast-IPS", dashboard.HardwareLatencyText);
                Assert.Contains("🟢 Ping 12ms", dashboard.HardwareLatencyText);

                // Simulate latency change
                dashboard.NetworkPingMs = 85;
                dashboard.UpdateRealtimeHud();
                Assert.Contains("🔴 Ping 85ms", dashboard.HardwareLatencyText);

                dashboard.NetworkPingMs = 45;
                dashboard.UpdateRealtimeHud();
                Assert.Contains("🟡 Ping 45ms", dashboard.HardwareLatencyText);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_LiveChatTab_QuickChips_SendMessagesAndCreateBubbles()
        {
            RunOnSTA(() =>
            {
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 100000m, "Standard");
                using var dashboard = new FormClientDashboard(new DAL.DBStation(), new DAL.DBBilling(), new DAL.DBUser(), stationId: 3)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                dashboard.Show();
                System.Windows.Forms.Application.DoEvents();

                dashboard.SwitchToChatTab();
                System.Windows.Forms.Application.DoEvents();

                int stId = dashboard.CurrentStation?.StationId ?? 3;

                // Test quick chip 1: Water
                dashboard.btnChipWater.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                var messages = StationChatService.GetMessages(stId);
                Assert.Contains(messages, m => m.MessageText == "Xin thêm nước lọc/đá" && m.QuickTag == "Water");

                // Test quick chip 2: Hardware
                dashboard.btnChipHardware.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                messages = StationChatService.GetMessages(stId);
                Assert.Contains(messages, m => m.MessageText == "Kẹt phím/chuột" && m.QuickTag == "Hardware");

                // Test quick chip 3: Food
                dashboard.btnChipFood.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                messages = StationChatService.GetMessages(stId);
                Assert.Contains(messages, m => m.MessageText == "Bao lâu nữa có đồ ăn ạ?" && m.QuickTag == "Food");

                // Verify UI bubbles count
                Assert.Equal(3, dashboard.ClientChatMessageCount);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_LiveChatTab_TypingInputWithEnter_SendsMessage()
        {
            RunOnSTA(() =>
            {
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using var dashboard = new FormClientDashboard(new DAL.DBStation(), new DAL.DBBilling(), new DAL.DBUser(), stationId: 1)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                dashboard.Show();
                System.Windows.Forms.Application.DoEvents();

                dashboard.SwitchToChatTab();
                System.Windows.Forms.Application.DoEvents();

                int stId = dashboard.CurrentStation?.StationId ?? 1;

                dashboard.txtClientChatMessage.Text = "Em muốn hỏi mật khẩu wifi phòng máy ạ";
                dashboard.SendClientTypedMessage();
                System.Windows.Forms.Application.DoEvents();

                var messages = StationChatService.GetMessages(stId);
                Assert.Contains(messages, m => m.MessageText == "Em muốn hỏi mật khẩu wifi phòng máy ạ");
                Assert.Equal(string.Empty, dashboard.txtClientChatMessage.Text);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_UnreadBadgeUpdates_WhenStaffRepliesAndMemberIsNotOnTab()
        {
            RunOnSTA(() =>
            {
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Nguyễn Văn A", 50000m, "Standard");
                using var dashboard = new FormClientDashboard(new DAL.DBStation(), new DAL.DBBilling(), new DAL.DBUser(), stationId: 4)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                dashboard.Show();
                System.Windows.Forms.Application.DoEvents();

                int stId = dashboard.CurrentStation?.StationId ?? 4;

                // Send member message
                dashboard.SendClientChatMessage("Bao lâu nữa có đồ ăn ạ?", "Food");
                System.Windows.Forms.Application.DoEvents();

                // Staff replies
                StationChatService.SendMessage(stId, "PC-04", "Staff", "staff01", "Thu Ngân Ly", "Bếp đang làm đồ, đợi 2 phút nhé", "Kitchen");
                System.Windows.Forms.Application.DoEvents();

                // Member is currently on tab 0 (Game Hub)
                Assert.True(dashboard.tabMain.SelectedIndex != 2);
                Assert.Equal(1, StationChatService.GetUnreadCountForStation(stId));
                Assert.Contains("(1)", dashboard.tabChat.Text);

                // Member switches to tabChat
                dashboard.SwitchToChatTab();
                System.Windows.Forms.Application.DoEvents();
                Assert.Equal(0, StationChatService.GetUnreadCountForStation(stId));
                Assert.Equal("💬 HỖ TRỢ & CHAT QUẦY", dashboard.tabChat.Text);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormClientDashboard_VietQrDrawer_GeneratesValidQrAndCorrectNapSyntax()
        {
            RunOnSTA(() =>
            {
                CurrentUserSession.SetSession(2, "towie1206", "Member", "Nguyễn Công Bằng", 50000m, "Gold");
                using var dashboard = new FormClientDashboard(new DAL.DBStation(), new DAL.DBBilling(), new DAL.DBUser(), stationId: 2)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                dashboard.Show();
                System.Windows.Forms.Application.DoEvents();

                dashboard.RefreshVietQrDrawer(50000m);

                Assert.NotNull(dashboard.picDrawerQr.Image);
                Assert.Contains("NAP", dashboard.lblDrawerSyntax.Text);
                Assert.Contains("50,000", dashboard.lblDrawerSyntax.Text);

                dashboard.SwitchToChatTab();
                System.Windows.Forms.Application.DoEvents();

                dashboard.btnOpenVietQrTopUp.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                Assert.True(dashboard.pnlVietQrDrawer.Visible);

                dashboard.btnCloseDrawer.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                Assert.False(dashboard.pnlVietQrDrawer.Visible);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormCashierChatHub_StationSelect_LoadsHistoryAndMarksRead()
        {
            RunOnSTA(() =>
            {
                StationChatService.SendMessage(1, "PC-01", "Member", "gamer01", "Gamer 1", "Tin nhắn 1");
                StationChatService.SendMessage(2, "PC-02", "Member", "gamer02", "Gamer 2", "Tin nhắn 2");

                CurrentUserSession.SetSession(5, "staff01", "Staff", "Nhân Viên Quầy");
                using var hub = new FormCashierChatHub(new DAL.DBStation(), initialStationId: 2)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                hub.Show();
                System.Windows.Forms.Application.DoEvents();

                Assert.Equal(2, hub.SelectedStationId);
                // Station 2 unread should be cleared by opening
                Assert.Equal(0, StationChatService.GetUnreadCountForStaff(2));
                // Station 1 unread remains 1
                Assert.Equal(1, StationChatService.GetUnreadCountForStaff(1));

                // Switch to Station 1
                hub.SelectStation(1);
                System.Windows.Forms.Application.DoEvents();
                Assert.Equal(1, hub.SelectedStationId);
                Assert.Equal(0, StationChatService.GetUnreadCountForStaff(1));

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void FormCashierChatHub_CashierQuickReplyButtons_SendExpectedMessages()
        {
            RunOnSTA(() =>
            {
                CurrentUserSession.SetSession(5, "staff01", "Staff", "Thu Ngân Linh");
                using var hub = new FormCashierChatHub(new DAL.DBStation(), initialStationId: 1)
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                };
                hub.Show();
                System.Windows.Forms.Application.DoEvents();

                hub.btnQuickBring.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                hub.btnQuickDeposit.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                hub.btnQuickTech.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                hub.btnQuickKitchen.PerformClick();
                System.Windows.Forms.Application.DoEvents();

                var messages = StationChatService.GetMessages(1);
                Assert.Equal(4, messages.Count);
                Assert.Equal("Nhân viên đang mang qua ngay", messages[0].MessageText);
                Assert.Equal("Đã nạp tiền thành công nhé", messages[1].MessageText);
                Assert.Equal("Kỹ thuật đang đến hỗ trợ", messages[2].MessageText);
                Assert.Equal("Bếp đang làm đồ, đợi 2 phút nhé", messages[3].MessageText);

                CurrentUserSession.Clear();
            });
        }

        [Fact]
        public void Form1_ChatHubButton_And_RolePermissions()
        {
            RunOnSTA(() =>
            {
                // Admin role
                CurrentUserSession.SetSession(1, "admin", "Admin", "Quản Lý");
                using (var form1 = new Form1
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                })
                {
                    form1.Show();
                    System.Windows.Forms.Application.DoEvents();
                    form1.ApplyRolePermissions();
                    Assert.True(form1.btnChatHub.Enabled);
                    Assert.True(form1.btnChatStation.Enabled);

                    using var hub = form1.OpenChatHub(1);
                    Assert.NotNull(hub);
                    Assert.Equal(1, hub.SelectedStationId);
                }

                // Staff role
                CurrentUserSession.SetSession(5, "staff01", "Staff", "Nhân Viên");
                using (var form1Staff = new Form1
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                })
                {
                    form1Staff.Show();
                    System.Windows.Forms.Application.DoEvents();
                    form1Staff.ApplyRolePermissions();
                    Assert.True(form1Staff.btnChatHub.Enabled);
                    Assert.True(form1Staff.btnChatStation.Enabled);
                }

                // Member role
                CurrentUserSession.SetSession(2, "gamer01", "Member", "Hội Viên");
                using (var form1Member = new Form1
                {
                    ShowInTaskbar = false,
                    StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                    Location = new Point(-2000, -2000)
                })
                {
                    form1Member.Show();
                    System.Windows.Forms.Application.DoEvents();
                    form1Member.ApplyRolePermissions();
                    Assert.False(form1Member.btnChatHub.Enabled);
                    Assert.False(form1Member.btnChatStation.Enabled);
                }

                CurrentUserSession.Clear();
            });
        }
    }
}
