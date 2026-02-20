using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using UserAccessManagement.Api.Extensions;
using UserAccessManagement.Api.Middleware;
using UserAccessManagement.Api.Routing;
using UserAccessManagement.Application;
using UserAccessManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RoutePrefixConvention("api/v1"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)     
    .AddSwaggerDocumentation();

#endregion

#region Logging (Serilog)

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), 
    "../logs/useraccess.json", 
    rollingInterval: RollingInterval.Day,
    shared: true)
    .CreateLogger();


builder.Host.UseSerilog();

#endregion

var app = builder.Build();

#region Middleware Pipeline

app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
