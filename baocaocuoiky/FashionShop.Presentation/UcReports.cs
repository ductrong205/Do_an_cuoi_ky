using FashionShop.Business;
using FashionShop.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FashionShop.Presentation
{
    public partial class UcReports : UserControl, IReloadable
    {
        // ===== THEME (theo ảnh) =====
        private static readonly Color C_BG = Color.FromArgb(20, 20, 20);
        private static readonly Color C_PANEL = Color.FromArgb(28, 28, 28);
        private static readonly Color C_PANEL2 = Color.FromArgb(32, 32, 32);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_TEXT = Color.Gainsboro;
        private static readonly Color C_MUTED = Color.FromArgb(150, 150, 150);

        private static readonly Color C_ACCENT = Color.FromArgb(155, 60, 255);   // tím (Revenue)
        private static readonly Color C_TARGET = Color.FromArgb(70, 70, 75);    // xám (Target)
        private static readonly Color C_OK = Color.FromArgb(60, 200, 120);
        private static readonly Color C_BAD = Color.FromArgb(255, 80, 80);

        // ===== ROOT =====
        private TableLayoutPanel tlpRoot;

        // header
        private Panel pnHeader;
        private Label lbH1, lbH2;
        private FlowLayoutPanel flTimeRange;
        private Button btnToday, btnWeek, btnMonth, btnCustom;
        private Button btnPrint, btnExport;

        // cards
        private TableLayoutPanel tlpCards;
        private Label vRevenue, vProfit, vOrders, vAov;
        private Label dRevenue, dProfit, dOrders, dAov;

        // middle
        private TableLayoutPanel tlpMid;
        private Panel pnChartWrap, pnDonutWrap;
        private Chart chartRevenue;
        private Chart chartDonut;
        private Label lbDonutCenter1, lbDonutCenter2;
        private TableLayoutPanel tlpDonutLegend;

        // bottom
        private Panel pnGridWrap;
        private Button btnRefresh, btnFilter;
        private LinkLabel lnkFull;
        private DataGridView dgv;

        // data demo
        private List<BarPoint> _bars;
        private List<CategoryPoint> _cats;
        private List<ProductRow> _products;

        private readonly ReportService _reportService;

        public UcReports()
        {
            _reportService = new ReportService(new FashionShopDb());

            BackColor = C_BG;
            Dock = DockStyle.Fill;

            BuildUI();
            Wire();

            LoadDataFromDb("Tháng");
            BindAll();
            ReloadData();
        }

        public void ReloadData()
        {
            LoadDataFromDb(GetSelectedRange());
            BindAll();
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
                RowCount = 4,
                Padding = new Padding(18, 12, 18, 12)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));   // header
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));  // cards
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 330));  // charts
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // grid
            Controls.Add(tlpRoot);

            // ===== HEADER =====
            pnHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tlpRoot.Controls.Add(pnHeader, 0, 0);

            var pnHL = new Panel { Dock = DockStyle.Left, Width = 520, BackColor = Color.Transparent };
            pnHeader.Controls.Add(pnHL);

            lbH1 = new Label
            {
                AutoSize = true,
                Text = "Thống kê",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Location = new Point(0, 8)
            };
            pnHL.Controls.Add(lbH1);

            lbH2 = new Label
            {
                AutoSize = true,
                Text = "Dữ liệu sau khi cập nhật",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f),
                Location = new Point(2, 36)
            };
            pnHL.Controls.Add(lbH2);

            var pnHR = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnHeader.Controls.Add(pnHR);

            // time range group
            var pnTR = new Panel
            {
                Height = 40,
                Width = 360,
                BackColor = C_PANEL,
                Padding = new Padding(10, 7, 10, 7),
                Margin = new Padding(0, 0, 12, 0)
            };
            pnTR.Paint += BorderPaint;
            pnHR.Controls.Add(pnTR);

            var lbTR = new Label
            {
                AutoSize = true,
                Text = "Khoảng thời gian:",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(10, 11)
            };
            pnTR.Controls.Add(lbTR);

            flTimeRange = new FlowLayoutPanel
            {
                Location = new Point(130, 6),
                Size = new Size(220, 28),
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            pnTR.Controls.Add(flTimeRange);

            // ✅ đổi text nút sang tiếng Việt (giữ nguyên code/logic chọn range)
            btnToday = MakeSegButton("Hôm nay");
            btnWeek = MakeSegButton("Tuần");
            btnMonth = MakeSegButton("Tháng", selected: true);
            btnCustom = MakeSegButton("Tùy chọn");

            flTimeRange.Controls.Add(btnToday);
            flTimeRange.Controls.Add(btnWeek);
            flTimeRange.Controls.Add(btnMonth);
            flTimeRange.Controls.Add(btnCustom);

            btnPrint = MakeTopButton("🖨  In", Color.FromArgb(45, 45, 45), C_TEXT, 90);
            btnExport = MakeTopButton("⬇  Xuất Excel", Color.FromArgb(30, 110, 60), Color.White, 140);

            pnHR.Controls.Add(btnPrint);
            pnHR.Controls.Add(btnExport);

            // ===== CARDS =====
            tlpCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 8)
            };
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpRoot.Controls.Add(tlpCards, 0, 1);

            // ✅ đổi title/subtitle sang tiếng Việt
            tlpCards.Controls.Add(MakeKpiCard("Doanh thu", "Tổng doanh thu (gộp)", out vRevenue, out dRevenue, "↑ 12%", C_OK, "💵"), 0, 0);
            tlpCards.Controls.Add(MakeKpiCard("Lợi nhuận ròng", "Sau thuế & chi phí", out vProfit, out dProfit, "↑ 5%", C_OK, "🏛"), 1, 0);
            tlpCards.Controls.Add(MakeKpiCard("Tổng đơn hàng", "Giao dịch hoàn tất", out vOrders, out dOrders, "↓ 2%", C_BAD, "🛒"), 2, 0);
            tlpCards.Controls.Add(MakeKpiCard("Giá trị TB/đơn", "Trên mỗi giao dịch", out vAov, out dAov, "↑ 8%", C_OK, "📈"), 3, 0);

            // ===== MIDDLE =====
            tlpMid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)
            };
            tlpMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            tlpMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            tlpRoot.Controls.Add(tlpMid, 0, 2);

            // ✅ đổi title wrap sang tiếng Việt
            pnChartWrap = MakeWrap("Phân tích doanh thu (Biểu đồ cột)");
            pnChartWrap.Margin = new Padding(0, 0, 12, 0);
            tlpMid.Controls.Add(pnChartWrap, 0, 0);

            pnDonutWrap = MakeWrap("Phân bổ theo danh mục");
            pnDonutWrap.Margin = new Padding(0);
            tlpMid.Controls.Add(pnDonutWrap, 1, 0);

            BuildRevenueChart(pnChartWrap);
            BuildDonut(pnDonutWrap);

            // ===== GRID =====
            pnGridWrap = MakeWrap("Sản phẩm bán chạy (Bảng dữ liệu)");
            pnGridWrap.Margin = new Padding(0);
            tlpRoot.Controls.Add(pnGridWrap, 0, 3);

            BuildGrid(pnGridWrap);
        }

        private Panel MakeWrap(string title)
        {
            var wrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(14, 12, 14, 14)
            };
            wrap.Paint += BorderPaint;

            var lb = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = title,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            wrap.Controls.Add(lb);

            return wrap;
        }

        private void BorderPaint(object sender, PaintEventArgs e)
        {
            var p = sender as Control;
            if (p == null) return;
            using (var pen = new Pen(C_BORDER))
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        private Button MakeTopButton(string text, Color back, Color fore, int w)
        {
            var b = new Button
            {
                Text = text,
                Width = w,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false,
                Margin = new Padding(0, 0, 12, 0)
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Button MakeSegButton(string text, bool selected = false)
        {
            var b = new Button
            {
                Text = text,
                Width = 70,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TabStop = false,
                Margin = new Padding(4, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);

            SetSegSelected(b, selected);
            return b;
        }

        private void SetSegSelected(Button b, bool selected)
        {
            b.BackColor = selected ? C_ACCENT : Color.FromArgb(45, 45, 45);
            b.ForeColor = selected ? Color.White : C_TEXT;
        }

        private Panel MakeKpiCard(string title, string subtitle, out Label vMain, out Label vDelta,
                                  string deltaText, Color deltaColor, string icon)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(14, 12, 14, 12),
                Margin = new Padding(0, 0, 12, 0)
            };
            card.Paint += BorderPaint;

            // title row
            var lbT = new Label
            {
                AutoSize = true,
                Text = title,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(10, 10)
            };
            card.Controls.Add(lbT);

            var lbIcon = new Label
            {
                AutoSize = false,
                Size = new Size(30, 30),
                Text = icon,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(40, 40, 40),
                Location = new Point(card.Width - 44, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(lbIcon);

            vMain = new Label
            {
                AutoSize = true,
                Text = "0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Location = new Point(10, 34)
            };
            card.Controls.Add(vMain);

            var lbSub = new Label
            {
                AutoSize = true,
                Text = subtitle,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(10, 66)
            };
            card.Controls.Add(lbSub);

            var lbVs = new Label
            {
                AutoSize = true,
                Text = "so với tháng trước",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(10, 88)
            };
            card.Controls.Add(lbVs);

            vDelta = new Label
            {
                AutoSize = true,
                Text = deltaText,
                ForeColor = deltaColor,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(card.Width - 55, 86),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(vDelta);

            return card;
        }

        // ===================== CHARTS =====================
        private void BuildRevenueChart(Panel host)
        {
            // legend (Revenue/Target)
            var pnLegend = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = Color.Transparent
            };
            host.Controls.Add(pnLegend);
            pnLegend.BringToFront();

            // ✅ đổi legend tiếng Việt
            var lbRev = MakeLegendItem("Doanh thu", C_ACCENT);
            lbRev.Location = new Point(host.Width - 200, 4);
            lbRev.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnLegend.Controls.Add(lbRev);

            var lbTar = MakeLegendItem("Mục tiêu", C_TARGET);
            lbTar.Location = new Point(host.Width - 95, 4);
            lbTar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnLegend.Controls.Add(lbTar);

            chartRevenue = new Chart { Dock = DockStyle.Fill, BackColor = C_PANEL };
            host.Controls.Add(chartRevenue);

            var area = new ChartArea("A");
            area.BackColor = C_PANEL;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(40, 40, 40);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(40, 40, 40);
            area.AxisX.LabelStyle.ForeColor = C_MUTED;
            area.AxisY.LabelStyle.ForeColor = C_MUTED;
            area.AxisX.LineColor = C_BORDER;
            area.AxisY.LineColor = C_BORDER;
            area.AxisX.MajorTickMark.LineColor = C_BORDER;
            area.AxisY.MajorTickMark.LineColor = C_BORDER;
            area.AxisY.Minimum = 0;
            area.AxisX.Interval = 1;

            chartRevenue.ChartAreas.Add(area);
            chartRevenue.Legends.Clear();

            var sTarget = new Series("Target")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(130, C_TARGET),
                BorderColor = Color.FromArgb(90, 90, 90),
                BorderWidth = 1
            };
            sTarget["PointWidth"] = "0.6";

            var sRevenue = new Series("Revenue")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(220, C_ACCENT),
                BorderColor = C_ACCENT,
                BorderWidth = 1
            };
            sRevenue["PointWidth"] = "0.35";

            chartRevenue.Series.Add(sTarget);
            chartRevenue.Series.Add(sRevenue);
        }

        private Control MakeLegendItem(string text, Color color)
        {
            var pn = new Panel { Size = new Size(95, 18), BackColor = Color.Transparent };

            var box = new Panel
            {
                Size = new Size(10, 10),
                BackColor = color,
                Location = new Point(0, 4)
            };
            pn.Controls.Add(box);

            var lb = new Label
            {
                AutoSize = true,
                Text = text,
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(14, 1)
            };
            pn.Controls.Add(lb);

            return pn;
        }

        private void BuildDonut(Panel host)
        {
            // Tạo panel chứa biểu đồ (giới hạn kích thước)
            var chartContainer = new Panel
            {
                Size = new Size(200, 200), // ← Kích thước nhỏ hơn
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.None
            };
            host.Controls.Add(chartContainer);
            chartContainer.BringToFront();

            // Căn giữa biểu đồ trong host
            host.Resize += (_, __) =>
            {
                chartContainer.Left = (host.ClientSize.Width - chartContainer.Width) / 2;
                chartContainer.Top = 30; // Cách từ trên xuống 30px
            };
            chartContainer.Left = (host.ClientSize.Width - chartContainer.Width) / 2;
            chartContainer.Top = 30;

            // donut chart
            chartDonut = new Chart { Dock = DockStyle.Fill, BackColor = C_PANEL };
            chartContainer.Controls.Add(chartDonut);

            var area = new ChartArea("P");
            area.BackColor = C_PANEL;
            area.Area3DStyle.Enable3D = false;
            chartDonut.ChartAreas.Add(area);
            chartDonut.Legends.Clear();

            var s = new Series("Cats")
            {
                ChartType = SeriesChartType.Doughnut,
                ChartArea = "P"
            };
            s["DoughnutRadius"] = "60"; // ← Bán kính 60% là hợp lý
            s["PieLabelStyle"] = "Disabled";
            chartDonut.Series.Add(s);

            // center text overlay
            var pnCenter = new Panel
            {
                BackColor = Color.Transparent,
                Size = new Size(60, 40), // ← Nhỏ hơn
                Anchor = AnchorStyles.None
            };
            chartContainer.Controls.Add(pnCenter);
            pnCenter.BringToFront();

            // Căn giữa text trong biểu đồ
            pnCenter.Left = (chartContainer.Width - pnCenter.Width) / 2;
            pnCenter.Top = (chartContainer.Height - pnCenter.Height) / 2;

            lbDonutCenter1 = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), // ← Font nhỏ hơn
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnCenter.Controls.Add(lbDonutCenter1);

            lbDonutCenter2 = new Label
            {
                Dock = DockStyle.Fill,
                Text = "SẢN PHẨM",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.TopCenter
            };
            pnCenter.Controls.Add(lbDonutCenter2);

            // legend list (custom)
            tlpDonutLegend = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 100, // ← Giảm chiều cao legend
                ColumnCount = 3,
                RowCount = 4,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 0, 6, 0)
            };
            tlpDonutLegend.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            tlpDonutLegend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpDonutLegend.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            for (int i = 0; i < 4; i++)
                tlpDonutLegend.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            host.Controls.Add(tlpDonutLegend);
            tlpDonutLegend.BringToFront();
        }

        // ===================== GRID =====================
        private void BuildGrid(Panel host)
        {
            // top toolbar inside wrap
            var pnTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.Transparent
            };
            host.Controls.Add(pnTop);
            pnTop.BringToFront();

            // ✅ đổi text nút
            btnRefresh = MakeMiniButton("⟳  Tải lại");
            btnRefresh.Location = new Point(0, 2);
            pnTop.Controls.Add(btnRefresh);

            btnFilter = MakeMiniButton("⏷  Lọc");
            btnFilter.Location = new Point(btnRefresh.Right + 10, 2);
            pnTop.Controls.Add(btnFilter);

            // ✅ đổi link
            lnkFull = new LinkLabel
            {
                AutoSize = true,
                Text = "Xem toàn bộ dữ liệu >>",
                LinkColor = C_ACCENT,
                ActiveLinkColor = C_ACCENT,
                VisitedLinkColor = C_ACCENT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(host.Width - 170, 8)
            };
            lnkFull.LinkBehavior = LinkBehavior.NeverUnderline;
            pnTop.Controls.Add(lnkFull);

            pnTop.Resize += (s, e) =>
            {
                lnkFull.Left = pnTop.ClientSize.Width - lnkFull.Width - 6;
            };

            // datagrid
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

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 45);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.BackColor = C_PANEL;
            dgv.DefaultCellStyle.ForeColor = C_TEXT;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.RowTemplate.Height = 56;

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "#",
                DataPropertyName = "No",
                Width = 50
            });

            var colImg = new DataGridViewImageColumn
            {
                HeaderText = "",
                DataPropertyName = "Img",
                Width = 44,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgv.Columns.Add(colImg);

            // ✅ đổi tiêu đề cột sang tiếng Việt
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên sản phẩm",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã SKU",
                DataPropertyName = "Sku",
                Width = 120
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Đã bán",
                DataPropertyName = "Sold",
                Width = 90,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Trạng thái",
                DataPropertyName = "Status",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Doanh thu",
                DataPropertyName = "RevenueText",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgv.CellPainting += Dgv_CellPainting_StatusPill;

            host.Controls.Add(dgv);
            dgv.BringToFront();
        }

        private Button MakeMiniButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Width = 110,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void Dgv_CellPainting_StatusPill(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Status column index = 5 (0..6)
            if (e.ColumnIndex == 5)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string text = Convert.ToString(e.FormattedValue) ?? "";
                var r = e.CellBounds;
                r.Inflate(-10, -12);

                Color border = Color.FromArgb(90, 90, 90);
                Color fore = C_TEXT;

                if (text.IndexOf("CÒN HÀNG", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    border = Color.FromArgb(40, 170, 90);
                    fore = Color.FromArgb(60, 200, 120);
                }
                else if (text.IndexOf("HẾT HÀNG", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    border = Color.FromArgb(200, 170, 60);
                    fore = Color.FromArgb(240, 200, 90);
                }

                using (var pen = new Pen(border, 1))
                using (var br = new SolidBrush(Color.FromArgb(28, 28, 28)))
                {
                    e.Graphics.FillRectangle(br, r);
                    e.Graphics.DrawRectangle(pen, r);
                }

                TextRenderer.DrawText(e.Graphics, text,
                    new Font("Segoe UI", 9f, FontStyle.Bold),
                    r, fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                return;
            }
        }

        // ===================== EVENTS =====================
        private void Wire()
        {
            // giữ nguyên logic SelectRange, chỉ truyền text Việt theo nút
            btnToday.Click += (s, e) => SelectRange("Hôm nay");
            btnWeek.Click += (s, e) => SelectRange("Tuần");
            btnMonth.Click += (s, e) => SelectRange("Tháng");
            btnCustom.Click += (s, e) => SelectRange("Tùy chọn");

            btnPrint.Click += (s, e) =>
            {
                MessageBox.Show("In (demo). chưa phát triển.");
            };

            btnExport.Click += (s, e) => ExportCsv();

            btnRefresh.Click += (s, e) => { LoadDataFromDb(GetSelectedRange()); BindAll(); };
            btnFilter.Click += (s, e) => MessageBox.Show("Bạn muốn lọc theo gì? SKU/Trạng thái/Khoảng thời gian?");
            lnkFull.Click += (s, e) => MessageBox.Show("Xem toàn bộ dữ liệu.");
        }

        private void SelectRange(string range)
        {
            // ✅ chọn theo range Việt
            SetSegSelected(btnToday, range == "Hôm nay");
            SetSegSelected(btnWeek, range == "Tuần");
            SetSegSelected(btnMonth, range == "Tháng");
            SetSegSelected(btnCustom, range == "Tùy chọn");

            LoadDataFromDb(range);
            BindAll();
        }

        private string GetSelectedRange()
        {
            if (btnToday.BackColor == C_ACCENT) return "Hôm nay";
            if (btnWeek.BackColor == C_ACCENT) return "Tuần";
            if (btnCustom.BackColor == C_ACCENT) return "Tùy chọn";
            return "Tháng";
        }

        // ===================== DATA =====================
        private void LoadDataFromDb(string range)
        {
            try
            {
                lbH2.Text = "Khoảng thời gian: " + range + " | Cập nhật lúc: " + DateTime.Now.ToString("HH:mm");

                var stats = _reportService.GetStatistics(range);
                var dailyRevenue = _reportService.GetRevenueChart(range);
                var categorySales = _reportService.GetCategoryDonut();
                var topProducts = _reportService.GetTopSellingProducts();

                // Cập nhật KPI cards
                vRevenue.Text = stats.RevenueText;
                vProfit.Text = stats.ProfitText;
                vOrders.Text = stats.Orders.ToString("#,0");
                vAov.Text = stats.AovText;

                dRevenue.Text = stats.RevenueChangeText;
                dProfit.Text = stats.ProfitChangeText;
                dOrders.Text = stats.OrdersChangeText;
                dAov.Text = stats.AovChangeText;

                dRevenue.ForeColor = stats.RevenueChangeColor;
                dProfit.ForeColor = stats.ProfitChangeColor;
                dOrders.ForeColor = stats.OrdersChangeColor;
                dAov.ForeColor = stats.AovChangeColor;

                // Tự tạo model từ dữ liệu thô
                _bars = new List<BarPoint>();
                var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                var now = DateTime.Now;
                DateTime startDate = range == "Hôm nay" ? now.Date :
                                   range == "Tuần" ? now.AddDays(-7).Date :
                                   new DateTime(now.Year, now.Month, 1);

                for (int i = 0; i < 7; i++)
                {
                    var day = startDate.AddDays(i).DayOfWeek;
                    var label = days[(int)day];
                    var revenue = dailyRevenue.FirstOrDefault(d => d.Date.DayOfWeek == day)?.Total ?? 0;
                    var target = revenue * 1.2m;
                    _bars.Add(new BarPoint(label, (double)revenue, (double)target));
                }

                _cats = new List<CategoryPoint>();
                int totalQty = categorySales.Sum(x => x.TotalQuantity);
                int index = 0;
                foreach (var item in categorySales)
                {
                    var percent = totalQty > 0 ? (item.TotalQuantity * 100.0 / totalQty) : 0;
                    var color = GetCategoryColor(index++);
                    _cats.Add(new CategoryPoint(item.CategoryName, percent, color));
                }

                _products = new List<ProductRow>();
                int no = 1;
                foreach (var p in topProducts)
                {
                    var status = p.Sold >= 20 ? "CÒN HÀNG" : "SẮP HẾT HÀNG";
                    _products.Add(ProductRow.Make(no++, p.Name, p.Sku, p.Sold, status, p.Revenue));
                }

                BindAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê: " + ex.Message);
            }
        }

        // Thêm hàm GetCategoryColor vào UcReports.cs
        private Color GetCategoryColor(int index)
        {
            var colors = new[]
            {
        Color.FromArgb(0, 120, 215),
        Color.FromArgb(255, 80, 160),
        Color.FromArgb(0, 160, 200),
        Color.FromArgb(100, 100, 110)
    };
            return colors[index % colors.Length];
        }

        private void BindAll()
        {
            BindRevenueChart();
            BindDonut();
            BindGrid();
        }

        private void BindRevenueChart()
        {
            if (chartRevenue == null) return;

            if (_bars == null)
            {
                chartRevenue.Series[0].Points.Clear();
                chartRevenue.Series[1].Points.Clear();
                return;
            }

            chartRevenue.Series[0].Points.Clear(); // Target
            chartRevenue.Series[1].Points.Clear(); // Revenue

            foreach (var p in _bars)
            {
                chartRevenue.Series[0].Points.AddXY(p.Label, p.Target);
                chartRevenue.Series[1].Points.AddXY(p.Label, p.Revenue);
            }
        }

        private void BindDonut()
        {
            if (chartDonut == null) return;

            var s = chartDonut.Series[0];
            s.Points.Clear();

            if (_cats == null) return;

            foreach (var c in _cats)
            {
                int idx = s.Points.AddY(c.Percent);   // AddY trả về index (int)
                DataPoint dp = s.Points[idx];         // lấy DataPoint theo index
                dp.Color = c.Color;
            }

            // Tính tổng số lượng đã bán từ _products
            int totalSold = _products?.Sum(p => p.Sold) ?? 0;
            lbDonutCenter1.Text = totalSold.ToString("#,0");
            lbDonutCenter2.Text = "SẢN PHẨM";

            tlpDonutLegend.Controls.Clear();
            for (int i = 0; i < _cats.Count; i++)
            {
                var c = _cats[i];

                var colorBox = new Panel { Width = 5, Height = 10, BackColor = c.Color, Margin = new Padding(0, 6, 0, 0) };
                var lbName = new Label { Dock = DockStyle.Fill, Text = c.Name, ForeColor = C_TEXT, Font = new Font("Segoe UI", 9f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
                var lbPct = new Label { Dock = DockStyle.Fill, Text = c.Percent.ToString("0") + "%", ForeColor = C_TEXT, Font = new Font("Segoe UI", 9f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };

                tlpDonutLegend.Controls.Add(colorBox, 0, i);
                tlpDonutLegend.Controls.Add(lbName, 1, i);
                tlpDonutLegend.Controls.Add(lbPct, 2, i);
            }
        }

        private void BindGrid()
        {
            if (dgv == null) return;

            dgv.DataSource = null;
            dgv.DataSource = _products ?? new List<ProductRow>();
        }

        private void ExportCsv()
        {
            if (_products == null || _products.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "bao_cao_san_pham.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var lines = new List<string>();
                lines.Add("No,ProductName,SKU,Sold,Status,Revenue");

                foreach (var p in _products)
                {
                    string revenue = p.RevenueText.Replace(",", "").Replace(".", "");
                    lines.Add($"{p.No},\"{p.Name}\",{p.Sku},{p.Sold},\"{p.Status}\",{revenue}");
                }

                System.IO.File.WriteAllLines(sfd.FileName, lines);
                MessageBox.Show("Xuất CSV thành công!");
            }
        }

        // ===================== MODELS =====================
        private sealed class BarPoint
        {
            public string Label;
            public double Revenue;
            public double Target;
            public BarPoint(string label, double revenue, double target)
            {
                Label = label; Revenue = revenue; Target = target;
            }
        }

        private sealed class CategoryPoint
        {
            public string Name;
            public double Percent;
            public Color Color;
            public CategoryPoint(string name, double percent, Color color)
            {
                Name = name; Percent = percent; Color = color;
            }
        }

        private sealed class ProductRow
        {
            public int No { get; private set; }
            public Image Img { get; private set; }
            public string Name { get; private set; }
            public string Sku { get; private set; }
            public int Sold { get; private set; }
            public string Status { get; private set; }
            public string RevenueText { get; private set; }

            public static ProductRow Make(int no, string name, string sku, int sold, string status, decimal revenue)
            {
                return new ProductRow
                {
                    No = no,
                    Img = MakeThumb(name),
                    Name = name,
                    Sku = sku,
                    Sold = sold,
                    Status = status,
                    RevenueText = Money(revenue)
                };
            }

            private static string Money(decimal v)
            {
                var nfi = (NumberFormatInfo)CultureInfo.GetCultureInfo("vi-VN").NumberFormat.Clone();
                nfi.NumberGroupSeparator = ".";
                return v.ToString("#,0", nfi);
            }

            private static Image MakeThumb(string seed)
            {
                // thumbnail demo (không cần file ảnh)
                var bmp = new Bitmap(38, 38);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(45, 45, 45));
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    int h = (seed ?? "").GetHashCode();
                    var col = Color.FromArgb(80 + Math.Abs(h % 140), 80 + Math.Abs((h / 3) % 140), 80 + Math.Abs((h / 7) % 140));

                    using (var br = new SolidBrush(col))
                        g.FillRectangle(br, 6, 6, 26, 26);

                    using (var pen = new Pen(Color.FromArgb(90, 90, 90)))
                        g.DrawRectangle(pen, 0, 0, 37, 37);
                }
                return bmp;
            }
        }
    }

}
