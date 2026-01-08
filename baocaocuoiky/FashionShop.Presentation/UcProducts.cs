using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FashionShop.Business;
using FashionShop.Data;

namespace FashionShop.Presentation
{
    public partial class UcProducts : UserControl, IReloadable
    {
        private static readonly Color C_BG = Color.FromArgb(20, 20, 20);
        private static readonly Color C_PANEL = Color.FromArgb(30, 30, 30);
        private static readonly Color C_BORDER = Color.FromArgb(70, 70, 70);
        private static readonly Color C_TEXT = Color.White;
        private static readonly Color C_MUTED = Color.FromArgb(160, 160, 160);
        private static readonly Color C_INPUT = Color.FromArgb(18, 18, 18);
        private static readonly Color C_BTN = Color.FromArgb(25, 25, 25);

        private TableLayoutPanel root;
        private Panel pnToolbar, pnLeft, pnRight;

        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnGo;
        private TextBox txtSearch;
        private DataGridView dgv;

        // Right detail
        private Label lbId;
        private TextBox txtName, txtSku, txtDesc;
        private ComboBox cboCategory, cboStatus;
        private NumericUpDown numPrice, numStock;
        private PictureBox pic1, pic2;
        private Panel pnAddImg;
        private Button btnCancel, btnSave;

        private readonly ProductService _svc = new ProductService();
        private readonly CategoryService _catSvc = new CategoryService();

        private int _selectedId = 0;
        private string _imagePath = null;

        public UcProducts()
        {
            InitializeComponent();
            BuildUI();

            Dock = DockStyle.Fill;
            BackColor = C_BG;

            Load += (s, e) =>
            {
                LoadCategories();
                LoadGrid();
            };
        }

        public void ReloadData()
        {
            LoadCategories();
            LoadGrid();
        }

        // ================== UI BUILD ==================
        private void BuildUI()
        {
            Controls.Clear();

            root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 2
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            pnToolbar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnLeft = new Panel { Dock = DockStyle.Fill, BackColor = C_PANEL, Padding = new Padding(10) };
            pnRight = new Panel { Dock = DockStyle.Fill, BackColor = C_PANEL, Padding = new Padding(12) };

            pnLeft.Paint += DrawBorder;
            pnRight.Paint += DrawBorder;

            root.Controls.Add(pnToolbar, 0, 0);
            root.SetColumnSpan(pnToolbar, 2);
            root.Controls.Add(pnLeft, 0, 1);
            root.Controls.Add(pnRight, 1, 1);

            BuildToolbar();
            BuildGrid();
            BuildDetail();
        }

        private void DrawBorder(object sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            if (p == null) return;
            using (var pen = new Pen(C_BORDER))
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        private void BuildToolbar()
        {
            var flLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            btnAdd = MakeTopButton("Thêm mới", 100);
            btnEdit = MakeTopButton("Sửa", 80);
            btnDelete = MakeTopButton("Xóa", 80);
            btnRefresh = MakeTopButton("Làm mới", 100);



            flLeft.Controls.Add(btnAdd);
            flLeft.Controls.Add(btnEdit);
            flLeft.Controls.Add(btnDelete);
            flLeft.Controls.Add(btnRefresh);

            var flRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };

            var lbFind = new Label
            {
                Text = "Tìm kiếm:",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 6, 6, 0)
            };

            txtSearch = new TextBox
            {
                Width = 220,
                Margin = new Padding(0, 2, 6, 0),
                BackColor = C_INPUT,
                ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnGo = MakeTopButton("🔍", 44);

            flRight.Controls.Add(lbFind);
            flRight.Controls.Add(txtSearch);
            flRight.Controls.Add(btnGo);

            pnToolbar.Controls.Add(flLeft);
            pnToolbar.Controls.Add(flRight);

            btnGo.Click += (s, e) => LoadGrid();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadGrid(); };

            btnRefresh.Click += (s, e) => { txtSearch.Text = ""; LoadGrid(); ClearDetail(); };
            btnAdd.Click += (s, e) => { ClearDetail(); if (txtName != null) txtName.Focus(); };
            btnEdit.Click += (s, e) => UpdateProduct();
            btnDelete.Click += (s, e) => DeleteProduct();
        }

