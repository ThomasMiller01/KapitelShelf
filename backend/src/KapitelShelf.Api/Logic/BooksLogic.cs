// <copyright file="BooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="BooksLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The auto mapper.</param>
public class BooksLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Get all books.
    /// </summary>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<IList<BookDTO>> GetBooksAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Books
            .AsNoTracking()

            .Include(x => x.Series)
            .Include(x => x.Author)
            .Include(x => x.Categories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Tags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Cover)
            .Include(x => x.Location)
#nullable disable
                .ThenInclude(x => x.FileInfo)
#nullable restore

            .Select(x => this.mapper.Map<BookDTO>(x))
            .ToListAsync();
    }

    /// <summary>
    /// Get a book by id.
    /// </summary>
    /// <param name="bookId">The id of the book to fetch.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<BookDTO?> GetBookByIdAsync(Guid bookId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Books
            .AsNoTracking()

            .Include(x => x.Author)
            .Include(x => x.Series)
            .Include(x => x.Cover)
            .Include(x => x.Location)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                .ThenInclude(x => x.FileInfo)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            .Include(x => x.Categories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Tags)
                .ThenInclude(x => x.Tag)

            .Where(x => x.Id == bookId)

            .Select(x => this.mapper.Map<BookDTO>(x))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create a new book.
    /// </summary>
    /// <param name="bookDto">The new book dto.</param>
    /// <returns>A <see cref="Task{BookDTO}"/> representing the result of the asynchronous operation.</returns>
    public async Task<BookDTO?> CreateBookAsync(BookDTO bookDto)
    {
        if (bookDto is null)
        {
            return null;
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var duplicates = await this.GetDuplicates(bookDto);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        var book = this.mapper.Map<BookModel>(bookDto);
        book.Id = Guid.NewGuid();

        foreach (var category in book.Categories)
        {
            category.CategoryId = book.Id;
        }

        foreach (var tag in book.Tags)
        {
            tag.BookId = book.Id;
        }

        context.Books.Add(book);
        await context.SaveChangesAsync();

        return this.mapper.Map<BookDTO>(book);
    }

    /// <summary>
    /// Update a book.
    /// </summary>
    /// <param name="bookId">The id of the book to update.</param>
    /// <param name="bookDto">The updated book dto.</param>
    /// <returns>A <see cref="Task{BookDTO}"/> representing the result of the asynchronous operation.</returns>
    public async Task<BookDTO?> UpdateBookAsync(Guid bookId, BookDTO bookDto)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var book = await context.Books.FindAsync(bookId);
        if (book is null)
        {
            return null;
        }

        // .Map() applies the changes to book
        this.mapper.Map(bookDto, book);
        await context.SaveChangesAsync();

        return this.mapper.Map<BookDTO>(book);
    }

    /// <summary>
    /// Delete a book.
    /// </summary>
    /// <param name="bookId">The id of the book to delete.</param>
    /// <returns>A <see cref="Task{BookDTO}"/> representing the result of the asynchronous operation.</returns>
    public async Task<BookDTO?> DeleteBookAsync(Guid bookId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var book = await context.Books.FindAsync(bookId);
        if (book is null)
        {
            return null;
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        return this.mapper.Map<BookDTO>(book);
    }

    private async Task<IList<BookModel>> GetDuplicates(BookDTO bookDto)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var bookDtoSHA = bookDto.Location?.FileInfo?.Sha256;

        return await context.Books
            .AsNoTracking()
            .Where(x => x.Title == bookDto.Title)
            .ToListAsync();
    }
}
