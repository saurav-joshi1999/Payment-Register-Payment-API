using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Hosting;
using PaymentAPI.Models;
using Serilog;

// ──────────────────────────────────────────────────────────
// BOOTSTRAP LOGGER — catches errors during startup itself
// ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting PaymentAPI application...");

    var builder = WebApplication.CreateBuilder(args);

    // ──────────────────────────────────────────────────────
    // SERILOG — replaces the default .NET logging completely
    // Reads all sink config from appsettings.{Environment}.json
    // ──────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("Application", "PaymentAPI")
    );

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<PaymentDetailContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection"));
    });

    var app = builder.Build();

    // AUTOMATICALLY APPLY MIGRATIONS
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDetailContext>();
        dbContext.Database.Migrate();
    }

    // ──────────────────────────────────────────────────────
    // SERILOG REQUEST LOGGING — logs every HTTP request
    // with status code, elapsed time, method, path, etc.
    // ──────────────────────────────────────────────────────
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // CORS - read allowed origins from environment-specific appsettings
    var allowedOrigins = builder.Configuration.GetValue<string>("AllowedCorsOrigins") ?? "http://localhost:4200";
    app.UseCors(options => options.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
