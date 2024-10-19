using CSCDNMA.Database;
using CSCDNMA.Model;
using Microsoft.AspNetCore.Mvc;

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
        PopulateFields();

        string ver = $"v{Math.Max(version.Major, 0)}.{Math.Max(version.Minor, 0)}.{Math.Max(version.Build, 0)}";

        string currentPath = Request.Headers["Path"].FirstOrDefault() ?? Request.Headers[":Path"].FirstOrDefault() ?? Request.Path;

        try
        {
            AssetType assetType = Enum.Parse<AssetType>(type, true);
            var prod = _settings.Products.First(prod => prod.Id.ToLower().Equals(productId.ToLower()));
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