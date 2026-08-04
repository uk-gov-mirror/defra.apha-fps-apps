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
    [Route("api/v{version:apiVersion}/jobcode")]
    public class JobCodeController : ControllerBase
    {
        private readonly IJobCodeService _service;
        private readonly IMapper _mapper;

        public JobCodeController(IJobCodeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetJobCodesAsync();
            return Ok(_mapper.Map<IEnumerable<JobCodeRes>>(items));
        }

        [HttpGet("zt")]
        public async Task<IActionResult> GetZtCodesAsync()
        {
            var result = await _service.GetZtCodeLookupAsync();
            return Ok(_mapper.Map<IEnumerable<JobCodeZtRes>>(result));
        }

        [HttpGet("project")]
        public async Task<IActionResult> GetByProject([FromQuery] string parentProject)
        {
            var items = await _service.GetJobCodesByProjectAsync(parentProject);
            var result = _mapper.Map<IEnumerable<JobCodeRes>>(items);
            return Ok(result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query, [FromQuery] string? parentProject)
        {
            var pagedResult = await _service.GetPagedJobCodesAsync(query, parentProject);
            return Ok(_mapper.Map<PaginationRes<JobCodeRes>>(pagedResult));
        }

        [HttpGet("jobCodeId")]
        public async Task<IActionResult> GetById([FromQuery] string jobCodeId)
        {
            var item = await _service.GetJobCodeByIdAsync(jobCodeId);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<JobCodeRes>(item));
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTypes()
        {
            var types = await _service.GetTypesAsync();
            return Ok(types);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobCodeReq request)
        {
            var dto = _mapper.Map<JobCodeDto>(request);
            var created = await _service.CreateJobCodeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { jobCodeId = created.JobCodeId }, _mapper.Map<JobCodeRes>(created));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JobCodeReq request)
        {
            var dto = _mapper.Map<JobCodeDto>(request);
            var updated = await _service.UpdateJobCodeAsync(dto);
            return Ok(_mapper.Map<JobCodeRes>(updated));
        }

        [HttpDelete("jobCodeId")]
        public async Task<IActionResult> Delete([FromQuery] string jobCodeId)
        {
            var deleted = await _service.DeleteJobCodeAsync(jobCodeId);            
            if (!deleted)
            {
                throw new ArgumentException("JobCode record with ID: {jobCodeId} not found for deletion", jobCodeId);
            }
            return Ok(deleted);
        }
    }
}
