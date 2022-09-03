using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JCTO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {
        private readonly IEntryService _entryService;

        public EntriesController(IEntryService entryService)
        {
            _entryService = entryService;
        }

        [HttpPost]
        public async Task<EntityCreateResult> CreateEntry(EntryDto entry)
        {
            return await _entryService.CreateAsync(entry);
        }

        [HttpGet]
        public async Task<PagedResultsDto<EntryListItemDto>> Search([FromQuery] EntrySearchDto filter)
        {
            return await _entryService.SearchEntriesAsync(filter);
        }

        [HttpPost("Approve")]
        public async Task<EntityCreateResult> ApproveEntry(EntryApprovalDto dto)
        {
            return await _entryService.AddApprovalAsync(dto);
        }

        [HttpGet("{entryNo}/balance")]
        public async Task<EntryBalanceQtyDto> GetEntryBalanceQuantities(string entryNo)
        {
            return await _entryService.GetEntryBalanceQuantitiesAsync(entryNo);
        }
    }
}
