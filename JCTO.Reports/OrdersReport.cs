using JCTO.Reports.Dtos;
using OfficeOpenXml;
using System.Reflection;

namespace JCTO.Reports
{
    public static class OrderReport
    {
        public async static Task<byte[]> GenerateAsync(OrderReportDto data)
        {
            string rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var filePath = Path.Combine(rootPath, "ReportTemplates/OrdersReport.xlsx");

            FileInfo existingFile = new FileInfo(filePath);

            using (var package = new ExcelPackage(existingFile))
            {
                var sheet = package.Workbook.Worksheets[0];

                sheet.Cells["B3"].Value = data.Filter.Product;
                sheet.Cells["B4"].Value = data.Filter.BuyerType;
                sheet.Cells["B5"].Value = data.Filter.DateRange;
                sheet.Cells["F3"].Value = data.Filter.Customer;
                sheet.Cells["F4"].Value = data.Filter.BuyerName;
                sheet.Cells["F5"].Value = data.Filter.Status;
                sheet.Cells["J3"].Value = data.Filter.TotalQuantity;
                sheet.Cells["J4"].Value = data.Filter.TotalUndeliveredQuantity;
                sheet.Cells["J5"].Value = data.Filter.TotalCancelledQuantity;

                var row = 9;
                foreach (var order in data.Orders)
                {
                    sheet.Cells[row, 1].Value = order.OrderDate;
                    sheet.Cells[row, 2].Value = order.Customer;
                    sheet.Cells[row, 3].Value = order.Product;
                    sheet.Cells[row, 4].Value = order.OrderNo;
                    sheet.Cells[row, 5].Value = order.Status;
                    sheet.Cells[row, 6].Value = order.BuyerType;
                    sheet.Cells[row, 7].Value = order.BuyerName;
                    sheet.Cells[row, 8].Value = order.Quantity;
                    sheet.Cells[row, 9].Value = order.IssueCommencedTime;
                    sheet.Cells[row, 10].Value = order.IssueCompletedTime;

                    row++;
                }

                return await package.GetAsByteArrayAsync();
            }
        }
    }
}