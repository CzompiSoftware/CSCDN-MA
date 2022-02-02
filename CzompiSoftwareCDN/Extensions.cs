using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace CSCDNMA
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
