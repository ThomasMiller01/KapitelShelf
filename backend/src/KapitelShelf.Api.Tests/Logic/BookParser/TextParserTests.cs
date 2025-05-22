// <copyright file="TextParserTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;
using NSubstitute;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser;

/// <summary>
/// Unit tests for the TextParser class.
/// </summary>
[TestFixture]
public class TextParserTests
{
    private TextParser testee;

    /// <summary>
    /// Gets the supported extensions for <see cref="TextParser"/> as test cases.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of supported file extensions.</returns>
    private static readonly IReadOnlyCollection<string> SupportedExtensions = new TextParser().SupportedExtensions;

    /// <summary>
    /// Sets up a new TextParser before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testee = new TextParser();
    }

    /// <summary>
    /// Tests Parse sets BookDTO.Title to the file name without extension.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Parse_SetsBookTitle_FromFileName()
    {
        // Setup
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("mybook.txt");

        // Execute
        var result = await this.testee.Parse(file);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Book, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Title, Is.EqualTo("mybook"));
            Assert.That(result.CoverFile, Is.Null);
        });
    }

    /// <summary>
    /// Tests Parse replaces underscores in filename with spaces in title.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Parse_ReplacesUnderscoresWithSpacesInTitle()
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("some_cool_title.txt");

        var result = await this.testee.Parse(file);

        Assert.That(result.Book.Title, Is.EqualTo("some cool title"));
    }

    /// <summary>
    /// Tests Parse handles filenames with multiple dots.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Parse_UsesEverythingExceptExtension_WhenMultipleDots()
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("this.is_my_book.txt");

        var result = await this.testee.Parse(file);

        Assert.That(result.Book.Title, Is.EqualTo("this.is my book"));
    }

    /// <summary>
    /// Tests Parse throws on null file.
    /// </summary>
    [Test]
    public void Parse_ThrowsOnNullFile()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => await this.testee.Parse(null!));
    }

    /// <summary>
    /// Tests that Parse works for each supported extension.
    /// </summary>
    /// <param name="ext">The file extension to test.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestCaseSource(nameof(SupportedExtensions))]
    public async Task Parse_WorksForSupportedExtension(string ext)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns($"title_case.{ext}");

        var result = await this.testee.Parse(file);

        Assert.That(result.Book.Title, Is.EqualTo("title case"), $"Failed for extension: {ext}");
    }
}
