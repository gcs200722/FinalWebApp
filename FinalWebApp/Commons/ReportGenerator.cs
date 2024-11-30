using ClosedXML.Excel;
using FinalWebApp.Data.Dto;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FinalWebApp.Commons
{
    public class ReportGenerator
    {
        public byte[] GenerateExcelReport(List<RevenueReportDto> revenueData, string reportType)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet(reportType);

                worksheet.Cell(1, 1).Value = "Period";
                worksheet.Cell(1, 2).Value = "Total Revenue";
                worksheet.Cell(1, 3).Value = "Menu Item";
                worksheet.Cell(1, 4).Value = "Quantity Sold";
                worksheet.Cell(1, 5).Value = "Category";

                int row = 2;
                foreach (var data in revenueData)
                {
                    foreach (var menuItem in data.MenuItems)
                    {
                        worksheet.Cell(row, 1).Value = data.Period;
                        worksheet.Cell(row, 2).Value = data.TotalRevenue;
                        worksheet.Cell(row, 3).Value = menuItem.MenuItem;
                        worksheet.Cell(row, 4).Value = menuItem.QuantitySold;
                        worksheet.Cell(row, 5).Value = menuItem.Category;
                        row++;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public byte[] GeneratePdfReport(List<RevenueReportDto> revenueData, string reportType)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document();
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title
                document.Add(new Paragraph($"{reportType} Report"));
                document.Add(new Paragraph($"Generated on {DateTime.Now}"));

                // Table header
                var table = new PdfPTable(5);
                table.AddCell("Period");
                table.AddCell("Total Revenue");
                table.AddCell("Menu Item");
                table.AddCell("Quantity Sold");
                table.AddCell("Category");

                // Table content
                foreach (var data in revenueData)
                {
                    foreach (var menuItem in data.MenuItems)
                    {
                        table.AddCell(data.Period);
                        table.AddCell(data.TotalRevenue.ToString());
                        table.AddCell(menuItem.MenuItem);
                        table.AddCell(menuItem.QuantitySold.ToString());
                        table.AddCell(menuItem.Category);
                    }
                }

                document.Add(table);
                document.Close();
                return ms.ToArray();
            }
        }
    }
}
