using QRCoder;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class FrmPaymentQR : Form
    {
        private Timer timer;
        private int countdownSeconds = 60; 

        // Thuộc tính có thể thiết lập từ ngoài (ví dụ: từ UcCustomers)
        public string BankName { get; set; } = "BIDV";
        public string BankAccount { get; set; } = "8867042148";
        public string AccountHolder { get; set; } = "VU DUC TRONG";
        public decimal Amount { get; set; } = 0;
        public string ReferenceCode { get; set; } = string.Empty;

        public FrmPaymentQR()
        {
            InitializeComponent();
            InitializeTimer();
            UpdateCountdownLabel();
        }

            public void RefreshUI()
        {
            UpdateDisplayTexts();
            LoadQRCode();
        }

        private void UpdateDisplayTexts()
        {
            lblBank.Text = $"Tên ngân hàng: {BankName}";
            lblAccount.Text = $"Số tài khoản: {BankAccount}";
            lblHolder.Text = $"Tên chủ tài khoản: {AccountHolder}";
            lblAmount.Text = $"Số tiền chuyển: {Amount:N0} VND";
            lblContent.Text = $"Nội dung chuyển khoản: {ReferenceCode}";
        }

        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 1000; // 1 giây
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;
            if (countdownSeconds <= 0)
            {
                timer?.Stop();
                this.Close();
                return;
            }
            UpdateCountdownLabel();
        }

        private void UpdateCountdownLabel()
        {
            // Hiển thị dạng: 00:09, 00:08, ...
            lblCountdown.Text = $"00:{countdownSeconds:D2}";
        }

        private void LoadQRCode()
        {
            string url = $"https://vietqr.io/{BankAccount}/{(long)Amount}/{Uri.EscapeDataString(ReferenceCode)}";

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrImage = qrCode.GetGraphic(10);
                pbQR.Image = qrImage;
            }
        }

        private void btnDownloadQR_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG Image|*.png";
                saveDialog.FileName = $"QR_{ReferenceCode}.png";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pbQR.Image?.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        MessageBox.Show("Đã lưu QR code thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ⚠️ Lưu ý: Tên hàm phải KHỚP với Designer (btnCopyAccount, không phải btnCopyBankAccount)
        private void btnCopyAccount_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(BankAccount);
            MessageBox.Show("Đã sao chép số tài khoản!", "Sao chép", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCopyHolder_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(AccountHolder);
            MessageBox.Show("Đã sao chép tên tài khoản!", "Sao chép", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCopyAmount_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Amount.ToString("N0"));
            MessageBox.Show("Đã sao chép số tiền!", "Sao chép", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCopyContent_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ReferenceCode);
            MessageBox.Show("Đã sao chép nội dung!", "Sao chép", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FrmPaymentQR_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer?.Stop();
            timer?.Dispose();
        }

        private void lblAmount_Click(object sender, EventArgs e)
        {

        }
    }
}