// <copyright file="BookParserBaseTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser;

/// <summary>
/// Unit tests for the BookParserBase class.
/// </summary>
[TestFixture]
public class BookParserBaseTests
{
    private TestBookParserBase testee;

    /// <summary>
    /// Sets up a new test instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testee = new TestBookParserBase();
    }

    /// <summary>
    /// Tests that SanitizeText removes HTML tags and handles null/empty input.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="expected">The expected output string.</param>
    [TestCase("Some <b>bold</b> and <i>italic</i> text.", "Some bold and italic text.")]
    [TestCase("<p>Hello</p>", "Hello")]
    [TestCase("No tags here", "No tags here")]
    [TestCase(null!, "")]
    [TestCase("", "")]
    public void SanitizeText_RemovesHtmlTags_AndHandlesNull(string input, string expected)
    {
        var result = this.testee.SanitizeTextPublic(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests ParseAuthor parses and splits author names in different formats.
    /// </summary>
    /// <param name="input">The author string.</param>
    /// <param name="expectedFirst">Expected first name.</param>
    /// <param name="expectedLast">Expected last name.</param>
    [TestCase("Miller, Thomas", "Thomas", "Miller")]
    [TestCase("Jane Doe", "Jane", "Doe")]
    [TestCase("Mary Ann Smith", "Mary Ann", "Smith")]
    [TestCase("Plato", "Plato", "")]
    [TestCase(null!, "", "")]
    [TestCase("", "", "")]
    public void ParseAuthor_ParsesVariousFormats(string input, string expectedFirst, string expectedLast)
    {
        var (firstName, lastName) = this.testee.ParseAuthorPublic(input);
        Assert.Multiple(() =>
        {
            Assert.That(firstName, Is.EqualTo(expectedFirst));
            Assert.That(lastName, Is.EqualTo(expectedLast));
        });
    }

    /// <summary>
    /// Tests ParseTitle cleans and normalizes titles.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="expected">The expected cleaned output.</param>
    [TestCase("hello!@#$%^&*()world", "hello world")]
    [TestCase("   the     quick   brown   fox   ", "the quick brown fox")]
    [TestCase("Harry-Potter's: Adventures, Vol.1", "Harry-Potter's: Adventures, Vol.1")]
    [TestCase("---strange---...,,,''", "---strange---...,,,''")]
    [TestCase("hello___world", "hello world")]
    public void ParseTitle_CleansTitle(string input, string expected)
    {
        var result = this.testee.ParseTitlePublic(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests ParseTitleFromFile removes extension and cleans title.
    /// </summary>
    /// <param name="filename">The filename input.</param>
    /// <param name="expected">The expected cleaned title.</param>
    [TestCase("the_witcher_3!.pdf", "the witcher 3")]
    [TestCase("my.awesome-book_v2.2023.epub", "my.awesome-book v2.2023")]
    [TestCase(null!, "")]
    [TestCase("", "")]
    public void ParseTitleFromFile_RemovesExtensionAndCleans(string filename, string expected)
    {
        var result = this.testee.ParseTitleFromFilePublic(filename);
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// A minimal sealed test parser exposing protected members for testing.
    /// </summary>
    private sealed class TestBookParserBase : BookParserBase
    {
        public override IReadOnlyCollection<string> SupportedExtensions => ["unit"];
        public override Task<BookParsingResult> Parse(IFormFile file) => throw new NotImplementedException();
        public string SanitizeTextPublic(string text) => this.SanitizeText(text);
        public (string firstName, string lastName) ParseAuthorPublic(string author) => this.ParseAuthor(author);
        public string ParseTitlePublic(string str) => this.ParseTitle(str);
        public string ParseTitleFromFilePublic(string fileName) => this.ParseTitleFromFile(fileName);
    }
}
