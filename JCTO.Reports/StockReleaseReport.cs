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

                sheet.Cells["B4"].Value = 10;

                return await package.GetAsByteArrayAsync();
            }
        }
    }
}