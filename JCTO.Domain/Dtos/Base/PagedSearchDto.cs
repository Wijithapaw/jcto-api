using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos.Base
{
    public class PagedSearchDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
