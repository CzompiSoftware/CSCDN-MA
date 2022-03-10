using CSCDNMA.Database;
using CSCDNMA.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSCDNMA.Controllers
{
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly ILogger<AssetsController> _logger;
        private readonly CzSoftCDNDatabaseContext _settings;

        public AssetsController(CzSoftCDNDatabaseContext settings, ILogger<AssetsController> logger)
        {
            _logger = logger;
            _settings = settings;
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
            var lst = _settings.AccessConfig.ToList().Where(itm => itm.ProductId is not null && itm.ProductId.ToString().ToLowerInvariant().Equals(product?.ToLowerInvariant())).ToList();
            if (!lst.Any())
            {
                var wildcard = _settings.AccessConfig.Where(itm => itm.ProductId == null).ToList();
                // deny access when request route is not match the regex value or is not a wildcard.
                if (!wildcard.Any() || (wildcard.Any() && !wildcard.First().RequestRoute.Equals("*") && !new Regex(wildcard.First().RequestRoute).IsMatch(referer ?? "")))
                {
                    _logger.LogError($"{clientInfo} | {bareFileName} | AccessForbidden");
                    return StatusCode(403, Globals.Error.AccessForbidden);
                }
            }
            // When the asset route is a wildcard value
            else if (lst.Any(itm => itm.AssetRoute.Equals("*"))) 
            {
                var itm = lst.First(itm => itm.AssetRoute.Equals("*"));
                // deny access when request route is not match the regex value or is not a wildcard.
                if (!itm.RequestRoute.Equals("*") && !new Regex(itm.RequestRoute).IsMatch(referer ?? "")) 
                {
                    _logger.LogError($"{clientInfo} | {bareFileName} | AccessForbidden");
                    return StatusCode(403, Globals.Error.AccessForbidden);
                }
            }
            // When the asset route is matching the regex value
            else if (lst.Any(itm => new Regex(itm.AssetRoute).IsMatch(currentPath))) 
            {
                var itm = lst.First(itm => new Regex(itm.AssetRoute).IsMatch(currentPath));
                // deny access when request route is not match the regex value or is not a wildcard.
                if (!itm.RequestRoute.Equals("*") && !new Regex(itm.RequestRoute).IsMatch(referer ?? ""))
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
                var prods = _settings.Products.ToList();
                var prod = _settings.Products.ToList().First(prod => prod.Id.ToString().ToLowerInvariant().Equals(product.ToLowerInvariant()));
                if (prod is not null)
                {
                    var fileName = Path.GetFullPath(Path.Combine(Globals.AssetsDirectory, assetType.ToString().ToLower(), prod.Name, remaining.Replace('/', Path.DirectorySeparatorChar)));
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