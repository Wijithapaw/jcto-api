using JCTO.Domain;
using JCTO.Domain.Dtos;
using JCTO.Domain.Entities;
using JCTO.Domain.Enums;
using JCTO.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Services
{
    public class EntryService : BaseService, IEntryService
    {
        private readonly IDataContext _dataContext;

        public EntryService(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<EntityCreateResult> CreateAsync(EntryDto dto)
        {
            var newEntry = new Entry
            {
                CustomerId = dto.CustomerId,
                ProductId = dto.ProductId,
                EntryNo = dto.EntryNo,
                EntryDate = dto.EntryDate,
                InitialQualtity = dto.InitialQuantity,
                Status = dto.Status,
            };

            newEntry.Transactions = EntryTransactionService.GetEntryTransactions(EntryTransactionType.In, dto.InitialQuantity);

            _dataContext.Entries.Add(newEntry);

            await _dataContext.SaveChangesAsync();

            return GetEntityCreateResult(newEntry);
        }
    }
}
