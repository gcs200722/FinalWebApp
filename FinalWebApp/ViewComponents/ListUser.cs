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
            // Lấy tất cả người dùng từ cơ sở dữ liệu
            var users = await _context.Users
               .OrderByDescending(u => u.Id)
               .ToListAsync();
            // Lấy các vai trò cho từng người dùng
            foreach (var user in users)
            {
                user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }

            return View(users); // Trả về view với danh sách người dùng và vai trò
        }
    }
}
