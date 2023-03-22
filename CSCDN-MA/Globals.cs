using CSCDNMA;
using CSCDNMA.Model;
using System.IO;
using System.Net;
using System.Text.Json;

namespace CSCDNMA;

internal class Globals
{
    #region Directories
    internal static string DataDirectory
    {
        get
        {
            var dir = Path.GetFullPath(Path.Combine("..", "data"));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
    }
    internal static string AssetsDirectory
    {
        get
        {
            var dir = Path.GetFullPath(Path.Combine(DataDirectory, "assets"));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
    }
    internal static string LogsDirectory
    {
        get
        {
            var dir = Path.GetFullPath(Path.Combine(DataDirectory, "logs"));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
    }
    #endregion

    internal static string ProductsFile => Path.Combine(DataDirectory, "products.json");
    internal static string ConfigFile => Path.Combine(DataDirectory, "config.json");
    internal static string EnabledHostsFile => Path.Combine(DataDirectory, "enabledhosts.json");
    internal static string DataSourceFile => Path.Combine(DataDirectory, "settings.db");

    internal static HostEnvironment Environment { get; set; }
    internal static Metrics Metrics { get; set; }

    internal static JsonSerializerOptions JsonSerializerOptions => new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
    };

    internal static ApiInformation ApiInformation { get; set; }

    internal class Error
    {
        internal static ErrorResult AccessForbidden
        {
            get => new()
            {
                Error = nameof(FileNotFoundException),
                ErrorMessage = "You does not have permission to access this asset."
            };
        }

        internal static ErrorResult FileNotExists
        {
            get => new()
            {
                Error = nameof(AccessViolationException),
                ErrorMessage = "Specified asset does not exists.",
            };
        }

        internal static ErrorResult UnsupportedAssetType
        {
            get => new()
            {
                Error = nameof(NotSupportedException),
                ErrorMessage = "Invalid asset type selected.",
            };
        }
    }
}