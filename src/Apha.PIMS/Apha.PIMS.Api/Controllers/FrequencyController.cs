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
    [Route("api/v{version:apiVersion}/frequency")]
    public class FrequencyController : ControllerBase
    {
        private readonly IFrequencyService _service;
        private readonly IMapper _mapper;

        public FrequencyController(IFrequencyService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all frequencies.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllFrequencies()
        {
          
            List<FrequencyDto> result = await _service.GetAllFrequenciesAsync();
            return Ok(_mapper.Map<List<FrequencyRes>>(result));
        }

        /// <summary>Get paged frequencies.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedFrequencies([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedFrequenciesAsync(query);
            return Ok(_mapper.Map<PaginationRes<FrequencyRes>>(result));
        }

        /// <summary>Get a single frequency by frequencyid.</summary>
        [HttpGet("{frequencyId:int}")]
        public async Task<IActionResult> GetFrequencyById(int frequencyId)
        {
            FrequencyDto? result = await _service.GetFrequencyByIdAsync(frequencyId);
            return result is null ? NotFound() : Ok(_mapper.Map<FrequencyRes>(result));
        }

        /// <summary>Create a new frequency.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateFrequency([FromBody] FrequencyReq request)
        {
            FrequencyDto dto = _mapper.Map<FrequencyDto>(request);
            FrequencyDto created = await _service.CreateFrequencyAsync(dto);
            FrequencyRes res = _mapper.Map<FrequencyRes>(created);
            return CreatedAtAction(nameof(GetFrequencyById), new { frequencyId = res.Frequencyid, version = "1.0" }, res);
        }

        /// <summary>Update an existing frequency.</summary>
        [HttpPut("{frequencyId:int}")]
        public async Task<IActionResult> UpdateFrequency(int frequencyId, [FromBody] FrequencyReq request)
        {
            FrequencyDto dto = _mapper.Map<FrequencyDto>(request);
            dto.FrequencyId = frequencyId;
            FrequencyDto updated = await _service.UpdateFrequencyAsync(dto);
            return Ok(_mapper.Map<FrequencyRes>(updated));
        }

        /// <summary>Delete a frequency by frequencyid.</summary>
        [HttpDelete("{frequencyId:int}")]
        public async Task<IActionResult> DeleteFrequency(int frequencyId)
        {
            bool deleted = await _service.DeleteFrequencyAsync(frequencyId);
            return Ok(deleted);
        }
    }
}
