// <copyright file="BookParserManagerTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;
using NSubstitute;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic
{
    /// <summary>
    /// Unit tests for the BookParserManager class.
    /// </summary>
    [TestFixture]
    public class BookParserManagerTests
    {
        private BookParserManager testee;

        /// <summary>
        /// Sets up a fresh BookParserManager before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.testee = new BookParserManager(
            [
                typeof(MockParser)
            ]);
        }

        /// <summary>
        /// Tests Parse returns parsing result with BookDTO when extension is supported.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_ReturnsResultWithBookDTO_WhenSupportedExtension()
        {
            // Setup
            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns("test.mock");
            var expectedBook = new BookDTO { Id = Guid.NewGuid(), Title = "Test Title" };
            var expectedResult = new BookParsingResult { Book = expectedBook };

            // Set static result on mock parser for verification
            MockParser.StaticResult = expectedResult;

            // Execute
            var result = await this.testee.Parse(formFile);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Book, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Book.Title, Is.EqualTo(expectedBook.Title));
                Assert.That(result.CoverFile, Is.Null); // default
            });
        }

        /// <summary>
        /// Tests Parse can return result with a CoverFile when the parser provides one.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_ReturnsResultWithCoverFile_WhenProvidedByParser()
        {
            // Setup
            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns("test.mock");
            var expectedBook = new BookDTO { Id = Guid.NewGuid(), Title = "With Cover" };
            var mockCover = Substitute.For<IFormFile>();
            var expectedResult = new BookParsingResult
            {
                Book = expectedBook,
                CoverFile = mockCover,
            };
            MockParser.StaticResult = expectedResult;

            // Execute
            var result = await this.testee.Parse(formFile);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Book, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Book.Title, Is.EqualTo(expectedBook.Title));
                Assert.That(result.CoverFile, Is.EqualTo(mockCover));
            });
        }

        /// <summary>
        /// Tests Parse throws if file is null.
        /// </summary>
        [Test]
        public void Parse_ThrowsIfFileIsNull()
        {
            // Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.Parse(null!));
            Assert.That(ex!.Message, Does.Contain("File must be set"));
        }

        /// <summary>
        /// Tests Parse throws if extension is unsupported.
        /// </summary>
        [Test]
        public void Parse_ThrowsIfExtensionUnsupported()
        {
            // Setup
            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns("test.unsupported");

            // Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.Parse(formFile));
            Assert.That(ex!.Message, Does.Contain("Unsupported file extension"));
        }

        /// <summary>
        /// Tests Parse throws if filename has no extension.
        /// </summary>
        [Test]
        public void Parse_ThrowsIfFileHasNoExtension()
        {
            // Setup
            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns("test");

            // Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await this.testee.Parse(formFile));
            Assert.That(ex!.Message, Does.Contain("File name must have an extension"));
        }

        /// <summary>
        /// Tests Parse selects the correct parser based on extension.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_SelectsCorrectParser()
        {
            // Use two different parser types for two extensions
            this.testee = new BookParserManager(
            [
                typeof(MockParser),
                typeof(AnotherMockParser)
            ]);

            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns("test.another");
            var expectedBook = new BookDTO { Id = Guid.NewGuid(), Title = "Another Book" };
            var expectedResult = new BookParsingResult { Book = expectedBook };
            AnotherMockParser.StaticResult = expectedResult;

            var result = await this.testee.Parse(formFile);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Book, Is.Not.Null);
            Assert.That(result.Book.Title, Is.EqualTo("Another Book"));
        }

        // Mock parser types for testing
        private sealed class MockParser : IBookParser
        {
            public static BookParsingResult StaticResult { get; set; } = new BookParsingResult
            {
                Book = new BookDTO
                {
                    Title = "default",
                },
            };

            public IReadOnlyCollection<string> SupportedExtensions => ["mock"];

            public Task<BookParsingResult> Parse(IFormFile file) => Task.FromResult(StaticResult);
        }

        private sealed class AnotherMockParser : IBookParser
        {
            public static BookParsingResult StaticResult { get; set; } = new BookParsingResult
            {
                Book = new BookDTO
                {
                    Title = "default another",
                },
            };

            public IReadOnlyCollection<string> SupportedExtensions => ["another"];

            public Task<BookParsingResult> Parse(IFormFile file) => Task.FromResult(StaticResult);
        }
    }
}
