using CSCDNMA;
using CSCDNMA.Database;
using CSCDNMA.Model;
using CzomPack.Attributes;
using CzomPack.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

var _args = Arguments.Parse(args, "--");

CzomPack.Settings.Application = new(Assembly.GetAssembly(typeof(Globals)));
CzomPack.Settings.WorkingDirectory = Globals.DataDirectory;

#region ApiInformation

var appProcess = Process.GetCurrentProcess();
var id = Dns.GetHostName();
Globals.ApiInformation = new(appProcess.StartTime, id);

#endregion

Serilog.Debugging.SelfLog.Enable(msg =>
{
    try
    {
        File.AppendAllText(Path.Combine(Globals.LogsDirectory, $"SelfLog.{DateTime.Now:yyyy-MM-dd}.log"), msg, Encoding.UTF8);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
});

#region Start app

try
{
    var builder = WebApplication.CreateBuilder(args);
    var selection = builder.Configuration.GetSection("Metrics");
    if (!selection.Exists())
    {
        selection = builder.Configuration.GetSection("Telemetry");
    }
    else
    {
        Logger.Warning<Program>("You're using the legacy method for setting up your telemetry provider. This will be removed in future editions.\r\nPlease use `Telemetry` configuration option instead!");
    }

    // If no telemetry provider was presented, but it was enabled, try to use default Grafana Loki configuration.
    if (selection.Exists())
    {
        Globals.Telemetry = selection.Get<Telemetry>() ?? new()
        {
            Provider = TelemetryProvider.GrafanaLoki,
            Host = "http://loki:3100"
        };
    }


    #region Logger

    var logFile = Path.Combine(Globals.LogsDirectory, @$"{Assembly.GetAssembly(typeof(Globals)).FullName}.{{Date:yyyy-MM-dd}}.log");
    var loggerConfig = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.File(
            logFile,
            outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1))
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Literate
        );
    if (Globals.Telemetry is not null)
    {
        var telemetryCollectorConfig = new LoggerConfiguration();

        if (Globals.Telemetry.Provider is TelemetryProvider.GrafanaLoki)
        {
            telemetryCollectorConfig.WriteTo.GrafanaLoki(Globals.Telemetry.Host ?? "http://loki:3100", new List<LokiLabel>() { new() { Key = "product", Value = "CSCDN/MA" }, new() { Key = "version", Value = Globals.ApiInformation.Version }, new() { Key = "node", Value = Globals.ApiInformation.Node ?? "n/a" } });
        }
        else if (Globals.Telemetry.Provider is TelemetryProvider.OpenTelemetry)
        {
            telemetryCollectorConfig.WriteTo.OpenTelemetry(options => OpenTelemetryConfiguration(options, "usage"));
            loggerConfig.WriteTo.OpenTelemetry(options => OpenTelemetryConfiguration(options, "logging"));
        }

        Log.Logger = loggerConfig.CreateLogger();
        Globals.TelemetryCollector = telemetryCollectorConfig.CreateLogger();
    }

    #endregion

    Logger.Info<Program>("Starting host...");
    Globals.Environment = Enum.Parse<HostEnvironment>(builder.Environment.EnvironmentName, true);
    if (Globals.Environment is not HostEnvironment.Production) Logger.Warning<Program>("WARNING! Do not use {environment} mode in public-facing environment.", Globals.Environment);
    Logger.Info<Program>("---------------- Czompi Software CDN ------------------");
    Logger.Info<Program>("  Node: {node}", Globals.ApiInformation.Node);
    Logger.Info<Program>("  ApplicationId: {id}", Globals.ApiInformation.Id);
    Logger.Info<Program>("  Build: \"{build}\"", Globals.ApiInformation.Build);
    Logger.Info<Program>("  Version: \"{version}\"", Globals.ApiInformation.Version);
    Logger.Info<Program>("  Telemetry provider: \"{telemetryProvider}\"", Globals.Telemetry?.Provider);
    Logger.Info<Program>("  Environment: \"{environment}\"", Globals.Environment);
    Logger.Info<Program>("  CompileTime: \"{compileTime:yyyy'.'MM'.'dd'T'HH':'mm':'ss}\"", Globals.ApiInformation.CompileTime);
    Logger.Info<Program>("-------------------------------------------------------");
    Logger.Debug<Program>("Application startup parameters: {args}", _args.ToString());
    Logger.Info<Program>("Working directory: {dataDirectory}", CzomPack.Settings.WorkingDirectory);


    builder.Host.UseSerilog();
    builder.Services.AddControllers();

    var passthroughMode = string.Equals(builder.Configuration["PASSTHROUGH_MODE"], "true", StringComparison.OrdinalIgnoreCase);
    Globals.IsPassthroughMode = passthroughMode;
    if (!Globals.IsPassthroughMode)
    {
        string db = _args.Count != 0 && _args.ContainsName("connectionString") ? _args.WithName("connectionString") : builder.Configuration["ConnectionString"];
        db ??= builder.Configuration["Connection_String"];

        builder.Services.AddDbContext<ApplicationDatabaseContext>(options => { options.UseSqlServer(db); }, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
    }

    builder.Services.AddCors(options => { options.AddDefaultPolicy(builder => { builder.AllowAnyOrigin(); }); });

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    app.UseCors();
    app.UseAuthorization();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

void OpenTelemetryConfiguration(BatchedOpenTelemetrySinkOptions options, string category)
{
    options.Endpoint = Globals.Telemetry.Host ?? "http://localhost:4317";
    options.ResourceAttributes = new Dictionary<string, object>
    {
        ["service.name"] = $"CSCDN-MA.{Globals.ApiInformation.Node}:{category}",
        ["service.instance.id"] = Globals.ApiInformation.Id,
        ["build"] = Globals.ApiInformation.Build,
        ["version"] = Globals.ApiInformation.Version,
        ["start_time"] = Globals.ApiInformation.StartTime,
        ["compile_time"] = Globals.ApiInformation.CompileTime,
    };

    if (Globals.Telemetry.Token != null)
    {
        options.Headers.Add("x-otlp-api-key", Globals.Telemetry.Token);
    }
}

#endregion