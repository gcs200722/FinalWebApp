using FinalWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using FinalWebApp.Commons;
using Constants = FinalWebApp.Commons.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinalWebApp.ViewModels;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Authorization;
namespace FinalWebApp.Controllers
{
    [Authorize(Roles ="ADMIN,MANAGER")]
    public class ItemController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly FinalWebDbContext _context;
        private readonly ILogger<ItemController> _logger;
        public ItemController(FinalWebDbContext context, IWebHostEnvironment environment, ILogger<ItemController> logger)
        {
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetCategorySelectList();
            return View();
        }
        private async Task<SelectList> GetCategorySelectList()
        {
            var listCategory = await _context.Categories
                .Select(a => new
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .ToListAsync();
            return new SelectList(listCategory, "Id", "Name");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemViewModel itemViewModel)
         {
            // Kiểm tra tính hợp lệ của ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategorySelectList();
                return View(itemViewModel);
            }

            try
            {
                string uploadedFileName = null;
                // Upload file và kiểm tra tên file
                if (itemViewModel.Image != null && itemViewModel.Image.Length > 0)
                {
                    // Nếu có tệp, gọi phương thức UploadFile
                    uploadedFileName = await UploadFile(itemViewModel.Image);
                    if (string.IsNullOrEmpty(uploadedFileName))
                    {
                        ModelState.AddModelError("", "File upload failed or invalid file.");
                        ViewBag.Categories = await GetCategorySelectList();
                        return View(itemViewModel);
                    }
                }

                    // Tạo mới đối tượng Item
                    var newItem = new Item
                {
                    Name = itemViewModel.Name,
                    Description = itemViewModel.Description,
                    Price = itemViewModel.Price,
                    CategoryId = itemViewModel.CategoryId,
                    Image = uploadedFileName,  // Lưu tên file hình ảnh
                };

                // Thêm vào cơ sở dữ liệu và lưu
                _context.Items.Add(newItem);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công
                TempData["SuccessMessage"] = "Item has been successfully created.";

                // Chuyển hướng về trang Create sau khi lưu thành công
                return RedirectToAction(nameof(Create));
            }
            catch (DbUpdateException dbEx)
            {
                // Ghi log lỗi DbUpdateException nếu có
                ModelState.AddModelError("", "There was a problem saving data to the database. Please try again.");
                _logger.LogError(dbEx, "Error during database update");
                ViewBag.Categories = await GetCategorySelectList();
                return View(itemViewModel);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chung
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                _logger.LogError(ex, "Unexpected error occurred during item creation");
                ViewBag.Categories = await GetCategorySelectList();
                return View(itemViewModel);
            }
        }

        public async Task<IActionResult> Edit(Guid idItem, IFormFile file)
        {
            var item = await _context.Items.FindAsync(idItem);
            if (item == null) { return BadRequest(); }
            var itemVM = new ItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                CategoryId = item.CategoryId,
                //FileName = music.FileName,
            };
            ViewBag.Categories = await GetCategorySelectList();
            //return View("Create", music);
            return View(nameof(Create), itemVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Data.Entities.Music music)
        public async Task<IActionResult> Edit(ItemViewModel itemViewModel)
        {
            try
            {
                //_context.Entry(music).State = EntityState.Modified;
                var item = await _context.Items.FindAsync(itemViewModel.Id);
                if (item == null)
                {
                    ModelState.AddModelError("", "Item not found.");
                    ViewBag.Categories = await GetCategorySelectList();
                    return View(itemViewModel); // Trả về trang Edit với thông báo lỗi
                }

                //=== Sửa trường nào thì cập nhật trường đó ===//
                item.Name = itemViewModel.Name;
                item.Description = itemViewModel.Description;
                item.Price = itemViewModel.Price;
                item.CategoryId = itemViewModel.CategoryId;
                if (itemViewModel.Image != null)
                {
                    var uploadedFileName = await UploadFile(itemViewModel.Image);
                    if (string.IsNullOrEmpty(uploadedFileName))
                    {
                        ModelState.AddModelError("", "File upload failed or invalid file.");
                        ViewBag.Categories = await GetCategorySelectList();
                        return View(nameof(Create), itemViewModel);
                    }

                    // Cập nhật tên file vào item
                    item.Image = uploadedFileName;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            catch
            {
                ViewBag.Categories = await GetCategorySelectList();
                return View(nameof(Create), itemViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(ItemViewModel itemVM)
        {
            if (itemVM == null || itemVM.Id == Guid.Empty)
            {
                return BadRequest(); // Trả về lỗi nếu ID không hợp lệ
            }

            try
            {
                var item = await _context.Items.FindAsync(itemVM.Id);
                if (item == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy
                }

                _context.Items.Remove(item); // Xóa category
                await _context.SaveChangesAsync();
                TempData["Message"] = "Xóa thành công"; // Thông báo thành công
                return RedirectToAction(nameof(Create), itemVM); // Quay về danh sách
            }
            catch (Exception ex) // Bắt lỗi cụ thể
            {
                TempData["Message"] = "Thất bại: " + ex.Message;
                return View(); // Hoặc có thể trả về một view cụ thể nếu cần
            }
        }
    }
}

