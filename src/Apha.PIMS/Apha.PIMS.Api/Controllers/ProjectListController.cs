using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectlist")]
    public class ProjectListController : ControllerBase
    {
        private readonly IProjectListService _service;
        private readonly IMapper _mapper;

        public ProjectListController(IProjectListService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectsAsync([FromQuery] PaginationReq<string> query, [FromQuery] int showWhichProjects = 2)
        {    
            QueryParameters<string> filter = _mapper.Map<QueryParameters<string>>(query);
            PaginatedResult<ProjectListViewDto> result = await _service.GetAllProjectsAsync(filter, showWhichProjects);
            return Ok(_mapper.Map<PaginationRes<ProjectListRes>>(result));
        }

        [HttpGet("AllProjectsList")]
        public async Task<IActionResult> GetAllProjectsForDropDownAsync([FromQuery] int showWhichProjects = 2)
        {
            List<ProjectListViewDto> result = await _service.GetAllProjectsForDropDownAsync(showWhichProjects);
            return Ok(_mapper.Map<List<ProjectListRes>>(result));
        }

        [HttpGet("{parentproject}/yearly")]
        public async Task<IActionResult> GetYearlyDetailsByProject(string parentproject)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            List<ProjectsDto> result = await _service.GetYearlyDetailsByProjectAsync(parentproject);
            return Ok(_mapper.Map<List<ProjectsRes>>(result));
        }
        [HttpGet("AllProjectsMilestone")]
        public async Task<IActionResult> GetAllProjectsForMilestoneAsync()
        {
            List<ProjectListMilestoneDto> result = await _service.GetAllProjectsForMilestoneAsync();
            return Ok(_mapper.Map<List<ProjectListMilestoneRes>>(result));
        }
        [HttpGet("ProjectDetailsMilestone/{parentproject}")]
        public async Task<IActionResult> GetProjectsDetailsForMilestoneAsync(string parentproject)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            ProjectDetailsMilestoneDto? result = await _service.GetProjectsDetailsForMilestoneAsync(parentproject);
            if (result is null)
            {
                return CreateNullSuccessResponse<ProjectDetailsMilestoneRes>();
            }

            return Ok(_mapper.Map<ProjectDetailsMilestoneRes>(result));
        }

        private static JsonResult CreateNullSuccessResponse<T>()
        {
            return new JsonResult(new Apha.Common.Contracts.ApiResponse<T>
            {
                Success = true,
                Data = default,
                Meta = new Apha.Common.Contracts.ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            });
        }
    }
}
