using System.Net.Sockets;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Apha.BatchJobs.Application.FailureHandling;

/// <summary>
/// Single source of truth for mapping a non-cancellation exception to an exit code, failure
/// category, and CloudWatch <c>ErrorType</c> marker. Used by both <see cref="JobOrchestrator"/>
/// (which logs the marker once, at the point of failure, before re-throwing) and the Worker's
/// run summary (which reuses the same classification without re-logging the exception).
/// <c>OperationCanceledException</c> is handled separately by cancellation-context logic, not
/// this classifier.
/// </summary>
public sealed class BatchFailureClassifier
{
    private readonly IConfiguration _configuration;

    public BatchFailureClassifier(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>Classifies a non-cancellation exception. Do not pass <see cref="OperationCanceledException"/>.</summary>
    public BatchFailureClassification Classify(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            JobValidationException => Build(BatchExitCodes.ConfigurationFailure, BatchFailureCategory.Configuration, MarkerKey.Validation),
            MabArchiveYearConfigurationException => Build(BatchExitCodes.ConfigurationFailure, BatchFailureCategory.Configuration, MarkerKey.General),
            NotificationSettingsConfigurationException => Build(BatchExitCodes.ConfigurationFailure, BatchFailureCategory.Configuration, MarkerKey.General),
            JobLockException => Build(BatchExitCodes.LockFailure, BatchFailureCategory.Concurrency, MarkerKey.Concurrency),
            BusinessEmailException => Build(BatchExitCodes.EmailFailure, BatchFailureCategory.Email, MarkerKey.General),
            _ => ClassifyByExceptionChain(exception)
        };
    }

    private BatchFailureClassification ClassifyByExceptionChain(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            switch (current)
            {
                case PostgresException or DbUpdateException:
                    return Build(BatchExitCodes.DatabaseFailure, BatchFailureCategory.Sql, MarkerKey.Sql);
                case NpgsqlException or SocketException:
                    return Build(BatchExitCodes.DatabaseFailure, BatchFailureCategory.DependencyOutage, MarkerKey.Sql);
                case TimeoutException:
                    return Build(BatchExitCodes.DatabaseFailure, BatchFailureCategory.Timeout, MarkerKey.General);
                case UnauthorizedAccessException:
                    return Build(BatchExitCodes.UnhandledFailure, BatchFailureCategory.Authorization, MarkerKey.General);
            }
        }

        return Build(BatchExitCodes.UnhandledFailure, BatchFailureCategory.Business, MarkerKey.General);
    }

    private BatchFailureClassification Build(int exitCode, BatchFailureCategory category, MarkerKey markerKey)
    {
        var marker = markerKey switch
        {
            MarkerKey.Sql => _configuration["ExceptionTypes:Sql"] ?? "FPSBatchJobs.SQL_EXCEPTION",
            MarkerKey.Concurrency => _configuration["ExceptionTypes:Concurrency"] ?? "FPSBatchJobs.CONCURRENCY_EXCEPTION",
            MarkerKey.Validation => _configuration["ExceptionTypes:Validation"] ?? "FPSBatchJobs.VALIDATION_EXCEPTION",
            _ => _configuration["ExceptionTypes:General"] ?? "FPSBatchJobs.GENERAL_EXCEPTION"
        };

        return new BatchFailureClassification(exitCode, category, marker);
    }

    private enum MarkerKey
    {
        General,
        Sql,
        Concurrency,
        Validation
    }
}
