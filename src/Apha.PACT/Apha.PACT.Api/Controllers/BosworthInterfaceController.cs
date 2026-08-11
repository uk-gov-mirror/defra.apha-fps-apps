using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/bosworth-interface")]
    public class BosworthInterfaceController : ControllerBase
    {
        private readonly IBosworthInterfaceService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="BosworthInterfaceController"/> with the required
        /// Bosworth interface service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve time purchase project data.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public BosworthInterfaceController(IBosworthInterfaceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves time purchase project data for the specified project.
        /// </summary>
        /// <param name="project">The project code to filter by.</param>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{TimePurchaseProjectRes}"/> containing matching records.
        /// </returns>
        [HttpGet("time-purchase-project")]
        public async Task<IActionResult> GetTimePurchaseProject([FromQuery] string project)
        {
            var result = await _service.GetTimePurchaseProjectAsync(project);
            return Ok(_mapper.Map<IEnumerable<TimePurchaseProjectRes>>(result));
        }

        /// <summary>
        /// Retrieves time sale data for the specified profit centre.
        /// </summary>
        /// <param name="profitCentre">The profit centre to filter by.</param>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{TimeSaleProfitCentreRes}"/> containing matching records.
        /// </returns>
        [HttpGet("time-sale-profit-centre")]
        public async Task<IActionResult> GetTimeSaleProfitCentre([FromQuery] string profitCentre)
        {
            var result = await _service.GetTimeSaleProfitCentreAsync(profitCentre);
            return Ok(_mapper.Map<IEnumerable<TimeSaleProfitCentreRes>>(result));
        }

        /// <summary>
        /// Retrieves time sale data for the specified selling workgroup.
        /// </summary>
        /// <param name="request">Request containing the workgroup to filter by.</param>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{TimeSaleWorkGroupRes}"/> containing matching records.
        /// </returns>
        [HttpGet("time-sale-workgroup")]
        public async Task<IActionResult> GetTimeSaleWorkGroup([FromQuery] TimeSaleWorkGroupReq request)
        {
            var result = await _service.GetTimeSaleWorkGroupAsync(request.WorkGroup ?? string.Empty);
            return Ok(_mapper.Map<IEnumerable<TimeSaleWorkGroupRes>>(result));
        }

        /// <summary>
        /// Retrieves test sale data for the specified selling workgroup.
        /// </summary>
        /// <param name="workGroup">The workgroup to filter by.</param>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{TestSaleSellingWorkgroupRes}"/> containing matching records.
        /// </returns>
        [HttpGet("test-sale-selling-workgroup")]
        public async Task<IActionResult> GetTestSaleSellingWorkgroup([FromQuery] string workGroup)
        {
            var result = await _service.GetTestSaleSellingWorkgroupAsync(workGroup);
            return Ok(_mapper.Map<IEnumerable<TestSaleSellingWorkgroupRes>>(result));
        }

        /// <summary>
        /// Retrieves test sale data for the specified buying project.
        /// </summary>
        /// <param name="parentProject">The parent project to filter by.</param>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{TestSaleBuyingProjectRes}"/> containing matching records.
        /// </returns>
        [HttpGet("test-sale-buying-project")]
        public async Task<IActionResult> GetTestSaleBuyingProject([FromQuery] string parentProject)
        {
            var result = await _service.GetTestSaleBuyingProjectAsync(parentProject);
            return Ok(_mapper.Map<IEnumerable<TestSaleBuyingProjectRes>>(result));
        }
    }
}