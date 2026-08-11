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
    [Route("api/v{version:apiVersion}/accessuser")]
    public class AccessUserController : ControllerBase
    {
        private readonly IAccessUserService _service;
        private readonly IMapper _mapper;

        public AccessUserController(IAccessUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access users.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<AccessUserDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessUserRes>>(result));
        }

        /// <summary>Get paged access users.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<AccessUserRes>>(result));
        }

        /// <summary>Get all access users for a specific system.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetBySystemId(int systemid)
        {
            List<AccessUserDto> result = await _service.GetBySystemIdAsync(systemid);
            return Ok(_mapper.Map<List<AccessUserRes>>(result));
        }

        /// <summary>Get a specific access user by composite key.</summary>
        [HttpGet("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> GetById(int systemid, string ntlogin)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            AccessUserDto? result = await _service.GetByIdAsync(systemid, decodedLogin);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessUserRes>(result));
        }

        /// <summary>Create a new access user.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccessUserReq request)
        {
            AccessUserDto dto = _mapper.Map<AccessUserDto>(request);
            AccessUserDto created = await _service.CreateAsync(dto);
            AccessUserRes res = _mapper.Map<AccessUserRes>(created);
            return CreatedAtAction(nameof(GetById), new { systemid = res.SystemId, ntlogin = res.NtLogin, version = "1.0" }, res);
        }

        /// <summary>Update an existing access user.</summary>
        [HttpPut("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> Update(int systemid, string ntlogin, [FromBody] AccessUserReq request)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            AccessUserDto dto = _mapper.Map<AccessUserDto>(request);
            dto.SystemId = systemid;
            dto.NtLogin = decodedLogin;
            AccessUserDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<AccessUserRes>(updated));
        }

        /// <summary>Delete an access user by composite key.</summary>
        [HttpDelete("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> Delete(int systemid, string ntlogin)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            bool deleted = await _service.DeleteAsync(systemid, decodedLogin);
            return Ok(deleted);
        }
    }
}
