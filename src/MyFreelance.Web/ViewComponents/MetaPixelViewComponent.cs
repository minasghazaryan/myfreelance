using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.ViewComponents;

public class MetaPixelViewComponent : ViewComponent
{
    public const string PixelId = "1661311945626039";

    public static readonly string Script = $$"""
        <!-- Meta Pixel Code -->
        <script>
        !function(f,b,e,v,n,t,s)
        {if(f.fbq)return;n=f.fbq=function(){n.callMethod?
        n.callMethod.apply(n,arguments):n.queue.push(arguments)};
        if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
        n.queue=[];t=b.createElement(e);t.async=!0;
        t.src=v;s=b.getElementsByTagName(e)[0];
        s.parentNode.insertBefore(t,s)}(window, document,'script',
        'https://connect.facebook.net/en_US/fbevents.js');
        fbq('init', '{{PixelId}}');
        fbq('track', 'PageView');
        </script>
        <noscript><img height="1" width="1" style="display:none"
        src="https://www.facebook.com/tr?id={{PixelId}}&ev=PageView&noscript=1"
        /></noscript>
        <!-- End Meta Pixel Code -->
        """;

    public IViewComponentResult Invoke()
    {
        if (!HttpContext.Request.Host.Host.Equals(RegistrationSources.PromoHost, StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        var path = HttpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        return new HtmlContentViewComponentResult(new HtmlString(Script));
    }
}
