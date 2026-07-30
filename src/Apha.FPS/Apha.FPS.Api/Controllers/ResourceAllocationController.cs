using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Stage 2 Check Resource Allocation (frmResourceAllocation).
    /// Provides read-only grid data for staff allocations and staff job lines.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/ResourceAllocation")]
    public class ResourceAllocationController : ControllerBase
    {
        private readonly IResourceAllocationService _service;
        private readonly IMapper _mapper;

        public ResourceAllocationController(IResourceAllocationService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paged, sorted and filtered set of staff allocation rows for a workgroup grade.
        /// </summary>
        [HttpGet("staffallocations/paged")]
        public async Task<IActionResult> GetPagedStaffAllocationsByWorkGroupGradeAsync([FromQuery] string workGroupGrade, [FromQuery] QueryParameters<string> query)
        {
            var paged = await _service.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query ?? new QueryParameters<string>());
            return Ok(_mapper.Map<PaginationRes<ResourceStaffAllocationRes>>(paged));
        }

        /// <summary>
        /// Returns a paged, sorted and filtered set of distinct job detail rows for a staff member.
        /// </summary>
        [HttpGet("staffjobdetails/paged")]
        public async Task<IActionResult> GetPagedStaffJobDetailsByStaffIdAsync([FromQuery] string staffId, [FromQuery] QueryParameters<string> query)
        {
            var paged = await _service.GetPagedStaffJobDetailsByStaffIdAsync(staffId, query ?? new QueryParameters<string>());
            return Ok(_mapper.Map<PaginationRes<ResourceStaffJobDetailRes>>(paged));
        }
    }
}