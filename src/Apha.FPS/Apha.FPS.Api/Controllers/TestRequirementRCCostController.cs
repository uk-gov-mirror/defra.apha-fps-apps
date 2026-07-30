using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for project-specific component charges (TestRequirementRCCost).
    /// Manages CRUD for the fps.tbltestrequirementrccost resource.
    /// Composite PK: TestCode + Buyer + ProfitCentre + FpsYear.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin,API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testrequirementrccost")]
    public class TestRequirementRCCostController : ControllerBase
    {
        private readonly ITestRequirementRCCostService _service;
        private readonly IMapper _mapper;

        public TestRequirementRCCostController(ITestRequirementRCCostService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns all project-specific component charges for a given test code for the current FPS year.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [HttpGet("{testCode}")]
        public async Task<IActionResult> GetByTestCodeAsync(
            string testCode,
            [FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPagedByTestCodeAsync(filter, testCode);
            return Ok(_mapper.Map<PaginationRes<TestRequirementRCCostRes>>(result));
        }

        /// <summary>
        /// Returns a single project-specific component charge by composite key
        /// (TestCode + Buyer + ProfitCentre) for the current FPS year.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="buyer">The buyer (project) code.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        [HttpGet("{testCode}/{buyer}/{profitCentre}")]
        public async Task<IActionResult> GetByKeyAsync(string testCode, string buyer, string profitCentre)
        {
            var result = await _service.GetByKeyAsync(testCode, buyer, profitCentre);
            if (result == null)
                throw new KeyNotFoundException("Project component charge entry not found.");
            return Ok(_mapper.Map<TestRequirementRCCostRes>(result));
        }
    }
}
