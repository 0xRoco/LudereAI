using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Infrastructure;
using LudereAI.Infrastructure.Gateways;
using LudereAI.Infrastructure.Services;
using LudereAI.Web.Core;
using LudereAI.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    Console.Title = "LudereAI Web";
    // Configure core services
    ConfigureSentry(builder);
    
    builder.Host.UseSerilog((context, config) => 
        config.ReadFrom.Configuration(context.Configuration));
    
    ConfigureServices(builder);
    
    var app = builder.Build();
    
    // Configure logging
    ConfigureLogging(app, args);
    
    // Configure middleware pipeline
    ConfigureMiddleware(app);
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
return;

void ConfigureSentry(WebApplicationBuilder builder)
{
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = builder.Configuration["Sentry:Dsn"];
        o.Environment = builder.Environment.EnvironmentName;
        o.TracesSampleRate = 0.2;
        o.MinimumEventLevel = LogLevel.Error;
        o.MinimumBreadcrumbLevel = LogLevel.Information;
        o.SendDefaultPii = true;
        o.AttachStacktrace = true;

        o.InitializeSdk = builder.Environment.IsProduction();
    });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddControllersWithViews();
    
    ConfigureAuthentication(builder);
    ConfigureHttpClient(builder);
    ConfigureOptions(builder);
    RegisterDependencies(builder);
    
    builder.Services.AddOptions();
    builder.Services.AddHttpContextAccessor();
    
    Log.Information("Services registered");
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdmin", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
    
    Log.Information("Authentication configured");
}

void ConfigureHttpClient(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient("LudereAI", client =>
    {
        var env = builder.Environment.EnvironmentName;
        if (env == "Development")
        {
            client.BaseAddress = new Uri("https://localhost:9099/");
        }
        else if (env == "Staging")
        {
            client.BaseAddress = new Uri("https://api-staging.LudereAI.com/");
        }
        else
        {
            client.BaseAddress = new Uri("https://api.LudereAI.com/");
        }
    }).AddHttpMessageHandler<BearerTokenHandler>();
    
    builder.Services.AddTransient<BearerTokenHandler>();
    
    Log.Information("HTTP client configured");
}

void ConfigureOptions(WebApplicationBuilder builder)
{
    builder.Services.Configure<SystemConfig>(
        builder.Configuration.GetSection("SystemConfig"));  
    
    Log.Information("Options configured");
}

void RegisterDependencies(WebApplicationBuilder builder)
{
    builder.Services.AddTransient<IAPIClient, APIClient>();
    builder.Services.AddTransient<IAuthGateway, AuthGateway>();
    builder.Services.AddTransient<IWaitlistGateway, WaitlistGateway>();
    
    Log.Information("Dependencies registered");
}

void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseSentryTracing();
    app.UseMiddleware<TokenAuthenticationMiddleware>();
    app.UseMiddleware<WaitlistMiddleware>();
    app.UseMiddleware<CloudflareMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapStaticAssets();
    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();
    
    Log.Information("Middleware pipeline configured");
}

void ConfigureLogging(WebApplication app, string[] args)
{
    Log.Information("Logging started at {Now}", DateTime.UtcNow.ToString("u"));
    Log.Information("Log directory: {Directory}", "$LogDirectory$");
    Log.Information("Command line arguments: [ {Args} ]", string.Join(" ", args));
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    
    if (app.Environment.IsDevelopment())
    {
        Log.Warning("Development environment detected. Using development Web URL: {URL}", "https://localhost:8088/");
        Log.Debug("Sentry is disabled in development environment");
    }else if (app.Environment.IsStaging())
    {
        Log.Information("Staging environment detected. Using staging Web URL: {URL}", "https://staging.LudereAI.com/");
    }
    else
    {
        Log.Information("Production environment detected. Using production Web URL: {URL}", "https://LudereAI.com/");
    }
}