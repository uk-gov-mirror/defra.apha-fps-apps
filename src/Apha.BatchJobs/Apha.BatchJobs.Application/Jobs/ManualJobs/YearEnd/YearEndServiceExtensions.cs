using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Microsoft.Extensions.DependencyInjection;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;

/// <summary>
/// Registers the Year End job module services.
/// Step registration order is intentional — the workflow executes steps in the order they are
/// resolved from <see cref="IEnumerable{IYearEndDataSetupStep}"/>.
/// </summary>
public static class YearEndServiceExtensions
{
    public static IServiceCollection AddYearEndJob(
        this IServiceCollection services)
    {
        services.AddScoped<IYearEndDataSetupStep, ValidateYearEndContextStep>();
        services.AddScoped<IYearEndDataSetupStep, ValidateYearScopedSchemaStep>();
        services.AddScoped<IYearEndDataSetupStep, CreatePlannedYearStep>();
        services.AddScoped<IYearEndDataSetupStep, CopyFpsYearScopedTablesStep>();
        services.AddScoped<IYearEndDataSetupStep, CopyMabArchiveYearScopedTablesStep>();
        services.AddScoped<IYearEndDataSetupStep, PeriodSetupStep>();
        services.AddScoped<IYearEndDataSetupStep, ProjectFinancialResetStep>();
        services.AddScoped<IYearEndDataSetupStep, ConfiguredPlanningResetStep>();
        services.AddScoped<IYearEndDataSetupStep, InactiveEmployeeCleanupStep>();
        services.AddScoped<IYearEndDataSetupStep, TargetYearEmptyTablesStep>();
        services.AddScoped<IYearEndDataSetupStep, FinalValidationStep>();

        services.AddScoped<IYearEndDataSetupService, YearEndDataSetupService>();
        services.AddScoped<IYearEndCutoverService, YearEndCutoverService>();

        return services;
    }
}
