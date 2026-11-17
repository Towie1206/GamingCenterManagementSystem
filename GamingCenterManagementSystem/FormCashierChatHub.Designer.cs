namespace GamingCenterManagementSystem
{
    partial class FormCashierChatHub
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new System.Windows.Forms.Panel();
            lblTitle = new System.Windows.Forms.Label();
            lblSubtitle = new System.Windows.Forms.Label();
            lblUnreadTotal = new System.Windows.Forms.Label();
            pnlStationSelect = new System.Windows.Forms.Panel();
            lblSelectPrompt = new System.Windows.Forms.Label();
            cboStation = new System.Windows.Forms.ComboBox();
            btnRefresh = new System.Windows.Forms.Button();
            lblSelectedStationDetails = new System.Windows.Forms.Label();
            btnQuickQr = new System.Windows.Forms.Button();
            pnlChatContainer = new System.Windows.Forms.Panel();
            flpChatHistory = new System.Windows.Forms.FlowLayoutPanel();
            pnlQuickReplies = new System.Windows.Forms.Panel();
            lblQuickPrompt = new System.Windows.Forms.Label();
            btnQuickBring = new System.Windows.Forms.Button();
            btnQuickDeposit = new System.Windows.Forms.Button();
            btnQuickTech = new System.Windows.Forms.Button();
            btnQuickKitchen = new System.Windows.Forms.Button();
            pnlInputBar = new System.Windows.Forms.Panel();
            txtMessage = new System.Windows.Forms.TextBox();
            btnSend = new System.Windows.Forms.Button();
            pnlHeader.SuspendLayout();
            pnlStationSelect.SuspendLayout();
            pnlChatContainer.SuspendLayout();
            pnlQuickReplies.SuspendLayout();
            pnlInputBar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 24, 38);
            pnlHeader.Controls.Add(lblUnreadTotal);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(860, 65);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 210, 255);
            lblTitle.Location = new System.Drawing.Point(15, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(430, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💬 TRUNG TÂM CHAT & HỖ TRỢ TRẠM MÁY (CASHIER)";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(160, 180, 210);
            lblSubtitle.Location = new System.Drawing.Point(17, 37);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(415, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Kênh tương tác trực tiếp 2 chiều giữa Thu ngân quầy và Khách hàng chơi máy";
            // 
            // lblUnreadTotal
            // 
            lblUnreadTotal.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblUnreadTotal.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            lblUnreadTotal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblUnreadTotal.ForeColor = System.Drawing.Color.White;
            lblUnreadTotal.Location = new System.Drawing.Point(680, 16);
            lblUnreadTotal.Name = "lblUnreadTotal";
            lblUnreadTotal.Size = new System.Drawing.Size(165, 32);
            lblUnreadTotal.TabIndex = 2;
            lblUnreadTotal.Text = "🔔 0 tin chưa đọc";
            lblUnreadTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStationSelect
            // 
            pnlStationSelect.BackColor = System.Drawing.Color.FromArgb(24, 32, 48);
            pnlStationSelect.Controls.Add(btnQuickQr);
            pnlStationSelect.Controls.Add(lblSelectedStationDetails);
            pnlStationSelect.Controls.Add(btnRefresh);
            pnlStationSelect.Controls.Add(cboStation);
            pnlStationSelect.Controls.Add(lblSelectPrompt);
            pnlStationSelect.Dock = System.Windows.Forms.DockStyle.Top;
            pnlStationSelect.Location = new System.Drawing.Point(0, 65);
            pnlStationSelect.Name = "pnlStationSelect";
            pnlStationSelect.Size = new System.Drawing.Size(860, 50);
            pnlStationSelect.TabIndex = 1;
            // 
            // lblSelectPrompt
            // 
            lblSelectPrompt.AutoSize = true;
            lblSelectPrompt.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblSelectPrompt.ForeColor = System.Drawing.Color.White;
            lblSelectPrompt.Location = new System.Drawing.Point(15, 15);
            lblSelectPrompt.Name = "lblSelectPrompt";
            lblSelectPrompt.Size = new System.Drawing.Size(107, 17);
            lblSelectPrompt.TabIndex = 0;
            lblSelectPrompt.Text = "🖥️ Chọn trạm:";
            // 
            // cboStation
            // 
            cboStation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboStation.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            cboStation.FormattingEnabled = true;
            cboStation.Location = new System.Drawing.Point(125, 12);
            cboStation.Name = "cboStation";
            cboStation.Size = new System.Drawing.Size(180, 24);
            cboStation.TabIndex = 1;
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = System.Drawing.Color.FromArgb(50, 65, 90);
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnRefresh.ForeColor = System.Drawing.Color.White;
            btnRefresh.Location = new System.Drawing.Point(312, 11);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(75, 27);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "🔄 Tải lại";
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // lblSelectedStationDetails
            // 
            lblSelectedStationDetails.AutoSize = true;
            lblSelectedStationDetails.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSelectedStationDetails.ForeColor = System.Drawing.Color.FromArgb(180, 210, 240);
            lblSelectedStationDetails.Location = new System.Drawing.Point(400, 16);
            lblSelectedStationDetails.Name = "lblSelectedStationDetails";
            lblSelectedStationDetails.Size = new System.Drawing.Size(185, 15);
            lblSelectedStationDetails.TabIndex = 3;
            lblSelectedStationDetails.Text = "Trạng thái: Sẵn sàng tương tác";
            // 
            // btnQuickQr
            // 
            btnQuickQr.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnQuickQr.BackColor = System.Drawing.Color.FromArgb(13, 148, 136);
            btnQuickQr.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuickQr.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            btnQuickQr.ForeColor = System.Drawing.Color.White;
            btnQuickQr.Location = new System.Drawing.Point(705, 10);
            btnQuickQr.Name = "btnQuickQr";
            btnQuickQr.Size = new System.Drawing.Size(140, 28);
            btnQuickQr.TabIndex = 4;
            btnQuickQr.Text = "⚡ VietQR Nạp Ví";
            btnQuickQr.UseVisualStyleBackColor = false;
            // 
            // pnlChatContainer
            // 
            pnlChatContainer.BackColor = System.Drawing.Color.FromArgb(14, 18, 26);
            pnlChatContainer.Controls.Add(flpChatHistory);
            pnlChatContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlChatContainer.Location = new System.Drawing.Point(0, 115);
            pnlChatContainer.Name = "pnlChatContainer";
            pnlChatContainer.Padding = new System.Windows.Forms.Padding(12);
            pnlChatContainer.Size = new System.Drawing.Size(860, 360);
            pnlChatContainer.TabIndex = 2;
            // 
            // flpChatHistory
            // 
            flpChatHistory.AutoScroll = true;
            flpChatHistory.BackColor = System.Drawing.Color.FromArgb(16, 22, 34);
            flpChatHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            flpChatHistory.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flpChatHistory.Location = new System.Drawing.Point(12, 12);
            flpChatHistory.Name = "flpChatHistory";
            flpChatHistory.Padding = new System.Windows.Forms.Padding(8);
            flpChatHistory.Size = new System.Drawing.Size(836, 336);
            flpChatHistory.TabIndex = 0;
            flpChatHistory.WrapContents = false;
            // 
            // pnlQuickReplies
            // 
            pnlQuickReplies.BackColor = System.Drawing.Color.FromArgb(22, 28, 42);
            pnlQuickReplies.Controls.Add(btnQuickKitchen);
            pnlQuickReplies.Controls.Add(btnQuickTech);
            pnlQuickReplies.Controls.Add(btnQuickDeposit);
            pnlQuickReplies.Controls.Add(btnQuickBring);
            pnlQuickReplies.Controls.Add(lblQuickPrompt);
            pnlQuickReplies.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlQuickReplies.Location = new System.Drawing.Point(0, 475);
            pnlQuickReplies.Name = "pnlQuickReplies";
            pnlQuickReplies.Size = new System.Drawing.Size(860, 42);
            pnlQuickReplies.TabIndex = 3;
            // 
            // lblQuickPrompt
            // 
            lblQuickPrompt.AutoSize = true;
            lblQuickPrompt.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblQuickPrompt.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            lblQuickPrompt.Location = new System.Drawing.Point(12, 13);
            lblQuickPrompt.Name = "lblQuickPrompt";
            lblQuickPrompt.Size = new System.Drawing.Size(86, 15);
            lblQuickPrompt.TabIndex = 0;
            lblQuickPrompt.Text = "⚡ Trả lời nhanh:";
            // 
            // btnQuickBring
            // 
            btnQuickBring.BackColor = System.Drawing.Color.FromArgb(38, 50, 72);
            btnQuickBring.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuickBring.Font = new System.Drawing.Font("Segoe UI", 8F);
            btnQuickBring.ForeColor = System.Drawing.Color.White;
            btnQuickBring.Location = new System.Drawing.Point(105, 7);
            btnQuickBring.Name = "btnQuickBring";
            btnQuickBring.Size = new System.Drawing.Size(175, 28);
            btnQuickBring.TabIndex = 1;
            btnQuickBring.Text = "Nhân viên đang mang qua ngay";
            btnQuickBring.UseVisualStyleBackColor = false;
            // 
            // btnQuickDeposit
            // 
            btnQuickDeposit.BackColor = System.Drawing.Color.FromArgb(38, 50, 72);
            btnQuickDeposit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuickDeposit.Font = new System.Drawing.Font("Segoe UI", 8F);
            btnQuickDeposit.ForeColor = System.Drawing.Color.White;
            btnQuickDeposit.Location = new System.Drawing.Point(286, 7);
            btnQuickDeposit.Name = "btnQuickDeposit";
            btnQuickDeposit.Size = new System.Drawing.Size(165, 28);
            btnQuickDeposit.TabIndex = 2;
            btnQuickDeposit.Text = "Đã nạp tiền thành công nhé";
            btnQuickDeposit.UseVisualStyleBackColor = false;
            // 
            // btnQuickTech
            // 
            btnQuickTech.BackColor = System.Drawing.Color.FromArgb(38, 50, 72);
            btnQuickTech.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuickTech.Font = new System.Drawing.Font("Segoe UI", 8F);
            btnQuickTech.ForeColor = System.Drawing.Color.White;
            btnQuickTech.Location = new System.Drawing.Point(457, 7);
            btnQuickTech.Name = "btnQuickTech";
            btnQuickTech.Size = new System.Drawing.Size(155, 28);
            btnQuickTech.TabIndex = 3;
            btnQuickTech.Text = "Kỹ thuật đang đến hỗ trợ";
            btnQuickTech.UseVisualStyleBackColor = false;
            // 
            // btnQuickKitchen
            // 
            btnQuickKitchen.BackColor = System.Drawing.Color.FromArgb(38, 50, 72);
            btnQuickKitchen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuickKitchen.Font = new System.Drawing.Font("Segoe UI", 8F);
            btnQuickKitchen.ForeColor = System.Drawing.Color.White;
            btnQuickKitchen.Location = new System.Drawing.Point(618, 7);
            btnQuickKitchen.Name = "btnQuickKitchen";
            btnQuickKitchen.Size = new System.Drawing.Size(180, 28);
            btnQuickKitchen.TabIndex = 4;
            btnQuickKitchen.Text = "Bếp đang làm đồ, đợi 2 phút nhé";
            btnQuickKitchen.UseVisualStyleBackColor = false;
            // 
            // pnlInputBar
            // 
            pnlInputBar.BackColor = System.Drawing.Color.FromArgb(18, 24, 38);
            pnlInputBar.Controls.Add(btnSend);
            pnlInputBar.Controls.Add(txtMessage);
            pnlInputBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlInputBar.Location = new System.Drawing.Point(0, 517);
            pnlInputBar.Name = "pnlInputBar";
            pnlInputBar.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            pnlInputBar.Size = new System.Drawing.Size(860, 55);
            pnlInputBar.TabIndex = 4;
            // 
            // txtMessage
            // 
            txtMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtMessage.BackColor = System.Drawing.Color.FromArgb(30, 38, 56);
            txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtMessage.ForeColor = System.Drawing.Color.White;
            txtMessage.Location = new System.Drawing.Point(15, 14);
            txtMessage.Name = "txtMessage";
            txtMessage.PlaceholderText = "Nhập tin nhắn phản hồi tới máy trạm... (Nhấn Enter để gửi)";
            txtMessage.Size = new System.Drawing.Size(710, 25);
            txtMessage.TabIndex = 0;
            // 
            // btnSend
            // 
            btnSend.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnSend.BackColor = System.Drawing.Color.FromArgb(0, 180, 136);
            btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSend.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnSend.ForeColor = System.Drawing.Color.White;
            btnSend.Location = new System.Drawing.Point(735, 11);
            btnSend.Name = "btnSend";
            btnSend.Size = new System.Drawing.Size(110, 32);
            btnSend.TabIndex = 1;
            btnSend.Text = "Gửi 📤";
            btnSend.UseVisualStyleBackColor = false;
            // 
            // FormCashierChatHub
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(860, 572);
            Controls.Add(pnlChatContainer);
            Controls.Add(pnlQuickReplies);
            Controls.Add(pnlInputBar);
            Controls.Add(pnlStationSelect);
            Controls.Add(pnlHeader);
            MinimumSize = new System.Drawing.Size(780, 480);
            Name = "FormCashierChatHub";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "💬 Trung Tâm Chat Thu Ngân & Máy Trạm";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStationSelect.ResumeLayout(false);
            pnlStationSelect.PerformLayout();
            pnlChatContainer.ResumeLayout(false);
            pnlQuickReplies.ResumeLayout(false);
            pnlQuickReplies.PerformLayout();
            pnlInputBar.ResumeLayout(false);
            pnlInputBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUnreadTotal;
        private System.Windows.Forms.Panel pnlStationSelect;
        private System.Windows.Forms.Label lblSelectPrompt;
        public System.Windows.Forms.ComboBox cboStation;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblSelectedStationDetails;
        private System.Windows.Forms.Button btnQuickQr;
        private System.Windows.Forms.Panel pnlChatContainer;
        public System.Windows.Forms.FlowLayoutPanel flpChatHistory;
        private System.Windows.Forms.Panel pnlQuickReplies;
        private System.Windows.Forms.Label lblQuickPrompt;
        public System.Windows.Forms.Button btnQuickBring;
        public System.Windows.Forms.Button btnQuickDeposit;
        public System.Windows.Forms.Button btnQuickTech;
        public System.Windows.Forms.Button btnQuickKitchen;
        private System.Windows.Forms.Panel pnlInputBar;
        public System.Windows.Forms.TextBox txtMessage;
        public System.Windows.Forms.Button btnSend;
    }
}
