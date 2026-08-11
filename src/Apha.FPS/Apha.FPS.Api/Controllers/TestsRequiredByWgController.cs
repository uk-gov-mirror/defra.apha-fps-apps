using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Test Manager WG Pivot (Tests Required By Work Group) misc report.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testsrequiredbywg")]
    public class TestsRequiredByWgController : ControllerBase
    {
        private readonly ITestsRequiredByWgService _service;
        private readonly IMapper _mapper;

        public TestsRequiredByWgController(ITestsRequiredByWgService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns the Tests Required By Work Group export rows. When a resource centre
        /// (profit centre) is supplied, the results are filtered to that resource centre.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTestsRequiredByWgAsync([FromQuery] string? profitCentre)
        {
            var result = await _service.GetTestsRequiredByWgAsync(profitCentre);
            return Ok(_mapper.Map<List<TestsRequiredByWgRes>>(result));
        }
    }
}
