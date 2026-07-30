using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin,API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroup")]
    public class WorkGroupController : ControllerBase
    {
        private readonly IWorkGroupService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="WorkGroupController"/> with the required
        /// work group service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve work group and time code data.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public WorkGroupController(IWorkGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all work groups available in the system.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{WorkGroupRes}"/> containing all work groups.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllWorkGroupsAsync();
            return Ok(_mapper.Map<IEnumerable<WorkGroupRes>>(items));
        }

        /// <summary>Returns all WorkGroup names for dropdown population.</summary>
        [HttpGet("names")]
        public async Task<IActionResult> GetAllWorkGroupNamesAsync()
        {
            var result = await _service.GetAllWorkGroupNamesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paged and sorted list of time codes associated with work groups,
        /// optionally filtered by work group name and calendar month.
        /// </summary>
        /// <param name="query">Pagination, sorting, and column filter parameters for the request.</param>
        /// <param name="workGroup">Optional work group name to restrict results to a specific work group.</param>
        /// <param name="monthNumber">Optional calendar month number to restrict results to a specific month.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{WorkGroupTimeCodeRes}"/> containing the paged time code records
        /// and associated pagination metadata.
        /// </returns>
        [HttpGet("paged/timecodes")]
        public async Task<IActionResult> GetPagedWorkGroupTimeCodes(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workGroup,
            [FromQuery] int monthNumber = 1)
        {
            var result = await _service.GetWorkGroupTimeCodeAsync(query, workGroup, monthNumber);
            return Ok(_mapper.Map<PaginationRes<WorkGroupTimeCodeRes>>(result));
        }

        /// <summary>
        /// Retrieves a paged and sorted list of valid time codes associated with work groups,
        /// optionally filtered by work group name. Each record joins TimeCodeValid with the
        /// corresponding Project to include the project title.
        /// </summary>
        /// <param name="query">Pagination, sorting, and column filter parameters for the request.</param>
        /// <param name="workGroup">Optional work group name to restrict results to a specific work group.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{WorkGroupValidTimeCodeRes}"/> containing the paged valid time code records
        /// and associated pagination metadata.
        /// </returns>
        [HttpGet("paged/validtimecodes")]
        public async Task<IActionResult> GetPagedWorkGroupValidTimeCodes(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workGroup)
        {
            var result = await _service.GetWorkGroupValidTimeCodeAsync(query, workGroup);
            return Ok(_mapper.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(result));
        }

        /// <summary>
        /// Retrieves work group time usage rows pivoted across the 12 fiscal-year months (April – March).
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="workGroup">Work group name to filter results by.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="WgSummarisedStaffTimeUsageRes"/> containing paged rows and
        /// pre-computed footer summary.
        /// </returns>
        [HttpGet("staff/paged/summarisedtimeusage")]
        public async Task<IActionResult> GetWgSummarisedStaffTimeUsage(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string staffName)
        {
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, staffName);
            return Ok(_mapper.Map<WgSummarisedStaffTimeUsageRes>(result));
        }

        /// <summary>
        /// Retrieves a paged and sorted list of summarised workgroup time data,
        /// optionally filtered by work group name. Returns pivoted monthly time allocations
        /// along with computed totals and budget information.
        /// </summary>
        /// <param name="query">Pagination, sorting, and column filter parameters for the request.</param>
        /// <param name="workGroup">Optional work group name to restrict results to a specific work group.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="SummarisedWgTimePivotRes"/> containing the summarised workgroup time data
        /// with dynamic month columns and pagination metadata.
        /// </returns>
        [HttpGet("paged/summarisedtimeusage")]
        public async Task<IActionResult> GetPagedSummarisedWorkgroupTime(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workGroup)
        {
            var result = await _service.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
            return Ok(_mapper.Map<SummarisedWgTimePivotRes>(result));
        }

        /// <summary>Returns workgroups filtered by profit centre for budget pages (user-email filtered view).</summary>
        [HttpGet("budget/by-profitcentre")]
        public async Task<IActionResult> GetWorkGroupsByProfitCentreForBudgetAsync([FromQuery] string profitCentre)
        {
            var result = await _service.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
            return Ok(_mapper.Map<List<WorkGroupViewRes>>(result));
        }

        /// <summary>Returns a paged, filtered and sorted list of workgroups for a profit centre (budget view).</summary>
        [HttpGet("budget/by-profitcentre/paged")]
        public async Task<IActionResult> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string profitCentre)
        {
            var result = await _service.GetWorkGroupsByProfitCentreForBudgetPagedAsync(query, profitCentre);
            return Ok(_mapper.Map<PaginationRes<WorkGroupViewRes>>(result));
        }

        /// <summary>
        /// Returns a paged, filtered, and sorted list of work groups for the specified profit centre.
        /// Pagination, sort column, sort direction, and column filters are supplied via
        /// <paramref name="query"/>; the target profit centre is supplied via <paramref name="profitCentre"/>.
        /// </summary>
        /// <param name="query">Pagination and filter parameters forwarded from the DataGrid client.</param>
        /// <param name="profitCentre">The profit-centre code used to scope the work-group query.</param>
        [HttpGet("profitcentre")]
        public async Task<IActionResult> GetWorkGroupsByProfitCentre(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string profitCentre)
        {
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, profitCentre);
            return Ok(_mapper.Map<PaginationRes<WorkGroupRes>>(result));
        }

        /// <summary>
        /// Sets or clears the <c>SendEmail</c> flag for all work groups belonging to the specified
        /// profit centre. Pass <c>SendEmail = 1</c> to flag for sending, or <c>0</c> to clear.
        /// </summary>
        /// <param name="request">Contains the target profit-centre code and the desired flag value.</param>
        [HttpPut("setsendemail/profitcentre")]
        public async Task<IActionResult> SetSendEmailForProfitCentreWorkGroupsAsync([FromBody] UpdateSendEmailFlagReq request)
        {
            if (string.IsNullOrWhiteSpace(request.ProfitCentre))
                return BadRequest("ProfitCentre is required.");

            var success = await _service.SetSendEmailForProfitCentreWorkGroupsAsync(request.ProfitCentre, request.SendEmail);
            return Ok(success);
        }

        /// <summary>
        /// Clears the <c>SendEmail</c> flag (sets to <c>0</c>) for every work group across all
        /// profit centres. Typically called before a fresh selection is made.
        /// </summary>
        /// <param name="request">Contains the flag value to apply (expected <c>0</c> to clear).</param>
        [HttpPut("setsendemail/all")]
        public async Task<IActionResult> SetSendEmailForAllWorkGroupsAsync([FromBody] UpdateSendEmailFlagReq request)
        {
            var success = await _service.SetSendEmailForAllWorkGroupsAsync(request.SendEmail);
            return Ok(success);
        }

        /// <summary>
        /// Updates the <c>SendEmail</c> flag and <c>EmailRecipient</c> for a single work group
        /// identified by <paramref name="workGroupName"/>.
        /// </summary>
        /// <param name="workGroupName">The unique name of the work group to update.</param>
        /// <param name="request">Contains the new send-email flag value and optional email recipient address.</param>
        [HttpPut("{workGroupName}/email")]
        public async Task<IActionResult> UpdateWorkGroupEmail(
            string workGroupName,
            [FromBody] UpdateWorkGroupEmailReq request)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return BadRequest("WorkGroupName is required.");

            if (!string.IsNullOrWhiteSpace(request.WorkGroupName) &&
                !string.Equals(request.WorkGroupName, workGroupName, StringComparison.OrdinalIgnoreCase))
                return BadRequest("WorkGroupName in the request body does not match the route parameter.");

            var success = await _service.UpdateWorkGroupEmailAsync(workGroupName, request.SendEmail, request.EmailRecipient);
            return Ok(success);
        }

        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of workgroups for maintenance.
        /// </summary>
        [HttpGet("paged")]
        public async Task<ActionResult<PaginationRes<WorkGroupMaintenanceRes>>> GetPagedAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedAsync(query);
            if (result == null)
            {
                throw new ArgumentException("Workgroup records not found.");
            }
            return Ok(_mapper.Map<PaginationRes<WorkGroupMaintenanceRes>>(result));
        }

        /// <summary>
        /// Returns a single workgroup by its WorkGroupName.
        /// </summary>
        [HttpGet("maintenance/{workGroupName}")]
        public async Task<ActionResult<WorkGroupMaintenanceRes>> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var dto = await _service.GetByKeyAsync(workGroupName);
            if (dto == null)
            {
                throw new KeyNotFoundException($"Workgroup '{workGroupName}' not found.");
            }
            return Ok(_mapper.Map<WorkGroupMaintenanceRes>(dto));
        }

        /// <summary>
        /// Creates a new workgroup record.
        /// </summary>
        [HttpPost("maintenance")]
        public async Task<ActionResult<WorkGroupMaintenanceRes>> CreateAsync([FromBody] WorkGroupMaintenanceReq request)
        {
            var dto = _mapper.Map<WorkGroupDto>(request);
            var created = await _service.CreateAsync(dto);
            return Ok(_mapper.Map<WorkGroupMaintenanceRes>(created));
        }

        /// <summary>
        /// Updates an existing workgroup identified by the original WorkGroupName in the route.
        /// </summary>
        [HttpPut("maintenance/{workGroupName}")]
        public async Task<ActionResult<WorkGroupMaintenanceRes>> UpdateAsync(
            string workGroupName,
            [FromBody] WorkGroupMaintenanceReq request)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var dto = _mapper.Map<WorkGroupDto>(request);
            var updated = await _service.UpdateAsync(workGroupName, dto);
            return Ok(_mapper.Map<WorkGroupMaintenanceRes>(updated));
        }

        /// <summary>
        /// Deletes the workgroup with the given WorkGroupName.
        /// </summary>
        [HttpDelete("maintenance/{workGroupName}")]
        public async Task<IActionResult> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new ArgumentException("WorkGroupName cannot be null or empty.", nameof(workGroupName));
            }

            var deleted = await _service.DeleteAsync(workGroupName);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Workgroup '{workGroupName}' not found.");
            }
            return Ok(true);
        }

        /// <summary>
        /// Returns all available profit centre identifiers for the ResourceCentre dropdown.
        /// </summary>
        [HttpGet("profitcentres")]
        public async Task<ActionResult<IEnumerable<string>>> GetProfitCentresAsync()
        {
            var result = await _service.GetAllProfitCentresAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns all manager records for the Owner dropdown.
        /// </summary>
        [HttpGet("owners")]
        public async Task<ActionResult<IEnumerable<OwnerRes>>> GetOwnersAsync()
        {
            var ownerDtos = await _service.GetOwnersAsync();
            return Ok(_mapper.Map<IEnumerable<OwnerRes>>(ownerDtos));
        }

        /// <summary>
        /// Returns cost centre values linked to the given profit centre for the cascading dropdown.
        /// </summary>
        [HttpGet("costcentres")]
        public async Task<ActionResult<IEnumerable<double?>>> GetCostCentresAsync([FromQuery] string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new ArgumentException("ProfitCentre cannot be null or empty.", nameof(profitCentre));
            }

            var result = await _service.GetCostCentresByProfitCentreAsync(profitCentre);
            return Ok(result);
        }
    }
}
