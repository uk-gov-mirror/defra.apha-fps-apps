using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
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
    [Route("api/v{version:apiVersion}/setting")]
    public class SettingController : ControllerBase
    {
        private readonly ISettingService _service;
        private readonly IMapper _mapper;

        public SettingController(ISettingService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all settings.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSettings()
        {
            List<SettingDto> result = await _service.GetAllSettingsAsync();
            return Ok(_mapper.Map<List<SettingRes>>(result));
        }

        /// <summary>Get all user-updateable settings.</summary>
        [HttpGet("userupdateable")]
        public async Task<IActionResult> GetAllUserUpdateableSettings()
        {
            List<SettingDto> result = await _service.GetAllUserUpdateableSettingsAsync();
            return Ok(_mapper.Map<List<SettingRes>>(result));
        }

        /// <summary>Get a single setting by id.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSettingById(string id)
        {
            var decoded = HttpUtility.UrlDecode(id);
            SettingDto? result = await _service.GetSettingByIdAsync(decoded);
            return result is null ? NotFound() : Ok(_mapper.Map<SettingRes>(result));
        }

        /// <summary>Update an existing setting value.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSetting(string id, [FromBody] SettingReq request)
        {
           
            var decoded = HttpUtility.UrlDecode(id);
            SettingDto dto = _mapper.Map<SettingDto>(request);
            dto.Id = decoded;
            SettingDto updated = await _service.UpdateSettingAsync(dto);
            return Ok(_mapper.Map<SettingRes>(updated));
        }
    }
}
