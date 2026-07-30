using Amazon;
using Amazon.EventBridge;
using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.EventPublisher;
using Apha.PACT.Api.Filters;
using Apha.PACT.Api.Mappings;
using Apha.PACT.Api.Middleware;
using Apha.PACT.Application.Mappings;
using Apha.PACT.DataAccess.Data;
using Asp.Versioning;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.OpenApi;
using System.Globalization;

namespace Apha.PACT.Api.Extensions
{
    public static class ProgramExtension
    {
        private static readonly string[] GraphScopes = ["https://graph.microsoft.com/.default"];

        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Add database context
            services.AddDbContext<FpsDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("FPSConnectionString")
                         , npgsqlOptions =>
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

            // MS Graph Email
            var graphSettings = configuration.GetRequiredSection("GraphEmailSettings").Get<GraphEmailSettings>()
                ?? throw new InvalidOperationException("GraphEmailSettings configuration section is missing or could not be bound.");

            if (string.IsNullOrWhiteSpace(graphSettings.TenantId))
                throw new InvalidOperationException("GraphEmailSettings:TenantId is required but was not configured.");
            if (string.IsNullOrWhiteSpace(graphSettings.ClientId))
                throw new InvalidOperationException("GraphEmailSettings:ClientId is required but was not configured.");
            if (string.IsNullOrWhiteSpace(graphSettings.ClientSecret))
                throw new InvalidOperationException("GraphEmailSettings:ClientSecret is required but was not configured.");

            services.AddSingleton<GraphServiceClient>(_ =>
            {
                var credential = new ClientSecretCredential(
                    graphSettings.TenantId,
                    graphSettings.ClientId,
                    graphSettings.ClientSecret);

                return new GraphServiceClient(
                    credential,
                    GraphScopes);
            });

            // Application services
            services.AddOptions<WorkGroupReportEmailSettings>()
                .Bind(configuration.GetRequiredSection(WorkGroupReportEmailSettings.SectionName))
                .ValidateOnStart();
            
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
                    Title = "PACT API",
                    Version = "v1",
                    Description = "PACT Web API"
                });
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

            // Error handling
            if (env.IsDevelopment() || env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PACT API v1");
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