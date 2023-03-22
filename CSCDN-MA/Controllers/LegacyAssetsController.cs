using Azure.Core;
using CSCDNMA.Database;
using CSCDNMA.Model;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSCDNMA.Controllers;

[ApiController]
public class LegacyAssetsController : BaseController<LegacyAssetsController>
{
    public LegacyAssetsController(ILogger<LegacyAssetsController> logger, ApplicationDatabaseContext settings) : base(logger, settings)
    {
    }

    [HttpGet]
    [Route("{type}/{productId}/v{version}/{*remaining}")]
    public async Task<IActionResult> GetAssetAsync(string type, string productId, Version version, string remaining = null)
    {
        #region Definitions
        _ip ??= Request.Headers["cf-connecting-ip"].FirstOrDefault() ?? Request.Headers["x-real-ip"].FirstOrDefault() ?? Request.Headers["x-forwarded-for"].FirstOrDefault();
#if NET6_0_OR_GREATER
        _referer ??= Request.Headers.Referer.FirstOrDefault();
#else
        _referer ??= Request.Headers["Referer"].ToString();
#endif
        _nodeId ??= Request.Headers["NodeId"].ToString();
        #endregion

        string fileName = "";
        string ver = $"v{Math.Max(version.Major, 0)}.{Math.Max(version.Minor, 0)}.{Math.Max(version.Build, 0)}";

        AssetType assetType = Enum.Parse<AssetType>(type, true);
        var prod = _settings.Products.First(prod => prod.Id.Replace("-", "").ToLower().Equals(productId.ToLower()));
        if (prod is not null)
        {
            return await HandleResponseAsync(assetType, prod.Name, ver, remaining.Replace('/', Path.DirectorySeparatorChar));
        }
        LogWarning(fileName, HttpStatusCode.Forbidden);
        return StatusCode(404, Globals.Error.FileNotExists);
    }


}