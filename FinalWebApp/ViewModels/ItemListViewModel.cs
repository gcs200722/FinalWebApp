using FinalWebApp.Data.Entities;

namespace FinalWebApp.ViewModels
{
    public class ItemListViewModel
    {
        public IEnumerable<Item> Items { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public Guid? SelectedCategoryId { get; set; }
    }
}
