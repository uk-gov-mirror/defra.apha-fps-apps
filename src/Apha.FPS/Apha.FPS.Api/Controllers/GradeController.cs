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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/Grade")]
    [ApiController]
    [ApiVersion("1.0")]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly IMapper _mapper;

        public GradeController(IGradeService gradeService, IMapper mapper)
        {
            _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of grades for the active FPS year.
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paginated list of <see cref="GradeRes"/>.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _gradeService.GetAllPagedAsync(query);
            if (result == null)
            {
                throw new ArgumentException("Grade records not found");
            }
            return Ok(_mapper.Map<PaginationRes<GradeRes>>(result));
        }

        /// <summary>
        /// Returns a single grade by its GradeCode for the active FPS year.
        /// </summary>
        /// <param name="gradeCode">Grade code identifier.</param>
        /// <returns><see cref="GradeRes"/> if found.</returns>
        [HttpGet("{gradeCode}")]
        public async Task<ActionResult<GradeRes>> GetByIdAsync(string gradeCode)
        {
            var dto = await _gradeService.GetByIdAsync(gradeCode);
            if (dto == null)
            {
                throw new ArgumentException($"Grade record '{gradeCode}' not found");
            }
            return Ok(_mapper.Map<GradeRes>(dto));
        }

        /// <summary>
        /// Creates a new grade record.
        /// </summary>
        /// <param name="request">Grade creation request.</param>
        /// <returns>Created <see cref="GradeRes"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<GradeRes>> CreateAsync([FromBody] GradeReq request)
        {
            var dto = _mapper.Map<GradeDto>(request);
            var created = await _gradeService.CreateAsync(dto);
            return Ok(_mapper.Map<GradeRes>(created));
        }

        /// <summary>
        /// Updates an existing grade record identified by <paramref name="gradeCode"/>.
        /// </summary>
        /// <param name="gradeCode">Original grade code to identify the record.</param>
        /// <param name="request">Grade update request (may contain a new GradeCode for rename).</param>
        /// <returns>Updated <see cref="GradeRes"/>.</returns>
        [HttpPut("{gradeCode}")]
        public async Task<ActionResult<GradeRes>> UpdateAsync(
            string gradeCode,
            [FromBody] GradeReq request)
        {
            var dto = _mapper.Map<GradeDto>(request);
            var updated = await _gradeService.UpdateAsync(gradeCode, dto);
            return Ok(_mapper.Map<GradeRes>(updated));
        }

        /// <summary>
        /// Deletes the grade with the given GradeCode in the active FPS year.
        /// </summary>
        /// <param name="gradeCode">Grade code of the record to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        [HttpDelete("{gradeCode}")]
        public async Task<IActionResult> DeleteAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                throw new ArgumentException("Grade code cannot be null or empty.", nameof(gradeCode));
            }

            var deleted = await _gradeService.DeleteAsync(gradeCode);
            if (!deleted)
            {
                throw new ArgumentException($"Grade record '{gradeCode}' not found");
            }
            return Ok(true);
        }
    }
}
