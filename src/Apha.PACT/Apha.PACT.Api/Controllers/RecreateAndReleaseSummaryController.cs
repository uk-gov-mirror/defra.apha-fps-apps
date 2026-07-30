using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/recreatereleasesummary")]
    public class RecreateAndReleaseSummaryController : ControllerBase
    {
        private readonly IRecreateAndReleaseSummaryService _service;
        private readonly IBatchJobService _batchJobService;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="RecreateAndReleaseSummaryController"/> with the required
        /// service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve recreate summaries log and release summary data.</param>
        /// <param name="batchJobService">Application service used to retrieve batch job history and status.</param>
        /// <param name="fpsRequestContext">Provides the identity of the currently authenticated user.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public RecreateAndReleaseSummaryController(IRecreateAndReleaseSummaryService service, IBatchJobService batchJobService, IFpsRequestContext fpsRequestContext, IMapper mapper)
        {
            _service = service;
            _batchJobService = batchJobService;
            _fpsRequestContext = fpsRequestContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all recreate summaries logs from the system with pagination and sorting support.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters from FPSApps client.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{T}"/> containing the logs and pagination metadata.
        /// </returns>
        [HttpGet("recreatesummary/log")]
        public async Task<IActionResult> GetRecreateSummaryLog([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetRecreateSummaryLogAsync(query);
            return Ok(_mapper.Map<PaginationRes<RecreateSummaryLogRes>>(result));
        }

        /// <summary>
        /// Retrieves all release summary periods from <c>tblPeriod</c>.
        /// </summary>
        /// <returns><c>200 OK</c> with a list of <see cref="ReleasePeriodRes"/>.</returns>
        [HttpGet("releasesummary")]
        public async Task<IActionResult> GetReleaseSummaries()
        {
            var result = await _service.GetReleaseSummariesAsync();
            return Ok(_mapper.Map<ReleaseSummaryRes>(result));
        }

        /// <summary>
        /// Retrieves all release periods from <c>tblPeriod</c> without settings.
        /// </summary>
        /// <returns><c>200 OK</c> with a list of <see cref="ReleasePeriodRes"/>.</returns>
        [HttpGet("releaseperiods")]
        public async Task<IActionResult> GetReleasePeriods()
        {
            var result = await _service.GetReleasePeriodsAsync();
            return Ok(_mapper.Map<IReadOnlyList<ReleasePeriodRes>>(result));
        }

        /// <summary>
        /// Updates the <c>FinalSummariesRun</c> flag for the specified release period.
        /// </summary>
        /// <param name="request">The period name and new flag value.</param>
        /// <returns><c>200 OK</c> with the updated <see cref="ReleasePeriodRes"/> on success; <c>404 Not Found</c> if the period does not exist.</returns>
        [HttpPut("releasesummary/finalrun")]
        public async Task<IActionResult> SetFinalSummaryRun([FromBody] ReleasePeriodReq request)
        {
            var result = await _service.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail);

            return Ok(_mapper.Map<ReleasePeriodRes>(result));
        }

        /// <summary>
        /// Retrieves the execution history for a batch job by job name.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with a list of <see cref="BatchJobHistoryRes"/>.</returns>
        [HttpGet("/api/v1/recreatesummary/batchjob/history")]
        public async Task<IActionResult> GetBatchJobHistory([FromQuery] QueryParameters<string> query, [FromQuery] string jobName)
        {
            var result = await _batchJobService.GetBatchJobsHistoryAsync(query,jobName);
            return Ok(_mapper.Map<PaginationRes<BatchJobHistoryRes>>(result));
        }

        /// <summary>
        /// Checks whether a batch job can be run (i.e. it is not currently running).
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if the job can run; <c>false</c> if it is already running.</returns>
        [HttpGet("/api/v1/recreatesummary/batchjob/canrun")]
        public async Task<IActionResult> CanRunBatchJob([FromQuery] string jobName)
        {
            var result = await _batchJobService.CanRunBatchJobAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Triggers the RecreateSummaries batch job for the specified month.
        /// Validates that <paramref name="request"/>.<c>Month</c> is between 1 and 12,
        /// confirms a matching release period exists, verifies no instance is already running,
        /// then enqueues the job.
        /// </summary>
        /// <param name="request">Request body containing the target month (1–12).</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobEventTriggerRes"/>.</returns>
        [HttpPost("/api/v1/recreatesummary/trigger")]
        public async Task<IActionResult> TriggerRecreateSummariesJob([FromBody] RecreateSummariesReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _batchJobService.TriggerRecreateSummariesJobAsync(request.Month, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);
            
            return Ok(_mapper.Map<BatchJobEventTriggerRes>(result));
        }
    }
}
