// <copyright file="DemoDataController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="DemoDataController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The demodata logic.</param>
[ApiController]
[Route("demodata")]
public class DemoDataController(ILogger<DemoDataController> logger, DemoDataLogic logic) : Controller
{
    private readonly ILogger<DemoDataController> logger = logger;

    private readonly DemoDataLogic logic = logic;

    /// <summary>
    /// Generates and inserts demodata into the database.
    /// </summary>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
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
