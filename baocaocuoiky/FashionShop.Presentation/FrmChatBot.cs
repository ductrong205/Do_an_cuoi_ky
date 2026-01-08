using FashionShop.Data; // ← cần using này
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace FashionShop.Presentation
{
    public partial class FrmChatBot : Form
    {
        private const string OLLAMA_URL = "http://localhost:11434/api/generate";
        private const string MODEL_NAME = "qwen2:0.5b"; //chatbot

        private RichTextBox rtbChat;
        private TextBox txtInput;
        private Button btnSend;
        private Panel pnlInput;
        private FashionShopDb _db;
        private string _cachedContext; // Cache dữ liệu DB để tránh truy vấn lại

        public FrmChatBot()
        {
            InitializeComponent();

            // Khởi tạo DB context
            try
            {
                _db = new FashionShopDb();
                _cachedContext = GetAppContextFromDb();
            }
            catch (Exception ex)
            {
                _cachedContext = $"⚠️ Lỗi kết nối DB: {ex.Message}";
            }

            this.Text = "🤖 Trợ lý AI - FashionShop";
            this.Size = new Size(420, 500);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Width - this.Width - 20,
                Screen.PrimaryScreen.WorkingArea.Height - this.Height - 80
            );

            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            rtbChat = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12)
            };

            pnlInput = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = Color.FromArgb(35, 35, 35),
                Padding = new Padding(8)
            };

            txtInput = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 8, 0)
            };

            btnSend = new Button
            {
                Text = "Gửi",
                Size = new Size(60, 28),
                Location = new Point(pnlInput.ClientSize.Width - 70, 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;

            pnlInput.Resize += (s, e) =>
            {
                btnSend.Location = new Point(pnlInput.ClientSize.Width - btnSend.Width - 4, 6);
            };

            pnlInput.Controls.Add(btnSend);
            pnlInput.Controls.Add(txtInput);

            btnSend.Click += BtnSend_Click;
            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnSend_Click(s, e);
                    e.SuppressKeyPress = true;
                }
            };

            this.Controls.Add(rtbChat);
            this.Controls.Add(pnlInput);

            AppendMessage("🤖 AI", "Xin chào! Tôi là trợ lý AI, đã được kết nối với cơ sở dữ liệu shop thời trang của bạn. Hỏi tôi bất cứ điều gì!", Color.FromArgb(100, 255, 200));

            if (_cachedContext.Contains("⚠️"))
            {
                AppendMessage("❌ Cảnh báo", _cachedContext, Color.OrangeRed);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _db?.Dispose();
            base.OnFormClosed(e);
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string userMsg = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userMsg)) return;

            AppendMessage("👤 Bạn", userMsg, Color.FromArgb(173, 216, 230));
            txtInput.Clear();
            SetInputEnabled(false);

            AppendMessage("🤖 AI", "Đang suy nghĩ...", Color.Gray);

            try
            {
                string aiReply = await CallOllamaAsync(userMsg);

                string fullText = rtbChat.Text;
                int thinkingIndex = fullText.LastIndexOf("\n🤖 AI: Đang suy nghĩ...\n");
                if (thinkingIndex >= 0)
                {
                    rtbChat.Text = fullText.Substring(0, thinkingIndex);
                }

                AppendMessage("🤖 AI", aiReply, Color.FromArgb(100, 255, 200));
            }
            catch (Exception ex)
            {
                string fullText = rtbChat.Text;
                int thinkingIndex = fullText.LastIndexOf("\n🤖 AI: Đang suy nghĩ...\n");
                if (thinkingIndex >= 0)
                {
                    rtbChat.Text = fullText.Substring(0, thinkingIndex);
                }

                string errorMsg = ex.Message;
                if (errorMsg.Contains("localhost") || errorMsg.Contains("refused"))
                {
                    errorMsg = "⚠️ Ollama chưa chạy! Vui lòng mở ứng dụng Ollama trước khi dùng chatbot.";
                }
                else
                {
                    errorMsg = $"⚠️ Lỗi: {ex.Message}";
                }

                AppendMessage("❌ Hệ thống", errorMsg, Color.OrangeRed);
            }
            finally
            {
                SetInputEnabled(true);
                txtInput.Focus();
            }
        }

        private void SetInputEnabled(bool enabled)
        {
            txtInput.Enabled = enabled;
            btnSend.Enabled = enabled;
        }

        private void AppendMessage(string sender, string msg, Color color)
        {
            string fullMsg = $"\n{sender}: {msg}\n";
            rtbChat.AppendText(fullMsg);

            int start = rtbChat.TextLength - fullMsg.Length + 1;
            int length = sender.Length + 2;
            if (start >= 0 && length > 0)
            {
                rtbChat.Select(start, length);
                rtbChat.SelectionColor = color;
                rtbChat.DeselectAll();
            }

            rtbChat.ScrollToCaret();
        }

        private async Task<string> CallOllamaAsync(string prompt)
        {
            // Phân tích câu hỏi để xác định có cần dữ liệu chi tiết hay không
            string context = GetAppContextFromDb(); // ← dữ liệu tổng quát

            // Nếu câu hỏi liên quan đến đơn hàng cụ thể → thêm dữ liệu chi tiết
            if (prompt.Contains("ORD-") || prompt.Contains("đơn hàng"))
            {
                var orderCode = ExtractOrderCode(prompt);
                if (!string.IsNullOrEmpty(orderCode))
                {
                    var orderDetail = GetOrderDetail(orderCode);
                    context += $"\n\n=== CHI TIẾT ĐƠN HÀNG {orderCode} ===\n{orderDetail}";
                }
            }

            // Nếu câu hỏi liên quan đến khách hàng cụ thể → thêm dữ liệu chi tiết
            else if (prompt.Contains("khách hàng") || prompt.Contains("Hồ Ngọc Diệp") || prompt.Contains("Nguyễn Thị Mai"))
            {
                var customerName = ExtractCustomerName(prompt);
                if (!string.IsNullOrEmpty(customerName))
                {
                    var customerHistory = GetCustomerHistory(customerName);
                    context += $"\n\n=== LỊCH SỬ MUA HÀNG CỦA {customerName} ===\n{customerHistory}";
                }
            }

            string fullPrompt = $@"
Bạn là trợ lý AI của hệ thống quản lý shop thời trang.
Dưới đây là dữ liệu thực tế từ cơ sở dữ liệu:

{context}

Câu hỏi người dùng: {prompt}
Hãy trả lời ngắn gọn, rõ ràng, và chỉ dựa trên dữ liệu đã cung cấp. Nếu không biết, hãy nói ""Tôi không có thông tin này trong dữ liệu hiện tại."";

Trả lời:";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(120);

                var payload = new
                {
                    model = MODEL_NAME,
                    prompt = fullPrompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3
                    }
                };

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(OLLAMA_URL, content);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ollama trả về lỗi: {response.StatusCode}");
                }

                dynamic result = JsonConvert.DeserializeObject(responseText);
                return result.response.ToString();
            }
        }

        private string ExtractOrderCode(string prompt)
        {
            var match = System.Text.RegularExpressions.Regex.Match(prompt, @"ORD-\d+");
            return match.Success ? match.Value : null;
        }

        private string GetOrderDetail(string orderCode)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
            if (order == null) return "Không tìm thấy đơn hàng.";

            var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
            var items = order.OrderItems.ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Khách hàng: {customer?.FullName ?? "Unknown"}");
            sb.AppendLine($"Ngày đặt: {order.OrderDate:dd/MM/yyyy}");
            sb.AppendLine($"Tổng tiền: {order.TotalAmount:N0}đ");
            sb.AppendLine("Chi tiết sản phẩm:");

            foreach (var item in items)
            {
                var product = _db.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                sb.AppendLine($"- {product?.ProductName ?? "Unknown"} | Số lượng: {item.Quantity} | Giá: {item.UnitPrice:N0}đ");
            }

            return sb.ToString();
        }

        private string ExtractCustomerName(string prompt)
        {
            if (prompt.Contains("Hồ Ngọc Diệp")) return "Hồ Ngọc Diệp";
            if (prompt.Contains("Nguyễn Thị Mai")) return "Nguyễn Thị Mai";
            if (prompt.Contains("Trần Văn Hùng")) return "Trần Văn Hùng";
            if (prompt.Contains("Lê Thị Lan")) return "Lê Thị Lan";
            if (prompt.Contains("Phạm Văn Minh")) return "Phạm Văn Minh";
            return null;
        }

        private string GetCustomerHistory(string customerName)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.FullName == customerName);
            if (customer == null) return "Không tìm thấy khách hàng.";

            var orders = _db.Orders
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Tổng số đơn hàng: {orders.Count}");
            sb.AppendLine("Các đơn hàng gần đây:");

            foreach (var order in orders)
            {
                var totalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
                sb.AppendLine($"- {order.OrderCode} | {order.OrderDate:dd/MM/yyyy} | Tổng: {totalAmount:N0}đ");
            }

            return sb.ToString();
        }

        private string GetAppContextFromDb()
        {
            if (_db == null)
                return "Không thể kết nối đến cơ sở dữ liệu.";

            var sb = new StringBuilder();

            // 0. Thông tin hệ thống
            sb.AppendLine("=== HỆ THỐNG SHOP ===");
            sb.AppendLine("Tên shop: Fashion Shop Manager");
            sb.AppendLine("Loại hình: Thời trang nam/nữ, phụ kiện");
            sb.AppendLine("Số lượng sản phẩm: " + _db.Products.Count());
            sb.AppendLine("Số lượng khách hàng: " + _db.Customers.Count());
            sb.AppendLine("Số lượng đơn hàng: " + _db.Orders.Count());

            // 1. Đơn hàng gần đây (5 đơn mới nhất)
            sb.AppendLine("\n=== ĐƠN HÀNG GẦN ĐÂY ===");
            var recentOrders = _db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            foreach (var order in recentOrders)
            {
                var customer = _db.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
                var totalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

                sb.AppendLine($"- {order.OrderCode} | {customer?.FullName ?? "Unknown"} | {order.OrderDate:dd/MM/yyyy} | Tổng: {totalAmount:N0}đ");
            }

            // 2. Khách hàng nổi bật (VIP hoặc tổng chi > 1 triệu)
            sb.AppendLine("\n=== KHÁCH HÀNG NỔI BẬT ===");
            var topCustomers = _db.Customers
                .Where(c => c.IsVip)
                .Select(c => new
                {
                    c.CustomerId,
                    c.FullName,
                    c.IsVip,
                    TotalSpent = _db.Orders
                        .Where(o => o.CustomerId == c.CustomerId)
                        .Sum(o => o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice))
                })
                .Where(x => x.TotalSpent > 0)
                .OrderByDescending(x => x.TotalSpent)
                .Take(3)
                .ToList();

            foreach (var c in topCustomers)
            {
                sb.AppendLine($"- {c.FullName} ({c.CustomerId}) | {(c.IsVip ? "VIP" : "Thường")} | Tổng chi: {c.TotalSpent:N0}đ");
            }

            // 3. Sản phẩm bán chạy
            sb.AppendLine("\n=== SẢN PHẨM BÁN CHẠY ===");
            var topProducts = _db.Products
                .Select(p => new
                {
                    p.ProductName,
                    SoldCount = p.OrderItems.Sum(oi => oi.Quantity),
                    p.Price
                })
                .Where(x => x.SoldCount > 0)
                .OrderByDescending(x => x.SoldCount)
                .Take(5)
                .ToList();

            foreach (var p in topProducts)
            {
                sb.AppendLine($"- {p.ProductName} | Đã bán: {p.SoldCount} cái | Giá: {p.Price:N0}đ");
            }

            // 4. Tồn kho thấp (< 5 cái)
            sb.AppendLine("\n=== TỒN KHO THẤP (còn < 5 cái) ===");
            var lowStock = _db.Inventories
                .Where(i => i.Quantity < 5)
                .Join(_db.ProductVariants, i => i.VariantId, v => v.VariantId, (i, v) => new { i.Quantity, v.Product })
                .Where(x => x.Product != null)
                .Select(x => new { ProductName = x.Product.ProductName, Quantity = x.Quantity })
                .ToList();

            if (lowStock.Count == 0)
            {
                sb.AppendLine("Không có sản phẩm nào sắp hết hàng.");
            }
            else
            {
                foreach (var item in lowStock)
                {
                    sb.AppendLine($"- {item.ProductName} | Còn: {item.Quantity} cái");
                }
            }

            // 5. Danh mục sản phẩm
            sb.AppendLine("\n=== DANH MỤC SẢN PHẨM ===");
            var categories = _db.Category.ToList();
            foreach (var cat in categories)
            {
                var productsInCat = _db.Products.Where(p => p.CategoryId == cat.CategoryId).Count();
                sb.AppendLine($"- {cat.CategoryName}: {productsInCat} sản phẩm");
            }

            // 6. Sản phẩm theo danh mục (ví dụ: áo sơ mi)
            sb.AppendLine("\n=== SẢN PHẨM THEO DANH MỤC ===");
            var shirtCategory = _db.Category.FirstOrDefault(c => c.CategoryName.Contains("sơ mi"));
            if (shirtCategory != null)
            {
                var shirts = _db.Products.Where(p => p.CategoryId == shirtCategory.CategoryId).ToList();
                sb.AppendLine($"Áo sơ mi ({shirts.Count} sản phẩm):");
                foreach (var p in shirts.Take(3)) // chỉ hiển thị 3 sản phẩm đầu
                {
                    sb.AppendLine($"- {p.ProductName} | Giá: {p.Price:N0}đ | Tồn: {p.Stock}");
                }
                if (shirts.Count > 3)
                    sb.AppendLine($"... và {shirts.Count - 3} sản phẩm khác.");
            }

            return sb.ToString();
        }
    }
}