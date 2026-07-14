using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Income/Contribution from Time Sales (frmTimeSellerPC).
    /// Provides read-only grid data and footer totals for a given selling profit centre.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/timeseller")]
    public class ContributionSummaryController : ControllerBase
    {
        private readonly IContributionSummaryService _service;
        private readonly IMapper _mapper;

        public ContributionSummaryController(IContributionSummaryService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns the grid rows for Income/Contribution from Time Sales for a selling profit centre.
        /// Rows are ordered by WorkGroup then WgGrade, scoped to the current FPS year.
        /// </summary>
        /// <param name="sellingPc">Selling profit centre code (e.g. "ASU", "ENV").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("{sellingPc}/rows")]
        public async Task<IActionResult> GetRowsAsync(
            string sellingPc,
            CancellationToken cancellationToken)
        {
            ValidateSellingPc(sellingPc);

            var result = await _service.GetRowsAsync(sellingPc, cancellationToken);
            return Ok(_mapper.Map<List<ContributionSummaryRowRes>>(result));
        }

        /// <summary>
        /// Returns footer totals for the Income/Contribution from Time Sales form.
        /// Includes the animal cost adjustment when sellingPc is "ASU".
        /// </summary>
        /// <param name="sellingPc">Selling profit centre code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("{sellingPc}/totals")]
        public async Task<IActionResult> GetTotalsAsync(
            string sellingPc,
            CancellationToken cancellationToken)
        {
            ValidateSellingPc(sellingPc);

            var result = await _service.GetTotalsAsync(sellingPc, cancellationToken);
            return Ok(_mapper.Map<ContributionSummaryTotalsRes>(result));
        }

        private static void ValidateSellingPc(string sellingPc)
        {
            if (string.IsNullOrWhiteSpace(sellingPc))
                throw new ArgumentException("sellingPc is required.", nameof(sellingPc));
        }
    }
}
