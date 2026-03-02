using Event_Management_System.API.Application.BackgroundServices;
using Event_Management_System.API.Application.Exceptions;
using Event_Management_System.API.Application.Implementation;
using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Application.Payments;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Extensions;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Event_Management_System.API.Infrastructures.Repositories;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PayStack.Net;
using Serilog;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

ILogger<Program> logger = null;

// this creates the builder for the web application(configuration, DI, logging, etc.)   
var builder = WebApplication.CreateBuilder(args);

// Get configuration and service name
var configuration = builder.Configuration;
var serviceName = configuration["ServiceName"] ?? "EventManagementSystemAPI";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure logging: configures Serilog as the app logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.File(
        path: $"Logs/{serviceName}/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 31)
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Tell the Generic Host to use Serilog as the logging provider
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));
// Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Services   
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventCentreService, EventCentreService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IOrganizerRequestService, OrganizerRequestService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Notification Services
builder.Services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
builder.Services.AddScoped<INotificationChannel, EmailNotificationChannel>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Background Services
builder.Services.AddHostedService<ExpireReservedTicketsService>();
builder.Services.AddHostedService<ExpireReservedBookingService>();

// Paystack payment integration
var paystackSecretKey = configuration["PayStack:SecretKey"]
    ?? throw new InvalidOperationException("PayStack:SecretKey is not configured");
builder.Services.AddSingleton(new PayStackApi(paystackSecretKey));
builder.Services.AddScoped<IPaymentService, PaystackPaymentService>();

builder.Services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

builder.Services.AddScoped(typeof(IDatabaseRepository<,>), typeof(DatabaseRepository<,>));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:ValidIssuer"],
        ValidAudience = configuration["JwtSettings:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});

// Hangfire Configuration
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Health Check Configuration
builder.Services.AddHealthChecks()
    .AddCheck("Event_Management_System_API", () =>
        HealthCheckResult.Healthy("---> API is running"));

// --- Add HealthChecks UI ---
builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage();

// Swagger / OpenAPI (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Management Platform API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \",\"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });

    c.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("00:00:00")
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = cacheSettings.ConnectionString;
});
builder.Services.Configure<CacheSettings>(configuration.GetSection(nameof(CacheSettings)));
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpClient();

// Set Up Https Forward Headers : Forwarding headers from API Gateway to our microservice
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();
logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    Log.Information("---------------------------- Starting up the application -----------------------");
   // if (app.Environment.IsDevelopment())
    //{
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API v1");
            // To serve Swagger UI at root (https://localhost:7004/), uncomment next line:
            c.RoutePrefix = string.Empty;
        });
    //}
    // Apply any pending migrations and create the database if it doesn't exist
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed.");
        }
    }
    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseMiddleware<ExceptionHandlingMiddleware>();


    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.UseHangfireDashboard("/hangfire"); // accessible at /hangfire

    // --- Map Health Check Endpoints ---
    app.MapHealthChecks("/health", new HealthCheckOptions     // default health endpoint
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString()
                }),
                totalDuration = report.TotalDuration.ToString()
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui";       // dashboard available at /health-ui
    });

    // Log application start
    logger.LogInformation("----> Application {ServiceName} started successfully at {Time}", serviceName, DateTime.UtcNow);

    app.Run();


}
catch (Exception ex)
{
    // Fatal errors during startup are captured here
    Log.Fatal(ex, "----> Application {ServiceName} terminated unexpectedly", serviceName);
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application exit
    Log.CloseAndFlush();
}


