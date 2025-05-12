using Microsoft.AspNetCore.Mvc;
using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace FinalWebApp.ViewComponents
{
    public class ListUserViewComponent : ViewComponent
    {
        private readonly FinalWebDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListUserViewComponent(
            FinalWebDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int page = 1, int pageSize = 5, string sortByRole = "ALL")
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);

            // Lấy danh sách tất cả người dùng
            var usersQuery = _context.Users.AsQueryable();

            // Nếu filter theo vai trò, chỉ lấy người dùng có vai trò tương ứng
            if (sortByRole != "ALL")
            {
                usersQuery = usersQuery
                    .Where(u => u.Roles.Any(role => role == sortByRole)) // Kiểm tra xem vai trò có tồn tại trong danh sách Roles của user
                    .OrderByDescending(u => u.Id);
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(u => u.Id);
            }

            // Lấy danh sách người dùng theo query đã lọc
            var users = await usersQuery.ToListAsync();

            // Thêm vai trò vào mỗi người dùng
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Roles = roles.ToList();
            }

            // Phân trang người dùng
            var pagedUsers = users.ToPagedList(page, pageSize);

            // Truyền biến sortByRole vào ViewBag để hiển thị trên dropdown
            ViewBag.SortByRole = sortByRole;

            return View(pagedUsers);
        }

    }
}
