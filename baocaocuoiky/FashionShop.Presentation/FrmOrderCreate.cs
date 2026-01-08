using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.Data;
using FashionShop.Business;

namespace FashionShop.Presentation
{
    public partial class FrmOrderCreate : Form
    {
        public Order CreatedOrder { get; private set; }

        // Controls
        private TextBox txtCode;
        private TextBox txtCustomerName;
        private TextBox txtPhone;     
        private TextBox txtEmail;
        private ListView lvProducts;
        private TextBox txtTotal;
        private TextBox txtNote;
        private Button btnSave;
        private Button btnCancel;

        // Dữ liệu hỗ trợ
        private List<Product> _allProducts;

        // === KHỞI TẠO FORM ===
        public FrmOrderCreate()
        {
            InitializeComponent();
            InitializeFormAppearance();
            InitializeControls();
            LoadProductData();
            WireEvents();
        }

        public static class CustomerRanks
        {
            public const string Normal = "Normal";
            public const string Silver = "Silver Member";
            public const string Gold = "Gold Member";
        }

        // === GIAO DIỆN FORM ===
        private void InitializeFormAppearance()
        {
            this.Text = "Tạo đơn hàng mới";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
        }

        // === TẠO CÁC CONTROL ===
        private void InitializeControls()
        {
            // Tiêu đề
            var lblTitle = new Label
            {
                Text = "Thông tin đơn hàng",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitle);

            // Mã đơn
            var lblCode = new Label { Text = "Mã đơn:", Location = new Point(20, 60), AutoSize = true, ForeColor = Color.White };
            txtCode = new TextBox
            {
                Location = new Point(100, 60),
                Width = 120,
                ReadOnly = true,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Text = GenerateOrderCode()
            };

            // Khách hàng 
            var lblCustomer = new Label { Text = "Khách hàng:", Location = new Point(20, 100), AutoSize = true, ForeColor = Color.White };
            txtCustomerName = new TextBox
            {
                Location = new Point(100, 100),
                Width = 250,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            // Số điện thoại
            var lblPhone = new Label { Text = "Số điện thoại:", Location = new Point(20, 130), AutoSize = true, ForeColor = Color.White };
            txtPhone = new TextBox
            {
                Location = new Point(100, 130),
                Width = 250,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
            };

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 160), AutoSize = true, ForeColor = Color.White };
            txtEmail = new TextBox
            {
                Location = new Point(100, 160),
                Width = 250,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
            };

            // Sản phẩm — DỜI XUỐNG DƯỚI EMAIL
            var lblProduct = new Label { Text = "Sản phẩm:", Location = new Point(20, 190), AutoSize = true, ForeColor = Color.White };
            lvProducts = new ListView
            {
                Location = new Point(100, 190), // ← Dời xuống Y=190
                Width = 300,
                Height = 100,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = true,
                CheckBoxes = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };
            lvProducts.Columns.Add("Tên sản phẩm", 200);
            lvProducts.Columns.Add("Giá", 80, HorizontalAlignment.Right);

            // Tổng tiền — DỜI XUỐNG PHÙ HỢP
            var lblTotal = new Label { Text = "Tổng tiền:", Location = new Point(20, 310), AutoSize = true, ForeColor = Color.White };
            txtTotal = new TextBox
            {
                Location = new Point(100, 310), // ← Dời xuống Y=310
                Width = 120,
                ReadOnly = true,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            // Ghi chú
            var lblNote = new Label { Text = "Ghi chú:", Location = new Point(20, 340), AutoSize = true, ForeColor = Color.White };
            txtNote = new TextBox
            {
                Location = new Point(100, 340), // ← Dời xuống Y=340
                Width = 400,
                Height = 18,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            // Nút Hủy / Lưu — DỜI XUỐNG DƯỚI GHI CHÚ
            btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(420, 380), // ← Y=380
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave = new Button
            {
                Text = "Lưu đơn",
                Location = new Point(520, 380), // ← Y=380
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Cập nhật kích thước form
            this.Size = new Size(620, 440); // ← Tăng chiều cao để đủ chỗ

            // Thêm tất cả vào form
            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblCode, txtCode,
                lblCustomer, txtCustomerName,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblProduct, lvProducts,
                lblTotal, txtTotal,
                lblNote, txtNote,
                btnCancel, btnSave
            });
        }

        // === TẢI DỮ LIỆU SẢN PHẨM ===
        private void LoadProductData()
        {
            using (var db = new FashionShopDb())
            {
                _allProducts = db.Products.ToList();
                foreach (var p in _allProducts.Take(10))
                {
                    var item = new ListViewItem(p.ProductName) { Tag = p };
                    item.SubItems.Add(p.Price.ToString("#,0") + "đ");
                    lvProducts.Items.Add(item);
                }
            }
        }

        // === GẮN SỰ KIỆN ===
        private void WireEvents()
        {
            lvProducts.ItemChecked += OnProductSelectionChanged;
            btnSave.Click += OnSaveOrder;
            btnCancel.Click += (s, e) => this.Close();
        }

        // === TÍNH LẠI TỔNG TIỀN KHI CHỌN SẢN PHẨM ===
        private void OnProductSelectionChanged(object sender, ItemCheckedEventArgs e)
        {
            decimal total = lvProducts.CheckedItems.Cast<ListViewItem>()
                .Select(item => ((Product)item.Tag).Price)
                .Sum();
            txtTotal.Text = total == 0 ? "" : total.ToString("#,0") + "đ";
        }

