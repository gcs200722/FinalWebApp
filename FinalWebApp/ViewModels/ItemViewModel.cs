using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    public class ItemViewModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Item Name is required")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        public Guid CategoryId { get; set; }
        public IFormFile? Image { get; set; } // Đây là file upload cho hình ảnh

        // Tùy chọn: Bạn có thể thêm thuộc tính tên danh mục để hiển thị tên danh mục đã chọn trong form
        public string? CategoryName { get; set; }

        // Nếu bạn cần danh sách các danh mục cho dropdown
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }


}
