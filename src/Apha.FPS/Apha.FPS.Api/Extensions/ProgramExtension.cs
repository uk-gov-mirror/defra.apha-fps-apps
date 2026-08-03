using Amazon;
using Amazon.EventBridge;
using Apha.Common.Utilities.EventPublisher;
using Apha.FPS.Api.Filters;
using Apha.FPS.Api.Mappings;
using Apha.FPS.Api.Middleware;
using Apha.FPS.Application.Mappings;
using Apha.FPS.DataAccess.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Globalization;

namespace Apha.FPS.Api.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.AddDbContext<FpsDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("FPSConnectionString")
                        ,npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorCodesToAdd: null);
                            // Structural safeguard: avoid hanging commands under load
                            npgsqlOptions.CommandTimeout(30);
                        }
                        ), ServiceLifetime.Scoped);
                       
            if (builder.Environment.IsEnvironment("local"))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("RedisConnectionString");
                    options.InstanceName = "RedisInstance";
                });
            }


            // AutoMapper
            services.AddAutoMapper(config =>
            {
                config.AddMaps(typeof(EntityMapper).Assembly);
                config.AddMaps(typeof(RequestMapper));
            });

            // MVC API
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiResponseActionFilter>();
            });

            // API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Application services
            services.AddApplicationServices();

            builder.Services.AddSingleton<IAmazonEventBridge>(_ =>
                new AmazonEventBridgeClient(
                    RegionEndpoint.GetBySystemName(configuration.GetValue<string>("EventBridge:Region"))));

            builder.Services.AddScoped<IEventPublisherService, EventBridgePublisherService>();

            // Authentication
            services.AddAuthenticationServices(configuration);

            // HTTP Context
            services.AddHttpContextAccessor();

            // Health checks
            services.AddHealthChecks();

            // Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FPS API",
                    Version = "v1",
                    Description = "Field Productive System (FPS) Web API"
                });
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                options.CustomSchemaIds(type => type.FullName);
            });
        }

        public static void ConfigureMiddleware(this WebApplication app)
        {
            var env = app.Environment;

            // Set the default culture to en-GB (Great Britain)
            var cultureSet = "en-GB";
            var supportedCultures = new[] { new CultureInfo(cultureSet) };

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(cultureSet),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            app.UseRequestLocalization(localizationOptions);

            // Health checks endpoint
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            // Error handling — must be first to catch exceptions from all downstream middleware
            if (env.IsDevelopment() || env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FPS API v1");
                });
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseMiddleware<RequestContextMiddleware>();
            app.UseAuthorization();

            // Default route
            app.MapControllers();
        }
    }
}