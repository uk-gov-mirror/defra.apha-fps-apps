using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailNotificationServiceTests
{
    private static readonly IOptions<AwsLoggingSettings> DefaultAwsLoggingSettings =
        Options.Create(new AwsLoggingSettings { LogGroupName = "batchjobs-log-group" });

    // Used wherever the send path must never be reached — proves EmailNotificationService
    // resolves IEmailService lazily (only once a notification is actually about to send), not
    // eagerly on construction. If this ever fires, that laziness guarantee has regressed.
    private static Func<IEmailService> ThrowingEmailServiceFactory =>
        () => throw new InvalidOperationException("IEmailService should not have been resolved");

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailNotificationService(
                null!,
                Options.Create(new BatchAlertingSettings()),
                DefaultAwsLoggingSettings,
                ThrowingEmailServiceFactory));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenSettingsAreNull_ShouldUseDefaultsAndReturn()
    {
        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance, settings: null!, awsLoggingSettings: null!, ThrowingEmailServiceFactory);

        await service.SendFailureNotificationAsync("cid-null-settings", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenNotificationsDisabled_ShouldReturnWithoutResolvingEmailService()
    {
        var settings = Options.Create(new BatchAlertingSettings
        {
            EnableEmailNotifications = false,
            AdminNotificationEmail = "alerts@example.com"
        });

        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance, settings, DefaultAwsLoggingSettings, ThrowingEmailServiceFactory);

        await service.SendFailureNotificationAsync("cid-1", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenAdminEmailMissing_ShouldReturnWithoutResolvingEmailService()
    {
        var settings = Options.Create(new BatchAlertingSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "   "
        });

        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance, settings, DefaultAwsLoggingSettings, ThrowingEmailServiceFactory);

        await service.SendFailureNotificationAsync("cid-2", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenEnabledAndConfigured_SendsThroughEmailService()
    {
        var settings = Options.Create(new BatchAlertingSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "alerts@example.com"
        });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>()).Returns(EmailSendResult.Sent());

        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance, settings, DefaultAwsLoggingSettings, () => emailService);

        await service.SendFailureNotificationAsync("cid-3", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);

        await emailService.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To.Single() == "alerts@example.com" && m.Subject.Contains("MABArchive")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenEmailServiceReportsFailure_LogsWarningWithoutThrowing()
    {
        var settings = Options.Create(new BatchAlertingSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "alerts@example.com"
        });
        var emailService = Substitute.For<IEmailService>();
        emailService.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Failed("graph unavailable"));

        var service = new EmailNotificationService(
            NullLogger<EmailNotificationService>.Instance, settings, DefaultAwsLoggingSettings, () => emailService);

        await service.SendFailureNotificationAsync("cid-4", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenLoggerThrowsInTryBlock_ShouldRethrow()
    {
        var settings = Options.Create(new BatchAlertingSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "alerts@example.com"
        });

        var service = new EmailNotificationService(new ThrowOnInfoLogger(), settings, DefaultAwsLoggingSettings, ThrowingEmailServiceFactory);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendFailureNotificationAsync("cid-logger-throw", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None));

        // Confirms the logger's own failure is what propagated, not the (deliberately throwing)
        // email factory — proves the failure happened before the email send was even attempted.
        Assert.Equal("logger write failed", ex.Message);
    }

    private sealed class ThrowOnInfoLogger : ILogger<EmailNotificationService>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                throw new InvalidOperationException("logger write failed");
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
