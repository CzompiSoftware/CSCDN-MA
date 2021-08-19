using Microsoft.AspNetCore.StaticFiles;

namespace CzompiSoftwareCDN
{
    public static class Extensions
    {
        public static string GetMime(this FileInfo fi)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fi.FullName, out string contentType);
            return contentType ?? "application/octet-stream";
        }
    }
}
