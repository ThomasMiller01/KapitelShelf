// <copyright file="AuthorsController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="AuthorsController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The authors logic.</param>
[ApiController]
[Route("authors")]
public class AuthorsController(ILogger<AuthorsController> logger, IAuthorsLogic logic) : ControllerBase
{
    private readonly ILogger<AuthorsController> logger = logger;

    private readonly IAuthorsLogic logic = logic;

    /// <summary>
    /// Fetch all authors.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="sortBy">Sort the authors by this field.</param>
    /// <param name="sortDir">Sort the authors in this direction.</param>
    /// <param name="filter">Filter the authors.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuthorDTO>>> GetAuthors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] AuthorSortByDTO sortBy = AuthorSortByDTO.Default,
        [FromQuery] SortDirectionDTO sortDir = SortDirectionDTO.Desc,
        [FromQuery] string? filter = null)
    {
        try
        {
            var series = await this.logic.GetAuthorsAsync(page, pageSize, sortBy, sortDir, filter);
            return Ok(series);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching authors");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book author.
    /// </summary>
    /// <param name="partialAuthor">The partial author.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<string>>> AutocompleteAuthor(string? partialAuthor)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteAsync(partialAuthor);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting author for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
