namespace JCTO.Domain.Dtos.Base
{
    public class PagedResultsDto<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; }
    }
}
