using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/monthlytime")]
    public class MonthlyTimeController : ControllerBase
    {
        private readonly IMonthlyTimeService _service;
        private readonly IMapper _mapper;

        public MonthlyTimeController(IMonthlyTimeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("timecode/{timeCode}/workgroup/{workGroup}/project/{parentProject}")]
        public async Task<IActionResult> GetMonthlyTimeByTimeCodeAndProject(string timeCode, string workGroup, string parentProject)
        {
            var items = await _service.GetMonthlyTimeByTimeCodeAndProjectAsync(timeCode, workGroup, parentProject);
            return Ok(_mapper.Map<IEnumerable<MonthlyTimeRes>>(items));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedMonthlyTime(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? timeCode,
            [FromQuery] string? workGroup,
            [FromQuery] string? parentProject)
        {
            var pagedResult = await _service.GetPagedMonthlyTimeAsync(query, timeCode, workGroup, parentProject);
            return Ok(pagedResult);
        }

        [HttpGet("{pactStaffId}/{timeCode}/{month}/{parentProject}")]
        public async Task<IActionResult> GetMonthlyTimeById(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var item = await _service.GetMonthlyTimeByIdAsync(pactStaffId, timeCode, month, parentProject);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<MonthlyTimeRes>(item));
        }

        [HttpPost]
        public async Task<IActionResult> CreateMonthlyTime([FromBody] MonthlyTimeReq request)
        {
            var dto = _mapper.Map<MonthlyTimeDto>(request);
            var created = await _service.CreateMonthlyTimeAsync(dto);
            return CreatedAtAction(nameof(GetMonthlyTimeById),
                new { pactStaffId = created.PactStaffId, timeCode = created.TimeCode, month = created.Month, parentProject = created.ParentProject },
                _mapper.Map<MonthlyTimeRes>(created));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMonthlyTime([FromBody] MonthlyTimeReq request)
        {
            var dto = _mapper.Map<MonthlyTimeDto>(request);
            var updated = await _service.UpdateMonthlyTimeAsync(dto);
            return Ok(_mapper.Map<MonthlyTimeRes>(updated));
        }

        [HttpDelete("{pactStaffId}/{timeCode}/{month}/{parentProject}")]
        public async Task<IActionResult> DeleteMonthlyTime(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var deleted = await _service.DeleteMonthlyTimeAsync(pactStaffId, timeCode, month, parentProject);
            if (!deleted)
                throw new ArgumentException($"MonthlyTime record not found for deletion.");
            return Ok(deleted);
        }
    }
}
