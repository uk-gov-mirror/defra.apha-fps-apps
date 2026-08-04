using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Enums;
using Apha.FPSApps.Web.Handler;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Middleware
{
    public class FpsYearMiddleware
    {
        private readonly RequestDelegate _next;        
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public FpsYearMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            IFpsYearContext fyContext,
            IYearMasterService yearMasterService,
            IAppStateService appStateService)
        {
            var area = context.GetRouteData()?.Values["area"]?.ToString();
            var path = context.Request.Path.Value;

            if ((path is not null && Path.HasExtension(path))
                || !(string.Equals(area, "FPS", StringComparison.OrdinalIgnoreCase)
                || string.Equals(area, "PACT", StringComparison.OrdinalIgnoreCase))
                )
            {
                await _next(context);
                return;
            }

            var tempYear = GetCurrentFPSYear();
            context.Items["SelectedFPSYear"] = tempYear;

            int year = tempYear;
            YearStatus yearStatus = YearStatus.Closed;

            try
            {
                var allYears = await GetCachedYearsAsync(yearMasterService, appStateService);

                if (allYears != null)
                {
                    year = ResolveYear(context, allYears, tempYear);

                    var selectedYear = allYears.FirstOrDefault(y => y.FpsYear == year);
                    yearStatus = selectedYear?.YearStatus is null
                        ? YearStatus.Closed
                        : Enum.Parse<YearStatus>(selectedYear.YearStatus, ignoreCase: true);
                }
            }
            catch (Exception ex) when (
                ex is MicrosoftIdentityWebChallengeUserException ||
                ex.InnerException is MicrosoftIdentityWebChallengeUserException)
            {
                // Token expired or re-consent required — let it propagate
                // so OIDC middleware can redirect the user back to Azure AD
                throw;
            }
            catch
            {
                // All other failures (network, API down, etc.) — fall back silently
                year = tempYear;
            }

            fyContext.Year = year;
            context.Items["SelectedFPSYear"] = year;

            fyContext.IsReadOnly = ResolveIsReadOnly(area, yearStatus);

            await _next(context);
        }

        private static async Task<IEnumerable<YearMasterDto>?> GetCachedYearsAsync(
            IYearMasterService yearMasterService,
            IAppStateService appStateService)
        {
            var cached = await appStateService.GetCacheValueAsync<IEnumerable<YearMasterDto>>(FpsCacheKeys.AllYears);
            if (cached != null)
                return cached;

            var response = await yearMasterService.GetAllFpsYearsAsync();
            var data = response?.Data;

            if (data != null)
                await appStateService.SetCacheValueAsync(FpsCacheKeys.AllYears, data, CacheDuration);

            return data;
        }

        private static int ResolveYear(HttpContext context, IEnumerable<YearMasterDto> allYears, int fallback)
        {
            if (context.Request.Query.TryGetValue("year", out var q) && !string.IsNullOrEmpty(q))
                return int.Parse(q!);

            if (context.Request.Headers.TryGetValue("X-FPS-Year", out var h) && !string.IsNullOrEmpty(h))
                return int.Parse(h!);

            if (context.Request.HasFormContentType &&
                context.Request.Form.TryGetValue("FPSYear", out var f) && !string.IsNullOrEmpty(f))
                return int.Parse(f!);

            return allYears.FirstOrDefault(y => y.YearStatus?.Equals("Open", StringComparison.OrdinalIgnoreCase) == true)
                           ?.FpsYear ?? fallback;
        }

        private static bool ResolveIsReadOnly(string? area, YearStatus yearStatus) =>
            area?.ToUpperInvariant() switch
            {
                "FPS" => yearStatus is not (YearStatus.Planned or YearStatus.Open),
                "PACT" => yearStatus != YearStatus.Open,
                _ => false
            };

        private static int GetCurrentFPSYear()
        {
            var today = DateTime.Today;
            return today.Month >= 4 ? today.Year : today.Year - 1;
        }
    }
}
