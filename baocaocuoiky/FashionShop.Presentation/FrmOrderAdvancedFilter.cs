// File: FrmOrderAdvancedFilter.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace FashionShop.Presentation
{
    public partial class FrmOrderAdvancedFilter : Form
    {
        public DateTime? FromDate { get; private set; }
        public DateTime? ToDate { get; private set; }
        public string CustomerName { get; private set; } = "";
        public decimal? MinTotal { get; private set; }
        public decimal? MaxTotal { get; private set; }
        public string Status { get; private set; } = "";

        private TextBox txtMinTotal;
        private TextBox txtMaxTotal;

        public FrmOrderAdvancedFilter()
        {
            this.Text = "Lọc nâng cao đơn hàng";
            this.Size = new Size(420, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            var lblFrom = new Label { Text = "Từ ngày:", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.White };
            var dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(100, 20), Width = 120 };

            var lblTo = new Label { Text = "Đến ngày:", Location = new Point(20, 60), AutoSize = true, ForeColor = Color.White };
            var dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(100, 60), Width = 120 };

            var lblCustomer = new Label { Text = "Khách hàng:", Location = new Point(20, 100), AutoSize = true, ForeColor = Color.White };
            var cboCustomer = new ComboBox
            {
                Location = new Point(100, 100),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDown,
                ForeColor = Color.Black
            };
            cboCustomer.Items.Add("(Tất cả)");
            cboCustomer.SelectedIndex = 0;

            var lblMinTotal = new Label { Text = "Tổng tiền từ:", Location = new Point(20, 140), AutoSize = true, ForeColor = Color.White };
            txtMinTotal = new TextBox { Location = new Point(100, 140), Width = 100 };

            var lblMaxTotal = new Label { Text = "Đến:", Location = new Point(220, 140), AutoSize = true, ForeColor = Color.White };
            txtMaxTotal = new TextBox { Location = new Point(250, 140), Width = 100 };

            var lblStatus = new Label { Text = "Trạng thái:", Location = new Point(20, 180), AutoSize = true, ForeColor = Color.White };
            var cboStatus = new ComboBox
            {
                Location = new Point(100, 180),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                ForeColor = Color.Black
            };
            cboStatus.Items.AddRange(new object[] { "(Tất cả)", "Chờ xử lý", "Đang giao", "Hoàn thành", "Hủy" });
            cboStatus.SelectedIndex = 0;

            var btnApply = new Button
            {
                Text = "Áp dụng",
                Location = new Point(200, 280),
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(300, 280),
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 65);

            // ✅ RÀNG BUỘC CHỈ NHẬP SỐ
            txtMinTotal.TextChanged += (s, e) => RestrictToNumber(txtMinTotal);
            txtMaxTotal.TextChanged += (s, e) => RestrictToNumber(txtMaxTotal);

            // Gắn sự kiện
            btnApply.Click += (s, e) =>
            {
                // Reset giá trị trước
                MinTotal = null;
                MaxTotal = null;

                // Xử lý ngày
                FromDate = dtpFrom.Value.Date;
                ToDate = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

                // Xử lý khách hàng
                CustomerName = cboCustomer.SelectedItem?.ToString() ?? "";

                // ✅ XỬ LÝ TỔNG TIỀN AN TOÀN
                if (!string.IsNullOrWhiteSpace(txtMinTotal.Text))
                {
                    if (decimal.TryParse(txtMinTotal.Text, out decimal min))
                        MinTotal = min;
                    else
                    {
                        MessageBox.Show("Giá trị 'Tổng tiền từ' không hợp lệ.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtMaxTotal.Text))
                {
                    if (decimal.TryParse(txtMaxTotal.Text, out decimal max))
                        MaxTotal = max;
                    else
                    {
                        MessageBox.Show("Giá trị 'Đến' không hợp lệ.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Xử lý trạng thái
                Status = cboStatus.SelectedIndex > 0 ? cboStatus.SelectedItem.ToString() : "";

                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel.Click += (s, e) => this.Close();

            // Thêm vào form
            this.Controls.AddRange(new Control[] {
                lblFrom, dtpFrom, lblTo, dtpTo, lblCustomer, cboCustomer,
                lblMinTotal, txtMinTotal, lblMaxTotal, txtMaxTotal,
                lblStatus, cboStatus, btnApply, btnCancel
            });
        }

        // ✅ HÀM GIÚP CHỈ NHẬP SỐ VÀ DẤU CHẤM/PHẨY
        private void RestrictToNumber(TextBox tb)
        {
            string text = tb.Text;
            if (string.IsNullOrEmpty(text)) return;

            // Cho phép: chữ số, dấu chấm (.), dấu phẩy (,)
            string valid = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            // Nếu có thay đổi → cập nhật lại
            if (valid != text)
            {
                int pos = tb.SelectionStart;
                tb.Text = valid;
                tb.SelectionStart = Math.Min(pos, tb.Text.Length);
            }
        }
    }
}