using System.Net.Sockets;
using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="BatchFailureClassifier"/> — the single source of truth for mapping a
/// non-cancellation exception to an exit code, failure category, and CloudWatch
/// <c>ErrorType</c> marker, shared by <see cref="Apha.BatchJobs.Application.JobOrchestrator"/>
/// and the Worker's run summary.
/// </summary>
public sealed class BatchFailureClassifierTests
{
    // A real ConfigurationRoot (not a mock) so the indexer's actual "null when key is absent"
    // behavior is exercised — an unconfigured IConfiguration substitute returns string.Empty
    // for unconfigured members, not null, which would silently defeat the `??` fallback below.
    private static BatchFailureClassifier CreateClassifier(string? json = null)
    {
        var builder = new ConfigurationBuilder();
        if (json is not null)
        {
            builder.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
        }

        return new BatchFailureClassifier(builder.Build());
    }

    [Fact]
    public void Classify_JobValidationException_MapsToConfigurationFailureWithValidationMarker()
    {
        // Marker is "Validation" (not "General") to match JobOrchestrator.ThrowWithStructuredLog's
        // existing behavior for JobValidationException thrown during job execution — centralizing
        // classification must not silently change the marker for exceptions already flowing
        // through the orchestrator today.
        var result = CreateClassifier().Classify(new JobValidationException("bad input"));

        Assert.Equal(BatchExitCodes.ConfigurationFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Configuration, result.Category);
        Assert.Equal("FPSBatchJobs.VALIDATION_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_MabArchiveYearConfigurationException_MapsToConfigurationFailure()
    {
        var result = CreateClassifier().Classify(new MabArchiveYearConfigurationException("bad year config"));

        Assert.Equal(BatchExitCodes.ConfigurationFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Configuration, result.Category);
        Assert.Equal("FPSBatchJobs.GENERAL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_JobLockException_MapsToLockFailure()
    {
        var result = CreateClassifier().Classify(new JobLockException("lock held"));

        Assert.Equal(BatchExitCodes.LockFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Concurrency, result.Category);
        Assert.Equal("FPSBatchJobs.CONCURRENCY_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_BusinessEmailException_MapsToEmailFailureButGeneralMarker()
    {
        var result = CreateClassifier().Classify(new BusinessEmailException("smtp failure"));

        Assert.Equal(BatchExitCodes.EmailFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Email, result.Category);
        Assert.Equal("FPSBatchJobs.GENERAL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_PostgresException_MapsToSql()
    {
        var postgresException = new PostgresException("syntax error", "ERROR", "ERROR", "42601");

        var result = CreateClassifier().Classify(postgresException);

        Assert.Equal(BatchExitCodes.DatabaseFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Sql, result.Category);
        Assert.Equal("FPSBatchJobs.SQL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_DbUpdateException_MapsToSql()
    {
        var result = CreateClassifier().Classify(new DbUpdateException("save failed"));

        Assert.Equal(BatchExitCodes.DatabaseFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Sql, result.Category);
        Assert.Equal("FPSBatchJobs.SQL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_NpgsqlExceptionThatIsNotPostgresException_MapsToDependencyOutage()
    {
        var result = CreateClassifier().Classify(new NpgsqlException("connection refused"));

        Assert.Equal(BatchExitCodes.DatabaseFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.DependencyOutage, result.Category);
        Assert.Equal("FPSBatchJobs.SQL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_SocketException_MapsToDependencyOutage()
    {
        var result = CreateClassifier().Classify(new SocketException());

        Assert.Equal(BatchExitCodes.DatabaseFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.DependencyOutage, result.Category);
        Assert.Equal("FPSBatchJobs.SQL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_TimeoutException_MapsToTimeout()
    {
        var result = CreateClassifier().Classify(new TimeoutException("exceeded runtime timeout"));

        Assert.Equal(BatchExitCodes.DatabaseFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Timeout, result.Category);
        Assert.Equal("FPSBatchJobs.GENERAL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_UnauthorizedAccessException_MapsToAuthorization()
    {
        var result = CreateClassifier().Classify(new UnauthorizedAccessException("denied"));

        Assert.Equal(BatchExitCodes.UnhandledFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Authorization, result.Category);
        Assert.Equal("FPSBatchJobs.GENERAL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_UnrecognisedException_MapsToBusinessUnhandled()
    {
        var result = CreateClassifier().Classify(new InvalidOperationException("unexpected"));

        Assert.Equal(BatchExitCodes.UnhandledFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Business, result.Category);
        Assert.Equal("FPSBatchJobs.GENERAL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_WrappedPostgresException_WalksInnerExceptionChainToSql()
    {
        var postgresException = new PostgresException("syntax error", "ERROR", "ERROR", "42601");
        var wrapper = new InvalidOperationException("wrapper", postgresException);

        var result = CreateClassifier().Classify(wrapper);

        Assert.Equal(BatchExitCodes.DatabaseFailure, result.ExitCode);
        Assert.Equal(BatchFailureCategory.Sql, result.Category);
        Assert.Equal("FPSBatchJobs.SQL_EXCEPTION", result.ErrorType);
    }

    [Fact]
    public void Classify_UsesConfiguredMarkerOverrideWhenPresent()
    {
        var classifier = CreateClassifier("""{"ExceptionTypes":{"Sql":"Custom.SQL_MARKER"}}""");

        var result = classifier.Classify(new DbUpdateException("save failed"));

        Assert.Equal("Custom.SQL_MARKER", result.ErrorType);
    }
}
