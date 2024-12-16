using FinalWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.ViewComponents
{
    public class ItemList : ViewComponent
    {
        private readonly FinalWebDbContext _context;
        public ItemList(FinalWebDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(Guid? categoryId)
        {
            var itemsQuery = _context.Items.Include(i => i.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                itemsQuery = itemsQuery.Where(i => i.CategoryId == categoryId.Value);
            }

            var items = await itemsQuery.ToListAsync();
            return View(items);
        }

    }
}
