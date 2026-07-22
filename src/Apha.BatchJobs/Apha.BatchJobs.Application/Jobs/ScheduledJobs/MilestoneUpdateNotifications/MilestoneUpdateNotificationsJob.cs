using System.Text.Json;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications;

/// <summary>
/// MilestoneUpdateNotifications scheduled batch job. Sends monthly milestone update emails to
/// project managers, sourced from the mabarchive legacy-parity views (plan section 7). Runs on
/// a monthly schedule; manual reruns are supported with an optional month override (plan
/// section 6.2).
///
/// Lock lifecycle is owned exclusively by JobOrchestrator. This job must not acquire or release
/// the distributed lock. It reads its resolved identity and parameters from
/// <see cref="ICurrentJobExecutionContext"/> — populated once by JobOrchestrator before this
/// job runs — rather than re-parsing environment variables or looking up its own jobQueueId.
///
/// No transaction wraps the whole run: email sending cannot participate in one, and durability
/// instead comes from per-recipient audit rows in fps.notification_delivery, written
/// Pending -&gt; Sending -&gt; Sent/Failed around each send. A single recipient failure is recorded
/// and the loop continues (AC-19, AC-27); only batch-level failures (configuration, candidate
/// query, audit-record failures) propagate to JobOrchestrator for retry/failure handling.
/// </summary>
public sealed class MilestoneUpdateNotificationsJob : IBatchJob
{
    /// <summary>
    /// Template version stamped on every <c>fps.notification_delivery</c> row for auditability.
    /// Increment when the rendered email template changes shape (spec §16).
    /// </summary>
    private const string TemplateVersion = "v1";

    /// <summary>
    /// Canonical notification type key written to all audit rows (matches <c>notificationtype</c>
    /// column in <c>fps.notification_delivery</c> and <c>fps.notification_run_summary</c>).
    /// </summary>
    private const string NotificationType = "MilestoneUpdate";

    private readonly IMilestoneNotificationReadRepository _readRepository;
    private readonly INotificationSettingsPreflight _preflight;
    private readonly IReportingYearResolver _yearResolver;
    private readonly INotificationGroupingService _groupingService;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IEmailService _emailService;
    private readonly INotificationDeliveryRepository _deliveryRepository;
    private readonly ICapsSummaryService _capsSummaryService;
    private readonly ICurrentJobExecutionContext _executionContext;
    private readonly MilestoneNotificationsSettings _settings;
    private readonly ILogger<MilestoneUpdateNotificationsJob> _logger;

    public string Name => BatchJobNames.MilestoneUpdateNotifications;
    public string IdempotencyStrategy => "RecipientMonthDeduplicationKey";
    public string? ScheduleExpression => null; // TBD — cron expression pending stakeholder confirmation
    public string? ScheduleDescription => null; // TBD
    public int? MaxExecutionSeconds => null;

    public MilestoneUpdateNotificationsJob(
        IMilestoneNotificationReadRepository readRepository,
        INotificationSettingsPreflight preflight,
        IReportingYearResolver yearResolver,
        INotificationGroupingService groupingService,
        IEmailTemplateRenderer templateRenderer,
        IEmailService emailService,
        INotificationDeliveryRepository deliveryRepository,
        ICapsSummaryService capsSummaryService,
        ICurrentJobExecutionContext executionContext,
        IOptions<MilestoneNotificationsSettings> settings,
        ILogger<MilestoneUpdateNotificationsJob> logger)
    {
        _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
        _preflight = preflight ?? throw new ArgumentNullException(nameof(preflight));
        _yearResolver = yearResolver ?? throw new ArgumentNullException(nameof(yearResolver));
        _groupingService = groupingService ?? throw new ArgumentNullException(nameof(groupingService));
        _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
        _capsSummaryService = capsSummaryService ?? throw new ArgumentNullException(nameof(capsSummaryService));
        _executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Executes the MilestoneUpdateNotifications job.</summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Execution contract: JobOrchestrator must Initialize the shared execution context with
        // a real jobQueueId before this job can run. A Guid.Empty here means the context was
        // never populated — fail loudly before touching candidates or sending any email, rather
        // than silently writing audit rows with no execution to attribute them to.
        if (_executionContext.JobQueueId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "MilestoneUpdateNotifications: JobQueueId was not resolved for this execution. " +
                "ICurrentJobExecutionContext must be initialized by JobOrchestrator before ExecuteAsync runs.");
        }

