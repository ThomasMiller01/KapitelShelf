// <copyright file="MetadataController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Logic;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="MetadataController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The books logic.</param>
[ApiController]
[Route("metadata")]
public class MetadataController(ILogger<MetadataController> logger, MetadataLogic logic) : ControllerBase
{
    private readonly ILogger<MetadataController> logger = logger;

    private readonly MetadataLogic logic = logic;

    /// <summary>
    /// Fetch the metadata for a book from the specified source.
    /// </summary>
    /// <param name="source">The source from which to fetch the metadata.</param>
    /// <param name="title">The title of the book to scrape metadata for.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{source}")]
    public async Task<ActionResult<IList<MetadataDTO>>> GetMetadataBySource(MetadataSources source, [FromQuery] string title)
    {
        try
        {
            var metadata = await this.logic.ScrapeFromSourceAsnyc(source, title);
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error scraping metadata from: '{Source}'", source);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
