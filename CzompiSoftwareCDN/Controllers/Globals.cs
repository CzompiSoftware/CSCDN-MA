using CzompiSoftwareCDN.Model;
using System.Text.Json;

namespace CzompiSoftwareCDN.Controllers
{
    internal class Globals
    {
        #region Directories
        public static string DataDirectory
        {
            get
            {
                var dir = Path.GetFullPath(Path.Combine("..", "data"));
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return dir;
            }
        }
        public static string AssetsDirectory
        {
            get
            {
                var dir = Path.GetFullPath(Path.Combine(DataDirectory, "assets"));
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return dir;
            }
        }
        public static string LogsDirectory
        {
            get
            {
                var dir = Path.GetFullPath(Path.Combine(DataDirectory, "logs"));
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return dir;
            }
        }
        #endregion
        public static string ProductsFile => Path.Combine(DataDirectory, "products.json");
        public static string ConfigFile => Path.Combine(DataDirectory, "config.json");
        public static string EnabledHostsFile => Path.Combine(DataDirectory, "enabledhosts.json");

        public static Assets Assets { get; set; }
        public static JsonSerializerOptions JsonSerializerOptions => new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
        };

        public static Config Config { get; internal set; }
        public static ApiInformation ApiInformation { get; internal set; }
        public static EnabledHosts EnabledHosts { get; internal set; }

        public class Error
        {
            public static ErrorResult AccessForbidden
            {
                get => new()
                {
                    Error = "AccessForbiddenException",
                    ErrorMessage = "You does not have permission to access this asset."
                };
            }

            public static ErrorResult FileNotExists
            {
                get => new()
                {
                    Error = "FileNotExistsException",
                    ErrorMessage = "Specified asset does not exists.",
                };
            }

            public static ErrorResult UnsupportedAssetType
            {
                get => new()
                {
                    Error = "UnsupportedAssetTypeException",
                    ErrorMessage = "Invalid asset type selected.",
                };
            }
        }
    }
}