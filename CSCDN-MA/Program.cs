using CSCDNMA;
using CSCDNMA.Database;
using CzomPack.Attributes;
using CzomPack.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

var _args = Arguments.Parse(args, " ");

CzomPack.Settings.Application = new(Assembly.GetAssembly(typeof(Globals)));
CzomPack.Settings.WorkingDirectory = Globals.DataDirectory;
Console.WriteLine(CzomPack.Settings.WorkingDirectory);

#region Logger
Log.Logger = CzomPack.Logging.Logger.GetLogger();
#endregion

#region ApiInformation
var appProcess = Process.GetCurrentProcess();
Globals.ApiInformation = new(appProcess.StartTime);
#endregion

#region Start app
try
{
	Logger.Info("Starting host...");

	Logger.Info($" ---------------- Czompi Software CDN ------------------");
	Logger.Info($"  Version: \"{Globals.ApiInformation.Version}\"");
	Logger.Info($"  Build: \"{Globals.ApiInformation.Build}\"");
	Logger.Info($"  ApplicationId: \"{Globals.ApiInformation.Id}\"");
	Logger.Info($"  CompileTime: \"{Globals.ApiInformation.CompileTime:yyyy'.'MM'.'dd'T'HH':'mm':'ss}\"");
	Logger.Info($" -------------------------------------------------------");

	var builder = WebApplication.CreateBuilder();

	builder.Services.AddControllers();
	builder.Host.UseSerilog();
	var db = _args.Any() && _args.ContainsName("connectionString") ? _args.WithName("connectionString") : builder.Configuration["CzSoftDatabase"];

	builder.Services.AddDbContext<CzSoftCDNDatabaseContext>(options =>
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