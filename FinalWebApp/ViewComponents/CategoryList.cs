using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.ViewComponents
{
    public class CategoryList : ViewComponent
    {
        private readonly FinalWebDbContext _context;
        public CategoryList(FinalWebDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await GetItemsAsync();
            return View(items);
        }

        private Task<List<Category>> GetItemsAsync()
        {
            return _context.Category.ToListAsync();
        }

    }
}
