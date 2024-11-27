using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.ViewComponents
{
    public class TableList : ViewComponent
    {
        private readonly FinalWebDbContext _context;
        public TableList(FinalWebDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await GetItemsAsync();
            return View(items);
        }

        private Task<List<Table>> GetItemsAsync()
        {
            return _context.Tables.ToListAsync();
        }

    }
}
