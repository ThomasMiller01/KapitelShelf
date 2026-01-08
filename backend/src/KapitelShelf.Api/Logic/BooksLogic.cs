// <copyright file="BooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.MetadataScraper;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="BooksLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="bookParserManager">The book parser manager.</param>
/// <param name="bookStorage">The book storage.</param>
/// <param name="metadataScraperManager">The metadata scraper manager.</param>
/// <param name="notifications">The notifications logic.</param>
public class BooksLogic(
    IDbContextFactory<KapitelShelfDBContext> dbContextFactory,
    Mapper mapper,
    IBookParserManager bookParserManager,
    IBookStorage bookStorage,
    IMetadataScraperManager metadataScraperManager,
    INotificationsLogic notifications) : IBooksLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    private readonly IBookParserManager bookParserManager = bookParserManager;

    private readonly IBookStorage bookStorage = bookStorage;

    private readonly IMetadataScraperManager metadataScraperManager = metadataScraperManager;

    private readonly INotificationsLogic notifications = notifications;

    /// <inheritdoc/>
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

            .Select(x => this.mapper.BookModelToBookDto(x))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<BookDTO?> GetBookByIdAsync(Guid bookId, Guid? userId = null)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var book = await context.Books
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

            .Select(x => this.mapper.BookModelToBookDto(x))
            .FirstOrDefaultAsync();

        // mark book as visited for user
        await this.MarkBookAsVisited(bookId, userId);

        return book;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BookDTO>> Search(string searchterm, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(searchterm))
        {
            return new PagedResult<BookDTO> { Items = [], TotalCount = 0 };
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();
        context.ChangeTracker.LazyLoadingEnabled = false;

        var query = context.BookSearchView
            .AsNoTracking()

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Cover)
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Author)
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Series)
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Location)
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            .FilterBySearchtermQuery(searchterm);

        var items = await query
            .SortBySearchtermQuery(searchterm)

            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.BookModelToBookDtoNullable(x.BookModel))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<BookDTO>
        {
            Items = items
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList(),
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
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
            series = this.mapper.CreateSeriesDtoToSeriesModel(createBookDTO.Series);
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
                author = this.mapper.CreateAuthorDtoToAuthorModel(createBookDTO.Author);
                context.Authors.Add(author);
            }
        }

        // save now, because of book foreign key constraints
        await context.SaveChangesAsync();

        var book = this.mapper.CreateBookDtoToBookModel(createBookDTO);
        book.Id = Guid.NewGuid();
        book.Series = series;
        book.Author = author;

        // clear categories and tags, as they will be added again
        book.Categories.Clear();
        book.Tags.Clear();

        // map categories and check for existing categories
        foreach (var category in createBookDTO.Categories)
        {
            var categoryModel = await context.Categories
                .Where(x => x.Name == category.Name)
                .FirstOrDefaultAsync();

            if (categoryModel is null)
            {
                categoryModel = this.mapper.CreateCategoryDtoToCategoryModel(category);
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
                tagModel = this.mapper.CreateTagDtoToTagModel(tag);
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

        return this.mapper.BookModelToBookDto(book);
    }

    /// <inheritdoc/>
    public async Task<BookDTO?> UpdateBookAsync(Guid bookId, BookDTO bookDto)
    {
        if (bookDto is null)
        {
            return null;
        }

        // check for other duplicate books
        var duplicates = await this.GetDuplicatesAsync(bookDto.Title, bookDto.Location?.Url);
        if (duplicates.Any(x => x.Id != bookId))
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
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
            .AsSingleQuery()
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
            throw new ArgumentNullException(nameof(bookDto.Series), "Series cannot be null when updating a book.");
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
                Type = this.mapper.LocationTypeDtoToLocationType(bookDto.Location.Type),
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

        return this.mapper.BookModelToBookDto(book);
    }

    /// <inheritdoc/>
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

        return this.mapper.BookModelToBookDto(book);
    }

    /// <inheritdoc/>
    public async Task<ImportResultDTO> ImportBookAsync(IFormFile file, Guid? userId = null)
    {
        if (this.bookParserManager.IsBulkFile(file))
        {
            return await this.ImportBulkBookAsync(file, userId);
        }

        return await this.ImportSingleBookAsync(file);
    }

    /// <inheritdoc/>
    public async Task<ImportResultDTO?> ImportBookFromAsinAsync(string asin)
    {
        var metadataScraper = (IAmazonScraper)this.metadataScraperManager.GetNewScraper(MetadataSources.Amazon);
        var metadata = await metadataScraper.ScrapeFromAsin(asin);
        if (metadata is null)
        {
            return null;
        }

        var importResult = new ImportResultDTO
        {
            IsBulkImport = false,
            Errors = [],
            ImportedBooks = [],
        };

        try
        {
            var bookDto = await this.CreateBookFromMetadata(metadata, MetadataSources.Amazon);
            if (bookDto.Location is not null)
            {
                // add the asin to the location
                bookDto.Location.Url = asin;
                await this.UpdateBookAsync(bookDto.Id, bookDto);
            }

            importResult.ImportedBooks.Add(new ImportBookDTO
            {
                Id = bookDto.Id,
                Title = bookDto.Title,
            });
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
        {
            importResult.Errors.Add($"{metadata.Title}: A book with this title (or location) already exists");
        }

        return importResult;
    }

    /// <inheritdoc/>
    public void DeleteFiles(Guid bookId) => this.bookStorage.DeleteDirectory(bookId);

    /// <inheritdoc/>
    public async Task<bool> BookFileExists(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var checksum = file.Checksum();

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Books
            .AsNoTracking()
            .Include(x => x.Location)
                .ThenInclude(x => x!.FileInfo)
            .AnyAsync(x => x.Location!.FileInfo!.Sha256 == checksum);
    }

    /// <inheritdoc/>
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
                    .Include(x => x.Cover)
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

    private async Task<BookDTO> CreateBookFromParsingResult(BookParsingResult parsingResult, IFormFile? file)
    {
        // create book
        var createBookDto = this.mapper.BookDtoToCreateBookDto(parsingResult.Book);
        var bookDto = await this.CreateBookAsync(createBookDto);
        ArgumentNullException.ThrowIfNull(bookDto);

        // save cover file
        if (parsingResult.CoverFile is not null)
        {
            var cover = await this.bookStorage.Save(bookDto.Id, parsingResult.CoverFile);
            bookDto.Cover = cover;
        }

        // save book file
        FileInfoDTO? locationFile = null;
        if (file is not null)
        {
            locationFile = await this.bookStorage.Save(bookDto.Id, file);
        }

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

    private async Task<BookDTO> CreateBookFromMetadata(MetadataDTO metadata, MetadataSources source)
    {
        // create book
        var createBookDto = this.mapper.MetadataDtoToCreateBookDto(metadata);
        var bookDto = await this.CreateBookAsync(createBookDto);
        ArgumentNullException.ThrowIfNull(bookDto);

        // save cover file
        var coverFile = await metadata.DownloadCover();
        if (coverFile is not null)
        {
            var cover = await this.bookStorage.Save(bookDto.Id, coverFile);
            bookDto.Cover = cover;
        }

        bookDto.Location = new LocationDTO
        {
            Type = this.mapper.MetadataSourceToLocationTypeDto(source),
        };

        // update book with new cover and location
        await this.UpdateBookAsync(bookDto.Id, bookDto);

        return bookDto;
    }

    private async Task<ImportResultDTO> ImportSingleBookAsync(IFormFile file)
    {
        var parsingResult = await this.bookParserManager.Parse(file);

        var importResult = new ImportResultDTO
        {
            IsBulkImport = false,
            Errors = [],
            ImportedBooks = [],
        };

        try
        {
            var bookDto = await this.CreateBookFromParsingResult(parsingResult, file);
            importResult.ImportedBooks.Add(new ImportBookDTO
            {
                Id = bookDto.Id,
                Title = bookDto.Title,
            });
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
        {
            importResult.Errors.Add($"{parsingResult.Book.Title}: A book with this title (or location) already exists");
        }

        return importResult;
    }

    private async Task<ImportResultDTO> ImportBulkBookAsync(IFormFile file, Guid? userId = null)
    {
        // TODO: replace import result with notifications?
        var importResult = new ImportResultDTO
        {
            IsBulkImport = true,
            Errors = [],
            ImportedBooks = [],
        };

        var bulkResults = await this.bookParserManager.ParseBulk(file);
        foreach (var bulkResult in bulkResults)
        {
            try
            {
                var bookDto = await this.CreateBookFromParsingResult(bulkResult, null);
                importResult.ImportedBooks.Add(new ImportBookDTO
                {
                    Id = bookDto.Id,
                    Title = bookDto.Title,
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == StaticConstants.DuplicateExceptionKey)
            {
                importResult.Errors.Add($"{bulkResult.Book.Title}: A book with this title (or location) already exists");
            }
        }

        if (importResult.Errors.Count > 0)
        {
            var importErrorsText = string.Join("\n- ", importResult.Errors);

            _ = this.notifications.AddNotification(
                "BookBulkImportFailed",
                titleArgs: [importResult.Errors.Count],
                messageArgs: [importErrorsText],
                type: NotificationTypeDto.Error,
                severity: NotificationSeverityDto.Medium,
                source: "Book Import",
                userId: userId);
        }

        return importResult;
    }

    private async Task MarkBookAsVisited(Guid bookId, Guid? userId = null)
    {
        if (bookId == Guid.Empty || userId is null || userId == Guid.Empty)
        {
            return;
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var visitedBook = await context.VisitedBooks
            .Where(x => x.BookId == bookId && x.UserId == userId)
            .FirstOrDefaultAsync();

        if (visitedBook == null)
        {
            context.VisitedBooks.Add(new VisitedBooksModel
            {
                BookId = bookId,
                UserId = (Guid)userId,
                VisitedAt = DateTime.UtcNow,
            });
        }
        else
        {
            visitedBook.VisitedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }
}
