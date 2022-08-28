using JCTO.Domain.Dtos.Base;
using Microsoft.EntityFrameworkCore;

namespace JCTO.Services
{
    public static class EFExtentions
    {
        public static PagedResultsDto<T> GetPagedList<T>(this IQueryable<T> query,
                                         PagedSearchDto filter) where T : class
        {
            var result = new PagedResultsDto<T>();
            result.Total = query.Count();

            var skip = (filter.Page - 1) * filter.PageSize;
            result.Items = query.Skip(skip).Take(filter.PageSize).ToList();

            return result;
        }

        public static async Task<PagedResultsDto<T>> GetPagedListAsync<T>(this IQueryable<T> query,
                                        PagedSearchDto filter) where T : class
        {
            var result = new PagedResultsDto<T>();
            result.Total = await query.CountAsync();

            var skip = (filter.Page - 1) * filter.PageSize;
            result.Items = await query.Skip(skip).Take(filter.PageSize).ToListAsync();

            return result;
        }
    }
}
