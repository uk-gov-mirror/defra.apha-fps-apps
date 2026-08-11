using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Test Manager RC Pivot (Tests Required By Resource Centre) misc report.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testsrequiredbyrc")]
    public class TestsRequiredByRcController : ControllerBase
    {
        private readonly ITestsRequiredByRcService _service;
        private readonly IMapper _mapper;

        public TestsRequiredByRcController(ITestsRequiredByRcService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns the Tests Required By Resource Centre export rows. When a resource centre
        /// (profit centre) is supplied, the results are filtered to that resource centre.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTestsRequiredByRcAsync([FromQuery] string? profitCentre)
        {
            var result = await _service.GetTestsRequiredByRcAsync(profitCentre);
            return Ok(_mapper.Map<List<TestsRequiredByRcRes>>(result));
        }
    }
}
