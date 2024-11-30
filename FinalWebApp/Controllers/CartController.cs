using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using FinalWebApp.Enum;
using FinalWebApp.Session;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalWebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly FinalWebDbContext _context;

        public CartController(FinalWebDbContext context)
        {
            _context = context;
        }
        public IActionResult Cart()
        {
            // Lấy giỏ hàng từ session
            var cartItemIds = HttpContext.Session.Get<List<Guid>>("Cart") ?? new List<Guid>();

            // Lấy thông tin chi tiết sản phẩm từ database
            var items = _context.Items
                .Where(i => cartItemIds.Contains(i.Id))
                .Select(i => new CartItemViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = cartItemIds.Count(id => id == i.Id), // Tính số lượng
                    Image = i.Image
                })
                .ToList();

            var viewModel = new CartViewModel
            {
                Items = items
            };

            return View(viewModel);

        }
        [HttpPost]
        public IActionResult RemoveFromCart(Guid id)
        {
            // Lấy giỏ hàng từ session
            var cart = HttpContext.Session.Get<List<Guid>>("Cart") ?? new List<Guid>();

            // Xóa sản phẩm đầu tiên khớp với ID
            cart.Remove(id);

            // Lưu lại giỏ hàng vào session
            HttpContext.Session.Set("Cart", cart);

            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public IActionResult AddToCart(Guid itemId)
        {
            var item = _context.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return NotFound();
            }
            // Lấy danh sách giỏ hàng từ session (hoặc tạo mới nếu chưa có)
            var cart = HttpContext.Session.Get<List<Guid>>("Cart") ?? new List<Guid>();

            // Thêm sản phẩm vào giỏ hàng
            cart.Add(itemId);

            // Lưu lại giỏ hàng vào session
            HttpContext.Session.Set("Cart", cart);

            TempData["SuccessMessage"] = "Item added to cart!";
            return RedirectToAction("Index", "Customer");
        }
        [HttpPost]
        public IActionResult CreateOrder(Guid tableId, DateTime diningTime)
        {
            // Lấy giỏ hàng từ session
            var cartItemIds = HttpContext.Session.Get<List<Guid>>("Cart") ?? new List<Guid>();

            if (!cartItemIds.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty!";
                return RedirectToAction("Cart");
            }

            // Kiểm tra bàn có sẵn
            var table = _context.Tables.FirstOrDefault(t => t.Id == tableId && t.Status == Enum.StatusTableEnum.Empty);
            if (table == null)
            {
                TempData["ErrorMessage"] = "The selected table is not available.";
                return RedirectToAction("SelectTable");
            }

            // Lấy thông tin sản phẩm từ database
            var items = _context.Items
      .Where(i => cartItemIds.Contains(i.Id))
      .Select(i => new
      {
          i.Id,
          i.Name,
          i.Price,
          Quantity = cartItemIds.Count(id => id == i.Id),
          i.Image,
          i.CategoryId // Lấy CategoryId
      })
      .ToList();

            var totalAmount = items.Sum(item => item.Price * item.Quantity);

            // Kiểm tra xem CategoryId có tồn tại trong bảng Categories không
            foreach (var item in items)
            {
                var categoryExists = _context.Categories.Any(c => c.Id == item.CategoryId);
                if (!categoryExists)
                {
                    TempData["ErrorMessage"] = $"The category with Id {item.CategoryId} does not exist.";
                    return RedirectToAction("Cart");
                }
            }

            // Tạo đơn hàng
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderStatus = OrderStatusEnum.Pending,
                TableId = tableId,
                CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier), // Lấy đúng CustomerId từ ASP.NET Identity
                DiningTime = diningTime, // Lưu thời gian dùng bữa
                OrderItems = items.Select(item => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    CategoryId = item.CategoryId, // Gán đúng CategoryId
                    Quantity = item.Quantity,
                    Price = item.Price,
                    ItemName = item.Name,
                    ItemImage = item.Image
                }).ToList()
            };

            // Thêm đơn hàng vào database
            try
            {
                _context.Orders.Add(order);
                table.Status = StatusTableEnum.Booked; // Cập nhật trạng thái bàn
                _context.SaveChanges();

                // Xóa giỏ hàng sau khi đặt hàng
                HttpContext.Session.Remove("Cart");

                TempData["SuccessMessage"] = "Your order has been placed successfully!";
                return RedirectToAction("OrderDetails", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while placing your order.";
                return RedirectToAction("Cart");
            }
        }

            public IActionResult SelectTable()
        {
            // Lấy danh sách bàn có sẵn
            var availableTables = _context.Tables.Where(t => t.Status == Enum.StatusTableEnum.Empty).ToList();
            return View(availableTables);
        }
        public IActionResult OrderDetails(Guid orderId)
        {
            // Lấy thông tin đơn hàng
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Include(o => o.Table)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Truy vấn thông tin người dùng đóng vai trò khách hàng
            var customer = _context.Users.FirstOrDefault(u => u.Id == order.CustomerId);

            // Tạo ViewModel cho trang chi tiết đơn hàng
            var viewModel = new OrderViewModel
            {
                Items = order.OrderItems.Select(oi => new CartItemViewModel
                {
                    Id = oi.ItemId,
                    Name = oi.ItemName,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    Image = oi.ItemImage
                }).ToList(),
                TotalAmount = order.TotalAmount,
                CustomerName = customer?.UserName ?? "Guest", // Lấy tên khách hàng từ User
                OrderDate = order.OrderDate,
                OrderTime = order.OrderDate,
                TableNumber = order.Table?.Name ?? "Not Assigned"
            };
            return View(viewModel);
        }
    }
}
