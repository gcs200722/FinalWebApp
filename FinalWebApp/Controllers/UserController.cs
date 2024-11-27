using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FinalWebApp.Models;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Http;
using FinalWebApp.Data;
using FinalWebApp.Commons; // Các model tùy chỉnh như RegisterViewModel, LoginViewModel

namespace FinalWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _environment;
        private readonly FinalWebDbContext _context;
        private readonly ILogger<ItemController> _logger;
        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, FinalWebDbContext context, IWebHostEnvironment environment, ILogger<ItemController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _environment = environment;
            _logger = logger;

        }
        public async Task<string> UploadFile(IFormFile file)
        {
            if (file?.Length > 0)
            {
                // Tạo một tên tệp mới bằng GUID
                var fileGuidName = Guid.NewGuid().ToString();

                // Kiểm tra tên file
                var fileName = file.FileName;

                // Kiểm tra nếu tên tệp rỗng hoặc null
                if (string.IsNullOrEmpty(fileName))
                {
                    return null; // Trả về null nếu không có tên tệp
                }

                // Lấy phần mở rộng của tệp
                var fileExtension = Path.GetExtension(fileName)?.ToLower();

                // Kiểm tra phần mở rộng hợp lệ
                if (string.IsNullOrEmpty(fileExtension) || !Constants.Valid_Extensions.Contains(fileExtension))
                {
                    return null; // Trả về null nếu phần mở rộng không hợp lệ
                }

                // Tạo tên tệp hoàn chỉnh với GUID và phần mở rộng
                var fileGuidNameWithExtension = $"{fileGuidName}{fileExtension}";

                // Định nghĩa đường dẫn để lưu tệp
                var webRoot = _environment.WebRootPath.Normalize();
                var physicalItemPath = Path.Combine(webRoot, "Image/");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(physicalItemPath))
                {
                    Directory.CreateDirectory(physicalItemPath);
                }

                // Đường dẫn vật lý đầy đủ của tệp
                var physicalPath = Path.Combine(physicalItemPath, fileGuidNameWithExtension);

                try
                {
                    // Lưu tệp một cách không đồng bộ
                    using (var stream = System.IO.File.Create(physicalPath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Trả về tên tệp đã lưu
                    return fileGuidNameWithExtension;
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu có
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                    return null; // Trả về null nếu có lỗi trong quá trình lưu file
                }
            }

            return null; // Trả về null nếu không có tệp nào được tải lên
        }

        #region Đăng Ký Người Dùng (Register)
        // GET: /User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { 
                    UserName = model.Email,
                    Email = model.Email,
                    Fullname = model.Fullname, // Thêm Fullname
                    Gender = model.Gender,     // Thêm Gender
                    NumberPhone = model.NumberPhone, // Thêm NumberPhone
                    DateOfBirth = model.DateOfBirth // Thêm DateOfBirth               
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Đăng nhập ngay sau khi đăng ký
                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
        #endregion

        #region Đăng Nhập Người Dùng (Login)
        // GET: /User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }
        #endregion

        #region Đăng Xuất (Logout)
        // POST: /User/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Quản Lý Hồ Sơ Người Dùng (Profile Management)
        // GET: /User/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User); // Lấy người dùng hiện tại
            if (user == null)
            {
                return NotFound();
            }

            // Khởi tạo model với dữ liệu người dùng

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                Fullname = user.Fullname,
                Gender = user.Gender,
                DateOfBirth = (DateTime)user.DateOfBirth,
                NumberPhone = user.NumberPhone,
                Avatar = null, // Nếu đã có ảnh đại diện, sẽ lấy từ đây
                 AvatarUrl = user.Avatar
            };
            ViewData["AvatarUrl"] = user.Avatar;
            return View(model);
        }

        // POST: /User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User); // Lấy người dùng hiện tại
                if (user == null)
                {
                    return NotFound();
                }
                try
                {
                    // Kiểm tra nếu người dùng có tải lên ảnh đại diện mới
                    if (model.Avatar != null && model.Avatar.Length > 0)
                    {
                        // Gọi phương thức UploadFile để tải ảnh lên và lấy tên file
                        string uploadedFileName = await UploadFile(model.Avatar);
                        if (string.IsNullOrEmpty(uploadedFileName))
                        {
                            ModelState.AddModelError("", "Tải file lên thất bại hoặc file không hợp lệ.");
                            return View(model);
                        }

                        // Cập nhật tên ảnh đại diện trong cơ sở dữ liệu
                        user.Avatar = uploadedFileName;
                    }

                    // Cập nhật thông tin người dùng
                    user.Fullname = model.Fullname;
                    user.Gender = model.Gender;
                    user.DateOfBirth = model.DateOfBirth;
                    user.NumberPhone = model.NumberPhone;

                    // Cập nhật người dùng trong cơ sở dữ liệu
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Profile");
                    }

                    // Nếu có lỗi trong quá trình cập nhật
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            return View(model);
        }

        #endregion

        #region Thay Đổi Mật Khẩu (Change Password)
        // GET: /User/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User); // Lấy người dùng hiện tại
                if (user == null)
                {
                    return NotFound();
                }

                // Kiểm tra mật khẩu hiện tại
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user); // Làm mới đăng nhập sau khi thay đổi mật khẩu
                    TempData["SuccessMessage"] = "Your password has been changed successfully.";
                    return RedirectToAction("Profile"); // Quay lại trang hồ sơ người dùng
                }

                // Nếu có lỗi trong quá trình thay đổi mật khẩu
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        #endregion
    }
}
