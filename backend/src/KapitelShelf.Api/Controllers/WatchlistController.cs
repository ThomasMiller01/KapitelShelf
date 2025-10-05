// <copyright file="WatchlistController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Watchlist;
using KapitelShelf.Api.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="WatchlistController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The watchlist logic.</param>
[ApiController]
[Route("watchlist")]
public class WatchlistController(ILogger<WatchlistController> logger, IWatchlistLogic logic) : ControllerBase
{
    private readonly ILogger<WatchlistController> logger = logger;

    private readonly IWatchlistLogic logic = logic;

    /// <summary>
    /// Get the watchlists of a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<List<SeriesWatchlistDTO>>> GetWatchlistByUser(Guid userId)
    {
        try
        {
            var seriesWatchlist = await this.logic.GetWatchlistAsync(userId);
            return Ok(seriesWatchlist);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching series watchlists of user '{UserId}'", userId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Check, if this series is on the watchlist.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("series/{seriesId}/iswatched")]
    public async Task<ActionResult<bool>> IsSeriesOnWatchlist(Guid seriesId, Guid userId)
    {
        try
        {
            var isOnWatchlist = await this.logic.IsOnWatchlist(seriesId, userId);
            return Ok(isOnWatchlist);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error checking if series with Id: {SeriesId} is on the watchlist", seriesId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Add a series to the watchlist.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("series/{seriesId}/watch")]
    public async Task<IActionResult> AddSeriesToWatchlist(Guid seriesId, Guid userId)
    {
        try
        {
            var seriesWatchlist = await this.logic.AddToWatchlist(seriesId, userId);
            if (seriesWatchlist is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding series with Id: {SeriesId} to the watchlist", seriesId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Remove a series from the watchlist.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete("series/{seriesId}/watch")]
    public async Task<IActionResult> RemoveSeriesFromWatchlist(Guid seriesId, Guid userId)
    {
        try
        {
            var seriesWatchlist = await this.logic.RemoveFromWatchlist(seriesId, userId);
            if (seriesWatchlist is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error removing series with Id: {SeriesId} from the watchlist", seriesId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Add a watchlist result to the library.
    /// </summary>
    /// <param name="resultId">The result id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("result/{resultId}/library")]
    public async Task<ActionResult<BookDTO>> AddSeriesToWatchlist(Guid resultId)
    {
        try
        {
            var bookDto = await this.logic.AddResultToLibrary(resultId);
            if (bookDto is null)
            {
                return NotFound();
            }

            return Ok(bookDto);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding result with Id: {SeriesId} to the library", resultId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
