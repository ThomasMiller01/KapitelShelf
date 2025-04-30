// <copyright file="BooksController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="BooksController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The books logic.</param>
/// <param name="mapper">The mapper.</param>
[ApiController]
[Route("books")]
public class BooksController(ILogger<BooksController> logger, BooksLogic logic, IMapper mapper) : ControllerBase
{
    private readonly ILogger<BooksController> logger = logger;

    private readonly BooksLogic logic = logic;

    private readonly IMapper mapper = mapper;

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
        catch (InvalidOperationException ex)
        {
            if (ex.Message == StaticConstants.DuplicateExceptionKey)
            {
                return Conflict(new { error = "A book with this title already exists." });
            }

            // re-throw exception
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating book with title: {Title}", createBookDto?.Title);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get book by the id.
    /// </summary>
    /// <param name="bookId">The id of the book to get.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{bookId}")]
    public async Task<ActionResult<BookDTO>> GetBookById(Guid bookId)
    {
        try
        {
            var book = await this.logic.GetBookByIdAsync(bookId);
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

            // TODO save file to disk
            var filePath = "TODO";
            var sha256 = "TODO";

            var cover = this.mapper.Map<FileInfoDTO>(coverFile);
            cover.FilePath = filePath;
            cover.Sha256 = sha256;

            book.Cover = cover;

            await this.logic.UpdateBookAsync(bookId, book);

            return Ok();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching book");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a book.
    /// </summary>
    /// <param name="bookId">The id of the book to update.</param>
    /// <param name="bookDto">The updated book dto.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{bookId}")]
    public async Task<IActionResult> UpdateBook(Guid bookId, BookDTO bookDto)
    {
        try
        {
            var book = await this.logic.UpdateBookAsync(bookId, bookDto);
            if (book is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating book with Id: {BookId}", bookId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete a book.
    /// </summary>
    /// <param name="bookId">The id of the book to delete.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete("{bookId:guid}")]
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
}
