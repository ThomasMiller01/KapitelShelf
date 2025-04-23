using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace KapitelShelf.Api.Controllers;

[ApiController]
[Route("version")]
public class VersionController : ControllerBase
{
    private readonly ILogger<BooksController> logger;

    public VersionController(ILogger<BooksController> logger)
    {
        this.logger = logger;
    }

    [HttpGet]
    public ActionResult<string> GetVersion()
    {
        try
        {
            var informationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var version = informationalVersion?.Split("+")[0];
            return Ok(version);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error parsing version");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
