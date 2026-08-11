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
    [Route("api/v{version:apiVersion}/reportgrouplink")]
    public class ReportGroupLinkController : ControllerBase
    {
        private readonly IReportGroupLinkService _service;
        private readonly IMapper _mapper;

        public ReportGroupLinkController(IReportGroupLinkService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all report group links.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReportGroupLinks()
        {
            List<ReportGroupLinkDto> result = await _service.GetAllReportGroupLinksAsync();
            return Ok(_mapper.Map<List<ReportGroupLinkRes>>(result));
        }

        /// <summary>Get all report group links for a specific report.</summary>
        [HttpGet("{reportid:int}")]
        public async Task<IActionResult> GetReportGroupLinksByReportId(int reportid)
        {
            List<ReportGroupLinkDto> result = await _service.GetReportGroupLinksByReportIdAsync(reportid);
            return Ok(_mapper.Map<List<ReportGroupLinkRes>>(result));
        }

        /// <summary>Get a specific report group link by composite key.</summary>
        [HttpGet("{reportid:int}/{groupid:int}")]
        public async Task<IActionResult> GetReportGroupLinkById(int reportid, int groupid)
        {
            ReportGroupLinkDto? result = await _service.GetReportGroupLinkByIdAsync(reportid, groupid);
            return result is null ? NotFound() : Ok(_mapper.Map<ReportGroupLinkRes>(result));
        }

        /// <summary>Create a new report group link.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateReportGroupLink([FromBody] ReportGroupLinkReq request)
        {
            ReportGroupLinkDto dto = _mapper.Map<ReportGroupLinkDto>(request);
            ReportGroupLinkDto created = await _service.CreateReportGroupLinkAsync(dto);
            ReportGroupLinkRes res = _mapper.Map<ReportGroupLinkRes>(created);
            return CreatedAtAction(nameof(GetReportGroupLinkById), new { reportid = res.ReportId, groupid = res.GroupId, version = "1.0" }, res);
        }

        /// <summary>Delete a report group link by composite key.</summary>
        [HttpDelete("{reportid:int}/{groupid:int}")]
        public async Task<IActionResult> DeleteReportGroupLink(int reportid, int groupid)
        {
            bool deleted = await _service.DeleteReportGroupLinkAsync(reportid, groupid);
            return Ok(deleted);
        }
    }
}
