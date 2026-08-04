using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{

    /// <summary>
    /// API controller for Project Month (Cost Profile Grid) operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectmonth")]
    public class ProjectMonthController : ControllerBase
    {
        private readonly IProjectMonthService _service;
        private readonly IMapper _mapper;

        public ProjectMonthController(IProjectMonthService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all cost profile months for a given project.</summary>
        /// <param name="project">The project code to retrieve cost profile months for.</param>
        /// <returns>Returns <c>200 OK</c> with a list of <see cref="ProjectMonthRes"/> objects.</returns>
        [HttpGet("project")]
        public async Task<IActionResult> GetProjectMonthByProject([FromQuery] string project)
        {
            IList<ProjectMonthDto> items = await _service.GetProjectMonthByProjectAsync(project);
            return Ok(_mapper.Map<IList<ProjectMonthRes>>(items));
        }

        /// <summary>Retrieves a single cost profile month record by project and month number.</summary>
        /// <param name="project">The project code the month record belongs to.</param>
        /// <param name="monthNo">The month number to retrieve.</param>
        /// <returns>Returns <c>200 OK</c> with the matching <see cref="ProjectMonthRes"/>, or throws <see cref="KeyNotFoundException"/> if not found.</returns>
        [HttpGet("project/month")]
        public async Task<IActionResult> GetProjectMonth([FromQuery] string project, [FromQuery] int monthNo)
        {
            ProjectMonthDto? item = await _service.GetProjectMonthAsync(project, monthNo);
            if (item is null)
                throw new KeyNotFoundException($"Project month record not found for project '{project}', month {monthNo}.");
            return Ok(_mapper.Map<ProjectMonthRes>(item));
        }
        
        /// <summary>Creates a new cost profile month record.</summary>
        /// <param name="request">The cost profile month data to create.</param>
        /// <returns>Returns <c>201 Created</c> with the newly created <see cref="ProjectMonthRes"/> and a location header.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProjectMonth([FromBody] ProjectMonthReq request)
        {
            ProjectMonthDto dto = _mapper.Map<ProjectMonthDto>(request);
            ProjectMonthDto created = await _service.CreateProjectMonthAsync(dto);
            return CreatedAtAction(
                nameof(GetProjectMonth),
                new { project = created.Project, monthNo = created.MonthNo },
                _mapper.Map<ProjectMonthRes>(created));
        }

        /// <summary>Updates an existing cost profile month record.</summary>
        /// <param name="request">The updated cost profile month data.</param>
        /// <returns>Returns <c>200 OK</c> with the updated <see cref="ProjectMonthRes"/>.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateProjectMonth([FromBody] ProjectMonthReq request)
        {
            ProjectMonthDto dto = _mapper.Map<ProjectMonthDto>(request);
            ProjectMonthDto updated = await _service.UpdateProjectMonthAsync(dto);
            return Ok(_mapper.Map<ProjectMonthRes>(updated));
        }

        /// <summary>Deletes a cost profile month record for the specified project and month.</summary>
        /// <param name="project">The project code the month record belongs to.</param>
        /// <param name="monthNo">The month number of the record to delete.</param>
        /// <returns>Returns <c>200 OK</c> with a success flag if deleted, or <c>404 Not Found</c> if the record does not exist.</returns>
        [HttpDelete("project/month")]
        public async Task<IActionResult> DeleteProjectMonth([FromQuery] string project, [FromQuery] int monthNo)
        {
            bool deleted = await _service.DeleteProjectMonthAsync(project, monthNo);
            if (!deleted)
                return NotFound();
            return Ok(new { success = deleted });
        }
    }
}
