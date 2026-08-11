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
    [Route("api/v{version:apiVersion}/report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;
        private readonly IMapper _mapper;

        public ReportController(IReportService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all reports.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            
            List<ReportDto> result = await _service.GetAllReportsAsync();
            return Ok(_mapper.Map<List<ReportRes>>(result));
        }

        /// <summary>Retrieves a paged list of reports.</summary>
        /// <param name="query">Paging, sorting and filter parameters.</param>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedReports([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedReportsAsync(query);
            return Ok(_mapper.Map<PaginationRes<ReportRes>>(result));
        }

        /// <summary>Get a single report by id.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            ReportDto? result = await _service.GetReportByIdAsync(id);
            return result is null ? NotFound() : Ok(_mapper.Map<ReportRes>(result));
        }

        /// <summary>Create a new report.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] ReportReq request)
        {
            ReportDto dto = _mapper.Map<ReportDto>(request);
            ReportDto created = await _service.CreateReportAsync(dto);
            ReportRes res = _mapper.Map<ReportRes>(created);
            return CreatedAtAction(nameof(GetReportById), new { id = res.Id, version = "1.0" }, res);
        }

        /// <summary>Update an existing report.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] ReportReq request)
        {
            ReportDto dto = _mapper.Map<ReportDto>(request);
            dto.Id = id;
            ReportDto updated = await _service.UpdateReportAsync(dto);
            return Ok(_mapper.Map<ReportRes>(updated));
        }

        /// <summary>Delete a report by id.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            bool deleted = await _service.DeleteReportAsync(id);
            return Ok(deleted);
        }
    }
}
