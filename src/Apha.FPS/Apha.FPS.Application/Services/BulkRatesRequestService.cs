using System.Security.Cryptography;
using System.Text.Json;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.FPS.Application.Services
{
    public class BulkRatesRequestService : IBulkRatesRequestService
    {
        // Status names as stored in fps.job_status.statusname
        private const string StatusInitiated = "Initiated";
        private const string StatusReleasedForApproval = "ReleasedForApproval";
        private const string StatusRejected = "Rejected";
        private const string StatusApproved = "Approved";
        private const string StatusCancelled = "Cancelled";

        private readonly IBulkRatesRepository _repository;
        private readonly BulkRatesExcelParser _parser;
        private readonly BulkRatesValidator _validator;
        private readonly IEventBridgePublisher _eventBridgePublisher;
        private readonly IBulkRatesNotificationService _notificationService;
        private readonly ILogger<BulkRatesRequestService> _logger;

        public BulkRatesRequestService(
            IBulkRatesRepository repository,
            BulkRatesExcelParser parser,
            BulkRatesValidator validator,
            IEventBridgePublisher eventBridgePublisher,
            IBulkRatesNotificationService notificationService,
            ILogger<BulkRatesRequestService> logger)
        {
            _repository = repository;
            _parser = parser;
            _validator = validator;
            _eventBridgePublisher = eventBridgePublisher;
            _notificationService = notificationService;
            _logger = logger;
        }

        // ── US-API-01: Create request ────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> CreateRequestAsync(
            string jobName, int fpsYear, string requestedBy, CancellationToken ct = default)
        {
            var jobId = await _repository.GetJobIdByNameAsync(jobName, ct)
                ?? throw new BusinessValidationErrorException([
                    new($"Job name '{jobName}' is not a registered Bulk Rates job.", "INVALID_JOB_NAME")]);

            if (!await _repository.FpsYearExistsAsync(fpsYear, ct))
                throw new BusinessValidationErrorException([
                    new($"FPS year {fpsYear} does not exist.", "INVALID_FPS_YEAR")]);

            var initiatedStatusId = await _repository.GetStatusIdByNameAsync(jobId, StatusInitiated, ct)
                ?? throw new InvalidOperationException($"Status '{StatusInitiated}' not found for job '{jobName}'.");

            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var entry = await _repository.CreateRequestAsync(
                jobQueueId, jobExecutionId, jobId, initiatedStatusId,
                requestedBy, now, fpsYear, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Request created for FPS year {fpsYear} ({jobName}).",
                requestedBy, ct);

            _logger.LogInformation(
                "Bulk Rates request {JobQueueId} created by {RequestedBy} for {JobName} year {FpsYear}.",
                jobQueueId, requestedBy, jobName, fpsYear);

            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-02/03/05: Upload file ─────────────────────────────────────────

        public async Task<BulkRatesUploadResultDto> UploadFileAsync(
            Guid jobQueueId, byte[] fileBytes, string filename,
            string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobQueueId, ct);

            if (entry.RequestedBy != requestedBy)
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may upload files for this request.", "NOT_INITIATOR")]);

            // US-API-05: if Rejected, auto-transition back to Initiated before replacing staging
            if (string.Equals(entry.Status, StatusRejected, StringComparison.OrdinalIgnoreCase))
            {
                var initiatedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusInitiated, ct)
                    ?? throw new InvalidOperationException($"Status '{StatusInitiated}' not found for job {entry.JobId}.");
                var rejectedStatusId = entry.StatusId;

                await _repository.TransitionStatusAsync(jobQueueId, rejectedStatusId, initiatedStatusId, ct);
                await _repository.WriteJobQueueLogAsync(jobQueueId, "Request re-opened for correction via re-upload.", requestedBy, ct);
                entry.StatusId = initiatedStatusId;
                entry.Status = StatusInitiated;
            }

            if (!string.Equals(entry.Status, StatusInitiated, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"File upload is only permitted when the request is in '{StatusInitiated}' status. Current status: {entry.Status}.", "INVALID_STATUS_FOR_UPLOAD")]);

            // Parse the Excel file
            var parseResult = _parser.Parse(fileBytes, filename, entry.JobName, entry.JobQueueId);

            // Compute SHA-256 checksum
            var checksum = ComputeSha256(fileBytes);

            // Run validation (structural + reference) and classify rows
            var validationResult = await _validator.ValidateAsync(parseResult, entry.FpsYear, entry.JobName, ct);

            // Determine upload version
            var existing = DeserializeMetadata(entry.ConfigurationJson);
            var newVersion = (existing?.UploadVersion ?? 0) + 1;

            // Replace staging and validation errors
            await ReplaceStagingAsync(entry.JobName, jobQueueId, parseResult, ct);
            await _repository.ReplaceValidationErrorsAsync(jobQueueId, validationResult.Errors, ct);

            // Build and persist updated configuration_json
            var counts = validationResult.RowCounts;
            var metadata = new BulkRatesUploadMetadata
            {
                Filename = filename,
                ChecksumSha256 = checksum,
                UploadVersion = newVersion,
                ValidationCompletedAtUtc = DateTime.UtcNow,
                RowCounts = counts
            };
            await _repository.UpdateConfigurationJsonAsync(
                jobQueueId, JsonSerializer.Serialize(metadata, JsonOptions), ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"File uploaded (v{newVersion}): {filename}. Rows: {counts.Total} total, {counts.Invalid} invalid, {counts.Insert} insert, {counts.Update} update, {counts.Unchanged} unchanged.",
                requestedBy, ct);

            _logger.LogInformation(
                "Upload v{Version} for request {JobQueueId}: {Total} rows, {Invalid} invalid.",
                newVersion, jobQueueId, counts.Total, counts.Invalid);

            return new BulkRatesUploadResultDto
            {
                JobQueueId = jobQueueId,
                Status = StatusInitiated,
                UploadVersion = newVersion,
                Filename = filename,
                RowCounts = counts,
                ValidationErrors = validationResult.Errors
            };
        }

        // ── US-API-04: Get validation results ────────────────────────────────────

        public async Task<BulkRatesUploadResultDto> GetValidationResultsAsync(
            Guid jobQueueId, string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobQueueId, ct);

            if (entry.RequestedBy != requestedBy)
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may view validation results.", "NOT_INITIATOR")]);

            var errors = await _repository.GetValidationErrorsAsync(jobQueueId, ct);
            var metadata = DeserializeMetadata(entry.ConfigurationJson);

            return new BulkRatesUploadResultDto
            {
                JobQueueId = jobQueueId,
                Status = entry.Status,
                UploadVersion = metadata?.UploadVersion ?? 0,
                Filename = metadata?.Filename,
                RowCounts = metadata?.RowCounts ?? new(),
                ValidationErrors = errors
            };
        }

        // ── US-API-06/12/13: Release for approval ────────────────────────────────

        public async Task<BulkRatesRequestDto> ReleaseForApprovalAsync(
            Guid jobQueueId, string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobQueueId, ct);

            if (entry.RequestedBy != requestedBy)
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may release this request.", "NOT_INITIATOR")]);

            RequireStatus(entry, StatusInitiated, "release for approval");

            // US-API-12: Verify upload metadata (checksum) exists
            var metadata = DeserializeMetadata(entry.ConfigurationJson);
            if (metadata?.ChecksumSha256 == null)
                throw new BusinessValidationErrorException([
                    new("No file has been uploaded for this request. Upload a valid file before releasing.", "NO_UPLOAD")]);

            // US-API-06: All blocking errors must be resolved
            var errors = await _repository.GetValidationErrorsAsync(jobQueueId, ct);
            var blockingCount = errors.Count(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase));
            if (blockingCount > 0)
                throw new BusinessValidationErrorException([
                    new($"Cannot release: {blockingCount} blocking validation error(s) must be corrected first.", "BLOCKING_ERRORS")]);

            var initiatedStatusId = entry.StatusId;
            var releasedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusReleasedForApproval, ct)
                ?? throw new InvalidOperationException($"Status '{StatusReleasedForApproval}' not found.");

            await _repository.TransitionStatusAsync(jobQueueId, initiatedStatusId, releasedStatusId, ct);
            await _repository.WriteJobQueueLogAsync(jobQueueId, "Request released for approval.", requestedBy, ct);

            await _notificationService.NotifyAsync(
                BulkRatesNotificationEvent.ReleasedForApproval,
                new BulkRatesNotificationContext
                {
                    JobQueueId = jobQueueId,
                    JobName = entry.JobName,
                    FpsYear = entry.FpsYear,
                    RequestedBy = entry.RequestedBy,
                    RowCounts = metadata.RowCounts
                }, ct);

            _logger.LogInformation("Request {JobQueueId} released for approval by {RequestedBy}.", jobQueueId, requestedBy);

            entry.StatusId = releasedStatusId;
            entry.Status = StatusReleasedForApproval;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-07/09/10/12/13: Approve ──────────────────────────────────────

        public async Task<BulkRatesRequestDto> ApproveAsync(
            Guid jobQueueId, string approvedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobQueueId, ct);

            RequireStatus(entry, StatusReleasedForApproval, "approve");

            // US-API-09: Maker-checker — approver must differ from initiator
            if (string.Equals(entry.RequestedBy, approvedBy, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new("The approver cannot be the same person as the initiator (maker-checker rule).", "MAKER_CHECKER_VIOLATION")]);

            // US-API-12: Verify checksum is stored (immutability of frozen upload)
            var metadata = DeserializeMetadata(entry.ConfigurationJson);
            if (metadata?.ChecksumSha256 == null)
                throw new BusinessValidationErrorException([
                    new("Upload metadata is missing. The request cannot be approved.", "MISSING_CHECKSUM")]);

            var releasedStatusId = entry.StatusId;
            var approvedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusApproved, ct)
                ?? throw new InvalidOperationException($"Status '{StatusApproved}' not found.");

            var now = DateTime.UtcNow;

            await _repository.SetApprovalAsync(
                jobQueueId, entry.JobExecutionId,
                approvedBy, now,
                approvedBy, now,
                approvedStatusId, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId, $"Request approved. EventBridge trigger published.", approvedBy, ct);

            // US-API-10: Publish EventBridge trigger
            var payload = new BulkRatesEventPayload
            {
                JobExecutionId = entry.JobExecutionId,
                JobName = entry.JobName,
                RunMode = "Manual",
                RequestedBy = entry.RequestedBy,
                RequestedAtUtc = entry.RequestedAtUtc,
                ParametersJson = new BulkRatesEventParameters { Year = entry.FpsYear }
            };

            await _eventBridgePublisher.PublishApprovalEventAsync(payload, ct);

            _logger.LogInformation(
                "Request {JobQueueId} approved by {ApprovedBy}. EventBridge event published for execution {JobExecutionId}.",
                jobQueueId, approvedBy, entry.JobExecutionId);

            entry.ApprovedBy = approvedBy;
            entry.ApprovedAtUtc = now;
            entry.StatusId = approvedStatusId;
            entry.Status = StatusApproved;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-08/13: Reject ─────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> RejectAsync(
            Guid jobQueueId, string rejectedBy, string reason, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessValidationErrorException([
                    new("Rejection reason is mandatory.", "REASON_REQUIRED")]);

            var entry = await RequireRequestAsync(jobQueueId, ct);

            RequireStatus(entry, StatusReleasedForApproval, "reject");

            // Approver must differ from initiator (same maker-checker rule applies for rejection)
            if (string.Equals(entry.RequestedBy, rejectedBy, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new("The rejector cannot be the same person as the initiator (maker-checker rule).", "MAKER_CHECKER_VIOLATION")]);

            var releasedStatusId = entry.StatusId;
            var rejectedStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusRejected, ct)
                ?? throw new InvalidOperationException($"Status '{StatusRejected}' not found.");

            var now = DateTime.UtcNow;
            await _repository.SetRejectionAsync(
                jobQueueId, rejectedBy, now, reason, rejectedStatusId, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId, $"Request rejected. Reason: {reason}", rejectedBy, ct);

            await _notificationService.NotifyAsync(
                BulkRatesNotificationEvent.Rejected,
                new BulkRatesNotificationContext
                {
                    JobQueueId = jobQueueId,
                    JobName = entry.JobName,
                    FpsYear = entry.FpsYear,
                    RequestedBy = entry.RequestedBy,
                    Reason = reason
                }, ct);

            _logger.LogInformation("Request {JobQueueId} rejected by {RejectedBy}.", jobQueueId, rejectedBy);

            entry.RejectedBy = rejectedBy;
            entry.RejectionReason = reason;
            entry.StatusId = rejectedStatusId;
            entry.Status = StatusRejected;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-14/13: Cancel ─────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> CancelAsync(
            Guid jobQueueId, string cancelledBy, string? reason, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobQueueId, ct);

            // Only original initiator may cancel
            if (!string.Equals(entry.RequestedBy, cancelledBy, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may cancel this request.", "NOT_INITIATOR")]);

            if (!string.Equals(entry.Status, StatusInitiated, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Status, StatusRejected, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"Cancellation is only permitted for '{StatusInitiated}' or '{StatusRejected}' requests. Current status: {entry.Status}.", "INVALID_STATUS_FOR_CANCEL")]);

            var cancelledStatusId = await _repository.GetStatusIdByNameAsync(entry.JobId, StatusCancelled, ct)
                ?? throw new InvalidOperationException($"Status '{StatusCancelled}' not found.");

            var now = DateTime.UtcNow;

            // Atomically: set Cancelled + delete all staging rows for this request
            await _repository.CancelAndClearStagingAsync(
                jobQueueId, entry.JobName,
                cancelledBy, now, reason, cancelledStatusId, ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                string.IsNullOrWhiteSpace(reason)
                    ? "Request cancelled by initiator."
                    : $"Request cancelled by initiator. Reason: {reason}",
                cancelledBy, ct);

            await _notificationService.NotifyAsync(
                BulkRatesNotificationEvent.Cancelled,
                new BulkRatesNotificationContext
                {
                    JobQueueId = jobQueueId,
                    JobName = entry.JobName,
                    FpsYear = entry.FpsYear,
                    RequestedBy = entry.RequestedBy,
                    Reason = reason
                }, ct);

            _logger.LogInformation("Request {JobQueueId} cancelled by {CancelledBy}.", jobQueueId, cancelledBy);

            entry.CancelledBy = cancelledBy;
            entry.CancelledAtUtc = now;
            entry.CancellationReason = reason;
            entry.StatusId = cancelledStatusId;
            entry.Status = StatusCancelled;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-11: Query ─────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto?> GetRequestAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var entry = await _repository.GetRequestAsync(jobQueueId, ct);
            if (entry == null) return null;
            return await BuildRequestDtoAsync(entry, ct);
        }

        public async Task<IReadOnlyList<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status, CancellationToken ct = default)
        {
            return await _repository.GetRequestsAsync(jobName, fpsYear, status, ct);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private async Task<BulkRatesQueueEntry> RequireRequestAsync(Guid jobQueueId, CancellationToken ct)
        {
            var entry = await _repository.GetRequestAsync(jobQueueId, ct);
            if (entry == null)
                throw new BusinessValidationErrorException([
                    new($"Bulk Rates request {jobQueueId} not found.", "NOT_FOUND")]);
            return entry;
        }

        private static void RequireStatus(BulkRatesQueueEntry entry, string expectedStatus, string action)
        {
            if (!string.Equals(entry.Status, expectedStatus, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"Cannot {action}: request must be in '{expectedStatus}' status. Current status: {entry.Status}.", "INVALID_STATUS_TRANSITION")]);
        }

        private async Task<BulkRatesRequestDto> BuildRequestDtoAsync(BulkRatesQueueEntry entry, CancellationToken ct)
        {
            var logs = await _repository.GetJobQueueLogsAsync(entry.JobQueueId, ct);
            var errors = await _repository.GetValidationErrorsAsync(entry.JobQueueId, ct);
            var metadata = DeserializeMetadata(entry.ConfigurationJson);

            return new BulkRatesRequestDto
            {
                Entry = entry,
                UploadMetadata = metadata,
                Log = logs,
                ErrorCount = errors.Count(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase)),
                WarningCount = errors.Count(e => string.Equals(e.Severity, "Warning", StringComparison.OrdinalIgnoreCase))
            };
        }

        private async Task ReplaceStagingAsync(
            string jobName, Guid jobQueueId,
            BulkRatesParseResult parseResult,
            CancellationToken ct)
        {
            if (jobName == "BulkTestRatesUpdate")
                await _repository.ReplaceStagingFecAsync(jobQueueId, parseResult.FecRows, parseResult.AgrupRows, ct);
            else if (jobName == "BulkStaffRatesUpdate")
                await _repository.ReplaceStagingStaffAsync(jobQueueId, parseResult.StaffRows, ct);
            else if (jobName == "BulkAnimalRatesUpdate")
                await _repository.ReplaceStagingAnimalAsync(jobQueueId, parseResult.AnimalRows, ct);
        }

        private static BulkRatesUploadMetadata? DeserializeMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<BulkRatesUploadMetadata>(json, JsonOptions); }
            catch { return null; }
        }

        private static string ComputeSha256(byte[] data)
        {
            var hash = SHA256.HashData(data);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }
}
