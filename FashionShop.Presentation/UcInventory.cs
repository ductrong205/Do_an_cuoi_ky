using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public sealed partial class UcInventory : UserControl, IReloadable
    {
        // ===== THEME =====
        private static readonly Color C_BG = Color.FromArgb(20, 20, 20);
        private static readonly Color C_PANEL = Color.FromArgb(30, 30, 30);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_TEXT = Color.Gainsboro;
        private static readonly Color C_MUTED = Color.FromArgb(150, 150, 150);
        private static readonly Color C_ACCENT = Color.FromArgb(0, 120, 215);

        private static readonly Color C_GREEN = Color.FromArgb(0, 170, 90);
        private static readonly Color C_RED = Color.FromArgb(210, 60, 60);
        private static readonly Color C_ORANGE = Color.FromArgb(235, 170, 40);
        private static readonly Color C_BLUE = Color.FromArgb(0, 120, 215);

        private const string PLACEHOLDER = "Nhập SKU hoặc vị trí...";

        // ===== UI =====
        private TableLayoutPanel tlpRoot;
        private TableLayoutPanel tlpTop;
        private TableLayoutPanel tlpCards;
        private Panel pnSearchWrap;
        private TextBox txtSearch;
        private Button btnSearch;

        private Panel pnGridWrap;
        private Label lbGridTitle;
        private DoubleBufferedGrid dgv;

        // cards value labels
        private Label vTotalProducts, vStockValue, vLowAlert, vInbound;

        // ===== DATA =====
        private List<InvRow> _all = new List<InvRow>();
        private List<InvRow> _filtered = new List<InvRow>();

        public UcInventory()
        {
            BackColor = C_BG;
            Dock = DockStyle.Fill;

            BuildUI();
            Wire();

            SeedDemo();
            ApplyFilter();
        }

        public void ReloadData()
        {
            SeedDemo();
            ApplyFilter();
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
                RowCount = 2,
                Padding = new Padding(18, 12, 18, 12)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(tlpRoot);

            // ===== TOP BAR: cards + search =====
            tlpTop = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1
            };
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 74f));
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26f));
            tlpRoot.Controls.Add(tlpTop, 0, 0);

            tlpCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = Color.Transparent,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 0, 12, 0)
            };
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpTop.Controls.Add(tlpCards, 0, 0);

            var c1 = MakeCard("Tổng sản phẩm", "📦", out vTotalProducts, C_BLUE);
            var c2 = MakeCard("Giá trị tồn", "💰", out vStockValue, C_GREEN);
            var c3 = MakeCard("Cảnh báo thấp", "⚠", out vLowAlert, C_RED);
            var c4 = MakeCard("Đang nhập", "🚚", out vInbound, C_BLUE);

            c1.Margin = new Padding(0, 0, 12, 0);
            c2.Margin = new Padding(0, 0, 12, 0);
            c3.Margin = new Padding(0, 0, 12, 0);
            c4.Margin = new Padding(0);

            tlpCards.Controls.Add(c1, 0, 0);
            tlpCards.Controls.Add(c2, 1, 0);
            tlpCards.Controls.Add(c3, 2, 0);
            tlpCards.Controls.Add(c4, 3, 0);

            pnSearchWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 0)
            };
            tlpTop.Controls.Add(pnSearchWrap, 1, 0);

            var pnSearchBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.White,
                Padding = new Padding(10, 8, 8, 8),
                Margin = new Padding(0, 6, 0, 0)
            };

            btnSearch = new Button
            {
                Dock = DockStyle.Right,
                Width = 44,
                Text = "🔍",
                FlatStyle = FlatStyle.Flat,
                BackColor = C_ACCENT,
                ForeColor = Color.White,
                TabStop = false
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            pnSearchBox.Controls.Add(btnSearch);

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                BackColor = Color.White,
                Text = PLACEHOLDER
            };
            pnSearchBox.Controls.Add(txtSearch);

            var lbSearchTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Text = "TÌM KIẾM NHANH KHO",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            // ✅ Dock Top: add BOX trước, add TITLE sau -> TITLE nằm trên
            pnSearchWrap.Controls.Add(pnSearchBox);
            pnSearchWrap.Controls.Add(lbSearchTitle);

            // ===== GRID =====
            pnGridWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(16, 12, 16, 16),
                Margin = new Padding(0, 12, 0, 0)
            };
            pnGridWrap.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnGridWrap.Width - 1, pnGridWrap.Height - 1);
            };
            tlpRoot.Controls.Add(pnGridWrap, 0, 1);

            lbGridTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = "DANH SÁCH TỒN KHO CHI TIẾT",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            pnGridWrap.Controls.Add(lbGridTitle);

            dgv = new DoubleBufferedGrid
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
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = Color.FromArgb(45, 45, 45)
            };

            var pnGridHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = Color.Transparent
            };
            pnGridHost.Controls.Add(dgv);
            pnGridWrap.Controls.Add(pnGridHost);

            StyleGrid();
            BuildColumns();
        }

        private Panel MakeCard(string title, string icon, out Label value, Color iconColor)
        {
            var pn = new Panel
            {
                BackColor = C_PANEL,
                Height = 68,
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 12, 14, 12)
            };
            pn.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawRectangle(pen, 0, 0, pn.Width - 1, pn.Height - 1);
            };

            // ✅ Add FILL trước, LEFT sau để không bị đè chữ
            var pnText = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 0, 0, 0)
            };
            pn.Controls.Add(pnText);

            value = new Label
            {
                Dock = DockStyle.Fill,
                Text = "0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnText.Controls.Add(value);

            var lbTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                Text = title,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            pnText.Controls.Add(lbTitle);

            var lbIcon = new Label
            {
                Dock = DockStyle.Left,
                Width = 34,
                Text = icon,
                ForeColor = iconColor,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pn.Controls.Add(lbIcon);

            return pn;
        }


        private void StyleGrid()
        {
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 32);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.BackColor = C_PANEL;
            dgv.DefaultCellStyle.ForeColor = C_TEXT;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.RowTemplate.Height = 42;
            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellFormatting += Dgv_CellFormatting;
        }

        private void BuildColumns()
        {
            dgv.Columns.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "STT",
                DataPropertyName = "Stt",
                Width = 60
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã SKU",
                DataPropertyName = "Sku",
                Width = 140
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên sản phẩm",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vị trí",
                DataPropertyName = "Location",
                Width = 110
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Ngày nhập",
                DataPropertyName = "DateText",
                Width = 120
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tồn thực tế",
                DataPropertyName = "Stock",
                Width = 150
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Trạng thái",
                DataPropertyName = "Status",
                Width = 140
            });
        }

        // ===================== EVENTS =====================
        private void Wire()
        {
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
            btnSearch.Click += (s, e) => ApplyFilter();
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // SKU xanh như link
            if (e.ColumnIndex == ColIndex("Mã SKU"))
            {
                e.CellStyle.ForeColor = C_ACCENT;
                e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }

            // Tên sản phẩm bold nhẹ
            if (e.ColumnIndex == ColIndex("Tên sản phẩm"))
            {
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9.6f, FontStyle.Bold);
            }

            // Tổng tồn (số) canh giữa
            if (e.ColumnIndex == ColIndex("Tồn thực tế"))
            {
                e.CellStyle.ForeColor = Color.White;
            }
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int colStock = ColIndex("Tồn thực tế");
            int colStatus = ColIndex("Trạng thái");

            // ===== Progress bar tồn thực tế =====
            if (e.ColumnIndex == colStock)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                int stock = 0;
                if (dgv.Rows[e.RowIndex].DataBoundItem is InvRow row)
                    stock = row.Stock;

                var r = e.CellBounds;
                r.Inflate(-10, -12);

                // track
                using (var brTrack = new SolidBrush(Color.FromArgb(45, 45, 45)))
                    e.Graphics.FillRectangle(brTrack, r);

                // fill theo mức (cap 200)
                int max = 200;
                float pct = Math.Max(0f, Math.Min(1f, stock / (float)max));
                int fillW = (int)(r.Width * pct);

                Color fillColor = stock <= 10 ? C_RED : (stock >= 160 ? C_ORANGE : C_GREEN);
                using (var brFill = new SolidBrush(fillColor))
                    e.Graphics.FillRectangle(brFill, new Rectangle(r.X, r.Y, fillW, r.Height));

                // border
                using (var pen = new Pen(Color.FromArgb(80, 80, 80)))
                    e.Graphics.DrawRectangle(pen, r);

                // number text (bên trái)
                var textRect = e.CellBounds;
                textRect.Inflate(-10, -6);
                TextRenderer.DrawText(e.Graphics, stock.ToString(),
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    textRect, Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                return;
            }

            // ===== Badge trạng thái =====
            if (e.ColumnIndex == colStatus)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                var status = "";
                if (dgv.Rows[e.RowIndex].DataBoundItem is InvRow row)
                    status = row.Status ?? "";


                Color c = C_TEXT;
                if (status == "LOW STOCK") c = C_RED;
                else if (status == "READY") c = C_GREEN;
                else if (status == "INBOUND") c = C_BLUE;
                else if (status == "HIGH STOCK") c = C_ORANGE;

                var r = e.CellBounds;
                r.Inflate(-10, -10);

                // badge box
                using (var br = new SolidBrush(Color.FromArgb(30, 30, 30)))
                    e.Graphics.FillRectangle(br, r);
                using (var pen = new Pen(c))
                    e.Graphics.DrawRectangle(pen, r);

                TextRenderer.DrawText(e.Graphics, status,
                    new Font("Segoe UI", 9.2f, FontStyle.Bold),
                    r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                return;
            }
        }

        private int ColIndex(string header)
        {
            for (int i = 0; i < dgv.Columns.Count; i++)
                if (dgv.Columns[i].HeaderText == header) return i;
            return -1;
        }

        // ===================== DATA =====================
        private void SeedDemo()
        {
            _all = new List<InvRow>
            {
                new InvRow(1,"TS-001-M","Áo Thun Basic - Size M","KỆ A-02", new DateTime(2023,10,20), 4),
                new InvRow(2,"QJ-SLIM-32","Quần Jean Slimfit - 32","KỆ B-12", new DateTime(2023,10,15), 45),
                new InvRow(3,"DR-FL-RED","Váy Hoa Nhí - Đỏ","KỆ C-05", new DateTime(2023,9,12), 82),
                new InvRow(4,"JKT-BKR-001","Áo Khoác Da Biker","KHO B", null, 0),
                new InvRow(5,"ACC-SCF-02","Khăn Len Basic","KỆ D-01", new DateTime(2023,11,1), 195),
            };

            foreach (var x in _all) x.Status = BuildStatus(x.Stock);

            // cards
            vTotalProducts.Text = _all.Count.ToString("#,0", CultureInfo.InvariantCulture);
            vStockValue.Text = "850.5M";     // demo
            vLowAlert.Text = _all.Count(x => x.Stock > 0 && x.Stock <= 10) + " SKU";
            vInbound.Text = "5 đơn";         // demo
        }

        private string BuildStatus(int stock)
        {
            if (stock == 0) return "INBOUND";
            if (stock <= 10) return "LOW STOCK";
            if (stock >= 160) return "HIGH STOCK";
            return "READY";
        }

        private void ApplyFilter()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLower();
            if (q == PLACEHOLDER.ToLower()) q = "";

            _filtered = _all.Where(x =>
                    string.IsNullOrEmpty(q)
                    || (x.Sku != null && x.Sku.ToLower().Contains(q))
                    || (x.Location != null && x.Location.ToLower().Contains(q))
                    || (x.Name != null && x.Name.ToLower().Contains(q))
                )
                .ToList();

            dgv.DataSource = null;
            dgv.DataSource = _filtered;
        }

        // ===================== MODELS =====================
        private sealed class InvRow
        {
            public int Stt { get; set; }
            public string Sku { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public string DateText { get; set; }
            public int Stock { get; set; }
            public string Status { get; set; }

            public InvRow(int stt, string sku, string name, string location, DateTime? date, int stock)
            {
                Stt = stt;
                Sku = sku;
                Name = name;
                Location = location;
                DateText = date.HasValue ? date.Value.ToString("dd/MM/yyyy") : "--/--/----";
                Stock = stock;
                Status = "";
            }
        }

        private sealed class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                UpdateStyles();
            }
        }
    }
}
