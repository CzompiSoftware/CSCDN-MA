using CSCDNMA.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using CSCDNMA.Database;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Runtime.ConstrainedExecution;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CSCDNMA.Controllers;

public class BaseController<T> : ControllerBase
{
    protected readonly ILogger<T> _logger;
    protected readonly ApplicationDatabaseContext _settings;

    protected string _ip = null;
    protected string _referer = null;
    protected string _nodeId = null;

    public BaseController(ILogger<T> logger, ApplicationDatabaseContext settings)
    {
        _logger = logger;
        _settings = settings;

        //_ip ??= Request?.Headers?["cf-connecting-ip"].FirstOrDefault() ?? Request?.Headers?["x-real-ip"].FirstOrDefault() ?? Request?.Headers?["x-forwarded-for"].FirstOrDefault();
#if NET6_0_OR_GREATER
        _referer ??= Request?.Headers?.Referer.FirstOrDefault();
#else
        _referer ??= Request?.Headers?["Referer"].ToString();
#endif
        _nodeId ??= Globals.ApiInformation.Node;
    }

    #region HandleResponse
    [NonAction]
    protected async Task<IActionResult> HandleResponseAsync(AssetType assetType, string product, string version, string path)
    {
        string currentPath = Request.Headers["Path"].FirstOrDefault() ?? Request.Headers[":Path"].FirstOrDefault() ?? Request.Path;
        var fileName = Path.GetFullPath(Path.Combine(Globals.AssetsDirectory, assetType.ToString().ToLowerInvariant(), product, version, path));

        try
        {
            await CheckRights(referer: _referer, assetRoute: $"{Request.Path}");
        }
        catch (AccessViolationException avex)
        {
            LogError(currentPath, $"{nameof(AccessViolationException)}.g");

            return StatusCode(403, ErrorJson(avex));
        }
        catch (Exception ex)
        {
            return StatusCode(403, Globals.Environment is not HostEnvironment.Production ? ErrorJson(ex): Globals.Error.AccessForbidden);
        }

        try
        {

            if (System.IO.File.Exists(fileName))
            {
               LogInformation(currentPath, HttpStatusCode.OK);
                var res = new FileInfo(fileName);
                var mime = res.GetMime();
                var ext = Path.GetExtension(fileName).ToLower();
                string[] nonByteContent = { ".css", ".scss", ".js", ".json", ".md", ".xmd" };
                byte[] contentBytes;
                if (nonByteContent.Contains(ext))
                {
                    var content = await System.IO.File.ReadAllTextAsync(fileName);
                    foreach (var t in Enum.GetNames<AssetType>())
                    {
                        content = content.Replace($"${{assetPath:{t}}}", $"${{cdnRoot}}{t.ToLowerInvariant()}/{product}@{version}/", StringComparison.OrdinalIgnoreCase);
                    }
                    content = content.Replace($"${{cdnRoot}}", "https://${cdnHost}/", StringComparison.OrdinalIgnoreCase);
                    content = content.Replace($"${{cdnScheme}}", $"{Request.HttpContext.Request.Scheme}", StringComparison.OrdinalIgnoreCase);
                    content = content.Replace($"${{cdnHost}}", $"{Request.HttpContext.Request.Headers.Host}", StringComparison.OrdinalIgnoreCase);
                    contentBytes = Encoding.UTF8.GetBytes(content);
                }
                else
                {
                    contentBytes = await System.IO.File.ReadAllBytesAsync(fileName);
                }
                return File(contentBytes, mime);
            }
            else
            {
                LogWarning(currentPath, HttpStatusCode.NotFound);
                return StatusCode(404, Globals.Error.FileNotExists);
            }

        }
        catch (Exception ex)
        {
            LogError(currentPath, ex.Message);

            if(Globals.Environment is not HostEnvironment.Production)
                return StatusCode(403, ErrorJson(ex));
            return StatusCode(415, Globals.Error.UnsupportedAssetType);
        }
    }

