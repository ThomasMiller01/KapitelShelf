// <copyright file="SearchController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="SearchController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The search logic.</param>
[ApiController]
[Route("search")]
public class SearchController(ILogger<SearchController> logger, IBooksLogic logic) : ControllerBase
{
    private readonly ILogger<SearchController> logger = logger;

    private readonly IBooksLogic logic = logic;

    /// <summary>
    /// Search books with a search term.
    /// </summary>
    /// <param name="searchterm">The search term.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    [Obsolete("Use GET /books/search instead", false)]
    public async Task<ActionResult<PagedResult<BookDTO>>> GetSearchResult(
        [FromQuery] string searchterm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24)
    {
        try
        {
            var result = await this.logic.Search(searchterm, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error searching books by searchterm: {SearchTerm}", searchterm);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Search suggestions with a search term.
    /// </summary>
    /// <param name="searchterm">The search term.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("suggestions")]
    [Obsolete("Use GET /books/search/suggestions instead", false)]
    public async Task<ActionResult<List<BookDTO>>> GetSearchSuggestions([FromQuery] string searchterm)
    {
        try
        {
            var result = await this.logic.Search(searchterm, page: 1, pageSize: StaticConstants.MaxSearchSuggestions);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching suggestions by searchterm: {SearchTerm}", searchterm);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
