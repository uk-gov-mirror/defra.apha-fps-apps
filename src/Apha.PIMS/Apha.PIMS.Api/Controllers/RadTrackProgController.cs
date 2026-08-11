using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/radtrackprog")]
    public class RadTrackProgController : ControllerBase
    {
        private readonly IRadTrackProgService _service;
        private readonly IMapper _mapper;

        public RadTrackProgController(IRadTrackProgService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all RadTrack programmes.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRadTrackProgs()
        {
            List<RadTrackProgDto> result = await _service.GetAllRadTrackProgsAsync();
            return Ok(_mapper.Map<List<RadTrackProgRes>>(result));
        }

        /// <summary>Get distinct non-null Programme names from MY_tlkpProject for dropdown binding.</summary>
        [HttpGet("programs")]
        public async Task<IActionResult> GetAllProgramNames()
        {
            List<string> programs = await _service.GetAllProgramNamesAsync();
            return Ok(programs);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedRadTrackProgs([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedRadTrackProgsAsync(query);
            return Ok(_mapper.Map<PaginationRes<RadTrackProgRes>>(result));
        }

        /// <summary>Get a single RadTrack programme by its natural string PK (program).</summary>
        [HttpGet("{program}")]
        public async Task<IActionResult> GetRadTrackProgByProgram(string program)
        {
            RadTrackProgDto? result = await _service.GetRadTrackProgByProgramAsync(program);
            return result is null ? NotFound() : Ok(_mapper.Map<RadTrackProgRes>(result));
        }

        /// <summary>Create a new RadTrack programme.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateRadTrackProg([FromBody] RadTrackProgReq request)
        {
            RadTrackProgDto dto = _mapper.Map<RadTrackProgDto>(request);
            RadTrackProgDto created = await _service.CreateRadTrackProgAsync(dto);
            RadTrackProgRes res = _mapper.Map<RadTrackProgRes>(created);
            return CreatedAtAction(nameof(GetRadTrackProgByProgram), new { program = res.Program, version = "1.0" }, res);
        }

        /// <summary>Update an existing RadTrack programme.</summary>
        [HttpPut("{program}")]
        public async Task<IActionResult> UpdateRadTrackProg(string program, [FromBody] RadTrackProgReq request)
        {
            RadTrackProgDto dto = _mapper.Map<RadTrackProgDto>(request);
            dto.Program = program;
            RadTrackProgDto updated = await _service.UpdateRadTrackProgAsync(dto);
            return Ok(_mapper.Map<RadTrackProgRes>(updated));
        }

        /// <summary>Delete a RadTrack programme by its natural string PK (program).</summary>
        [HttpDelete("{program}")]
        public async Task<IActionResult> DeleteRadTrackProg(string program)
        {
            bool deleted = await _service.DeleteRadTrackProgAsync(program);
            return Ok(deleted);
        }
    }
}
