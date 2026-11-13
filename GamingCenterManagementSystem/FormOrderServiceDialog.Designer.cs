namespace GamingCenterManagementSystem
{
    partial class FormOrderServiceDialog
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
            pnlHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            lblSelectFood = new Label();
            cboFood = new ComboBox();
            lblItemCategory = new Label();
            lblUnitPrice = new Label();
            lblQuantity = new Label();
            nudQuantity = new NumericUpDown();
            lblNote = new Label();
            txtNote = new TextBox();
            pnlSummary = new Panel();
            lblTotalAmount = new Label();
            lblTotalTitle = new Label();
            btnConfirm = new Button();
            btnCancel = new Button();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).BeginInit();
            pnlSummary.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(18, 22, 38);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(464, 75);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblSubtitle.ForeColor = Color.FromArgb(0, 210, 255);
            lblSubtitle.Location = new Point(18, 44);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(295, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Quản trị F&B trực tiếp trên Màn hình chính Dashboard";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(16, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(244, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🛒 GỌI MÓN F&B CHO MÁY";
            // 
            // lblSelectFood
            // 
            lblSelectFood.AutoSize = true;
            lblSelectFood.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSelectFood.ForeColor = Color.FromArgb(24, 30, 54);
            lblSelectFood.Location = new Point(20, 95);
            lblSelectFood.Name = "lblSelectFood";
            lblSelectFood.Size = new Size(160, 17);
            lblSelectFood.TabIndex = 1;
            lblSelectFood.Text = "Chọn món ăn / đồ uống:";
            // 
            // cboFood
            // 
            cboFood.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFood.Font = new Font("Segoe UI", 9.5F);
            cboFood.FormattingEnabled = true;
            cboFood.Location = new Point(20, 120);
            cboFood.Name = "cboFood";
            cboFood.Size = new Size(424, 25);
            cboFood.TabIndex = 2;
            // 
            // lblItemCategory
            // 
            lblItemCategory.AutoSize = true;
            lblItemCategory.Font = new Font("Segoe UI", 9F);
            lblItemCategory.ForeColor = Color.FromArgb(80, 90, 105);
            lblItemCategory.Location = new Point(20, 155);
            lblItemCategory.Name = "lblItemCategory";
            lblItemCategory.Size = new Size(130, 15);
            lblItemCategory.TabIndex = 3;
            lblItemCategory.Text = "Danh mục: -- | Tồn kho: --";
            // 
            // lblUnitPrice
            // 
            lblUnitPrice.AutoSize = true;
            lblUnitPrice.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUnitPrice.ForeColor = Color.FromArgb(0, 102, 204);
            lblUnitPrice.Location = new Point(280, 155);
            lblUnitPrice.Name = "lblUnitPrice";
            lblUnitPrice.Size = new Size(95, 15);
            lblUnitPrice.TabIndex = 4;
            lblUnitPrice.Text = "Đơn giá: 0 VNĐ";
            // 
            // lblQuantity
            // 
            lblQuantity.AutoSize = true;
            lblQuantity.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblQuantity.ForeColor = Color.FromArgb(24, 30, 54);
            lblQuantity.Location = new Point(20, 190);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(100, 17);
            lblQuantity.TabIndex = 5;
            lblQuantity.Text = "Số lượng gọi:";
            // 
            // nudQuantity
            // 
            nudQuantity.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            nudQuantity.Location = new Point(20, 215);
            nudQuantity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudQuantity.Name = "nudQuantity";
            nudQuantity.Size = new Size(130, 25);
            nudQuantity.TabIndex = 6;
            nudQuantity.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblNote.ForeColor = Color.FromArgb(24, 30, 54);
            lblNote.Location = new Point(20, 255);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(125, 17);
            lblNote.TabIndex = 7;
            lblNote.Text = "Ghi chú (tùy chọn):";
            // 
            // txtNote
            // 
            txtNote.Font = new Font("Segoe UI", 9.5F);
            txtNote.Location = new Point(20, 280);
            txtNote.MaxLength = 255;
            txtNote.Name = "txtNote";
            txtNote.PlaceholderText = "Ví dụ: Ít đá, không cay, làm liền...";
            txtNote.Size = new Size(424, 24);
            txtNote.TabIndex = 8;
            // 
            // pnlSummary
            // 
            pnlSummary.BackColor = Color.FromArgb(235, 245, 255);
            pnlSummary.BorderStyle = BorderStyle.FixedSingle;
            pnlSummary.Controls.Add(lblTotalAmount);
            pnlSummary.Controls.Add(lblTotalTitle);
            pnlSummary.Location = new Point(20, 325);
            pnlSummary.Name = "pnlSummary";
            pnlSummary.Size = new Size(424, 65);
            pnlSummary.TabIndex = 9;
            // 
            // lblTotalAmount
            // 
            lblTotalAmount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalAmount.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTotalAmount.ForeColor = Color.FromArgb(0, 130, 0);
            lblTotalAmount.Location = new Point(160, 18);
            lblTotalAmount.Name = "lblTotalAmount";
            lblTotalAmount.Size = new Size(250, 28);
            lblTotalAmount.TabIndex = 1;
            lblTotalAmount.Text = "0 VNĐ";
            lblTotalAmount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalTitle
            // 
            lblTotalTitle.AutoSize = true;
            lblTotalTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTotalTitle.ForeColor = Color.FromArgb(24, 30, 54);
            lblTotalTitle.Location = new Point(12, 23);
            lblTotalTitle.Name = "lblTotalTitle";
            lblTotalTitle.Size = new Size(137, 17);
            lblTotalTitle.TabIndex = 0;
            lblTotalTitle.Text = "💵 TIỀN DỊCH VỤ:";
            // 
            // btnConfirm
            // 
            btnConfirm.BackColor = Color.FromArgb(34, 139, 34);
            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConfirm.ForeColor = Color.White;
            btnConfirm.Location = new Point(20, 410);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(270, 42);
            btnConfirm.TabIndex = 10;
            btnConfirm.Text = "✅ XÁC NHẬN GỌI MÓN";
            btnConfirm.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(304, 410);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(140, 42);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "❌ Đóng";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // FormOrderServiceDialog
            // 
            AcceptButton = btnConfirm;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(464, 475);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Controls.Add(pnlSummary);
            Controls.Add(txtNote);
            Controls.Add(lblNote);
            Controls.Add(nudQuantity);
            Controls.Add(lblQuantity);
            Controls.Add(lblUnitPrice);
            Controls.Add(lblItemCategory);
            Controls.Add(cboFood);
            Controls.Add(lblSelectFood);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormOrderServiceDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gọi Món F&B Cho Máy Trạm";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).EndInit();
            pnlSummary.ResumeLayout(false);
            pnlSummary.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblSelectFood;
        private ComboBox cboFood;
        private Label lblItemCategory;
        private Label lblUnitPrice;
        private Label lblQuantity;
        private NumericUpDown nudQuantity;
        private Label lblNote;
        private TextBox txtNote;
        private Panel pnlSummary;
        private Label lblTotalTitle;
        private Label lblTotalAmount;
        private Button btnConfirm;
        private Button btnCancel;
    }
}
