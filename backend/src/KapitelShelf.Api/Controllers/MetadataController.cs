// <copyright file="MetadataController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
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
        catch (HttpRequestException ex) when (ex.Message == StaticConstants.MetadataScrapingBlockedKey)
        {
            return StatusCode(403, new { error = $"Access to {source.ToSourceName()} was blocked, possibly due to bot detection." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(403, new { error = $"Could not fetch metadata from {source.ToSourceName()}." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error scraping metadata from: '{Source}'", source);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Download images server-side as a proxy for the client.
    /// </summary>
    /// <param name="coverUrl">The url of the cover to proxy.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("proxy-cover")]
    public async Task<ActionResult<IFormFile>> ProxyCover([FromQuery] string coverUrl)
    {
        try
        {
            var (data, contentType) = await this.logic.ProxyCover(coverUrl);
            return File(data, contentType);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error prroxy-fetching cover from: '{CoverUrl}'", coverUrl);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
