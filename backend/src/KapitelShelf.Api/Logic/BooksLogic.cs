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

        // check for duplicate books
        var duplicates = await this.GetDuplicates(createBookDTO.Title, createBookDTO.Location?.Url);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        if (createBookDTO.Series is null)
        {
            // create new series based on book title
            createBookDTO.Series = new CreateSeriesDTO
            {
                Name = createBookDTO.Title,
            };
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        // check for existing series
        var series = await context.Series
            .Where(x => x.Name == createBookDTO.Series.Name)
            .FirstOrDefaultAsync();

        if (series is null)
        {
            // create new series
            series = this.mapper.Map<SeriesModel>(createBookDTO.Series);
            series.Id = Guid.NewGuid();

            context.Series.Add(series);
        }
        else
        {
            // update series.UpdatedAt
            series.UpdatedAt = DateTime.UtcNow;
        }

        // check for existing author
        AuthorModel? author = null;
        if (createBookDTO.Author is not null)
        {
            author = await context.Authors
                .Where(x => x.FirstName == createBookDTO.Author.FirstName && x.LastName == createBookDTO.Author.LastName)
                .FirstOrDefaultAsync();

            if (author is null)
            {
                // create new author
                author = this.mapper.Map<AuthorModel>(createBookDTO.Author);
                series.Id = Guid.NewGuid();

                context.Authors.Add(author);
            }
        }

        // save now, because of book foreign key constraints
        await context.SaveChangesAsync();

        var book = this.mapper.Map<BookModel>(createBookDTO);
        book.Id = Guid.NewGuid();
        book.SeriesId = series.Id;
        book.AuthorId = author?.Id;

        // map categories and check for existing categories
        foreach (var category in createBookDTO.Categories)
        {
            var categoryModel = await context.Categories
                .Where(x => x.Name == category.Name)
                .FirstOrDefaultAsync();

            if (categoryModel is null)
            {
                categoryModel = this.mapper.Map<CategoryModel>(category);
                categoryModel.Id = Guid.NewGuid();

                context.Categories.Add(categoryModel);
            }

            book.Categories.Add(new BookCategoryModel
            {
                BookId = book.Id,
                CategoryId = categoryModel.Id,
            });
        }

        // map tags and check for existing tags
        foreach (var tag in createBookDTO.Tags)
        {
            var tagModel = await context.Tags
                .Where(x => x.Name == tag.Name)
                .FirstOrDefaultAsync();

            if (tagModel is null)
            {
                tagModel = this.mapper.Map<TagModel>(tag);
                tagModel.Id = Guid.NewGuid();

                context.Tags.Add(tagModel);
            }

            book.Tags.Add(new BookTagModel
            {
                BookId = book.Id,
                TagId = tagModel.Id,
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
        if (bookDto is null)
        {
            return null;
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var book = await context.Books
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

            .FirstOrDefaultAsync();

        if (book is null)
        {
            return null;
        }

        book.Title = bookDto.Title;
        book.Description = bookDto.Description;
        book.PageNumber = bookDto.PageNumber;
        book.ReleaseDate = bookDto.ReleaseDate;

        // author
        if (bookDto.Author is not null)
        {
            book.AuthorId = bookDto.Author.Id;
            if (book.Author is null)
            {
                book.Author = new AuthorModel
                {
                    Id = bookDto.Author.Id,
                };
            }

            book.Author.FirstName = bookDto.Author.FirstName;
            book.Author.LastName = bookDto.Author.LastName;
        }
        else
        {
            book.AuthorId = null;
            book.Author = null;
        }

        // series
        book.SeriesId = bookDto.Series.Id;
        book.Series!.Name = bookDto.Series.Name;
        book.Series.UpdatedAt = bookDto.Series.UpdatedAt;
        book.SeriesNumber = bookDto.SeriesNumber;

        // cover
        if (bookDto.Cover is not null)
        {
            if (book.Cover is null)
            {
                book.Cover = new FileInfoModel
                {
                    Id = bookDto.Cover.Id,
                };
            }

            book.Cover.FilePath = bookDto.Cover.FilePath;
            book.Cover.FileSizeBytes = bookDto.Cover.FileSizeBytes;
            book.Cover.MimeType = bookDto.Cover.MimeType;
            book.Cover.Sha256 = bookDto.Cover.Sha256;
        }
        else
        {
            book.Cover = null;
        }

        // location
        if (bookDto.Location is not null)
        {
            if (book.Location is null)
            {
                book.Location = new LocationModel
                {
                    Id = bookDto.Location.Id,
                };
            }

            book.Location.Type = this.mapper.Map<LocationType>(bookDto.Location.Type);
            book.Location.Url = bookDto.Location.Url;

            // fileinfo
            if (bookDto.Location.FileInfo is not null)
            {
                if (book.Location.FileInfo is null)
                {
                    book.Location.FileInfo = new FileInfoModel
                    {
                        Id = bookDto.Location.FileInfo.Id,
                    };
                }

                book.Location.FileInfo.FilePath = bookDto.Location.FileInfo.FilePath;
                book.Location.FileInfo.FileSizeBytes = bookDto.Location.FileInfo.FileSizeBytes;
                book.Location.FileInfo.MimeType = bookDto.Location.FileInfo.MimeType;
                book.Location.FileInfo.Sha256 = bookDto.Location.FileInfo.Sha256;
            }
            else
            {
                book.Location.FileInfo = null;
            }
        }
        else
        {
            book.Location = null;
        }

        // TODO categories
        // TODO tags
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

    private async Task<IList<BookModel>> GetDuplicates(string title, string? url)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return await context.Books
            .AsNoTracking()
            .Include(x => x.Location)
                .ThenInclude(x => x.FileInfo)
            .Where(x =>
                x.Title == title ||
                (url != null && x.Location.Url == url))
            .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
