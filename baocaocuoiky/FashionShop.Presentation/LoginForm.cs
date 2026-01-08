using FashionShop.Business;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public partial class LoginForm : Form
    {

        // ===== COLORS =====
        private static readonly Color C_BG = Color.FromArgb(12, 12, 12);
        private static readonly Color C_PANEL = Color.FromArgb(18, 18, 18);
        private static readonly Color C_PANEL2 = Color.FromArgb(22, 22, 22);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_MUTED = Color.FromArgb(160, 160, 160);
        private static readonly Color C_PURPLE = Color.FromArgb(146, 66, 255);

        // ===== ROOT =====
        private TableLayoutPanel root;
        private Panel pnTitle, pnLeft, pnRight, pnStatus;

        // Title buttons
        private Button btnMin, btnClose;

        // Left
        private PictureBox picLeft;

        // Right
        private Label lbH1, lbH2, lbUserLabel, lbPassLabel;
        private TextBox tbUser, tbPass;
        private Button btnLogin, btnTogglePass;
        private CheckBox chkRemember;
        private LinkLabel lnkForgot, lnkSignup;

        // Status
        private Label lbStatus;

        // Drag form
        private bool _dragging;
        private Point _dragStart;

        // Password placeholder state
        private bool _passPlaceholder = true;



        public LoginForm()
        {
            InitializeComponent();
            BuildUI();


        }

        private void BuildUI()
        {
            SuspendLayout();
            Controls.Clear();

            // ===== FORM =====
            Text = "Fashion Shop Manager - Đăng nhập";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = C_BG;
            Size = new Size(1100, 650);
            MinimumSize = new Size(980, 600);
            FormBorderStyle = FormBorderStyle.None;

            // ===== TITLE BAR =====
            pnTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(8, 8, 8)
            };
            pnTitle.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(30, 30, 30)))
                    e.Graphics.DrawLine(pen, 0, pnTitle.Height - 1, pnTitle.Width, pnTitle.Height - 1);
            };
            Controls.Add(pnTitle);

            var lbTitle = new Label
            {
                AutoSize = true,
                Text = "Fashion Shop Manager - Đăng nhập",
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(12, 9)
            };
            pnTitle.Controls.Add(lbTitle);

            btnClose = new Button
            {
                Text = "✕",
                Width = 46,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(18, 18, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(40, 40, 40);
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(35, 10, 10);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(18, 18, 18);
            pnTitle.Controls.Add(btnClose);

            btnMin = new Button
            {
                Text = "—",
                Width = 46,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(18, 18, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false
            };
            btnMin.FlatAppearance.BorderColor = Color.FromArgb(40, 40, 40);
            btnMin.FlatAppearance.BorderSize = 1;
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;
            btnMin.MouseEnter += (s, e) => btnMin.BackColor = Color.FromArgb(28, 28, 28);
            btnMin.MouseLeave += (s, e) => btnMin.BackColor = Color.FromArgb(18, 18, 18);
            pnTitle.Controls.Add(btnMin);

            void LayoutTitleButtons()
            {
                btnClose.Location = new Point(pnTitle.ClientSize.Width - 60, 4);
                btnMin.Location = new Point(pnTitle.ClientSize.Width - 110, 4);
            }
            pnTitle.SizeChanged += (s, e) => LayoutTitleButtons();
            Shown += (s, e) => LayoutTitleButtons();

            // drag window bằng title bar
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

            // ===== STATUS BAR =====
            pnStatus = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                BackColor = Color.FromArgb(10, 10, 10)
            };
            lbStatus = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(120, 255, 120),
                Font = new Font("Segoe UI", 8.5f),
                Text = "●  SYSTEM READY",
                Location = new Point(12, 6)
            };
            pnStatus.Controls.Add(lbStatus);
            Controls.Add(pnStatus);

            // ===== ROOT 2 CỘT =====
            root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            root.ColumnStyles.Clear();
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));

            root.RowStyles.Clear();
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // <<< QUAN TRỌNG

            Controls.Add(root);

            // ===== LEFT (ẢNH) =====
            pnLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            picLeft = new PictureBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.trangtri
            };

            // phủ tím bằng Paint (đừng dùng overlay Panel)
            picLeft.Paint += (s, e) =>
            {
                using (var br = new SolidBrush(Color.FromArgb(110, 70, 0, 120)))
                    e.Graphics.FillRectangle(br, picLeft.ClientRectangle);
            };

            pnLeft.Controls.Add(picLeft);

            // add đúng vào cột 0
            root.Controls.Add(pnLeft, 0, 0);

            // ===== RIGHT (FORM LOGIN) =====
            pnRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL2,
                Padding = new Padding(36),
                Margin = new Padding(0)
            };

            // add đúng vào cột 1
            root.Controls.Add(pnRight, 1, 0);

            var pnCard = new Panel
            {
                Dock = DockStyle.None,
                Width = 560,
                Height = 420,
                BackColor = C_PANEL,
                Padding = new Padding(26),
            };
            pnCard.Paint += DrawBorder;
            pnRight.Controls.Add(pnCard);

            // canh giữa card
            void CenterCard()
            {
                pnCard.Left = Math.Max(20, (pnRight.ClientSize.Width - pnCard.Width) / 2);
                pnCard.Top = Math.Max(20, (pnRight.ClientSize.Height - pnCard.Height) / 2);
            }
            pnRight.Resize += (s, e) => CenterCard();
            Shown += (s, e) => CenterCard();

            lbH1 = new Label
            {
                AutoSize = true,
                Text = "Đăng nhập hệ thống",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(0, 0)
            };
            pnCard.Controls.Add(lbH1);

            lbH2 = new Label
            {
                AutoSize = true,
                Text = "Vui lòng nhập thông tin xác thực",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 10),
                Location = new Point(2, 40)
            };
            pnCard.Controls.Add(lbH2);

            int y = 84;

            lbUserLabel = MakeFieldLabel("TÊN ĐĂNG NHẬP", 0, y);
            pnCard.Controls.Add(lbUserLabel);
            y += 22;

            tbUser = MakeInput(0, y, pnCard.Width - 52);
            SetUserPlaceholder();
            tbUser.Enter += (s, e) =>
            {
                if (tbUser.ForeColor == Color.Gray)
                {
                    tbUser.Text = "";
                    tbUser.ForeColor = Color.Gainsboro;
                }
            };
            tbUser.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tbUser.Text)) SetUserPlaceholder();
            };
            pnCard.Controls.Add(tbUser);
            y += 62;

            lbPassLabel = MakeFieldLabel("MẬT KHẨU", 0, y);
            pnCard.Controls.Add(lbPassLabel);
            y += 22;

            // ===== PASSWORD (SAFE) =====
            tbPass = MakeInput(0, y, pnCard.Width - 52 - 46);
            pnCard.Controls.Add(tbPass);

            SetPassPlaceholder();
            tbPass.Enter += (s, e) =>
            {
                if (_passPlaceholder) ClearPassPlaceholder();
            };
            tbPass.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tbPass.Text)) SetPassPlaceholder();
            };

            btnTogglePass = new Button
            {
                Width = 46,
                Height = tbPass.Height,
                Left = tbPass.Right + 6,
                Top = tbPass.Top,
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 26, 26),
                ForeColor = Color.Gainsboro,
                TabStop = false
            };
            btnTogglePass.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btnTogglePass.FlatAppearance.BorderSize = 1;
            btnTogglePass.Click += (s, e) =>
            {
                if (_passPlaceholder) return; 
                tbPass.UseSystemPasswordChar = !tbPass.UseSystemPasswordChar;
            };
            pnCard.Controls.Add(btnTogglePass);
            btnTogglePass.BringToFront();

            y += 58;

            chkRemember = new CheckBox
            {
                AutoSize = true,
                Text = "Ghi nhớ đăng nhập",
                ForeColor = Color.Gainsboro,
                Location = new Point(2, y + 2)
            };
            pnCard.Controls.Add(chkRemember);

            lnkForgot = new LinkLabel
            {
                AutoSize = true,
                Text = "Quên mật khẩu?",
                LinkColor = C_PURPLE,
                ActiveLinkColor = C_PURPLE,
                VisitedLinkColor = C_PURPLE,
                Location = new Point(pnCard.Width - 26 - 120, y + 2)
            };
            pnCard.Controls.Add(lnkForgot);

            y += 38;

            btnLogin = new Button
            {
                Text = "ĐĂNG NHẬP",
                Width = pnCard.Width - 52,
                Height = 52,
                Left = 0,
                Top = y,
                FlatStyle = FlatStyle.Flat,
                BackColor = C_PURPLE,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            pnCard.Controls.Add(btnLogin);
            AcceptButton = btnLogin;

            y += 78;

            var lbNoAcc = new Label
            {
                AutoSize = true,
                Text = "Chưa có tài khoản?",
                ForeColor = C_MUTED,
                Location = new Point(2, y)
            };
            pnCard.Controls.Add(lbNoAcc);

            lnkSignup = new LinkLabel
            {
                AutoSize = true,
                Text = "Đăng ký mới",
                LinkColor = Color.Gainsboro,
                ActiveLinkColor = Color.White,
                VisitedLinkColor = Color.Gainsboro,
                Location = new Point(lbNoAcc.Right + 12, y)
            };
            pnCard.Controls.Add(lnkSignup);

            // ===== LOGIN CLICK =====
            btnLogin.Click += (s, e) =>
            {
                if (tbUser.ForeColor == Color.Gray || string.IsNullOrWhiteSpace(tbUser.Text) ||
                    tbPass.ForeColor == Color.Gray || string.IsNullOrWhiteSpace(tbPass.Text))
                {
                    MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu!");
                    return;
                }

                var auth = new AuthService();
                var user = auth.Login(tbUser.Text.Trim(), tbPass.Text.Trim());

                if (user == null)
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
                    return;
                }

                // Login thành công
                this.DialogResult = DialogResult.OK;
                this.Close();
            };


            ResumeLayout();
        }

        // ====== PLACEHOLDER HELPERS ======
        private void SetUserPlaceholder()
        {
            tbUser.ForeColor = Color.Gray;
            tbUser.Text = "Nhập tên người dùng...";
        }

        private void SetPassPlaceholder()
        {
            _passPlaceholder = true;
            tbPass.UseSystemPasswordChar = false;
            tbPass.ForeColor = Color.Gray;
            tbPass.Text = "Nhập mật khẩu...";
        }

        private void ClearPassPlaceholder()
        {
            _passPlaceholder = false;
            tbPass.Text = "";
            tbPass.ForeColor = Color.Gainsboro;
            tbPass.UseSystemPasswordChar = true;
        }

        private void DrawBorder(object sender, PaintEventArgs e)
        {
            if (!(sender is Panel p)) return;

            using (var pen = new Pen(C_BORDER))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            }
        }

        private Label MakeFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                ForeColor = Color.FromArgb(190, 190, 190),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(x, y)
            };
        }

        private TextBox MakeInput(int x, int y, int w)
        {
            return new TextBox
            {
                Left = x,
                Top = y,
                Width = w,
                Height = 36,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(26, 26, 26)
            };
        }
    }
}
