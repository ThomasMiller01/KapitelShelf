// <copyright file="SeriesController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.Logic;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="SeriesController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The books logic.</param>
[ApiController]
[Route("series")]
public class SeriesController(ILogger<SeriesController> logger, SeriesLogic logic) : ControllerBase
{
    private readonly ILogger<SeriesController> logger = logger;

    private readonly SeriesLogic logic = logic;

    /// <summary>
    /// Fetch all series summaries.
    /// </summary>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("summary")]
    public async Task<ActionResult<IList<SeriesSummaryDTO>>> GetSeriesSummary()
    {
        try
        {
            var series = await this.logic.GetSeriesSummaryAsync();
            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching series summaries");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
