using FashionShop.Business;
using FashionShop.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class UcCustomers : UserControl, IReloadable
    {
        // ===== THEME =====
        private static readonly Color C_BG = Color.FromArgb(20, 20, 20);
        private static readonly Color C_PANEL = Color.FromArgb(30, 30, 30);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_TEXT = Color.Gainsboro;
        private static readonly Color C_MUTED = Color.FromArgb(150, 150, 150);
        private static readonly Color C_ACCENT = Color.FromArgb(0, 120, 215);

        private const string PLACEHOLDER = "Tìm tên, số điện thoại...";

        // ===== ROOT LAYOUT =====
        private TableLayoutPanel tlpRoot;

        // ===== LEFT =====
        private Panel pnLeft;
        private TextBox txtSearch;
        private Button btnClearSearch;
        private ListBox lbCustomers;
        private Label lbLeftFooter;

        // ===== RIGHT TOP BAR =====
        private Panel pnRight;
        private TableLayoutPanel tlpTopbar;
        private Label lbHeaderTitle;
        private Label lbCodeBadge;
        private Button btnDelete;

        // ===== RIGHT BODY =====
        private TableLayoutPanel tlpRightBody;
        private TableLayoutPanel tlpTopCards;

        private Panel pnProfile;
        private Panel pnInfo;
        private Panel pnHistory;

        private PictureBox picAvatar;
        private Panel pnRankBox;
        private Label lbRankCaption;
        private Label lbRankValue;

        // basic info value labels (caption:value)
        private Label vName, vPhone, vEmail, vJoinDate;

        // history grid
        private DataGridView dgvHistory;

        // ===== DATA =====
        private List<Customer> _all = new List<Customer>();
        private List<Customer> _filtered = new List<Customer>();

        private readonly CustomerService _customerService = new CustomerService(new FashionShopDb());

        public UcCustomers()
        {
            BackColor = C_BG;
            Dock = DockStyle.Fill;

            BuildUI();
            Wire();

            LoadDataFromDb();
            ApplyFilter();
            SelectFirst();
        }

        public void ReloadData()
        {
            LoadDataFromDb();
            ApplyFilter();
            SelectFirst();
        }

        // ===================== UI =====================
        private void BuildUI()
        {
            SuspendLayout();
            Controls.Clear();

            // ROOT
            tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(12, 10, 12, 10)
            };
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(tlpRoot);

            BuildLeft();
            BuildRight();

            ResumeLayout(true);
        }

        private void BuildLeft()
        {
            pnLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 12, 0)
            };
            pnLeft.Paint += DrawCardBorder;
            tlpRoot.Controls.Add(pnLeft, 0, 0);

            var tlpLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            pnLeft.Controls.Add(tlpLeft);

            // Search row
            var pnSearchBox = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(10, 9, 8, 8)
            };
            tlpLeft.Controls.Add(pnSearchBox, 0, 0);

            var tlpSearchInner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlpSearchInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpSearchInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            pnSearchBox.Controls.Add(tlpSearchInner);

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Text = PLACEHOLDER,
                Margin = new Padding(0, 2, 0, 0)
            };
            tlpSearchInner.Controls.Add(txtSearch, 0, 0);

            btnClearSearch = new Button
            {
                Text = "✕",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(80, 80, 80),
                TabStop = false,
                Margin = Padding.Empty
            };
            btnClearSearch.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnClearSearch.FlatAppearance.BorderSize = 1;
            tlpSearchInner.Controls.Add(btnClearSearch, 1, 0);

            // List
            lbCustomers = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 74,
                BackColor = C_PANEL,
                ForeColor = C_TEXT,
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty
            };
            tlpLeft.Controls.Add(lbCustomers, 0, 1);

            // Footer
            lbLeftFooter = new Label
            {
                Dock = DockStyle.Fill,
                Text = "0 records displayed",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };
            tlpLeft.Controls.Add(lbLeftFooter, 0, 2);
        }

        private void BuildRight()
        {
            pnRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            tlpRoot.Controls.Add(pnRight, 1, 0);

            // Right layout: Topbar + Body
            var tlpRightRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlpRightRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tlpRightRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            pnRight.Controls.Add(tlpRightRoot);

            BuildTopBar(tlpRightRoot);
            BuildRightBody(tlpRightRoot);
        }

        private void BuildTopBar(TableLayoutPanel host)
        {
            tlpTopbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = new Padding(0, 2, 0, 0)
            };
            tlpTopbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpTopbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            host.Controls.Add(tlpTopbar, 0, 0);

            // Left: title + badge
            var pnTopLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = new Padding(0, 10, 0, 0)
            };
            tlpTopbar.Controls.Add(pnTopLeft, 0, 0);

            lbHeaderTitle = new Label
            {
                AutoSize = true,
                Text = "Hồ sơ khách hàng",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            pnTopLeft.Controls.Add(lbHeaderTitle);

            lbCodeBadge = new Label
            {
                AutoSize = false,
                Size = new Size(52, 24),
                Text = "---",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(20, 70, 110),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(0, 3, 0, 0)
            };
            pnTopLeft.Controls.Add(lbCodeBadge);

            // Right: buttons
            var pnTopRight = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = new Padding(0, 8, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            tlpTopbar.Controls.Add(pnTopRight, 1, 0);

            // ✅ THÊM NÚT THANH TOÁN
            Button btnPayment = MakeTopButton("💳  Thanh toán", Color.FromArgb(45, 45, 45), C_TEXT, 130);
            btnPayment.Click += BtnPayment_Click;
            pnTopRight.Controls.Add(btnPayment);

            btnDelete = MakeTopButton("🗑  Xóa hồ sơ", Color.FromArgb(45, 45, 45), C_TEXT, 130);

            pnTopRight.Controls.Add(btnDelete);
        }

        private void BuildRightBody(TableLayoutPanel host)
        {
            tlpRightBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = new Padding(0, 10, 0, 0)
            };
            tlpRightBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 270));
            tlpRightBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            host.Controls.Add(tlpRightBody, 0, 1);

            // Top cards: profile + info
            tlpTopCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlpTopCards.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            tlpTopCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpTopCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tlpRightBody.Controls.Add(tlpTopCards, 0, 0);

            BuildProfileCard();

            // ✅ FIX: tạo pnInfo trước khi BuildBasicInfo
            pnInfo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(16, 12, 16, 16),
                Margin = Padding.Empty
            };
            pnInfo.Paint += DrawCardBorder;
            tlpTopCards.Controls.Add(pnInfo, 1, 0);

            BuildBasicInfo(pnInfo);

            // History card
            pnHistory = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(16, 12, 16, 16),
                Margin = new Padding(0, 12, 0, 0)
            };
            pnHistory.Paint += DrawCardBorder;
            tlpRightBody.Controls.Add(pnHistory, 0, 1);

            BuildHistory(pnHistory);
        }

        private void BuildProfileCard()
        {
            pnProfile = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 12, 0)
            };
            pnProfile.Paint += DrawCardBorder;
            tlpTopCards.Controls.Add(pnProfile, 0, 0);

            var tlpProfile = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlpProfile.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tlpProfile.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
            pnProfile.Controls.Add(tlpProfile);

            // Avatar
            var pnAvatarHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };
            tlpProfile.Controls.Add(pnAvatarHost, 0, 0);

            picAvatar = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = Padding.Empty
            };
            pnAvatarHost.Controls.Add(picAvatar);

            // Rank box
            pnRankBox = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10),
                Margin = Padding.Empty
            };
            pnRankBox.Paint += DrawCardBorder;
            tlpProfile.Controls.Add(pnRankBox, 0, 1);

            var tlpRank = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlpRank.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            tlpRank.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            pnRankBox.Controls.Add(tlpRank);

            lbRankCaption = new Label
            {
                Dock = DockStyle.Fill,
                Text = "XẾP HẠNG",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty
            };
            tlpRank.Controls.Add(lbRankCaption, 0, 0);

            lbRankValue = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Gold Member",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty
            };
            tlpRank.Controls.Add(lbRankValue, 0, 1);
        }

        // ===================== BASIC INFO (Caption: Value) =====================
        private void BuildBasicInfo(Panel host)
        {
            if (host == null) return;

            host.Controls.Clear();

            var lbHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = "THÔNG TIN CƠ BẢN",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(0, 10, 0, 0),
                Margin = Padding.Empty,
                AutoSize = true
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 4; i++)
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            // add rows...
            tlp.Controls.Add(MakeCap("Họ và tên:"), 0, 0); vName = MakeVal(); tlp.Controls.Add(vName, 1, 0);
            tlp.Controls.Add(MakeCap("Số điện thoại:"), 0, 1); vPhone = MakeVal(); tlp.Controls.Add(vPhone, 1, 1);
            tlp.Controls.Add(MakeCap("Email:"), 0, 2); vEmail = MakeVal(); tlp.Controls.Add(vEmail, 1, 2);
            tlp.Controls.Add(MakeCap("Ngày gia nhập:"), 0, 3); vJoinDate = MakeVal(); tlp.Controls.Add(vJoinDate, 1, 3);

            // ✅ QUAN TRỌNG: tlp add trước, header add sau
            host.Controls.Add(tlp);
            host.Controls.Add(lbHeader);
        }


        private Label MakeCap(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };
        }

        private Label MakeVal()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Margin = Padding.Empty
            };
        }

        // ===================== HISTORY =====================
        private void BuildHistory(Panel host)
        {
            host.Controls.Clear();

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            host.Controls.Add(tlp);

            var lb = new Label
            {
                Dock = DockStyle.Fill,
                Text = "LỊCH SỬ GIAO DỊCH",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };
            tlp.Controls.Add(lb, 0, 0);

            var pnGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            tlp.Controls.Add(pnGrid, 0, 1);

            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = C_PANEL,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                GridColor = Color.FromArgb(45, 45, 45)
            };
            pnGrid.Controls.Add(dgvHistory);

            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 32);
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgvHistory.ColumnHeadersHeight = 40;

            dgvHistory.DefaultCellStyle.BackColor = C_PANEL;
            dgvHistory.DefaultCellStyle.ForeColor = C_TEXT;
            dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgvHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dgvHistory.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvHistory.RowTemplate.Height = 40;

            dgvHistory.Columns.Clear();
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã Đơn", DataPropertyName = "OrderCode", Width = 120 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày mua", DataPropertyName = "TimeText", Width = 170 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nội dung", DataPropertyName = "Content", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tổng tiền",
                DataPropertyName = "TotalText",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvHistory.CellPainting -= DgvHistory_CellPainting;
            dgvHistory.CellPainting += DgvHistory_CellPainting;
        }

        private void DrawCardBorder(object sender, PaintEventArgs e)
        {
            var p = sender as Control;
            if (p == null) return;
            using (var pen = new Pen(C_BORDER))
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        private Button MakeTopButton(string text, Color back, Color fore, int width)
        {
            var b = new Button
            {
                Text = text,
                Width = width,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 55);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        // ===================== EVENTS =====================
        private void Wire()
        {
            lbCustomers.DrawItem += LbCustomers_DrawItem;
            lbCustomers.SelectedIndexChanged += (s, e) => ShowSelectedCustomer();

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == PLACEHOLDER)
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };

            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = PLACEHOLDER;
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            txtSearch.TextChanged += (s, e) => ApplyFilter();

            btnClearSearch.Click += (s, e) =>
            {
                txtSearch.Text = "";
                txtSearch.Focus();
            };


            btnDelete.Click += (s, e) =>
            {
                var c = GetSelected();
                if (c == null) return;
                var ok = MessageBox.Show("Xóa khách hàng " + c.Name + " ?", "Xác nhận", MessageBoxButtons.YesNo);
                if (ok != DialogResult.Yes) return;

                _all.Remove(c);
                ApplyFilter();
                SelectFirst();
            };
        }

        private void LbCustomers_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= lbCustomers.Items.Count) return;

            var c = (Customer)lbCustomers.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var g = e.Graphics;
            var bounds = e.Bounds;

            using (var br = new SolidBrush(selected ? Color.FromArgb(12, 70, 110) : C_PANEL))
                g.FillRectangle(br, bounds);

            using (var pen = new Pen(Color.FromArgb(45, 45, 45)))
                g.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

            var av = new Rectangle(bounds.Left + 10, bounds.Top + 14, 44, 44);
            if (c.Avatar != null) g.DrawImage(c.Avatar, av);

            var nameRect = new Rectangle(av.Right + 12, bounds.Top + 14, bounds.Width - 120, 22);
            TextRenderer.DrawText(g, c.Name, new Font("Segoe UI", 9.5f, FontStyle.Bold),
                nameRect, Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            var phoneRect = new Rectangle(av.Right + 12, bounds.Top + 38, bounds.Width - 120, 20);
            TextRenderer.DrawText(g, c.Phone, new Font("Segoe UI", 9f),
                phoneRect, Color.FromArgb(210, 210, 210), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            if (c.IsVip)
            {
                var badge = new Rectangle(bounds.Right - 44, bounds.Top + 18, 34, 18);
                using (var br = new SolidBrush(Color.FromArgb(60, 60, 60)))
                    g.FillRectangle(br, badge);
                using (var pen = new Pen(Color.Gold))
                    g.DrawRectangle(pen, badge);

                TextRenderer.DrawText(g, "VIP", new Font("Segoe UI", 8f, FontStyle.Bold),
                    badge, Color.Gold, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DgvHistory_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 0)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string text = Convert.ToString(e.FormattedValue);
                var r = e.CellBounds; r.Inflate(-8, -6);

                TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    r, C_ACCENT, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                return;
            }

            if (e.ColumnIndex == 3)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string text = Convert.ToString(e.FormattedValue);
                var r = e.CellBounds; r.Inflate(-8, -6);

                TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 9.8f, FontStyle.Bold),
                    r, Color.White, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        // ===================== DATA + BINDING =====================
        private void LoadDataFromDb()
        {
            try
            {
                var customers = _customerService.GetAll();

                _all = customers.Select(c => new Customer(
                    c.CustomerCode,
                    c.FullName,
                    c.Phone,
                    c.Email,
                    c.JoinDate,
                    c.IsVip,
                    c.Rank
                )).ToList();

                // Tạo avatar cho mỗi khách hàng
                for (int i = 0; i < _all.Count; i++)
                    _all[i].Avatar = MakeAvatar(_all[i].Name);

                // Thêm lịch sử giao dịch (nếu có)
                foreach (var customer in _all)
                {
                    var orders = _customerService.GetOrdersByCustomerId(customer.Code);
                    customer.History.Clear();
                    foreach (var order in orders)
                    {
                        customer.History.Add(new Purchase(
                            order.OrderCode,
                            order.OrderDate,
                            GetOrderContent(order.OrderId), // Hàm này bạn sẽ viết
                            order.TotalAmount
                        ));
                    }
                }

                ApplyFilter();
                SelectFirst();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải khách hàng: " + ex.Message);
                _all = new List<Customer>();
            }
        }

        private string GetOrderContent(int orderId)
        {
            try
            {
                using (var db = new FashionShopDb())
                {
                    var products = (from oi in db.OrderItems
                                    join p in db.Products on oi.ProductId equals p.ProductId
                                    where oi.OrderId == orderId
                                    select p.ProductName)
                                   .Take(2) // Lấy tối đa 2 sản phẩm
                                   .ToList();

                    if (products.Count == 0) return "Không có sản phẩm";
                    return string.Join(", ", products);
                }
            }
            catch
            {
                return "Lỗi tải nội dung";
            }
        }

        private void ApplyFilter()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLower();
            if (q == PLACEHOLDER.ToLower()) q = "";

            _filtered = _all
                .Where(x => string.IsNullOrEmpty(q)
                    || (x.Name != null && x.Name.ToLower().Contains(q))
                    || (x.Phone != null && x.Phone.ToLower().Contains(q)))
                .ToList();

            lbCustomers.BeginUpdate();
            lbCustomers.Items.Clear();
            for (int i = 0; i < _filtered.Count; i++)
                lbCustomers.Items.Add(_filtered[i]);
            lbCustomers.EndUpdate();

            lbLeftFooter.Text = _filtered.Count + " records displayed";
        }

        private void SelectFirst()
        {
            if (lbCustomers.Items.Count > 0) lbCustomers.SelectedIndex = 0;
            else ClearRight();
        }

        private Customer GetSelected()
        {
            return lbCustomers.SelectedItem as Customer;
        }

        private void ShowSelectedCustomer()
        {
            var c = GetSelected();
            if (c == null) { ClearRight(); return; }

            lbCodeBadge.Text = c.Code;
            picAvatar.Image = c.Avatar;

            lbRankValue.Text = c.RankText;
            lbRankValue.ForeColor = c.RankText.Contains("Vàng") ? Color.Gold
                : (c.RankText.Contains("Bạc") ? Color.Silver : C_TEXT);

            // Cập nhật lại History từ DB nếu cần (nếu bạn muốn luôn mới)
            c.History.Clear();
            var orders = _customerService.GetOrdersByCustomerId(c.Code);
            foreach (var order in orders)
            {
                c.History.Add(new Purchase(
                    order.OrderCode,
                    order.OrderDate,
                    GetOrderContent(order.OrderId),
                    order.TotalAmount
                ));
            }

            // Hiển thị thông tin
            if (vName != null) vName.Text = c.Name;
            if (vPhone != null) vPhone.Text = c.Phone;
            if (vEmail != null) vEmail.Text = c.Email;
            if (vJoinDate != null) vJoinDate.Text = c.JoinDate.ToString("dd/MM/yyyy");

            dgvHistory.DataSource = null;
            dgvHistory.DataSource = c.History.Select(h => new PurchaseRow(h)).ToList();
        }

        private void ClearRight()
        {
            lbCodeBadge.Text = "---";
            picAvatar.Image = null;
            lbRankValue.Text = "-";

            if (vName != null) vName.Text = "";
            if (vPhone != null) vPhone.Text = "";
            if (vEmail != null) vEmail.Text = "";
            if (vJoinDate != null) vJoinDate.Text = "";

            if (dgvHistory != null) dgvHistory.DataSource = null;
        }

        // ===================== AVATAR (demo) =====================
        private Image MakeAvatar(string name)
        {
            string initials = GetInitials(name);
            var bmp = new Bitmap(44, 44);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                int hash = (name ?? "").GetHashCode();
                int r = 80 + Math.Abs(hash % 120);
                int gr = 80 + Math.Abs((hash / 3) % 120);
                int b = 80 + Math.Abs((hash / 7) % 120);
                var bg = Color.FromArgb(r, gr, b);

                using (var br = new SolidBrush(bg))
                    g.FillEllipse(br, 0, 0, 44, 44);

                using (var f = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (var brText = new SolidBrush(Color.White))
                {
                    var sz = g.MeasureString(initials, f);
                    g.DrawString(initials, f, brText, (44 - sz.Width) / 2f, (44 - sz.Height) / 2f + 1);
                }
            }
            return bmp;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "U";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        // ===================== MODELS =====================
        private sealed class Customer
        {
            public string Code;
            public string Name;
            public string Phone;
            public string Email;
            public DateTime JoinDate;
            public bool IsVip;
            public string RankText;
            public Image Avatar;
            public List<Purchase> History = new List<Purchase>();

            public Customer(string code, string name, string phone, string email, DateTime join, bool vip, string rank)
            {
                Code = code; Name = name; Phone = phone; Email = email; JoinDate = join; IsVip = vip; RankText = rank;
            }

            public override string ToString() { return Name; }
        }

        private sealed class Purchase
        {
            public string OrderCode;
            public DateTime Time;
            public string Content;
            public decimal Total;

            public Purchase(string code, DateTime time, string content, decimal total)
            {
                OrderCode = code; Time = time; Content = content; Total = total;
            }
        }

        private sealed class PurchaseRow
        {
            public string OrderCode { get; private set; }
            public string TimeText { get; private set; }
            public string Content { get; private set; }
            public string TotalText { get; private set; }

            public PurchaseRow(Purchase p)
            {
                OrderCode = p.OrderCode;
                TimeText = p.Time.ToString("dd/MM/yyyy HH:mm");
                Content = p.Content;

                var nfi = (NumberFormatInfo)CultureInfo.GetCultureInfo("vi-VN").NumberFormat.Clone();
                nfi.NumberGroupSeparator = ".";
                TotalText = p.Total.ToString("#,0", nfi) + "đ";
            }
        }

        // ✅ THÊM PHƯƠNG THỨC XỬ LÝ SỰ KIỆN THANH TOÁN
        private void BtnPayment_Click(object sender, EventArgs e)
        {
            var c = GetSelected();
            if (c == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng trước!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Lấy lại danh sách đơn hàng mới nhất từ DB cho khách hàng này
                var orders = _customerService.GetOrdersByCustomerId(c.Code);

                if (orders == null || !orders.Any())
                {
                    MessageBox.Show("Khách hàng này chưa có đơn hàng nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Tính tổng tiền từ các đơn hàng
                decimal totalAmount = orders.Sum(o => o.TotalAmount);

                if (totalAmount <= 0)
                {
                    MessageBox.Show("Không có đơn hàng hợp lệ để thanh toán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Sinh chuỗi ngẫu nhiên 8 ký tự (C# 7.3 compatible)
                string guid = Guid.NewGuid().ToString("N"); // 32 ký tự không dấu gạch
                string referenceCode = guid.Substring(0, 8).ToUpper();

                var frm = new FrmPaymentQR
                {
                    Amount = totalAmount,
                    ReferenceCode = referenceCode,

                    BankAccount = "8867042148",
                    AccountHolder = "VU DUC TRONG",
                    BankName = "BIDV"
                };
                frm.RefreshUI();
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy dữ liệu đơn hàng: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}