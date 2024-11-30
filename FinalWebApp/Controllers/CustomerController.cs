using FinalWebApp.Data;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalWebApp.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly FinalWebDbContext _context;

        public CustomerController(FinalWebDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(Guid? categoryId)
        {
            var items = _context.Items.AsQueryable();

            if (categoryId.HasValue)
            {
                items = items.Where(i => i.CategoryId == categoryId.Value);
            }
            var categories = await _context.Categories.ToListAsync();

            var viewModel = new ItemListViewModel
            {
                Items = await items.Include(i => i.Category).ToListAsync(),
                Categories = categories,
                SelectedCategoryId = categoryId
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var item = await _context.Items.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }
        public IActionResult OrderHistory()
        {
            // Lấy CustomerId từ hiện tại của người dùng
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách đơn hàng của người dùng từ database
            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId) // Chỉ lấy đơn hàng của người dùng hiện tại
                .Include(o => o.OrderItems) // Bao gồm các chi tiết món ăn trong đơn hàng
                .OrderByDescending(o => o.OrderDate) // Sắp xếp theo ngày đặt hàng
                .ToList();

            return View(orders); // Trả về View với danh sách đơn hàng
        }
    }
}
