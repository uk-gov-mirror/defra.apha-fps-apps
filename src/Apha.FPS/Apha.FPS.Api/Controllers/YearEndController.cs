using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/yearend")]
    public class YearEndController : ControllerBase
    {
        private readonly IYearEndService _yearEndService;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="YearEndController"/> with the required
        /// service and AutoMapper dependencies.
        /// </summary>
        /// <param name="yearEndService">Application service used to manage year-end operations.</param>
        /// <param name="fpsRequestContext">Provides the identity of the currently authenticated user.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public YearEndController(IYearEndService yearEndService, IFpsRequestContext fpsRequestContext, IMapper mapper)
        {
            _yearEndService = yearEndService;
            _fpsRequestContext = fpsRequestContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the execution history for a batch job by job name.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with a list of <see cref="BatchJobHistoryRes"/>.</returns>
        [HttpGet("batchjob/history")]
        public async Task<IActionResult> GetYearEndDataSetupBatchJobHistory([FromQuery] QueryParameters<string> query, [FromQuery] string jobName)
        {
            var result = await _yearEndService.GetBatchJobsHistoryAsync(query, jobName);
            return Ok(_mapper.Map<PaginationRes<BatchJobHistoryRes>>(result));
        }

        /// <summary>
        /// Checks whether year end datasetup batch job can be initiated.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if the job can run; <c>false</c> if it is already running.</returns>
        [HttpGet("dataSetup/caninitiate")]
        public async Task<IActionResult> CanInitiateYearEndDataSetupRequestAsync([FromQuery] string jobName)
        {
            var result = await _yearEndService.CanInitiateYearEndDataSetupRequestAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Checks whether year end datasetup batch job can be Approved.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if initiate request exists for the job; <c>false</c> if no initiate request exists for the job.</returns>
        [HttpGet("dataSetup/canapproveorreject")]
        public async Task<IActionResult> CanApproveOrRejectYearEndDataSetupRequestAsync([FromQuery] string jobName)
        {
            var result = await _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Enqueue the YearEndInitiation batch job for approval.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// all config exists, verifies no instance is already running, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("dataSetup/initiation")]
        public async Task<IActionResult> EnqueueYearEndDataSetupInitiationJob([FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(_mapper.Map<BatchJobQueueRes>(result));

        }

        /// <summary>
        /// Enqueue the YearEndInitiation batch job for approve and publish event for batch job.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// all config exists, aproval and initiator are not same, verifies no instance is already running, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("dataSetup/approval")]
        public async Task<IActionResult> EnqueueYearEndDataSetupApprovalJob([FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupApprovalJobAsync(request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(_mapper.Map<BatchJobEventTriggerRes>(result));
        }

        /// <summary>
        /// Enqueue the YearEndInitiation batch job for approve and publish event for batch job.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// all config exists, aproval and initiator are not same, verifies no instance is already running, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("dataSetup/reject")]
        public async Task<IActionResult> EnqueueYearEndDataSetupRejectJob([FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(result);
        }
    } 
}
