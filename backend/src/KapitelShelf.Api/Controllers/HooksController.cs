// <copyright file="HooksController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="HooksController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The hooks logic.</param>
[ApiController]
[Route("hooks")]
public class HooksController(ILogger<HooksController> logger, IHooksLogic logic) : ControllerBase
{
    private readonly ILogger<HooksController> logger = logger;

    private readonly IHooksLogic logic = logic;

    /// <summary>
    /// User sessions starts, once he opened the application.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A task.</returns>
    [HttpPost("session-start")]
    public async Task<IActionResult> SessionStart(Guid userId)
    {
        try
        {
            await this.logic.DispatchSessionStartAsync(userId);
            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error dispatching hook: session-start");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
