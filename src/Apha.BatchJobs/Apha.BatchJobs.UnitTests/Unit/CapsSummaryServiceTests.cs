using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class CapsSummaryServiceTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CapsSummaryService CreateService(
        IEmailService? emailService = null,
        INotificationDeliveryRepository? deliveryRepository = null,
        MilestoneNotificationsSettings? settings = null)
    {
        return new CapsSummaryService(
            emailService ?? Substitute.For<IEmailService>(),
            deliveryRepository ?? Substitute.For<INotificationDeliveryRepository>(),
            Options.Create(settings ?? new MilestoneNotificationsSettings { CapsMailbox = "caps@example.com" }),
            NullLogger<CapsSummaryService>.Instance);
    }

    private static NotificationRunSummaryCounters DefaultCounters() => new()
    {
        CandidateCount = 5,
        CandidateProjectCount = 5,
        IdentifiedRecipientCount = 3,
        ManagerDeliveryCount = 3,
        ManagerEmailAttemptCount = 2,
        ManagerEmailSentCount = 2,
        ManagerEmailFailedCount = 0,
        ManagerEmailSkippedCount = 1,
        DisabledRecipientCount = 1,
        DiagnosticAvailable = true,
        UnresolvedRecipientCount = 0,
        UnresolvedProjectCount = 0
    };

    // -------------------------------------------------------------------------
    // Constructor guards
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenEmailServiceIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CapsSummaryService(
                null!,
                Substitute.For<INotificationDeliveryRepository>(),
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<CapsSummaryService>.Instance));
        Assert.Equal("emailService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenDeliveryRepositoryIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CapsSummaryService(
                Substitute.For<IEmailService>(),
                null!,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<CapsSummaryService>.Instance));
        Assert.Equal("deliveryRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CapsSummaryService(
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                null!,
                NullLogger<CapsSummaryService>.Instance));
        Assert.Equal("settings", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CapsSummaryService(
                Substitute.For<IEmailService>(),
                Substitute.For<INotificationDeliveryRepository>(),
                Options.Create(new MilestoneNotificationsSettings()),
                null!));
        Assert.Equal("logger", ex.ParamName);
    }

    // -------------------------------------------------------------------------
    // Send attempted unconditionally; outcome recorded either way
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendAndRecordAsync_WhenEmailSendSucceeds_ShouldFinalizeAsSent()
    {
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepository = Substitute.For<INotificationDeliveryRepository>();

        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Sent());

        var service = CreateService(emailService, deliveryRepository);
        var runSummaryId = Guid.NewGuid();

        await service.SendAndRecordAsync(runSummaryId, Guid.NewGuid(), 2026, 7, DefaultCounters(), default);

        await emailService.ReceivedWithAnyArgs(1).SendAsync(default!, default);
        await deliveryRepository.Received(1).FinalizeRunSummaryAsync(
            runSummaryId,
            "Sent",
            null,
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAndRecordAsync_WhenEmailSendFails_ShouldFinalizeAsFailed()
    {
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepository = Substitute.For<INotificationDeliveryRepository>();

        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Failed("Graph API unavailable"));

        var service = CreateService(emailService, deliveryRepository);
        var runSummaryId = Guid.NewGuid();

        // Must not throw — non-fatal.
        await service.SendAndRecordAsync(runSummaryId, Guid.NewGuid(), 2026, 7, DefaultCounters(), default);

        await deliveryRepository.Received(1).FinalizeRunSummaryAsync(
            runSummaryId,
            "Failed",
            "Graph API unavailable",
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAndRecordAsync_WhenEmailServiceThrows_ShouldNotPropagateAndShouldFinalizeAsFailed()
    {
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepository = Substitute.For<INotificationDeliveryRepository>();

        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns<EmailSendResult>(_ => throw new Exception("Unexpected provider error"));

        var service = CreateService(emailService, deliveryRepository);
        var runSummaryId = Guid.NewGuid();

        // Must not throw — non-fatal.
        await service.SendAndRecordAsync(runSummaryId, Guid.NewGuid(), 2026, 7, DefaultCounters(), default);

        await deliveryRepository.Received(1).FinalizeRunSummaryAsync(
            runSummaryId,
            "Failed",
            Arg.Any<string?>(),
            null,
            Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // CapsMailbox not configured — NotAttempted
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendAndRecordAsync_WhenCapsMailboxNotConfigured_ShouldFinalizeAsNotAttempted()
    {
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepository = Substitute.For<INotificationDeliveryRepository>();

        var service = CreateService(
            emailService,
            deliveryRepository,
            new MilestoneNotificationsSettings { CapsMailbox = null });

        var runSummaryId = Guid.NewGuid();

        await service.SendAndRecordAsync(runSummaryId, Guid.NewGuid(), 2026, 7, DefaultCounters(), default);

        // No email send should be attempted.
        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);

        await deliveryRepository.Received(1).FinalizeRunSummaryAsync(
            runSummaryId,
            "NotAttempted",
            Arg.Any<string?>(),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAndRecordAsync_WhenCapsMailboxIsWhitespace_ShouldFinalizeAsNotAttempted()
    {
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepository = Substitute.For<INotificationDeliveryRepository>();

        var service = CreateService(
            emailService,
            deliveryRepository,
            new MilestoneNotificationsSettings { CapsMailbox = "   " });

        await service.SendAndRecordAsync(Guid.NewGuid(), Guid.NewGuid(), 2026, 7, DefaultCounters(), default);

        await emailService.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    // -------------------------------------------------------------------------
    // OperationCanceledException propagates
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendAndRecordAsync_WhenCancelled_ShouldPropagate()
    {
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns<EmailSendResult>(_ => throw new OperationCanceledException());

        var service = CreateService(emailService);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.SendAndRecordAsync(Guid.NewGuid(), Guid.NewGuid(), 2026, 7, DefaultCounters(), default));
    }

    // -------------------------------------------------------------------------
    // Diagnostic-unavailable body variant
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SendAndRecordAsync_WhenDiagnosticUnavailable_ShouldStillSendEmail()
    {
        var emailService = Substitute.For<IEmailService>();
        var deliveryRepository = Substitute.For<INotificationDeliveryRepository>();

        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Sent());

        var counters = DefaultCounters();
        counters.DiagnosticAvailable = false;
        counters.UnresolvedRecipientCount = null;
        counters.UnresolvedProjectCount = null;

        var service = CreateService(emailService, deliveryRepository);

        // Should succeed without throwing even when diagnostic counts are unavailable.
        await service.SendAndRecordAsync(Guid.NewGuid(), Guid.NewGuid(), 2026, 7, counters, default);

        await emailService.ReceivedWithAnyArgs(1).SendAsync(default!, default);
    }
}
