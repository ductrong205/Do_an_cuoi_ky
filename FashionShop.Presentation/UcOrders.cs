using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class UcOrders : UserControl, IReloadable
    {
        // ===== THEME =====
        private static readonly Color C_BG = Color.FromArgb(20, 20, 20);
        private static readonly Color C_PANEL = Color.FromArgb(30, 30, 30);
        private static readonly Color C_PANEL2 = Color.FromArgb(34, 34, 34);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_TEXT = Color.Gainsboro;
        private static readonly Color C_MUTED = Color.FromArgb(150, 150, 150);
        private static readonly Color C_ACCENT = Color.FromArgb(0, 120, 215);

        // status colors
        private static readonly Color S_SHIPPING = Color.FromArgb(18, 92, 170);
        private static readonly Color S_PENDING = Color.FromArgb(220, 140, 0);
        private static readonly Color S_PROCESS = Color.FromArgb(120, 120, 120);
        private static readonly Color S_DONE = Color.FromArgb(0, 140, 90);
        private static readonly Color S_CANCEL = Color.FromArgb(180, 40, 40);

        // ===== UI =====
        private TableLayoutPanel tlp;
        private Panel pnTop;
        private Panel pnGridWrap;
        private Panel pnBottom;

        private FlowLayoutPanel flLeft;
        private FlowLayoutPanel flTabs;
        private FlowLayoutPanel flRight;

        private Button btnNew;
        private Button btnAdv;
        private Button btnExport;

        private Button tabAll, tabPending, tabShipping, tabProcessing, tabDone, tabCancel;

        private DataGridView dgv;
        private Label lbInfo;
        private FlowLayoutPanel flPager;
        private Button pgPrev, pg1, pg2, pg3, pgNext;

        // ===== DATA =====
        private List<OrderRow> _all = new List<OrderRow>();
        private List<OrderRow> _view = new List<OrderRow>();
        private OrderStatus? _filter = null; // null = All
        private int _page = 1;
        private readonly int _pageSize = 12;

        // pager state
        private int _totalPages = 1;

        // columns index
        private const int COL_CHECK = 0;
        private const int COL_CODE = 1;
        private const int COL_TIME = 2;
        private const int COL_CUS = 3;
        private const int COL_PROD = 4;
        private const int COL_TOTAL = 5;
        private const int COL_STATUS = 6;
        private const int COL_ACTION = 7;

        public UcOrders()
        {
            BackColor = C_BG;
            Dock = DockStyle.Fill;

            BuildUI();
            Wire();

            SeedDemo();
            ApplyFilterAndPaging();
        }

        public void ReloadData()
        {
            SeedDemo();
            ApplyFilterAndPaging();
        }

        // ===================== UI =====================
        private void BuildUI()
        {
            Controls.Clear();

            tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(18, 12, 18, 12)
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(tlp);

            // ===== TOP BAR =====
            pnTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.Transparent
            };
            tlp.Controls.Add(pnTop, 0, 0);

            // left group (New + Tabs)
            flLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            pnTop.Controls.Add(flLeft);

            btnNew = MakeButton("➕  Tạo mới", C_PANEL2, C_TEXT);
            btnNew.Margin = new Padding(0, 8, 10, 0);
            flLeft.Controls.Add(btnNew);

            flTabs = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            flLeft.Controls.Add(flTabs);

            tabAll = MakeTab("Tất cả");
            tabPending = MakeTab("Chờ xử lý");
            tabShipping = MakeTab("Đang giao");
            tabProcessing = MakeTab("Đang xử lý");
            tabDone = MakeTab("Hoàn thành");
            tabCancel = MakeTab("Đã hủy"); // ✅ thêm tab Cancel

            flTabs.Controls.Add(tabAll);
            flTabs.Controls.Add(tabPending);
            flTabs.Controls.Add(tabShipping);
            flTabs.Controls.Add(tabProcessing);
            flTabs.Controls.Add(tabDone);
            flTabs.Controls.Add(tabCancel);

            // right group (Adv filter + Export)
            flRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            pnTop.Controls.Add(flRight);

            btnAdv = MakeButton("🔽  Lọc nâng cao", C_PANEL2, C_TEXT);
            btnExport = MakeButton("⬇  Xuất Excel", Color.FromArgb(0, 110, 70), Color.White);
            btnAdv.Margin = new Padding(0, 8, 10, 0);
            btnExport.Margin = new Padding(0, 8, 0, 0);
            flRight.Controls.Add(btnAdv);
            flRight.Controls.Add(btnExport);

            // underline bottom line
            pnTop.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawLine(pen, 0, pnTop.Height - 1, pnTop.Width, pnTop.Height - 1);
            };

            // ===== GRID WRAP =====
            pnGridWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                Padding = new Padding(10)
            };
            tlp.Controls.Add(pnGridWrap, 0, 1);

            pnGridWrap.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnGridWrap.Width - 1, pnGridWrap.Height - 1);
            };

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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = false,          // để checkbox tick được
                MultiSelect = true,        // tick nhiều dòng cũng ok
                EditMode = DataGridViewEditMode.EditOnEnter,
                GridColor = Color.FromArgb(45, 45, 45),
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 32);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 42;

            dgv.DefaultCellStyle.BackColor = C_PANEL;
            dgv.DefaultCellStyle.ForeColor = C_TEXT;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowTemplate.Height = 46;

            // columns
            dgv.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Width = 42,
                HeaderText = "",
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã Đơn",
                DataPropertyName = "Code",
                Width = 120
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Thời gian",
                DataPropertyName = "TimeText",
                Width = 160
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Khách hàng",
                DataPropertyName = "Customer",
                Width = 260
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Sản phẩm",
                DataPropertyName = "Products",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tổng tiền",
                DataPropertyName = "TotalText",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Trạng thái",
                DataPropertyName = "Status",
                Width = 150
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "",
                DataPropertyName = "Actions",
                Width = 90
            });

            pnGridWrap.Controls.Add(dgv);

            // ===== BOTTOM =====
            pnBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 8, 4, 0)
            };
            tlp.Controls.Add(pnBottom, 0, 2);

            lbInfo = new Label
            {
                AutoSize = true,
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9f),
                Text = "Hiển thị 0",
                Location = new Point(0, 10)
            };
            pnBottom.Controls.Add(lbInfo);

            flPager = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            pnBottom.Controls.Add(flPager);

            pgPrev = MakePager("◀");
            pg1 = MakePager("1");
            pg2 = MakePager("2");
            pg3 = MakePager("3");
            pgNext = MakePager("▶");

            flPager.Controls.Add(pgPrev);
            flPager.Controls.Add(pg1);
            flPager.Controls.Add(pg2);
            flPager.Controls.Add(pg3);
            flPager.Controls.Add(pgNext);

            SetTabActive(tabAll);
        }

        private Button MakeButton(string text, Color back, Color fore)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(14, 0, 14, 0),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 55);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Button MakeTab(string text)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(14, 0, 14, 0),
                Margin = new Padding(0, 8, 0, 0),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 55);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private Button MakePager(string text)
        {
            var b = new Button
            {
                Text = text,
                Width = 34,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(6, 0, 0, 0),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 55);
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        // ===================== EVENTS =====================
        private void Wire()
        {
            btnNew.Click += (s, e) => MessageBox.Show("Tạo mới đơn hàng: bạn làm form riêng sau.");
            btnAdv.Click += (s, e) => MessageBox.Show("Lọc nâng cao: bạn có thể làm dialog lọc theo ngày/khách/giá.");
            btnExport.Click += (s, e) => ExportCsv();

            tabAll.Click += (s, e) => { _filter = null; SetTabActive(tabAll); _page = 1; ApplyFilterAndPaging(); };
            tabPending.Click += (s, e) => { _filter = OrderStatus.Pending; SetTabActive(tabPending); _page = 1; ApplyFilterAndPaging(); };
            tabShipping.Click += (s, e) => { _filter = OrderStatus.Shipping; SetTabActive(tabShipping); _page = 1; ApplyFilterAndPaging(); };
            tabProcessing.Click += (s, e) => { _filter = OrderStatus.Processing; SetTabActive(tabProcessing); _page = 1; ApplyFilterAndPaging(); };
            tabDone.Click += (s, e) => { _filter = OrderStatus.Completed; SetTabActive(tabDone); _page = 1; ApplyFilterAndPaging(); };
            tabCancel.Click += (s, e) => { _filter = OrderStatus.Cancelled; SetTabActive(tabCancel); _page = 1; ApplyFilterAndPaging(); }; // ✅

            // ===== pager chuẩn: không tăng vô hạn =====
            pgPrev.Click += (s, e) => { if (_page > 1) { _page--; ApplyFilterAndPaging(); } };
            pg1.Click += (s, e) => { if (_totalPages >= 1) { _page = 1; ApplyFilterAndPaging(); } };
            pg2.Click += (s, e) => { if (_totalPages >= 2) { _page = 2; ApplyFilterAndPaging(); } };
            pg3.Click += (s, e) => { if (_totalPages >= 3) { _page = 3; ApplyFilterAndPaging(); } };
            pgNext.Click += (s, e) => { if (_page < _totalPages) { _page++; ApplyFilterAndPaging(); } }; // ✅ không vượt totalPages

            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;
            Resize += (s, e) => dgv.Invalidate();
        }

        private void SetTabActive(Button active)
        {
            Button[] all = { tabAll, tabPending, tabShipping, tabProcessing, tabDone, tabCancel };
            for (int i = 0; i < all.Length; i++)
            {
                var b = all[i];
                bool on = (b == active);
                b.BackColor = on ? C_ACCENT : Color.FromArgb(28, 28, 28);
                b.ForeColor = on ? Color.White : C_TEXT;
                b.FlatAppearance.BorderColor = on ? C_ACCENT : Color.FromArgb(55, 55, 55);
            }
        }

        // ===================== DATA =====================
        private void SeedDemo()
        {
            _all = new List<OrderRow>
            {
                new OrderRow("#ORD-9281","14:30, Hôm nay","Nguyễn Văn A","Áo thun nam Basic (+2)", 2500000, OrderStatus.Shipping),
                new OrderRow("#ORD-9280","10:15, Hôm nay","Trần Thị B","Váy hoa nhí", 850000, OrderStatus.Pending),
                new OrderRow("#ORD-9275","18:00, Hôm qua","Phạm Minh C","Mũ lưỡi trai", 450000, OrderStatus.Completed),
                new OrderRow("#ORD-9270","10:00, Hôm qua","Vũ Thị D","Đầm dạ hội", 1200000, OrderStatus.Cancelled),
            };

            
        }

        private void ApplyFilterAndPaging()
        {
            IEnumerable<OrderRow> q = _all;

            if (_filter.HasValue)
                q = q.Where(x => x.Status == _filter.Value);

            _view = q.ToList();

            int total = _view.Count;
            _totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)_pageSize));
            if (_page > _totalPages) _page = _totalPages;
            if (_page < 1) _page = 1;

            int skip = (_page - 1) * _pageSize;
            var pageItems = _view.Skip(skip).Take(_pageSize).ToList();

            dgv.DataSource = null;
            dgv.DataSource = pageItems;

            int from = total == 0 ? 0 : skip + 1;
            int to = total == 0 ? 0 : Math.Min(skip + _pageSize, total);
            lbInfo.Text = string.Format("Hiển thị {0}-{1} trên {2} bản ghi", from, to, total);

            SetPagerActive(_totalPages);
        }

        private void SetPagerActive(int totalPages)
        {
            // hiện 1..3 trang đầu (demo), và khóa prev/next đúng chuẩn
            Button[] pages = { pg1, pg2, pg3 };
            for (int i = 0; i < pages.Length; i++)
            {
                int pageNum = i + 1;
                pages[i].Visible = pageNum <= totalPages;
                pages[i].BackColor = (pageNum == _page) ? C_ACCENT : Color.FromArgb(28, 28, 28);
                pages[i].ForeColor = (pageNum == _page) ? Color.White : C_TEXT;
                pages[i].FlatAppearance.BorderColor = (pageNum == _page) ? C_ACCENT : Color.FromArgb(55, 55, 55);
            }

            // ✅ disable đúng chuẩn
            pgPrev.Enabled = _page > 1;
            pgNext.Enabled = _page < totalPages;

            pgPrev.BackColor = pgPrev.Enabled ? Color.FromArgb(28, 28, 28) : Color.FromArgb(22, 22, 22);
            pgNext.BackColor = pgNext.Enabled ? Color.FromArgb(28, 28, 28) : Color.FromArgb(22, 22, 22);

            pgPrev.ForeColor = pgPrev.Enabled ? C_TEXT : C_MUTED;
            pgNext.ForeColor = pgNext.Enabled ? C_TEXT : C_MUTED;

            pgPrev.FlatAppearance.BorderColor = pgPrev.Enabled ? Color.FromArgb(55, 55, 55) : Color.FromArgb(35, 35, 35);
            pgNext.FlatAppearance.BorderColor = pgNext.Enabled ? Color.FromArgb(55, 55, 55) : Color.FromArgb(35, 35, 35);
        }

        // ===================== GRID CUSTOM DRAW =====================
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Code column: màu xanh giống link
            if (e.ColumnIndex == COL_CODE)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string text = Convert.ToString(e.FormattedValue);
                var r = e.CellBounds;
                r.Inflate(-8, -6);

                using (var f = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, text, f, r, C_ACCENT, TextFormatFlags.Left | TextFormatFlags.Top);
                }

                // underline nhẹ
                using (var pen = new Pen(Color.FromArgb(90, C_ACCENT)))
                {
                    int y = e.CellBounds.Bottom - 10;
                    e.Graphics.DrawLine(pen, e.CellBounds.Left + 8, y, e.CellBounds.Left + 68, y);
                }
                return;
            }

            // Total column: bold
            if (e.ColumnIndex == COL_TOTAL)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string text = Convert.ToString(e.FormattedValue);
                var r = e.CellBounds;
                r.Inflate(-8, -6);

                using (var f = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, text, f, r, Color.White,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            // Status column: badge
            if (e.ColumnIndex == COL_STATUS)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                var row = dgv.Rows[e.RowIndex].DataBoundItem as OrderRow;
                if (row == null) return;

                string text = row.StatusText;
                Color back = GetStatusColor(row.Status);

                var b = e.CellBounds;
                int bw = 110;
                int bh = 24;
                int x = b.Left + (b.Width - bw) / 2;
                int y = b.Top + (b.Height - bh) / 2;
                var rect = new Rectangle(x, y, bw, bh);

                using (var br = new SolidBrush(Color.FromArgb(35, back)))
                    e.Graphics.FillRectangle(br, rect);
                using (var pen = new Pen(back))
                    e.Graphics.DrawRectangle(pen, rect);

                TextRenderer.DrawText(e.Graphics, text, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    rect, back, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // Actions column: vẽ 2 icon (sửa + in)
            if (e.ColumnIndex == COL_ACTION)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                var b = e.CellBounds;
                int icon = 18;
                int gap = 12;
                int y = b.Top + (b.Height - icon) / 2;

                // icon 1
                var r1 = new Rectangle(b.Left + 18, y, icon, icon);
                // icon 2
                var r2 = new Rectangle(r1.Right + gap, y, icon, icon);

                DrawPencil(e.Graphics, r1, C_MUTED);
                DrawPrinter(e.Graphics, r2, C_MUTED);
                return;
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex].DataBoundItem as OrderRow;
            if (row == null) return;

            if (e.ColumnIndex == COL_CODE)
            {
                MessageBox.Show("Mở chi tiết đơn: " + row.Code);
                return;
            }

            if (e.ColumnIndex == COL_ACTION)
            {
                // xác định click vào icon nào theo X
                var cell = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                int x = Cursor.Position.X;
                var p = dgv.PointToClient(new Point(x, 0));

                int icon = 18;
                int gap = 12;
                int r1Left = cell.Left + 18;
                int r2Left = r1Left + icon + gap;

                if (p.X >= r1Left && p.X <= r1Left + icon)
                {
                    MessageBox.Show("Sửa đơn: " + row.Code);
                }
                else if (p.X >= r2Left && p.X <= r2Left + icon)
                {
                    MessageBox.Show("In hóa đơn: " + row.Code);
                }
            }
        }

        private static Color GetStatusColor(OrderStatus st)
        {
            switch (st)
            {
                case OrderStatus.Shipping: return S_SHIPPING;
                case OrderStatus.Pending: return S_PENDING;
                case OrderStatus.Processing: return S_PROCESS;
                case OrderStatus.Completed: return S_DONE;
                case OrderStatus.Cancelled: return S_CANCEL;
                default: return C_MUTED;
            }
        }

        // ===== simple vector icons =====
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

        private void DrawPrinter(Graphics g, Rectangle r, Color c)
        {
            using (var pen = new Pen(c, 2))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // body
                g.DrawRectangle(pen, r.Left + 3, r.Top + 7, r.Width - 6, r.Height - 9);
                // top
                g.DrawRectangle(pen, r.Left + 5, r.Top + 2, r.Width - 10, 6);
                // paper
                g.DrawLine(pen, r.Left + 6, r.Top + 12, r.Right - 6, r.Top + 12);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }
        }

        // ===================== EXPORT =====================
        private void ExportCsv()
        {
            try
            {
                using (var save = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "orders.csv" })
                {
                    if (save.ShowDialog() != DialogResult.OK) return;

                    // export đang xem theo filter (toàn bộ, không chỉ page)
                    IEnumerable<OrderRow> src = _all;
                    if (_filter.HasValue) src = src.Where(x => x.Status == _filter.Value);

                    using (var sw = new StreamWriter(save.FileName, false, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("Code,Time,Customer,Products,Total,Status");
                        foreach (var o in src)
                        {
                            sw.WriteLine(string.Join(",",
                                Csv(o.Code),
                                Csv(o.TimeText),
                                Csv(o.Customer),
                                Csv(o.Products),
                                Csv(o.TotalText),
                                Csv(o.StatusText)));
                        }
                    }

                    MessageBox.Show("Xuất CSV xong: " + save.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message);
            }
        }

        private static string Csv(string s)
        {
            if (s == null) return "\"\"";
            s = s.Replace("\"", "\"\"");
            return "\"" + s + "\"";
        }

        public int GetTotalOrdersCount()
        {
            return _all.Count; // hoặc list bạn đang dùng
        }


        // ===================== MODEL =====================
        private enum OrderStatus
        {
            Pending,
            Shipping,
            Processing,
            Completed,
            Cancelled
        }

        private sealed class OrderRow
        {
            public string Code { get; private set; }
            public string TimeText { get; private set; }
            public string Customer { get; private set; }
            public string Products { get; private set; }
            public decimal Total { get; private set; }
            public OrderStatus Status { get; private set; }

            public string TotalText
            {
                get
                {
                    var nfi = (NumberFormatInfo)CultureInfo.GetCultureInfo("vi-VN").NumberFormat.Clone();
                    nfi.NumberGroupSeparator = ".";
                    return Total.ToString("#,0", nfi) + "đ";
                }
            }

            public string StatusText
            {
                get
                {
                    switch (Status)
                    {
                        case OrderStatus.Shipping: return "Vận chuyển";
                        case OrderStatus.Pending: return "Chờ xử lý";
                        case OrderStatus.Completed: return "Hoàn thành";
                        case OrderStatus.Cancelled: return "Đã hủy";
                        default: return "";
                    }
                }
            }

            public string Actions { get { return ""; } }

            public OrderRow(string code, string time, string customer, string products, decimal total, OrderStatus st)
            {
                Code = code;
                TimeText = time;
                Customer = customer;
                Products = products;
                Total = total;
                Status = st;
            }
        }
    }
}
