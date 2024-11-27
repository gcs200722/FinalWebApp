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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //var items = await GetItemsAsync();
            var items = await _context.Item
                .Include(c => c.Category)
                .ToListAsync();
            return View(items);
        }

        /*private Task<List<Item>> GetItemsAsync()
        {
            return _context.Items.ToListAsync();
        }*/
    }
}
