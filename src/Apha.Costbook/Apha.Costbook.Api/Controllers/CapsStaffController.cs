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
    [Route("api/v{version:apiVersion}/capsstaff")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class CapsStaffController : ControllerBase
    {
        private readonly ICapsStaffService _service;
        private readonly IMapper _mapper;

        public CapsStaffController(ICapsStaffService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCapsStaff()
        {
            var dtos = await _service.GetAllStaffAsync();
            return Ok(_mapper.Map<List<StaffRes>>(dtos));
        }
               
        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedCapsStaff([FromQuery] PaginationReq<string> query)
        {
            var parameters = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPaginatedAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<StaffRes>>(result));
        }

        [HttpGet("{mNumber}")]
        public async Task<IActionResult> GetCapsStaff(string mNumber)
        {
            var dto = await _service.GetByMNumberAsync(mNumber);
            if (dto == null) return NotFound();
            return Ok(_mapper.Map<StaffRes>(dto));
        }

        [HttpPost]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> AddCapsStaff([FromBody] StaffReq req)
        {
            var dto = _mapper.Map<StaffDto>(req);
            var created = await _service.AddStaffAsync(dto);
            return CreatedAtAction(nameof(GetCapsStaff), new { mNumber = created.Mnumber }, _mapper.Map<StaffRes>(created));
        }

        [HttpPut("{mNumber}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateCapsStaff(string mNumber, [FromBody] StaffReq req)
        {
            var dto = _mapper.Map<StaffDto>(req);
            var updated = await _service.UpdateStaffAsync(mNumber, dto);
            return Ok(_mapper.Map<StaffRes>(updated));
        }

        [HttpDelete("{mNumber}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> DeleteCapsStaff(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber is required for deletion.");

            await _service.DeleteStaffAsync(mNumber);
            return Ok(new { success = true, message = "Deleted successfully" });
        }
    }
}
