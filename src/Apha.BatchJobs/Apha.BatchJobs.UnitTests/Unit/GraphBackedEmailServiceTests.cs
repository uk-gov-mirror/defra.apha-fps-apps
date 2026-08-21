using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.BatchJobs.UnitTests;

public sealed class GraphBackedEmailServiceTests
{
    [Fact]
    public void Constructor_WhenGraphEmailServiceIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new GraphBackedEmailService(null!, NullLogger<GraphBackedEmailService>.Instance));

        Assert.Equal("graphEmailService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new GraphBackedEmailService(Substitute.For<IGraphEmailService>(), null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task SendAsync_WhenNullMessage_ShouldThrowArgumentNullException()
    {
        var service = new GraphBackedEmailService(Substitute.For<IGraphEmailService>(), NullLogger<GraphBackedEmailService>.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenGraphCallSucceeds_ShouldReturnSentResult_AndMapFields()
    {
        var graphEmailService = Substitute.For<IGraphEmailService>();
        var service = new GraphBackedEmailService(graphEmailService, NullLogger<GraphBackedEmailService>.Instance);
        var message = new EmailMessage(["jane@example.com"], "Test subject", "<p>Body</p>");

        var result = await service.SendAsync(message, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Null(result.FailureMessage);
        await graphEmailService.Received(1).SendEmailAsync(
            Arg.Is<EmailMessageModel>(m =>
                m.To.SequenceEqual(message.To) &&
                m.Subject == message.Subject &&
                m.Body == message.HtmlBody &&
                m.IsBodyHtml),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenGraphCallThrows_ShouldReturnFailedResult_NotThrow()
    {
        var graphEmailService = Substitute.For<IGraphEmailService>();
        graphEmailService
            .SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Graph unavailable"));
        var service = new GraphBackedEmailService(graphEmailService, NullLogger<GraphBackedEmailService>.Instance);
        var message = new EmailMessage(["jane@example.com"], "Test subject", "<p>Body</p>");

        var result = await service.SendAsync(message, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Graph unavailable", result.FailureMessage);
    }

    [Fact]
    public async Task SendAsync_WhenCancelled_ShouldPropagateOperationCanceledException()
    {
        var graphEmailService = Substitute.For<IGraphEmailService>();
        graphEmailService
            .SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var service = new GraphBackedEmailService(graphEmailService, NullLogger<GraphBackedEmailService>.Instance);
        var message = new EmailMessage(["jane@example.com"], "Test subject", "<p>Body</p>");

        await Assert.ThrowsAsync<OperationCanceledException>(() => service.SendAsync(message, CancellationToken.None));
    }
}
