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
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectmanager")]
    public class ProjectManagerController : ControllerBase
    {
        private readonly IProjectManagerService _service;
        private readonly IMapper _mapper;

        public ProjectManagerController(IProjectManagerService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all project managers with paging/sorting/filter support.</summary>
        [HttpGet]
        public async Task<IActionResult> GetPagedProjectManagers([FromQuery] QueryParameters<string>? query = null)
        {
            var result = await _service.GetPagedProjectManagersAsync(query);
            return Ok(_mapper.Map<PaginationRes<ProjectManagerRes>>(result));
        }

        /// <summary>Get manager names for the add/edit dropdown.</summary>
        [HttpGet("names")]
        public async Task<IActionResult> GetManagerNames()
        {
            var result = await _service.GetManagerNamesAsync();
            return Ok(result);
        }

        /// <summary>Get a single project manager by name.</summary>
        [HttpGet("{projectmanager}")]
        public async Task<IActionResult> GetProjectManagerByName(string projectmanager)
        {
            var decoded = HttpUtility.UrlDecode(projectmanager);
            ProjectManagerDto? result = await _service.GetProjectManagerByNameAsync(decoded);
            return result is null ? NotFound() : Ok(_mapper.Map<ProjectManagerRes>(result));
        }

        /// <summary>Create a new project manager.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateProjectManager([FromBody] ProjectManagerReq request)
        {
            ProjectManagerDto dto = _mapper.Map<ProjectManagerDto>(request);
            ProjectManagerDto created = await _service.CreateProjectManagerAsync(dto);
            ProjectManagerRes res = _mapper.Map<ProjectManagerRes>(created);
            return CreatedAtAction(nameof(GetProjectManagerByName), new { projectmanager = res.ProjectManager, version = "1.0" }, res);
        }

        /// <summary>Update an existing project manager.</summary>
        [HttpPut("{projectmanager}")]
        public async Task<IActionResult> UpdateProjectManager(string projectmanager, [FromBody] ProjectManagerReq request)
        {
            var decoded = HttpUtility.UrlDecode(projectmanager);
            ProjectManagerDto dto = _mapper.Map<ProjectManagerDto>(request);
            dto.ProjectManager = decoded;
            ProjectManagerDto updated = await _service.UpdateProjectManagerAsync(dto);
            return Ok(_mapper.Map<ProjectManagerRes>(updated));
        }

        /// <summary>Delete a project manager by name.</summary>
        [HttpDelete("{projectmanager}")]
        public async Task<IActionResult> DeleteProjectManager(string projectmanager)
        {
            var decoded = HttpUtility.UrlDecode(projectmanager);
            bool deleted = await _service.DeleteProjectManagerAsync(decoded);
            return Ok(deleted);
        }
    }
}
