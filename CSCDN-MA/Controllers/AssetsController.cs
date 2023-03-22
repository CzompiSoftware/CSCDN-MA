using CSCDNMA.Database;
using CSCDNMA.Model;
using Microsoft.AspNetCore.Mvc;

namespace CSCDNMA.Controllers;

[ApiController]
public class AssetsController : BaseController<AssetsController>
{
    public AssetsController(ILogger<AssetsController> logger, ApplicationDatabaseContext settings) : base(logger, settings)
    {
    }

    [HttpGet]
    [Route("{type}/{product}@v{version}/{*remaining}")]
    public async Task<IActionResult> GetAssetV2Async(string type, string product, Version version, string remaining = null)
    {
        _ip ??= Request.Headers["cf-connecting-ip"].FirstOrDefault() ?? Request.Headers["x-real-ip"].FirstOrDefault() ?? Request.Headers["x-forwarded-for"].FirstOrDefault();
#if NET6_0_OR_GREATER
        _referer ??= Request.Headers.Referer.FirstOrDefault();
#else
        _referer ??= Request.Headers["Referer"].ToString();
#endif
        _nodeId ??= Request.Headers["NodeId"].ToString();

        string fileName = "";
        string ver = $"v{Math.Max(version.Major, 0)}.{Math.Max(version.Minor, 0)}.{Math.Max(version.Build, 0)}";

        string currentPath = $"{Request.Headers["Path"].FirstOrDefault()}";

        try
        {
            fileName = Path.GetFullPath(Path.Combine(Globals.AssetsDirectory, type?.ToLower() ?? "", product?.ToLower() ?? "", remaining?.Replace('/', Path.DirectorySeparatorChar) ?? "") ?? "");
        }
        catch (Exception ex)
        {
            LogError(fileName, ex.GetType().Name);
            return StatusCode(404, Globals.Error.FileNotExists);
        }

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
            LogError(fileName, ex.GetType().Name);
            return StatusCode(404, Globals.Error.FileNotExists);
        }
        LogError(fileName, $"{typeof(AccessViolationException).Name}.g");
        return StatusCode(403, Globals.Error.AccessForbidden);
    }

}
