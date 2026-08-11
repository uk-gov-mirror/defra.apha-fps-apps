using Amazon;
using Amazon.S3;
using Apha.FPSApps.Infrastructure.Mappings;
using Apha.FPSApps.Web.Mappings;
using Apha.FPSApps.Web.Middleware;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Apha.FPSApps.Web.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

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

            services.AddSession(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "VIR.Session";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // AutoMapper  
            services.AddAutoMapper(config =>
            {
                config.AddMaps(typeof(FpsApiDtoMapper).Assembly);
                config.AddMaps(typeof(PactApiDtoMapper).Assembly);
                config.AddMaps(typeof(CostbookApiDtoMapper).Assembly);
                config.AddMaps(typeof(PimsApiDtoMapper).Assembly);
                config.AddMaps(typeof(FpsViewModelMapper));
                config.AddMaps(typeof(PactViewModelMapper));
                config.AddMaps(typeof(CostbookViewModelMapper));
                config.AddMaps(typeof(PimsViewModelMapper));
            });

            // HTTP Context
            services.AddHttpContextAccessor();

            // MVC
            services.AddControllersWithViews();

            // Authentication
            services.AddAuthenticationServices(configuration, builder.Environment);

            //API clients
            services.AddApiClient(builder.Configuration);

            // Application services
            services.AddApplicationServices();

            // AWS S3 client
            var regionName = configuration["S3Storage:Region"]
                ?? throw new InvalidOperationException("S3Storage:Region is not configured.");

            services.AddSingleton<IAmazonS3>(_ =>
            {
                var region = RegionEndpoint.GetBySystemName(regionName);
                return new AmazonS3Client(region);
            });

            // In-memory cache (used by FpsYearMiddleware)
            services.AddMemoryCache();

            // Configure forwarded headers for proxy/load balancer support
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Health checks
            services.AddHealthChecks();
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
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHsts();
            app.UseHttpsRedirection();

            // Use forwarded headers - must be before authentication
            app.UseForwardedHeaders();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            // FpsYearMiddleware must run after authentication to access API with bearer token
            app.UseMiddleware<FpsYearMiddleware>();

            // Default route
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}