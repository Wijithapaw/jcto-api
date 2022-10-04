using System.Reflection;
using System.Text;

namespace JCTO.Reports
{
    internal class HtmlReportHelper
    {
        internal static async Task<string> GetHtmlAsync(string name, object data)
        {
            string rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var filePath = Path.Combine(rootPath, $"ReportTemplates/Html/{name}.html");

            var html = await File.ReadAllTextAsync(filePath);

            var reportHtml = ReplacePlaceholders(html, data);

            return reportHtml;
        }

        private static string ReplacePlaceholders(string html, object data)
        {
            var sb = new StringBuilder(html);
            foreach (var prop in data.GetType().GetProperties())
            {
                var field = $"[##{prop.Name}]";
                var value = prop.GetValue(data, null)?.ToString() ?? String.Empty;

                sb.Replace(field, value);
            }

            return sb.ToString();
        }
    }
}
