// <copyright file="MapperCategoriesTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the category mapper.
/// </summary>
[TestFixture]
public class MapperCategoriesTests
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
    /// Tests that CategoryModelToCategoryDto correctly maps all properties and ignores Books.
    /// </summary>
    [Test]
    public void CategoryModelToCategoryDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = "Adventure",
            Books =
            [
                new BookCategoryModel
                {
                    Book = new BookModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Book Title",
                        Description = "Desc",
                    },
                },
            ],
        };

        // execute
        var dto = this.testee.CategoryModelToCategoryDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Name, Is.EqualTo(model.Name));
        });
    }

    /// <summary>
    /// Tests that CreateCategoryDtoToCategoryModel correctly maps properties and ignores Id and Books.
    /// </summary>
    [Test]
    public void CreateCategoryDtoToCategoryModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateCategoryDTO
        {
            Name = "Fantasy",
        };

        // execute
        var model = this.testee.CreateCategoryDtoToCategoryModel(dto);

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
    /// Tests that CategoryDtoToCreateCategoryDto correctly maps all properties and ignores Id.
    /// </summary>
    [Test]
    public void CategoryDtoToCreateCategoryDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CategoryDTO
        {
            Id = Guid.NewGuid(),
            Name = "Mystery",
        };

        // execute
        var createDto = this.testee.CategoryDtoToCreateCategoryDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createDto.Name, Is.EqualTo(dto.Name));
        });
    }
}
