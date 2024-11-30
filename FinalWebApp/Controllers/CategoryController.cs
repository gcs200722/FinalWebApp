using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly FinalWebDbContext _context;
        public CategoryController(FinalWebDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            TempData.Keep();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryViewModel categoryViewModel)
        {
            try
            {
                var newcategory = new Category
                {
                    Name = categoryViewModel.Name,
                };
                _context.Categories.Add(newcategory);
                await _context.SaveChangesAsync();

                //ViewBag.Message = "Thêm thành công";
                TempData["Message"] = "More success";
                //=== Lưu thành công thì quay về cho tạo mới ===//
                return RedirectToAction(nameof(Create));
            }
            catch
            {
                //=== Không thành công thì kiểm tra lại dữ liệu
                //ViewBag.Message = "Thất bại";
                TempData["Message"] = "False";

                return View(nameof(Create), categoryViewModel);
                //return View("Create", artist);
            }
        }
        public async Task<ActionResult> Edit(Guid idCategory)
        {
            //var artist = await _context.Artists.FindAsync(idArtist);
            var category = await _context.Categories
                .Where(a => a.Id.Equals(idCategory))
                .SingleOrDefaultAsync();
            var categoryVM = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
            };
            return View(nameof(Create), categoryVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind("Id, Name")]ArtistViewModel artistVM)
        public async Task<ActionResult> Edit(CategoryViewModel categoryVM)
        {
            try
            {
                //_context.Entry(artistVM).State = EntityState.Modified;
                var category = await _context.Categories.FindAsync(categoryVM.Id);
                if (category == null) { return BadRequest(); }

                category.Name = categoryVM.Name?.Trim();

                await _context.SaveChangesAsync();
                TempData["Message"] = "Update Successful";
                //=== Lưu thành công thì quay về cho tạo mới ===//
                return RedirectToAction(nameof(Create));
            }
            catch
            {
                TempData["Message"] = "False";
                return View(nameof(Create), categoryVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(CategoryViewModel categoryVM)
        {
            if (categoryVM == null || categoryVM.Id == Guid.Empty)
            {
                return BadRequest(); // Trả về lỗi nếu ID không hợp lệ
            }

            try
            {
                var category = await _context.Categories.FindAsync(categoryVM.Id);
                if (category == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy
                }

                _context.Categories.Remove(category); // Xóa category
                await _context.SaveChangesAsync();
                TempData["Message"] = "Delete Successful"; // Thông báo thành công
                return RedirectToAction(nameof(Create), categoryVM); // Quay về danh sách
            }
            catch (Exception ex) // Bắt lỗi cụ thể
            {
                TempData["Message"] = "False " + ex.Message;
                return View(); // Hoặc có thể trả về một view cụ thể nếu cần
            }
        }
    }
}
