using CSCDNMA.Database;
using CSCDNMA.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CSCDNMA.Controllers;

[ApiController]
public class AssetsController : BaseController<AssetsController>
{
    public AssetsController(ILogger<AssetsController> logger, ApplicationDatabaseContext settings) : base(logger, settings)
    {
    }

    [HttpGet]
    [Route("{type}/{product}@v{version}/{*remaining}")]
    public async Task<IActionResult> GetAssetV2Async(string type, string product, string version, string remaining = null)
    {
        PopulateFields();

        var matchver = new Regex("^[0-9]{1,}(\\.[0-9]{1,}(\\.[0-9]{1,}|)|)").Match(version).Value;
        var mainver = matchver.TrimStart('v');
        if (mainver.Count(x => x.Equals('.')) == 1) mainver += ".0";
        Version _version = Version.Parse(mainver);
        string ver = $"v{Math.Max(_version.Major, 0)}.{Math.Max(_version.Minor, 0)}.{Math.Max(_version.Build, 0)}";

        if (matchver.Length < version.Length) 
            ver += version[matchver.Length..];

        string currentPath = Request.Headers["Path"].FirstOrDefault() ?? Request.Headers[":Path"].FirstOrDefault() ?? Request.Path;

        try
        {
            AssetType assetType = Enum.Parse<AssetType>(type, true);
            var prod = _settings.Products.First(prod => prod.Name.ToLower().Equals(product.ToLower()));
            if (prod is not null)
            {
                return await HandleResponseAsync(assetType, prod.Name, ver, remaining.Replace('/', Path.DirectorySeparatorChar));
            }

        }
        catch (Exception ex)
        {
            LogError(currentPath, ex.GetType().Name);
            return StatusCode(415, Globals.Error.UnsupportedAssetType);
        }
        LogError(currentPath, $"{typeof(AccessViolationException).Name}.g");
        return StatusCode(403, Globals.Error.AccessForbidden);
    }

}
