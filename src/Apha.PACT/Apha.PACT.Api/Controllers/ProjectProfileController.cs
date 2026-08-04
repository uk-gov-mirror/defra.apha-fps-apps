using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{

    /// <summary>
    /// API controller for Project Profile graph data operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectprofile")]
    public class ProjectProfileController : ControllerBase
    {
        private readonly IProjectProfileService _service;
        private readonly IMapper _mapper;

        public ProjectProfileController(IProjectProfileService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves the monthly profile and cost data for a given project, used to render the non-cumulative chart.</summary>
        /// <param name="project">The project code to retrieve profile data for.</param>
        /// <returns>Returns <c>200 OK</c> with a list of <see cref="ProjectProfileRes"/> objects.</returns>
        [HttpGet("project/data")]
        public async Task<IActionResult> GetProfile([FromQuery] string project)
        {
            IList<ProjectProfileDto> data = await _service.GetProfileDataAsync(project);
            return Ok(_mapper.Map<IList<ProjectProfileRes>>(data));
        }

        /// <summary>Retrieves the cumulative profile and cost data for a given project, used to render the cumulative chart.</summary>
        /// <param name="project">The project code to retrieve cumulative data for.</param>
        /// <returns>Returns <c>200 OK</c> with a list of <see cref="ProjectProfileCumulativeRes"/> objects.</returns>
        [HttpGet("project/data/cumulative")]
        public async Task<IActionResult> GetCumulative([FromQuery] string project)
        {
            IList<ProjectProfileCumulativeDto> data = await _service.GetCumulativeDataAsync(project);
            return Ok(_mapper.Map<IList<ProjectProfileCumulativeRes>>(data));
        }
    }
}
