using LudereAI.Web.Models;
using Microsoft.Extensions.Options;

namespace LudereAI.Web.Core;

public class WaitlistMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger, IOptions<SystemConfig> options)
{
    private readonly SystemConfig _config = options.Value;
    
    private readonly List<string> _allowedPaths = new()
    {
        "/Waitlist",
        "/css",
        "/js",
        "/images"
    };
    
    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true || !_config.GeneralSettings.IsWaitlistActive)
        {
            await next(context);
            return;
        }
        
        var path = context.Request.Path;
        
        var isAllowed = _allowedPaths.Any(p => path.StartsWithSegments(p));

        if (!isAllowed)
        {
            //context.Response.Redirect("https://ludereai.com/");
            context.Response.Redirect("/Waitlist");
        }
        else
        {
            await next(context);
        }
    }
}