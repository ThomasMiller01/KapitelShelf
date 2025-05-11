// <copyright file="BooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Location;
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
/// <param name="bookParserManager">The book parser manager.</param>
/// <param name="bookStorage">The book storage.</param>
public class BooksLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, BookParserManager bookParserManager, BookStorage bookStorage)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    private readonly BookParserManager bookParserManager = bookParserManager;

    private readonly BookStorage bookStorage = bookStorage;

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
        var duplicates = await this.GetDuplicatesAsync(createBookDTO.Title, createBookDTO.Location?.Url);
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
        await using var transaction = await context.Database.BeginTransactionAsync();

        // check for existing series
        var series = await context.Series
            .Where(x => x.Name == createBookDTO.Series.Name)
            .FirstOrDefaultAsync();

        if (series is null)
        {
            // create new series
            series = this.mapper.Map<SeriesModel>(createBookDTO.Series);
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
                context.Authors.Add(author);
            }
        }

        // save now, because of book foreign key constraints
        await context.SaveChangesAsync();

        var book = this.mapper.Map<BookModel>(createBookDTO);
        book.Id = Guid.NewGuid();
        book.Series = series;
        book.Author = author;

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
        await transaction.CommitAsync();

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

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var book = await context.Books
            .Include(b => b.Author)
            .Include(b => b.Series)
            .Include(b => b.Cover)
            .Include(b => b.Location)!
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                .ThenInclude(l => l.FileInfo)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Include(b => b.Tags)
                .ThenInclude(bt => bt.Tag)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book is null)
        {
            return null;
        }

        // patch book root scalars
        context.Entry(book).CurrentValues.SetValues(new
        {
            bookDto.Title,
            bookDto.Description,
            bookDto.PageNumber,
            bookDto.ReleaseDate,
            bookDto.SeriesNumber,
        });

        // upsert Author (N:1)
        if (bookDto.Author is null)
        {
            book.Author = null;
        }
        else
        {
            var author = await context.Authors
                   .SingleOrDefaultAsync(x =>
                        x.FirstName == bookDto.Author.FirstName
                        && x.LastName == bookDto.Author.LastName)
                    ?? new AuthorModel
                    {
                        FirstName = bookDto.Author.FirstName,
                        LastName = bookDto.Author.LastName,
                    };

            book.Author = author;
        }

        // upsert Series (N:1)
        if (bookDto.Series is null)
        {
            book.Series = null;
        }
        else
        {
            var series = await context.Series
                   .SingleOrDefaultAsync(x => x.Name == bookDto.Series.Name)
                    ?? new SeriesModel
                    {
                        Name = bookDto.Series.Name,
                    };

            book.Series = series;
        }

        // upsert Cover (1:1)
        if (bookDto.Cover is null)
        {
            book.Cover = null;
        }
        else
        {
            var cover = await context.FileInfos
                   .SingleOrDefaultAsync(x => x.FilePath == bookDto.Cover.FilePath)
                    ?? new FileInfoModel
                    {
                        FilePath = bookDto.Cover.FilePath,
                        FileSizeBytes = bookDto.Cover.FileSizeBytes,
                        MimeType = bookDto.Cover.MimeType,
                        Sha256 = bookDto.Cover.Sha256,
                    };

            book.Cover = cover;
        }

        // upsert Location (1:1)
        if (bookDto.Location is null)
        {
            book.Location = null;
        }
        else
        {
            book.Location = new LocationModel
            {
                Type = this.mapper.Map<LocationType>(bookDto.Location.Type),
                Url = bookDto.Location.Url,
                FileInfo = bookDto.Location.FileInfo is null
                    ? null
                    : await context.FileInfos.SingleOrDefaultAsync(x => x.FilePath == bookDto.Location.FileInfo.FilePath)
                        ?? new FileInfoModel
                        {
                            FilePath = bookDto.Location.FileInfo.FilePath,
                            FileSizeBytes = bookDto.Location.FileInfo.FileSizeBytes,
                            MimeType = bookDto.Location.FileInfo.MimeType,
                            Sha256 = bookDto.Location.FileInfo.Sha256,
                        },
            };
        }

        // wipe & rebuild Categories (M:N)
        book.Categories.Clear();
        foreach (var name in bookDto.Categories.Select(c => c.Name).Distinct())
        {
            var category = await context.Categories
                .SingleOrDefaultAsync(c => c.Name == name)
                ?? new CategoryModel { Name = name };

            book.Categories.Add(new BookCategoryModel { Category = category });
        }

        // wipe & rebuild Tags (M:N)
        book.Tags.Clear();
        foreach (var name in bookDto.Tags.Select(c => c.Name).Distinct())
        {
            var tag = await context.Tags
                .SingleOrDefaultAsync(c => c.Name == name)
                ?? new TagModel { Name = name };

            book.Tags.Add(new BookTagModel { Tag = tag });
        }

        // commit
        await context.SaveChangesAsync();

        await this.CleanupDatabase();

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

        this.DeleteFiles(book.Id);

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        await this.CleanupDatabase();

        return this.mapper.Map<BookDTO>(book);
    }

    /// <summary>
    /// Import a book from a file.
    /// </summary>
    /// <param name="file">The book file to import.</param>
    /// <returns>The imported book.</returns>
    public async Task<BookDTO> ImportBookAsync(IFormFile file)
    {
        var parsingResult = await this.bookParserManager.Parse(file);

        // create book
        var createBookDto = this.mapper.Map<CreateBookDTO>(parsingResult.Book);
        var bookDto = await this.CreateBookAsync(createBookDto);
        ArgumentNullException.ThrowIfNull(bookDto);

        // save cover file
        if (parsingResult.CoverFile is not null)
        {
            var cover = await this.bookStorage.Save(bookDto.Id, parsingResult.CoverFile);
            bookDto.Cover = cover;
        }

        // save book file
        var locationFile = await this.bookStorage.Save(bookDto.Id, file);
        bookDto.Location = new LocationDTO
        {
            Type = LocationTypeDTO.KapitelShelf,
            Url = null,
            FileInfo = locationFile,
        };

        // update book with new cover and file
        await this.UpdateBookAsync(bookDto.Id, bookDto);

        return bookDto;
    }

    /// <summary>
    /// Delete all files of a book.
    /// </summary>
    /// <param name="bookId">The id of the book.</param>
    public void DeleteFiles(Guid bookId) => this.bookStorage.DeleteDirectory(bookId);

    /// <summary>
    /// Cleanup the database of orphaned entries.
    /// </summary>
    /// <returns>A task.</returns>
    public async Task CleanupDatabase()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        // Authors
        var orphanAuthors = await context.Authors
            .Where(a => !context.Books.Any(b => b.AuthorId == a.Id))
            .ToListAsync();

        context.Authors.RemoveRange(orphanAuthors);

        // Series
        var orphanSeries = await context.Series
            .Where(s => !context.Books.Any(b => b.SeriesId == s.Id))
            .ToListAsync();

        context.Series.RemoveRange(orphanSeries);

        // Categories
        var orphanCategories = await context.Categories
            .Where(c => !context.BookCategories.Any(bc => bc.CategoryId == c.Id))
            .ToListAsync();

        context.Categories.RemoveRange(orphanCategories);

        // Tags
        var orphanTags = await context.Tags
            .Where(t => !context.BookTags.Any(bt => bt.TagId == t.Id))
            .ToListAsync();

        context.Tags.RemoveRange(orphanTags);

        // Location
        var orphanLocations = await context.Locations
            .Where(l => !context.Books
                .Include(x => x.Location)
                .Where(x => x.Location != null)
                .Any(b => b.Location!.Id == l.Id))
            .ToListAsync();

        context.Locations.RemoveRange(orphanLocations);

        // cover or location file
        var orphanFileInfos = await context.FileInfos
        .Where(fi =>
                !context.Books
                    .Include(x => x.Location)
                    .Where(x => x.Cover != null)
                    .Any(b => b.Cover!.Id == fi.Id)
                && !context.Locations
                    .Include(x => x.FileInfo)
                    .Where(x => x.FileInfo != null)
                    .Any(l => l.FileInfo!.Id == fi.Id))
            .ToListAsync();

        context.FileInfos.RemoveRange(orphanFileInfos);

        foreach (var fileInfo in orphanFileInfos)
        {
            this.bookStorage.DeleteFile(fileInfo.FilePath);
        }

        await context.SaveChangesAsync();
    }

    private async Task<IList<BookModel>> GetDuplicatesAsync(string title, string? url)
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
