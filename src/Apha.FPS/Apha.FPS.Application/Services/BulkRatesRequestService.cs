using System.Security.Cryptography;
using System.Text.Json;
using Apha.Common.Constants;
using Apha.Common.Utilities.ExcelExport;
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
        private const string StatusFailed = "Failed";

        private readonly IBulkRatesRepository _repository;
        private readonly BulkRatesExcelParser _parser;
        private readonly BulkRatesValidator _validator;
        private readonly IEventBridgePublisher _eventBridgePublisher;
        private readonly IBulkRatesNotificationService _notificationService;
        private readonly IExcelExportService _excelExportService;
        private readonly ILogger<BulkRatesRequestService> _logger;

        public BulkRatesRequestService(
            IBulkRatesRepository repository,
            BulkRatesExcelParser parser,
            BulkRatesValidator validator,
            IEventBridgePublisher eventBridgePublisher,
            IBulkRatesNotificationService notificationService,
            IExcelExportService excelExportService,
            ILogger<BulkRatesRequestService> logger)
        {
            _repository = repository;
            _parser = parser;
            _validator = validator;
            _eventBridgePublisher = eventBridgePublisher;
            _notificationService = notificationService;
            _excelExportService = excelExportService;
            _logger = logger;
        }

        // ── US-API-01: Create request ────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> CreateRequestAsync(
            string jobName, int fpsYear, string requestedBy, CancellationToken ct = default)
        {
            var jobId = await _repository.GetJobIdByNameAsync(jobName, ct)
                ?? throw new BusinessValidationErrorException([
                    new($"Job name '{jobName}' is not a registered Bulk Rates job.", "INVALID_JOB_NAME")]);

            var active = await _repository.GetActiveRequestAsync(jobName, ct);
            if (active != null)
                throw new BusinessValidationErrorException([
                    new($"An active {jobName} request already exists (JobExecutionId={active.JobExecutionId}, " +
                        $"Status={active.Status}, Requested by {active.RequestedBy}). Complete, reject, " +
                        "or cancel it before creating a new one.", "ACTIVE_REQUEST_EXISTS")]);

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
                "[BulkRates.RequestCreated] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor}",
                jobQueueId, jobName, fpsYear, requestedBy);

            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-02/03/05: Upload file ─────────────────────────────────────────

        public async Task<BulkRatesUploadResultDto> UploadFileAsync(
            Guid jobExecutionId, byte[] fileBytes, string filename,
            string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            if (!string.Equals(entry.RequestedBy, requestedBy, StringComparison.OrdinalIgnoreCase))
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
                _logger.LogInformation(
                    "[BulkRates.RequestReopened] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                    jobQueueId, entry.JobName, entry.FpsYear, requestedBy, StatusRejected, StatusInitiated);
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
            var newVersion = (entry.UploadVersion ?? 0) + 1;

            // Staging always holds every parsed row — valid and invalid alike — so the user can
            // review and correct invalid rows in place; only Release is gated on zero blocking
            // errors (see ReleaseForApprovalAsync). Validation errors are stored per staged row.
            await ReplaceStagingAsync(entry.JobName, jobQueueId, parseResult, ct);
            await _repository.ReplaceValidationErrorsAsync(jobQueueId, validationResult.Errors, ct);

            // Persist upload metadata as typed columns (CR051 — configuration_json retired)
            var counts = validationResult.RowCounts;
            await _repository.UpdateUploadMetadataAsync(
                jobQueueId, filename, checksum, newVersion, DateTime.UtcNow,
                JsonSerializer.Serialize(counts, JsonOptions), ct);

            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"File uploaded (v{newVersion}): {filename}. Rows: {counts.Total} total, {counts.Invalid} invalid, {counts.Insert} insert, {counts.Update} update, {counts.Unchanged} unchanged.",
                requestedBy, ct);

            _logger.LogInformation(
                "[BulkRates.FileUploaded] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | UploadVersion={UploadVersion} | TotalRows={TotalRows} | InvalidRows={InvalidRows}",
                jobQueueId, entry.JobName, entry.FpsYear, requestedBy, newVersion, counts.Total, counts.Invalid);

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
            Guid jobExecutionId, string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            if (!string.Equals(entry.RequestedBy, requestedBy, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may view validation results.", "NOT_INITIATOR")]);

            var errors = await _repository.GetValidationErrorsAsync(jobQueueId, ct);
            var metadata = BuildUploadMetadata(entry);

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
            Guid jobExecutionId, string requestedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            if (!string.Equals(entry.RequestedBy, requestedBy, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may release this request.", "NOT_INITIATOR")]);

            RequireStatus(entry, StatusInitiated, "release for approval");

            // US-API-12: Verify upload metadata (checksum) exists
            if (entry.UploadChecksumSha256 == null)
                throw new BusinessValidationErrorException([
                    new("No file has been uploaded for this request. Upload a valid file before releasing.", "NO_UPLOAD")]);
            // BuildUploadMetadata only returns null when both UploadChecksumSha256 and
            // UploadFilename are null — already ruled out by the check above.
            var metadata = BuildUploadMetadata(entry)
                ?? throw new InvalidOperationException("Upload metadata missing despite checksum present.");

            // A checksum can exist for a file that parsed to zero data rows — releasing that
            // would approve/run a no-op bulk update, which makes no business sense.
            if (metadata.RowCounts.Total == 0)
                throw new BusinessValidationErrorException([
                    new("The uploaded file contains no data rows. Upload a file with at least one row before releasing.", "NO_ROWS")]);

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

            _logger.LogInformation(
                "[BulkRates.ReleasedForApproval] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, requestedBy, StatusInitiated, StatusReleasedForApproval);

            entry.StatusId = releasedStatusId;
            entry.Status = StatusReleasedForApproval;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-07/09/10/12/13: Approve ──────────────────────────────────────

        public async Task<BulkRatesRequestDto> ApproveAsync(
            Guid jobExecutionId, string approvedBy, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            RequireStatus(entry, StatusReleasedForApproval, "approve");

            // US-API-09: Maker-checker — approver must differ from initiator.
            // TEMPORARILY DISABLED at the requester's request so a single admin can
            // self-approve during testing. Restore this check before release.

            // US-API-12: Verify checksum is stored (immutability of frozen upload)
            if (entry.UploadChecksumSha256 == null)
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
                "[BulkRates.Approved] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | JobExecutionId={JobExecutionId} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, approvedBy, entry.JobExecutionId, StatusReleasedForApproval, StatusApproved);

            entry.ApprovedBy = approvedBy;
            entry.ApprovedAtUtc = now;
            entry.StatusId = approvedStatusId;
            entry.Status = StatusApproved;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-08/13: Reject ─────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> RejectAsync(
            Guid jobExecutionId, string rejectedBy, string reason, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessValidationErrorException([
                    new("Rejection reason is mandatory.", "REASON_REQUIRED")]);

            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

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

            _logger.LogInformation(
                "[BulkRates.Rejected] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, rejectedBy, StatusReleasedForApproval, StatusRejected);

            entry.RejectedBy = rejectedBy;
            entry.RejectionReason = reason;
            entry.StatusId = rejectedStatusId;
            entry.Status = StatusRejected;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-14/13: Cancel ─────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto> CancelAsync(
            Guid jobExecutionId, string cancelledBy, string? reason, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);
            var jobQueueId = entry.JobQueueId;

            // Only original initiator may cancel
            if (!string.Equals(entry.RequestedBy, cancelledBy, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new("Only the original initiator may cancel this request.", "NOT_INITIATOR")]);

            if (!string.Equals(entry.Status, StatusInitiated, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Status, StatusRejected, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Status, StatusFailed, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationErrorException([
                    new($"Cancellation is only permitted for '{StatusInitiated}', '{StatusRejected}', or '{StatusFailed}' requests. Current status: {entry.Status}.", "INVALID_STATUS_FOR_CANCEL")]);

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

            _logger.LogInformation(
                "[BulkRates.Cancelled] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear} | Actor={Actor} | FromStatus={FromStatus} | ToStatus={ToStatus}",
                jobQueueId, entry.JobName, entry.FpsYear, cancelledBy, entry.Status, StatusCancelled);

            entry.CancelledBy = cancelledBy;
            entry.CancelledAtUtc = now;
            entry.CancellationReason = reason;
            entry.StatusId = cancelledStatusId;
            entry.Status = StatusCancelled;
            return await BuildRequestDtoAsync(entry, ct);
        }

        // ── US-API-11: Query ─────────────────────────────────────────────────────

        public async Task<BulkRatesRequestDto?> GetRequestAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await _repository.GetRequestAsync(jobExecutionId, ct);
            if (entry == null) return null;
            return await BuildRequestDtoAsync(entry, ct);
        }

        public async Task<IReadOnlyList<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status, CancellationToken ct = default)
        {
            return await _repository.GetRequestsAsync(jobName, fpsYear, status, ct);
        }

        public async Task<BulkRatesQueueEntry?> GetActiveRequestAsync(string jobName, CancellationToken ct = default)
        {
            return await _repository.GetActiveRequestAsync(jobName, ct);
        }

        // ── Export ───────────────────────────────────────────────────────────────

        public async Task<byte[]> ExportFecTestDataAsync(int fpsYear, CancellationToken ct = default)
        {
            var fecRows = await _repository.GetFecRowsForExportAsync(fpsYear, ct);
            var agrupRows = await _repository.GetAgrupRowsForExportAsync(fpsYear, ct);

            _logger.LogInformation(
                "[BulkRates.ExportFecTestData] FpsYear={FpsYear} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                fpsYear, fecRows.Count, agrupRows.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildFecAgrupSheets(fecRows, agrupRows));
        }

        public async Task<byte[]> ExportStaffTestDataAsync(int fpsYear, CancellationToken ct = default)
        {
            var staffRows = await _repository.GetStaffRowsForExportAsync(fpsYear, ct);

            _logger.LogInformation(
                "[BulkRates.ExportStaffTestData] FpsYear={FpsYear} | StaffRows={StaffRows}",
                fpsYear, staffRows.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildStaffSheet(staffRows));
        }

        public async Task<byte[]> ExportAnimalTestDataAsync(int fpsYear, CancellationToken ct = default)
        {
            var animalRows = await _repository.GetAnimalRowsForExportAsync(fpsYear, ct);

            _logger.LogInformation(
                "[BulkRates.ExportAnimalTestData] FpsYear={FpsYear} | AnimalRows={AnimalRows}",
                fpsYear, animalRows.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildAnimalSheet(animalRows));
        }

        // ── Staging grid (Detail page — "FEC Data (Staging)" / "Agrup Details") ────

        public async Task<BulkRatesStagingDataDto> GetStagingDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);

            // No file uploaded yet — there is nothing staged to diff against live data, so every
            // live row would otherwise look "Deleted"/"Not Found". Return no rows until an upload exists.
            if (entry.UploadChecksumSha256 == null)
                return new BulkRatesStagingDataDto();

            if (string.Equals(entry.JobName, BulkRatesJobNames.Staff, StringComparison.OrdinalIgnoreCase))
                return await GetStaffStagingDataAsync(entry, ct);

            if (string.Equals(entry.JobName, BulkRatesJobNames.Animal, StringComparison.OrdinalIgnoreCase))
                return await GetAnimalStagingDataAsync(entry, ct);

            if (!string.Equals(entry.JobName, BulkRatesJobNames.Fec, StringComparison.OrdinalIgnoreCase))
                return new BulkRatesStagingDataDto();

            var stagedFec = await _repository.GetFecStagingRowsAsync(entry.JobQueueId, ct);
            var stagedAgrup = await _repository.GetAgrupStagingRowsAsync(entry.JobQueueId, ct);
            var liveFec = await _repository.GetFecRowsForExportAsync(entry.FpsYear, ct);

            var liveByTestCode = liveFec.ToDictionary(r => r.TestCode, StringComparer.OrdinalIgnoreCase);
            var stagedTestCodes = stagedFec.Select(r => r.TestCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var fecRows = new List<BulkRatesStagingFecRowDto>();
            foreach (var row in stagedFec)
            {
                var isNew = !liveByTestCode.TryGetValue(row.TestCode, out var live);
                if (!isNew && row.FecNewRate == live!.DefraUnitPrice)
                    continue; // Unchanged — spec says only New/Updated/Deleted are shown

                fecRows.Add(new BulkRatesStagingFecRowDto
                {
                    Status = isNew ? "Inserted" : "Updated",
                    TestCode = row.TestCode,
                    UnitPriceVla = row.UnitPriceVla,
                    DefraUnitPrice = row.DefraUnitPrice,
                    FecNewRate = row.FecNewRate,
                    ItemDescription = row.ItemDescription,
                    ShortDescription = row.ShortDescription,
                    Owner = row.Owner,
                    Comments = row.Comments
                });
            }

            foreach (var live in liveFec)
            {
                if (stagedTestCodes.Contains(live.TestCode))
                    continue;

                fecRows.Add(new BulkRatesStagingFecRowDto
                {
                    Status = "Deleted",
                    TestCode = live.TestCode,
                    UnitPriceVla = live.UnitPriceVla,
                    DefraUnitPrice = live.DefraUnitPrice,
                    ItemDescription = live.ItemDescription,
                    ShortDescription = live.ShortDescription,
                    Owner = live.Owner,
                    Comments = live.Comments
                });
            }

            var liveAgrup = await _repository.GetAgrupRowsForExportAsync(entry.FpsYear, ct);
            var liveAgrupByKey = liveAgrup.ToDictionary(r => AgrupKey(r.TestCode, r.Buyer));
            var stagedAgrupKeys = stagedAgrup.Select(r => AgrupKey(r.TestCode, r.Buyer)).ToHashSet();

            var agrupRows = new List<BulkRatesStagingAgrupRowDto>();
            foreach (var row in stagedAgrup)
            {
                var isNew = !liveAgrupByKey.TryGetValue(AgrupKey(row.TestCode, row.Buyer), out var live);
                if (!isNew && row.AgrupNew == live!.Agrup)
                    continue; // Unchanged — same rule as FEC: only New/Updated/Deleted are shown

                agrupRows.Add(new BulkRatesStagingAgrupRowDto
                {
                    Status = isNew ? "Inserted" : "Updated",
                    TestCode = row.TestCode,
                    Buyer = row.Buyer,
                    Agrup = row.Agrup,
                    AgrupNew = row.AgrupNew,
                    NoRequired = row.NoRequired,
                    DateCreated = row.DateCreated,
                    Active = row.Active,
                    Comments = row.Comments
                });
            }

            foreach (var live in liveAgrup)
            {
                if (stagedAgrupKeys.Contains(AgrupKey(live.TestCode, live.Buyer)))
                    continue;

                agrupRows.Add(new BulkRatesStagingAgrupRowDto
                {
                    Status = "Deleted",
                    TestCode = live.TestCode,
                    Buyer = live.Buyer,
                    Agrup = live.Agrup,
                    NoRequired = live.NoRequired,
                    DateCreated = live.DateCreated,
                    Active = live.Active,
                    Comments = live.Comments
                });
            }

            return new BulkRatesStagingDataDto { FecRows = fecRows, AgrupRows = agrupRows };
        }

        private static (string TestCode, string Buyer) AgrupKey(string testCode, string buyer) =>
            (testCode.ToUpperInvariant(), buyer.ToUpperInvariant());

        // Staff/Animal are update-only (see BulkStaffRatesService/BulkAnimalRatesService in the
        // worker): a staged row whose key doesn't exist live is skipped, never inserted, and a
        // live row absent from the upload is left untouched, never deleted. So unlike FEC/AGRUP,
        // there is no "Inserted"/"Deleted" status here — only "Updated" (will be applied) and
        // "Not Found" (won't be — surfaced so the initiator can catch a typo'd grade/animal type
        // before release, matching exactly what the worker will do).

        private async Task<BulkRatesStagingDataDto> GetStaffStagingDataAsync(BulkRatesQueueEntry entry, CancellationToken ct)
        {
            var stagedStaff = await _repository.GetStaffStagingRowsAsync(entry.JobQueueId, ct);
            var liveStaff = await _repository.GetStaffRowsForExportAsync(entry.FpsYear, ct);
            var liveByGrade = liveStaff.ToDictionary(r => r.PcGrade, StringComparer.OrdinalIgnoreCase);

            var rows = new List<BulkRatesStagingStaffRowDto>();
            foreach (var row in stagedStaff)
            {
                if (!liveByGrade.TryGetValue(row.PcGrade, out var live))
                {
                    rows.Add(new BulkRatesStagingStaffRowDto
                    {
                        Status = "Not Found",
                        PcGrade = row.PcGrade,
                        PayRateNew = row.PayRate,
                        NprNew = row.Npr,
                        OhrNew = row.Ohr
                    });
                    continue;
                }

                var payRateChanged = row.PayRate.HasValue && row.PayRate.Value != live.PayRate;
                var nprChanged = row.Npr.HasValue && row.Npr.Value != live.Npr;
                var ohrChanged = row.Ohr.HasValue && row.Ohr.Value != live.Ohr;
                if (!payRateChanged && !nprChanged && !ohrChanged)
                    continue; // Unchanged — the worker will skip this row too

                rows.Add(new BulkRatesStagingStaffRowDto
                {
                    Status = "Updated",
                    PcGrade = row.PcGrade,
                    PayRate = live.PayRate,
                    PayRateNew = row.PayRate,
                    Npr = live.Npr,
                    NprNew = row.Npr,
                    Ohr = live.Ohr,
                    OhrNew = row.Ohr
                });
            }

            return new BulkRatesStagingDataDto { StaffRows = rows };
        }

        private async Task<BulkRatesStagingDataDto> GetAnimalStagingDataAsync(BulkRatesQueueEntry entry, CancellationToken ct)
        {
            var stagedAnimal = await _repository.GetAnimalStagingRowsAsync(entry.JobQueueId, ct);
            var liveAnimal = await _repository.GetAnimalRowsForExportAsync(entry.FpsYear, ct);
            var liveByType = liveAnimal.ToDictionary(r => r.AnimalType, StringComparer.OrdinalIgnoreCase);

            var rows = new List<BulkRatesStagingAnimalRowDto>();
            foreach (var row in stagedAnimal)
            {
                if (!liveByType.TryGetValue(row.AnimalType, out var live))
                {
                    rows.Add(new BulkRatesStagingAnimalRowDto
                    {
                        Status = "Not Found",
                        AnimalType = row.AnimalType,
                        Species = row.Species,
                        SecurityLevel = row.SecurityLevel,
                        DailyRateNew = row.DailyRate,
                        DefraDailyRateNew = row.DefraDailyRate,
                        PlanByWeek = row.PlanByWeek
                    });
                    continue;
                }

                var dailyRateChanged = row.DailyRate.HasValue && row.DailyRate.Value != live.DailyRate;
                var defraDailyRateChanged = row.DefraDailyRate.HasValue && row.DefraDailyRate.Value != live.DefraDailyRate;
                var planByWeekChanged = row.PlanByWeek.HasValue && row.PlanByWeek.Value != live.PlanByWeek;
                var speciesChanged = row.Species is not null && row.Species != live.Species;
                var securityLevelChanged = row.SecurityLevel is not null && row.SecurityLevel != live.SecurityLevel;
                if (!dailyRateChanged && !defraDailyRateChanged && !planByWeekChanged && !speciesChanged && !securityLevelChanged)
                    continue; // Unchanged — the worker will skip this row too

                rows.Add(new BulkRatesStagingAnimalRowDto
                {
                    Status = "Updated",
                    AnimalType = row.AnimalType,
                    Species = row.Species ?? live.Species,
                    SecurityLevel = row.SecurityLevel ?? live.SecurityLevel,
                    DailyRate = live.DailyRate,
                    DailyRateNew = row.DailyRate,
                    DefraDailyRate = live.DefraDailyRate,
                    DefraDailyRateNew = row.DefraDailyRate,
                    PlanByWeek = row.PlanByWeek ?? live.PlanByWeek
                });
            }

            return new BulkRatesStagingDataDto { AnimalRows = rows };
        }

        public async Task<byte[]> ExportStagingDataAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var entry = await RequireRequestAsync(jobExecutionId, ct);

            var stagedFec = await _repository.GetFecStagingRowsAsync(entry.JobQueueId, ct);
            var stagedAgrup = await _repository.GetAgrupStagingRowsAsync(entry.JobQueueId, ct);

            _logger.LogInformation(
                "[BulkRates.ExportStagingData] JobExecutionId={JobExecutionId} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobExecutionId, stagedFec.Count, stagedAgrup.Count);

            return _excelExportService.ExportToExcelMultiSheet(BuildFecAgrupSheets(stagedFec, stagedAgrup));
        }

        private static List<ExcelSheetDefinition> BuildFecAgrupSheets(
            IReadOnlyList<FecStagingRow> fecRows, IReadOnlyList<AgrupStagingRow> agrupRows)
        {
            var fecExportRows = fecRows.Select(r => new FecExportRow
            {
                TestCode         = r.TestCode,
                UnitPriceVla     = r.UnitPriceVla,
                DefraUnitPrice   = r.DefraUnitPrice,
                FecNew           = r.FecNewRate,
                Change           = r.Change,
                ItemDescription  = r.ItemDescription,
                ShortDescription = r.ShortDescription,
                Owner            = r.Owner,
                Comments         = r.Comments
            }).ToList();

            var agrupExportRows = agrupRows.Select(r => new AgrupExportRow
            {
                TestCode    = r.TestCode,
                Buyer       = r.Buyer,
                Agrup       = r.Agrup,
                AgrupNew    = r.AgrupNew,
                Change      = r.Change,
                NoRequired  = r.NoRequired,
                DateCreated = r.DateCreated,
                Active      = r.Active,
                Comments    = r.Comments
            }).ToList();

            return
            [
                new() { SheetName = "FEC",   Data = fecExportRows.Cast<object>(),   DataType = typeof(FecExportRow) },
                new() { SheetName = "AGRUP", Data = agrupExportRows.Cast<object>(), DataType = typeof(AgrupExportRow) }
            ];
        }

        // Sheet name matches BulkRatesExcelParser's StaffSheet ("Staff") so a downloaded
        // template re-uploads without modification.
        private static List<ExcelSheetDefinition> BuildStaffSheet(IReadOnlyList<StaffStagingRow> rows)
        {
            var exportRows = rows.Select(r => new StaffExportRow
            {
                PcGrade = r.PcGrade,
                PayRate = r.PayRate,
                Npr     = r.Npr,
                Ohr     = r.Ohr
            }).ToList();

            return [new() { SheetName = "Staff", Data = exportRows.Cast<object>(), DataType = typeof(StaffExportRow) }];
        }

        // Sheet name matches BulkRatesExcelParser's AnimalSheet ("Animals") so a downloaded
        // template re-uploads without modification.
        private static List<ExcelSheetDefinition> BuildAnimalSheet(IReadOnlyList<AnimalStagingRow> rows)
        {
            var exportRows = rows.Select(r => new AnimalExportRow
            {
                AnimalType     = r.AnimalType,
                Species        = r.Species,
                SecurityLevel  = r.SecurityLevel,
                DailyRate      = r.DailyRate,
                DefraDailyRate = r.DefraDailyRate,
                PlanByWeek     = r.PlanByWeek
            }).ToList();

            return [new() { SheetName = "Animals", Data = exportRows.Cast<object>(), DataType = typeof(AnimalExportRow) }];
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private async Task<BulkRatesQueueEntry> RequireRequestAsync(Guid jobExecutionId, CancellationToken ct)
        {
            var entry = await _repository.GetRequestAsync(jobExecutionId, ct);
            if (entry == null)
                throw new BusinessValidationErrorException([
                    new($"Bulk Rates request with JobExecutionId {jobExecutionId} not found.", "NOT_FOUND")]);
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
            var metadata = BuildUploadMetadata(entry);

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
            if (jobName == BulkRatesJobNames.Fec)
                await _repository.ReplaceStagingFecAsync(jobQueueId, parseResult.FecRows, parseResult.AgrupRows, ct);
            else if (jobName == BulkRatesJobNames.Staff)
                await _repository.ReplaceStagingStaffAsync(jobQueueId, parseResult.StaffRows, ct);
            else if (jobName == BulkRatesJobNames.Animal)
                await _repository.ReplaceStagingAnimalAsync(jobQueueId, parseResult.AnimalRows, ct);
        }

        private static BulkRatesUploadMetadata? BuildUploadMetadata(BulkRatesQueueEntry entry)
        {
            if (entry.UploadChecksumSha256 == null && entry.UploadFilename == null)
                return null;

            return new BulkRatesUploadMetadata
            {
                Filename = entry.UploadFilename,
                ChecksumSha256 = entry.UploadChecksumSha256,
                UploadVersion = entry.UploadVersion ?? 0,
                ValidationCompletedAtUtc = entry.UploadValidatedAtUtc,
                RowCounts = DeserializeRowCounts(entry.UploadRowCountsJson)
            };
        }

        private static BulkRatesRowCounts DeserializeRowCounts(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try { return JsonSerializer.Deserialize<BulkRatesRowCounts>(json, JsonOptions) ?? new(); }
            catch { return new(); }
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
