using CSCDNMA.Database;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CSCDNMA.Controllers;

[ApiController]
public class RootController : BaseController<RootController>
{
    public RootController(ILogger<RootController> logger, ApplicationDatabaseContext settings) : base(logger, settings)
    {
    }

    [HttpGet]
    [Route("api/{*remaining}")]
    public async Task<IActionResult> ApiInfoAsync(string remaining = null)
    {
        _ip ??= Request.Headers["cf-connecting-ip"].FirstOrDefault() ?? Request.Headers["x-real-ip"].FirstOrDefault() ?? Request.Headers["x-forwarded-for"].FirstOrDefault();
#if NET6_0_OR_GREATER
        _referer ??= Request.Headers.Referer.FirstOrDefault();
#else
        _referer ??= Request.Headers["Referer"].ToString();
#endif
        _nodeId ??= Request.Headers["NodeId"].ToString();

        LogInformation("~/", HttpStatusCode.OK);
        return await Task.Run(() => Ok(Globals.ApiInformation));
    }
}
