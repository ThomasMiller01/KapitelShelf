﻿// <copyright file="SeriesController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
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
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("summary")]
    [Obsolete("Use GET /series instead", false)]
    public async Task<ActionResult<PagedResult<SeriesSummaryDTO>>> GetSeriesSummary(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24)
    {
        try
        {
            var series = await this.logic.GetSeriesSummaryAsync(page, pageSize);
            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching series summaries");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Fetch all series.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<SeriesDTO>>> GetSeries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24)
    {
        try
        {
            var series = await this.logic.GetSeriesAsync(page, pageSize);
            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching series");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get series by its id.
    /// </summary>
    /// <param name="seriesId">The id of the series to get.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{seriesId}")]
    public async Task<ActionResult<SeriesDTO>> GetSeriesById(Guid seriesId)
    {
        try
        {
            var series = await this.logic.GetSeriesByIdAsync(seriesId);
            if (series is null)
            {
                return NotFound();
            }

            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching series");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Search series with the series name.
    /// </summary>
    /// <param name="name">The series name.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<BookDTO>>> GetSearchResult(
        [FromQuery] string name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24)
    {
        try
        {
            var result = await this.logic.Search(name, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error searching series by name: {Name}", name);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Search suggestions with the series name.
    /// </summary>
    /// <param name="name">The series name.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("search/suggestions")]
    public async Task<ActionResult<List<BookDTO>>> GetSearchSuggestions([FromQuery] string name)
    {
        try
        {
            var result = await this.logic.Search(name, page: 1, pageSize: StaticConstants.MaxSearchSuggestions);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching suggestions by series name: {Name}", name);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete a series.
    /// </summary>
    /// <param name="seriesId">The id of the seriesto delete.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete("{seriesId}")]
    public async Task<IActionResult> DeleteSeries(Guid seriesId)
    {
        try
        {
            var series = await this.logic.DeleteSeriesAsync(seriesId);
            if (series is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting series with Id: {SeriesId}", seriesId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a series.
    /// </summary>
    /// <param name="seriesId">The id of the series to update.</param>
    /// <param name="series">The updated series.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{seriesId}")]
    public async Task<IActionResult> UpdateSeries(Guid seriesId, SeriesDTO series)
    {
        try
        {
            var updatedSeries = await this.logic.UpdateSeriesAsync(seriesId, series);
            if (updatedSeries is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
        {
            return Conflict(new { error = "A book with this title (or location) already exists." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating series with Id: {SeriesId}", seriesId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Merge the series into the target series.
    /// </summary>
    /// <param name="seriesId">The source series id.</param>
    /// <param name="targetSeriesId">The target series id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{seriesId}/merge/{targetSeriesId}")]
    public async Task<IActionResult> UpdateSeries(Guid seriesId, Guid targetSeriesId)
    {
        try
        {
            await this.logic.MergeSeries(seriesId, targetSeriesId);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating series with Id: {SeriesId}", seriesId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get books by the series id.
    /// </summary>
    /// <param name="seriesId">The series id of the books to get.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{seriesId}/books")]
    public async Task<ActionResult<PagedResult<BookDTO>>> GetBooksBySeriesId(
        Guid seriesId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24)
    {
        try
        {
            var series = await this.logic.GetBooksBySeriesIdAsync(seriesId, page, pageSize);
            if (series is null)
            {
                return NotFound();
            }

            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching series books");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
