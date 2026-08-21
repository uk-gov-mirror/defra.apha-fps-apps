using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Infrastructure.DependencyInjection;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.UnitTests;

public sealed class ServiceCollectionSetupTests
{
    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Builds the same configuration stack that <c>WorkerHostExtensions</c> uses in
    /// production, then calls <see cref="BatchJobsServiceExtensions.AddBatchJobs"/>.
    /// Replaces the old <c>ServiceCollectionSetup.CreateDefaultServices</c> helper.
    /// </summary>
    private static IServiceCollection CreateServices(string basePath)
    {
        var environment = Apha.BatchJobs.Domain.EnvironmentResolver.GetEnvironmentName("Development");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddBatchInfrastructure(config);
        services.AddBatchJobs(config);
        return services;
    }

    /// <summary>
    /// Builds a minimal in-memory configuration with only the connection string set,
    /// then registers all batch job services via <see cref="BatchJobsServiceExtensions.AddBatchJobs"/>.
    /// Replaces <c>ServiceCollectionSetup.ConfigureBatchJobServices(services, config)</c> call sites.
    /// </summary>
    private static ServiceProvider BuildServiceProvider(Dictionary<string, string?>? extra = null)
    {
        var dict = new Dictionary<string, string?>
        {
            ["ConnectionStrings:FPSConnectionString"] =
                "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD"
        };

        if (extra != null)
            foreach (var (k, v) in extra)
                dict[k] = v;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddBatchInfrastructure(config);
        services.AddBatchJobs(config);
        return services.BuildServiceProvider();
    }

    private static string GetBatchJobsRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var workerPath = Path.Combine(current.FullName, "Apha.BatchJobs.Worker");
            if (Directory.Exists(workerPath) && File.Exists(Path.Combine(workerPath, "appsettings.json")))
                return workerPath;

