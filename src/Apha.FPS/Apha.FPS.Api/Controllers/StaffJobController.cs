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
    /// API controller for managing staff job assignments and related data.
    /// </summary>
    /// 
   [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
   [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/staffjob")]
    public class StaffJobController : ControllerBase
    {
        private readonly IStaffJobService _staffJobService;
        private readonly IMapper _mapper;
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffJobController"/> class.
        /// </summary>
        /// <param name="staffJobService">Service for staff job operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public StaffJobController(
                        IStaffJobService staffJobService,
                        IMapper mapper)
        {
            _staffJobService = staffJobService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of staff job costs.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <returns>Paginated list of staff job cost view results.</returns>
        [HttpGet]
        public async Task<IActionResult> GetJobStaffCostAsync([FromQuery] PaginationReq<string> query, string jobCode)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _staffJobService.GetJobStaffCostAsync(filter, jobCode);
            return Ok(_mapper.Map<PaginationRes<StaffJobViewRes>>(result));
        }

        /// <summary>
        /// Retrieves a lookup list of staff workgroups.
        /// </summary>
        /// <returns>List of staff workgroup lookup results.</returns>
        [HttpGet("workgrouplookup")]
        public async Task<IActionResult> GetStaffWorkgroupLookup()
        {
            var result = await _staffJobService.GetStaffWorkgroupLookup();
            return Ok(_mapper.Map<List<StaffWorkgroupLookupRes>>(result));
        }

        /// <summary>
        /// Retrieves the time-summary data (HrsPaid, Leave, SickSpecial, HrsAvail) for a specific staff member.
        /// </summary>
        [HttpGet("staffsummary")]
        public async Task<IActionResult> GetStaffSummaryByIdAsync([FromQuery] string staffId)
        {
            var result = await _staffJobService.GetStaffSummaryByIdAsync(staffId);
            if (result == null)
                return NotFound();
            return Ok(_mapper.Map<StaffWorkgroupLookupRes>(result));
        }

        /// <summary>
        /// Returns the total planned ZT hours for a specific staff member.
        /// </summary>
        [HttpGet("zttotalhours")]
        public async Task<IActionResult> GetZtTotalHoursByStaffIdAsync([FromQuery] string staffId)
        {
            var total = await _staffJobService.GetZtTotalHoursByStaffIdAsync(staffId);
            return Ok(total);
        }

        /// <summary>
        /// Returns a paged, sorted and filtered list of staff job allocation rows for a job code and workgroup grade.
        /// </summary>
        [HttpGet("staffjobsallocation/paged")]
        public async Task<IActionResult> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string jobcode,
            [FromQuery] string wgGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _staffJobService.GetStaffJobsAllocationByJobCodeWgGradePagedAsync(filter, jobcode, wgGrade);
            return Ok(_mapper.Map<PaginationRes<StaffJobViewRes>>(result));
        }

        /// <summary>
        /// Returns a paged, sorted and filtered list of ZT-type staff job rows for a specific staff member.
        /// </summary>
        [HttpGet("ztstaffjobs/paged")]
        public async Task<IActionResult> GetZtStaffJobsByStaffIdPagedAsync([FromQuery] PaginationReq<string> query, [FromQuery] string staffId)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _staffJobService.GetZtStaffJobsByStaffIdPagedAsync(filter, staffId);
            return Ok(_mapper.Map<PaginationRes<StaffJobZtViewRes>>(result));
        }

        /// <summary>
        /// Returns a single ZT staff job record with description for a specific staff member and job code.
        /// </summary>
        [HttpGet("ztstaffjob/{staffId}/{jobCode}")]
        public async Task<IActionResult> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode)
        {
            var result = await _staffJobService.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
            if (result == null)
                throw new KeyNotFoundException($"ZT plan entry for staff '{staffId}' and job code '{jobCode}' not found.");
            return Ok(_mapper.Map<StaffJobZtViewRes>(result));
        }

        /// <summary>
        /// Retrieves the charge rate for a specific staff member and job code.
        /// </summary>
        /// <param name="staffId">The staff member's identifier.</param>
        /// <param name="jobcode">The job code.</param>
        /// <returns>The charge rate as a decimal value.</returns>
        [HttpGet("chargerate")]
        public async Task<IActionResult> GetStaffChargeRate([FromQuery] string staffId, [FromQuery] string jobcode)
        {
            var chargeRate = await _staffJobService.GetStaffChargeRate(staffId, jobcode);
            return Ok(chargeRate);
        }

        /// <summary>
        /// Returns the total staff cost (all records, unpaged) for a given job code.
        /// </summary>
        /// <param name="jobCode">The job code.</param>
        /// <returns>Total staff cost as a decimal.</returns>
        [HttpGet("totalstaffcost")]
        public async Task<IActionResult> GetTotalStaffCostAsync([FromQuery] string jobCode)
        {
            var total = await _staffJobService.GetTotalStaffCostAsync(jobCode);
            return Ok(total);
        }

        /// <summary>
        /// Retrieves a staff job assignment by staff ID and job code.
        /// </summary>
        /// <param name="staffId">The staff member's identifier.</param>
        /// <param name="jobCode">The job code.</param>
        /// <returns>The staff job assignment details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the staff job is not found.</exception>
        [HttpGet("{staffId}/{jobCode}")]
        public async Task<IActionResult> GetByIdAsync(string staffId, string jobCode)
        {
            var result = await _staffJobService.GetByIdAsync(staffId, jobCode);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<StaffJobRes>(result));
        }

        [HttpGet("view")]
        public async Task<IActionResult> GetViewByStaffIdAsync([FromQuery] string staffId, [FromQuery] string jobcode)
        {
            var staffRecord = await _staffJobService.GetViewByStaffIdAsync(staffId, jobcode);
            return Ok(_mapper.Map<StaffJobViewRes>(staffRecord));
        }

        /// <summary>
        /// Adds a new staff job assignment.
        /// </summary>
        /// <param name="staffJobReq">The staff job request data.</param>
        /// <returns>The created staff job assignment.</returns>
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] StaffJobReq staffJobReq)
        {
            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobReq);
            var result = await _staffJobService.AddAsync(staffJobDto);
            return CreatedAtAction(nameof(GetByIdAsync), new { staffId = result.StaffId, jobCode = result.JobCode }, _mapper.Map<StaffJobRes>(result));
        }

        /// Updates an existing staff job assignment.
        /// </summary>
        /// <param name="staffJobReq">The staff job request data.</param>
        /// <returns>The updated staff job assignment.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] StaffJobReq staffJobReq)
        {
            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobReq);
            var result = await _staffJobService.UpdateAsync(staffJobDto);
            return Ok(_mapper.Map<StaffJobRes>(result));
        }

        /// <summary>
        /// Deletes a staff job assignment by staff ID and job code.
        /// </summary>
        /// <param name="staffId">The staff member's identifier.</param>
        /// <param name="jobCode">The job code.</param>
        /// <returns>No content if deletion is successful; NotFound if not found.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string staffId, [FromQuery] string jobCode)
        {
            var isDeleted = await _staffJobService.DeleteAsync(staffId, jobCode);
            if (!isDeleted)
                throw new KeyNotFoundException("Data not found.");
            return Ok(isDeleted);
        }

        /// <summary>
        /// Returns a paged, sorted and filtered resource utilisation summary for a given workgroup.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="workgroup">The workgroup identifier to filter by.</param>
        /// <returns>Paginated list of staff resource utilisation rows.</returns>
        [HttpGet("resourceutilisation")]
        public async Task<IActionResult> GetStaffResourceUtilisationAsync(
            [FromQuery] PaginationReq<string> query, [FromQuery] string workgroup)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _staffJobService.GetStaffResourceUtilisationAsync(filter, workgroup);
            return Ok(_mapper.Map<PaginationRes<StaffResourceUtilisationRes>>(result));
        }
    }
}