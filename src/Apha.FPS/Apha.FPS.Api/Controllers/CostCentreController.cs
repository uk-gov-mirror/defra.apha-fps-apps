using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Cost Centre maintenance operations.
    /// Exposes paged list, single-record lookup, create, update, and delete endpoints
    /// derived from MS Access frmMaintCostCentres, plus a workgroup-lookup endpoint
    /// backed by the stored-procedure repository.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/costcentre")]
    public class CostCentreController : ControllerBase
    {
        private readonly ICostCentreService _costCentreService;
        private readonly IStoredProcRepository _storedProcRepository;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;

        public CostCentreController(
            ICostCentreService costCentreService,
            IStoredProcRepository storedProcRepository,
            IFpsRequestContext fpsRequestContext,
            IMapper mapper)
        {
            _costCentreService = costCentreService ?? throw new ArgumentNullException(nameof(costCentreService));
            _storedProcRepository = storedProcRepository ?? throw new ArgumentNullException(nameof(storedProcRepository));
            _fpsRequestContext = fpsRequestContext ?? throw new ArgumentNullException(nameof(fpsRequestContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // ─── Workgroup Lookup (retained from original implementation) ──────────────

        /// <summary>
        /// Returns all cost centres with their associated work groups for lookup purposes.
        /// Backed by the stored-procedure repository (GetAllCostCentreWorkgroupAsync).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CostCentreWorkgroupRes>>> GetAllCostCentresAsync()
        {
            var costCentres = await _storedProcRepository.GetAllCostCentreWorkgroupAsync();
            return Ok(_mapper.Map<IEnumerable<CostCentreWorkgroupRes>>(costCentres));
        }

        // ─── CRUD Endpoints (frmMaintCostCentres migration) ───────────────────────

        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of cost centres for the active FPS year.
        /// Drives the DataGrid in fps_costcenter_maintenance.html (#gridContainer_costcenterList).
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paginated list of <see cref="CostCentreRes"/>.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllCostCentresPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _costCentreService.GetAllCostCentresPagedAsync(query);
            if (result == null)
                throw new ArgumentException("Cost centre records not found");

            return Ok(_mapper.Map<PaginationRes<CostCentreRes>>(result));
        }

        /// <summary>
        /// Returns a single cost centre by its cost centre number for the active FPS year.
        /// Populates the Edit modal fields (modal-cc-number, modal-cc-profit) in fps_costcenter_maintenance.html.
        /// </summary>
        /// <param name="costCentreNo">Cost centre number (double precision).</param>
        /// <returns><see cref="CostCentreRes"/> if found.</returns>
        [HttpGet("{costCentreNo}")]
        public async Task<ActionResult<CostCentreRes>> GetCostCentreByIdAsync(double costCentreNo)
        {
            var dto = await _costCentreService.GetCostCentreByIdAsync(costCentreNo, _fpsRequestContext.FpsYear);
            if (dto == null)
                throw new ArgumentException($"Cost centre record '{costCentreNo}' not found for FPS year '{_fpsRequestContext.FpsYear}'");

            return Ok(_mapper.Map<CostCentreRes>(dto));
        }

        /// <summary>
        /// Creates a new cost centre record for the active FPS year.
        /// Maps to saveTblCostCentre() in costcenter_maintenance.js.
        /// </summary>
        /// <param name="request">Cost centre creation request (CostCentreNo + ProfitCentre).</param>
        /// <returns>Created <see cref="CostCentreRes"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<CostCentreRes>> CreateCostCentreAsync([FromBody] CostCentreReq request)
        {
            var dto = _mapper.Map<CostCentreDto>(request);
            dto.FpsYear = _fpsRequestContext.FpsYear;
            var created = await _costCentreService.CreateCostCentreAsync(dto);
            return Ok(_mapper.Map<CostCentreRes>(created));
        }

        /// <summary>
        /// Updates an existing cost centre record identified by its cost centre number in the active FPS year.
        /// Maps to updateTblCostCentre() in costcenter_maintenance.js.
        /// </summary>
        /// <param name="costCentreNo">Original cost centre number to identify the record.</param>
        /// <param name="request">Cost centre update request.</param>
        /// <returns>Updated <see cref="CostCentreRes"/>.</returns>
        [HttpPut("{costCentreNo}")]
        public async Task<ActionResult<CostCentreRes>> UpdateCostCentreAsync(
            double costCentreNo,
            [FromBody] CostCentreReq request)
        {
            var dto = _mapper.Map<CostCentreDto>(request);
            dto.FpsYear = _fpsRequestContext.FpsYear;
            var updated = await _costCentreService.UpdateCostCentreAsync(costCentreNo, _fpsRequestContext.FpsYear, dto);
            return Ok(_mapper.Map<CostCentreRes>(updated));
        }

        /// <summary>
        /// Deletes the cost centre record with the given cost centre number in the active FPS year.
        /// Maps to handleTblCostCentreDelete() in costcenter_maintenance.js.
        /// </summary>
        /// <param name="costCentreNo">Cost centre number of the record to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        [HttpDelete("{costCentreNo}")]
        public async Task<IActionResult> DeleteCostCentreAsync(double costCentreNo)
        {
            var deleted = await _costCentreService.DeleteCostCentreAsync(costCentreNo, _fpsRequestContext.FpsYear);

            if (!deleted)
                throw new ArgumentException($"Cost centre record '{costCentreNo}' for FPS year '{_fpsRequestContext.FpsYear}' not found for deletion");

            return Ok(true);
        }
    }
}
