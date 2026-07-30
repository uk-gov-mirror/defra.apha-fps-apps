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
            return Ok(_mapper.Map<List<ProjectYearManagerRes>>(result));
        }

        [HttpGet("milestones")]
        public async Task<IActionResult> GetPMDMilestones([FromQuery] QueryParameters<string> parameters, [FromQuery] string project)
        {
            PaginatedResult<MilestoneDto> result = await _service.GetPMDMilestonesAsync(parameters, project);
            return Ok(_mapper.Map<PaginationRes<MilestoneRes>>(result));
        }
    }
}
