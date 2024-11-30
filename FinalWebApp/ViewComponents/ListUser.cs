// File: ViewComponents/ListUserViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using FinalWebApp.Data;  // Chắc chắn rằng bạn đã có DbContext để truy cập dữ liệu
using Microsoft.EntityFrameworkCore;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinalWebApp.ViewComponents
{
    public class ListUserViewComponent : ViewComponent
    {
        private readonly FinalWebDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ListUserViewComponent(FinalWebDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);

            // Lấy tất cả người dùng từ cơ sở dữ liệu
            var users = await _context.Users
                .OrderByDescending(u => u.Id)
                .ToListAsync();
            foreach (var user in users)
            {
                user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }
            // Kiểm tra nếu người dùng có vai trò Admin
            if (currentRoles.Contains("ADMIN"))
            {
                // Nếu là Admin, trả về toàn bộ người dùng và vai trò của họ
                foreach (var user in users)
                {
                    // Lấy các vai trò của người dùng và gán vào thuộc tính Roles
                    user.Roles = (await _userManager.GetRolesAsync(user)).ToList(); // Sử dụng ToList() ở đây
                }
            }
            // Kiểm tra nếu người dùng có vai trò Manager
            else if (currentRoles.Contains("MANAGER"))
            {
                // Nếu là Manager, lọc chỉ những người có vai trò Customer hoặc Staff
                users = users.Where(user =>
                    (user.Roles.Contains("CUSTOMER") || user.Roles.Contains("STAFF"))
                    ).ToList(); // Lọc người dùng có vai trò "Customer" hoặc "Staff"

                // Cập nhật roles cho người dùng sau khi lọc
                foreach (var user in users)
                {
                    user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                }
            }

            // Trả về view với danh sách người dùng và vai trò
            return View(users);
        }



    }
}
