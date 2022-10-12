using JCTO.Reports.Dtos;
using NumericWordsConversion;
using OfficeOpenXml;
using System.Reflection;

namespace JCTO.Reports
{
    public static class PDDocument
    {
        public async static Task<byte[]> GenerateAsync(PDDocumentDto data)
        {
            string rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var filePath = Path.Combine(rootPath, "ReportTemplates/PDDocument.xlsx");

            FileInfo existingFile = new FileInfo(filePath);

            var bowserCount = data.BowserList.Sum(b => b.Count);

            NumericWordsConverter converter = new NumericWordsConverter();

            using (var package = new ExcelPackage(existingFile))
            {
                for (var i = 2; i <= bowserCount; i++)
                {
                    package.Workbook.Worksheets.Copy("Sheet1", $"Sheet{i}");
                }

                var sheetIndex = 0;
                foreach (var bowser in data.BowserList.OrderBy(b => b.Capacity))
                {
                    for (var i = 1; i <= bowser.Count; i++)
                    {
                        var sheet = package.Workbook.Worksheets[sheetIndex++];

                        //sheet.Cells.Clear();

                        sheet.Cells["E8"].Value = data.Customer;
                        sheet.Cells["E10"].Value = data.Buyer;

                        sheet.Cells["M6"].Value = data.EntryNo;
                        sheet.Cells["M8"].Value = data.ObRef;

                        sheet.Cells["B15"].Value = sheetIndex;

                        sheet.Cells["D15"].Value = data.Product;
                        sheet.Cells["D17"].Value = $"{converter.ToWords(bowser.Capacity)} Liters only";

                        sheet.Cells["K17"].Value = "Ltrs";
                        sheet.Cells["L17"].Value = bowser.Capacity;
                    }
                }

                return await package.GetAsByteArrayAsync();
            }
        }
    }
}