        private Button MakeTopButton(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Height = 36,
                Width = width,
                FlatStyle = FlatStyle.Flat,
                BackColor = C_BTN,
                ForeColor = C_TEXT,
                Margin = new Padding(0, 0, 10, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TabStop = false
            };
            b.FlatAppearance.BorderColor = C_BORDER;
            b.FlatAppearance.BorderSize = 1;
            return b;
        }

        private void BuildGrid()
        {
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = C_PANEL,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_TEXT;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            dgv.DefaultCellStyle.BackColor = C_PANEL;
            dgv.DefaultCellStyle.ForeColor = C_TEXT;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 90, 160);
            dgv.DefaultCellStyle.SelectionForeColor = C_TEXT;

            dgv.GridColor = Color.FromArgb(45, 45, 45);
            dgv.RowTemplate.Height = 32;

            EnableDoubleBuffering(dgv);

            pnLeft.Controls.Add(dgv);

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                dgv.Rows[e.RowIndex].Selected = true;
                SyncSelectedId();
                if (_selectedId > 0) LoadDetail(_selectedId);
            };

            dgv.SelectionChanged += (s, e) =>
            {
                SyncSelectedId();
                if (_selectedId > 0) LoadDetail(_selectedId);
            };
        }

        private void EnableDoubleBuffering(DataGridView grid)
        {
            try
            {
                typeof(DataGridView).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.SetProperty,
                    null, grid, new object[] { true });
            }
            catch { }
        }

        private void BuildDetail()
        {
            pnRight.Controls.Clear();
            pnRight.AutoScroll = false;

            // Title bar
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.Transparent };

            var lbTitle = new Label
            {
                Text = "Thông tin chi tiết",
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true
            };

            lbId = new Label
            {
                Text = "ID: ",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Right,
                AutoSize = true
            };

            titleBar.Controls.Add(lbId);
            titleBar.Controls.Add(lbTitle);
            pnRight.Controls.Add(titleBar);

            // Bottom buttons
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = C_PANEL, Padding = new Padding(0, 8, 0, 0) };
            pnRight.Controls.Add(bottom);

            btnCancel = MakeTopButton("Hủy bỏ", 110);
            btnSave = MakeTopButton("Lưu", 110);

            btnCancel.BackColor = Color.FromArgb(45, 45, 45);
            btnSave.BackColor = Color.Gainsboro;
            btnSave.ForeColor = Color.Black;

            bottom.Controls.Add(btnSave);
            bottom.Controls.Add(btnCancel);

            bottom.Resize += (s, e) =>
            {
                btnSave.Location = new Point(bottom.ClientSize.Width - btnSave.Width, 8);
                btnCancel.Location = new Point(bottom.ClientSize.Width - btnSave.Width - btnCancel.Width - 10, 8);
            };

            btnCancel.Click += (s, e) => ClearDetail();
            btnSave.Click += (s, e) =>
            {
                if (_selectedId <= 0) CreateProduct();
                else UpdateProduct();
            };

            // Body
            var body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(0, 8, 0, 0), BackColor = C_PANEL };
            pnRight.Controls.Add(body);
            body.BringToFront();

            var stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            body.Controls.Add(stack);

            stack.Controls.Add(MakeGroupHeader("Định danh"));

            stack.Controls.Add(MakeFieldLabel("Tên sản phẩm"));
            txtName = MakeInputTextBox();
            stack.Controls.Add(txtName);

            stack.Controls.Add(MakeFieldLabel("Danh mục"));
            cboCategory = MakeComboBox();
            stack.Controls.Add(cboCategory);

            var rowSkuStatus = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            rowSkuStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            rowSkuStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var leftSku = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
            leftSku.Controls.Add(MakeFieldLabel("Mã SKU"));
            txtSku = MakeInputTextBox();
            leftSku.Controls.Add(txtSku);
            txtSku.BringToFront();

            var rightStatus = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 0, 0) };
            rightStatus.Controls.Add(MakeFieldLabel("Trạng thái"));
            cboStatus = MakeComboBox();
            cboStatus.Items.AddRange(new object[] { "Đang bán", "Sắp hết", "Hết hàng", "Bản nháp" });
            cboStatus.SelectedIndex = 0;
            rightStatus.Controls.Add(cboStatus);
            cboStatus.BringToFront();

            rowSkuStatus.Controls.Add(leftSku, 0, 0);
            rowSkuStatus.Controls.Add(rightStatus, 1, 0);
            stack.Controls.Add(rowSkuStatus);

            stack.Controls.Add(MakeGroupHeader("Hình ảnh"));

            var imgRow = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            pic1 = MakeThumb();
            pic2 = MakeThumb();

            pnAddImg = new Panel
            {
                Width = 110,
                Height = 86,
                BackColor = Color.FromArgb(25, 25, 25),
                Margin = new Padding(8, 6, 0, 6),
                Cursor = Cursors.Hand
            };
            pnAddImg.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnAddImg.Width - 1, pnAddImg.Height - 1);

                TextRenderer.DrawText(e.Graphics, "＋\nTải ảnh", new Font("Segoe UI", 9, FontStyle.Bold),
                    new Rectangle(0, 0, pnAddImg.Width, pnAddImg.Height),
                    C_MUTED, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            pnAddImg.Click += (s, e) => PickImage();

            imgRow.Controls.Add(pic1, 0, 0);
            imgRow.Controls.Add(pic2, 1, 0);
            imgRow.Controls.Add(pnAddImg, 2, 0);
            stack.Controls.Add(imgRow);

            stack.Controls.Add(MakeGroupHeader("Giá & Tồn kho"));

            var priceGrid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            priceGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            priceGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var pnPrice = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
            pnPrice.Controls.Add(MakeFieldLabel("Giá bán"));
            numPrice = MakeMoneyBox();
            pnPrice.Controls.Add(numPrice);
            numPrice.BringToFront();

            var pnQty = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 0, 0) };
            pnQty.Controls.Add(MakeFieldLabel("Số lượng"));
            numStock = MakeIntBox(1000000);
            pnQty.Controls.Add(numStock);
            numStock.BringToFront();

            priceGrid.Controls.Add(pnPrice, 0, 0);
            priceGrid.Controls.Add(pnQty, 1, 0);
            stack.Controls.Add(priceGrid);

            stack.Controls.Add(MakeGroupHeader("Mô tả"));

            txtDesc = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                Height = 90,
                ScrollBars = ScrollBars.Vertical,
                BackColor = C_INPUT,
                ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
            stack.Controls.Add(txtDesc);
        }

        private Label MakeGroupHeader(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = C_TEXT,
                AutoSize = false,
                Height = 26,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Padding(0, 10, 0, 0),
                Margin = new Padding(0, 10, 0, 0)
            };
        }

        private PictureBox MakeThumb()
        {
            return new PictureBox
            {
                Width = 86,
                Height = 86,
                BackColor = Color.FromArgb(25, 25, 25),
                Margin = new Padding(0, 6, 8, 6),
                SizeMode = PictureBoxSizeMode.Zoom
            };
        }

        private NumericUpDown MakeMoneyBox()
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Top,
                Maximum = 1000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Height = 28,
                BackColor = C_INPUT,
                ForeColor = C_TEXT
            };
        }

        private NumericUpDown MakeIntBox(int max)
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Top,
                Maximum = max,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Height = 28,
                BackColor = C_INPUT,
                ForeColor = C_TEXT
            };
        }

        private Label MakeFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = C_MUTED,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Padding = new Padding(0, 10, 0, 4)
            };
        }

        private TextBox MakeInputTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = C_INPUT,
                ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
        }

        private ComboBox MakeComboBox()
        {
            return new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = C_INPUT,
                ForeColor = C_TEXT,
                FlatStyle = FlatStyle.Flat
            };
        }

        // ================== DATA ==================
        private void LoadCategories()
        {
            if (cboCategory == null) return;

            var cats = _catSvc.GetAll();
            cboCategory.DataSource = cats;
            cboCategory.DisplayMember = "CategoryName";
            cboCategory.ValueMember = "CategoryId";
        }

        private void LoadGrid()
        {
            var keyword = (txtSearch == null) ? "" : txtSearch.Text;
            var data = _svc.GetAll(keyword);

            // ✅ CHỈ HIỆN SẢN PHẨM ĐANG BÁN (IsActive = true)
            if (data != null)
                data = data.FindAll(x => x.IsActive);   // nếu data là List<...>

            dgv.DataSource = null;
            dgv.DataSource = data;

            if (dgv.Columns["ProductId"] != null) dgv.Columns["ProductId"].Visible = false;
            if (dgv.Columns["ImagePath"] != null) dgv.Columns["ImagePath"].Visible = false;

            SetCol("Sku", "Mã SKU", 90);
            SetCol("ProductName", "Tên sản phẩm", 220);
            SetCol("CategoryName", "Danh mục", 140);
            SetCol("Price", "Giá bán", 110);
            SetCol("Stock", "Tồn kho", 80);
            SetCol("IsActive", "Trạng thái", 90);

            if (dgv.Rows.Count > 0)
            {
                dgv.ClearSelection();
                dgv.Rows[0].Selected = true;
                if (dgv.Columns["ProductName"] != null)
                    dgv.CurrentCell = dgv.Rows[0].Cells["ProductName"];
            }
        }

        private void SetCol(string name, string header, int weight)
        {
            var col = dgv.Columns[name];
            if (col == null) return;
            col.HeaderText = header;
            col.FillWeight = weight;
        }

        private void LoadDetail(int id)
        {
            var p = _svc.GetById(id);
            if (p == null) return;

            _selectedId = p.ProductId;
            if (lbId != null) lbId.Text = "ID: " + _selectedId;

            if (txtName != null) txtName.Text = p.ProductName;
            if (txtSku != null) txtSku.Text = p.Sku;

            if (cboCategory != null && cboCategory.DataSource != null)
                cboCategory.SelectedValue = p.CategoryId;

            if (numPrice != null) numPrice.Value = Convert.ToDecimal(p.Price);
            if (numStock != null) numStock.Value = Convert.ToDecimal(p.Stock);

            // map status UI theo IsActive + Stock
            string st;
            if (!p.IsActive) st = "Bản nháp";
            else if (p.Stock <= 0) st = "Hết hàng";
            else if (p.Stock < 10) st = "Sắp hết";
            else st = "Đang bán";

            if (cboStatus != null)
            {
                int idx = cboStatus.Items.IndexOf(st);
                cboStatus.SelectedIndex = (idx >= 0) ? idx : 0;
            }

            if (txtDesc != null) txtDesc.Text = p.Description;

            _imagePath = p.ImagePath;
            LoadImageToPic(_imagePath);
        }

        private void ClearDetail()
        {
            _selectedId = 0;
            if (lbId != null) lbId.Text = "ID: ";

            if (txtName != null) txtName.Text = "";
            if (txtSku != null) txtSku.Text = "";
            if (txtDesc != null) txtDesc.Text = "";

            if (cboCategory != null && cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            if (cboStatus != null) cboStatus.SelectedIndex = 0;

            if (numPrice != null) numPrice.Value = 0;
            if (numStock != null) numStock.Value = 0;

            _imagePath = null;

            ClearPic(pic1);
            ClearPic(pic2);
        }

        private void ClearPic(PictureBox pb)
        {
            if (pb == null) return;
            if (pb.Image == null) return;
            var old = pb.Image;
            pb.Image = null;
            old.Dispose();
        }

        private void PickImage()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                _imagePath = ofd.FileName;
                LoadImageToPic(_imagePath);
            }
        }

        private void LoadImageToPic(string path)
        {
            try
            {
                ClearPic(pic1);
                ClearPic(pic2);

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var img = Image.FromStream(fs))
                {
                    if (pic1 != null) pic1.Image = new Bitmap(img);
                    if (pic2 != null) pic2.Image = new Bitmap(img);
                }
            }
            catch
            {
                ClearPic(pic1);
                ClearPic(pic2);
            }
        }

        private bool ValidateInput()
        {
            if (txtName == null || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tên sản phẩm không được trống!");
                return false;
            }
            if (txtSku == null || string.IsNullOrWhiteSpace(txtSku.Text))
            {
                MessageBox.Show("SKU không được trống!");
                return false;
            }
            if (cboCategory == null || cboCategory.SelectedValue == null)
            {
                MessageBox.Show("Chưa chọn danh mục!");
                return false;
            }
            return true;
        }

        private void CreateProduct()
        {
            if (!ValidateInput()) return;

            bool isActive = true;
            if (cboStatus != null && cboStatus.SelectedItem != null)
                isActive = (cboStatus.SelectedItem.ToString() != "Bản nháp");

            var p = new Product
            {
                ProductName = txtName.Text.Trim(),
                Sku = txtSku.Text.Trim(),
                CategoryId = Convert.ToInt32(cboCategory.SelectedValue),
                Price = numPrice != null ? numPrice.Value : 0,
                Stock = numStock != null ? Convert.ToInt32(numStock.Value) : 0,
                ImagePath = _imagePath,
                Description = txtDesc != null ? txtDesc.Text : "",
                IsActive = isActive
            };

            try
            {
                _svc.Create(p);
                LoadGrid();
                SelectRowById(p.ProductId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm sản phẩm: " + ex.Message);
            }
        }

        private void UpdateProduct()
        {
            if (_selectedId <= 0)
            {
                MessageBox.Show("Chọn 1 sản phẩm để sửa!");
                return;
            }
            if (!ValidateInput()) return;

            var p = _svc.GetById(_selectedId);
            if (p == null) return;

            bool isActive = true;
            if (cboStatus != null && cboStatus.SelectedItem != null)
                isActive = (cboStatus.SelectedItem.ToString() != "Bản nháp");

            p.ProductName = txtName.Text.Trim();
            p.Sku = txtSku.Text.Trim();
            p.CategoryId = Convert.ToInt32(cboCategory.SelectedValue);
            p.Price = numPrice != null ? numPrice.Value : 0;
            p.Stock = numStock != null ? Convert.ToInt32(numStock.Value) : 0;
            p.ImagePath = _imagePath;
            p.Description = txtDesc != null ? txtDesc.Text : "";
            p.IsActive = isActive;

            try
            {
                _svc.Update(p);
                LoadGrid();
                SelectRowById(_selectedId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }

        private void DeleteProduct()
        {
            if (_selectedId <= 0)
            {
                MessageBox.Show("Chọn 1 sản phẩm để xóa!");
                return;
            }

            var ok = MessageBox.Show("Xóa sản phẩm này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (ok != DialogResult.Yes) return;

            try
            {
                _svc.DeleteHard(_selectedId);
                LoadGrid();
                ClearDetail();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
        }

        private int GetSelectedIdFromGrid()
        {
            if (dgv == null) return 0;

            DataGridViewRow r = null;

            if (dgv.SelectedRows != null && dgv.SelectedRows.Count > 0)
                r = dgv.SelectedRows[0];
            else if (dgv.CurrentRow != null)
                r = dgv.CurrentRow;

            if (r == null) return 0;

            // đổi "ProductId" nếu cột bạn tên khác
            var v = r.Cells["ProductId"]?.Value;
            int id;
            return (v != null && int.TryParse(v.ToString(), out id)) ? id : 0;
        }

        private void SyncSelectedId()
        {
            _selectedId = GetSelectedIdFromGrid();
            if (lbId != null) lbId.Text = (_selectedId > 0) ? ("ID: " + _selectedId) : "ID: ";
        }


        private void SelectRowById(int id)
        {
            if (dgv == null) return;

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                var v = dgv.Rows[i].Cells["ProductId"].Value;
                if (v == null) continue;

                int rid;
                if (int.TryParse(v.ToString(), out rid) && rid == id)
                {
                    dgv.ClearSelection();
                    dgv.Rows[i].Selected = true;
                    if (dgv.Columns["ProductName"] != null)
                        dgv.CurrentCell = dgv.Rows[i].Cells["ProductName"];
                    return;
                }
            }
        }
    }
}
