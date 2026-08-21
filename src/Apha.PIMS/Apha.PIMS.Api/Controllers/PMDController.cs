using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
            bool viewSpecificProject = User.IsInRole("API-PIMSProjectManager") && !User.IsInRole("API-PMDAdmin");
            string? loginEmail = viewSpecificProject
                ? User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("preferred_username") ?? User.Identity?.Name
                : null;

            List<ProjectYearManagerDto> result = await _service.GetProjectYearManagersAsync(year, loginEmail, viewSpecificProject);
            return Ok(_mapper.Map<List<ProjectYearManagerRes>>(result) ?? []);
        }

        [HttpGet("milestones")]
        public async Task<IActionResult> GetPMDMilestones([FromQuery] QueryParameters<string> parameters, [FromQuery] string project)
        {
            PaginatedResult<MilestoneDto> result = await _service.GetPMDMilestonesAsync(parameters, project);
            return Ok(_mapper.Map<PaginationRes<MilestoneRes>>(result));
        }

        [HttpGet("milestone")]
        public async Task<IActionResult> GetMilestoneAsync_PMD([FromQuery] string project, [FromQuery] string number)
        {
            MilestoneDto? result = await _service.GetMilestoneAsync(project, HttpUtility.UrlDecode(number));

            if (result is null)
            {
                return new JsonResult(new Apha.Common.Contracts.ApiResponse<MilestoneRes>
                {
                    Success = true,
                    Data = null,
                    Meta = new Apha.Common.Contracts.ApiMeta
                    {
                        CorrelationId = Guid.NewGuid().ToString(),
                        TimestampUtc = DateTime.UtcNow
                    }
                });
            }

            return Ok(_mapper.Map<MilestoneRes>(result));
        }

        [HttpGet("formdates")]
        public async Task<IActionResult> GetMilestoneFormDatesAsync_PMD([FromQuery] string parentProject, [FromQuery] short year)
        {
            MilestoneFormDatesDto? result = await _service.GetMilestoneFormDatesAsync(year, parentProject);

            if (result is null)
            {
                return new JsonResult(new Apha.Common.Contracts.ApiResponse<MilestoneFormDatesRes>
                {
                    Success = true,
                    Data = null,
                    Meta = new Apha.Common.Contracts.ApiMeta
                    {
                        CorrelationId = Guid.NewGuid().ToString(),
                        TimestampUtc = DateTime.UtcNow
                    }
                });
            }

            return Ok(_mapper.Map<MilestoneFormDatesRes>(result));
        }

        [HttpPost("formdates")]
        public async Task<IActionResult> SaveMilestoneFormDatesAsync_PMD([FromQuery] string parentProject, [FromBody] MilestoneFormDatesReq request)
        {
            MilestoneFormDatesDto dto = _mapper.Map<MilestoneFormDatesDto>(request);
            dto.ParentProject = parentProject;
            MilestoneFormDatesDto result = await _service.SaveMilestoneFormDatesAsync(dto);
            return Ok(_mapper.Map<MilestoneFormDatesRes>(result));
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateMilestone_PMD(
            [FromQuery] string? project,
            [FromQuery] string? number,
            [FromBody] MilestoneReq request)
        {
            MilestoneDto dto = _mapper.Map<MilestoneDto>(request);
            string? changedBy = User.Identity?.Name;
            MilestoneDto result = await _service.UpdateMilestoneAsync_PMD(
                project!,
                HttpUtility.UrlDecode(number!),
                dto.UnderSdReview,
                dto.OnTarget ?? 0,
                dto.DateCompleted,
                dto.ProjectLeaderComment,
                changedBy);

            return Ok(_mapper.Map<MilestoneRes>(result));
        }

    }
}
