using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
