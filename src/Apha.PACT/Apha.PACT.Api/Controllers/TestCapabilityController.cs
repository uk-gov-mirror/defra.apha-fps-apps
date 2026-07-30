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
    /// API controller for TestCapability operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testcapability")]
    public class TestCapabilityController : ControllerBase
    {
        private readonly ITestCapabilityService _service;
        private readonly IMapper _mapper;

        public TestCapabilityController(ITestCapabilityService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // ── TEST CAPABILITY (Grid 1) ──────────────────────────────────────────

        /// <summary>Retrieves a paged list of TestCapability records filtered by WorkGroup.</summary>
        [HttpGet("paged/workgroup")]
        public async Task<IActionResult> GetPagedByWorkGroup(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? workGroup)
        {
            var result = await _service.GetPagedByWorkGroupAsync(query, workGroup);
            return Ok(_mapper.Map<PaginationRes<TestCapabilityRes>>(result));
        }

        /// <summary>Retrieves a paged list of TestCapability records filtered by TestCode.</summary>
        [HttpGet("paged/testcode")]
        public async Task<IActionResult> GetPagedByTestCode(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? testCode)
        {
            var result = await _service.GetPagedByTestCodeAsync(query, testCode);
            return Ok(_mapper.Map<PaginationRes<TestCapabilityRes>>(result));
        }

        /// <summary>Retrieves a paged list of TestCapability records filtered by PlanPortfolio.</summary>
        [HttpGet("paged/portfolio")]
        public async Task<IActionResult> GetPagedTestCapabilityByPortfolio(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? portfolio)
        {
            var result = await _service.GetPagedTestCapabilityByPortfolioAsync(query, portfolio);
            return Ok(_mapper.Map<PaginationRes<TestCapabilityRes>>(result));
        }

        /// <summary>Retrieves a TestCapability record by composite key.</summary>
        [HttpGet("testcapability/{testCode}/{workGroup}")]
        public async Task<IActionResult> GetTestCapabilityById(string testCode, string workGroup)
        {
            var result = await _service.GetTestCapabilityByIdAsync(testCode, workGroup);
            if (result is null)
                throw new KeyNotFoundException($"TestCapability with TestCode '{testCode}' and WorkGroup '{workGroup}' not found.");
            return Ok(_mapper.Map<TestCapabilityRes>(result));
        }

        /// <summary>Creates a new TestCapability record.</summary>
        [HttpPost("testcapability")]
        public async Task<IActionResult> CreateTestCapability([FromBody] TestCapabilityReq request)
        {
            var dto = _mapper.Map<TestCapabilityDto>(request);
            var result = await _service.AddTestCapabilityAsync(dto);
            return Ok(_mapper.Map<TestCapabilityRes>(result));
        }

        /// <summary>Updates an existing TestCapability record.</summary>
        [HttpPut("testcapability")]
        public async Task<IActionResult> UpdateTestCapability([FromBody] TestCapabilityReq request)
        {
            var dto = _mapper.Map<TestCapabilityDto>(request);
            var result = await _service.UpdateTestCapabilityAsync(dto);
            return Ok(_mapper.Map<TestCapabilityRes>(result));
        }

        /// <summary>Deletes a TestCapability record by composite key.</summary>
        [HttpDelete("testcapability/{testCode}/{workGroup}")]
        public async Task<IActionResult> DeleteTestCapability(string testCode, string workGroup)
        {
            var deleted = await _service.DeleteTestCapabilityAsync(testCode, workGroup);
            return Ok(deleted);
        }

        /// <summary>
        /// Retrieves user test capabilities for the specified workgroup.
        /// </summary>
        /// <param name="workGroup">The workgroup to filter by.</param>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{UserTestCapabilityRes}"/> containing matching records.
        /// </returns>
        [HttpGet("paged/wg-test-capabilities")]
        public async Task<IActionResult> GetPagedWgTestCapabilitiesWithDescriptionAsync([FromQuery] QueryParameters<string> query, [FromQuery] string workGroup)
        {
            var result = await _service.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, workGroup);
            return Ok(_mapper.Map<PaginationRes<WgTestCapabilitiesWithDescriptionRes>>(result));
        }

        // ── PLAN CROSSTAB ────────────────────────────────────────────────────

        /// <summary>Retrieves a paged list of Test Plan CrossTab records with all dynamic columns.</summary>
        [HttpGet("paged/plantestcrosstab")]
        public async Task<IActionResult> GetPagedTestPlanCrossTab(
            [FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);
            return Ok(result);
        }
    }
}
