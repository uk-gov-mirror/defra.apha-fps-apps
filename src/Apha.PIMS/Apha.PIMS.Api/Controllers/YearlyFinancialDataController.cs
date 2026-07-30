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
    [Route("api/v{version:apiVersion}/yearlyfinancialdata")]
    public class YearlyFinancialDataController : ControllerBase
    {
        private readonly IYearlyFinancialDataService _service;
        private readonly IMapper _mapper;

        public YearlyFinancialDataController(IYearlyFinancialDataService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        
        [HttpGet("{project}")]
        public async Task<IActionResult> GetAll(string project, [FromQuery] PaginationReq<string> query)
        {
            
            QueryParameters<string> parameters = _mapper.Map<QueryParameters<string>>(query);
            parameters.Filter = project;

            PaginatedResult<YearlyFinancialDataDto> result = await _service.GetAllAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<YearlyFinancialDataRes>>(result));
        }

      
        [HttpGet("{year:int}/{project}")]
        public async Task<IActionResult> GetByKey(int year, string project)
        {
            
            YearlyFinancialDataDto? result = await _service.GetByKeyAsync((short)year, project);
            return result is null ? NotFound() : Ok(_mapper.Map<YearlyFinancialDataRes>(result));
        }

      
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] YearlyFinancialDataReq request)
        {
            YearlyFinancialDataDto dto = _mapper.Map<YearlyFinancialDataDto>(request);
            YearlyFinancialDataDto result = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetByKey),
                new { year = result.Year, project = result.Project },
                _mapper.Map<YearlyFinancialDataRes>(result));
        }

        
        [HttpPut("{year:int}/{project}")]
        public async Task<IActionResult> Update(int year, string project, [FromBody] YearlyFinancialDataReq request)
        {
           
            YearlyFinancialDataDto dto = _mapper.Map<YearlyFinancialDataDto>(request);
            dto.Year = (short)year;
            dto.Project = project;

            YearlyFinancialDataDto result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<YearlyFinancialDataRes>(result));
        }

        [HttpDelete("{year:int}/{project}")]
        public async Task<IActionResult> Delete(int year, string project)
        {
            bool deleted = await _service.DeleteAsync((short)year, project);
            return Ok(new { success = deleted });
        }

        
        [HttpGet("{project}/{year:int}/pactcosts")]
        public async Task<IActionResult> GetPactCosts(string project, int year)
        {
            IReadOnlyList<PactProjectYearCostsDto> result = await _service.GetPactCostsAsync(project, (short)year);
            return Ok(_mapper.Map<IReadOnlyList<PactProjectYearCostsRes>>(result));
        }

        [HttpGet("settings/{id}")]
        public async Task<IActionResult> GetSettingValueById(string? id)
        {
            string? result = await _service.GetSettingValueByIdAsync(id ?? string.Empty);
            return Ok(result ?? string.Empty);
        }
    }
}
