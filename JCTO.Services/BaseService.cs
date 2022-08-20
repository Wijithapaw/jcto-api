using JCTO.Domain.Dtos;
using JCTO.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Services
{
    public class BaseService
    {
        protected EntityCreateResult GetEntityCreateResult(BaseEntity entity)
        {
            return new EntityCreateResult { Id = entity.Id, ConcurrencyKey = entity.ConcurrencyKey };
        }

        protected EntityUpdateResult GetEntityUpdateResult(BaseEntity entity)
        {
            return new EntityUpdateResult { ConcurrencyKey = entity.ConcurrencyKey };
        }
    }
}
