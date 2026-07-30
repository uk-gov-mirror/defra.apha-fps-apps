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
    /// <summary>
    /// API controller for TestRequirement operations.
    /// </summary>    
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testrequirement")]
    public class TestRequirementController : ControllerBase
    {
        private readonly ITestRequirementService _service;
        private readonly IMapper _mapper;

        public TestRequirementController(ITestRequirementService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves a paged list of TestReqmt records for a given TestCode.</summary>
        [HttpGet("paged/{testCode}")]
        public async Task<IActionResult> GetPagedTestReqmt(
            [FromQuery] QueryParameters<string> query,
            string testCode)
        {
            var result = await _service.GetPagedTestReqmtAsync(query, testCode);
            return Ok(_mapper.Map<PaginationRes<TestRequirementtRes>>(result));
        }

        /// <summary>Retrieves a paged supplier list for a given test code including project manager and computed test cost.</summary>
        [HttpGet("supplier/paged/{testCode}")]
        public async Task<IActionResult> GetPagedBySupplierTestCode(
            [FromQuery] QueryParameters<string> query,
            string testCode,
            [FromQuery] bool showRejected = false)
        {
            var result = await _service.GetPagedBySupplierTestCodeAsync(query, testCode, showRejected);
            return Ok(_mapper.Map<PaginationRes<TestSupplierViewRes>>(result));
        }

        /// <summary>Retrieves a paged list of TestReqmt records for a given ParentProject.</summary>
        [HttpGet("pagedbyproject/{parentProject}")]
        public async Task<IActionResult> GetPagedByProject(
            [FromQuery] QueryParameters<string> query,
            string parentProject)
        {
            var result = await _service.GetPagedTestReqmtByProjectAsync(query, parentProject);
            return Ok(_mapper.Map<PaginationRes<TestRequirementtRes>>(result));
        }

        /// <summary>Retrieves all TestReqmt records for a given TestCode without pagination (for export).</summary>
        [HttpGet("all/{testCode}")]
        public async Task<IActionResult> GetAllTestReqmtForExport(string testCode, [FromQuery] string? filter = null)
        {
            var items = await _service.GetAllTestReqmtForExportAsync(testCode, filter);
            return Ok(_mapper.Map<IEnumerable<TestRequirementtRes>>(items));
        }

        /// <summary>Retrieves a TestReqmt record by composite key.</summary>
        [HttpGet("{testCode}/{buyer}")]
        public async Task<IActionResult> GetTestReqmtById(string testCode, string buyer)
        {
            var result = await _service.GetTestReqmtByIdAsync(testCode, buyer);
            if (result is null)
                throw new KeyNotFoundException($"TestReqmt with TestCode '{testCode}' and Buyer '{buyer}' not found.");
            return Ok(_mapper.Map<TestRequirementtRes>(result));
        }

        /// <summary>Creates a new TestReqmt record.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateTestReqmt([FromBody] TestRequirementReq request)
        {
            var dto = _mapper.Map<TestRequirementtDto>(request);
            var result = await _service.AddTestReqmtAsync(dto);
            return Ok(_mapper.Map<TestRequirementtRes>(result));
        }

        /// <summary>Updates an existing TestReqmt record.</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateTestReqmt([FromBody] TestRequirementReq request)
        {
            var dto = _mapper.Map<TestRequirementtDto>(request);
            var result = await _service.UpdateTestReqmtAsync(dto);
            return Ok(_mapper.Map<TestRequirementtRes>(result));
        }

        /// <summary>Deletes a TestReqmt record by composite key.</summary>
        [HttpDelete("{testCode}/{buyer}")]
        public async Task<IActionResult> DeleteTestReqmt(string testCode, string buyer)
        {
            var deleted = await _service.DeleteTestReqmtAsync(testCode, buyer);
            return Ok(deleted);
        }

        /// <summary>Looks up RecUnitPrice and IsDefraProject. ProjectCode is optional — omitting it returns DefraUnitPrice by default.</summary>
        [HttpGet("pricing")]
        public async Task<IActionResult> GetTestReqmtPricing(
            [FromQuery] string testCode, [FromQuery] string? projectCode = null)
        {
            var result = await _service.GetTestReqmtPricingAsync(testCode, projectCode);
            if (result is null)
                return NotFound($"No pricing found for TestCode '{testCode}'.");
            return Ok(_mapper.Map<TestRequirementtRes>(result));
        }

        /// <summary>Returns paged test requirement breakdown rows from fps.vtestreqbreakdown.</summary>
        [HttpGet("testreqbreakdown")]
        public async Task<IActionResult> GetPlannedTestsByWorkgroup([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPlannedTestsByWorkgroupAsync(query);
            return Ok(_mapper.Map<PaginationRes<TestReqBreakdownRes>>(result));
        }

        /// <summary>Returns paged actuals tests with planned data rows from fps.vqryTestsActualBreakdown.</summary>
        [HttpGet("getactualstestswithplanneddatabyworkgroup")]
        public async Task<IActionResult> GetActualsTestsWithPlannedDataByWorkgroupAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            return Ok(_mapper.Map<PaginationRes<TestActualBreakdownRes>>(result));
        }
    }
}
