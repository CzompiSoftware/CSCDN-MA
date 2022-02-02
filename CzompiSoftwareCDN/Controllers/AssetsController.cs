using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        [Route("api/{*remaining}")]
        public async Task<IActionResult> ApiInfoAsync(string remaining = null) => await Task.Run(() => ApiInfo(remaining));

        [NonAction]
        public IActionResult ApiInfo(string remaining = null)
        {
            return Ok(Globals.ApiInformation);
        }

        [HttpGet]
        [Route("{type?}/{product?}/{*remaining}")]
        public async Task<IActionResult> GetAssetAsync(string type = null, string product = null, string remaining = null) => await Task.Run(() => GetAsset(type, product, remaining));

        [NonAction]
        public IActionResult GetAsset(string type = null, string product = null, string remaining = null)
        {

            #region Definitions
            string ip = Request.Headers["cf-connecting-ip"].FirstOrDefault(), referer =
#if NET6_0_OR_GREATER
                Request.Headers.Referer.FirstOrDefault()
#else
                Request.Headers["Referer"].ToString()
#endif
                ;
            Globals.EnabledHosts = JsonSerializer.Deserialize<EnabledHosts>(System.IO.File.ReadAllText(Globals.EnabledHostsFile));

            string bareFileName = "", clientInfo = $"{ip} | {referer} | {Request.Headers["X-WAWS-Unencoded-URL"].FirstOrDefault()}";

            try
            {
                bareFileName = Path.GetFullPath(Path.Combine(Globals.AssetsDirectory, type?.ToLower() ?? "", product?.ToLower() ?? "", remaining?.Replace('/', Path.DirectorySeparatorChar) ?? "") ?? "");

            }
            catch (Exception)
            {
                bareFileName = "InvalidPathException";
            }

            string currentPath = $"{Request.Headers[":Path"].FirstOrDefault()}";
#endregion

            var json = $"{JsonSerializer.Serialize(Request.Headers, Globals.JsonSerializerOptions)}";

            if (Globals.EnabledHosts.ContainsKey(currentPath))
            {
                if (!Globals.EnabledHosts[currentPath].Where(x => (referer ?? "").StartsWith(x ?? "", StringComparison.OrdinalIgnoreCase)).Any())
                {
                    _logger.LogError($"{clientInfo} | {bareFileName} | AccessForbidden");
                    return StatusCode(403, Globals.Error.AccessForbidden);
                }
            }
            else if (Globals.EnabledHosts.ContainsKey("*"))
            {
                if (!Globals.EnabledHosts["*"].Where(x => (referer ?? "").StartsWith(x ?? "", StringComparison.OrdinalIgnoreCase)).Any())
                {
                    _logger.LogError($"{clientInfo} | {bareFileName} | AccessForbidden");
                    return StatusCode(403, Globals.Error.AccessForbidden);
                }
            }

#region Handle response
            if (type == null || product == null || remaining == null)
            {
                _logger.LogError($"{clientInfo} | {bareFileName} | AccessForbidden");
                var error = Globals.Error.AccessForbidden;
                return StatusCode(403, error);
            }
            try
            {
                var assetType = Enum.Parse(typeof(AssetType), type, true);
                if (Globals.Assets.ContainsKey(product.ToLower()))
                {
                    var fileName = Path.GetFullPath(Path.Combine(Globals.AssetsDirectory, assetType.ToString().ToLower(), Globals.Assets[product.ToLower()], remaining.Replace('/', Path.DirectorySeparatorChar)));
                    //_logger.LogInformation($"{JsonSerializer.Serialize(Request.Headers, Globals.JsonSerializerOptions)} | {fileName}");
                    if (System.IO.File.Exists(fileName))
                    {
                        _logger.LogInformation($"{clientInfo} | {fileName} | OK");
                        var res = new FileInfo(fileName);
                        var mime = res.GetMime();
                        return File(System.IO.File.ReadAllBytes(fileName), mime);
                    }
                    else
                    {
                        _logger.LogInformation($"{clientInfo} | {fileName} | FileNotExists");
                        return StatusCode(404, Globals.Error.FileNotExists);
                    }
                }

                _logger.LogError($"{clientInfo} | {bareFileName} | FileNotExists");
                return StatusCode(404, Globals.Error.FileNotExists);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{clientInfo} | {bareFileName} | {ex}");
                return StatusCode(415, Globals.Error.UnsupportedAssetType);
            }
#endregion

        }
    }
}
