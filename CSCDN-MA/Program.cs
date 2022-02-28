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

        #region Globals
        #region Globals.Assets
        if (!File.Exists(Globals.ProductsFile))
        {
            File.WriteAllText(Globals.ProductsFile, JsonSerializer.Serialize(new Assets(), Globals.JsonSerializerOptions));
        }
        Globals.Assets = JsonSerializer.Deserialize<Assets>(File.ReadAllText(Globals.ProductsFile));
        #endregion

        #region Globals.EnabledHosts
        if (!File.Exists(Globals.EnabledHostsFile))
        {
            EnabledHosts enabledHosts = new();
            enabledHosts.Add("/path/to/asset", new List<string> {
                "https://assetdomain.tld",
            });
            enabledHosts.Add("*", new List<string> {
                "https://czompisoftware.hu",
                "https://beta.czompisoftware.hu",
                "https://*.czompisoftware.hu",
                "https://czompi.hu",
                "https://beta.czompi.hu",
                "https://*.czompi.hu",
            });
            File.WriteAllText(Globals.EnabledHostsFile, JsonSerializer.Serialize(enabledHosts, Globals.JsonSerializerOptions));
        }
        Globals.EnabledHosts = JsonSerializer.Deserialize<EnabledHosts>(File.ReadAllText(Globals.EnabledHostsFile));
        #endregion

        #region Globals.Config
        if (!File.Exists(Globals.ConfigFile))
        {
            File.WriteAllText(Globals.ConfigFile, JsonSerializer.Serialize(new Config
            {
                AppGuid = Guid.NewGuid(),
                DeployTime = DateTime.Parse(DateTime.Now.ToString("yyyy'.'MM'.'dd'T'HH':'mm':'ss"))
            }, Globals.JsonSerializerOptions));
        }
        Globals.Config = JsonSerializer.Deserialize<Config>(File.ReadAllText(Globals.ConfigFile));
        #endregion

        #region Globals.ApiInformation
        var appProcess = Process.GetCurrentProcess();
        Globals.ApiInformation = new(appProcess.StartTime);
        #endregion
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
            Log.Information($"  DeployTime: \"{Globals.Config.DeployTime:yyyy'.'MM'.'dd'T'HH':'mm':'ss}\"");
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
