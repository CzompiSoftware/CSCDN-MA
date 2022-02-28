using CSCDNMA.Controllers;
using Microsoft.AspNetCore.Hosting;
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

namespace CSCDNMA;

public class Program
{
    public static void Main(string[] args)
    {
        CzomPack.Settings.Application = new(Assembly.GetExecutingAssembly());
        CzomPack.Settings.WorkingDirectory = Globals.DataDirectory;

        #region ApiInformation
        var appProcess = Process.GetCurrentProcess();
        Globals.ApiInformation = new(appProcess.StartTime);
        #endregion

        #region Logger
        Log.Logger = CzomPack.Logging.Logger.GetLogger();
        #endregion

        #region Start app
        try
        {
            Log.Information("Starting host...");

            Log.Information($" ---------------- Czompi Software CDN ------------------");
            Log.Information($"  Version: \"{Globals.ApiInformation.Version}\"");
            Log.Information($"  Build: \"{Globals.ApiInformation.Build}\"");
            Log.Information($"  ApplicationId: \"{Globals.ApiInformation.Id}\"");
            Log.Information($"  CompileTime: \"{Globals.ApiInformation.CompileTime:yyyy'.'MM'.'dd'T'HH':'mm':'ss}\"");
            Log.Information($" -------------------------------------------------------");
            CreateHostBuilder(args).Build().Run();
            //return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
            //return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
        #endregion
    }

    #region CreateHostBuilder
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    #endregion
}
