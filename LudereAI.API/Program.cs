using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using LudereAI.API.Core;
using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Mappers;
using LudereAI.Domain.Models.AI;
using LudereAI.Domain.Models.Configs;
using LudereAI.Domain.Models.Features;
using LudereAI.Domain.Models.Tiers;
using LudereAI.Infrastructure;
using LudereAI.Infrastructure.Repositories;
using LudereAI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Scalar.AspNetCore;
using Serilog;
using Stripe;
using Stripe.Checkout;
using AccountService = LudereAI.Infrastructure.Services.AccountService;
using SubscriptionService = LudereAI.Infrastructure.Services.SubscriptionService;

var builder = WebApplication.CreateBuilder(args);

try
{
    Console.Title = "LudereAI API";
    
    // Configure Sentry
    ConfigureSentry(builder);
    
    // Configure Serilog
    builder.Host.UseSerilog((context, config) => 
        config.ReadFrom.Configuration(context.Configuration));
    
    // Add services to container
    ConfigureServices(builder);
    
    var app = builder.Build();
    
    // Configure logging
    ConfigureLogging(app, args);
    
    // Ensure database is created
    await EnsureDatabaseAsync(app);
    
    // Configure middleware pipeline
    ConfigureMiddleware(app);
    
    
    await app.RunAsync();
    
}catch (Exception ex)
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
        o.AttachStacktrace = true;
    });
    
    Log.Information("Sentry configured");
}

void ConfigureStripe(WebApplicationBuilder builder)
{
    StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"] ??
                                 throw new InvalidOperationException("Invalid Stripe secret key");

    builder.Services.AddTransient<SessionService>();
    builder.Services.AddTransient<Stripe.SubscriptionService>();
    builder.Services.AddTransient<CustomerService>();
    
    Log.Information("Stripe configured");
}

void ConfigureMapper(WebApplicationBuilder builder)
{
    builder.Services.AddAutoMapper(typeof(DomainMappingProfile));
    
    Log.Information("Mapper configured");
}


void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddControllers();
    
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });
    
    ConfigureAuthentication(builder);
    ConfigureRateLimit(builder);
    ConfigureMinio(builder);
    ConfigureStripe(builder);
    ConfigureMapper(builder);
    ConfigureOptions(builder);
    ConfigureDatabase(builder);
    RegisterDependencies(builder);
    
    Log.Information("Services registered");
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? 
                        throw new InvalidOperationException("Invalid secret key")))
            };
        });
    
    Log.Information("Authentication configured");
}

void ConfigureMinio(WebApplicationBuilder builder)
{
    builder.Services.AddMinio(configureClient => configureClient
        .WithEndpoint(builder.Configuration["Minio:Endpoint"])
        .WithCredentials(builder.Configuration["Minio:AccessKey"], 
            builder.Configuration["Minio:SecretKey"])
        .WithSSL());
    
    Log.Information("Minio configured");
}

void ConfigureRateLimit(WebApplicationBuilder builder)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var path = context.Request.Path.Value ?? "";
            
            if (path.StartsWith("/waitlist/join", StringComparison.OrdinalIgnoreCase))
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(10),
                        PermitLimit = 3,
                        QueueLimit = 0,
                    });
            }
            
            
            return RateLimitPartition.GetNoLimiter<string>("default");
        });
    });
    
    Log.Information("Rate limiting configured");
}

void ConfigureOptions(WebApplicationBuilder builder)
{
    builder.Services.AddOptions();
    builder.Services.Configure<FeatureFlagsConfig>(
        builder.Configuration.GetSection("FeatureFlags"));
    builder.Services.Configure<TierLimitsConfig>(
        builder.Configuration.GetSection("TierLimits"));
    builder.Services.Configure<AIConfig>(
        builder.Configuration.GetSection("AI"));
    builder.Services.Configure<StripeConfig>(builder.Configuration.GetSection("Stripe"));
    builder.Services.Configure<ElevenLabsConfig>(builder.Configuration.GetSection("ElevenLabs"));

    builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("Email"));
    
    Log.Information("Options configured");
}

void ConfigureDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<DatabaseContext>(
        options =>
        {
            options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Invalid database connection string"));
            
            if (builder.Environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
        }, ServiceLifetime.Transient);
    
    Log.Information("Database configured");
}

void RegisterDependencies(WebApplicationBuilder builder)
{
    // Repositories
    builder.Services.AddTransient<IAccountRepository, AccountRepository>();
    builder.Services.AddTransient<IGuestRepository, GuestRepository>();
    builder.Services.AddTransient<ISubscriptionRepository, SubscriptionRepository>();
    builder.Services.AddTransient<IConversationRepository, ConversationRepository>();
    builder.Services.AddTransient<IMessageRepository, MessageRepository>();
    builder.Services.AddTransient<IWaitlistRepository, WaitlistRepository>();

    // Core Services
    builder.Services.AddTransient<IAuthService, AuthService>();
    builder.Services.AddTransient<ISecurityService, SecurityService>();
    builder.Services.AddTransient<IOpenAIService, OpenAIService>();
    builder.Services.AddTransient<IAuditService, AuditService>();
    builder.Services.AddTransient<IWaitlistService, WaitlistService>();
    builder.Services.AddTransient<IEmailService, EmailService>();
    builder.Services.AddTransient<IEmailTemplateService, EmailTemplateService>();

    // Business Services
    builder.Services.AddTransient<IAccountService, AccountService>();
    builder.Services.AddTransient<IGuestService, GuestService>();
    builder.Services.AddTransient<IConversationsService, ConversationsService>();
    builder.Services.AddTransient<IMessageService, MessageService>();
    builder.Services.AddTransient<IStripeService, StripeService>();
    builder.Services.AddTransient<ISubscriptionService, SubscriptionService>();
    builder.Services.AddTransient<IFileStorageService, FileStorageService>();
    builder.Services.AddTransient<IFeatureFlagsService, FeatureFlagsService>();
    builder.Services.AddTransient<IAccountUsageService, AccountUsageService>();
    
    // Factories and Handlers
    builder.Services.AddTransient<IAccountFactory, AccountFactory>();
    builder.Services.AddTransient<IInstructionLoader, InstructionLoader>();
    builder.Services.AddTransient<IChatClientFactory, ChatClientFactory>();
    builder.Services.AddTransient<IMessageHandler, MessageHandler>();
    builder.Services.AddTransient<IAudioService, AudioService>();
    
    Log.Information("Dependencies registered");
}

void ConfigureLogging(WebApplication app, string[] args)
{
    Log.Information("Logging started at {Now}", DateTime.UtcNow.ToString("u"));
    Log.Information("Log directory: {Directory}", "$LogDirectory$");
    Log.Information("Command line arguments: [ {Args} ]", string.Join(" ", args));
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    
    if (app.Environment.IsDevelopment())
    {
        Log.Warning("Development environment detected. Using development API URL: {URL}", "https://localhost:9099/scalar/");
        Log.Debug("Sentry is disabled in development environment");
    }else if (app.Environment.IsStaging())
    {
        Log.Information("Staging environment detected. Using staging API URL: {URL}", "https://api-staging.LudereAI.com/");
    }
    else
    {
        Log.Information("Production environment detected. Using production API URL: {URL}", "https://api.LudereAI.com/");
    }
}

async Task EnsureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    Log.Information("Warming up database connection...");
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    await context.Database.EnsureCreatedAsync();
    Log.Information("Database connection warmed up.");
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithHttpBearerAuthentication(bearerOptions =>
            {
                bearerOptions.Token = "Bearer";
            });
            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecurityScheme = "Bearer"
            };
        });
    }

    app.UseHttpsRedirection();
    app.UseSentryTracing();
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseRateLimiter();
    app.UseMiddleware<CloudflareMiddleware>();
    
    Log.Information("Middleware pipeline configured");
}
