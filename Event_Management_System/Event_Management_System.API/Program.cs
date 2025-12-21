using Event_Management_System.API.Application;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Extensions;
using Event_Management_System.API.Infrastructures;
using Event_Management_System.API.Infrastructures.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

ILogger<Program> logger = null;

// this creates the builder for the web application(configuration, DI, logging, etc.)   
var builder = WebApplication.CreateBuilder(args);

// Get configuration and service name
var configuration = builder.Configuration;
var serviceName = configuration["ServiceName"] ?? "EventManagementSystemAPI";

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
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

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


// Payment providers
//builder.Services.AddScoped<Event_Management_System.API.Application.Payments.IPaymentProvider, Event_Management_System.API.Infrastructure.Payments.FlutterwavePaymentProvider>();
//builder.Services.AddScoped<Event_Management_System.API.Application.Payments.IPaymentProvider, Event_Management_System.API.Infrastructure.Payments.PaystackPaymentProvider>();
//builder.Services.AddScoped<Event_Management_System.API.Application.Payments.IPaymentService, Event_Management_System.API.Application.Payments.PaymentService>();

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
try
{
    Log.Information("---------------------------- Starting up the application -----------------------");
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API v1");
            // To serve Swagger UI at root (https://localhost:7004/), uncomment next line:
            c.RoutePrefix = string.Empty;
        });
    }
    // Apply any pending migrations and create the database if it doesn't exist
    //using (var scope = app.Services.CreateScope())
    //{
    //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //    dbContext.Database.Migrate();
    //}
    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    // Log application start
    logger = app.Services.GetRequiredService<ILogger<Program>>();
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


