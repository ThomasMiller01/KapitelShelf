// <copyright file="DocxParserTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser;

/// <summary>
/// Unit tests for the DocxParser class.
/// </summary>
[TestFixture]
public class DocxParserTests
{
    private const string TestDataDirectory = "Docx";

    private DocxParser testee;

    /// <summary>
    /// Initializes the test class.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Not strictly needed for OpenXML, but good for consistency
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Sets up a new FB2Parser before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testee = new DocxParser();
    }

    /// <summary>
    /// Tests that Parse throws ArgumentNullException when file is null.
    /// </summary>
    [Test]
    public void Parse_ThrowsArgumentNullException_IfFileNull()
    {
        // Execute and Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => this.testee.Parse(null!));
    }

    /// <summary>
    /// Tests that Parse extracts all fields from a real .docx file.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning the parsed <see cref="BookParsingResult"/>.</returns>
    [Test]
    public async Task Parse_AllFields()
    {
        // Setup
        var file = LoadFile("AllFields.docx", TestDataDirectory);

        // Act
        var result = await this.testee.Parse(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Book, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Title, Is.EqualTo("Test Title"));
            Assert.That(result.Book.Description, Is.EqualTo("Test Subject"));
            Assert.That(result.Book.Author?.FirstName, Is.EqualTo("Thomas"));
            Assert.That(result.Book.Author?.LastName, Is.EqualTo("Miller"));
            Assert.That(result.Book.ReleaseDate, Is.EqualTo(new DateTime(2025, 5, 24, 11, 25, 0, DateTimeKind.Utc)));
        });
    }

    /// <summary>
    /// Tests that Parse uses the file name as title when no title is present.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning the parsed <see cref="BookParsingResult"/>.</returns>
    [Test]
    public async Task Parse_UsesFileNameAsTitle_WhenTitleMissing()
    {
        // Setup
        var file = LoadFile("NoTitle.docx", TestDataDirectory);

        // Execute
        var result = await this.testee.Parse(file);

        // Assert
        Assert.That(result.Book.Title, Is.EqualTo("NoTitle"));
    }

    /// <summary>
    /// Tests that Parse sets author names to empty when missing.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning the parsed <see cref="BookParsingResult"/>.</returns>
    [Test]
    public async Task Parse_EmptyAuthor_WhenNonePresent()
    {
        // Setup
        var file = LoadFile("NoAuthor.docx", TestDataDirectory);

        // Execute
        var result = await this.testee.Parse(file);

        // Assert
        Assert.That(result.Book.Author, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Author!.FirstName, Is.EqualTo(string.Empty));
            Assert.That(result.Book.Author.LastName, Is.EqualTo(string.Empty));
        });
    }

    private static IFormFile LoadFile(string fileName, string subDir = "")
    {
        var path = Testhelper.GetResourcePath(subDir + Path.DirectorySeparatorChar + fileName);
        var stream = File.OpenRead(path);

        var formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/msword",
        };
        return formFile;
    }
}
