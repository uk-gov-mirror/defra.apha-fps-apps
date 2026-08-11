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
    /// API controller for Profit Centre (Resource Centre) maintenance operations.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/profitcentres")]
    public class ProfitCentreController : ControllerBase
    {
        private readonly IProfitCentreService _profitCentreService;
        private readonly IMapper _mapper;

        public ProfitCentreController(IProfitCentreService profitCentreService, IMapper mapper)
        {
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns all profit centres for the Resource Centre dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            return Ok(_mapper.Map<List<ProfitCentreRes>>(result));
        }

        /// <summary>
        /// Returns all profit centres including their associated timesheet, output-sheet, and layout settings.         
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProfitCentres()
        {
            var items = await _profitCentreService.GetAllProfitCentresAsync();
            return Ok(_mapper.Map<IEnumerable<ProfitCentreRes>>(items));
        }

        /// <summary>
        /// Returns a paginated list of profit centres for maintenance.
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetAllProfitCentresPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _profitCentreService.GetAllProfitCentresPagedAsync(query);
            if (result == null)
                throw new ArgumentException("Profit centre records not found");

            return Ok(_mapper.Map<PaginationRes<ProfitCentreRes>>(result));
        }

        /// <summary>
        /// Returns a single profit centre by ID.
        /// </summary>
        [HttpGet("{profitCentreId}")]
        public async Task<IActionResult> GetProfitCentreByIdAsync(string profitCentreId)
        {
            var result = await _profitCentreService.GetProfitCentreByIdAsync(profitCentreId);
            if (result == null)
                throw new ArgumentException($"Profit centre record with ID: {profitCentreId} not found");

            return Ok(_mapper.Map<ProfitCentreRes>(result));
        }

        /// <summary>
        /// Creates a new profit centre record.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProfitCentreAsync([FromBody] ProfitCentreReq request)
        {
            var dto = _mapper.Map<ProfitCentreDto>(request);
            var created = await _profitCentreService.CreateProfitCentreAsync(dto);
            return Ok(_mapper.Map<ProfitCentreRes>(created));
        }

        /// <summary>
        /// Updates an existing profit centre record.
        /// </summary>
        [HttpPut("{profitCentreId}")]
        public async Task<IActionResult> UpdateProfitCentreAsync(string profitCentreId, [FromBody] ProfitCentreReq request)
        {
            var dto = _mapper.Map<ProfitCentreDto>(request);
            var updated = await _profitCentreService.UpdateProfitCentreAsync(profitCentreId, dto);
            return Ok(_mapper.Map<ProfitCentreRes>(updated));
        }

        /// <summary>
        /// Deletes a profit centre record by ID.
        /// </summary>
        [HttpDelete("{profitCentreId}")]
        public async Task<IActionResult> DeleteProfitCentreAsync(string profitCentreId)
        {
            if (string.IsNullOrWhiteSpace(profitCentreId))
                throw new ArgumentException("Profit centre ID cannot be null or empty.", nameof(profitCentreId));

            var isDeleted = await _profitCentreService.DeleteProfitCentreAsync(profitCentreId);
            if (!isDeleted)
                throw new ArgumentException($"Profit centre record with ID: {profitCentreId} not found for deletion");

            return Ok(isDeleted);
        }

        /// <summary>
        /// Partially updates the timesheet, output-sheet, and timesheet-layout settings for the
        /// specified profit centre. Only the three settings fields are written; other profit-centre
        /// data is left unchanged.
        /// </summary>
        /// <param name="request">Contains the profit-centre code and the new values for
        /// <c>Timesheet</c>, <c>Outputsheet</c>, and <c>TimesheetLayout</c>.</param>
        [HttpPatch("settings")]
        public async Task<IActionResult> PatchSettings([FromBody] UpdateProfitCentreSettingsReq request)
        {
            if (string.IsNullOrWhiteSpace(request.ProfitCentre))
                return BadRequest("ProfitCentre is required.");

            var success = await _profitCentreService.UpdateProfitCentreSettingsAsync(
                request.ProfitCentre,
                request.Timesheet,
                request.Outputsheet,
                request.TimesheetLayout);

            return Ok(success);
        }

        /// <summary>
        /// Returns paginated profit centres with aggregated cost from TimeCostCalcs where Class = 'charge'.
        /// Supports pagination, sorting, and month filtering.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters.</param>
        /// <param name="monthNumber">Month number to filter the cost calculations.</param>
        [HttpGet("paged/costsummary")]
        public async Task<IActionResult> GetPagedProfitCenterCostSummary(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] double monthNumber)
        {
            var result = await _profitCentreService.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);
            return Ok(_mapper.Map<PaginationRes<ProfitCentreCostRes>>(result));
        }

        /// <summary>
        /// Returns a paginated list of workgroup staff plan records from fps.vpvtworkgroupstaffplan,
        /// filtered by the specified workgroup.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters.</param>
        /// <param name="workGroup">Workgroup name to filter by (required).</param>
        [HttpGet("wgstaffplan")]
        public async Task<IActionResult> GetPagedWgStaffPlan(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workGroup)
        {
            if (string.IsNullOrWhiteSpace(workGroup))
                return BadRequest("workGroup is required.");

            var result = await _profitCentreService.GetPagedWgStaffPlanAsync(query, workGroup);
            return Ok(_mapper.Map<PaginationRes<WgStaffPlanViewRes>>(result));
        }

    }
}
