using KapitelShelf.Api.Logic;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

[ApiController]
[Route("demodata")]
public class DemoDataController : Controller
{
    private readonly ILogger<DemoDataController> logger;

    private readonly DemoDataLogic logic;

    public DemoDataController(ILogger<DemoDataController> logger, DemoDataLogic logic)
    {
        this.logger = logger;
        this.logic = logic;
    }

    [HttpPut("generate")]
    public async Task<IActionResult> Generate()
    {
        try
        {
            await this.logic.GenerateAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error generating demo-data");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
