using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class MainForm : Form
    {

        // ===== COLORS =====
        private static readonly Color C_TITLE = Color.FromArgb(8, 8, 8);
        private static readonly Color C_BG = Color.FromArgb(18, 18, 18);
        private static readonly Color C_SIDEBAR = Color.FromArgb(26, 26, 26);
        private static readonly Color C_MAIN = Color.FromArgb(30, 30, 30);
        private static readonly Color C_BORDER = Color.FromArgb(55, 55, 55);
        private static readonly Color C_TEXT = Color.Gainsboro;
        private static readonly Color C_MUTED = Color.FromArgb(140, 140, 140);
        private static readonly Color C_ACTIVE = Color.FromArgb(18, 92, 170);
        private static readonly Color C_ACCENT = Color.FromArgb(0, 120, 215);

        // ===== ROOT =====
        private Panel pnTitle;
        private Panel pnRoot;
        private StatusStrip status;
        private ToolStripStatusLabel stReady, stUser, stServer, stCaps;

        // ===== LAYOUT =====
        private TableLayoutPanel tlpRoot;
        private Panel pnSidebar;
        private Panel pnMainArea;
        private Panel pnContent;

        // ===== SIDEBAR PARTS =====
        private Panel pnSideHeader;
        private Panel pnSideFooter;
        private Panel pnSideBody;
        private Panel pnModulesWrap;
        private Panel pnSystemWrap;

        private FlowLayoutPanel flModules;
        private FlowLayoutPanel flSystem;


        // ===== TITLE BUTTONS =====
        private Button btnMin;
        private Button btnClose;

        // ===== NAV ITEMS =====
        private Panel navDashboard, navProducts, navOrders, navCustomers, navInventory, navReports, navSettings;
        private Label badgeOrders;

        // ===== PAGES =====
        private UserControl _current;
        private UcDashboard _ucDashboard;
        private UcProducts _ucProducts;

        // ===== DRAG WINDOW =====
        private bool _dragging;
        private Point _dragStart;

        // ===== NAV TAG =====
        private sealed class NavInfo
        {
            public bool IsSection;
            public bool IsActive;
            public Action ClickAction;
        }

        private UcOrders _ucOrders;
        private void ShowOrders()
        {
            if (_ucOrders == null) _ucOrders = new UcOrders();

            LoadPage(_ucOrders);

            // cập nhật badge sau khi load page
            int count = _ucOrders.GetTotalOrdersCount();  // hàm bạn sẽ tạo bên UcOrders
            SetOrdersBadge(count);
        }


        private UcCustomers _ucCustomers;
        private void ShowCustomers()
        {
            if (_ucCustomers == null) _ucCustomers = new UcCustomers();
            LoadPage(_ucCustomers);
        }

        private UcInventory _ucInventory;

        private void ShowInventory()
        {
            if (_ucInventory == null) _ucInventory = new UcInventory();
            LoadPage(_ucInventory);
        }

        private UcReports _ucReports;

        private void ShowReports()
        {
            if (_ucReports == null) _ucReports = new UcReports();
            LoadPage(_ucReports);
        }

        private UcSettings _ucSettings;

        private void ShowUcSettings()
        {
            if (_ucSettings == null) _ucSettings = new UcSettings();
            LoadPage(_ucSettings);
        }


        public MainForm()
        {
            InitializeComponent();
            BuildUI();
            WireEvents();
            AddChatBubble();
            ShowDashboard();
            SetActiveNav(navDashboard);
            AutoScaleMode = AutoScaleMode.None;

        }

        // =========================
        // BUILD UI (FULL)
        // =========================
        private void BuildUI()
        {
            SuspendLayout();
            Controls.Clear();

            // ===== FORM =====
            Text = "Fashion Shop Manager - Dashboard";
            StartPosition = FormStartPosition.Manual;
            BackColor = C_BG;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 480);
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;

            // ===== ROOT FILL (giữa title và status) =====
            pnRoot = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };


            // ===== TITLE BAR (ĐEN + – X) =====
            pnTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = C_TITLE
            };

            var lbTitle = new Label
            {
                AutoSize = true,
                Text = "Fashion Shop Manager",
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(12, 9)
            };
            pnTitle.Controls.Add(lbTitle);

            // line xanh nhỏ bên trái (giống gốc)
            var accentLine = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 3,
                BackColor = C_ACCENT
            };
            pnTitle.Controls.Add(accentLine);

            btnClose = new System.Windows.Forms.Button
            {
                Text = "✕",
                Width = 46,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(18, 18, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false,
                FlatAppearance = { BorderColor = Color.FromArgb(40, 40, 40), BorderSize = 1 }
            };
            pnTitle.Controls.Add(btnClose);

            btnMin = new System.Windows.Forms.Button
            {
                Text = "—",
                Width = 46,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(18, 18, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false,
                FlatAppearance = { BorderColor = Color.FromArgb(40, 40, 40), BorderSize = 1 }
            };
            pnTitle.Controls.Add(btnMin);

            pnTitle.Resize += (s, e) =>
            {
                btnClose.Location = new Point(pnTitle.ClientSize.Width - 54, 4);
                btnMin.Location = new Point(pnTitle.ClientSize.Width - 104, 4);
            };
            btnClose.Location = new Point(pnTitle.ClientSize.Width - 54, 4);
            btnMin.Location = new Point(pnTitle.ClientSize.Width - 104, 4);

            // ===== MAIN TABLE (SIDEBAR | SEP | MAIN) =====
            tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1)); // line
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            pnRoot.Controls.Add(tlpRoot);

            // ===== SIDEBAR =====
            pnSidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_SIDEBAR,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tlpRoot.Controls.Add(pnSidebar, 0, 0);

            // separator
            var sep = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BORDER,
                Margin = new Padding(0)
            };
            tlpRoot.Controls.Add(sep, 1, 0);

            // ===== MAIN AREA =====
            pnMainArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_MAIN,
                Margin = new Padding(0),
                Padding = new Padding(6, 10, 12, 6)
            };
            tlpRoot.Controls.Add(pnMainArea, 2, 0);

            

            // ===== CONTENT =====
            pnContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_MAIN,
                Margin = new Padding(0),
                Padding = new Padding(0, 0, 0, 0)
            };
            pnMainArea.Controls.Add(pnContent);

            // ===== SIDEBAR PARTS =====
            pnSideHeader = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(16, 10, 16, 10) };
            pnSideFooter = new Panel { Dock = DockStyle.Bottom, Height = 86, Padding = new Padding(12, 10, 12, 10) };
            pnSideBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

            pnSidebar.Controls.Add(pnSideBody);
            pnSidebar.Controls.Add(pnSideFooter);
            pnSidebar.Controls.Add(pnSideHeader);

            BuildSideFooter();
            BuildSideBody();

            Controls.Add(pnRoot);
            Controls.Add(status);
            Controls.Add(pnTitle);

            ResumeLayout();
        }

        private void BuildSideBody()
        {
            pnSideBody.Controls.Clear();

            pnModulesWrap = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 0, 0),
                BackColor = C_SIDEBAR
            };

            pnSystemWrap = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 350,
                Padding = new Padding(0, 2, 0, 2),
                BackColor = C_SIDEBAR
            };

            pnSideBody.Controls.Add(pnModulesWrap);
            pnSideBody.Controls.Add(pnSystemWrap);

            flModules = MakeFlowMenu();
            flSystem = MakeFlowMenu();
            flSystem.AutoScroll = false;

            pnModulesWrap.Controls.Add(flModules);
            pnSystemWrap.Controls.Add(flSystem);

            AddSectionLabel(flModules, "MODULES");

            navDashboard = MakeNavItem("Tổng quan", "\uE80F", ShowDashboard);
            navProducts = MakeNavItem("Quản lý Sản phẩm", "\uE719", ShowProducts);
            navOrders = MakeNavItem("Đơn hàng", "\uE7BF", ShowOrders);
            navCustomers = MakeNavItem("Khách hàng", "\uE716", ShowCustomers);
            navInventory = MakeNavItem("Kho hàng", "\uE7B8", ShowInventory);
            navReports = MakeNavItem("Báo cáo_ Thống kê", "\uE9D2", ShowReports);

            flModules.Controls.Add(navDashboard);
            flModules.Controls.Add(navProducts);
            flModules.Controls.Add(navOrders);
            flModules.Controls.Add(navCustomers);
            flModules.Controls.Add(navInventory);
            flModules.Controls.Add(navReports);

            badgeOrders = new Label
            {
                AutoSize = false,
                Size = new Size(22, 22),
                Text = "0",
                Visible = false, // ẩn trước, có dữ liệu sẽ bật lên
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(220, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            navOrders.Controls.Add(badgeOrders);
            navOrders.Resize += (s, e) => PlaceBadge(navOrders, badgeOrders);
            PlaceBadge(navOrders, badgeOrders);

            AddSectionLabel(flSystem, "Hệ thống");
            navSettings = MakeNavItem("Cài đặt", "\uE713", ShowUcSettings);
            flSystem.Controls.Add(navSettings);

            pnModulesWrap.Resize += (s, e) => FixMenuItemWidth(flModules);
            pnSystemWrap.Resize += (s, e) => FixMenuItemWidth(flSystem);
            FixMenuItemWidth(flModules);
            FixMenuItemWidth(flSystem);
        }

        public void SetOrdersBadge(int count)
        {
            if (badgeOrders == null) return;

            badgeOrders.Text = count.ToString();
            badgeOrders.Visible = count > 0;
            PlaceBadge(navOrders, badgeOrders);
        }


        private static FlowLayoutPanel MakeFlowMenu()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = C_SIDEBAR
            };
        }

        private void FixMenuItemWidth(FlowLayoutPanel fl)
        {
            if (fl == null) return;

            int w = fl.Parent.ClientSize.Width;
            for (int i = 0; i < fl.Controls.Count; i++)
            {
                if (fl.Controls[i] is Panel p)
                    p.Width = w;
            }
        }

        private void PlaceBadge(Panel item, Label badge)
        {
            if (item == null || badge == null) return;

            int x = item.ClientSize.Width - badge.Width - 12;
            int y = (item.ClientSize.Height - badge.Height) / 2;   // << căn giữa theo chiều dọc
            badge.Location = new Point(x, y);
            badge.BringToFront();
        }


        

        private void BuildSideFooter()
        {
            pnSideFooter.Controls.Clear();

            var avatar = new Panel
            {
                Size = new Size(42, 42),
                BackColor = Color.FromArgb(45, 45, 45),
                Location = new Point(2, 18)
            };
            pnSideFooter.Controls.Add(avatar);

            var lbName = new Label
            {
                AutoSize = true,
                Text = "Vũ Đức Trọng",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(52, 18)
            };
            pnSideFooter.Controls.Add(lbName);

            var lbRole = new Label
            {
                AutoSize = true,
                Text = "Administrator",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(52, 38)
            };
            pnSideFooter.Controls.Add(lbRole);

            var line = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = C_BORDER
            };
            pnSideFooter.Controls.Add(line);
            line.BringToFront();
        }

        private void AddSectionLabel(FlowLayoutPanel parent, string text)
        {
            var wrap = new Panel
            {
                Height = 28,
                Width = parent.Parent.ClientSize.Width,
                BackColor = C_SIDEBAR,
                Margin = new Padding(0, 4, 0, 4),
                Padding = new Padding(12, 4, 12, 0),
                Tag = new NavInfo { IsSection = true }
            };

            var lb = new Label
            {
                Dock = DockStyle.Fill,                 // << quan trọng: Dock để ăn Padding
                TextAlign = ContentAlignment.TopLeft,
                Text = text,
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = false
            };

            wrap.Controls.Add(lb);
            parent.Controls.Add(wrap);
        }

        

        private void AddChatBubble()
        {
            // Tạo PictureBox
            var picChat = new PictureBox
            {
                Size = new Size(70, 70),
                Image = Properties.Resources.chatbox, // ← đổi tên theo ảnh bạn nhúng
                SizeMode = PictureBoxSizeMode.Zoom,   // ✅ giữ tỉ lệ, vừa khung
                Cursor = Cursors.Hand,
                Location = new Point(80, 700),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            // 🔘 Làm tròn PictureBox thành hình tròn
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddEllipse(0, 0, picChat.Width, picChat.Height);
                picChat.Region = new Region(path);
            }

            // Gắn sự kiện click
            picChat.Click += (s, e) =>
            {
                var frm = new FrmChatBot();
                frm.Show();
            };

            
            // Thêm vào form
            this.Controls.Add(picChat);
            picChat.BringToFront();
        }

        private Panel MakeNavItem(string text, string glyph, Action onClick)
        {
            var item = new Panel
            {
                Height = 44,
                Width = 260,
                BackColor = C_SIDEBAR,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 2),
                Padding = new Padding(12, 0, 10, 0),
                Tag = new NavInfo { ClickAction = onClick }
            };


            var ico = new Label
            {
                AutoSize = false,
                Width = 26,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe MDL2 Assets", 14f),
                ForeColor = C_TEXT,
                Text = glyph
            };

            var lb = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = C_TEXT,
                Text = "  " + text
            };

            item.Controls.Add(lb);
            item.Controls.Add(ico);

            item.MouseEnter += (s, e) => NavHover(item, true);
            item.MouseLeave += (s, e) => NavHover(item, false);

            foreach (Control c in item.Controls)
            {
                c.Cursor = Cursors.Hand;
                c.Click += (s, e) => NavClicked(item);
                c.MouseEnter += (s, e) => NavHover(item, true);
                c.MouseLeave += (s, e) => NavHover(item, false);
            }

            item.Click += (s, e) => NavClicked(item);
            return item;
        }

        private void NavHover(Panel item, bool enter)
        {
            if (item == null) return;

            if (item.Tag is NavInfo info && info.IsActive)
                return;

            item.BackColor = enter ? Color.FromArgb(34, 34, 34) : C_SIDEBAR;
        }

        private void NavClicked(Panel item)
        {
            if (item == null) return;

            SetActiveNav(item);

            if (item.Tag is NavInfo info && info.ClickAction != null)
                info.ClickAction();
        }

        private void SetActiveNav(Panel active)
        {
            SetActiveInFlow(flModules, active);
            SetActiveInFlow(flSystem, active);
        }

        private void SetActiveInFlow(FlowLayoutPanel fl, Panel active)
        {
            if (fl == null) return;

            for (int i = 0; i < fl.Controls.Count; i++)
            {
                if (!(fl.Controls[i] is Panel p)) continue;

                NavInfo info = null;
                if (p.Tag is NavInfo tmp) info = tmp;

                if (info != null && info.IsSection) continue;

                bool on = (p == active);
                if (info != null) info.IsActive = on;

                p.BackColor = on ? C_ACTIVE : C_SIDEBAR;

                foreach (Control child in p.Controls)
                {
                    if (child is Label lb)
                        lb.ForeColor = on ? Color.White : C_TEXT;
                }
            }
        }


        // =========================
        // EVENTS
        // =========================
        private void WireEvents()
        {
            btnClose.Click += (s, e) => Close();
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            pnTitle.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                _dragging = true;
                _dragStart = new Point(e.X, e.Y);
            };
            pnTitle.MouseMove += (s, e) =>
            {
                if (!_dragging) return;
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _dragStart.X, p.Y - _dragStart.Y);
            };
            pnTitle.MouseUp += (s, e) => _dragging = false;

        }

        // =========================
        // PAGES
        // =========================

        private void ShowDashboard()
        {
            if (_ucDashboard == null) _ucDashboard = new UcDashboard();
            LoadPage(_ucDashboard);
        }

        private void ShowProducts()
        {
            if (_ucProducts == null) _ucProducts = new UcProducts();
            LoadPage(_ucProducts);
        }

        private void LoadPage(UserControl uc)
        {
            if (uc == null) return;
            if (_current == uc) return;

            pnContent.SuspendLayout();
            pnContent.Controls.Clear();

            uc.Dock = DockStyle.Fill;

            uc.Margin = new Padding(0);     // << thêm
            uc.Padding = new Padding(0);    // << thêm (nếu bạn muốn control tự xử lý padding)

            pnContent.Controls.Add(uc);

            pnContent.ResumeLayout();
            _current = uc;


        }

    }

    public interface IReloadable
    {
        void ReloadData();
    }
}
