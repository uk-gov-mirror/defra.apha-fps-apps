using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-action")]
    public class FpsYearAnchorTagHelper : AnchorTagHelper
    {
        private readonly IFpsYearContext _fy;

        public FpsYearAnchorTagHelper(
            IHtmlGenerator generator,
            IFpsYearContext fy)
            : base(generator)
        {
            _fy = fy;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Get the current page's area from route data
            var currentArea = ViewContext?.RouteData?.Values["area"]?.ToString();

            // Only add year parameter if we're currently IN one of the application areas
            // Skip for landing page (no area) or other non-app pages
            if (string.IsNullOrEmpty(currentArea) || 
                (!string.Equals(currentArea, "FPS", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(currentArea, "PACT", StringComparison.OrdinalIgnoreCase)))
                return;

            var href = output.Attributes["href"]?.Value?.ToString();

            if (string.IsNullOrEmpty(href) || href.Contains("year="))
                return;

            var separator = href.Contains('?') ? "&" : "?";
            output.Attributes.SetAttribute("href", $"{href}{separator}year={_fy.Year}");

        }
    }
}