        // === XỬ LÝ LƯU ĐƠN HÀNG ===
        private void OnSaveOrder(object sender, EventArgs e)
        {
            try
            {
                // 1. Kiểm tra sản phẩm
                var selectedProducts = lvProducts.CheckedItems.Cast<ListViewItem>()
                    .Select(item => (Product)item.Tag)
                    .ToList();

                if (!selectedProducts.Any())
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Kiểm tra tên khách hàng
                string customerName = txtCustomerName.Text.Trim();
                if (string.IsNullOrEmpty(customerName))
                {
                    MessageBox.Show("Vui lòng nhập tên khách hàng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string phone = txtPhone.Text.Trim();   // ← Lấy số điện thoại
                string email = txtEmail.Text.Trim();   // ← Lấy email

                // 3. Đảm bảo khách hàng tồn tại (tạo mới nếu cần)
                int customerId = EnsureCustomerExists(customerName, phone, email);

                // 4. Tạo đơn hàng
                var order = CreateOrderInDatabase(customerId, selectedProducts);

                // CẬP NHẬT HẠNG KHÁCH HÀNG NGAY SAU KHI LƯU ĐƠN
                UpdateCustomerRank(customerId); // ← Cập nhật dựa trên tổng chi tiêu

                // 5. Gán kết quả và đóng form
                // Sau khi lưu đơn và cập nhật hạng
                CreatedOrder = order;
                MessageBox.Show("Đơn hàng đã được tạo thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 👇 GỬI EVENT CHO UcCustomers CẬP NHẬT
                var ucCustomers = Application.OpenForms.OfType<UcCustomers>().FirstOrDefault();
                if (ucCustomers != null)
                {
                    // Nếu UcCustomers có phương thức ReloadData()
                    var reloadable = ucCustomers as IReloadable;
                    if (reloadable != null)
                    {
                        reloadable.ReloadData();
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = ex.EntityValidationErrors
                    .SelectMany(er => er.ValidationErrors)
                    .Select(err => err.ErrorMessage);
                MessageBox.Show(string.Join("\n", errors));
            }
        }

        // === ĐẢM BẢO KHÁCH HÀNG TỒN TẠI ===
        private int EnsureCustomerExists(string customerName, string phone = null, string email = null)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                customerName = "Khách lẻ";

            using (var db = new FashionShopDb())
            {
                var existing = db.Customers.FirstOrDefault(c => c.FullName == customerName);
                if (existing != null)
                {
                    // 👇 CẬP NHẬT THÔNG TIN NẾU CÓ GIÁ TRỊ MỚI
                    if (!string.IsNullOrWhiteSpace(phone) && existing.Phone != phone)
                        existing.Phone = phone;

                    if (!string.IsNullOrWhiteSpace(email) && existing.Email != email)
                        existing.Email = email;

                    // Nếu bạn muốn cập nhật Rank (ví dụ: từ Normal lên Silver/Gold) → gọi UpdateCustomerRank
                    UpdateCustomerRank(existing.CustomerId); // ← Có thể bỏ qua nếu không cần cập nhật ngay

                    db.SaveChanges(); // ← Lưu thay đổi
                    return existing.CustomerId;
                }

                // Nếu chưa tồn tại → tạo mới
                var newCustomer = new Customer
                {
                    FullName = customerName,
                    CustomerCode = GenerateCustomerCode(db),
                    Phone = phone ?? "",
                    Email = email ?? "",
                    Address = "",
                    CreatedAt = DateTime.Now,
                    JoinDate = DateTime.Now,
                    Rank = "Normal",
                    IsVip = false,
                    IsActive = true
                };

                db.Customers.Add(newCustomer);
                db.SaveChanges();

                return newCustomer.CustomerId;
            }
        }

        private void UpdateCustomerRank(int customerId)
        {
            using (var db = new FashionShopDb())
            {
                var customer = db.Customers.Find(customerId);
                if (customer == null) return;

                // Tính tổng chi tiêu
                decimal totalSpent = db.Orders
                    .Where(o => o.CustomerId == customerId)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                // Quy tắc hạng (tùy bạn điều chỉnh)
                if (totalSpent >= 5000000) // 5 triệu
                    customer.Rank = CustomerRanks.Gold;
                else if (totalSpent >= 2000000) // 2 triệu
                    customer.Rank = CustomerRanks.Silver;
                else
                    customer.Rank = CustomerRanks.Normal;

                db.SaveChanges();
            }
        }

        // === TẠO ĐƠN HÀNG TRONG DATABASE ===
        private Order CreateOrderInDatabase(int customerId, List<Product> products)
        {
            using (var db = new FashionShopDb())
            {
                var order = new Order
                {
                    OrderCode = txtCode.Text,
                    OrderDate = DateTime.Now,
                    CustomerId = customerId,
                    TotalAmount = products.Sum(p => p.Price),
                    Status = 0, // Pending
                    Note = txtNote.Text
                };

                db.Orders.Add(order);
                db.SaveChanges(); // Cần SaveChanges để có OrderId

                // Thêm chi tiết đơn hàng
                foreach (var p in products)
                {
                    db.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = p.ProductId,
                        Quantity = 1,
                        UnitPrice = p.Price
                    });
                }

                db.SaveChanges();
                return order;
            }
        }

        // === SINH MÃ ĐƠN ===
        private string GenerateOrderCode()
        {
            using (var db = new FashionShopDb())
            {
                var lastOrder = db.Orders.OrderByDescending(o => o.OrderId).FirstOrDefault();
                int nextId = lastOrder != null ? lastOrder.OrderId + 1 : 1;
                return $"ORD-{nextId:D3}";
            }
        }

        // === SINH MÃ KHÁCH HÀNG ===
        private string GenerateCustomerCode(FashionShopDb db)
        {
            var lastCustomer = db.Customers.OrderByDescending(c => c.CustomerId).FirstOrDefault();
            int nextId = lastCustomer != null ? lastCustomer.CustomerId + 1 : 1;
            return $"C{nextId:D3}";
        }
    }
}