namespace FashionShop.Presentation
{
    partial class FrmPaymentQR
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblCountdownTitle = new System.Windows.Forms.Label();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.btnDownloadQR = new System.Windows.Forms.Button();
            this.lblBank = new System.Windows.Forms.Label();
            this.lblAccount = new System.Windows.Forms.Label();
            this.lblHolder = new System.Windows.Forms.Label();
            this.lblAmount = new System.Windows.Forms.Label();
            this.lblContent = new System.Windows.Forms.Label();
            this.btnCopyAccount = new System.Windows.Forms.Button();
            this.btnCopyHolder = new System.Windows.Forms.Button();
            this.btnCopyAmount = new System.Windows.Forms.Button();
            this.btnCopyContent = new System.Windows.Forms.Button();
            this.groupBoxNote = new System.Windows.Forms.GroupBox();
            this.lblNote1 = new System.Windows.Forms.Label();
            this.lblNote2 = new System.Windows.Forms.Label();
            this.lblNote3 = new System.Windows.Forms.Label();
            this.pbQR = new System.Windows.Forms.PictureBox();
            this.groupBoxNote.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbQR)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCountdownTitle
            // 
            this.lblCountdownTitle.AutoSize = true;
            this.lblCountdownTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCountdownTitle.Location = new System.Drawing.Point(170, 9);
            this.lblCountdownTitle.Name = "lblCountdownTitle";
            this.lblCountdownTitle.Size = new System.Drawing.Size(241, 20);
            this.lblCountdownTitle.TabIndex = 0;
            this.lblCountdownTitle.Text = "Trang chuyển khoản sẽ đóng sau:";
            // 
            // lblCountdown
            // 
            this.lblCountdown.AutoSize = true;
            this.lblCountdown.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCountdown.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblCountdown.Location = new System.Drawing.Point(251, 38);
            this.lblCountdown.Name = "lblCountdown";
            this.lblCountdown.Size = new System.Drawing.Size(77, 32);
            this.lblCountdown.TabIndex = 1;
            this.lblCountdown.Text = "00:00";
            // 
            // btnDownloadQR
            // 
            this.btnDownloadQR.Location = new System.Drawing.Point(195, 259);
            this.btnDownloadQR.Name = "btnDownloadQR";
            this.btnDownloadQR.Size = new System.Drawing.Size(180, 30);
            this.btnDownloadQR.TabIndex = 3;
            this.btnDownloadQR.Text = "Tải xuống mã QR";
            this.btnDownloadQR.Click += new System.EventHandler(this.btnDownloadQR_Click);
            // 
            // lblBank
            // 
            this.lblBank.AutoSize = true;
            this.lblBank.Location = new System.Drawing.Point(145, 304);
            this.lblBank.Name = "lblBank";
            this.lblBank.Size = new System.Drawing.Size(134, 16);
            this.lblBank.TabIndex = 4;
            this.lblBank.Text = "Tên ngân hàng: BIDV";
            // 
            // lblAccount
            // 
            this.lblAccount.AutoSize = true;
            this.lblAccount.Location = new System.Drawing.Point(145, 334);
            this.lblAccount.Name = "lblAccount";
            this.lblAccount.Size = new System.Drawing.Size(87, 16);
            this.lblAccount.TabIndex = 5;
            this.lblAccount.Text = "Số tài khoản: ";
            // 
            // lblHolder
            // 
            this.lblHolder.AutoSize = true;
            this.lblHolder.Location = new System.Drawing.Point(145, 364);
            this.lblHolder.Name = "lblHolder";
            this.lblHolder.Size = new System.Drawing.Size(118, 16);
            this.lblHolder.TabIndex = 7;
            this.lblHolder.Text = "Tên chủ tài khoản: ";
            // 
            // lblAmount
            // 
            this.lblAmount.AutoSize = true;
            this.lblAmount.Location = new System.Drawing.Point(145, 394);
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Size = new System.Drawing.Size(103, 16);
            this.lblAmount.TabIndex = 9;
            this.lblAmount.Text = "Số tiền chuyển:  ";
            this.lblAmount.Click += new System.EventHandler(this.lblAmount_Click);
            // 
            // lblContent
            // 
            this.lblContent.AutoSize = true;
            this.lblContent.Location = new System.Drawing.Point(145, 424);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(153, 16);
            this.lblContent.TabIndex = 11;
            this.lblContent.Text = "Nội dung chuyển khoản: ";
            // 
            // btnCopyAccount
            // 
            this.btnCopyAccount.Location = new System.Drawing.Point(422, 328);
            this.btnCopyAccount.Name = "btnCopyAccount";
            this.btnCopyAccount.Size = new System.Drawing.Size(96, 28);
            this.btnCopyAccount.TabIndex = 6;
            this.btnCopyAccount.Text = "Sao chép";
            this.btnCopyAccount.Click += new System.EventHandler(this.btnCopyAccount_Click);
            // 
            // btnCopyHolder
            // 
            this.btnCopyHolder.Location = new System.Drawing.Point(422, 358);
            this.btnCopyHolder.Name = "btnCopyHolder";
            this.btnCopyHolder.Size = new System.Drawing.Size(96, 28);
            this.btnCopyHolder.TabIndex = 8;
            this.btnCopyHolder.Text = "Sao chép";
            this.btnCopyHolder.Click += new System.EventHandler(this.btnCopyHolder_Click);
            // 
            // btnCopyAmount
            // 
            this.btnCopyAmount.Location = new System.Drawing.Point(422, 388);
            this.btnCopyAmount.Name = "btnCopyAmount";
            this.btnCopyAmount.Size = new System.Drawing.Size(96, 28);
            this.btnCopyAmount.TabIndex = 10;
            this.btnCopyAmount.Text = "Sao chép";
            this.btnCopyAmount.Click += new System.EventHandler(this.btnCopyAmount_Click);
            // 
            // btnCopyContent
            // 
            this.btnCopyContent.Location = new System.Drawing.Point(422, 418);
            this.btnCopyContent.Name = "btnCopyContent";
            this.btnCopyContent.Size = new System.Drawing.Size(96, 28);
            this.btnCopyContent.TabIndex = 12;
            this.btnCopyContent.Text = "Sao chép";
            this.btnCopyContent.Click += new System.EventHandler(this.btnCopyContent_Click);
            // 
            // groupBoxNote
            // 
            this.groupBoxNote.Controls.Add(this.lblNote1);
            this.groupBoxNote.Controls.Add(this.lblNote2);
            this.groupBoxNote.Controls.Add(this.lblNote3);
            this.groupBoxNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxNote.Location = new System.Drawing.Point(54, 449);
            this.groupBoxNote.Name = "groupBoxNote";
            this.groupBoxNote.Size = new System.Drawing.Size(443, 112);
            this.groupBoxNote.TabIndex = 13;
            this.groupBoxNote.TabStop = false;
            this.groupBoxNote.Text = "Lưu ý:";
            // 
            // lblNote1
            // 
            this.lblNote1.AutoSize = true;
            this.lblNote1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblNote1.Location = new System.Drawing.Point(10, 20);
            this.lblNote1.Name = "lblNote1";
            this.lblNote1.Size = new System.Drawing.Size(423, 19);
            this.lblNote1.TabIndex = 0;
            this.lblNote1.Text = "1. Vui lòng nhập chính xác nội dung và số tiền yêu cầu từ hệ thống.";
            // 
            // lblNote2
            // 
            this.lblNote2.AutoSize = true;
            this.lblNote2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblNote2.Location = new System.Drawing.Point(10, 45);
            this.lblNote2.Name = "lblNote2";
            this.lblNote2.Size = new System.Drawing.Size(425, 19);
            this.lblNote2.TabIndex = 1;
            this.lblNote2.Text = "2. Hệ thống sẽ tự động cập nhật trạng thái sau khi ngân hàng xử lý.";
            // 
            // lblNote3
            // 
            this.lblNote3.AutoSize = true;
            this.lblNote3.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblNote3.Location = new System.Drawing.Point(10, 75);
            this.lblNote3.Name = "lblNote3";
            this.lblNote3.Size = new System.Drawing.Size(364, 19);
            this.lblNote3.TabIndex = 2;
            this.lblNote3.Text = "3. Nếu quá 10 phút chưa cập nhật, vui lòng liên hệ admin.";
            // 
            // pbQR
            // 
            this.pbQR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbQR.Location = new System.Drawing.Point(195, 73);
            this.pbQR.Name = "pbQR";
            this.pbQR.Size = new System.Drawing.Size(180, 180);
            this.pbQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbQR.TabIndex = 2;
            this.pbQR.TabStop = false;
            // 
            // FrmPaymentQR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 578);
            this.Controls.Add(this.lblCountdownTitle);
            this.Controls.Add(this.lblCountdown);
            this.Controls.Add(this.pbQR);
            this.Controls.Add(this.btnDownloadQR);
            this.Controls.Add(this.lblBank);
            this.Controls.Add(this.lblAccount);
            this.Controls.Add(this.btnCopyAccount);
            this.Controls.Add(this.lblHolder);
            this.Controls.Add(this.btnCopyHolder);
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.btnCopyAmount);
            this.Controls.Add(this.lblContent);
            this.Controls.Add(this.btnCopyContent);
            this.Controls.Add(this.groupBoxNote);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPaymentQR";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thanh toán qua QR";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmPaymentQR_FormClosing);
            this.groupBoxNote.ResumeLayout(false);
            this.groupBoxNote.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbQR)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // === Khai báo các control ===
        private System.Windows.Forms.Label lblCountdownTitle;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.PictureBox pbQR;
        private System.Windows.Forms.Button btnDownloadQR;
        private System.Windows.Forms.Label lblBank;
        private System.Windows.Forms.Label lblAccount;
        private System.Windows.Forms.Button btnCopyAccount;
        private System.Windows.Forms.Label lblHolder;
        private System.Windows.Forms.Button btnCopyHolder;
        private System.Windows.Forms.Label lblAmount;
        private System.Windows.Forms.Button btnCopyAmount;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.Button btnCopyContent;
        private System.Windows.Forms.GroupBox groupBoxNote;
        private System.Windows.Forms.Label lblNote1;
        private System.Windows.Forms.Label lblNote2;
        private System.Windows.Forms.Label lblNote3;
    }
}