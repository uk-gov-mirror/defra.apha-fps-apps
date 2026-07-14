using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accountgroup")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class AccountGroupController : ControllerBase
    {
        private readonly IAccountGroupService _service;
        private readonly IMapper _mapper;

        public AccountGroupController(IAccountGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAccountGroups()
        {
            var dtos = await _service.GetAllAccountGroupAsync();
            return Ok(_mapper.Map<List<AccountGroupRes>>(dtos));
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedAccountGroups([FromQuery] PaginationReq<string> query)
        {
            var parameters = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPaginatedAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<AccountGroupRes>>(result));
        }

        [HttpGet("{csg7Group}")]
        public async Task<IActionResult> GetAccountGroup(string csg7Group)
        {
            var dto = await _service.GetByCsg7GroupAsync(csg7Group);
            if (dto == null) return NotFound();
            return Ok(_mapper.Map<AccountGroupRes>(dto));
        }

        [HttpPost]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> AddAccountGroup([FromBody] AccountGroupReq req)
        {
            var dto = _mapper.Map<AccountGroupDto>(req);
            var created = await _service.AddAccountGroupAsync(dto);
            return CreatedAtAction(nameof(GetAccountGroup), new { csg7Group = created.Csg7group }, _mapper.Map<AccountGroupRes>(created));
        }

        [HttpPut("{csg7Group}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateAccountGroup(string csg7Group, [FromBody] AccountGroupReq req)
        {
            var dto = _mapper.Map<AccountGroupDto>(req);
            var updated = await _service.UpdateAccountGroupAsync(csg7Group, dto);
            return Ok(_mapper.Map<AccountGroupRes>(updated));
        }

        [HttpDelete("{csg7Group}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> DeleteAccountGroup(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group is required for deletion.");

            await _service.DeleteAccountGroupAsync(csg7Group);
            return Ok(new { success = true, message = "Deleted successfully" });
        }
    }
}
