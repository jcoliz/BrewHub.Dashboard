using BrewHub.Dashboard.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrewHub.Dashboard.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScriptsController : ControllerBase
{
    private readonly ILogger<ScriptsController> _logger;

    public ScriptsController(ILogger<ScriptsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get list of all scripts
    /// </summary>
    /// <returns>Device names</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(Script[]), StatusCodes.Status200OK)]
    public ActionResult Scripts()
    {
        _logger.LogInformation("Scripts");

        var result = new Script[]
        {
            new() { Name = "Cold Mash", Environment = "Production", Version = "1.0.10", Updated = DateTimeOffset.Now - TimeSpan.FromDays(30) }
        };

        return Ok(result);
    }
}