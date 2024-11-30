using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using FinalWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.Controllers
{
    [Authorize(Roles ="ADMIN,MANAGER")]
    public class TableController : Controller
    {
        private readonly FinalWebDbContext _context;
        public TableController(FinalWebDbContext context)
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
        public async Task<ActionResult> Create(TableViewModel tableVM)
        {
            try
            {
                var newtable = new Table
                {
                    Name = tableVM.Name,
                    Capacity = tableVM.Capacity,
                    Status = tableVM.Status,
                };
                _context.Tables.Add(newtable);
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

                return View(nameof(Create), tableVM);
                //return View("Create", artist);
            }
        }
        public async Task<ActionResult> Edit(Guid idTable)
        {
            //var artist = await _context.Artists.FindAsync(idArtist);
            var table = await _context.Tables
                .Where(a => a.Id.Equals(idTable))
                .SingleOrDefaultAsync();
            var tableVM = new TableViewModel
            {
                Id = table.Id,
                Name = table.Name,
                Capacity = table.Capacity,
                Status = table.Status,
            };
            return View(nameof(Create), tableVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind("Id, Name")]ArtistViewModel artistVM)
        public async Task<ActionResult> Edit(TableViewModel tableVM)
        {
            try
            {
                //_context.Entry(artistVM).State = EntityState.Modified;
                var table = await _context.Tables.FindAsync(tableVM.Id);
                if (table == null) { return BadRequest(); }

                table.Name = tableVM.Name?.Trim();
                table.Capacity = tableVM.Capacity;
                table.Status = tableVM.Status;

                await _context.SaveChangesAsync();
                TempData["Message"] = "Update Successful";
                //=== Lưu thành công thì quay về cho tạo mới ===//
                return RedirectToAction(nameof(Create));
            }
            catch
            {
                TempData["Message"] = "False";
                return View(nameof(Create), tableVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(TableViewModel tableVM)
        {
            if (tableVM == null || tableVM.Id == Guid.Empty)
            {
                return BadRequest(); // Trả về lỗi nếu ID không hợp lệ
            }

            try
            {
                var table = await _context.Tables.FindAsync(tableVM.Id);
                if (table == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy
                }

                _context.Tables.Remove(table); // Xóa category
                await _context.SaveChangesAsync();
                TempData["Message"] = "Delete Successful"; // Thông báo thành công
                return RedirectToAction(nameof(Create), tableVM); // Quay về danh sách
            }
            catch (Exception ex) // Bắt lỗi cụ thể
            {
                TempData["Message"] = "False " + ex.Message;
                return View(); // Hoặc có thể trả về một view cụ thể nếu cần
            }
        }
    }
}
