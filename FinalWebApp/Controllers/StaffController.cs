using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using FinalWebApp.Enum;
using FinalWebApp.Session;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FinalWebApp.Controllers
{
    [Authorize(Roles = "STAFF")]
    public class StaffController : Controller
    {
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly FinalWebDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public StaffController(FinalWebDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IHubContext<OrderHub> orderHubContext)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
             _orderHubContext = orderHubContext;
        }

        // Hiển thị danh sách bàn ăn
        public IActionResult TableManagement()
        {
            var tables = _context.Tables
                .Select(t => new TableViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Capacity = t.Capacity,
                    Status = t.Status
                })
                .ToList(); // Lấy danh sách bàn

            return View(tables); // Truyền vào View
        }
        [HttpPost]
        public IActionResult UpdateTableToActive(Guid tableId)
        {
            // Tìm bàn với trạng thái Booked
            var table = _context.Tables.FirstOrDefault(t => t.Id == tableId && t.Status == StatusTableEnum.Booked);

            if (table == null)
            {
                TempData["Error"] = "Table not found or not in Booked status.";
                return RedirectToAction("TableManagement");
            }

            // Cập nhật trạng thái sang Active
            table.Status = StatusTableEnum.Active;

            // Lưu thay đổi
            _context.SaveChanges();

            TempData["Message"] = "Table status updated to Active.";
            return RedirectToAction("TableManagement");
        }
        public IActionResult Checkout(Guid orderId)
        {
            // Lấy thông tin đơn hàng từ database cùng với thông tin của bàn
            var order = _context.Orders
                .Include(o => o.Table) // Bao gồm thông tin bàn
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            // Lấy thông tin của bàn từ đơn hàng
            var table = order.Table;

            if (table == null)
            {
                return NotFound("Table not found");
            }

            // Cập nhật trạng thái bàn thành "Trống" (Empty)
            table.Status = StatusTableEnum.Empty;

            // Cập nhật trạng thái đơn hàng thành "Đã Thanh Toán"
            order.OrderStatus = OrderStatusEnum.Paid;

            // Lưu các thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Chuyển sang trang in hóa đơn
            return RedirectToAction("Invoice", new { orderId = order.Id });
        }


        public IActionResult Invoice(Guid orderId)
        {
            // Lấy thông tin đơn hàng từ database cùng với thông tin bàn và khách hàng
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item) // Nạp Item liên quan
                .Include(o => o.Table) // Bao gồm thông tin bàn
                .Include(o => o.Customer) // Bao gồm thông tin khách hàng
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult CreateOrderForGuest(Guid tableId)
        {
            // Kiểm tra bàn có tồn tại và còn trống không
            var table = _context.Tables.FirstOrDefault(t => t.Id == tableId && t.Status == StatusTableEnum.Empty);
            if (table == null)
            {
                TempData["Error"] = "The table is not available or already occupied.";
                return RedirectToAction("TableManagement");
            }

            // Tạo đơn hàng mới cho khách vãng lai
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                TotalAmount = 0, // Ban đầu để 0
                CustomerId = null,
                OrderStatus = OrderStatusEnum.Unpaid,
                TableId = table.Id
            };

            // Cập nhật trạng thái bàn
            table.Status = StatusTableEnum.Active;

            // Lưu vào cơ sở dữ liệu
            _context.Orders.Add(order);
            _context.SaveChanges();

            TempData["Message"] = "Guest order created successfully.";
            return RedirectToAction("TableManagement");
        }


        // Cập nhật trạng thái bàn
        [HttpPost]
        public IActionResult UpdateTableStatus(int tableId, StatusTableEnum status)
        {
            var table = _context.Tables.Find(tableId);
            if (table != null)
            {
                table.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction("TableManagement");
        }
        // GET: Add Item to Order
        public async Task<IActionResult> AddItem(Guid orderId, Guid? categoryId)
        {
            // Lấy danh sách món ăn và danh mục
            var items = _context.Items.Include(i => i.Category).AsQueryable();
            var categories = await _context.Categories.ToListAsync();

            // Nếu có categoryId, lọc món ăn theo categoryId
            if (categoryId.HasValue)
            {
                items = items.Where(i => i.CategoryId == categoryId.Value);
            }

            // Tạo view model và truyền dữ liệu
            var viewModel = new ItemListViewModel
            {
                Items = await items.ToListAsync(),
                Categories = categories,
                SelectedCategoryId = categoryId // Lưu giá trị categoryId vào ViewModel để biết danh mục nào được chọn
            };

            // Truyền orderId vào ViewData để sử dụng trên View
            ViewData["OrderId"] = orderId;

            return View(viewModel);
        }


        // POST: Add item to order
        [HttpPost]
        public async Task<IActionResult> AddToOrder(Guid itemId, Guid orderId)
        {
            // Lấy đơn hàng theo orderId
            var order = await _context.Orders.Include(o => o.OrderItems)
                                             .ThenInclude(oi => oi.Item)
                                             .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            // Lấy món ăn từ cơ sở dữ liệu
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found." });
            }

            // Kiểm tra nếu món ăn đã có trong đơn hàng
            var existingOrderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            if (existingOrderItem != null)
            {
                // Nếu món ăn đã có, cập nhật số lượng
                existingOrderItem.Quantity += 1;
            }
            else
            {
                // Thêm món ăn mới vào đơn hàng
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ItemId = itemId,
                    Quantity = 1,  // Số lượng mặc định là 1
                    Price = item.Price,
                    ItemName = item.Name,  // Cập nhật tên món ăn
                    ItemImage = item.Image, // Cập nhật hình ảnh món ăn
                    CategoryId = item.CategoryId // Đảm bảo CategoryId được gán đúng
                };

                // Thêm món ăn mới vào danh sách OrderItems
                order.OrderItems.Add(orderItem);
                _context.OrderItems.Add(orderItem);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về kết quả thành công
            return Json(new { success = true, message = $"{item.Name} has been added/updated in your order." });
        }




        public IActionResult OrderDetails(Guid orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)  // Nạp các món ăn trong đơn hàng
                .ThenInclude(oi => oi.Item)  // Nạp thông tin món ăn
                .Include(o => o.Table)       // Nạp thông tin bàn
                .Include(o => o.Customer)    // Nạp thông tin khách hàng
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(); // Nếu không tìm thấy đơn hàng
            }

            return View(order); // Trả về view chi tiết đơn hàng
        }
        [HttpPost]
        public IActionResult UpdateQuantity(Guid orderId, Guid orderItemId, int quantity)
        {
            // Tìm đơn hàng dựa trên orderId
            var order = _context.Orders.Include(o => o.OrderItems)
                                       .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                // Nếu không tìm thấy đơn hàng, trả về lỗi
                TempData["Error"] = "Order not found.";
                return RedirectToAction("OrderDetails", new { orderId });
            }

            // Tìm món ăn trong đơn hàng dựa trên orderItemId
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);
            if (orderItem == null)
            {
                // Nếu không tìm thấy món ăn, trả về lỗi
                TempData["Error"] = "Order item not found.";
                return RedirectToAction("OrderDetails", new { orderId });
            }

            // Kiểm tra số lượng có hợp lệ không
            if (quantity <= 0)
            {
                // Nếu số lượng <= 0, xóa món ăn khỏi đơn hàng
                _context.OrderItems.Remove(orderItem); // Xóa món ăn khỏi đơn hàng
                TempData["Message"] = $"{orderItem.ItemName} has been removed from your order.";
            }
            else
            {
                // Nếu số lượng > 0, cập nhật số lượng món ăn
                orderItem.Quantity = quantity;
                TempData["Message"] = $"{orderItem.ItemName} quantity has been updated to {quantity}.";
            }

            // Tính lại tổng tiền cho đơn hàng
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Chuyển hướng về chi tiết đơn hàng
            return RedirectToAction("OrderDetails", new { orderId });
        }





        // Xem danh sách đặt bàn

        // Bắt đầu truy vấn với các bảng liên quan
        public IActionResult ListOrder(string orderStatus, string tableStatus)
        {
            // Lấy danh sách đơn hàng kèm theo thông tin bàn
            var ordersQuery = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .AsQueryable();

            // Lọc theo trạng thái đơn hàng (Chỉ lấy các trạng thái Pending, Confirmed, Unpaid)
            if (!string.IsNullOrEmpty(orderStatus))
            {
                OrderStatusEnum parsedOrderStatus = OrderStatusEnum.Pending;  // Mặc định là Pending
                switch (orderStatus.ToLower())  // Chuyển sang dạng chữ thường để so sánh dễ dàng
                {
                    case "confirmed":
                        parsedOrderStatus = OrderStatusEnum.Confirmed;
                        break;
                    case "unpaid":
                        parsedOrderStatus = OrderStatusEnum.Unpaid;
                        break;
                }

                // Lọc chỉ những đơn hàng có trạng thái là Pending, Confirmed, hoặc Unpaid
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == parsedOrderStatus);
            }
            else
            {
                // Nếu không có filter orderStatus, chỉ lấy các đơn hàng với trạng thái Pending, Confirmed hoặc Unpaid
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == OrderStatusEnum.Pending ||
                                                      o.OrderStatus == OrderStatusEnum.Confirmed ||
                                                      o.OrderStatus == OrderStatusEnum.Unpaid);
            }

            // Lọc theo trạng thái bàn
            if (!string.IsNullOrEmpty(tableStatus))
            {
                StatusTableEnum parsedTableStatus = StatusTableEnum.Empty;  // Mặc định là Empty
                switch (tableStatus.ToLower())  // Chuyển sang dạng chữ thường để so sánh dễ dàng
                {
                    case "booked":
                        parsedTableStatus = StatusTableEnum.Booked;
                        break;
                    case "active":
                        parsedTableStatus = StatusTableEnum.Active;
                        break;
                }
                ordersQuery = ordersQuery.Where(o => o.Table != null && o.Table.Status == parsedTableStatus);
            }

            var orders = ordersQuery.ToList(); // Thực thi truy vấn
            return View(orders);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatusEnum status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đơn hàng
            order.OrderStatus = status;

            // Logic cập nhật trạng thái bàn
            if (order.Table != null)
            {
                switch (status)
                {
                    case OrderStatusEnum.Canceled:
                        order.Table.Status = StatusTableEnum.Empty;
                        break;

                    case OrderStatusEnum.Paid:
                        order.Table.Status = StatusTableEnum.Empty;
                        break;

                    case OrderStatusEnum.Unpaid:
                        order.Table.Status = StatusTableEnum.Active;
                        break;

                    default:
                        break;
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Lấy customerId từ đơn hàng
            var customerId = order.CustomerId; // Giả sử bạn có customerId trong order

            // Gửi thông báo đến nhóm của khách hàng sở hữu đơn hàng
            await _orderHubContext.Clients.Group($"customer-{customerId}")
                .SendAsync("SendOrderStatusNotification", orderId.ToString(), status.ToString());

            return Json(new { success = true, message = "Order status updated successfully!" });
        }

        public IActionResult OrderCompleted(string orderStatus = "paid")
        {
            // Lọc đơn hàng theo trạng thái (mặc định là Paid)
            var orders = _context.Orders
                .Where(o => (orderStatus == "paid" && o.OrderStatus == OrderStatusEnum.Paid) ||
                            (orderStatus == "canceled" && o.OrderStatus == OrderStatusEnum.Canceled))
                .Include(o => o.Table)  // Bao gồm thông tin bàn
                .Include(o => o.Customer)  // Bao gồm thông tin khách hàng
                .Include(o => o.OrderItems)  // Bao gồm các OrderItems
                .ToList(); // Thực thi truy vấn

            // Tính tổng tiền cho mỗi đơn hàng và thêm vào thuộc tính TotalAmount
            foreach (var order in orders)
            {
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);
            }

            // Trả về view với danh sách các đơn hàng và trạng thái đã chọn
            ViewData["SelectedOrderStatus"] = orderStatus;

            return View(orders);
        }

        public IActionResult Details(Guid orderId)
        {
            // Lấy thông tin chi tiết của đơn hàng theo orderId
            var order = _context.Orders
                .Include(o => o.Table)  // Bao gồm thông tin bàn
                .Include(o => o.Customer)  // Bao gồm thông tin khách hàng
                .Include(o => o.OrderItems)  // Bao gồm các OrderItems
                .FirstOrDefault(o => o.Id == orderId); // Lọc theo ID của đơn hàng

            if (order == null)
            {
                return NotFound();  // Nếu không tìm thấy đơn hàng
            }

            // Trả về view với thông tin chi tiết của đơn hàng
            return View(order);
        }
        public async Task<IActionResult> ListCustomers()
        {
            var users = _userManager.Users.ToList();
            var customers = new List<ApplicationUser>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    customers.Add(user);
                }
            }

            return View(customers);
        }

    }
}