    private ErrorResult ErrorJson(Exception avex, string message = null) => new()
    {
        Error = avex.GetType().Name,
        ErrorMessage = message ?? avex.Message,
        Cause = avex.InnerException?.GetType()?.Name,
    };

    private ErrorResult ErrorJson(string error, string message, string cause = null) => new()
    {
        Error = error,
        ErrorMessage = message,
        Cause = cause,
    };

    private async Task CheckRights(string referer, string assetRoute)
    {
        var assets = await _settings.AccessConfig.ToListAsync();
        var test = new Regex("^(http(s|):\\/\\/((.*.)|)(czompi(|software|refurb)|czsoft|hunlux(school|launcher)|kamera).(hu|eu|intra|dev)\\/)").Matches(referer);
        var trusted = assets.Select(asset =>
        {
            var matches = new Regex(asset.RequestRoute ?? "").Matches(referer);
            var wildcard = asset.RequestRoute?.Equals("*.*") ?? false;
            return wildcard || matches.Any();
        }).ToList();
        var is_referer_trusted = trusted.Contains(true);
        var is_assetroute_matched = assets.Where((itm, i) => trusted[i]).Select(asset => (asset.AssetRoute?.Equals("*.*") ?? false) || (new Regex(asset.AssetRoute ?? "").Matches(assetRoute).Any())).Contains(true);
        if (!is_referer_trusted) throw new AccessViolationException($"`{referer ?? "-"}` is not a trusted host.");
        if (!is_assetroute_matched) throw new AccessViolationException($"`{referer ?? "-"}` is a trusted host, but it's not allowed to access `{assetRoute}`.");
    }
    #endregion

    #region Logging
    [NonAction]
    protected void LogActionResult(LogLevel logLevel, string fileName, object status)
    {
        _ip ??= Request?.Headers?["cf-connecting-ip"].FirstOrDefault() ?? Request?.Headers?["x-forwarded-for"].FirstOrDefault() ?? Request?.Headers?["x-real-ip"].FirstOrDefault();
#if NET6_0_OR_GREATER
        _referer ??= Request?.Headers?.Referer.FirstOrDefault();
#else
        _referer ??= Request?.Headers?["Referer"].ToString();
#endif
        _nodeId ??= Globals.ApiInformation.Node;

        _logger.Log(logLevel, "{ip} | {referer} | {nodeId} | {fileName} | {@status}", _ip, _referer, _nodeId, fileName, status);
    }

    [NonAction]
    protected void LogTrace(string fileName, string status) => LogActionResult(LogLevel.Trace, fileName, status);
    [NonAction]
    protected void LogTrace(string fileName, HttpStatusCode status) => LogActionResult(LogLevel.Trace, fileName, status);

    [NonAction]
    protected void LogDebug(string fileName, string status) => LogActionResult(LogLevel.Debug, fileName, status);
    [NonAction]
    protected void LogDebug(string fileName, HttpStatusCode status) => LogActionResult(LogLevel.Debug, fileName, status);

    [NonAction]
    protected void LogInformation(string fileName, string status) => LogActionResult(LogLevel.Information, fileName, status);
    [NonAction]
    protected void LogInformation(string fileName, HttpStatusCode status) => LogActionResult(LogLevel.Information, fileName, status);

    [NonAction]
    protected void LogWarning(string fileName, string status) => LogActionResult(LogLevel.Warning, fileName, status);
    [NonAction]
    protected void LogWarning(string fileName, HttpStatusCode status) => LogActionResult(LogLevel.Warning, fileName, status);

    [NonAction]
    protected void LogError(string fileName, string status) => LogActionResult(LogLevel.Error, fileName, status);
    [NonAction]
    protected void LogError(string fileName, HttpStatusCode status) => LogActionResult(LogLevel.Error, fileName, status);

    [NonAction]
    protected void LogCritical(string fileName, string status) => LogActionResult(LogLevel.Critical, fileName, status);
    [NonAction]
    protected void LogCritical(string fileName, HttpStatusCode status) => LogActionResult(LogLevel.Critical, fileName, status);
    #endregion
}