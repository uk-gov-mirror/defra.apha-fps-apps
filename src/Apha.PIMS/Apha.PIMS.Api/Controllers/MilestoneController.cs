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
    [Route("api/v{version:apiVersion}/milestone")]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneService _service;
        private readonly IMapper _mapper;

        public MilestoneController(IMilestoneService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get paged milestones for a project.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMilestones(        
            [FromQuery] QueryParameters<string> parameters, string project)
        {
            PaginatedResult<MilestoneDto> result = await _service.GetAllMilestonesAsync(parameters, project);
            return Ok(_mapper.Map<PaginationRes<MilestoneRes>>(result));
        }

        /// <summary>Get a single milestone.</summary>
        [HttpGet("{project}/milestones/{number}")]
        public async Task<IActionResult> GetMilestone(string project, string number)
        {
            var decodedId = HttpUtility.UrlDecode(number);
            MilestoneDto? result = await _service.GetMilestoneAsync(project, decodedId);
            return Ok(result is null ? null : _mapper.Map<MilestoneRes>(result));
        }

        /// <summary>Create a milestone.</summary>
        [HttpPost("{project}/milestones")]
        public async Task<IActionResult> SaveMilestone(string project, [FromBody] MilestoneReq request)
        {
            MilestoneDto dto = _mapper.Map<MilestoneDto>(request);
            dto.Project = project;
            string? changedBy = User.Identity?.Name is { } name ? name[..Math.Min(10, name.Length)] : null;
            MilestoneDto result = await _service.SaveMilestoneAsync(dto, changedBy);
            return Ok(_mapper.Map<MilestoneRes>(result));
        }

        /// <summary>Update an existing milestone.</summary>
        [HttpPut("{project}/milestones/{number}")]
        public async Task<IActionResult> UpdateMilestone(string project, string number, [FromBody] MilestoneReq request)
        {
            var decodedId = HttpUtility.UrlDecode(number);
            MilestoneDto dto = _mapper.Map<MilestoneDto>(request);
            dto.Project = project;
            dto.Number = decodedId;
            string? changedBy = User.Identity?.Name is { } name ? name[..Math.Min(10, name.Length)] : null;
            MilestoneDto result = await _service.UpdateMilestoneAsync(dto, changedBy);
            return Ok(_mapper.Map<MilestoneRes>(result));
        }

        /// <summary>Delete a milestone.</summary>
        [HttpDelete("{project}/milestones/{number}")]
        public async Task<IActionResult> DeleteMilestone(string project, string number)
        {
            var decodedId = HttpUtility.UrlDecode(number);
            bool deleted = await _service.DeleteMilestoneAsync(project, decodedId);
            return Ok(new { success = deleted });
        }

        /// <summary>Update FormRequired flag for a project.</summary>
        [HttpPatch("{parentProject}/formrequired")]
        public async Task<IActionResult> UpdateFormRequired(string parentProject, [FromBody] bool formRequired)
        {
            bool updated = await _service.UpdateFormRequiredAsync(parentProject, formRequired);
            return Ok(new { success = updated });
        }

        /// <summary>Get milestone types, optionally filtered by milestone/deliverable flag.</summary>
        [HttpGet("milestonetypes")]
        public async Task<IActionResult> GetMilestoneTypes([FromQuery] string? milestoneDeliverable = null)
        {
            List<MilestoneTypeDto> result = await _service.GetMilestoneTypesAsync(milestoneDeliverable);
            return Ok(_mapper.Map<List<MilestoneTypeRes>>(result));
        }
        /// <summary>Get paged financial form dates for a project.</summary>
        [HttpGet("{parentProject}/formdates")]
        public async Task<IActionResult> GetAllMilestoneFormDates(            
            [FromQuery] QueryParameters<string> parameters, string parentProject)
        {
            PaginatedResult<MilestoneFormDatesDto> result =
                await _service.GetAllMilestoneFormDatesAsync(parameters, parentProject);
            return Ok(_mapper.Map<PaginationRes<MilestoneFormDatesRes>>(result));
        }
       
        /// <summary>Get a single financial form dates record.</summary>
        [HttpGet("{parentProject}/formdates/{year}")]
        public async Task<IActionResult> GetMilestoneFormDates(string parentProject, short year)
        {
            MilestoneFormDatesDto? result = await _service.GetMilestoneFormDatesAsync(year, parentProject);
            return result is null ? NotFound() : Ok(_mapper.Map<MilestoneFormDatesRes>(result));
        }

        /// <summary>Create or update a financial form dates record.</summary>
        [HttpPost("{parentProject}/formdates")]
        public async Task<IActionResult> SaveMilestoneFormDates(
            string parentProject, [FromBody] MilestoneFormDatesReq request)
        {
            MilestoneFormDatesDto dto = _mapper.Map<MilestoneFormDatesDto>(request);
            dto.ParentProject = parentProject;
            MilestoneFormDatesDto result = await _service.SaveMilestoneFormDatesAsync(dto);
            return Ok(_mapper.Map<MilestoneFormDatesRes>(result));
        }

        /// <summary>Delete a financial form dates record.</summary>
        [HttpDelete("{parentProject}/formdates/{year}")]
        public async Task<IActionResult> DeleteMilestoneFormDates(string parentProject, short year)
        {
            bool deleted = await _service.DeleteMilestoneFormDatesAsync(year, parentProject);
            return Ok(new { success = deleted });
        }

        /// <summary>Get paged log milestone changes with optional project and number filters.</summary>
        [HttpGet("log")]
        public async Task<IActionResult> GetLogMilestones([FromQuery] QueryParameters<string> parameters,[FromQuery] string? project = null,[FromQuery] string? numberPart1 = null,[FromQuery] string? numberPart2 = null)
        {
            PaginatedResult<LogMilestoneDto> result = await _service.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);
            return Ok(_mapper.Map<PaginationRes<LogMilestoneRes>>(result));
        }
        // ── Staging / Import ─────────────────────────────────────────────────
        /// <summary>Get staging milestone rows, optionally filtered by project.</summary>
        [HttpGet("allstaging")]
        public async Task<IActionResult> GetAllStagingRows([FromQuery] QueryParameters<string> parameters)
        {
            string? createdBy = User.Identity?.Name;
            PaginatedResult<StagingMilestoneDto> result = await _service.GetAllStagingRowsAsync(parameters, createdBy);
            return Ok(_mapper.Map<PaginationRes<StagingMilestoneRes>>(result));
        }

        /// <summary>Get staging milestone rows, optionally filtered by project.</summary>
        [HttpGet("staging")]
        public async Task<IActionResult> GetStagingRows([FromQuery] int id)
        {  
               
            List<StagingMilestoneDto> byId = await _service.GetStagingRowsAsync(id);
            return Ok(_mapper.Map<List<StagingMilestoneRes>>(byId));          

        }

        /// <summary>Add a staging milestone row.</summary>
        [HttpPost("staging/{year:int}")]
        public async Task<IActionResult> AddStagingRow(int year, [FromBody] StagingMilestoneReq request)
        {
            StagingMilestoneDto dto = _mapper.Map<StagingMilestoneDto>(request);
            string? createdBy = User.Identity?.Name;
            StagingMilestoneDto result = await _service.AddStagingRowAsync(dto, year, createdBy);
            return Ok(_mapper.Map<StagingMilestoneRes>(result));
        }

        /// <summary>Update a staging milestone row.</summary>
        [HttpPut("staging/{id:int}")]
        public async Task<IActionResult> UpdateStagingRow(int id, [FromBody] StagingMilestoneReq request)
        {
            StagingMilestoneDto dto = _mapper.Map<StagingMilestoneDto>(request);
            dto.Id = id;
            string? createdBy = User.Identity?.Name;
            StagingMilestoneDto result = await _service.UpdateStagingRowAsync(dto, createdBy);
            return Ok(_mapper.Map<StagingMilestoneRes>(result));
        }

        /// <summary>Delete a single staging milestone row by id.</summary>
        [HttpDelete("staging/{id:int}")]
        public async Task<IActionResult> DeleteStagingRow(int id)
        {
            string? createdBy = User.Identity?.Name;
            bool deleted = await _service.DeleteStagingRowAsync(id, createdBy);
            return Ok(new { success = deleted });
        }

        /// <summary>Clear all staging rows for a project.</summary>
        [HttpDelete("{project}/staging")]
        public async Task<IActionResult> ClearStaging(string project)
        {
            string? createdBy = User.Identity?.Name;
            int rows = await _service.ClearStagingAsync(project, createdBy);
            return Ok(new { deleted = rows });
        }

        /// <summary>Validate staging rows — checks dates, number format and duplicate detection.</summary>
        [HttpPost("{project}/staging/validate")]
        public async Task<IActionResult> ValidateStaging(
            string project,
            [FromQuery] string? typeId = null,
            [FromQuery] bool isDeliverableMode = false
            )
        {
            string? createdBy = User.Identity?.Name;
            await _service.ValidateStagingAsync(project, typeId, isDeliverableMode, createdBy);
            return Ok(new { success = true });
        }

        /// <summary>Import validated staging rows (Note IS NULL) into tblMilestone.</summary>
        [HttpPost("{project}/staging/import")]
        public async Task<IActionResult> ImportStaging(string project)
        {
            string? changedBy = User.Identity?.Name is { } name ? name[..Math.Min(10, name.Length)] : null;
            string? createdBy = User.Identity?.Name;
            int imported = await _service.ImportStagingAsync(project, changedBy, createdBy);
            return Ok(new { imported });
        }

        /// <summary>Import with overwrite — updates existing milestones from staging then clears matched rows.</summary>
        [HttpPost("{project}/staging/import-overwrite")]
        public async Task<IActionResult> ImportWithOverwrite(string project)
        {
            string? changedBy = User.Identity?.Name is { } name ? name[..Math.Min(10, name.Length)] : null;
            string? createdBy = User.Identity?.Name;
            int updated = await _service.ImportWithOverwriteAsync(project, changedBy, createdBy);
            return Ok(new { updated });
        }

        /// <summary>Get the next available milestone number for a project and year.</summary>
        [HttpGet("{project}/staging/nextnumber")]
        public async Task<IActionResult> GetNextMilestoneNumber(string project, [FromQuery] int year)
        {
            string next = await _service.GetNextMilestoneNumberAsync(project, year);
            return Ok(new { next });
        }
    }
}
