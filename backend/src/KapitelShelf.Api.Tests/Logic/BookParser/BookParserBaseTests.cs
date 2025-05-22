// <copyright file="BookParserBaseTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser
{
    /// <summary>
    /// Unit tests for the BookParserBase class.
    /// </summary>
    [TestFixture]
    public class BookParserBaseTests
    {
        private TestBookParserBase testee;

        /// <summary>
        /// Test data for HTML sanitization.
        /// </summary>
        private static readonly List<TestCaseData> SanitizeTextCases =
        [
            new TestCaseData("Some <b>bold</b> and <i>italic</i> text.", "Some bold and italic text."),
            new TestCaseData("<p>Hello</p>", "Hello"),
            new TestCaseData("No tags here", "No tags here"),
            new TestCaseData(null, string.Empty),
            new TestCaseData(string.Empty, string.Empty),
        ];

        /// <summary>
        /// Test data for author parsing.
        /// </summary>
        private static readonly List<TestCaseData> AuthorCases =
        [
            new TestCaseData("Miller, Thomas", "Thomas", "Miller"),
            new TestCaseData("Jane Doe", "Jane", "Doe"),
            new TestCaseData("Mary Ann Smith", "Mary Ann", "Smith"),
            new TestCaseData("Plato", "Plato", string.Empty),
            new TestCaseData(null, string.Empty, string.Empty),
            new TestCaseData(string.Empty, string.Empty, string.Empty),
        ];

        /// <summary>
        /// Test data for title cleaning.
        /// </summary>
        private static readonly List<TestCaseData> TitleCases =
        [
            new TestCaseData("hello!@#$%^&*()world", "hello world"),
            new TestCaseData("   the     quick   brown   fox   ", "the quick brown fox"),
            new TestCaseData("Harry-Potter's: Adventures, Vol.1", "Harry-Potter's: Adventures, Vol.1"),
            new TestCaseData("---strange---...,,,''", "---strange---...,,,''"),
            new TestCaseData("hello___world", "hello world"),
        ];

        /// <summary>
        /// Test data for file name title parsing.
        /// </summary>
        private static readonly List<TestCaseData> FileTitleCases =
        [
            new TestCaseData("the_witcher_3!.pdf", "the witcher 3"),
            new TestCaseData("my.awesome-book_v2.2023.epub", "my.awesome-book v2.2023"),
            new TestCaseData(null, string.Empty),
            new TestCaseData(string.Empty, string.Empty),
        ];

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
        [TestCaseSource(nameof(SanitizeTextCases))]
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
        [TestCaseSource(nameof(AuthorCases))]
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
        [TestCaseSource(nameof(TitleCases))]
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
        [TestCaseSource(nameof(FileTitleCases))]
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
}
