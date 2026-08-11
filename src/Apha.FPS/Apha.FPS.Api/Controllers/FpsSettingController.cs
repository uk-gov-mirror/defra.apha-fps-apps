using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for managing FPS settings.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/setting")]
    public class FpsSettingController : ControllerBase
    {
        private readonly IFpsSettingService _fpsSettingService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FpsSettingController"/> class.
        /// </summary>
        /// <param name="fpsSettingService">The FPS setting service.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        public FpsSettingController(
                        IFpsSettingService fpsSettingService,
                        IMapper mapper)
        {
            _fpsSettingService = fpsSettingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all FPS settings.
        /// </summary>
        /// <returns>A list of FPS settings.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = await _fpsSettingService.GetAllSettingsAsync();
            return Ok(_mapper.Map<List<FpsSettingRes>>(result));
        }

        /// <summary>
        /// Gets the configured number of working hours per day.
        /// </summary>
        /// <returns>The hours per day value.</returns>
        [HttpGet("hoursperday")]
        public async Task<IActionResult> GetHoursPerDayAsync()
        {
            var result = await _fpsSettingService.GetHoursPerDayAsync();
            return Ok(result);
        }

        /// <summary>
        /// Gets year-end settings for the current open and planned FPS years.
        /// </summary>
        /// <returns>A list of year-end settings.</returns>
        [HttpGet("yearend")]
        public async Task<IActionResult> GetYearEndSettingsAsync()
        {
            var result = await _fpsSettingService.GetYearEndSettingsAsync();
            return Ok(_mapper.Map<List<FpsYearEndSettingRes>>(result));
        }

        /// <summary>
        /// Creates a new FPS setting.
        /// </summary>
        /// <param name="request">The setting to create.</param>
        /// <returns>The created FPS setting.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] FpsSettingReq request)
        {
            var dto = _mapper.Map<FpsSettingDto>(request);
            var result = await _fpsSettingService.AddSettingAsync(dto);
            return CreatedAtAction(nameof(GetAsync), _mapper.Map<FpsSettingRes>(result));
        }

        /// <summary>
        /// Updates an existing FPS setting.
        /// </summary>
        /// <param name="id">The key of the setting to update.</param>
        /// <param name="request">The updated setting values.</param>
        /// <returns>The updated FPS setting.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(string id, [FromBody] FpsSettingReq request)
        {
            request.Id = id;
            var dto = _mapper.Map<FpsSettingDto>(request);
            var result = await _fpsSettingService.UpdateSettingAsync(dto);
            return Ok(_mapper.Map<FpsSettingRes>(result));
        }

        /// <summary>
        /// Saves an FPS setting (creates or updates).
        /// </summary>
        /// <param name="request">The setting to save.</param>
        /// <returns>The saved FPS setting.</returns>
        [HttpPost("save")]
        public async Task<IActionResult> SaveAsync([FromBody] FpsSettingReq request)
        {
            var dto = _mapper.Map<FpsSettingDto>(request);
            var result = await _fpsSettingService.SaveSettingAsync(dto);
            return Ok(_mapper.Map<FpsSettingRes>(result));
        }
    }
}
