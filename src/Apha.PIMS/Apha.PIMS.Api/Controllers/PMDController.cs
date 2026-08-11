using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PMDAdmin,API-PIMSProjectManager")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/pmd")]
    public class PMDController : ControllerBase
    {
        private readonly IMilestoneService _service;
        private readonly IMapper _mapper;

        public PMDController(IMilestoneService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("projectyearmanagers/{year:int}")]
        public async Task<IActionResult> GetProjectYearManagers(int year)
        {
            List<ProjectYearManagerDto> result = await _service.GetProjectYearManagersAsync(year);
            return Ok(_mapper.Map<List<ProjectYearManagerRes>>(result) ?? []);
        }

        [HttpGet("milestones")]
        public async Task<IActionResult> GetPMDMilestones([FromQuery] QueryParameters<string> parameters, [FromQuery] string project)
        {
            PaginatedResult<MilestoneDto> result = await _service.GetPMDMilestonesAsync(parameters, project);
            return Ok(_mapper.Map<PaginationRes<MilestoneRes>>(result));
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateMilestone_PMD(
            [FromQuery] string? project,
            [FromQuery] string? number,
            [FromBody] MilestoneReq request)
        {
            if (string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(number))
                return BadRequest(new { success = false, message = "Project and milestone number are required." });

            MilestoneDto dto = _mapper.Map<MilestoneDto>(request);
            string? changedBy = User.Identity?.Name is { } name ? name[..Math.Min(10, name.Length)] : null;
            MilestoneDto result = await _service.UpdateMilestoneAsync_PMD(
                project,
                HttpUtility.UrlDecode(number),
                dto.UnderSdReview,
                dto.OnTarget ?? 0,
                dto.DateCompleted,
                dto.ProjectLeaderComment,
                changedBy);

            return Ok(_mapper.Map<MilestoneRes>(result));
        }

    }
}
