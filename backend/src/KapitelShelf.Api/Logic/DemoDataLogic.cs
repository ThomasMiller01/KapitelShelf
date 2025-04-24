// <copyright file="DemoDataLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Bogus;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="DemoDataLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
public class DemoDataLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    /// <summary>
    /// Generate demodata and insert it into the db.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task GenerateAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var bookFaker = CreateBookFaker();

        var books = bookFaker.Generate(5);
        context.Books.AddRange(books);

        await context.SaveChangesAsync();
    }

    private static Faker<BookModel> CreateBookFaker()
    {
        var seriesFaker = CreateSeriesFaker();
        var series = seriesFaker.Generate(1).First();

        return new Faker<BookModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.Title, x => x.Lorem.Sentence())
            .RuleFor(x => x.Description, x => x.Lorem.Sentences(3))

            .RuleFor(x => x.ReleaseDate, x => x.Date.Past(10).ToUniversalTime())

            .RuleFor(x => x.PageNumber, x => x.Random.Number(30, 3000))

            .RuleFor(x => x.Series, x => CreateSeriesFaker().Generate())
            .RuleFor(x => x.SeriesId, (f, u) => u.Series!.Id)
            .RuleFor(x => x.SeriesNumber, x => x.Random.Number(0, 10))

            .RuleFor(x => x.Author, x => CreateAuthorFaker().Generate())
            .RuleFor(x => x.AuthorId, (f, u) => u.Author!.Id)

            .RuleFor(x => x.Categories, (f, u) => CreateBookCategoryFaker(u.Id).GenerateBetween(1, 2).ToList())

            .RuleFor(x => x.Tags, (f, u) => CreateBookTagFaker(u.Id).GenerateBetween(1, 5).ToList())

            .RuleFor(x => x.Cover, x => CreateFileInfoFaker().Generate())
            .RuleFor(x => x.Location, x => CreateLocationFaker().Generate());
    }

    private static Faker<SeriesModel> CreateSeriesFaker()
    {
        return new Faker<SeriesModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.Name, x => x.Lorem.Sentence(1, 3));
    }

    private static Faker<AuthorModel> CreateAuthorFaker()
    {
        return new Faker<AuthorModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.FirstName, x => x.Name.FirstName())
            .RuleFor(x => x.LastName, x => x.Name.LastName());
    }

    private static Faker<BookCategoryModel> CreateBookCategoryFaker(Guid bookId)
    {
        return new Faker<BookCategoryModel>()
            .RuleFor(x => x.BookId, x => bookId)

            .RuleFor(x => x.Category, x => CreateCategoryFaker().Generate())
            .RuleFor(x => x.CategoryId, (f, u) => u.Category.Id);
    }

    private static Faker<CategoryModel> CreateCategoryFaker()
    {
        return new Faker<CategoryModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.Name, x => x.Lorem.Sentence(1, 2));
    }

    private static Faker<BookTagModel> CreateBookTagFaker(Guid bookId)
    {
        return new Faker<BookTagModel>()
            .RuleFor(x => x.BookId, x => bookId)

            .RuleFor(x => x.Tag, x => CreateTagFaker().Generate())
            .RuleFor(x => x.TagId, (f, u) => u.Tag.Id);
    }

    private static Faker<TagModel> CreateTagFaker()
    {
        return new Faker<TagModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.Name, x => x.Lorem.Sentence(1, 2));
    }

    private static Faker<FileInfoModel> CreateFileInfoFaker()
    {
        return new Faker<FileInfoModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.FilePath, x => x.System.FilePath())
            .RuleFor(x => x.FileSizeBytes, x => x.Random.Number(1024 * 1024))
            .RuleFor(x => x.MimeType, x => x.System.MimeType())

            .RuleFor(x => x.Sha256, x => x.Random.Hash());
    }

    private static Faker<LocationModel> CreateLocationFaker()
    {
        return new Faker<LocationModel>()
            .RuleFor(x => x.Id, x => x.Random.Guid())

            .RuleFor(x => x.Type, x => x.Random.Enum<LocationType>())

            .RuleFor(x => x.Url, x => x.Internet.Url())
            .RuleFor(x => x.FileInfo, x => CreateFileInfoFaker().Generate());
    }
}
