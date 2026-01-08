using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class UcSettings : UserControl
    {
        private static readonly Color C_BG = Color.FromArgb(20, 20, 20);
        private static readonly Color C_PANEL = Color.FromArgb(30, 30, 30);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_TEXT = Color.White;
        private static readonly Color C_MUTED = Color.FromArgb(160, 160, 160);
        private static readonly Color C_ACCENT = Color.FromArgb(0, 120, 215);

        private TableLayoutPanel root;
        private Button btnSave;

        // store fields
        private TextBox txtStoreName, txtHotline, txtAddress, txtEmail, txtWebsite;

        // display fields
        private ComboBox cboLanguage, cboCurrency;

        // logo
        private PictureBox picLogo;
        private Button btnPickLogo;

        public UcSettings()
        {
            InitializeComponent();
            BuildUI();
            Dock = DockStyle.Fill;
            BackColor = C_BG;
        }

        private void BuildUI()
        {
            SuspendLayout();
            Controls.Clear();

            root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(18),
                ColumnCount = 2,
                RowCount = 2
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // header
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // content
            Controls.Add(root);

            // ===== TOP HEADER (title + save button) =====
            var pnHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var lbTitle = new Label
            {
                Text = "Cài đặt hệ thống",
                AutoSize = true,
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Location = new Point(0, 10)
            };

            btnSave = MakePrimaryButton("Lưu thay đổi", 140, 34);
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnHeader.Controls.Add(lbTitle);
            pnHeader.Controls.Add(btnSave);

            pnHeader.Resize += (s, e) =>
            {
                btnSave.Location = new Point(pnHeader.ClientSize.Width - btnSave.Width, 10);
            };

            root.Controls.Add(pnHeader, 0, 0);
            root.SetColumnSpan(pnHeader, 2);

            // ===== CONTENT LAYOUT (left stack | right logo) =====
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(content, 0, 1);
            root.SetColumnSpan(content, 2);

            // LEFT stack (2 cards) - DÙNG TABLELAYOUT để khỏi lệch + khỏi scroll ngang
            var leftStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            leftStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 280)); // Thông tin cửa hàng
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // Thiết lập hiển thị

            content.Controls.Add(leftStack, 0, 0);

            // RIGHT logo card
            var logoCard = MakeCard();
            content.Controls.Add(logoCard, 1, 0);

            // ===== CARD: Store info =====
            var storeCard = MakeCard();
            var displayCard = MakeCard();   // <<< THÊM DÒNG NÀY

            storeCard.Dock = DockStyle.Fill;
            displayCard.Dock = DockStyle.Fill;

            leftStack.Controls.Add(storeCard, 0, 0);
            leftStack.Controls.Add(displayCard, 0, 1);
            displayCard.Controls.Add(MakeCardHeader("\uE713", "Thiết lập hiển thị")); // settings icon
            displayCard.Controls.Add(MakeDivider());


            var storeGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0, 12, 0, 0)
            };
            storeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            storeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            storeGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            storeGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            storeGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            storeCard.Controls.Add(storeGrid);
            storeGrid.BringToFront();

            txtStoreName = MakeTextBox();
            txtHotline = MakeTextBox();
            txtAddress = MakeTextBox();
            txtEmail = MakeTextBox();
            txtWebsite = MakeTextBox();

            var fStore = MakeField("Tên cửa hàng", txtStoreName);
            var fHot = MakeField("Số điện thoại hotline", txtHotline);
            var fAddr = MakeField("Địa chỉ kinh doanh", txtAddress);
            var fEmail = MakeField("Email liên hệ", txtEmail);
            var fWeb = MakeField("Website", txtWebsite);

            storeGrid.Controls.Add(fStore, 0, 0);
            storeGrid.Controls.Add(fHot, 1, 0);

            storeGrid.Controls.Add(fAddr, 0, 1);
            storeGrid.SetColumnSpan(fAddr, 2);

            storeGrid.Controls.Add(fEmail, 0, 2);
            storeGrid.Controls.Add(fWeb, 1, 2);

            // ===== CARD: Display settings =====

            var displayGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0, 12, 0, 0)
            };
            displayGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            displayGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            displayGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            displayCard.Controls.Add(displayGrid);

            cboLanguage = MakeCombo();
            cboCurrency = MakeCombo();

            // sample data
            cboLanguage.Items.AddRange(new object[] { "Tiếng Việt", "English" });
            cboCurrency.Items.AddRange(new object[] { "VND (₫)", "USD ($)" });
            cboLanguage.SelectedIndex = 0;
            cboCurrency.SelectedIndex = 0;

            displayGrid.Controls.Add(MakeField("Ngôn ngữ", cboLanguage), 0, 0);
            displayGrid.Controls.Add(MakeField("Đơn vị tiền tệ", cboCurrency), 1, 0);

            // ===== CARD RIGHT: Logo =====
            logoCard.Controls.Add(MakeCardHeader("\uE7B8", "Logo & Nhận diện"));
            logoCard.Controls.Add(MakeDivider());

            var pnLogo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 260,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 14, 0, 0)
            };
            logoCard.Controls.Add(pnLogo);

            picLogo = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 170,
                BackColor = Color.FromArgb(25, 25, 25),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            picLogo.Paint += DrawDashedBorder;
            pnLogo.Controls.Add(picLogo);

            var lbHint = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f),
                Text = "Định dạng hỗ trợ: JPG, PNG.\nKích thước tối đa: 2MB.",
                Padding = new Padding(0, 8, 0, 0)
            };
            pnLogo.Controls.Add(lbHint);
            lbHint.BringToFront();

            btnPickLogo = MakeOutlineButton("Thay đổi Logo", 140, 36);
            btnPickLogo.Dock = DockStyle.Top;
            btnPickLogo.Margin = new Padding(0, 10, 0, 0);
            pnLogo.Controls.Add(btnPickLogo);
            btnPickLogo.BringToFront();

            // set card order (do Dock=Top nên add ngược sẽ bị đảo)
            storeGrid.BringToFront();
            displayGrid.BringToFront();
            pnLogo.BringToFront();

            ResumeLayout();
        }

        // ===== helpers =====

        private Panel MakeCard()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(18),
                Margin = new Padding(0, 0, 14, 14)
            };
            p.Paint += BorderPaint;
            return p;
        }


        private Control MakeCardHeader(string glyph, string title)
        {
            var fl = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 28,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            var ico = new Label
            {
                AutoSize = true,
                Text = glyph,
                Font = new Font("Segoe MDL2 Assets", 16f),
                ForeColor = C_ACCENT,
                Margin = new Padding(0, 2, 8, 0)
            };

            var lb = new Label
            {
                AutoSize = true,
                Text = title,
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(0, 4, 0, 0)
            };

            fl.Controls.Add(ico);
            fl.Controls.Add(lb);
            return fl;
        }

        private Control MakeDivider()
        {
            return new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(55, 55, 55),
                Margin = new Padding(0, 10, 0, 0)
            };
        }

        private Panel MakeField(string labelText, Control input)
        {
            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 12, 0)
            };

            var lb = new Label
            {
                Text = labelText,
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(0, 0, 0, 6)
            };

            input.Dock = DockStyle.Top;
            input.Height = 34;

            host.Controls.Add(input);
            host.Controls.Add(lb);

            return host;
        }

        private TextBox MakeTextBox()
        {
            return new TextBox
            {
                Multiline = true,          // <<< bắt buộc để ăn Height
                AcceptsReturn = false,
                WordWrap = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(22, 22, 22),
                ForeColor = C_TEXT,
                Height = 34
            };
        }


        private ComboBox MakeCombo()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(22, 22, 22),
                ForeColor = C_TEXT,
                FlatStyle = FlatStyle.Flat,
                IntegralHeight = false,
                Height = 34
            };
        }


        private Button MakePrimaryButton(string text, int w, int h)
        {
            var b = new Button
            {
                Text = text,
                Width = w,
                Height = h,
                FlatStyle = FlatStyle.Flat,
                BackColor = C_ACCENT,
                ForeColor = Color.White
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Button MakeOutlineButton(string text, int w, int h)
        {
            var b = new Button
            {
                Text = text,
                Width = w,
                Height = h,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = C_TEXT
            };
            b.FlatAppearance.BorderColor = C_BORDER;
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void BorderPaint(object sender, PaintEventArgs e)
        {
            if (!(sender is Control c)) return;
            using (var pen = new Pen(C_BORDER))
                e.Graphics.DrawRectangle(pen, 0, 0, c.Width - 1, c.Height - 1);
        }

        private void DrawDashedBorder(object sender, PaintEventArgs e)
        {
            if (!(sender is PictureBox pb)) return;

            var r = pb.ClientRectangle;
            r.Inflate(-8, -8);

            using (var pen = new Pen(Color.FromArgb(90, 90, 90), 1))
            {
                pen.DashStyle = DashStyle.Dash;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, r);
            }
        }
    }
}
