using CSCDNMA;
using CSCDNMA.Database;
using CSCDNMA.Model;
using CzomPack.Attributes;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
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
		File.AppendAllText("internallog.log", msg, Encoding.UTF8);
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
	Globals.Metrics = builder.Configuration.GetSection("Metrics").Get<Metrics>();
    #region Logger
    //Log.Logger = CzomPack.Logging.Logger.GetLogger<Program>();

    var loggerConfig = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.Enrich.FromLogContext()
		.WriteTo.File(
			Path.Combine(Globals.LogsDirectory, @$"{Assembly.GetExecutingAssembly().GetName().Name}.{DateTime.Now:yyyy-MM-dd}.log"),
			outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
			fileSizeLimitBytes: 1_000_000,
#if RELEASE
        restrictedToMinimumLevel: LogEventLevel.Information,
#else
			restrictedToMinimumLevel: LogEventLevel.Verbose,
#endif
			rollOnFileSizeLimit: true,
			shared: true,
			flushToDiskInterval: TimeSpan.FromSeconds(1))
		.WriteTo.Console(
			outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
			theme: AnsiConsoleTheme.Literate,
#if RELEASE
        restrictedToMinimumLevel: LogEventLevel.Information
#else
			restrictedToMinimumLevel: LogEventLevel.Verbose
#endif
		);
		loggerConfig.WriteTo.GrafanaLoki(Globals.MetricsProvider.Host ?? "http://loki:3100", new List<LokiLabel>() { new() { Key = "product", Value = "czsoftcdn-node" }, new() { Key = "node", Value = Globals.ApiInformation.Node ?? "n/a" } })
        Log.Logger = loggerConfig.CreateLogger();
    #endregion

    Log.Information("Starting host...");
	Globals.Environment = Enum.Parse<HostEnvironment>(builder.Environment.EnvironmentName, true);
    if (Globals.Environment is not HostEnvironment.Production) Log.Information("WARNING! Do not use {environment} mode in public-facing environment.", Globals.Environment);
    Log.Information("---------------- Czompi Software CDN ------------------");
	Log.Information("  Node: {node}", Globals.ApiInformation.Node);
	Log.Information("  ApplicationId: {id}", Globals.ApiInformation.Id);
	Log.Information("  Build: \"{build}\"", Globals.ApiInformation.Build);
	Log.Information("  Version: \"{version}\"", Globals.ApiInformation.Version);
	Log.Information("  Environment: \"{environment}\"", Globals.Environment);
	Log.Information("  CompileTime: \"{compileTime:yyyy'.'MM'.'dd'T'HH':'mm':'ss}\"", Globals.ApiInformation.CompileTime);
	Log.Information("-------------------------------------------------------");
	Log.Debug("Application startup parameters: {args}", _args.ToString());
	Log.Information("Working directory: {dataDirectory}", CzomPack.Settings.WorkingDirectory);


	builder.Host.UseSerilog();
	builder.Services.AddControllers();

	string db = _args.Any() && _args.ContainsName("connectionString") ? _args.WithName("connectionString") : builder.Configuration["ConnectionString"];
	db ??= builder.Configuration["Connection_String"];


    builder.Services.AddDbContext<ApplicationDatabaseContext>(options =>
	{
		options.UseSqlServer(db);
	}, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);

	builder.Services.AddCors(options =>
	{
		options.AddDefaultPolicy(builder =>
		{
			builder.AllowAnyOrigin();
		});
	});

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
#endregion