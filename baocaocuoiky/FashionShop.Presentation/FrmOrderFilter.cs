// File: FrmOrderFilter.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class FrmOrderFilter : Form
    {
        public DateTime? FromDate { get; private set; }
        public DateTime? ToDate { get; private set; }
        public string Status { get; private set; } = "";
        public string Keyword { get; private set; } = "";

        public FrmOrderFilter()
        {
            this.Text = "Lọc đơn hàng";
            this.Size = new Size(400, 320);
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

            var lblStatus = new Label { Text = "Trạng thái:", Location = new Point(20, 100), AutoSize = true, ForeColor = Color.White };
            var cboStatus = new ComboBox
            {
                Location = new Point(100, 100),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                ForeColor = Color.Black
            };
            cboStatus.Items.AddRange(new object[] { "(Tất cả)", "Chờ xử lý", "Đang giao", "Hoàn thành", "Hủy" });
            cboStatus.SelectedIndex = 0;

            var lblKeyword = new Label { Text = "Tìm kiếm:", Location = new Point(20, 140), AutoSize = true, ForeColor = Color.White };
            var txtKeyword = new TextBox { Location = new Point(100, 140), Width = 250 };

            var btnApply = new Button
            {
                Text = "Áp dụng",
                Location = new Point(200, 240),
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
                Location = new Point(300, 240),
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 65);

            btnApply.Click += (s, e) =>
            {
                FromDate = dtpFrom.Value.Date;
                ToDate = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
                Status = cboStatus.SelectedIndex > 0 ? cboStatus.SelectedItem.ToString() : "";
                Keyword = txtKeyword.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, lblStatus, cboStatus, lblKeyword, txtKeyword, btnApply, btnCancel });
        }
    }
}