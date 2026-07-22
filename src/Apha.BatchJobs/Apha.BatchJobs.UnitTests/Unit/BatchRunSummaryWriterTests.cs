using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Lifecycle;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="BatchRunSummaryWriter"/> — exactly one structured summary line per
/// call, at the level matching the outcome, with a human-readable message matching the
/// failure category / cancellation reason.
/// </summary>
public sealed class BatchRunSummaryWriterTests
{
    [Fact]
    public void WriteSummary_OnSuccess_LogsExactlyOneInformationEntry()
    {
        var logger = new RecordingLogger();
        var writer = new BatchRunSummaryWriter(logger);
        var request = new BatchExecutionRequest("RecreateSummary", Apha.BatchJobs.Domain.Enums.RunMode.Manual, Guid.NewGuid(), "arihant", null);
        var jobResult = new Apha.BatchJobs.Application.Interfaces.JobExecutionResult(Guid.NewGuid(), "RecreateSummary", Apha.BatchJobs.Domain.Enums.JobStatus.Completed, TimeSpan.FromSeconds(1), 42);
        var result = BatchExecutionResult.Success(request, jobResult);

        writer.WriteSummary(result, TimeSpan.FromSeconds(1));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains("completed successfully", entry.Message);
    }

    [Fact]
    public void WriteSummary_OnHostShutdownCancellation_LogsWarningWithShutdownMessage()
    {
        var logger = new RecordingLogger();
        var writer = new BatchRunSummaryWriter(logger);
        var result = BatchExecutionResult.Cancelled(request: null, ExecutionCancellationReason.HostShutdown);

        writer.WriteSummary(result, TimeSpan.FromSeconds(5));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("interrupted by host shutdown", entry.Message);
    }

    [Fact]
    public void WriteSummary_OnTimeoutCancellation_LogsWarningWithTimeoutMessage()
    {
        var logger = new RecordingLogger();
        var writer = new BatchRunSummaryWriter(logger);
        var result = BatchExecutionResult.Cancelled(request: null, ExecutionCancellationReason.Timeout);

        writer.WriteSummary(result, TimeSpan.FromSeconds(3600));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("exceeded the configured overall timeout", entry.Message);
    }

    [Fact]
    public void WriteSummary_OnSqlFailure_LogsErrorWithSqlMessage()
    {
        var logger = new RecordingLogger();
        var writer = new BatchRunSummaryWriter(logger);
        var classification = new BatchFailureClassification(20, BatchFailureCategory.Sql, "FPSBatchJobs.SQL_EXCEPTION");
        var result = BatchExecutionResult.Failure(request: null, classification, new InvalidOperationException("db down"));

        writer.WriteSummary(result, TimeSpan.FromSeconds(2));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("SQL error", entry.Message);
    }

    [Fact]
    public void WriteSummary_OnConfigurationFailure_LogsErrorWithConfigurationMessage()
    {
        var logger = new RecordingLogger();
        var writer = new BatchRunSummaryWriter(logger);
        var classification = new BatchFailureClassification(40, BatchFailureCategory.Configuration, "FPSBatchJobs.VALIDATION_EXCEPTION");
        var result = BatchExecutionResult.Failure(request: null, classification, new InvalidOperationException("bad config"));

        writer.WriteSummary(result, TimeSpan.FromMilliseconds(50));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("configuration or validation error", entry.Message);
    }

    [Fact]
    public void WriteSummary_NeverIncludesExceptionObjectOnLogEntry()
    {
        var logger = new RecordingLogger();
        var writer = new BatchRunSummaryWriter(logger);
        var exception = new InvalidOperationException("should not be re-logged here");
        var classification = new BatchFailureClassification(99, BatchFailureCategory.Business, "FPSBatchJobs.GENERAL_EXCEPTION");
        var result = BatchExecutionResult.Failure(request: null, classification, exception);

        writer.WriteSummary(result, TimeSpan.FromSeconds(1));

        var entry = Assert.Single(logger.Entries);
        Assert.Null(entry.Exception);
    }

    private sealed class RecordingLogger : ILogger<BatchRunSummaryWriter>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception), exception));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
