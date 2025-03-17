using System.Text;
using LudereAI.API.Core;
using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Mappers;
using LudereAI.Domain.Models.Configs;
using LudereAI.Domain.Models.Features;
using LudereAI.Infrastructure;
using LudereAI.Infrastructure.Repositories;
using LudereAI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    Console.Title = "LudereAI API";
    
    // Configure Serilog
    builder.Host.UseSerilog((context, config) => 
        config.ReadFrom.Configuration(context.Configuration));
    
    builder.WebHost.UseUrls("http://localhost:9098;https://localhost:9099");
    
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

void ConfigureOptions(WebApplicationBuilder builder)
{
    builder.Services.AddOptions();
    builder.Services.Configure<FeatureFlagsConfig>(builder.Configuration.GetSection("FeatureFlags"));
    builder.Services.Configure<AIConfig>(builder.Configuration.GetSection("AI"));
    builder.Services.Configure<PiperConfig>(builder.Configuration.GetSection("Piper"));
    builder.Services.Configure<ElevenLabsConfig>(builder.Configuration.GetSection("ElevenLabs"));
    
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
    builder.Services.AddTransient<IConversationRepository, ConversationRepository>();
    builder.Services.AddTransient<IMessageRepository, MessageRepository>();
    // Core Services
    builder.Services.AddTransient<IAuthService, AuthService>();
    builder.Services.AddTransient<ISecurityService, SecurityService>();
    builder.Services.AddTransient<IOpenAIService, OpenAIService>();

    // Business Services
    builder.Services.AddTransient<IAccountService, AccountService>();
    builder.Services.AddTransient<IGuestService, GuestService>();
    builder.Services.AddTransient<IConversationsService, ConversationsService>();
    builder.Services.AddTransient<IMessageService, MessageService>();
    
    // Factories and Handlers
    builder.Services.AddTransient<IAccountFactory, AccountFactory>();
    builder.Services.AddTransient<IInstructionLoader, InstructionLoader>();
    builder.Services.AddTransient<IChatClientFactory, ChatClientFactory>();
    builder.Services.AddTransient<IMessageHandler, MessageHandler>();
    
    Log.Information("Dependencies registered");
}

void ConfigureLogging(WebApplication app, string[] args)
{
    Log.Information("Logging started at {Now}", DateTime.UtcNow.ToString("u"));
    Log.Information("Log directory: {Directory}", "$LogDirectory$");
    Log.Information("Command line arguments: [ {Args} ]", string.Join(" ", args));
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
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
    if (app.Environment.IsDevelopment())
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
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseMiddleware<CloudflareMiddleware>();
    
    Log.Information("Middleware pipeline configured");
}
