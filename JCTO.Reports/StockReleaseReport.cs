using JCTO.Domain.Dtos;
using OfficeOpenXml;
using System.Reflection;

namespace JCTO.Reports
{
    public static class StockReleaseReport
    {
        public async static Task<byte[]> GenerateAsync(StockReleaseReportDto data)
        {
            string rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var filePath = Path.Combine(rootPath, "ReportTemplates/StockRelease.xlsx");

            FileInfo existingFile = new FileInfo(filePath);

            using (var package = new ExcelPackage(existingFile))
            {
                var sheet = package.Workbook.Worksheets[0];

                sheet.Cells["B4"].Value = data.OrderNo;
                sheet.Cells["B6"].Value = data.OrderDate;
                sheet.Cells["B5"].Value = data.ObRef;
                sheet.Cells["G5"].Value = data.EntryNo;
                sheet.Cells["G6"].Value = data.Customer;
                sheet.Cells["A9"].Value = data.Product;
                sheet.Cells["B9"].Value = data.TankNo;
                sheet.Cells["C9"].Value = data.ObRef;
                sheet.Cells["D9"].Value = data.Buyer;
                sheet.Cells["G9"].Value = data.Quantity;
                sheet.Cells["C13"].Value = data.QuantityInText;
                sheet.Cells["G17"].Value = data.Remarks;

                return await package.GetAsByteArrayAsync();
            }
        }
    }
}