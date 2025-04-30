// <copyright file="BooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;
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
            .AsSingleQuery()

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
            .AsSingleQuery()

            .Where(x => x.Id == bookId)

            .Select(x => this.mapper.Map<BookDTO>(x))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create a new book.
    /// </summary>
    /// <param name="createBookDTO">The create book dto.</param>
    /// <returns>A <see cref="Task{BookDTO}"/> representing the result of the asynchronous operation.</returns>
    public async Task<BookDTO?> CreateBookAsync(CreateBookDTO createBookDTO)
    {
        if (createBookDTO is null)
        {
            return null;
        }

        // TODO: checksum
        // check for duplicates
        var duplicates = await this.GetDuplicates(createBookDTO.Title, createBookDTO.Location?.Url, null);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        if (createBookDTO.Series is null)
        {
            // create new series based on book title
            createBookDTO.Series = new CreateSeriesDTO
            {
                Name = createBookDTO.Title,
            };
        }

        var series = this.mapper.Map<SeriesModel>(createBookDTO.Series);
        context.Series.Add(series);
        await context.SaveChangesAsync();

        var book = this.mapper.Map<BookModel>(createBookDTO);
        book.Id = Guid.NewGuid();

        book.SeriesId = series.Id;

        foreach (var category in createBookDTO.Categories)
        {
            book.Categories.Add(new BookCategoryModel
            {
                BookId = book.Id,
                Category = this.mapper.Map<CategoryModel>(category),
            });
        }

        foreach (var tag in createBookDTO.Tags)
        {
            book.Tags.Add(new BookTagModel
            {
                BookId = book.Id,
                Tag = this.mapper.Map<TagModel>(tag),
            });
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

    private async Task<IList<BookModel>> GetDuplicates(string title, string? url, string? checksum)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return await context.Books
            .AsNoTracking()
            .Include(x => x.Location)
                .ThenInclude(x => x.FileInfo)
            .Where(x =>
                x.Title == title ||
                (url != null && x.Location.Url == url) ||
                (checksum != null && x.Location.FileInfo.Sha256 == checksum))
            .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
