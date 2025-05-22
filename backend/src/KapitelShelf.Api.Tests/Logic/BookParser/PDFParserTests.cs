// <copyright file="PDFParserTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using iText.Kernel.Pdf;
using KapitelShelf.Api.Logic.BookParser;
using static KapitelShelf.Api.Tests.BookParserHelper;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.BookParser
{
    /// <summary>
    /// Unit tests for the PDFParser class.
    /// </summary>
    [TestFixture]
    public class PDFParserTests
    {
        private PDFParser parser;

        /// <summary>
        /// Sets up a new test instance before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            parser = new PDFParser();
        }

        /// <summary>
        /// Tests PDFParser parses title, author, and description from a real generated PDF.
        /// </summary>
        /// <param name="title">Title to embed in the PDF.</param>
        /// <param name="author">Author to embed in the PDF.</param>
        /// <param name="subject">Subject to embed in the PDF (used for description).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestCase("My Book", "Jane Doe", "A test PDF")]
        [TestCase("Test Title", "John Smith", "Integration PDF")]
        [TestCase(null, "NoTitle", "NoTitle PDF")] // should fallback to filename
        public async Task Parse_ParsesMetadata_FromGeneratedPdf(string? title, string? author, string? subject)
        {
            // Arrange
            var pdfFile = CreatePdf(title: title, author: author, subject: subject);

            // Act
            var result = await parser.Parse(pdfFile);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Book, Is.Not.Null);

            if (!string.IsNullOrEmpty(title))
            {
                Assert.That(result.Book.Title, Is.EqualTo(title));
            }
            else
            {
                Assert.That(result.Book.Title, Is.EqualTo("untitled"));
            }

            if (!string.IsNullOrEmpty(author))
            {
                Assert.That(result.Book.Author, Is.Not.Null);
            }

            if (!string.IsNullOrEmpty(subject))
            {
                Assert.That(result.Book.Description, Is.EqualTo(subject));
            }
        }

        /// <summary>
        /// Tests PDFParser parses release date from a generated PDF.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_ParsesReleaseDate_FromPdf()
        {
            var dateStr = "D:20240522150145";
            var expected = new DateTime(2024, 5, 22, 15, 1, 45, DateTimeKind.Utc);

            var pdfFile = CreatePdf(title: "DateTest", author: "Author", subject: "Description", creationDate: dateStr);
            var result = await parser.Parse(pdfFile);

            Assert.That(result.Book.ReleaseDate, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Book.ReleaseDate.Value.Year, Is.EqualTo(expected.Year));
                Assert.That(result.Book.ReleaseDate.Value.Month, Is.EqualTo(expected.Month));
                Assert.That(result.Book.ReleaseDate.Value.Day, Is.EqualTo(expected.Day));
                Assert.That(result.Book.ReleaseDate.Value.Hour, Is.EqualTo(expected.Hour));
                Assert.That(result.Book.ReleaseDate.Value.Minute, Is.EqualTo(expected.Minute));
                Assert.That(result.Book.ReleaseDate.Value.Second, Is.EqualTo(expected.Second));
            });
        }

        /// <summary>
        /// Tests that PDFParser falls back to filename if title is not set.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_FallbacksToFilename_WhenTitleIsMissing()
        {
            var pdfFile = CreatePdf(author: "Author", subject: "Description");
            var result = await parser.Parse(pdfFile);
            Assert.That(result.Book.Title, Is.EqualTo("untitled"));
        }

        /// <summary>
        /// Tests that PDFParser parses author fields into first and last name.
        /// </summary>
        /// <param name="author">The author.</param>
        /// <param name="expectedFirst">The expected first name.</param>
        /// <param name="expectedLast">The expected last name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestCase("Doe, John", "John", "Doe")]
        [TestCase("Jane Doe", "Jane", "Doe")]
        [TestCase("Mary Ann Smith", "Mary Ann", "Smith")]
        [TestCase("Plato", "Plato", "")]
        [TestCase(null!, "", "")]
        [TestCase("", "", "")]
        public async Task Parse_ParsesAuthorFields(string author, string expectedFirst, string expectedLast)
        {
            var pdfFile = CreatePdf("TestBook", author, "Description");
            var result = await parser.Parse(pdfFile);
            Assert.That(result.Book.Author, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Book.Author.FirstName, Is.EqualTo(expectedFirst));
                Assert.That(result.Book.Author.LastName, Is.EqualTo(expectedLast));
            });
        }

        /// <summary>
        /// Tests that PDFParser uses keywords if subject is not present.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_UsesKeywordsIfNoSubject()
        {
            var keywords = "MyKeywordDesc";
            var pdfFile = CreatePdf(title: "TestBook", author: "Author", keywords: keywords);
            var result = await parser.Parse(pdfFile);
            Assert.That(result.Book.Description, Is.EqualTo(keywords));
        }

        /// <summary>
        /// Tests that PDFParser sets description as empty string if subject and keywords are both missing.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_DescriptionIsEmpty_IfNoSubjectOrKeywords()
        {
            var pdfFile = CreatePdf(title: "TestBook", author: "Author");
            var result = await parser.Parse(pdfFile);
            Assert.That(result.Book.Description, Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// Tests that PDFParser sets the correct page count.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_ParsesPageCount()
        {
            var pageCount = 3;
            var pdfFile = CreatePdf(title: "Book", author: "Author", subject: "Description", pageCount: pageCount);
            var result = await parser.Parse(pdfFile);
            Assert.That(result.Book.PageNumber, Is.EqualTo(pageCount));
        }

        /// <summary>
        /// Tests that PDFParser returns null for cover if no images are present.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_ReturnsNullCover_IfNoImage()
        {
            var pdfFile = CreatePdf(title: "Book", author: "Author", subject: "Description");
            var result = await parser.Parse(pdfFile);
            Assert.That(result.CoverFile, Is.Null);
        }

        /// <summary>
        /// Tests that PDFParser can extract a cover image if present.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task Parse_ExtractsCoverImage_IfPresent()
        {
            var pdfFile = CreatePdf(title: "WithImage", author: "Author", subject: "Description", addCover: true);
            var result = await parser.Parse(pdfFile);

            // CoverFile is not null, and is a PNG (in your implementation)
            Assert.That(result.CoverFile, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.CoverFile!.FileName, Does.EndWith(".png"));
                Assert.That(result.CoverFile.Length, Is.GreaterThan(0));
            });
        }

        /// <summary>
        /// Helper to create a PDF with a specific number of pages.
        /// </summary>
        private static InMemoryFormFile CreatePdf(string? title = null, string? author = null, string? subject = null, string? keywords = null, string? creationDate = null, int pageCount = 1, bool addCover = false)
        {
            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                using var pdf = new PdfDocument(writer);
                var info = pdf.GetDocumentInfo();

                if (title != null)
                {
                    info.SetTitle(title);
                }

                if (author != null)
                {
                    info.SetAuthor(author);
                }

                if (subject != null)
                {
                    info.SetSubject(subject);
                }

                if (keywords != null)
                {
                    info.SetKeywords(keywords);
                }

                if (creationDate != null)
                {
                    info.SetMoreInfo("CreationDate", creationDate);
                }

                for (int i = 0; i < pageCount; i++)
                {
                    pdf.AddNewPage();
                }

                if (addCover)
                {
                    // draw a small PNG image as XObject (1x1 px)
                    var imgBytes = new byte[]
                    {
                        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                        0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                        0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                        0x54, 0x08, 0xD7, 0x63, 0xF8, 0x0F, 0x00, 0x01,
                        0x01, 0x01, 0x00, 0x18, 0xDD, 0x8E, 0x9B, 0x00,
                        0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                        0x42, 0x60, 0x82,
                    };
                    var stream = new PdfStream(imgBytes);
                    stream.Put(PdfName.Type, PdfName.XObject);
                    stream.Put(PdfName.Subtype, PdfName.Image);
                    stream.Put(PdfName.Width, new PdfNumber(1));
                    stream.Put(PdfName.Height, new PdfNumber(1));
                    stream.Put(PdfName.ColorSpace, PdfName.DeviceRGB);
                    stream.Put(PdfName.BitsPerComponent, new PdfNumber(8));
                    pdf.GetFirstPage().GetResources().AddImage(stream);
                }
            }

            return new InMemoryFormFile(ms.ToArray(), $"{title ?? "untitled"}.pdf");
        }
    }
}
