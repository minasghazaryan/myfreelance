using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Middleware;

public class GhanaVisitorGateMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    private const string PromoHost = "promo.africa-usainvest.com";
    private const string FillerPage = """
        <!DOCTYPE html>
        <html lang="ru">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="robots" content="noindex">
            <title>Цветы</title>
            <style>
                body { margin: 0; background: #f7f4ef; color: #2c2c2c; font-family: Georgia, "Times New Roman", serif; }
                main { max-width: 40rem; margin: 0 auto; padding: 3rem 1.25rem 4rem; }
                h1 { font-weight: normal; font-size: 2.4rem; margin-bottom: 0.25rem; }
                p.meta { color: #777; font-family: sans-serif; font-size: 0.85rem; margin-top: 0; }
                p { line-height: 1.7; font-size: 1.05rem; }
            </style>
        </head>
        <body>
            <main>
                <h1>Цветы</h1>
                <p class="meta">Заметки с подоконника</p>
                <p>Утром на кухне пахло мокрой землёй. Кто-то опять перелил герань, и вода стояла в поддоне тихим коричневым озером. На окне, кроме герани, жили фиалка с одним упрямым листом и кактус, который цвести не собирался уже третий год.</p>
                <p>Сосед снизу говорит, что розы надо обрезать в марте, сосед сверху говорит, что в апреле, а тётя Нина с рынка говорит, что розы вообще лучше не трогать, пока сами не попросят. В этом году они попросили в виде трёх кривых веток и одной жёлтой бутоньерки, похожей на скомканную салфетку.</p>
                <p>Тюльпаны из ведра у метро простояли четыре дня и легли на стол, как будто устали стоять в транспорте. Ландыши в стакане пахли сильнее, чем обещала этикетка. Одуванчики во дворе никто не считает цветами, пока их не поставят в банку из-под варенья.</p>
                <p>Если поливать всё подряд и никого не спрашивать, к маю на подоконнике становится тесно. Это не сад и не статья. Это просто цветы, которые решили пожить здесь ещё немного.</p>
            </main>
        </body>
        </html>
        """;

    private static readonly string[] AllowedPaths =
    [
        "/account/login",
        "/account/logout",
        "/account/accessdenied",
        "/account/forgotpassword",
        "/account/forgotpasswordconfirmation"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsPromoHost(context))
        {
            await next(context);
            return;
        }

        if (ShouldShowSite(context))
        {
            context.Response.OnStarting(() =>
            {
                AppendVary(context);
                return Task.CompletedTask;
            });
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";
        AppendVary(context);
        await context.Response.WriteAsync(FillerPage);
    }

    private static bool IsPromoHost(HttpContext context) =>
        context.Request.Host.Host.Equals(PromoHost, StringComparison.OrdinalIgnoreCase);

    private bool ShouldShowSite(HttpContext context)
    {
        if (context.User.IsInRole(AppRoles.Admin) || context.User.IsInRole(AppRoles.AdminReadOnly))
            return true;

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var allowed in AllowedPaths)
        {
            if (path.Equals(allowed, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var country = context.Request.Headers["CF-IPCountry"].ToString().Trim();
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(country))
            return true;

        return country.Equals("GH", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendVary(HttpContext context)
    {
        var vary = context.Response.Headers.Vary.ToString();
        if (vary.Contains("CF-IPCountry", StringComparison.OrdinalIgnoreCase))
            return;

        context.Response.Headers.Append("Vary", "CF-IPCountry");
    }
}
