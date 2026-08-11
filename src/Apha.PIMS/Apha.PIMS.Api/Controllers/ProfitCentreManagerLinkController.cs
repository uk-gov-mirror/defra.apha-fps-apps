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
    [Route("api/v{version:apiVersion}/profitcentremanagerlink")]
    public class ProfitCentreManagerLinkController : ControllerBase
    {
        private readonly IProfitCentreManagerLinkService _service;
        private readonly IMapper _mapper;

        public ProfitCentreManagerLinkController(IProfitCentreManagerLinkService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all profit centre manager links.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProfitCentreManagerLinks()
        {
            List<ProfitCentreManagerLinkDto> result = await _service.GetAllProfitCentreManagerLinksAsync();
            return Ok(_mapper.Map<List<ProfitCentreManagerLinkRes>>(result));
        }

        /// <summary>Get distinct profit centres for dropdown binding.</summary>
        [HttpGet("profitcentres")]
        public async Task<IActionResult> GetProfitCentres()
        {
            List<ProfitCentreLookupDto> result = await _service.GetProfitCentresAsync();
            return Ok(_mapper.Map<List<ProfitCentreLookupRes>>(result));
        }

        /// <summary>Get all profit centre manager links for a specific profit centre.</summary>
        [HttpGet("{profitcentre}")]
        public async Task<IActionResult> GetByProfitCentre(string profitcentre)
        {
            var decoded = HttpUtility.UrlDecode(profitcentre);
            List<ProfitCentreManagerLinkDto> result = await _service.GetByProfitCentreAsync(decoded);
            return Ok(_mapper.Map<List<ProfitCentreManagerLinkRes>>(result));
        }

        /// <summary>Get all profit centre manager links for a specific manager.</summary>
        [HttpGet("manager/{manager}")]
        public async Task<IActionResult> GetByManager(string manager)
        {
            var decoded = HttpUtility.UrlDecode(manager);
            List<ProfitCentreManagerLinkDto> result = await _service.GetByManagerAsync(decoded);
            return Ok(_mapper.Map<List<ProfitCentreManagerLinkRes>>(result));
        }

        /// <summary>Get paged profit centre manager links for a specific manager.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedByManager([FromQuery] QueryParameters<string> query, [FromQuery] string manager)
        {
            var decodedManager = HttpUtility.UrlDecode(manager);
            var result = await _service.GetPagedByManagerAsync(query, decodedManager);
            return Ok(_mapper.Map<PaginationRes<ProfitCentreManagerLinkRes>>(result));
        }

        /// <summary>Get a specific profit centre manager link by composite key.</summary>
        [HttpGet("{profitcentre}/{manager}")]
        public async Task<IActionResult> GetProfitCentreManagerLinkById(string profitcentre, string manager)
        {
            var decodedProfitCentre = HttpUtility.UrlDecode(profitcentre);
            var decodedManager = HttpUtility.UrlDecode(manager);
            ProfitCentreManagerLinkDto? result = await _service.GetProfitCentreManagerLinkByIdAsync(decodedProfitCentre, decodedManager);
            return result is null ? NotFound() : Ok(_mapper.Map<ProfitCentreManagerLinkRes>(result));
        }

        /// <summary>Create a new profit centre manager link.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateProfitCentreManagerLink([FromBody] ProfitCentreManagerLinkReq request)
        {
            ProfitCentreManagerLinkDto dto = _mapper.Map<ProfitCentreManagerLinkDto>(request);
            ProfitCentreManagerLinkDto created = await _service.CreateProfitCentreManagerLinkAsync(dto);
            ProfitCentreManagerLinkRes res = _mapper.Map<ProfitCentreManagerLinkRes>(created);
            return CreatedAtAction(nameof(GetProfitCentreManagerLinkById), new { profitcentre = res.ProfitCentre, manager = res.Manager, version = "1.0" }, res);
        }

        /// <summary>Delete a profit centre manager link by composite key.</summary>
        [HttpDelete("{profitcentre}/{manager}")]
        public async Task<IActionResult> DeleteProfitCentreManagerLink(string profitcentre, string manager)
        {
            var decodedProfitCentre = HttpUtility.UrlDecode(profitcentre);
            var decodedManager = HttpUtility.UrlDecode(manager);
            bool deleted = await _service.DeleteProfitCentreManagerLinkAsync(decodedProfitCentre, decodedManager);
            return Ok(deleted);
        }
    }
}