            current = current.Parent;
        }

        var testDir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && testDir?.Parent != null; i++)
        {
            testDir = testDir.Parent;
            var candidate = Path.Combine(testDir.FullName, "src", "Apha.BatchJobs", "Apha.BatchJobs.Worker");
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;
        }

        throw new DirectoryNotFoundException("Could not locate the Apha.BatchJobs.Worker directory with appsettings.json.");
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _originalValue;

        public EnvironmentVariableScope(string name, string value)
        {
            _name = name;
            _originalValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose() => Environment.SetEnvironmentVariable(_name, _originalValue);
    }

    // ---------------------------------------------------------------------------
    // Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void AddBatchJobs_ShouldRegisterExpectedFoundationServices()
    {
        using var _ = new EnvironmentVariableScope("BATCH_JOB_PARAMETERS_JSON", "{\"month\":\"2026-07\"}");
        var services = CreateServices(GetBatchJobsRoot());
        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<IConfiguration>());

        var jobFactory = serviceProvider.GetRequiredService<IBatchJobFactory>();
        Assert.Contains("HealthCheck", jobFactory.GetAvailableJobs());
        Assert.Equal("HealthCheck", jobFactory.Create("HealthCheck").Name);
        Assert.Contains("YearEndDataSetup", jobFactory.GetAvailableJobs());
        Assert.Contains("YearEndCutover", jobFactory.GetAvailableJobs());
    }

    [Fact]
    public void AddBatchJobs_ShouldRegisterExactlySixSupportedJobs()
    {
        // Regression guard: adding, removing, or deferring a job must force a deliberate update here.
        using var _ = new EnvironmentVariableScope("BATCH_JOB_PARAMETERS_JSON", "{\"month\":\"2026-07\"}");
        var services = CreateServices(GetBatchJobsRoot());
        using var serviceProvider = services.BuildServiceProvider();

        var registeredNames = serviceProvider.GetServices<IBatchJob>()
            .Select(j => j.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            new[] { "BulkAnimalRatesUpdate", "BulkStaffRatesUpdate", "BulkTestRatesUpdate",
                    "HealthCheck", "MABArchive", "RecreateSummary" },
            registeredNames);
    }

    [Fact]
    public void AddBatchJobs_AllRegisteredJobs_ShouldDeclareExplicitIdempotencyStrategy()
    {
        using var _ = new EnvironmentVariableScope("BATCH_JOB_PARAMETERS_JSON", "{\"month\":\"2026-07\"}");
        var services = CreateServices(GetBatchJobsRoot());
        using var serviceProvider = services.BuildServiceProvider();

        var jobs = serviceProvider.GetServices<IBatchJob>().ToList();
        Assert.NotEmpty(jobs);

        foreach (var job in jobs)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(job.IdempotencyStrategy),
                $"Job '{job.Name}' must declare a non-empty idempotency strategy.");
        }
    }

    [Fact]
    public void AddBatchJobs_ManualAdhocJobs_ShouldHaveNoScheduleExpression()
    {
        using var _ = new EnvironmentVariableScope("BATCH_JOB_PARAMETERS_JSON", "{\"month\":\"2026-07\"}");
        var services = CreateServices(GetBatchJobsRoot());
        using var serviceProvider = services.BuildServiceProvider();

        var jobs = serviceProvider.GetServices<IBatchJob>().ToList();

        var manualJobNames = new[] { "HealthCheck", "RecreateSummary",
            "BulkTestRatesUpdate", "BulkStaffRatesUpdate", "BulkAnimalRatesUpdate" };
        foreach (var jobName in manualJobNames)
        {
            var matchingJobs = jobs.Where(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.Single(matchingJobs);
            Assert.Null(matchingJobs[0].ScheduleExpression);
        }
    }

    [Fact]
    public void AddBatchJobs_WhenRecreateSummariesModeIsDotNetLinq_ShouldResolveLinqCatalog()
    {
        using var serviceProvider = BuildServiceProvider();
        var catalog = serviceProvider.GetRequiredService<Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries.IRecreateSummariesStepCatalog>();
        Assert.Equal("RecreateSummariesStepCatalog", catalog.GetType().Name);
    }

    [Fact]
    public void AddBatchJobs_DefaultMabArchiveMode_ShouldRegisterMabArchiveLoadersOnly()
    {
        using var serviceProvider = BuildServiceProvider();
        var loaders = serviceProvider.GetServices<IMabArchiveLoader>().ToList();

        Assert.Equal(24, loaders.Count);
        Assert.All(loaders, l => Assert.EndsWith("Loader", l.GetType().Name, StringComparison.Ordinal));
    }

    [Fact]
    public void AddBatchJobs_WhenMabArchiveModeIsConfigured_ShouldStillRegisterMabArchiveLoadersInExpectedOrder()
    {
        using var serviceProvider = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["BatchJobs:MabArchiveImplementationMode"] = "Sql"
        });

        var loaders = serviceProvider.GetServices<IMabArchiveLoader>().ToList();
        var ordered = loaders.OrderBy(l => l.Sequence).ToList();

        Assert.Equal(24, ordered.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, ordered.Select(l => l.Sequence));
        Assert.Equal(
            new[]
            {
                "my_tlkpprogram",
                "g_tlkpproject",
                "my_tlkpproject",
                "my_fpsyeartotals",
                "my_monthlyoutput",
                "my_monthlytime",
                "my_proj_invoice",
                "my_proj_subcontract",
                "my_projectmonthfinal",
                "my_tbladditionalcosts",
                "my_tblanimalreq",
                "my_tblcontract",
                "my_tblstaffjob",
                "my_timecostcalcs",
                "my_tlkptestreqmt",
                "tlkpyear",
                "my_workgroupgrade",
                "my_profitcentregrade",
                "my_tblprofitcentre",
                "my_testorproduct",
                "my_staff",
                "my_workgroup",
                "my_tblanimals",
                "my_tlkpproject_all"
            },
            ordered.Select(l => l.Name));
        Assert.All(ordered, l => Assert.EndsWith("Loader", l.GetType().Name, StringComparison.Ordinal));
    }

    [Fact]
    public void AddBatchJobs_ShouldRegisterYearEndDataSetupStepsInExpectedOrder()
    {
        using var serviceProvider = BuildServiceProvider();
        var steps = serviceProvider.GetServices<IYearEndDataSetupStep>().ToList();

        Assert.Equal(
            new[]
            {
                "ValidateYearEndContextStep",
                "ValidateYearScopedSchemaStep",
                "CreatePlannedYearStep",
                "CopyFpsYearScopedTablesStep",
                "CopyMabArchiveYearScopedTablesStep",
                "PeriodSetupStep",
                "ProjectFinancialResetStep",
                "ConfiguredPlanningResetStep",
                "InactiveEmployeeCleanupStep",
                "TargetYearEmptyTablesStep",
                "FinalValidationStep"
            },
            steps.Select(s => s.Name));
    }

    [Fact]
    public void AddBatchJobs_ShouldRegisterEmailTemplateRenderer()
    {
        using var serviceProvider = BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetRequiredService<IEmailTemplateRenderer>());
    }

    [Fact]
    public void AddBatchJobs_WhenGraphEmailSettingsMissing_RegistrationSucceeds_ButResolvingEmailServiceThrows()
    {
        // No GraphEmailSettings section - expected in every environment until a live Graph send
        // is authorised. Registration and BuildServiceProvider must not throw; only resolving
        // IEmailService itself should fail, and only at that point.

        using var serviceProvider = BuildServiceProvider();
        var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IEmailService>());
        Assert.Contains("GraphEmailSettings", ex.Message);
    }
}
