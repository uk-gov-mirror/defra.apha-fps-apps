using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("fps-year-script")]
    public class FpsYearScriptTagHelper : TagHelper
    {
        private readonly IFpsYearContext _fy;

        public FpsYearScriptTagHelper(IFpsYearContext fy)
        {
            _fy = fy;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent($@"
        window.FPS_YEAR = {_fy.Year};
                jQuery(document).ajaxSend(function (e, xhr) {{
                    xhr.setRequestHeader('X-FPS-Year', window.FPS_YEAR);
                }});
                (function () {{
                    if (!window.fetch || window.fetch.__fpsYearWrapped) {{
                        return;
                    }}
                    var nativeFetch = window.fetch.bind(window);
                    var wrappedFetch = function (input, init) {{
                        var headers = new Headers(
                            (init && init.headers) ||
                            (input instanceof Request ? input.headers : undefined)
                        );
                        if (!headers.has('X-FPS-Year')) {{
                            headers.set('X-FPS-Year', window.FPS_YEAR);
                        }}
                        init = init || {{}};
                        init.headers = headers;
                        return nativeFetch(input, init);
                    }};
                    wrappedFetch.__fpsYearWrapped = true;
                    window.fetch = wrappedFetch;
                }})();
                window.fpsNavigateTo = function (url) {{
                    var separator = url.indexOf('?') !== -1 ? '&' : '?';
                    window.location.href = url + separator + 'year=' + window.FPS_YEAR;
                }};
            ");
        }
    }
}

