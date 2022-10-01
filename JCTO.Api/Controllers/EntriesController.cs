using JCTO.Domain.Dtos;
using JCTO.Domain.Dtos.Base;
using JCTO.Domain.Services;
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

        [HttpGet("{id}")]
        public async Task<EntryDto> GetEntry(Guid id)
        {
            return await _entryService.GetAsync(id);
        }

        [HttpPost]
        public async Task<EntityCreateResult> CreateEntry(EntryDto entry)
        {
            return await _entryService.CreateAsync(entry);
        }

        [HttpPut("{id}")]
        public async Task<EntityUpdateResult> UpdateEntry(Guid id, EntryDto entry)
        {
            return await _entryService.UpdateAsync(id, entry);
        }

        [HttpDelete("{id}")]
        public async Task DeleteEntry(Guid id)
        {
            await _entryService.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<PagedResultsDto<EntryListItemDto>> Search([FromQuery] EntrySearchDto filter)
        {
            return await _entryService.SearchEntriesAsync(filter);
        }

        [HttpPost("Approval")]
        public async Task<EntityCreateResult> CreateApproval(EntryApprovalDto dto)
        {
            return await _entryService.AddApprovalAsync(dto);
        }

        [HttpDelete("Approval/{id}")]
        public async Task DeleteApproval(Guid id)
        {
            await _entryService.DeleteApprovalAsync(id);
        }

        [HttpGet("Approval/{id}")]
        public async Task<EntryApprovalDto> GetApproval(Guid id)
        {
            return await _entryService.GetApprovalAsync(id);
        }

        [HttpPut("Approval/{id}")]
        public async Task<EntityUpdateResult> UpdateApproval(Guid id, EntryApprovalDto dto)
        {
            return await _entryService.UpdateApprovalAsync(id, dto);
        }

        [HttpGet("Approval/{id}/Summary")]
        public async Task<EntryApprovalSummaryDto> GetApprovalSummary(Guid id)
        {
            return await _entryService.GetApprovalSummaryAsync(id);
        }

        [HttpGet("{entryNo}/balance")]
        public async Task<EntryBalanceQtyDto> GetEntryBalanceQuantities(string entryNo)
        {
            return await _entryService.GetEntryBalanceQuantitiesAsync(entryNo);
        }

        [HttpGet("{entryNo}/RemainingApprovals")]
        public async Task<List<EntryRemaningApprovalsDto>> GetEntryRemainingApprvals(string entryNo, [FromQuery] Guid? excludeOrderId = null)
        {
            return await _entryService.GetEntryRemainingApprovalsAsync(entryNo, excludeOrderId);
        }

        [HttpPost("{entryId}/RebondTo")]
        public async Task<EntityCreateResult> RebondTo(EntryRebondToDto dto)
        {
            return await _entryService.RebondToAsync(dto);
        }
    }
}
