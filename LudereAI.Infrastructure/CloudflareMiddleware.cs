using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure;

public class CloudflareMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CloudflareMiddleware> _logger;
    
    public CloudflareMiddleware(RequestDelegate next, ILogger<CloudflareMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        
        context.Items["Client-IP-Address"] = clientIp;
        context.Connection.RemoteIpAddress = IPAddress.Parse(clientIp);
        
        await _next(context);
    }
    
    private string GetClientIpAddress(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
        {
            return cfIp.ToString();
        }
        
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
        {
            return xff.ToString().Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "";
    }
}