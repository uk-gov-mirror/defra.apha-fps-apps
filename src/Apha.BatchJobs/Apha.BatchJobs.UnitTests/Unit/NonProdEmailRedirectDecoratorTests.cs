using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class NonProdEmailRedirectDecoratorTests
{
    [Fact]
    public void Constructor_WhenInnerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new NonProdEmailRedirectDecorator(
                null!,
                Options.Create(new MilestoneNotificationsSettings()),
                NullLogger<NonProdEmailRedirectDecorator>.Instance));

        Assert.Equal("inner", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new NonProdEmailRedirectDecorator(
                Substitute.For<IEmailService>(),
                Options.Create(new MilestoneNotificationsSettings()),
                null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task SendAsync_WhenNullMessage_ShouldThrowArgumentNullException()
    {
        using var envScope = new EnvironmentVariableScope("Development");
        var decorator = CreateDecorator(out _, NonProdSettings());

        await Assert.ThrowsAsync<ArgumentNullException>(() => decorator.SendAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenNonProduction_AndRedirectEnabled_ShouldRedirectToConfiguredRecipients()
    {
        var decorator = CreateDecorator(out var inner, NonProdSettings());
        var message = new EmailMessage(["real.manager@example.com"], "Milestone and Deliverable Update Request", "<p>body</p>");

        await decorator.SendAsync(message, CancellationToken.None);

        await inner.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m =>
                m.To.SequenceEqual(new[] { "test.mailbox@example.com" }) &&
                m.Subject.Contains("real.manager@example.com") &&
                m.Subject.Contains("Milestone and Deliverable Update Request")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenProduction_ShouldNotRedirect_EvenIfEnabled()
    {
        using var envScope = new EnvironmentVariableScope("Production");
        var decorator = CreateDecorator(out var inner, NonProdSettings());
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "<p>body</p>");

        await decorator.SendAsync(message, CancellationToken.None);

        await inner.Received(1).SendAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenNonProduction_AndRedirectDisabled_ShouldPassThroughUnchanged()
    {
        var settings = NonProdSettings();
        settings.NonProdRedirectEnabled = false;
        var decorator = CreateDecorator(out var inner, settings);
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "<p>body</p>");

        await decorator.SendAsync(message, CancellationToken.None);

        await inner.Received(1).SendAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenNonProduction_AndRedirectEnabled_ButNoRecipientsConfigured_ShouldThrow()
    {
        var settings = new MilestoneNotificationsSettings { NonProdRedirectEnabled = true, NonProdRedirectRecipients = [] };
        var decorator = CreateDecorator(out var inner, settings);
        var message = new EmailMessage(["real.manager@example.com"], "Subject", "<p>body</p>");

        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.SendAsync(message, CancellationToken.None));

        await inner.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    private static MilestoneNotificationsSettings NonProdSettings() => new()
    {
        NonProdRedirectEnabled = true,
        NonProdRedirectRecipients = ["test.mailbox@example.com"]
    };

    private static NonProdEmailRedirectDecorator CreateDecorator(out IEmailService inner, MilestoneNotificationsSettings settings)
    {
        inner = Substitute.For<IEmailService>();
        inner.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(EmailSendResult.Sent()));

        return new NonProdEmailRedirectDecorator(inner, Options.Create(settings), NullLogger<NonProdEmailRedirectDecorator>.Instance);
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private const string Name = "ASPNETCORE_ENVIRONMENT";
        private readonly string? _originalValue;

        public EnvironmentVariableScope(string value)
        {
            _originalValue = Environment.GetEnvironmentVariable(Name);
            Environment.SetEnvironmentVariable(Name, value);
        }

        public void Dispose() => Environment.SetEnvironmentVariable(Name, _originalValue);
    }
}
