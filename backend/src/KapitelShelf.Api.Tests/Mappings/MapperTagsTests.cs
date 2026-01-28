// <copyright file="MapperTagsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the tags mapper.
/// </summary>
[TestFixture]
public class MapperTagsTests
{
    private Mapper testee;

    /// <summary>
    /// Sets up the mapper instance before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // setup
#pragma warning disable IDE0022 // Use expression body for method
        this.testee = new Mapper();
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <summary>
    /// Tests that TagModelToTagDto maps scalar properties and sets TotalBooks.
    /// </summary>
    [Test]
    public void TagModelToTagDto_MapsAllPropertiesCorrectly_AndSetsTotalBooks()
    {
        // setup
        var model = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "Fantasy",
            Books =
            [
                new BookTagModel
                {
                    Book = new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Book 1",
                        Description = "Desc 1",
                    },
                },
                new BookTagModel
                {
                    Book = new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Book 2",
                        Description = "Desc 2",
                    },
                },
            ],
        };

        // execute
        var dto = this.testee.TagModelToTagDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Name, Is.EqualTo(model.Name));
            Assert.That(dto.TotalBooks, Is.EqualTo(model.Books.Count));
        });
    }

    /// <summary>
    /// Tests that TagModelToTagDto sets TotalBooks to 0 when tag has no books.
    /// </summary>
    [Test]
    public void TagModelToTagDto_SetsTotalBooksToZero_WhenNoBooks()
    {
        // setup
        var model = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "EmptyTag",
            Books = [],
        };

        // execute
        var dto = this.testee.TagModelToTagDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto.TotalBooks, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that TagModelToTagDtoCore maps scalar properties and leaves TotalBooks at default.
    /// </summary>
    [Test]
    public void TagModelToTagDtoCore_MapsScalars_AndDoesNotSetTotalBooks()
    {
        // setup
        var model = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "CoreTag",
            Books =
            [
                new BookTagModel
                {
                    Book = new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Should be ignored",
                        Description = "Books should not be mapped.",
                    },
                },
            ],
        };

        // execute
        var dto = this.testee.TagModelToTagDtoCore(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Name, Is.EqualTo(model.Name));

            // TotalBooks is ignored on core mapping and should remain default value.
            Assert.That(dto.TotalBooks, Is.Null);
        });
    }

    /// <summary>
    /// Tests that CreateTagDtoToTagModel maps all properties correctly and ignores Id and Books.
    /// </summary>
    [Test]
    public void CreateTagDtoToTagModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateTagDTO
        {
            Name = "Science Fiction",
        };

        // execute
        var model = this.testee.CreateTagDtoToTagModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(Guid.Empty)); // ignored
            Assert.That(model.Name, Is.EqualTo(dto.Name));
            Assert.That(model.Books, Is.Empty); // ignored
        });
    }

    /// <summary>
    /// Tests that TagDtoToCreateTagDto maps all fields correctly and ignores Id.
    /// </summary>
    [Test]
    public void TagDtoToCreateTagDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new TagDTO
        {
            Id = Guid.NewGuid(),
            Name = "Horror",
        };

        // execute
        var createDto = this.testee.TagDtoToCreateTagDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.That(createDto.Name, Is.EqualTo(dto.Name));
    }
}
