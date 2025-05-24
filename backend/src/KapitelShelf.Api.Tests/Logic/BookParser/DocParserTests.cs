// <copyright file="DocParserTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser;

/// <summary>
/// Unit tests for the DocParser class using a real .doc file.
/// </summary>
[TestFixture]
public class DocParserTests
{
    private const string TestDataDirectory = "Doc";

    private DocParser testee;

    /// <summary>
    /// Initializes the test class.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Register legacy code page support (Windows-1252)
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Sets up a new FB2Parser before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testee = new DocParser();
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
    /// Tests that Parse reads all expected fields from the .doc file.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Parse_AllFields()
    {
        // Setup
        var file = LoadFile("AllFields.doc", subDir: TestDataDirectory);

        // Execute
        var result = await this.testee.Parse(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Book, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Title, Is.EqualTo("Test Title"));
            Assert.That(result.Book.Description, Is.EqualTo("Test Subject"));
            Assert.That(result.Book.Author!.FirstName, Is.EqualTo("Thomas"));
            Assert.That(result.Book.Author.LastName, Is.EqualTo("Miller"));
            Assert.That(result.Book.ReleaseDate, Is.EqualTo(new DateTime(2025, 05, 24, 11, 09, 0, DateTimeKind.Utc)));
            Assert.That(result.Book.PageNumber, Is.EqualTo(1));
            Assert.That(result.CoverFile, Is.Null);
        });
    }

    /// <summary>
    /// Tests that Parse uses file name as title if none set.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Parse_UsesFileNameAsTitle_WhenTitleIsEmpty()
    {
        // Setup
        var file = LoadFile("NoTitle.doc", subDir: TestDataDirectory);

        // Execute
        var result = await this.testee.Parse(file);

        // Assert
        Assert.That(result.Book.Title, Is.EqualTo("NoTitle"));
    }

    /// <summary>
    /// Tests that Parse assigns empty author if not set.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Parse_EmptyAuthor_WhenNonePresent()
    {
        // Setup
        var file = LoadFile("NoAuthor.doc", subDir: TestDataDirectory);

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
