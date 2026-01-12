// <copyright file="BooksController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Resources;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="BooksController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The books logic.</param>
/// <param name="bookStorage">The book storage.</param>
[ApiController]
[Route("books")]
public class BooksController(ILogger<BooksController> logger, IBooksLogic logic, IBookStorage bookStorage) : ControllerBase
{
    private readonly ILogger<BooksController> logger = logger;

    private readonly IBooksLogic logic = logic;

    private readonly IBookStorage bookStorage = bookStorage;

    /// <summary>
    /// Fetch all books.
    /// </summary>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<IList<BookDTO>>> GetBooks()
    {
        try
        {
            var books = await this.logic.GetBooksAsync();
            return Ok(books);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching books");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Create a new book.
    /// </summary>
    /// <param name="createBookDto">The create book dto.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    public async Task<ActionResult<BookDTO>> CreateBook(CreateBookDTO createBookDto)
    {
        try
        {
            var book = await this.logic.CreateBookAsync(createBookDto);

            return CreatedAtAction(nameof(CreateBook), book);
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
        {
            return Conflict(new { error = "A book with this title (or location) already exists." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating book with title: {Title}", createBookDto?.Title);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Search books with a search term.
    /// </summary>
    /// <param name="searchterm">The search term.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("search")]
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
    [HttpGet("search/suggestions")]
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

    /// <summary>
    /// Import a new book.
    /// </summary>
    /// <param name="bookFile">Thebook file to import.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("import")]
    public async Task<ActionResult<ImportResultDTO>> ImportBook(IFormFile bookFile, Guid? userId)
    {
        try
        {
            var importResult = await this.logic.ImportBookAsync(bookFile, userId);

            return CreatedAtAction(nameof(ImportBook), importResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error importing book");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Import a new book from its asin.
    /// </summary>
    /// <param name="asin">The asin of the book to import.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("import/asin")]
    public async Task<ActionResult<ImportResultDTO>> ImportBookFromAsin(string asin)
    {
        try
        {
            var importResult = await this.logic.ImportBookFromAsinAsync(asin);
            if (importResult is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(ImportBookFromAsin), importResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error importing book from asin");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book series.
    /// </summary>
    /// <param name="partialSeriesName">The partial series name.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete/series")]
    public async Task<ActionResult<List<string>>> AutocompleteSeries(string? partialSeriesName)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteSeriesAsync(partialSeriesName);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting series for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book author.
    /// </summary>
    /// <param name="partialAuthor">The partial author.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete/author")]
    public async Task<ActionResult<List<string>>> AutocompleteAuthor(string? partialAuthor)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteAuthorAsync(partialAuthor);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting author for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book category.
    /// </summary>
    /// <param name="partialCategoryName">The partial category name.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete/category")]
    public async Task<ActionResult<List<string>>> AutocompleteCategory(string? partialCategoryName)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteCategoryAsync(partialCategoryName);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting category for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Autocomplete the book tag.
    /// </summary>
    /// <param name="partialTagName">The partial tag name.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("autocomplete/tag")]
    public async Task<ActionResult<List<string>>> AutocompleteTag(string? partialTagName)
    {
        try
        {
            var autocompleteResult = await this.logic.AutocompleteTagAsync(partialTagName);
            return Ok(autocompleteResult);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting tag for autocomplete");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get book by the id.
    /// </summary>
    /// <param name="bookId">The id of the book to get.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{bookId}")]
    public async Task<ActionResult<BookDTO>> GetBookById(Guid bookId, Guid? userId)
    {
        try
        {
            var book = await this.logic.GetBookByIdAsync(bookId, userId);
            if (book is null)
            {
                return NotFound();
            }

            return Ok(book);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching book");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get the cover for a book.
    /// </summary>
    /// <param name="bookId">The id of the book to get the cover for.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{bookId}/cover")]
    [EnableCors("CorsPolicy")]
    public async Task<IActionResult> GetBookCover(Guid bookId)
    {
        try
        {
            var book = await this.logic.GetBookByIdAsync(bookId);
            if (book is null || book.Cover is null)
            {
                return NotFound();
            }

            var stream = this.bookStorage.Stream(book.Cover);
            if (stream is null)
            {
                return NotFound();
            }

            return File(stream, book.Cover.MimeType, book.Cover.FileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting book cover");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Add the cover for a book.
    /// </summary>
    /// <param name="bookId">The id of the book to get.</param>
    /// <param name="coverFile">The cover image file.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("{bookId}/cover")]
    public async Task<IActionResult> AddBookCover(Guid bookId, IFormFile coverFile)
    {
        try
        {
            var book = await this.logic.GetBookByIdAsync(bookId);
            if (book is null)
            {
                return NotFound();
            }

            // save file to disk
            var cover = await this.bookStorage.Save(bookId, coverFile);

            book.Cover = cover;

            await this.logic.UpdateBookAsync(bookId, book);

            return Ok();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding book cover");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get the file for a book.
    /// </summary>
    /// <param name="bookId">The id of the book to get the file for.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{bookId}/file")]
    [EnableCors("CorsPolicy")]
    public async Task<IActionResult> GetBookFile(Guid bookId)
    {
        try
        {
            var book = await this.logic.GetBookByIdAsync(bookId);
            if (book is null || book.Location is null)
            {
                return NotFound();
            }

            if (book.Location.Type != LocationTypeDTO.KapitelShelf)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = $"Location {book.Location.Type} does not store files in KapitelShelf." });
            }

            if (book.Location.FileInfo is null)
            {
                return NotFound();
            }

            var stream = this.bookStorage.Stream(book.Location.FileInfo);
            if (stream is null)
            {
                return NotFound();
            }

            return File(stream, book.Location.FileInfo.MimeType, book.Location.FileInfo.FileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting book file");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Add the file for a book.
    /// </summary>
    /// <param name="bookId">The id of the book to get.</param>
    /// <param name="bookFile">The book file.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("{bookId}/file")]
    public async Task<IActionResult> AddBookFile(Guid bookId, IFormFile bookFile)
    {
        try
        {
            var book = await this.logic.GetBookByIdAsync(bookId);
            if (book is null)
            {
                return NotFound();
            }

            // save file to disk
            var file = await this.bookStorage.Save(bookId, bookFile);

            book.Location = new LocationDTO
            {
                Type = LocationTypeDTO.KapitelShelf,
                Url = null,
                FileInfo = file,
            };

            await this.logic.UpdateBookAsync(bookId, book);

            return Ok();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding book file");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete a book.
    /// </summary>
    /// <param name="bookId">The id of the book to delete.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete("{bookId}")]
    public async Task<IActionResult> DeleteBook(Guid bookId)
    {
        try
        {
            var book = await this.logic.DeleteBookAsync(bookId);
            if (book is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting book with Id: {BookId}", bookId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a book.
    /// </summary>
    /// <param name="bookId">The id of the book to update.</param>
    /// <param name="book">The updated book.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{bookId}")]
    public async Task<IActionResult> UpdateBook(Guid bookId, BookDTO book)
    {
        try
        {
            var updatedBook = await this.logic.UpdateBookAsync(bookId, book);
            if (updatedBook is null)
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
            this.logger.LogError(ex, "Error updating book with Id: {BookId}", bookId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
