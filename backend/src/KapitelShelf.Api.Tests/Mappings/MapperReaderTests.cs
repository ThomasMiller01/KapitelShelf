// <copyright file="MapperReaderTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the reader mapper.
/// </summary>
[TestFixture]
public class MapperReaderTests
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
    /// Tests that ReadingBookModelToReadingBookDto maps the book, user, section and page.
    /// </summary>
    /// <param name="currentSection">The section to map.</param>
    /// <param name="currentPage">The page to map.</param>
    [TestCase(2, 7)]
    [TestCase(0, 0)]
    public void ReadingBookModelToReadingBookDto_MapsAllPropertiesCorrectly(int currentSection, int currentPage)
    {
        // setup
        var model = new ReadingBooksModel
        {
            Id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CurrentSection = currentSection,
            CurrentPage = currentPage,
            LastReadAt = DateTime.UtcNow,
        };

        // execute
        var dto = this.testee.ReadingBookModelToReadingBookDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.BookId, Is.EqualTo(model.BookId));
            Assert.That(dto.UserId, Is.EqualTo(model.UserId));
            Assert.That(dto.CurrentSection, Is.EqualTo(model.CurrentSection));
            Assert.That(dto.CurrentPage, Is.EqualTo(model.CurrentPage));
        });
    }
}
