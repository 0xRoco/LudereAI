using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace LudereAI.Web.Core;

public class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext == null) return await base.SendAsync(request, cancellationToken);

        var token = httpContextAccessor.HttpContext.Request.Cookies["token"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            await httpContextAccessor.HttpContext.SignOutAsync();
        }

        return await base.SendAsync(request, cancellationToken);
    }
}