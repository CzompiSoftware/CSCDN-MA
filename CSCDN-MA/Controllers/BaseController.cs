using CSCDNMA.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using CSCDNMA.Database;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using CzomPack.Extensions;
using System.IO;

namespace CSCDNMA.Controllers;

public class BaseController<T> : ControllerBase
{
    protected readonly ILogger<T> _logger;
    protected readonly ApplicationDatabaseContext _settings;

    protected string _ip = null;
    protected string _referer = null;
    protected string _nodeId = null;
    protected string _path = null;

    public BaseController(ILogger<T> logger, ApplicationDatabaseContext settings)
    {
        _logger = logger;
        _settings = settings;
    }

    #region HandleResponse
    [NonAction]
    protected async Task<IActionResult> HandleResponseAsync(AssetType assetType, string product, string version, string path)
    {
        PopulateFields();

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

    protected void PopulateFields()
    {
        var ip = Request?.Headers?["cf-connecting-ip"].FirstOrDefault();
        if(ip is null)
        {
            var xff = Request?.Headers?["x-forwarded-for"].FirstOrDefault();
            if (xff?.Contains(',') ?? false) xff = xff.Split(',').First().Trim();
            ip ??= xff?.Trim();
        }   
        var referer = Request?.Headers?.Referer.FirstOrDefault();
        var nodeId = Globals.ApiInformation.Node;
        var path = (Request?.Headers?["Path"].FirstOrDefault() ?? Request?.Headers?[":Path"].FirstOrDefault()) ?? Request?.Path;
        if (_ip != ip) _ip = ip;
        if (_referer != referer) _referer = referer;
        if (_nodeId != nodeId) _nodeId = nodeId;
        if (_path != path) _path = path;
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
        bool is_referer_trusted = false;
        bool is_assetroute_matched = false;
        var assets = await _settings.AccessConfig.ToListAsync();
        foreach (var asset in assets)
        {
            var refererWildcard = asset.RequestRoute?.Equals("*.*") ?? false;
            if(!refererWildcard)
            {
                var refererMatches = new Regex(asset.RequestRoute ?? "-+-").Matches(referer ?? "-");

                if (!refererMatches.Any()) continue;
                is_referer_trusted = true;
            }

            var assetRouteWildcard = asset.AssetRoute?.Equals("*.*") ?? false;
            if(!assetRouteWildcard)
            {
                var assetRouteMatches = new Regex(asset.AssetRoute ?? "").Matches(assetRoute ?? "/");
                if (!assetRouteMatches.Any()) continue;
                is_assetroute_matched = true;
            }

            return;
        }

        if (!is_referer_trusted)
            throw new AccessViolationException($"`{referer ?? "-"}` is not a trusted host.");

        if (!is_assetroute_matched)
            throw new AccessViolationException($"`{referer ?? "-"}` is a trusted host, but it's not allowed to access `{assetRoute}`.");
    }
    #endregion

    #region Logging
    [NonAction]
    protected void LogActionResult(LogLevel logLevel, string path, object status)
    {
        PopulateFields();

        if(Globals.Telemetry is not null)
        {
            var level = $"{logLevel}";
            if (level.EqualsIgnoreCase($"{LogLevel.Trace}")) level = $"{LogEventLevel.Verbose}";
            if (level.EqualsIgnoreCase($"{LogLevel.Critical}")) level = $"{LogEventLevel.Fatal}";
            var logEventLevel = Enum.Parse<LogEventLevel>(level, true);
            Globals.TelemetryCollector.Write(logEventLevel, "{ip} | {referer} | {nodeId} | {path} | {@status}", _ip, _referer, _nodeId, path, status);
        }

        _logger.Log(logLevel, "{ip} | {referer} | {nodeId} | {path} | {@status}", _ip, _referer, _nodeId, path, status);
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