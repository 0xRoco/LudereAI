using Microsoft.AspNetCore.Authentication;

namespace LudereAI.Web.Core;

public class TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
{
    private readonly List<string> _restrictedPaths = new()
    {
        "/Account",
    };

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Cookies["token"];
        var path = context.Request.Path;
        
        var isLoggedIn = !string.IsNullOrWhiteSpace(token);


        if (!isLoggedIn && _restrictedPaths.Any(p => path.StartsWithSegments(p)))
        {
            // We log out the user if the token is missing, but ideally we would try to refresh the token if the auth cookie is still valid
            //TODO: Refresh token if auth cookie is still valid

            context.Response.Cookies.Delete("token");
            await context.SignOutAsync();
            context.Response.Redirect("/Login");
        }
        else
        {
            await next(context);
        }
    }
}