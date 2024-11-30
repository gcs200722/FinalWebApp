namespace FinalWebApp.Data.Dto
{

    public class RevenueReportDto
    {
        public string Period { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<MenuItemReport> MenuItems { get; set; }  // Danh sách các món ăn
    }

    public class MenuItemReport
    {
        public string MenuItem { get; set; }  // Tên món ăn
        public int QuantitySold { get; set; } // Số lượng bán
        public string Category { get; set; }  // Tên danh mục
    }

}