        var jobQueueId = _executionContext.JobQueueId;
        var startedAt = DateTime.UtcNow;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobExecutionId"] = _executionContext.JobExecutionId,
            ["JobQueueId"] = jobQueueId,
            ["JobName"] = Name
        });

        _logger.LogInformation("===========================================");
        _logger.LogInformation("MilestoneUpdateNotifications Job - Starting");
        _logger.LogInformation("===========================================");
        _logger.LogInformation("Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff} | ProcessId: {ProcessId}",
            startedAt, Environment.ProcessId);

        // Resolve the effective calendar month (plan section 6.2).
        var parametersJson = _executionContext.ParametersJson;
        var monthOverride = TryExtractMonthOverride(parametersJson);
        var effectiveMonth = ResolveEffectiveMonth(monthOverride);

        _logger.LogInformation(
            "Effective calendar month: {Month}{Override}",
            effectiveMonth,
            monthOverride.HasValue ? " (override from parameters)" : " (wall-clock)");

        // forceResend is only honoured in Manual run mode (plan §12).
        var isManualMode = _executionContext.RunMode == RunMode.Manual;
        var isForceResend = isManualMode && TryExtractForceResend(parametersJson);

        if (isForceResend)
            _logger.LogInformation("forceResend=true (Manual mode) — duplicate-send gate bypassed for Sent rows.");

        // Step 1: settings preflight (plan §8.1).
        await _preflight.ValidateAsync(cancellationToken);
        _logger.LogInformation("Settings preflight passed.");

        // Step 2: load all candidates unfiltered (plan §7.1).
        var candidates = await _readRepository.GetNotificationCandidatesAsync(cancellationToken);
        _logger.LogInformation("Loaded {CandidateCount} notification candidate row(s).", candidates.Count);

        // Step 3: resolve reporting year from candidate rows, or fall back to vlatestmonthyear.
        int reportingYear;
        if (candidates.Count > 0)
        {
            var years = candidates.Select(c => c.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                throw new InvalidOperationException(
                    "Milestone notification view returned candidates spanning multiple reporting years " +
                    $"({string.Join(", ", years)}) — the view or its underlying tables may be misconfigured.");
            }

            reportingYear = years[0];
            _logger.LogInformation("Reporting year resolved from candidate rows: {Year}.", reportingYear);
        }
        else
        {
            var fallback = await _yearResolver.ResolveAsync(cancellationToken);
            reportingYear = fallback.Year;
            _logger.LogInformation(
                "Zero candidates returned. Reporting year resolved via fallback (vlatestmonthyear): {Year}, LatestMonthReleased={Month}.",
                fallback.Year, fallback.LatestMonthReleased);
        }

        // Step 4: get-or-create the per-execution run summary row (plan §11.4). Idempotent by
        // jobQueueId so a whole-job retry resumes the same row instead of failing on insert.
        var runSummaryId = await _deliveryRepository.GetOrCreateRunSummaryAsync(
            jobQueueId, NotificationType, reportingYear, effectiveMonth, cancellationToken);

        _logger.LogInformation(
            "Run summary row ready | RunSummaryId={RunSummaryId} | Year={Year} | Month={Month}",
            runSummaryId, reportingYear, effectiveMonth);

        var counters = new NotificationRunSummaryCounters
        {
            CandidateCount = candidates.Count,
            CandidateProjectCount = candidates.Select(c => c.ParentProject).Distinct().Count()
        };

        // Step 5: diagnostic query — reporting only, non-fatal if it fails (plan §7.2).
        await RunDiagnosticQueryAsync(reportingYear, counters, cancellationToken);

        // Step 6: zero-candidate early exit — a valid Completed outcome per spec §19.
        if (candidates.Count == 0)
        {
            _logger.LogInformation(
                "No eligible projects found for year {Year}. Job completes with zero sends (valid outcome).",
                reportingYear);

            await _deliveryRepository.UpdateRunSummaryCountersAsync(runSummaryId, counters, cancellationToken);
            await _capsSummaryService.SendAndRecordAsync(
                runSummaryId, jobQueueId, reportingYear, effectiveMonth, counters, cancellationToken);

            _logger.LogInformation("===========================================");
            _logger.LogInformation("MilestoneUpdateNotifications Job - Completed (zero candidates)");
            _logger.LogInformation("===========================================");
            return;
        }

        // Step 7: group candidates into per-recipient notification groups (plan §9.1).
        var groups = _groupingService.GroupCandidates(candidates);
        counters.IdentifiedRecipientCount = groups.Count;
        _logger.LogInformation("Grouped candidates into {GroupCount} recipient group(s).", groups.Count);

        // Step 8: send loop — one failure does not stop the remaining sends (AC-19, plan §10.3).
        var includeConfirmationInstruction = _settings.MandatoryConfirmationMonths.Contains(effectiveMonth);

        foreach (var group in groups)
        {
            var deliveryKey = new NotificationDeliveryKey(
                NotificationType, reportingYear, effectiveMonth, group.RecipientId);

            // Disabled recipient — skip and audit (plan §7.1, AC-24).
            if (group.IsDisabled)
            {
                _logger.LogInformation(
                    "Skipping disabled recipient | RecipientId={RecipientId} | Manager={Manager} | Projects={ProjectCount}",
                    group.RecipientId, group.ProjectManager, group.Projects.Count);

                await _deliveryRepository.InsertSkippedDeliveryAsync(
                    jobQueueId, deliveryKey, group.DurablePersonId, group.ProjectManager, group.Email,
                    "RecipientDisabled", TemplateVersion,
                    group.Projects.Select(p => (p.ParentProject, p.Year)).ToList(),
                    cancellationToken);

                counters.DisabledRecipientCount++;
                counters.DisabledProjectCount += group.Projects.Count;
                counters.ManagerEmailSkippedCount++;
                continue;
            }

            // Missing email — skip and audit (plan §7.1, AC-26).
            if (string.IsNullOrWhiteSpace(group.Email))
            {
                _logger.LogInformation(
                    "Skipping recipient with no email address | RecipientId={RecipientId} | Manager={Manager} | Projects={ProjectCount}",
                    group.RecipientId, group.ProjectManager, group.Projects.Count);

                await _deliveryRepository.InsertSkippedDeliveryAsync(
                    jobQueueId, deliveryKey, group.DurablePersonId, group.ProjectManager, group.Email,
                    "EmailMissing", TemplateVersion,
                    group.Projects.Select(p => (p.ParentProject, p.Year)).ToList(),
                    cancellationToken);

                counters.MissingEmailRecipientCount++;
                counters.MissingEmailProjectCount += group.Projects.Count;
                counters.ManagerEmailSkippedCount++;
                continue;
            }

            // Three-outcome duplicate check (plan §11.2, §12).
            var existing = await _deliveryRepository.GetExistingAttemptAsync(deliveryKey, cancellationToken);
            if (existing != null)
            {
                if (existing.DeliveryStatus == "Sent" && !isForceResend)
                {
                    _logger.LogInformation(
                        "DuplicateSuccessfulDelivery — prior Sent row found, skipping | " +
                        "RecipientId={RecipientId} | Manager={Manager} | PriorDeliveryId={PriorDeliveryId}",
                        group.RecipientId, group.ProjectManager, existing.NotificationDeliveryId);
                    counters.DuplicateSkippedCount++;
                    continue;
                }

                if (existing.DeliveryStatus == "Sending")
                {
                    _logger.LogWarning(
                        "Prior Sending row found — transitioning to OutcomeUnknown; operational review required | " +
                        "RecipientId={RecipientId} | Manager={Manager} | PriorDeliveryId={PriorDeliveryId}",
                        group.RecipientId, group.ProjectManager, existing.NotificationDeliveryId);
                    await _deliveryRepository.TransitionToOutcomeUnknownAsync(
                        existing.NotificationDeliveryId, cancellationToken);
                    counters.OutcomeUnknownRecipientCount++;
                    continue;
                }

                // Outcome 3: Sent + forceResend=true — proceed with a fresh attempt.
                _logger.LogInformation(
                    "forceResend — prior Sent row found but forceResend=true; proceeding with new attempt | " +
                    "RecipientId={RecipientId} | Manager={Manager} | PriorDeliveryId={PriorDeliveryId}",
                    group.RecipientId, group.ProjectManager, existing.NotificationDeliveryId);
            }

            // Render template — excluded projects have invalid EditLink (plan §10.3).
            var renderResult = _templateRenderer.RenderManagerEmailBody(
                group.ProjectManager,
                group.Projects,
                includeConfirmationInstruction);

            if (renderResult.ExcludedProjects.Count > 0)
            {
                _logger.LogWarning(
                    "Excluded {ExcludedCount} project(s) with invalid EditLink | RecipientId={RecipientId} | Manager={Manager} | ExcludedProjects={Projects}",
                    renderResult.ExcludedProjects.Count,
                    group.RecipientId,
                    group.ProjectManager,
                    string.Join(", ", renderResult.ExcludedProjects.Select(p => p.ParentProject)));
            }

            // No valid project links remain — skip the send (plan §10.3).
            if (renderResult.IncludedProjects.Count == 0)
            {
                _logger.LogWarning(
                    "Skipping recipient — zero valid project links after EditLink validation | RecipientId={RecipientId} | Manager={Manager}",
                    group.RecipientId, group.ProjectManager);

                await _deliveryRepository.InsertSkippedDeliveryAsync(
                    jobQueueId, deliveryKey, group.DurablePersonId, group.ProjectManager, group.Email,
                    "NoValidProjectLinks", TemplateVersion,
                    group.Projects.Select(p => (p.ParentProject, p.Year)).ToList(),
                    cancellationToken);

                counters.ManagerEmailSkippedCount++;
                continue;
            }

            // Audit: Pending -> Sending -> (send) -> Sent/Failed (plan §11, write order).
            var children = renderResult.IncludedProjects
                .Select(p => (p.ParentProject, p.Year, "Pending", (string?)null))
                .Concat(renderResult.ExcludedProjects
                    .Select(p => (p.ParentProject, p.Year, "Skipped", (string?)"NoValidProjectLinks")))
                .ToList();

            var deliveryId = await _deliveryRepository.InsertPendingDeliveryAsync(
                jobQueueId, deliveryKey, group.DurablePersonId, group.ProjectManager, group.Email,
                isForceResend, TemplateVersion, children, cancellationToken);

            await _deliveryRepository.UpdateDeliveryToSendingAsync(deliveryId, cancellationToken);

            // Send email (AC-27: a failure here does not stop subsequent sends).
            var message = new EmailMessage(
                To: [group.Email],
                Subject: _templateRenderer.Subject,
                HtmlBody: renderResult.HtmlBody);

            var sendResult = await _emailService.SendAsync(message, cancellationToken);
            counters.ManagerEmailAttemptCount++;

            if (sendResult.Succeeded)
            {
                var sentAt = DateTime.UtcNow;
                await _deliveryRepository.UpdateDeliveryOutcomeAsync(
                    deliveryId, "Sent", null, sentAt, cancellationToken);
                _logger.LogInformation(
                    "EmailSent | RecipientId={RecipientId} | Manager={Manager} | Email={Email} | Projects={ProjectCount} | DeliveryId={DeliveryId}",
                    group.RecipientId, group.ProjectManager, group.Email,
                    renderResult.IncludedProjects.Count, deliveryId);
                counters.ManagerEmailSentCount++;
            }
            else
            {
                await _deliveryRepository.UpdateDeliveryOutcomeAsync(
                    deliveryId, "Failed", sendResult.FailureMessage, null, cancellationToken);
                _logger.LogError(
                    "EmailSendFailed | RecipientId={RecipientId} | Manager={Manager} | Email={Email} | Reason={Reason} | DeliveryId={DeliveryId}",
                    group.RecipientId, group.ProjectManager, group.Email,
                    sendResult.FailureMessage, deliveryId);
                counters.ManagerEmailFailedCount++;
            }
        }

        // Post-loop: finalize counters, persist, send CAPS summary.
        counters.ManagerDeliveryCount = counters.ManagerEmailAttemptCount + counters.ManagerEmailSkippedCount;

        var duration = DateTime.UtcNow - startedAt;
        _logger.LogInformation(
            "SendManagerEmailsStep complete | Year={Year} | Month={Month} | " +
            "Sent={Sent} | Failed={Failed} | SkippedDisabled={SkippedDisabled} | SkippedNoEmail={SkippedNoEmail} | " +
            "OutcomeUnknown={OutcomeUnknown} | DuplicateSkipped={DuplicateSkipped} | Duration={DurationSeconds}s",
            reportingYear, effectiveMonth,
            counters.ManagerEmailSentCount, counters.ManagerEmailFailedCount,
            counters.DisabledRecipientCount, counters.MissingEmailRecipientCount,
            counters.OutcomeUnknownRecipientCount, counters.DuplicateSkippedCount,
            (int)duration.TotalSeconds);

        await _deliveryRepository.UpdateRunSummaryCountersAsync(runSummaryId, counters, cancellationToken);
        await _capsSummaryService.SendAndRecordAsync(
            runSummaryId, jobQueueId, reportingYear, effectiveMonth, counters, cancellationToken);

        _logger.LogInformation("===========================================");
        _logger.LogInformation(
            "MilestoneUpdateNotifications Job - Completed | Sent={Sent} | Failed={Failed} | Duration={DurationSeconds}s",
            counters.ManagerEmailSentCount, counters.ManagerEmailFailedCount, (int)duration.TotalSeconds);
        _logger.LogInformation("===========================================");
    }

    /// <summary>
    /// Resolves the effective calendar month for this run.
    /// Supports a MILESTONE_TEST_UTCNOW environment variable for deterministic test overrides.
    /// </summary>
    internal int ResolveEffectiveMonth(int? monthOverride)
    {
        if (monthOverride.HasValue)
            return monthOverride.Value;

        var utcNow = DateTime.UtcNow;
        var overrideUtcNow = Environment.GetEnvironmentVariable("MILESTONE_TEST_UTCNOW");
        if (!string.IsNullOrWhiteSpace(overrideUtcNow)
            && DateTime.TryParse(
                overrideUtcNow,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                out var parsedOverride))
        {
            utcNow = parsedOverride;
        }

        return utcNow.Month;
    }

    /// <summary>
    /// Validates the month override production guard (plan §6.2, AC-31).
    /// </summary>
    internal static void ValidateMonthOverride(int? monthOverride, bool isProduction, bool allowMonthOverrideInProduction)
    {
        if (!monthOverride.HasValue)
            return;

        if (monthOverride < 1 || monthOverride > 12)
            throw new JobValidationException(
                $"monthOverride value '{monthOverride}' is out of range. Expected 1-12.");

        if (isProduction && !allowMonthOverrideInProduction)
            throw new JobValidationException(
                "monthOverride is not permitted in Production without AllowMonthOverrideInProduction = true (plan §6.2, AC-31).");
    }

    private int? TryExtractMonthOverride(string? parametersJson)
    {
        int? rawOverride = null;

        if (!string.IsNullOrWhiteSpace(parametersJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(parametersJson);
                if (doc.RootElement.TryGetProperty("monthOverride", out var el))
                {
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var num))
                        rawOverride = num;
                    else if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out var str))
                        rawOverride = str;
                }
            }
            catch (JsonException)
            {
                // Malformed JSON — treat as no override.
            }
        }

        var environment = EnvironmentResolver.GetEnvironmentName(string.Empty);
        var isProduction = string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase);
        ValidateMonthOverride(rawOverride, isProduction, _settings.AllowMonthOverrideInProduction);

        return rawOverride;
    }

    /// <summary>
    /// Extracts the forceResend boolean from the parameters JSON blob.
    /// Returns false when absent or unparseable.
    /// Callers must gate this on Manual run mode — Scheduled runs must never honour forceResend.
    /// </summary>
    internal static bool TryExtractForceResend(string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(parametersJson);
            if (doc.RootElement.TryGetProperty("forceResend", out var el))
            {
                return el.ValueKind == JsonValueKind.True
                    || (el.ValueKind == JsonValueKind.String
                        && bool.TryParse(el.GetString(), out var b)
                        && b);
            }
        }
        catch (JsonException)
        {
            // Malformed JSON — treat as no forceResend.
        }

        return false;
    }

    /// <summary>
    /// Runs the diagnostic query for unmatched recipient resolution issues (plan §7.2).
    /// Populates counters with the unresolved counts, or marks DiagnosticAvailable=false on failure.
    /// </summary>
    private async Task RunDiagnosticQueryAsync(
        int reportingYear,
        NotificationRunSummaryCounters counters,
        CancellationToken cancellationToken)
    {
        try
        {
            var issues = await _readRepository.GetRecipientResolutionIssuesAsync(cancellationToken);
            counters.DiagnosticAvailable = true;
            counters.UnresolvedRecipientCount = issues
                .Select(i => i.ProjectManager)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            counters.UnresolvedProjectCount = issues.Count;

            if (issues.Count > 0)
            {
                _logger.LogWarning(
                    "RecipientDiagnosticCompleted | {IssueCount} project(s) excluded from view due to unmatched tblprojectmanager entry | Year={Year} | Projects={Projects}",
                    issues.Count, reportingYear,
                    string.Join(", ", issues.Select(i => i.ParentProject)));
            }
            else
            {
                _logger.LogInformation(
                    "RecipientDiagnosticCompleted | No unmatched recipient resolution issues for year {Year}.",
                    reportingYear);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex,
                "RecipientDiagnosticUnavailable | GetRecipientResolutionIssuesAsync failed — " +
                "omitting unresolved-recipient count from this run (plan §7.2).");
            counters.DiagnosticAvailable = false;
            counters.UnresolvedRecipientCount = null;
            counters.UnresolvedProjectCount = null;
        }
    }
}
