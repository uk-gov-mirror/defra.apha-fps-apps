using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/radtrackinvoice")]
    public class RadTrackInvoiceController : ControllerBase
    {
        private readonly IRadTrackInvoiceService _service;
        private readonly IMapper _mapper;
        public RadTrackInvoiceController(IRadTrackInvoiceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters<RadTrackInvoiceFilter> parameters)
        {
            PaginatedResult<RadTrackInvoiceDto> result = await _service.GetAllAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<RadTrackInvoiceRes>>(result));
        }

        [HttpGet("totals")]
        public async Task<IActionResult> GetTotals([FromQuery] RadTrackInvoiceFilter? filter)
        {
            RadTrackInvoiceTotalsDto result = await _service.GetTotalsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            RadTrackInvoiceDto? result = await _service.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(_mapper.Map<RadTrackInvoiceRes>(result));
        }

        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RadTrackInvoiceReq request)
        {
            RadTrackInvoiceDto dto = _mapper.Map<RadTrackInvoiceDto>(request);
            RadTrackInvoiceDto result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                new { id = result.InvoiceCounter },
                _mapper.Map<RadTrackInvoiceRes>(result));
        }

        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RadTrackInvoiceReq request)
        {
            RadTrackInvoiceDto dto = _mapper.Map<RadTrackInvoiceDto>(request);           
            dto.InvoiceCounter = id;
            RadTrackInvoiceDto result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<RadTrackInvoiceRes>(result));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);
            return Ok(deleted);
        }

        // ── Lookup endpoints ─────────────────────────────────────────────────

        [HttpGet("lookups/projects")]
        public async Task<IActionResult> GetProjects()
            => Ok(await _service.GetProjectsAsync());

        [HttpGet("lookups/years")]
        public async Task<IActionResult> GetYears()
            => Ok(await _service.GetYearsAsync());

        [HttpGet("lookups/contracts")]
        public async Task<IActionResult> GetContracts()
            => Ok(await _service.GetContractsAsync());

        [HttpGet("lookups/programs")]
        public async Task<IActionResult> GetPrograms()
            => Ok(await _service.GetProgramsAsync());
    }
}
