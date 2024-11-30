using FinalWebApp.Data.Entities;
using FinalWebApp.Data.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalWebApp.ViewModels;
using FinalWebApp.Commons;
using AutoMapper;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace FinalWebApp.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ReportGenerator _reportGenerator;
        private readonly FinalWebDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ManagerController(FinalWebDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ReportGenerator reportGenerator)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _reportGenerator = reportGenerator;
        }
        public IActionResult RevenueReport(DateTime? startDate, DateTime? endDate, string interval = "Daily")
        {
            // Lọc đơn hàng theo khoảng thời gian
            var orders = _context.Orders
                .Include(o => o.OrderItems)  // Bao gồm OrderItems để lấy thông tin món ăn
                .ThenInclude(oi => oi.Item)  // Bao gồm Item để lấy thông tin tên món ăn và category
                .ThenInclude(i => i.Category) // Bao gồm Category của món ăn
                .Where(o => (!startDate.HasValue || o.OrderDate >= startDate) &&
                           (!endDate.HasValue || o.OrderDate <= endDate))
                .ToList();

            List<RevenueReportDto> revenueData = new List<RevenueReportDto>();

            var groupedData = interval switch
            {
                "Daily" => orders
                    .GroupBy(o => o.OrderDate.Date)  // Nhóm theo ngày
                    .Select(g => new RevenueReportDto
                    {
                        Period = g.Key.ToString("dd/MM/yyyy"),
                        TotalRevenue = g.Sum(o => o.TotalAmount),  // Tính tổng doanh thu trong ngày
                        MenuItems = g.SelectMany(o => o.OrderItems)
                                     .Where(oi => oi.Item != null)  // Kiểm tra nếu Item không null
                                     .GroupBy(oi => oi.Item.Name)  // Nhóm theo tên món ăn
                                     .Select(oiGroup => new MenuItemReport
                                     {
                                         MenuItem = oiGroup.Key,
                                         QuantitySold = oiGroup.Sum(oi => oi.Quantity),  // Tính tổng số lượng của từng món
                                         Category = string.Join(", ", oiGroup.Select(oi => oi.Item.Category?.Name).Distinct())
                                     }).ToList() ?? new List<MenuItemReport>() // Khởi tạo danh sách trống nếu không có dữ liệu
                    }).ToList(),
                "Monthly" => orders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })  // Nhóm theo tháng
                    .Select(g => new RevenueReportDto
                    {
                        Period = $"{g.Key.Month}/{g.Key.Year}",
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        MenuItems = g.SelectMany(o => o.OrderItems)
                                     .Where(oi => oi.Item != null)
                                     .GroupBy(oi => oi.Item.Name)
                                     .Select(oiGroup => new MenuItemReport
                                     {
                                         MenuItem = oiGroup.Key,
                                         QuantitySold = oiGroup.Sum(oi => oi.Quantity),
                                         Category = string.Join(", ", oiGroup.Select(oi => oi.Item.Category?.Name).Distinct())
                                     }).ToList() ?? new List<MenuItemReport>()
                    }).ToList(),

                _ => orders
                    .GroupBy(o => o.OrderDate.Date)  // Nhóm theo ngày
                    .Select(g => new RevenueReportDto
                    {
                        Period = g.Key.ToString("dd/MM/yyyy"),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        MenuItems = g.SelectMany(o => o.OrderItems)
                                     .Where(oi => oi.Item != null)
                                     .GroupBy(oi => oi.Item.Name)
                                     .Select(oiGroup => new MenuItemReport
                                     {
                                         MenuItem = oiGroup.Key,
                                         QuantitySold = oiGroup.Sum(oi => oi.Quantity),
                                         Category = string.Join(", ", oiGroup.Select(oi => oi.Item.Category?.Name).Distinct())
                                     }).ToList() ?? new List<MenuItemReport>()
                    }).ToList()
            };

            return View(groupedData);
        }
        [HttpPost]
        public IActionResult GenerateRevenueReport(List<string> MenuItems, string Period, decimal TotalRevenue)
        {
            // Kiểm tra nếu MenuItems là null hoặc rỗng
            if (MenuItems == null || MenuItems.Count == 0)
            {
                // Nếu không có món ăn nào, khởi tạo danh sách trống
                MenuItems = new List<string>();
            }

            // Tạo đối tượng RevenueReportDto
            var revenueReport = new RevenueReportDto
            {
                Period = Period,
                TotalRevenue = TotalRevenue,
                MenuItems = new List<MenuItemReport>()
            };

            // Truy vấn cơ sở dữ liệu để lấy thông tin chi tiết về các món ăn
            foreach (var menuItemName in MenuItems)
            {
                // Truy vấn cơ sở dữ liệu để lấy thông tin chi tiết về món ăn
                var menuItem = _context.Items
                                       .Where(m => m.Name == menuItemName)
                                       .Include(m => m.Category)  // Nếu có quan hệ Category, dùng Include
                                       .FirstOrDefault();  // Lấy món ăn đầu tiên hoặc null nếu không tìm thấy

                // Kiểm tra menuItem có null không
                if (menuItem != null && menuItem.Category != null)
                {
                    // Truy vấn số lượng bán từ bảng OrderDetails (giả sử có bảng OrderDetails)
                    var quantitySold = _context.OrderItems
                                                .Where(od => od.ItemId == menuItem.Id)
                                                .Sum(od => od.Quantity);

                    // Tạo đối tượng MenuItemReport với dữ liệu từ cơ sở dữ liệu
                    var menuItemReport = new MenuItemReport
                    {
                        MenuItem = menuItem.Name,            // Tên món ăn
                        QuantitySold = quantitySold,         // Số lượng bán
                        Category = menuItem.Category.Name    // Danh mục
                    };

                    // Thêm vào danh sách báo cáo
                    revenueReport.MenuItems.Add(menuItemReport);
                }
                else
                {
                    // Nếu không tìm thấy món ăn hoặc danh mục của món ăn bị null, bạn có thể xử lý theo cách khác
                    Console.WriteLine($"Không tìm thấy món ăn: {menuItemName} hoặc danh mục của món ăn bị null.");
                }
            }


            // Tạo file PDF từ RevenueReportDto
            using (var stream = new MemoryStream())
            {
                // Khởi tạo tài liệu PDF
                var document = new Document(PageSize.A4);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Tiêu đề báo cáo
                var title = new Paragraph("Revenue Report", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Thêm thông tin về khoảng thời gian báo cáo
                var periodParagraph = new Paragraph($"Period: {revenueReport.Period}", FontFactory.GetFont(FontFactory.HELVETICA, 12));
                periodParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(periodParagraph);

                // Thêm doanh thu
                var revenueParagraph = new Paragraph($"Total Revenue: {revenueReport.TotalRevenue:C}", FontFactory.GetFont(FontFactory.HELVETICA, 12));
                revenueParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(revenueParagraph);

                // Thêm danh sách các món ăn
                var menuTitle = new Paragraph("Menu Items List:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14));
                menuTitle.Alignment = Element.ALIGN_LEFT;
                document.Add(menuTitle);

                // Tạo bảng với 3 cột (Tên món ăn, Số lượng bán, Danh mục)
                var table = new PdfPTable(3); // Tạo bảng với 3 cột
                table.AddCell("Menu Item");
                table.AddCell("Quantity Sold");
                table.AddCell("Category");

                // Thêm các dòng dữ liệu vào bảng
                foreach (var menuItem in revenueReport.MenuItems)
                {
                    table.AddCell(menuItem.MenuItem);  // Tên món ăn
                    table.AddCell(menuItem.QuantitySold.ToString());  // Số lượng bán
                    table.AddCell(menuItem.Category);  // Danh mục món ăn
                }

                document.Add(table);

                // Kết thúc tài liệu PDF
                document.Close();

                // Trả về file PDF cho người dùng
                return File(stream.ToArray(), "application/pdf", "Revenue_Report.pdf");
            }
        }



        public async Task<IActionResult> CustomerFeedbackReport(DateTime? startDate, DateTime? endDate, int? month, int? year)
        {
            var query = _context.CustomerReviews.AsQueryable();

            // Lọc theo ngày
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(r => r.ReviewDate >= startDate.Value && r.ReviewDate <= endDate.Value);
            }

            // Lọc theo tháng và năm
            if (month.HasValue && year.HasValue)
            {
                query = query.Where(r => r.ReviewDate.Month == month.Value && r.ReviewDate.Year == year.Value);
            }

            var reviews = await query.ToListAsync();

            // Tính toán các thống kê
            var foodQualityAverage = reviews.Any() ? reviews.Average(r => r.FoodQualityRating) : 0;
            var serviceQualityAverage = reviews.Any() ? reviews.Average(r => r.ServiceRating) : 0;
            var ambienceAverage = reviews.Any() ? reviews.Average(r => r.AmbienceRating) : 0;

            var report = new CustomerFeedbackReportViewModel
            {
                TotalReviews = reviews.Count,
                AverageFoodQualityRating = foodQualityAverage,
                AverageServiceRating = serviceQualityAverage,
                AverageAmbienceRating = ambienceAverage,
                CustomerReviews = reviews,
                StartDate = startDate,
                EndDate = endDate,
                Month = month,
                Year = year
            };

            return View(report);
        }

public IActionResult ExportFeedbackReportToPdf(DateTime? startDate, DateTime? endDate, int? month, int? year)
    {
        // Truy xuất dữ liệu từ cơ sở dữ liệu và tạo ViewModel (giống như ví dụ trước)
        var viewModel = new CustomerFeedbackReportViewModel();
        var reviewsQuery = _context.CustomerReviews.AsQueryable();

        if (startDate.HasValue)
        {
            reviewsQuery = reviewsQuery.Where(r => r.ReviewDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            reviewsQuery = reviewsQuery.Where(r => r.ReviewDate <= endDate.Value);
        }
        if (month.HasValue)
        {
            reviewsQuery = reviewsQuery.Where(r => r.ReviewDate.Month == month.Value);
        }
        if (year.HasValue)
        {
            reviewsQuery = reviewsQuery.Where(r => r.ReviewDate.Year == year.Value);
        }

        var reviews = reviewsQuery.ToList();

        // Tính toán các giá trị tổng quan
        viewModel.TotalReviews = reviews.Count;
        viewModel.AverageFoodQualityRating = reviews.Average(r => r.FoodQualityRating);
        viewModel.AverageServiceRating = reviews.Average(r => r.ServiceRating);
        viewModel.AverageAmbienceRating = reviews.Average(r => r.AmbienceRating);
        viewModel.CustomerReviews = reviews;

        // Tạo PDF
        using (MemoryStream ms = new MemoryStream())
        {
            // Tạo đối tượng Document và PdfWriter
            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Thêm tiêu đề báo cáo
            Font titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
            Paragraph title = new Paragraph("Báo Cáo Phản Hồi Của Khách Hàng", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            document.Add(new Phrase("\n"));

            // Thêm thông tin tổng quan
            Font headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            document.Add(new Paragraph($"Tổng số đánh giá: {viewModel.TotalReviews}", headerFont));
            document.Add(new Paragraph($"Đánh giá trung bình chất lượng món ăn: {viewModel.AverageFoodQualityRating:0.0} / 5", headerFont));
            document.Add(new Paragraph($"Đánh giá trung bình chất lượng dịch vụ: {viewModel.AverageServiceRating:0.0} / 5", headerFont));
            document.Add(new Paragraph($"Đánh giá trung bình không gian: {viewModel.AverageAmbienceRating:0.0} / 5", headerFont));

            document.Add(new Phrase("\n"));

            // Thêm bảng danh sách phản hồi
            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 2, 2, 2, 2, 4, 2 });

            // Header của bảng
            table.AddCell(new PdfPCell(new Phrase("Tên Khách Hàng", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Chất Lượng Món Ăn", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Chất Lượng Dịch Vụ", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Không Gian", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Ý Kiến Đóng Góp", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Ngày Đánh Giá", headerFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Dữ liệu bảng
            foreach (var review in viewModel.CustomerReviews)
            {
                table.AddCell(new PdfPCell(new Phrase(review.CustomerName)));
                table.AddCell(new PdfPCell(new Phrase(review.FoodQualityRating.ToString())));
                table.AddCell(new PdfPCell(new Phrase(review.ServiceRating.ToString())));
                table.AddCell(new PdfPCell(new Phrase(review.AmbienceRating.ToString())));
                table.AddCell(new PdfPCell(new Phrase(review.Comments)));
                table.AddCell(new PdfPCell(new Phrase(review.ReviewDate.ToString("dd/MM/yyyy"))));
            }

            // Thêm bảng vào document
            document.Add(table);

            document.Close();

            // Trả về PDF
            byte[] byteArray = ms.ToArray();
            return File(byteArray, "application/pdf", "CustomerFeedbackReport.pdf");
        }
    }

}
}
