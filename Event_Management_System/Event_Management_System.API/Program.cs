using Event_Management_System.API.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

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
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Swagger / OpenAPI (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseHttpsRedirection();

    app.UseRouting();

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


