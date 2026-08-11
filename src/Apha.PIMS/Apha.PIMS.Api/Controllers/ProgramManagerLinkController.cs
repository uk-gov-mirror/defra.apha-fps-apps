using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/programmanagerlink")]
    public class ProgramManagerLinkController : ControllerBase
    {
        private readonly IProgramManagerLinkService _service;
        private readonly IMapper _mapper;

        public ProgramManagerLinkController(IProgramManagerLinkService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all program manager links.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProgramManagerLinks()
        {
            List<ProgramManagerLinkDto> result = await _service.GetAllProgramManagerLinksAsync();
            return Ok(_mapper.Map<List<ProgramManagerLinkRes>>(result));
        }

        /// <summary>Get distinct programs for dropdown binding.</summary>
        [HttpGet("programs")]
        public async Task<IActionResult> GetPrograms()
        {
            List<ProgramLookupDto> result = await _service.GetProgramsAsync();
            return Ok(_mapper.Map<List<ProgramLookupRes>>(result));
        }

        /// <summary>Get paged program manager links for a specific manager.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedByManager([FromQuery] QueryParameters<string> query, [FromQuery] string manager)
        {
            var decodedManager = HttpUtility.UrlDecode(manager);
            var result = await _service.GetPagedByManagerAsync(query, decodedManager);
            return Ok(_mapper.Map<PaginationRes<ProgramManagerLinkRes>>(result));
        }

        /// <summary>Get all program manager links for a specific program.</summary>
        [HttpGet("{program}")]
        public async Task<IActionResult> GetByProgram(string program)
        {
            var decoded = HttpUtility.UrlDecode(program);
            List<ProgramManagerLinkDto> result = await _service.GetByProgramAsync(decoded);
            return Ok(_mapper.Map<List<ProgramManagerLinkRes>>(result));
        }

        /// <summary>Get all program manager links for a specific manager.</summary>
        [HttpGet("manager/{manager}")]
        public async Task<IActionResult> GetByManager(string manager)
        {
            var decoded = HttpUtility.UrlDecode(manager);
            List<ProgramManagerLinkDto> result = await _service.GetByManagerAsync(decoded);
            return Ok(_mapper.Map<List<ProgramManagerLinkRes>>(result));
        }

        /// <summary>Get a specific program manager link by composite key.</summary>
        [HttpGet("{program}/{manager}")]
        public async Task<IActionResult> GetProgramManagerLinkById(string program, string manager)
        {
            var decodedProgram = HttpUtility.UrlDecode(program);
            var decodedManager = HttpUtility.UrlDecode(manager);
            ProgramManagerLinkDto? result = await _service.GetProgramManagerLinkByIdAsync(decodedProgram, decodedManager);
            return result is null ? NotFound() : Ok(_mapper.Map<ProgramManagerLinkRes>(result));
        }

        /// <summary>Create a new program manager link.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateProgramManagerLink([FromBody] ProgramManagerLinkReq request)
        {
            ProgramManagerLinkDto dto = _mapper.Map<ProgramManagerLinkDto>(request);
            ProgramManagerLinkDto created = await _service.CreateProgramManagerLinkAsync(dto);
            ProgramManagerLinkRes res = _mapper.Map<ProgramManagerLinkRes>(created);
            return CreatedAtAction(nameof(GetProgramManagerLinkById), new { program = res.Program, manager = res.Manager, version = "1.0" }, res);
        }

        /// <summary>Delete a program manager link by composite key.</summary>
        [HttpDelete("{program}/{manager}")]
        public async Task<IActionResult> DeleteProgramManagerLink(string program, string manager)
        {
            var decodedProgram = HttpUtility.UrlDecode(program);
            var decodedManager = HttpUtility.UrlDecode(manager);
            bool deleted = await _service.DeleteProgramManagerLinkAsync(decodedProgram, decodedManager);
            return Ok(deleted);
        }
    }
}
