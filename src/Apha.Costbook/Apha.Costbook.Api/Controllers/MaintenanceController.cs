using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Apha.Costbook.Api.Controllers
{
    //   Tab 1 (Inflation Figures)  → settings GET/PUT
    //   Tab 2 (Account Categories) → account-categories GET/PUT
    //   Tab 4 (Profit Margins)     → covered by same settings GET/PUT
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/maintenance")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceSettingsService _settingsService;
        private readonly IAccountCategoryMaintenanceService _accountCategoryService;
        private readonly IMapper _mapper;

        public MaintenanceController(
            IMaintenanceSettingsService settingsService,
            IAccountCategoryMaintenanceService accountCategoryService,
            IMapper mapper)
        {
            _settingsService = settingsService;
            _accountCategoryService = accountCategoryService;
            _mapper = mapper;
        }

        // ── Maintenance Settings (Tab 1 Inflation + Tab 4 Profit Margins) ────────
        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var dto = await _settingsService.GetSettingsAsync();
            return Ok(_mapper.Map<MaintenanceSettingsRes>(dto));
        }

        [HttpPut("settings")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateSettings([FromBody] MaintenanceSettingsReq req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<MaintenanceSettingsDto>(req);
            await _settingsService.UpdateSettingsAsync(dto);
            // Re-fetch the updated settings to return the current persisted state
            var updated = await _settingsService.GetSettingsAsync();
            return Ok(_mapper.Map<MaintenanceSettingsRes>(updated));
        }

        // ── Account Categories (Tab 2) ────────────────────────────────────────────
        [HttpGet("account-categories")]
        public async Task<IActionResult> GetAccountCategories()
        {
            var dtos = await _accountCategoryService.GetAllForMaintenanceAsync();
            return Ok(_mapper.Map<List<AccountCategoryMaintenanceRes>>(dtos));
        }

        [HttpGet("account-categories/paginated")]
        public async Task<IActionResult> GetAccountCategoriesPaginated([FromQuery] PaginationReq<string> query)
        {
            var parameters = _mapper.Map<QueryParameters<string>>(query);
            var result = await _accountCategoryService.GetPaginatedAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<AccountCategoryMaintenanceRes>>(result));
        }

        [HttpPut("account-categories/{accShortName}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateAccountCategory(string accShortName, [FromBody] AccountCategoryMaintenanceReq req)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
                throw new ArgumentException("AccShortName route parameter is required.");

            accShortName = WebUtility.UrlDecode(accShortName);

            var updated = await _accountCategoryService.UpdateCsg7GroupAsync(accShortName, req.Csg7Group);
            return Ok(_mapper.Map<AccountCategoryMaintenanceRes>(updated));
        }
    }
}
