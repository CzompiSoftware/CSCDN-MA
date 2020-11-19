using CzompiSoftwareCDN.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CzompiSoftwareCDN
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (File.Exists("products.json")) Globals.Assets = JsonSerializer.Deserialize<Assets>(File.ReadAllText("products.json"));
            else Globals.Assets = new Assets { };
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
