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
    [Route("api/v{version:apiVersion}/accesslevel")]
    public class AccessLevelController : ControllerBase
    {
        private readonly IAccessLevelService _service;
        private readonly IMapper _mapper;

        public AccessLevelController(IAccessLevelService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access levels.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            
            List<AccessLevelDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessLevelRes>>(result));
        }

        /// <summary>Get all access levels for a specific system.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetBySystemId(int systemid)
        {
            List<AccessLevelDto> result = await _service.GetBySystemIdAsync(systemid);
            return Ok(_mapper.Map<List<AccessLevelRes>>(result));
        }

        /// <summary>Get a specific access level by composite key.</summary>
        [HttpGet("{systemid:int}/{accesslevelid:int}")]
        public async Task<IActionResult> GetById(int systemid, int accesslevelid)
        {
            AccessLevelDto? result = await _service.GetByIdAsync(systemid, accesslevelid);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessLevelRes>(result));
        }

        /// <summary>Create a new access level.</summary>
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccessLevelRes request)
        {
            AccessLevelDto dto = _mapper.Map<AccessLevelDto>(request);
            AccessLevelDto created = await _service.CreateAsync(dto);
            AccessLevelRes res = _mapper.Map<AccessLevelRes>(created);
            return CreatedAtAction(nameof(GetById), new { systemid = res.SystemId, accesslevelid = res.AccessLevelId, version = "1.0" }, res);
        }

        /// <summary>Update an existing access level.</summary>
        [HttpPut("{systemid:int}/{accesslevelid:int}")]
        public async Task<IActionResult> Update(int systemid, int accesslevelid, [FromBody] AccessLevelRes request)
        {
            AccessLevelDto dto = _mapper.Map<AccessLevelDto>(request);
            dto.SystemId = systemid;
            dto.AccessLevelId = accesslevelid;
            AccessLevelDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<AccessLevelRes>(updated));
        }

        /// <summary>Delete an access level by composite key.</summary>
        [HttpDelete("{systemid:int}/{accesslevelid:int}")]
        public async Task<IActionResult> Delete(int systemid, int accesslevelid)
        {
            await _service.DeleteAsync(systemid, accesslevelid);
            return Ok(true);
        }
    }
}
