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
    /// API controller for component charges per profit centre (TestRCCost).
    /// Manages CRUD for the fps.tbltestrccost resource.
    /// Composite PK: TestCode + ProfitCentre + FpsYear.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin,API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testrccost")]
    public class TestRCCostController : ControllerBase
    {
        private readonly ITestRCCostService _service;
        private readonly IMapper _mapper;

        public TestRCCostController(ITestRCCostService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns all component charges for a given test code for the current FPS year.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [HttpGet("{testCode}")]
        public async Task<IActionResult> GetByTestCodeAsync(
            string testCode,
            [FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPagedByTestCodeAsync(filter, testCode);
            return Ok(_mapper.Map<PaginationRes<TestRCCostRes>>(result));
        }

        /// <summary>
        /// Returns a single component charge by composite key (TestCode + ProfitCentre) for the current FPS year.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        [HttpGet("{testCode}/{profitCentre}")]
        public async Task<IActionResult> GetByKeyAsync(string testCode, string profitCentre)
        {
            var result = await _service.GetByKeyAsync(testCode, profitCentre);

            if (result == null)
                return Ok(new TestRCCostRes());

            return Ok(_mapper.Map<TestRCCostRes>(result));
        }
    }
}
