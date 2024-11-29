using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using FinalWebApp.Enum;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.Controllers
{
    public class StaffController : Controller
    {
        private readonly FinalWebDbContext _context;

        public StaffController(FinalWebDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> AddItem(Guid orderId)
        {
            var items = _context.Item.AsQueryable();
            var categories = await _context.Category.ToListAsync();

            var viewModel = new ItemListViewModel
            {
                Items = await items.Include(i => i.Category).ToListAsync(),
                Categories = categories,
                SelectedCategoryId = null
            };

            // Truyền orderId vào ViewData hoặc ViewBag nếu cần sử dụng trên view
            ViewData["OrderId"] = orderId;

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult AddToOrder(Guid itemId, Guid orderId)
        {
            // Kiểm tra đơn hàng có tồn tại không
            var order = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                                       .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Lấy món ăn từ cơ sở dữ liệu
            var item = _context.Item.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu món ăn đã có trong đơn hàng
            var existingOrderItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            if (existingOrderItem != null)
            {
                // Nếu món ăn đã có, bạn có thể cập nhật số lượng thay vì thêm mới
                existingOrderItem.Quantity += 1;
                _context.SaveChanges();

                // Cập nhật lại tổng tiền đơn hàng
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                // Thêm thông báo cho người dùng
                TempData["Message"] = $"{item.Name} has been updated in your order.";
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
                    ItemImage = item.Image // Cập nhật hình ảnh món ăn
                };

                // Thêm món ăn mới vào danh sách OrderItems
                order.OrderItems.Add(orderItem);

                // Cập nhật lại tổng tiền đơn hàng
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                _context.OrderItems.Add(orderItem);
                _context.SaveChanges();

                // Thêm thông báo cho người dùng
                TempData["Message"] = $"{item.Name} has been added to your order.";
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Chuyển hướng đến trang chi tiết đơn hàng
            return RedirectToAction("AddItem", new { orderId = orderId });
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

            // Lọc theo trạng thái đơn
            if (!string.IsNullOrEmpty(orderStatus))
            {
                OrderStatusEnum parsedOrderStatus = OrderStatusEnum.Pending;  // Mặc định là Pending
                switch (orderStatus.ToLower())  // Chuyển sang dạng chữ thường để so sánh dễ dàng
                {
                    case "confirmed":
                        parsedOrderStatus = OrderStatusEnum.Confirmed;
                        break;
                    case "canceled":
                        parsedOrderStatus = OrderStatusEnum.Canceled;
                        break;
                    case "unpaid":
                        parsedOrderStatus = OrderStatusEnum.Unpaid;
                        break;
                    case "paid":
                        parsedOrderStatus = OrderStatusEnum.Paid;
                        break;
                }
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == parsedOrderStatus);
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
            // Lấy đơn hàng theo ID
            var order = await _context.Orders
                                      .Include(o => o.Table)
                                      .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
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

                    default:
                        // Các trạng thái khác không thay đổi trạng thái bàn
                        break;
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return RedirectToAction("ListOrder");
        }

    }
}
