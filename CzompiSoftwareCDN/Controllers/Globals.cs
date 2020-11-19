namespace CzompiSoftwareCDN.Controllers
{
    internal class Globals
    {
        public static Assets Assets { get; set; }
        public class Error
        {
            public static ErrorResult AccessForbidden
            {
                get => new()
                {
                    Error = "AccessForbiddenException",
                    ErrorMessage = "You does not have rights to this asset."
                };
            }

            public static ErrorResult FileNotExists
            {
                get => new()
                {
                    Error = "FileNotExistsException",
                    ErrorMessage = "Selected asset does not exists.",
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