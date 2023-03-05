using CSCDNMA;
using CSCDNMA.Database;
using CzomPack.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

var _args = Arguments.Parse(args, "--");

CzomPack.Settings.Application = new(Assembly.GetAssembly(typeof(Globals)));
CzomPack.Settings.WorkingDirectory = Globals.DataDirectory;

#region Logger
Log.Logger =new LoggerConfiguration().WriteTo
	.Console()
	.CreateLogger();
#endregion

#region ApiInformation
var appProcess = Process.GetCurrentProcess();
#endregion

#region Start app
try
{
	Log.Information("Starting host...");
	var builder = WebApplication.CreateBuilder();

	var id = _args.Any() && _args.ContainsName("appGuid") ? _args.WithName("appGuid") : builder.Configuration["ApplicationId"];
	Globals.ApiInformation = new(appProcess.StartTime, id);

	Log.Information("---------------- Czompi Software CDN ------------------");
	Log.Information("  ApplicationId: {id}", Globals.ApiInformation.Id);
	Log.Information("  Build: \"{build}\"", Globals.ApiInformation.Build);
	Log.Information("  Version: \"{version}\"", Globals.ApiInformation.Version);
	Log.Information("  CompileTime: \"{compileTime:yyyy'.'MM'.'dd'T'HH':'mm':'ss}\"", Globals.ApiInformation.CompileTime);
	Log.Information("-------------------------------------------------------");
	Log.Debug("Application startup parameters: {args}", _args.ToString());
	Log.Information("Working directory: {dataDirectory}", CzomPack.Settings.WorkingDirectory);


	builder.Services.AddControllers();
	builder.Host.UseSerilog();
	
	var db = _args.Any() && _args.ContainsName("connectionString") ? _args.WithName("connectionString") : builder.Configuration["ConnectionString"];

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