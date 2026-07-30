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
    /// API controller for WG Grades available within a given RC grade.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wggrades")]
    public class WorkGroupGradeController : ControllerBase
    {
        private readonly IWorkGroupGradeService _WorkGroupGradeService;
        private readonly IMapper _mapper;

        public WorkGroupGradeController(IWorkGroupGradeService WorkGroupGradeService, IMapper mapper)
        {
            _WorkGroupGradeService = WorkGroupGradeService ?? throw new ArgumentNullException(nameof(WorkGroupGradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of WG grades available within the given RC grade.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="pcGrade">The profit centre grade code.</param>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroupGradeAsync([FromQuery] PaginationReq<string> query, [FromQuery] string pcGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _WorkGroupGradeService.GetWorkGroupGradeAsync(filter, profitCentreGrade: pcGrade);
            return Ok(_mapper.Map<PaginationRes<WorkgroupGradeRes>>(result));
        }

        /// <summary>
        /// Deletes a WG grade by its grade code.
        /// </summary>
        /// <param name="wgGrade">The WG grade code to delete.</param>
        [HttpDelete("{wgGrade}")]
        public async Task<IActionResult> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            var isDeleted = await _WorkGroupGradeService.DeleteWorkGroupGradeAsync(wgGrade);
            if (!isDeleted)
                throw new KeyNotFoundException("WorkGroupGrade not found.");
            return Ok(isDeleted);
        }


        /// <summary>Retrieves a paginated list of WorkgroupGrade records.</summary>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllWorkgroupGradesPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _WorkGroupGradeService.GetAllWorkgroupGradesPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<WorkgroupGradeRes>>(result));
        }

        /// <summary>Retrieves a single WorkgroupGrade by WgGrade code.</summary>
        [HttpGet("{wgGrade}")]
        public async Task<ActionResult<WorkgroupGradeRes>> GetByWgGradeAsync(string wgGrade)
        {
            var dto = await _WorkGroupGradeService.GetByWgGradeAsync(wgGrade);
            if (dto is null)
                throw new KeyNotFoundException($"WorkgroupGrade '{wgGrade}' not found.");
            return Ok(_mapper.Map<WorkgroupGradeRes>(dto));
        }

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        [HttpPost]
        public async Task<ActionResult<WorkgroupGradeRes>> CreateAsync([FromBody] WorkgroupGradeReq request)
        {
            var dto = _mapper.Map<WorkgroupGradeDto>(request);
            var created = await _WorkGroupGradeService.CreateAsync(dto);
            return Ok(_mapper.Map<WorkgroupGradeRes>(created));
        }

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        [HttpPut("{wgGrade}")]
        public async Task<ActionResult<WorkgroupGradeRes>> UpdateAsync(string wgGrade, [FromBody] WorkgroupGradeReq request)
        {
            var dto = _mapper.Map<WorkgroupGradeDto>(request);
            dto.WgGrade = wgGrade;
            var updated = await _WorkGroupGradeService.UpdateAsync(dto);
            return Ok(_mapper.Map<WorkgroupGradeRes>(updated));
        }

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        [HttpDelete("maintain/{wgGrade}")]
        public async Task<ActionResult<bool>> DeleteAsync(string wgGrade)
        {
            var deleted = await _WorkGroupGradeService.DeleteAsync(wgGrade);
            return Ok(deleted);
        }

        /// <summary>Returns all Grade codes for dropdown population.</summary>
        [HttpGet("gradecodes")]
        public async Task<ActionResult<List<string>>> GetAllGradeCodesAsync()
        {
            var result = await _WorkGroupGradeService.GetAllGradeCodesAsync();
            return Ok(result);
        }

        /// <summary>Returns distinct WorkgroupGrade records for a given workgroup, ordered by WGGrade.</summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="workGroup">The workgroup name to filter by.</param>
        [HttpGet("byworkgroup")]
        public async Task<ActionResult> GetWorkgroupGradesByWorkGroupAsync(
            [FromQuery] string workGroup)
        {
            var result = await _WorkGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(workGroup);
            return Ok(_mapper.Map<List<WorkgroupGradeRes>>(result));
        }
    }
}
