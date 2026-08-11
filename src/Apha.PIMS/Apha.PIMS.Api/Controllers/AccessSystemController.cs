using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accesssystem")]
    public class AccessSystemController : ControllerBase
    {
        private readonly IAccessSystemService _service;
        private readonly IMapper _mapper;

        public AccessSystemController(IAccessSystemService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access systems (lookup list).</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<AccessSystemDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessSystemRes>>(result));
        }

        /// <summary>Get a specific access system by systemid.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetById(int systemid)
        {
            AccessSystemDto? result = await _service.GetByIdAsync(systemid);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessSystemRes>(result));
        }
    }
}
