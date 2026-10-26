using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;
using Asp.Versioning;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    /// <summary>Project Year Costs — Additional Cost vs Actual data.</summary>
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectyearcosts")]
    public class ProjectYearCostsController : ControllerBase
    {
        private readonly IProjectYearCostsService _service;
        private readonly IMapper _mapper;

        public ProjectYearCostsController(IProjectYearCostsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Returns paginated Additional Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/additionalactuals")]
        public async Task<IActionResult> GetAdditionalActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AdditionalCostDto> result = await _service.GetAdditionalActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AdditionalCostRes>>(result));
        }

        /// <summary>Returns paginated Additional Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/additionalplans")]
        public async Task<IActionResult> GetAdditionalPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AdditionalCostDto> result = await _service.GetAdditionalPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AdditionalCostRes>>(result));
        }

        /// <summary>Returns paginated Animal Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/animalactuals")]
        public async Task<IActionResult> GetAnimalActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AnimalCostDto> result = await _service.GetAnimalActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AnimalCostRes>>(result));
        }

        /// <summary>Returns paginated Animal Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/animalplans")]
        public async Task<IActionResult> GetAnimalPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<AnimalCostDto> result = await _service.GetAnimalPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<AnimalCostRes>>(result));
        }

        /// <summary>Returns paginated Test Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/testplans")]
        public async Task<IActionResult> GetTestPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<TestCostDto> result = await _service.GetTestPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<TestCostRes>>(result));
        }

        /// <summary>Returns paginated Test Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/testactuals")]
        public async Task<IActionResult> GetTestActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<TestCostDto> result = await _service.GetTestActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<TestCostRes>>(result));
        }

        /// <summary>Returns paginated Staff Cost plans for a given project and year.</summary>
        [HttpGet("{project}/{year}/staffplans")]
        public async Task<IActionResult> GetStaffPlans(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<StaffCostDto> result = await _service.GetStaffPlansAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<StaffCostRes>>(result));
        }

        /// <summary>Returns paginated Staff Cost actuals for a given project and year.</summary>
        [HttpGet("{project}/{year}/staffactuals")]
        public async Task<IActionResult> GetStaffActuals(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<StaffCostDto> result = await _service.GetStaffActualsAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<StaffCostRes>>(result));
        }

        /// <summary>Returns project year details for a given project and year.</summary>
        [HttpGet("{project}/{year}/projectyeardetails")]
        public async Task<IActionResult> GetProjectYearDetails(string project, short year)
        {
            project = Uri.UnescapeDataString(project);
            ProjectYearDetailsDto? result = await _service.GetProjectYearDetailsAsync(project, year);
            if (result is null)
            {
                return CreateNullSuccessResponse<ProjectYearDetailsRes>();
            }

            return Ok(_mapper.Map<ProjectYearDetailsRes>(result));
        }

        /// <summary>Returns paginated Pact Pay data (qryProjectTimeCostCalcs) for a given project and year.</summary>
        [HttpGet("{project}/{year}/pactpay")]
        public async Task<IActionResult> GetPactPay(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<PactPayDto> result = await _service.GetPactPayAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<PactPayRes>>(result));
        }

        /// <summary>Returns paginated Monthly Pact Data (my_projectmonthfinal) for a given project and year.</summary>
        [HttpGet("{project}/{year}/monthlypactdata")]
        public async Task<IActionResult> GetMonthlyPactData(
            string project, short year, [FromQuery] PaginationReq<string> query)
        {
            project = Uri.UnescapeDataString(project);
            PaginationParameters<string> paging = _mapper.Map<PaginationParameters<string>>(query);
            PaginatedResult<MonthlyPactDto> result = await _service.GetMonthlyPactDataAsync(project, year, paging);
            return Ok(_mapper.Map<PaginationRes<MonthlyPactRes>>(result));
        }
            /// <summary>Returns FPS Year Totals (my_fpsyeartotals) for a given project and year.</summary>
            [HttpGet("{project}/{year}/fpsyeartotals")]
            public async Task<IActionResult> GetFpsYearTotals(string project, short year)
            {
                project = Uri.UnescapeDataString(project);
                FpsYearTotalsDto? result = await _service.GetFpsYearTotalsAsync(project, year);
                if (result is null)
                {
                    return CreateNullSuccessResponse<FpsYearTotalsRes>();
                }

                return Ok(_mapper.Map<FpsYearTotalsRes>(result));
            }
                /// <summary>Exports Staff, Test, Animal and Additional Cost plan vs actuals as an Excel workbook (8 sheets).</summary>
                [HttpGet("{project}/{year}/export-excel")]
                public async Task<IActionResult> ExportToExcel(string project, short year)
                {
                    project = Uri.UnescapeDataString(project);
                    byte[] bytes = await _service.ExportProjectYearCostsToExcelAsync(project, year);
                    string fileName = $"ProjectYearCosts_{project}_{year}.xlsx";
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }

            private static JsonResult CreateNullSuccessResponse<T>()
            {
                return new JsonResult(new Apha.Common.Contracts.ApiResponse<T>
                {
                    Success = true,
                    Data = default,
                    Meta = new Apha.Common.Contracts.ApiMeta
                    {
                        CorrelationId = Guid.NewGuid().ToString(),
                        TimestampUtc = DateTime.UtcNow
                    }
                });
            }
            }
        }
