using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class MilestoneUpdateNotificationsJobHandlerTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static MilestoneUpdateNotificationsJobHandler CreateHandler(
        IMilestoneNotificationReadRepository? readRepository = null,
        INotificationSettingsPreflight? preflight = null,
        IReportingYearResolver? yearResolver = null,
        INotificationGroupingService? groupingService = null,
        IEmailTemplateRenderer? templateRenderer = null,
        IEmailService? emailService = null,
        INotificationDeliveryRepository? deliveryRepository = null,
        ICapsSummaryService? capsSummaryService = null,
        MilestoneNotificationsSettings? settings = null)
    {
        return new MilestoneUpdateNotificationsJobHandler(
            readRepository ?? Substitute.For<IMilestoneNotificationReadRepository>(),
            preflight ?? Substitute.For<INotificationSettingsPreflight>(),
            yearResolver ?? Substitute.For<IReportingYearResolver>(),
            groupingService ?? Substitute.For<INotificationGroupingService>(),
            templateRenderer ?? Substitute.For<IEmailTemplateRenderer>(),
            emailService ?? Substitute.For<IEmailService>(),
            deliveryRepository ?? Substitute.For<INotificationDeliveryRepository>(),
            capsSummaryService ?? Substitute.For<ICapsSummaryService>(),
            null, // executionRepository — null in unit tests; jobQueueId defaults to Guid.Empty
            Options.Create(settings ?? new MilestoneNotificationsSettings()),
            NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance);
    }

    // Shared delivery repo stub: returns a new RunSummaryId and no-op for other methods.
    private static INotificationDeliveryRepository DefaultDeliveryRepo()
    {
        var repo = Substitute.For<INotificationDeliveryRepository>();
        repo.InsertRunSummaryAsync(default, default!, default, default, default)
            .ReturnsForAnyArgs(Guid.NewGuid());
        return repo;
    }

    // -------------------------------------------------------------------------
    // Constructor guards
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenReadRepositoryIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                null!,
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("readRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenPreflightIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                null!,
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("preflight", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenYearResolverIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                null!,
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("yearResolver", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenGroupingServiceIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                null!,
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("groupingService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenTemplateRendererIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                null!,
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("templateRenderer", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenEmailServiceIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                null!,
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("emailService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenDeliveryRepositoryIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                null!,
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("deliveryRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenCapsSummaryServiceIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                null!,
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("capsSummaryService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                null!,
                NullLogger<MilestoneUpdateNotificationsJobHandler>.Instance));
        Assert.Equal("settings", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneUpdateNotificationsJobHandler(
                Substitute.For<IMilestoneNotificationReadRepository>(),
                Substitute.For<INotificationSettingsPreflight>(),
                Substitute.For<IReportingYearResolver>(),
                Substitute.For<INotificationGroupingService>(),
                Substitute.For<IEmailTemplateRenderer>(),
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Substitute.For<ICapsSummaryService>(),
                null,
                Options.Create(new MilestoneNotificationsSettings()),
                null!));
        Assert.Equal("logger", ex.ParamName);
    }

    // -------------------------------------------------------------------------
    // Metadata
    // -------------------------------------------------------------------------

    [Fact]
    public void Metadata_ShouldMatchExpectedContract()
    {
        var handler = CreateHandler();
        Assert.Equal(BatchJobNames.MilestoneUpdateNotifications, handler.Name);
        Assert.Equal("RecipientMonthDeduplicationKey", handler.IdempotencyStrategy);
    }

    // -------------------------------------------------------------------------
    // AC-31: Month-override production guard
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidateMonthOverride_WhenNoOverride_ShouldNotThrow()
    {
        MilestoneUpdateNotificationsJobHandler.ValidateMonthOverride(null, isProduction: true, allowMonthOverrideInProduction: false);
    }

    [Fact]
    public void ValidateMonthOverride_WhenOverrideInNonProduction_ShouldNotThrow()
    {
        MilestoneUpdateNotificationsJobHandler.ValidateMonthOverride(6, isProduction: false, allowMonthOverrideInProduction: false);
    }

    [Fact]
    public void ValidateMonthOverride_WhenOverrideInProductionWithFlagSet_ShouldNotThrow()
    {
        MilestoneUpdateNotificationsJobHandler.ValidateMonthOverride(6, isProduction: true, allowMonthOverrideInProduction: true);
    }

    [Fact]
    public void ValidateMonthOverride_WhenOverrideInProductionWithoutFlag_ShouldThrowJobValidationException()
    {
        Assert.Throws<JobValidationException>(() =>
            MilestoneUpdateNotificationsJobHandler.ValidateMonthOverride(6, isProduction: true, allowMonthOverrideInProduction: false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void ValidateMonthOverride_WhenOverrideOutOfRange_ShouldThrowJobValidationException(int month)
    {
        Assert.Throws<JobValidationException>(() =>
            MilestoneUpdateNotificationsJobHandler.ValidateMonthOverride(month, isProduction: false, allowMonthOverrideInProduction: false));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void ValidateMonthOverride_WhenOverrideInRange_ShouldNotThrow(int month)
    {
        MilestoneUpdateNotificationsJobHandler.ValidateMonthOverride(month, isProduction: false, allowMonthOverrideInProduction: false);
    }

    // -------------------------------------------------------------------------
    // TryExtractForceResend
    // -------------------------------------------------------------------------

    [Fact]
    public void TryExtractForceResend_WhenNullJson_ShouldReturnFalse()
    {
        Assert.False(MilestoneUpdateNotificationsJobHandler.TryExtractForceResend(null));
    }

    [Fact]
    public void TryExtractForceResend_WhenForceResendTrue_ShouldReturnTrue()
    {
        Assert.True(MilestoneUpdateNotificationsJobHandler.TryExtractForceResend("{\"forceResend\":true}"));
    }

    [Fact]
    public void TryExtractForceResend_WhenForceResendFalse_ShouldReturnFalse()
    {
        Assert.False(MilestoneUpdateNotificationsJobHandler.TryExtractForceResend("{\"forceResend\":false}"));
    }

    [Fact]
    public void TryExtractForceResend_WhenForceResendAbsent_ShouldReturnFalse()
    {
        Assert.False(MilestoneUpdateNotificationsJobHandler.TryExtractForceResend("{\"monthOverride\":6}"));
    }

    [Fact]
    public void TryExtractForceResend_WhenMalformedJson_ShouldReturnFalse()
    {
        Assert.False(MilestoneUpdateNotificationsJobHandler.TryExtractForceResend("not-json"));
    }

    // -------------------------------------------------------------------------
    // ResolveEffectiveMonth
    // -------------------------------------------------------------------------

    [Fact]
    public void ResolveEffectiveMonth_WhenOverrideProvided_ShouldReturnOverride()
    {
        var handler = CreateHandler();
        Assert.Equal(7, handler.ResolveEffectiveMonth(7));
    }

    [Fact]
    public void ResolveEffectiveMonth_WhenNoOverride_ShouldReturnCurrentMonth()
    {
        var handler = CreateHandler();
        Assert.Equal(DateTime.UtcNow.Month, handler.ResolveEffectiveMonth(null));
    }

    // -------------------------------------------------------------------------
    // AC-19: Successful send — one enabled group with valid links
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenOneEnabledRecipientWithValidLink_ShouldSendOneEmail()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var templateRenderer = Substitute.For<IEmailTemplateRenderer>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();
        var capsService = Substitute.For<ICapsSummaryService>();

        var candidate = new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M001", "jane@example.com", false, "<a href=\"https://example.com/proj-a\">PROJ-A</a>");
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[candidate]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var project = new NotificationProjectLink(2026, "PROJ-A", "<a href=\"https://example.com/proj-a\">PROJ-A</a>");
        var group = new NotificationGroup("recipientid1", "M001", "Jane Smith", "M001", "jane@example.com", false, [project]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([group]);

        templateRenderer.Subject.Returns("Milestone and Deliverable Update Request");
        templateRenderer.RenderManagerEmailBody(Arg.Any<string>(), Arg.Any<IReadOnlyList<NotificationProjectLink>>(), Arg.Any<bool>())
            .Returns(new EmailTemplateRenderResult("<html>body</html>", [project], []));

        emailService.SendAsync(Arg.Any<EmailMessage>(), default).ReturnsForAnyArgs(EmailSendResult.Sent());
        deliveryRepo.InsertPendingDeliveryAsync(default, default!, default, default!, default, default, default!, default!, default)
            .ReturnsForAnyArgs(Guid.NewGuid());

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService, templateRenderer: templateRenderer,
            emailService: emailService, deliveryRepository: deliveryRepo, capsSummaryService: capsService);

        await handler.ExecuteAsync();

        await emailService.ReceivedWithAnyArgs(1).SendAsync(default!, default);
        await deliveryRepo.ReceivedWithAnyArgs(1).InsertPendingDeliveryAsync(
            default, default!, default, default!, default, default, default!, default!, default);
        await capsService.ReceivedWithAnyArgs(1).SendAndRecordAsync(
            default, default, default, default, default!, default);
    }

    // -------------------------------------------------------------------------
    // AC-24: Disabled recipients are skipped and written as Skipped audit rows
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenRecipientIsDisabled_ShouldNotSendEmailAndShouldAuditAsSkipped()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();

        var candidate = new MilestoneNotificationCandidate(2026, "PROJ-B", "Bob Jones", "M002", "bob@example.com", true, null);
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[candidate]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var project = new NotificationProjectLink(2026, "PROJ-B", null);
        var group = new NotificationGroup("recipientid2", "M002", "Bob Jones", "M002", "bob@example.com", true, [project]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([group]);

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService, emailService: emailService,
            deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        await deliveryRepo.ReceivedWithAnyArgs(1).InsertSkippedDeliveryAsync(
            default, default!, default, default!, default, "RecipientDisabled", default!, default!, default);
    }

    // -------------------------------------------------------------------------
    // AC-26: Recipients with no email address are skipped
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenRecipientHasNoEmail_ShouldNotSendEmailAndShouldAuditAsSkipped()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();

        var candidate = new MilestoneNotificationCandidate(2026, "PROJ-C", "Alice Wu", null, null, false, null);
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[candidate]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var project = new NotificationProjectLink(2026, "PROJ-C", null);
        var group = new NotificationGroup("recipientid3", null, "Alice Wu", null, null /* no email */, false, [project]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([group]);

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService, emailService: emailService,
            deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        await deliveryRepo.ReceivedWithAnyArgs(1).InsertSkippedDeliveryAsync(
            default, default!, default, default!, default, "EmailMissing", default!, default!, default);
    }

    // -------------------------------------------------------------------------
    // AC-27: One send failure does not stop the remaining sends
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenOneSendFails_ShouldContinueSendingToRemainingRecipients()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var templateRenderer = Substitute.For<IEmailTemplateRenderer>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();
        deliveryRepo.InsertPendingDeliveryAsync(default, default!, default, default!, default, default, default!, default!, default)
            .ReturnsForAnyArgs(Guid.NewGuid());

        var c1 = new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M001", "jane@example.com", false, "<a href=\"https://example.com/a\">A</a>");
        var c2 = new MilestoneNotificationCandidate(2026, "PROJ-B", "Bob Jones", "M002", "bob@example.com", false, "<a href=\"https://example.com/b\">B</a>");
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[c1, c2]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var p1 = new NotificationProjectLink(2026, "PROJ-A", c1.EditLink);
        var p2 = new NotificationProjectLink(2026, "PROJ-B", c2.EditLink);
        var g1 = new NotificationGroup("r1", "M001", "Jane Smith", "M001", "jane@example.com", false, [p1]);
        var g2 = new NotificationGroup("r2", "M002", "Bob Jones", "M002", "bob@example.com", false, [p2]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([g1, g2]);

        templateRenderer.Subject.Returns("Milestone and Deliverable Update Request");
        templateRenderer.RenderManagerEmailBody(Arg.Any<string>(), Arg.Any<IReadOnlyList<NotificationProjectLink>>(), Arg.Any<bool>())
            .Returns(new EmailTemplateRenderResult("<html>body</html>", [p1], []));

        var callCount = 0;
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => ++callCount == 1 ? EmailSendResult.Failed("transient error") : EmailSendResult.Sent());

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService, templateRenderer: templateRenderer,
            emailService: emailService, deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.ReceivedWithAnyArgs(2).SendAsync(default!, default);
    }

    // -------------------------------------------------------------------------
    // Zero candidates — valid Completed outcome; CAPS still called
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenZeroCandidates_ShouldNotSendAnyEmailAndShouldCallCaps()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var yearResolver = Substitute.For<IReportingYearResolver>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();
        var capsService = Substitute.For<ICapsSummaryService>();

        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);
        yearResolver.ResolveAsync(default).ReturnsForAnyArgs(new ReportingYear(2026, 3));

        var handler = CreateHandler(readRepo, preflight, yearResolver, groupingService, emailService: emailService,
            deliveryRepository: deliveryRepo, capsSummaryService: capsService);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        groupingService.DidNotReceiveWithAnyArgs().GroupCandidates(default!);
        // CAPS summary must still be called even with zero candidates.
        await capsService.ReceivedWithAnyArgs(1).SendAndRecordAsync(
            default, default, default, default, default!, default);
    }

    // -------------------------------------------------------------------------
    // Preflight failure fails the job (plan §8.1, AC-34 preparation)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenPreflightFails_ShouldThrowBeforeQueryingCandidates()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();

        preflight.ValidateAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Required tbl_settings row missing.")));

        var handler = CreateHandler(readRepo, preflight);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.ExecuteAsync());
        await readRepo.DidNotReceiveWithAnyArgs().GetNotificationCandidatesAsync(default);
    }

    // -------------------------------------------------------------------------
    // Multi-year invariant check (plan §6.1)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenCandidatesSpanMultipleYears_ShouldThrow()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();

        var c1 = new MilestoneNotificationCandidate(2025, "PROJ-X", "Jane Smith", "M001", "jane@example.com", false, null);
        var c2 = new MilestoneNotificationCandidate(2026, "PROJ-Y", "Jane Smith", "M001", "jane@example.com", false, null);
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[c1, c2]);

        var handler = CreateHandler(readRepo, preflight);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.ExecuteAsync());
    }

    // -------------------------------------------------------------------------
    // Recipients with zero valid links after rendering are skipped
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenAllProjectLinksInvalid_ShouldSkipSendWithoutThrowing()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var templateRenderer = Substitute.For<IEmailTemplateRenderer>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();

        var candidate = new MilestoneNotificationCandidate(2026, "PROJ-D", "Carol Lee", "M003", "carol@example.com", false, null);
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[candidate]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var project = new NotificationProjectLink(2026, "PROJ-D", null);
        var group = new NotificationGroup("r4", "M003", "Carol Lee", "M003", "carol@example.com", false, [project]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([group]);

        templateRenderer.Subject.Returns("Milestone and Deliverable Update Request");
        templateRenderer.RenderManagerEmailBody(Arg.Any<string>(), Arg.Any<IReadOnlyList<NotificationProjectLink>>(), Arg.Any<bool>())
            .Returns(new EmailTemplateRenderResult(string.Empty, [], [project]));

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService, templateRenderer: templateRenderer,
            emailService: emailService, deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        await deliveryRepo.ReceivedWithAnyArgs(1).InsertSkippedDeliveryAsync(
            default, default!, default, default!, default, "NoValidProjectLinks", default!, default!, default);
    }

    // -------------------------------------------------------------------------
    // Diagnostic query failure is non-fatal (plan §7.2)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenDiagnosticQueryFails_ShouldContinueAndNotThrow()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var yearResolver = Substitute.For<IReportingYearResolver>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();

        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[]);
        readRepo.GetRecipientResolutionIssuesAsync(default)
            .Returns(Task.FromException<IReadOnlyList<RecipientResolutionIssue>>(new InvalidOperationException("DB unavailable")));
        yearResolver.ResolveAsync(default).ReturnsForAnyArgs(new ReportingYear(2026, 3));

        var handler = CreateHandler(readRepo, preflight, yearResolver, groupingService, emailService: emailService,
            deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    // -------------------------------------------------------------------------
    // Duplicate delivery prevention — Sent row found, no forceResend (plan §12)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenPriorSentRowExistsAndNoForceResend_ShouldSkipSendAndCountAsDuplicate()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var templateRenderer = Substitute.For<IEmailTemplateRenderer>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();

        var candidate = new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M001", "jane@example.com", false, "<a href=\"https://example.com/a\">A</a>");
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[candidate]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var project = new NotificationProjectLink(2026, "PROJ-A", candidate.EditLink);
        var group = new NotificationGroup("r1", "M001", "Jane Smith", "M001", "jane@example.com", false, [project]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([group]);

        // Existing Sent row — should prevent a new send (forceResend is false by default).
        deliveryRepo.GetExistingAttemptAsync(Arg.Any<NotificationDeliveryKey>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingDeliveryAttempt(Guid.NewGuid(), "Sent", Guid.NewGuid()));

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService, templateRenderer: templateRenderer,
            emailService: emailService, deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        await deliveryRepo.DidNotReceiveWithAnyArgs().InsertPendingDeliveryAsync(
            default, default!, default, default!, default, default, default!, default!, default);
    }

    // -------------------------------------------------------------------------
    // OutcomeUnknown: prior Sending row found (plan §11.2)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenPriorSendingRowExists_ShouldTransitionToOutcomeUnknownAndSkipSend()
    {
        var readRepo = Substitute.For<IMilestoneNotificationReadRepository>();
        var preflight = Substitute.For<INotificationSettingsPreflight>();
        var groupingService = Substitute.For<INotificationGroupingService>();
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepo = DefaultDeliveryRepo();

        var candidate = new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M001", "jane@example.com", false, "<a href=\"https://example.com/a\">A</a>");
        readRepo.GetNotificationCandidatesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<MilestoneNotificationCandidate>)[candidate]);
        readRepo.GetRecipientResolutionIssuesAsync(default).ReturnsForAnyArgs(
            (IReadOnlyList<RecipientResolutionIssue>)[]);

        var project = new NotificationProjectLink(2026, "PROJ-A", candidate.EditLink);
        var group = new NotificationGroup("r1", "M001", "Jane Smith", "M001", "jane@example.com", false, [project]);
        groupingService.GroupCandidates(Arg.Any<IReadOnlyList<MilestoneNotificationCandidate>>()).Returns([group]);

        var priorDeliveryId = Guid.NewGuid();
        deliveryRepo.GetExistingAttemptAsync(Arg.Any<NotificationDeliveryKey>(), Arg.Any<CancellationToken>())
            .Returns(new ExistingDeliveryAttempt(priorDeliveryId, "Sending", Guid.NewGuid()));

        var handler = CreateHandler(readRepo, preflight, groupingService: groupingService,
            emailService: emailService, deliveryRepository: deliveryRepo);

        await handler.ExecuteAsync();

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
        await deliveryRepo.Received(1).TransitionToOutcomeUnknownAsync(priorDeliveryId, Arg.Any<CancellationToken>());
    }
}