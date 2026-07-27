using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Apha.BatchJobs.Infrastructure.Services.BulkRates;
using Apha.Common.BulkRates.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;

public static class BulkRatesServiceExtensions
{
    public static IServiceCollection AddBulkRatesJobs(
        this IServiceCollection services)
    {
        services.AddScoped<IBulkRatesRepository, BulkRatesRepository>();
        services.AddScoped<IBulkRatesValidationService, BulkRatesValidationService>();
        services.AddScoped<IBulkTestRatesService, BulkTestRatesService>();
        services.AddScoped<IBulkStaffRatesService, BulkStaffRatesService>();
        services.AddScoped<IBulkAnimalRatesService, BulkAnimalRatesService>();

        return services;
    }
}
