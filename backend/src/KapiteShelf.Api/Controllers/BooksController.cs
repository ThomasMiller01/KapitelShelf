using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace KapitelShelf.Api.Controllers;

[ApiController]
[Route("books")]
public class BooksController : ControllerBase
{
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly BooksLogic logic;

    public BooksController(BooksLogic logic)
    {
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
            this.logger.Error(ex);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet("{id:guid}")]
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
            this.logger.Error(ex);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookDTO>> CreateBook(BookDTO bookDto)
    {
        try
        {
            var book = await this.logic.CreateBook(bookDto);

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
            this.logger.Error(ex);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
