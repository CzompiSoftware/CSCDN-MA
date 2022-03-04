using CzomPack.Attributes;
using CzomPack.Logging;
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

[Executable]
public partial class Program
{
    static partial void Main(Arguments args)
    {
        CzomPack.Settings.Application = new(Assembly.GetExecutingAssembly());
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
            CreateHostBuilder(args.GetArgumentList()).Build().Run();
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
