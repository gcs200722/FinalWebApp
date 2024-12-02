using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FinalWebApp.Data.Entities;
using FinalWebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FinalWebApp.Controllers
{
    [Authorize(Roles ="ADMIN,MANAGER,STAFF")]
    public class UserManagerController : Controller
    {
        private readonly FinalWebDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagerController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, FinalWebDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }
        // Hành động đăng ký người dùng
        [Authorize(Roles ="MANAGER,ADMIN,STAFF")]
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult ViewUser()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already registered.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Fullname = model.Fullname, // Thêm Fullname
                    Gender = model.Gender,      // Thêm Gender
                    NumberPhone = model.NumberPhone, // Thêm NumberPhone
                    DateOfBirth = model.DateOfBirth // Thêm DateOfBirth               
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán vai trò cho người dùng (Ví dụ: 'User')
                    await _userManager.AddToRoleAsync(user, "Customer");
                    if (User.IsInRole("STAFF"))
                    {
                        TempData["SuccessMessage"] = "Operation successful!";
                        return RedirectToAction("ListCustomers","Staff");
                    }
                    if (User.IsInRole("ADMIN")) { return RedirectToAction("ViewUser", "UserManager"); }
                        
                }

                // Nếu có lỗi trong quá trình đăng ký
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

                return View(model);
        }

        // Hành động chỉnh sửa người dùng
        public async Task<IActionResult> Edit(string id )
        {
            // Chuyển đổi Guid thành chuỗi
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(); // Nếu không tìm thấy, trả về NotFound
            }

            var model = new RegisterViewModel
            {
                Id = user.Id,
                Fullname = user.Fullname,
                Email = user.Email,
                Gender = user.Gender,
                NumberPhone = user.NumberPhone,
                DateOfBirth = user.DateOfBirth
            };

            return View("Create",model); // Trả về view với model
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id.ToString());

                if (user == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin người dùng
                user.Fullname = model.Fullname;
                user.Email = model.Email;
                user.DateOfBirth = model.DateOfBirth;
                user.NumberPhone = model.NumberPhone;
                user.Gender = model.Gender;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Message"] = "User information updated successfully!";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View("Create",model);
        }


        // Hành động xóa người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete the user.";
            }

            return RedirectToAction("ViewUser"); // Chuyển hướng về trang danh sách người dùng sau khi xóa
        }


        // Hành động gán vai trò cho người dùng
        public async Task<IActionResult> AssignRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AssignRoleViewModel
            {
                UserId = user.Id,
                Roles = await _roleManager.Roles.ToListAsync()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách các vai trò hiện tại của người dùng
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Loại bỏ các vai trò cũ
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Thêm các vai trò mới
            foreach (var role in model.SelectedRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    ModelState.AddModelError(string.Empty, $"Role '{role}' does not exist.");
                    return View(model);
                }

                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }
            }

            TempData["Message"] = "Roles updated successfully!";
            return RedirectToAction("ViewUser", "UserManager");
        }

    }
}
