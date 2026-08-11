using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reportgroup")]
    public class ReportGroupController : ControllerBase
    {
        private readonly IReportGroupService _service;
        private readonly IMapper _mapper;

        public ReportGroupController(IReportGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all report groups.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReportGroups()
        {
            
            List<ReportGroupDto> result = await _service.GetAllReportGroupsAsync();
            return Ok(_mapper.Map<List<ReportGroupRes>>(result));
        }

        /// <summary>Get paged report groups with optional report scope.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedReportGroups([FromQuery] QueryParameters<string> query, [FromQuery] int? reportId = null)
        {
            var result = await _service.GetPagedReportGroupsAsync(query, reportId);
            return Ok(_mapper.Map<PaginationRes<ReportGroupRes>>(result));
        }

        /// <summary>Get all report groups linked to a specific report.</summary>
        [HttpGet("byreport/{reportId:int}")]
        public async Task<IActionResult> GetReportGroupsByReportId(int reportId)
        {
            List<ReportGroupDto> result = await _service.GetReportGroupsByReportIdAsync(reportId);
            return Ok(_mapper.Map<List<ReportGroupRes>>(result));
        }

        /// <summary>Get a single report group by groupid.</summary>
        [HttpGet("{groupId:int}")]
        public async Task<IActionResult> GetReportGroupById(int groupId)
        {
            ReportGroupDto? result = await _service.GetReportGroupByIdAsync(groupId);
            return result is null ? NotFound() : Ok(_mapper.Map<ReportGroupRes>(result));
        }

        /// <summary>Create a new report group.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateReportGroup([FromBody] ReportGroupReq request)
        {
            ReportGroupDto dto = _mapper.Map<ReportGroupDto>(request);
            ReportGroupDto created = await _service.CreateReportGroupAsync(dto);
            ReportGroupRes res = _mapper.Map<ReportGroupRes>(created);
            return CreatedAtAction(nameof(GetReportGroupById), new { groupId = res.GroupId, version = "1.0" }, res);
        }

        /// <summary>Update an existing report group.</summary>
        [HttpPut("{groupId:int}")]
        public async Task<IActionResult> UpdateReportGroup(int groupId, [FromBody] ReportGroupReq request)
        {
            ReportGroupDto dto = _mapper.Map<ReportGroupDto>(request);
            dto.GroupId = groupId;
            ReportGroupDto updated = await _service.UpdateReportGroupAsync(dto);
            return Ok(_mapper.Map<ReportGroupRes>(updated));
        }

        /// <summary>Delete a report group by groupid.</summary>
        [HttpDelete("{groupId:int}")]
        public async Task<IActionResult> DeleteReportGroup(int groupId)
        {
            bool deleted = await _service.DeleteReportGroupAsync(groupId);
            return Ok(deleted);
        }
    }
}
