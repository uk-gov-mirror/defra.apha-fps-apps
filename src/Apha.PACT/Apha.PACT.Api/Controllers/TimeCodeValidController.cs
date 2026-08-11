using Apha.Common.Contracts;
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
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/timecodevalid")]
    public class TimeCodeValidController : ControllerBase
    {
        private readonly ITimeCodeValidService _service;
        private readonly IMapper _mapper;

        public TimeCodeValidController(ITimeCodeValidService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("jobcode")]
        public async Task<IActionResult> GetByJobCode([FromQuery] string jobCode, [FromQuery] string parentProject)
        {
            var items = await _service.GetByJobCodeAsync(jobCode, parentProject);
            return Ok(_mapper.Map<IEnumerable<TimeCodeValidRes>>(items));
        }

        [HttpGet("workgroup")]
        public async Task<IActionResult> GetTimeCodeValidsByWorkGroupAsync([FromQuery] string workGroup)
        {
            var items = await _service.GetTimeCodeValidsByWorkGroupAsync(workGroup);
            return Ok(_mapper.Map<IEnumerable<TimeCodeValidRes>>(items));
        }

        [HttpGet("projects")]
        public async Task<IActionResult> GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync([FromQuery] string workGroup, [FromQuery] string timeCode)
        {
            var items = await _service.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync(workGroup, timeCode);
            return Ok(items);
        }

        [HttpGet("timecodes/all")]
        public async Task<IActionResult> GetAllDistinctTimeCodesAsync()
        {
            var items = await _service.GetAllDistinctTimeCodesAsync();
            return Ok(items);
        }

        [HttpGet("projects/all")]
        public async Task<IActionResult> GetAllDistinctProjectsAsync()
        {
            var items = await _service.GetAllDistinctProjectsAsync();
            return Ok(items);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? jobCode, [FromQuery] string? parentProject)
        {
            var pagedResult = await _service.GetPagedTimeCodesAsync(query, jobCode, parentProject);            
            return Ok(_mapper.Map<PaginationRes<TimeCodeValidRes>>(pagedResult));
        }

        [HttpGet("paged/byprojectandtest")]
        public async Task<IActionResult> GetPagedByProjectAndTestCode(
            [FromQuery] QueryParameters<string> query, [FromQuery] string parentProject, [FromQuery] string testCode)
        {
            var pagedResult = await _service.GetPagedByProjectAndTestCodeAsync(query, parentProject, testCode);
            return Ok(_mapper.Map<PaginationRes<TimeCodeValidRes>>(pagedResult));
        }

        [HttpGet("wgtimecodeprojectcode")]
        public async Task<IActionResult> GetById([FromQuery] string workGroup, [FromQuery] string timeCode, [FromQuery] string parentProject)
        {
            var item = await _service.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<TimeCodeValidRes>(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeCodeValidReq request)
        {
            var dto = _mapper.Map<TimeCodeValidDto>(request);
            var created = await _service.CreateTimeCodeValidAsync(dto);
            return Ok(_mapper.Map<TimeCodeValidRes>(created));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TimeCodeValidReq request)
        {
            var dto = _mapper.Map<TimeCodeValidDto>(request);
            var updated = await _service.UpdateTimeCodeValidAsync(dto);
            return Ok(_mapper.Map<TimeCodeValidRes>(updated));
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] string workGroup, [FromQuery] string timeCode, [FromQuery] string parentProject)
        {
            var isDeleted = await _service.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (!isDeleted)
            {
                throw new ArgumentException("TimeCode record with ID: {timeCode} not found for deletion", timeCode);
            }
            return Ok(isDeleted);
        }

        [HttpDelete("deletebyjobcode")]
        public async Task<IActionResult> DeleteAllByJobCode([FromQuery] string jobCode, [FromQuery] string parentProject)
        {
           var isDeleted = await _service.DeleteAllByJobCodeAsync(jobCode, parentProject);
            if (!isDeleted)
            {
                throw new ArgumentException("JobCode record with ID: {jobCode} not found for deletion", jobCode);
            }
            return Ok(isDeleted);
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopyWorkGroup([FromQuery] string sourceJobCode, [FromQuery] string targetJobCode, [FromQuery] string parentProject)
        {
            var items = await _service.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);
            return Ok(_mapper.Map<IEnumerable<TimeCodeValidRes>>(items));
        }

        [HttpPost("deletebulk")]
        public async Task<IActionResult> DeleteBulk([FromBody] BulkDeleteTimeCodeReq request)
        {
            var items = request.Items.Select(i => (i.WorkGroup, i.TimeCode));
            var result = await _service.DeleteBulkAsync(items, request.ParentProject);
            return Ok(result);
        }

        [HttpPost("copybulkworkgroups")]
        public async Task<IActionResult> CopyBulkWorkGroups([FromBody] BulkCopyWorkGroupReq request)
        {
            var items = await _service.CopySelectedWorkGroupsAsync(
                request.WorkGroups, request.SourceJobCode, request.TargetJobCode, request.ParentProject);
            return Ok(_mapper.Map<IEnumerable<TimeCodeValidRes>>(items));
        }
    }
}
