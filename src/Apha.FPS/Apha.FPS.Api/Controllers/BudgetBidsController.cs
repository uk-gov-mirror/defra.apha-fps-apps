using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Budget Bids section in the Generic Bid feature.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/budgetbids")]
    public class BudgetBidsController : ControllerBase
    {
        private readonly IBudgetBidsService _service;
        private readonly IMapper _mapper;

        public BudgetBidsController(IBudgetBidsService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns bid view records for a given workgroup.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBidViewAsync([FromQuery] string workgroup)
        {
            var result = await _service.GetBidViewAsync(workgroup);
            return Ok(_mapper.Map<List<BidViewRes>>(result));
        }

        /// <summary>
        /// Returns a paged, filtered and sorted list of bid view records for a given workgroup.
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetBidViewPagedAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string workgroup)
        {
            var result = await _service.GetBidViewPagedAsync(query, workgroup);
            return Ok(_mapper.Map<PaginationRes<BidViewRes>>(result));
        }

        /// <summary>
        /// Returns a single bid by workgroup name and account.
        /// </summary>
        [HttpGet("{WorkGroupName}/{account}")]
        public async Task<IActionResult> GetBidByIdAsync(string WorkGroupName, string account)
        {
            var result = await _service.GetBidByIdAsync(WorkGroupName, account);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<BidRes>(result));
        }

        /// <summary>
        /// Adds a new bid record.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddBidAsync([FromBody] BidReq req)
        {
            var dto = _mapper.Map<BidDto>(req);
            var result = await _service.AddBidAsync(dto);
            return Ok(_mapper.Map<BidRes>(result));
        }

        /// <summary>
        /// Updates an existing bid record.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateBidAsync([FromBody] BidReq req)
        {
            var dto = _mapper.Map<BidDto>(req);
            var result = await _service.UpdateBidAsync(dto);
            return Ok(_mapper.Map<BidRes>(result));
        }

        /// <summary>
        /// Deletes a bid record by workgroup name and account.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteBidAsync([FromQuery] string WorkGroupName, [FromQuery] string account)
        {
            var isDeleted = await _service.DeleteBidAsync(WorkGroupName, account);
            if (!isDeleted)
                throw new KeyNotFoundException("Data not found.");
            return Ok(isDeleted);
        }

        /// <summary>
        /// Returns account categories for budget bids.
        /// </summary>
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccountCategoriesAsync()
        {
            var categories = await _service.GetAccountCategoriesAsync();
            return Ok(_mapper.Map<List<AccountCategoryRes>>(categories));
        }

        /// <summary>
        /// Returns a paged, filtered and sorted list of generic bid records
        /// joined across bids, workgroups and account categories.
        /// </summary>
        [HttpGet("generic/paged")]
        public async Task<IActionResult> GetGenericBidsPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetGenericBidsPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<GenericBidViewRes>>(result));
        }
    }
}
