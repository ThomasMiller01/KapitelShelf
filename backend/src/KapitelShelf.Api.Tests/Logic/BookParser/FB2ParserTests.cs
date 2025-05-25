// <copyright file="FB2ParserTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Xml;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;
using NSubstitute;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser;

/// <summary>
/// Unit tests for the FB2Parser class.
/// </summary>
[TestFixture]
public class FB2ParserTests
{
    private FB2Parser testee;

    /// <summary>
    /// Sets up a new FB2Parser before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testee = new FB2Parser();
    }

    /// <summary>
    /// Tests that Parse throws ArgumentNullException when the file is null.
    /// </summary>
    [Test]
    public void Parse_ThrowsArgumentNullException_IfFileIsNull()
    {
        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => testee.Parse(null!));
    }

    /// <summary>
    /// Tests that Parse throws if the XML is not valid.
    /// </summary>
    [Test]
    public void Parse_ThrowsIfXmlInvalid()
    {
        // Setup
        var file = CreateFile("not xml content");

        // Assert
        Assert.ThrowsAsync<XmlException>(() => testee.Parse(file));
    }

    /// <summary>
    /// Tests parsing of all fields in a valid .fb2 file.
    /// </summary>
    /// <returns>A task representing the async unit test. Returns the parsed <see cref="BookParsingResult"/>.</returns>
    [Test]
    public async Task Parse_ParsesAllFields()
    {
        // Setup
        var fb2 = CreateFB2(
            title: "Test Book",
            description: "Book description.",
            firstName: "John",
            lastName: "Doe",
            genre: "fiction",
            date: "2024-01-01",
            seriesName: "SeriesName",
            seriesNumber: "7",
            coverBase64: Convert.ToBase64String(Encoding.UTF8.GetBytes("fakeimagebytes")));

        var file = CreateFile(fb2);

        // Execute
        var result = await testee.Parse(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Title, Is.EqualTo("Test Book"));
            Assert.That(result.Book.Description, Does.Contain("Book description."));
            Assert.That(result.Book.Author?.FirstName, Is.EqualTo("John"));
            Assert.That(result.Book.Author?.LastName, Is.EqualTo("Doe"));
            Assert.That(result.Book.ReleaseDate, Is.Not.Null);

            // Adjust date for UTC
            Assert.That(result.Book.ReleaseDate?.Date, Is.EqualTo(DateTime.Parse("2023-12-31", CultureInfo.InvariantCulture).Date));
            Assert.That(result.Book.Series, Is.Not.Null);
            Assert.That(result.Book.Series!.Name, Is.EqualTo("SeriesName"));
            Assert.That(result.Book.Categories, Has.Count.EqualTo(1));
            Assert.That(result.Book.Categories![0].Name, Is.EqualTo("fiction"));
            Assert.That(result.CoverFile, Is.Not.Null);
        });
    }

    /// <summary>
    /// Tests parsing of missing/optional fields and fallbacks.
    /// </summary>
    /// <returns>A task. Returns the parsed <see cref="BookParsingResult"/> with fallbacks/defaults.</returns>
    [Test]
    public async Task Parse_ParsesFallbacks_WhenFieldsMissing()
    {
        // Setup
        var fb2 = CreateFB2(
            title: null,
            description: null,
            firstName: null,
            lastName: null,
            genre: null,
            date: null,
            seriesName: null,
            seriesNumber: null,
            coverBase64: null);

        var file = CreateFile(fb2);

        // Execute
        var result = await testee.Parse(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Title, Is.EqualTo(string.Empty));
            Assert.That(result.Book.Description, Is.EqualTo(string.Empty));
            Assert.That(result.Book.Author?.FirstName, Is.EqualTo(string.Empty));
            Assert.That(result.Book.Author?.LastName, Is.EqualTo(string.Empty));
            Assert.That(result.Book.Series, Is.Null);
            Assert.That(result.Book.Categories, Is.Empty);
            Assert.That(result.CoverFile, Is.Null);
        });
    }

    /// <summary>
    /// Tests that Parse parses author nicknames if present.
    /// </summary>
    /// <returns>A task. Returns the parsed <see cref="BookParsingResult"/> with nickname.</returns>
    [Test]
    public async Task Parse_ParsesAuthorNickname()
    {
        // Setup
        var fb2 = CreateFB2(
            title: "Book",
            firstName: null,
            lastName: "Last",
            nickname: "Nick",
            genre: null);

        var file = CreateFile(fb2);

        // Execute
        var result = await testee.Parse(file);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Book.Author?.FirstName, Is.EqualTo("Nick"));
            Assert.That(result.Book.Author?.LastName, Is.EqualTo("Last"));
        });
    }

    // ---- Helper Methods ----

    /// <summary>
    /// Helper to create an IFormFile with the given XML string.
    /// </summary>
    private static IFormFile CreateFile(string xml)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var file = Substitute.For<IFormFile>();
        file.OpenReadStream().Returns(stream);
        return file;
    }

    /// <summary>
    /// Helper to build a minimal FB2 XML for a test.
    /// </summary>
    private static string CreateFB2(
        string? title = "Test",
        string? description = "desc",
        string? firstName = "A",
        string? lastName = "B",
        string? nickname = null,
        string? genre = "fantasy",
        string? date = null,
        string? seriesName = null,
        string? seriesNumber = null,
        string? coverBase64 = null)
    {
        var @namespace = "http://www.gribuser.ru/xml/fictionbook/2.0";
        var titleInfo =
            $@"<title-info>
                    {(title != null ? $"<book-title>{title}</book-title>" : string.Empty)}
                    {(genre != null ? $"<genre>{genre}</genre>" : string.Empty)}
                    <author>
                        {(firstName != null ? $"<first-name>{firstName}</first-name>" : string.Empty)}
                        {(nickname != null ? $"<nickname>{nickname}</nickname>" : string.Empty)}
                        {(lastName != null ? $"<last-name>{lastName}</last-name>" : string.Empty)}
                    </author>
                    {(description != null ? $"<annotation>{description}</annotation>" : string.Empty)}
                    {(date != null ? $"<date value=\"{date}\"></date>" : string.Empty)}
                    {(seriesName != null ? $"<sequence name=\"{seriesName}\"{(seriesNumber != null ? $" number=\"{seriesNumber}\"" : string.Empty)}/>" : string.Empty)}
                </title-info>";

        var binary = coverBase64 != null
            ? $"<binary content-type=\"image/png\">{coverBase64}</binary>"
            : string.Empty;

        var xml =
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <FictionBook xmlns=""{@namespace}"">
                  <description>
                    {titleInfo}
                  </description>
                  {binary}
                </FictionBook>";
        return xml;
    }
}
