namespace LudereAI.Web.Core;

public class WaitlistMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
{
    private bool _isWaitlistActive;
    
    private readonly List<string> _allowedPaths = new()
    {
        "/Waitlist",
        "/css",
        "/js",
    };
    
    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true || !_isWaitlistActive)
        {
            await next(context);
            return;
        }
        
        var path = context.Request.Path;
        
        var isAllowed = _allowedPaths.Any(p => path.StartsWithSegments(p));

        if (!isAllowed)
        {
            context.Response.Redirect("/Waitlist");
        }
        else
        {
            await next(context);
        }
    }
}