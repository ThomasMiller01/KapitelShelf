// <copyright file="MapperAuthorsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the author mapper.
/// </summary>
[TestFixture]
public class MapperAuthorsTests
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
    /// Tests that AuthorModelToAuthorDto correctly maps all properties.
    /// </summary>
    [Test]
    public void AuthorModelToAuthorDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new AuthorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Books =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Book",
                    Description = "A book to test mapping.",
                },
            ],
        };

        // execute
        var dto = this.testee.AuthorModelToAuthorDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.FirstName, Is.EqualTo(model.FirstName));
            Assert.That(dto.LastName, Is.EqualTo(model.LastName));
        });
    }

    /// <summary>
    /// Tests that CreateAuthorDtoToAuthorModel correctly maps all properties and ignores Id and Books.
    /// </summary>
    [Test]
    public void CreateAuthorDtoToAuthorModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateAuthorDTO
        {
            FirstName = "Jane",
            LastName = "Smith",
        };

        // execute
        var model = this.testee.CreateAuthorDtoToAuthorModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(Guid.Empty)); // ignored
            Assert.That(model.Books, Is.Empty); // ignored
            Assert.That(model.FirstName, Is.EqualTo(dto.FirstName));
            Assert.That(model.LastName, Is.EqualTo(dto.LastName));
        });
    }

    /// <summary>
    /// Tests that AuthorDtoToCreateAuthorDto correctly maps all properties and ignores Id.
    /// </summary>
    [Test]
    public void AuthorDtoToCreateAuthorDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new AuthorDTO
        {
            Id = Guid.NewGuid(),
            FirstName = "Alan",
            LastName = "Walker",
        };

        // execute
        var createDto = this.testee.AuthorDtoToCreateAuthorDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createDto.FirstName, Is.EqualTo(dto.FirstName));
            Assert.That(createDto.LastName, Is.EqualTo(dto.LastName));
        });
    }

    /// <summary>
    /// Tests that SplitName correctly splits full names into first and last names.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="expectedFirst">The expected first name.</param>
    /// <param name="expectedLast">The expected last name.</param>
    [TestCase("", "", "")]
    [TestCase("Doe", "", "Doe")]
    [TestCase("John Doe", "John", "Doe")]
    [TestCase("John Michael Doe", "John", "Michael Doe")]
    public void SplitName_SplitsCorrectly(string input, string expectedFirst, string expectedLast)
    {
        // execute
        var (firstName, lastName) = Mapper.SplitName(input);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(firstName, Is.EqualTo(expectedFirst));
            Assert.That(lastName, Is.EqualTo(expectedLast));
        });
    }
}
