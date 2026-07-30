using Apha.Common.Utilities.EventPublisher;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using Microsoft.Graph.Models;
using System.Text.Json;

namespace Apha.PACT.Application.Services
{
    public class BatchJobService : IBatchJobService
    {
        private const string RecreateSummariesJobName = "RecreateSummary";

        private readonly IBatchJobRepository _repository;
        private readonly IRecreateAndReleaseSummaryRepository _releaseRepository;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IMapper _mapper;

        public BatchJobService(IBatchJobRepository repository, IRecreateAndReleaseSummaryRepository releaseRepository,
            IEventPublisherService eventPublisherService, IMapper mapper)
        {
            _repository = repository;
            _releaseRepository = releaseRepository;
            _eventPublisherService= eventPublisherService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var history = await _repository.GetBatchJobsHistoryAsync(filter, jobName);
            return _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(history);
        }

        public async Task<bool> CanRunBatchJobAsync(string jobName)
        {
            return await _repository.CanRunBatchJobAsync(jobName);
        }

        public async Task<BatchJobEventTriggerDto> TriggerRecreateSummariesJobAsync(int month, int contextyear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            
            var note = $"'{RecreateSummariesJobName}' is initiated for {month} - {contextyear}.";

            if (month < 1 || month > 12)
                errors.Add(new BusinessValidationError("Month must be a numeric value between 1 and 12.", "INVALID_MONTH"));

            if (contextyear == 0 || (contextyear < 1900 || contextyear > 9999))
                errors.Add(new BusinessValidationError($"The selected financial year is not valid. If the issue persists, contact support.", "INVALID_ContextYear"));

            if (string.IsNullOrEmpty(requestedBy))
                errors.Add(new BusinessValidationError($"Unable to identify the requester. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));


            var releasePeriods = await _releaseRepository.GetReleasePeriodsAsync();
            bool exists = releasePeriods.Any(p => p.FinalSummariesRun == -1 && p.EndPeriod >= month);

            if (exists)
                errors.Add(new BusinessValidationError($"You cannot rerun a period when a later period has been run.","INVALID_Rerun"));

            var canRun = await _repository.CanRunBatchJobAsync(RecreateSummariesJobName);
            if (!canRun)
            {
                errors.Add(new BusinessValidationError($"Job '{RecreateSummariesJobName}' is already running or initiated., Please try after sometime.", "INVALID_Rerun"));
            }

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _repository.EnqueueBatchJobAsync(RecreateSummariesJobName, requestedBy, correlationId, note);

            var eventDetail = BuildReCreateJobEvent(requestedBy, correlationId, month, contextyear);

            var eventId = await _eventPublisherService.PublishAsync(eventDetail, CancellationToken.None);

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);
            result.EventId = eventId; 
            return result;
        }

        private static EventDetail BuildReCreateJobEvent(string requestedBy, string correlationId, int month, int contextYear)
        {
            return new EventDetail
            {
                JobExecutionId = correlationId,
                JobName = RecreateSummariesJobName,
                RunMode = "Manual",
                RequestedBy = requestedBy,
                RequestedAtUtc = DateTime.UtcNow,
                ParametersJson = JsonSerializer.Serialize(new
                {
                    month = $"{contextYear:D4}-{month:D2}"
                })
            };
        }
    }
}
