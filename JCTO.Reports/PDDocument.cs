using JCTO.Reports.Dtos;
using SelectPdf;

namespace JCTO.Reports
{
    public static class PDDocument
    {
        public static async Task<byte[]> GenerateAsync(PDDocumentDto data)
        {
            var htmlToPdf = new HtmlToPdf(1000, 1414);
            htmlToPdf.Options.DrawBackground = true;
            htmlToPdf.Options.MarginTop = 30;
            htmlToPdf.Options.MarginBottom = 30;
            htmlToPdf.Options.MarginLeft = 30;
            htmlToPdf.Options.MarginRight = 30;

            var html = await HtmlReportHelper.GetHtmlAsync("PDDocument", data);
            var pdf = htmlToPdf.ConvertHtmlString(html);
            var bytes = pdf.Save();

            return bytes;
        }
    }
}
