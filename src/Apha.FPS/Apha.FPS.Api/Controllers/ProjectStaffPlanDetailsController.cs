using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Project Staff Plan Details view (fps.vwprojectstaffplandetails).
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectstaffplandetails")]
    public class ProjectStaffPlanDetailsController : ControllerBase
    {
        private readonly IProjectStaffPlanDetailsService _service;
        private readonly IMapper _mapper;

        public ProjectStaffPlanDetailsController(IProjectStaffPlanDetailsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paginated, filterable list of staff plan detail records from fps.vwprojectstaffplandetails.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query)
        {
            PaginatedResult<ProjectStaffPlanDetailsViewDto> result = await _service.GetPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectStaffPlanDetailsViewRes>>(result));
        }
    }
}
