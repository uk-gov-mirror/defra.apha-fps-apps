using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Asp.Versioning;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{

    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectdetails")]
    public class ProjectDetailsController : ControllerBase
    {
        private readonly IProjectDetailsService _service;
        private readonly IMapper _mapper;

        public ProjectDetailsController(IProjectDetailsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        
        [HttpGet("{parentproject}/fps")]
        public async Task<IActionResult> GetFpsProjectById(string parentproject)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            ProjectDto? result = await _service.GetFpsProjectByIdAsync(parentproject);
            if (result is null)
            {
                return CreateNullSuccessResponse<ProjectRes>();
            }

            return Ok(_mapper.Map<ProjectRes>(result));
        }

        [HttpGet("{parentproject}/pims")]
        public async Task<IActionResult> GetPimsDetail(string parentproject)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            ProjectDetailDto? result = await _service.GetPimsDetailAsync(parentproject);
            if (result is null)
            {
                return CreateNullSuccessResponse<ProjectDetailRes>();
            }

            return Ok(_mapper.Map<ProjectDetailRes>(result));
        }

       
        [HttpPost("{parentproject}/pims")]
        public async Task<IActionResult> SavePimsDetail(string parentproject, [FromBody] ProjectDetailReq request)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            ProjectDetailDto dto = _mapper.Map<ProjectDetailDto>(request);
            dto.Parentproject = parentproject;
            ProjectDetailDto result = await _service.SavePimsDetailAsync(dto);
            return Ok(_mapper.Map<ProjectDetailRes>(result));
        }

       
        [HttpGet("{parentproject}/proposed")]
        public async Task<IActionResult> GetProposedProject(string parentproject)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            ProposedProjectDto? result = await _service.GetProposedProjectAsync(parentproject);
            if (result is null)
            {
                return CreateNullSuccessResponse<ProposedProjectRes>();
            }

            return Ok(_mapper.Map<ProposedProjectRes>(result));
        }

        
        [HttpPut("{parentproject}/proposed")]
        public async Task<IActionResult> UpdateProposedProject(string parentproject, [FromBody] ProposedProjectReq request)
        {
            parentproject = Uri.UnescapeDataString(parentproject);
            ProposedProjectDto dto = _mapper.Map<ProposedProjectDto>(request);
            dto.Parentproject = parentproject;
            string transferTo = request.TransferTo ?? parentproject;
            ProposedProjectDto result = await _service.UpdateProposedProjectAsync(dto, transferTo);
            return Ok(_mapper.Map<ProposedProjectRes>(result));
        }

        [HttpGet("risks")]
        public async Task<IActionResult> GetAllRisk()
        {
            List<RiskDto> result = await _service.GetAllRiskAsync();
            return Ok(_mapper.Map<List<RiskRes>>(result));
        }

        [HttpGet("years")]
        public async Task<IActionResult> GetAllYear()
        {
            List<YearDto> result = await _service.GetAllYearAsync();
            return Ok(_mapper.Map<List<YearRes>>(result));
        }

        private static JsonResult CreateNullSuccessResponse<T>()
        {
            return new JsonResult(new Apha.Common.Contracts.ApiResponse<T>
            {
                Success = true,
                Data = default,
                Meta = new Apha.Common.Contracts.ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            });
        }
    }
}
