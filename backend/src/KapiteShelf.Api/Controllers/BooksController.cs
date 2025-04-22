using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

[ApiController]
[Route("books")]
public class BooksController : ControllerBase
{
    private readonly ILogger<BooksController> logger;

    private readonly BooksLogic logic;

    public BooksController(ILogger<BooksController> logger, BooksLogic logic)
    {
        this.logger = logger;
        this.logic = logic;
    }

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

    [HttpPost]
    public async Task<ActionResult<BookDTO>> CreateBook(BookDTO bookDto)
    {
        try
        {
            var book = await this.logic.CreateBookAsync(bookDto);

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
            this.logger.LogError(ex, "Error creating book with title: {title}", bookDto.Title);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet("{bookId:guid}")]
    public async Task<ActionResult<IList<BookDTO>>> GetBookById(Guid bookId)
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
            this.logger.LogError(ex, "Error fetching book with Id: {bookId}", bookId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpPut("{bookId:guid}")]
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
            this.logger.LogError(ex, "Error updating book with Id: {bookId}", bookId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

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
            this.logger.LogError(ex, "Error deleting book with Id: {bookId}", bookId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
