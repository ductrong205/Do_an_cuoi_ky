using FashionShop.Business;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FashionShop.Presentation
{
    public partial class UcDashboard : UserControl, IReloadable
    {
        // ===== THEME (gần giống ảnh) =====
        private static readonly Color C_BG = Color.FromArgb(24, 24, 24);
        private static readonly Color C_PANEL = Color.FromArgb(32, 32, 32);
        private static readonly Color C_PANEL2 = Color.FromArgb(36, 36, 36);
        private static readonly Color C_BORDER = Color.FromArgb(65, 65, 65);
        private static readonly Color C_TEXT = Color.Gainsboro;
        private static readonly Color C_MUTED = Color.FromArgb(140, 140, 140);

        // accent tím giống ảnh
        private static readonly Color C_ACCENT = Color.FromArgb(140, 60, 255);
        private static readonly Color C_BLUEBAR = Color.FromArgb(35, 60, 115);
        private static readonly Color C_GREEN = Color.FromArgb(60, 200, 120);
        private static readonly Color C_RED = Color.FromArgb(255, 80, 80);

        // ===== ROOT =====
        private TableLayoutPanel tlpRoot;

        // Cards
        private TableLayoutPanel tlpCards;
        private Label vRevenue, vOrders, vStock, vNewCus;

        // Middle
        private TableLayoutPanel tlpMid;
        private Panel pnChartWrap, pnTopWrap;
        private Chart chart;
        private Button btnRange7d;

        // Right top products
        private Panel pnTopProductsWrap;
        private ListView lvTop;
        private ImageList imgTop;
        private LinkLabel lnkTopDetail;

        // Bottom orders grid
        private Panel pnOrdersWrap;
        private DataGridView dgv;
        private Button btnFilter;
        private Button btnReload;
        private Label lbBottomInfo;
        private Button pgPrev, pgNext;

        // demo data
        private readonly List<OrderRow> _orders = new List<OrderRow>();

        public UcDashboard()
        {
            BackColor = C_BG;
            Dock = DockStyle.Fill;

            BuildUI();
            Wire();
            LoadDataFromService();
            ReloadData();
        }

        public void ReloadData()
        {
            LoadDataFromService();
        }

        // ===================== UI =====================
        private void BuildUI()
        {
            Controls.Clear();

            tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16, 14, 16, 14)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));    // cards
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));    // chart + top products
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));    // grid
            Controls.Add(tlpRoot);

            // ===== CARDS =====
            tlpCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpRoot.Controls.Add(tlpCards, 0, 0);

            var c1 = MakeKpiCard("DOANH THU NGÀY", out vRevenue, "↑ +15%  so với hôm qua", C_GREEN, "💰");
            var c2 = MakeKpiCard("ĐƠN HÀNG MỚI", out vOrders, "↑ +5%  tăng trưởng", C_GREEN, "🛒");
            var c3 = MakeKpiCard("TỒN KHO", out vStock, "↓ -2%  nhập hàng", C_RED, "📦");
            var c4 = MakeKpiCard("KHÁCH HÀNG MỚI", out vNewCus, "↑ +12%  so với tuần trước", C_GREEN, "👤");

            c1.Margin = new Padding(0, 0, 12, 0);
            c2.Margin = new Padding(0, 0, 12, 0);
            c3.Margin = new Padding(0, 0, 12, 0);
            c4.Margin = new Padding(0);

            tlpCards.Controls.Add(c1, 0, 0);
            tlpCards.Controls.Add(c2, 1, 0);
            tlpCards.Controls.Add(c3, 2, 0);
            tlpCards.Controls.Add(c4, 3, 0);

            // ===== MID (Chart + Top products) =====
            tlpMid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 12, 0, 12)
            };
            tlpMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72f));
            tlpMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            tlpRoot.Controls.Add(tlpMid, 0, 1);

            pnChartWrap = MakeWrap("Biểu đồ Doanh thu");
            pnChartWrap.Margin = new Padding(0, 0, 12, 0);
            tlpMid.Controls.Add(pnChartWrap, 0, 0);

            pnTopProductsWrap = MakeWrap("Sản phẩm bán chạy");
            pnTopProductsWrap.Margin = new Padding(0);
            tlpMid.Controls.Add(pnTopProductsWrap, 1, 0);

            BuildChart(pnChartWrap);
            BuildTopProducts(pnTopProductsWrap);

            // ===== BOTTOM ORDERS =====
            pnOrdersWrap = MakeWrap("Danh sách đơn hàng gần đây");
            pnOrdersWrap.Margin = new Padding(0);
            tlpRoot.Controls.Add(pnOrdersWrap, 0, 2);

            BuildOrdersGrid(pnOrdersWrap);
        }

        private Panel MakeWrap(string title)
        {
            var wrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(14, 12, 14, 12)
            };
            wrap.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawRectangle(pen, 0, 0, wrap.Width - 1, wrap.Height - 1);
            };

            var header = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.Transparent };
            wrap.Controls.Add(header);

            var lb = new Label
            {
                AutoSize = true,
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(0, 5)
            };
            header.Controls.Add(lb);

            return wrap;
        }

        private Panel MakeKpiCard(string title, out Label value, string deltaText, Color deltaColor, string icon)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(14, 12, 14, 12)
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lbT = new Label
            {
                AutoSize = true,
                Text = title,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(0, 0)
            };
            card.Controls.Add(lbT);

            var lbIcon = new Label
            {
                AutoSize = false,
                Size = new Size(28, 28),
                Text = icon,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(card.Width - 34, 0)
            };
            card.Controls.Add(lbIcon);
            card.Resize += (s, e) => lbIcon.Left = card.ClientSize.Width - lbIcon.Width - 6;

            value = new Label
            {
                AutoSize = true,
                Text = "0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                Location = new Point(0, 30)
            };
            card.Controls.Add(value);

            var lbDelta = new Label
            {
                AutoSize = true,
                Text = deltaText,
                ForeColor = deltaColor,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(0, 78)
            };
            card.Controls.Add(lbDelta);

            return card;
        }

        // ===================== CHART =====================
        private void BuildChart(Panel host)
        {
            // header right button "7 ngày qua"
            btnRange7d = new Button
            {
                Text = "7 ngày qua",
                Width = 100,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            btnRange7d.FlatAppearance.BorderColor = C_BORDER;
            btnRange7d.FlatAppearance.BorderSize = 1;
            host.Controls.Add(btnRange7d);
            btnRange7d.BringToFront();

            host.Resize += (sender, e) =>
            {
                btnRange7d.Left = host.ClientSize.Width - btnRange7d.Width - 14;
                btnRange7d.Top = 8;
            };
            btnRange7d.Left = host.ClientSize.Width - btnRange7d.Width - 14;
            btnRange7d.Top = 8;

            chart = new Chart { Dock = DockStyle.Fill, BackColor = C_PANEL };
            host.Controls.Add(chart);
            chart.BringToFront();

            var area = new ChartArea("A");
            area.BackColor = C_PANEL;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(45, 45, 45);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(45, 45, 45);
            area.AxisX.LabelStyle.ForeColor = C_MUTED;
            area.AxisY.LabelStyle.ForeColor = C_MUTED;
            area.AxisX.LineColor = C_BORDER;
            area.AxisY.LineColor = C_BORDER;
            area.AxisX.Interval = 1;
            area.AxisY.Minimum = 0;
            chart.ChartAreas.Add(area);

            chart.Legends.Clear();

            var seriesRevenue = new Series("Revenue")
            {
                ChartType = SeriesChartType.Column,
                ChartArea = "A",
            };
            seriesRevenue["PointWidth"] = "0.55";
            chart.Series.Add(seriesRevenue);
        }

        // ===================== TOP PRODUCTS =====================
        private void BuildTopProducts(Panel host)
        {
            // link "Chi tiết >"
            lnkTopDetail = new LinkLabel
            {
                AutoSize = true,
                Text = "Chi tiết >",
                LinkColor = C_ACCENT,
                ActiveLinkColor = C_ACCENT,
                VisitedLinkColor = C_ACCENT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                LinkBehavior = LinkBehavior.NeverUnderline,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            host.Controls.Add(lnkTopDetail);
            lnkTopDetail.BringToFront();

            host.Resize += (s, e) =>
            {
                lnkTopDetail.Left = host.ClientSize.Width - lnkTopDetail.Width - 14;
                lnkTopDetail.Top = 8;
            };
            lnkTopDetail.Left = host.ClientSize.Width - lnkTopDetail.Width - 14;
            lnkTopDetail.Top = 8;

            imgTop = new ImageList { ImageSize = new Size(40, 40), ColorDepth = ColorDepth.Depth32Bit };
            imgTop.Images.Add(MakeThumb("A"));
            imgTop.Images.Add(MakeThumb("J"));
            imgTop.Images.Add(MakeThumb("K"));
            imgTop.Images.Add(MakeThumb("H"));

            lvTop = new ListView
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                ForeColor = C_TEXT,
                BorderStyle = BorderStyle.None,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                HeaderStyle = ColumnHeaderStyle.None,
                SmallImageList = imgTop
            };
            lvTop.Columns.Add("", 260);
            lvTop.Columns.Add("", 60, HorizontalAlignment.Right);

            // padding phía trên để né header
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 18, 0, 0), BackColor = Color.Transparent };
            body.Controls.Add(lvTop);
            host.Controls.Add(body);
            body.BringToFront();
        }

        private Image MakeThumb(string seed)
        {
            var bmp = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(45, 45, 45));
                using (var br = new SolidBrush(Color.FromArgb(70, 70, 70)))
                    g.FillRectangle(br, 7, 7, 26, 26);

                TextRenderer.DrawText(g, seed, new Font("Segoe UI", 12f, FontStyle.Bold),
                    new Rectangle(0, 0, 40, 40), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                using (var pen = new Pen(Color.FromArgb(90, 90, 90)))
                    g.DrawRectangle(pen, 0, 0, 39, 39);
            }
            return bmp;
        }

        // ===================== ORDERS GRID =====================
        private void BuildOrdersGrid(Panel host)
        {
            // toolbar: filter + reload icon
            var pnTool = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
            host.Controls.Add(pnTool);
            pnTool.BringToFront();

            btnFilter = new Button
            {
                Text = "Lọc dữ liệu",
                Width = 110,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            btnFilter.FlatAppearance.BorderColor = C_BORDER;
            btnFilter.FlatAppearance.BorderSize = 1;
            pnTool.Controls.Add(btnFilter);

            btnReload = new Button
            {
                Text = "⟳",
                Width = 34,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            btnReload.FlatAppearance.BorderColor = C_BORDER;
            btnReload.FlatAppearance.BorderSize = 1;
            pnTool.Controls.Add(btnReload);

            pnTool.Resize += (s, e) =>
            {
                btnReload.Left = pnTool.ClientSize.Width - btnReload.Width;
                btnReload.Top = 2;
                btnFilter.Left = btnReload.Left - btnFilter.Width - 10;
                btnFilter.Top = 2;
            };
            btnReload.Left = pnTool.ClientSize.Width - btnReload.Width;
            btnReload.Top = 2;
            btnFilter.Left = btnReload.Left - btnFilter.Width - 10;
            btnFilter.Top = 2;

            dgv = new DataGridView
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

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 28, 28);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.BackColor = C_PANEL;
            dgv.DefaultCellStyle.ForeColor = C_TEXT;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.RowTemplate.Height = 48;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MÃ ĐƠN", DataPropertyName = "Code", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "KHÁCH HÀNG", DataPropertyName = "Customer", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "NGÀY ĐẶT", DataPropertyName = "DateText", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "TỔNG TIỀN",
                DataPropertyName = "TotalText",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TRẠNG THÁI", DataPropertyName = "StatusText", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TÁC VỤ", DataPropertyName = "Action", Width = 80 });

            dgv.CellPainting += Dgv_CellPainting;
            host.Controls.Add(dgv);
            dgv.BringToFront();

            // bottom footer: info + pager arrows
            var pnBottom = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = Color.Transparent };
            host.Controls.Add(pnBottom);
            pnBottom.BringToFront();

            lbBottomInfo = new Label
            {
                AutoSize = true,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f),
                Text = "Hiển thị 0 trên 0 đơn hàng",
                Location = new Point(0, 8)
            };
            pnBottom.Controls.Add(lbBottomInfo);

            pgPrev = MakePager("◀");
            pgNext = MakePager("▶");
            pnBottom.Controls.Add(pgPrev);
            pnBottom.Controls.Add(pgNext);

            pnBottom.Resize += (s, e) =>
            {
                pgNext.Left = pnBottom.ClientSize.Width - pgNext.Width;
                pgNext.Top = 2;
                pgPrev.Left = pgNext.Left - pgPrev.Width - 8;
                pgPrev.Top = 2;
            };
            pgNext.Left = pnBottom.ClientSize.Width - pgNext.Width;
            pgNext.Top = 2;
            pgPrev.Left = pgNext.Left - pgPrev.Width - 8;
            pgPrev.Top = 2;
        }

        private Button MakePager(string text)
        {
            var b = new Button
            {
                Text = text,
                Width = 34,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = C_BORDER;
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Code column: link tím
            if (e.ColumnIndex == 0)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                var text = Convert.ToString(e.FormattedValue) ?? "";
                var r = e.CellBounds; r.Inflate(-10, -10);

                TextRenderer.DrawText(e.Graphics, text,
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    r, C_ACCENT, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                using (var pen = new Pen(Color.FromArgb(120, C_ACCENT)))
                {
                    int y = e.CellBounds.Bottom - 12;
                    e.Graphics.DrawLine(pen, e.CellBounds.Left + 10, y, e.CellBounds.Left + 80, y);
                }
                return;
            }

            // Status pill column
            if (e.ColumnIndex == 4)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                var row = dgv.Rows[e.RowIndex].DataBoundItem as OrderRow;
                if (row == null) return;

                string text = row.StatusText;
                Color c = row.StatusColor;

                var b = e.CellBounds;
                var rect = new Rectangle(b.Left + 12, b.Top + (b.Height - 24) / 2, b.Width - 24, 24);

                using (var br = new SolidBrush(Color.FromArgb(28, c)))
                    e.Graphics.FillRectangle(br, rect);
                using (var pen = new Pen(c))
                    e.Graphics.DrawRectangle(pen, rect);

                TextRenderer.DrawText(e.Graphics, text,
                    new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    rect, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                return;
            }

            // Action column: vẽ icon bút
            if (e.ColumnIndex == 5)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                var b = e.CellBounds;
                int icon = 18;
                var r = new Rectangle(b.Left + (b.Width - icon) / 2, b.Top + (b.Height - icon) / 2, icon, icon);
                DrawPencil(e.Graphics, r, C_MUTED);
                return;
            }
        }

        private void DrawPencil(Graphics g, Rectangle r, Color c)
        {
            using (var pen = new Pen(c, 2))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(pen, r.Left + 3, r.Bottom - 4, r.Right - 4, r.Top + 3);
                g.DrawLine(pen, r.Right - 6, r.Top + 5, r.Right - 2, r.Top + 1);
                g.DrawLine(pen, r.Right - 5, r.Top + 6, r.Right - 1, r.Top + 2);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }
        }

        // ===================== EVENTS =====================
        private void Wire()
        {
            btnRange7d.Click += (s, e) => ReloadData();
            lnkTopDetail.Click += (s, e) => MessageBox.Show("Demo: mở danh sách sản phẩm bán chạy.");
            btnFilter.Click += (s, e) => {
                var frm = new FrmOrderFilter();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ApplyOrderFilter(frm.FromDate, frm.ToDate, frm.Status, frm.Keyword);
                }
            };
            btnReload.Click += (s, e) => ReloadData();

            pgPrev.Click += (s, e) => MessageBox.Show("Demo: trang trước.");
            pgNext.Click += (s, e) => MessageBox.Show("Demo: trang sau.");
        }

        // ===================== DEMO BIND =====================
        private void LoadDataFromService()
        {
            try
            {
                // Tạo service — giả sử bạn có AppSession.DbContext
                // Nếu không, thay bằng: new FashionShopDb()
                using (var context = new Data.FashionShopDb())
                {
                    var service = new DashboardService(context);
                    var data = service.GetSummary();

                    // ===== CẬP NHẬT KPI CARDS =====
                    vRevenue.Text = data.TodayRevenue.ToString("#,0") + " đ";
                    vOrders.Text = data.NewOrdersToday.ToString();
                    vStock.Text = data.TotalStock.ToString();
                    vNewCus.Text = data.NewCustomersToday.ToString();

                    // Cập nhật delta text (vẫn giữ nguyên logic màu như demo)
                    // Vì bạn KHÔNG muốn sửa class OrderRow, nên ta giữ nguyên cách hiển thị delta như mockup
                    // → Chỉ đổi SỐ, GIỮ NGUYÊN CHUỖI MÔ TẢ VÀ MÀU (như ảnh bạn gửi)

                    // (Tùy chọn: bạn có thể cập nhật delta text động, nhưng hiện tại để đơn giản → giữ nguyên như demo)

                    // ===== BIỂU ĐỒ 7 NGÀY =====
                    var s = chart.Series[0];
                    s.Points.Clear();
                    foreach (var point in data.Revenue7Days)
                    {
                        string dayName = point.Day.ToString("ddd"); // T2, T3, ..., CN
                        int idx = s.Points.AddXY(dayName, (double)point.Revenue);
                        var dp = s.Points[idx];
                        dp.Color = (dayName == "CN") ? C_ACCENT : C_BLUEBAR;
                        dp.BorderColor = Color.FromArgb(80, 80, 80);
                        dp.BorderWidth = 1;
                    }

                    // ===== TOP SẢN PHẨM =====
                    lvTop.Items.Clear();
                    for (int i = 0; i < data.TopProducts.Count && i < 4; i++)
                    {
                        var p = data.TopProducts[i];
                        AddTop(p.ProductName, p.Sku, p.Sold.ToString() + " cái", i);
                    }
                    FixTopCols();

                    // ===== ĐƠN HÀNG GẦN ĐÂY =====
                    _orders.Clear();
                    foreach (var order in data.RecentOrders)
                    {
                        // DÙNG ĐÚNG CONSTRUCTOR CỦA ORDERROW MÀ BẠN ĐÃ CÓ
                        _orders.Add(new OrderRow(
                            order.Code,
                            order.Customer,
                            order.DateText,
                            order.Total,
                            order.StatusText  // ← Class OrderRow sẽ tự suy ra màu từ text
                        ));
                    }

                    dgv.DataSource = null;
                    dgv.DataSource = _orders;

                    // Cập nhật footer
                    lbBottomInfo.Text = $"Hiển thị {_orders.Count} trên {data.RecentOrders.Count} đơn hàng";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTop(string name, string sub, string right, int imgIndex)
        {
            // cột trái: 2 dòng (name + sub)
            var it = new ListViewItem(name) { ImageIndex = imgIndex };
            it.SubItems.Add(right);

            // trick: dùng Tag để show phụ đề khi cần (hoặc bạn có thể custom draw)
            it.ToolTipText = sub;
            lvTop.Items.Add(it);
        }

        private void FixTopCols()
        {
            if (lvTop.Columns.Count < 2) return;
            int w = lvTop.ClientSize.Width;
            if (w <= 0) return;

            int col2 = 70;
            lvTop.Columns[0].Width = Math.Max(120, w - col2 - 8);
            lvTop.Columns[1].Width = col2;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            FixTopCols();
        }

        // ===================== LỌC DỮ LIỆU (MỚI THÊM) =====================
        private void ApplyOrderFilter(DateTime? fromDate, DateTime? toDate, string status, string keyword)
        {
            try
            {
                using (var context = new Data.FashionShopDb())
                {
                    var service = new DashboardService(context);
                    var allOrders = service.GetSummary().RecentOrders;

                    var filtered = new List<dynamic>(allOrders);

                    // Lọc theo ngày — chuyển DateText sang DateTime
                    if (fromDate.HasValue || toDate.HasValue)
                    {
                        var filteredWithDate = new List<dynamic>();
                        foreach (var o in filtered)
                        {
                            if (DateTime.TryParse(o.DateText, out DateTime orderDate))
                            {
                                bool match = true;
                                if (fromDate.HasValue && orderDate < fromDate.Value) match = false;
                                if (toDate.HasValue && orderDate > toDate.Value) match = false;
                                if (match) filteredWithDate.Add(o);
                            }
                        }
                        filtered = filteredWithDate;
                    }

                    // Lọc theo trạng thái
                    if (!string.IsNullOrEmpty(status))
                    {
                        int statusValue = -1;
                        if (status == "Chờ xử lý") statusValue = 0;
                        else if (status == "Đang giao") statusValue = 1;
                        else if (status == "Hoàn thành") statusValue = 2;
                        else if (status == "Hủy") statusValue = 3;

                        if (statusValue != -1)
                            filtered = filtered.FindAll(o => o.Status == statusValue);
                    }

                    // Lọc theo từ khóa (mã đơn, khách hàng)
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        string k = keyword.ToLower();
                        filtered = filtered.FindAll(o =>
                            o.Code.ToLower().Contains(k) ||
                            o.Customer.ToLower().Contains(k));
                    }

                    // Cập nhật lại danh sách hiển thị
                    _orders.Clear();
                    foreach (var order in filtered)
                    {
                        _orders.Add(new OrderRow(
                            order.Code,
                            order.Customer,
                            order.DateText,
                            order.Total,
                            order.StatusText
                        ));
                    }

                    dgv.DataSource = null;
                    dgv.DataSource = _orders;

                    lbBottomInfo.Text = $"Hiển thị {_orders.Count} trên {allOrders.Count} đơn hàng";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== MODEL =====================
        private sealed class OrderRow
        {
            public string Code { get; private set; }
            public string Customer { get; private set; }
            public string DateText { get; private set; }
            public decimal Total { get; private set; }
            public string StatusText { get; private set; }

            public string TotalText
            {
                get
                {
                    var nfi = (NumberFormatInfo)CultureInfo.GetCultureInfo("vi-VN").NumberFormat.Clone();
                    nfi.NumberGroupSeparator = ".";
                    return Total.ToString("#,0", nfi) + "đ";
                }
            }

            public string Action { get { return ""; } }

            public Color StatusColor
            {
                get
                {
                    // giống màu pill trong ảnh (xanh/ vàng/ xanh dương/ đỏ)
                    if (StatusText.IndexOf("Hoàn", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(0, 180, 120);
                    if (StatusText.IndexOf("Vận", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(220, 150, 0);
                    if (StatusText.IndexOf("Chờ", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(60, 140, 255);
                    if (StatusText.IndexOf("hủy", StringComparison.OrdinalIgnoreCase) >= 0) return Color.FromArgb(255, 80, 80);
                    return Color.FromArgb(120, 120, 120);
                }
            }

            public OrderRow(string code, string customer, string dateText, decimal total, string status)
            {
                Code = code;
                Customer = customer;
                DateText = dateText;
                Total = total;
                StatusText = status;
            }
        }
    }
}