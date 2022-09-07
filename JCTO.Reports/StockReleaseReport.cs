using JCTO.Domain.Dtos;

namespace JCTO.Reports
{
    public static class StockReleaseReport
    {
        public static Task GenerateAsync(StockReleaseReportDto data)
        {
            Task.Delay(100);
            return Task.CompletedTask;
        }
    }
}