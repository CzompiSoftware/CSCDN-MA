using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CzompiSoftwareCDN.Controllers
{
    //[Route("[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(ILogger<AssetsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{type?}/{product?}/{*remaining}")]
        public async Task<IActionResult> GetAsset(string type = null, string product = null, string remaining=null)
        {
            if (type == null || product == null || remaining == null)
            {
                var error = Globals.Error.AccessForbidden;
                return Ok(error);
            }
            try
            {
                var assetType = Enum.Parse(typeof(AssetType), type, true);
                if (Globals.Assets.ContainsKey(product.ToLower()))
                {
                    var fileName = System.IO.Path.GetFullPath(Path.Combine("..","data", assetType.ToString().ToLower(), Globals.Assets[product.ToLower()], remaining.Replace('/', Path.DirectorySeparatorChar)));
                    Console.Out.WriteLine(fileName);
                    if(System.IO.File.Exists(fileName))
                    {
                        var res = new FileInfo(fileName);
                        var mime = res.GetMime();
                        return File(System.IO.File.ReadAllBytes(fileName), mime);
                    }
                }

                return Ok(Globals.Error.FileNotExists);
            }
            catch (Exception)
            {
                var error = Globals.Error.UnsupportedAssetType;
                return Ok(error);
            }
        }
    }
}
