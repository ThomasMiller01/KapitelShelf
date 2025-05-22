// <copyright file="EPUBParserTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using KapitelShelf.Api.Logic.BookParser;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using VersOne.Epub;
using VersOne.Epub.Environment;
using VersOne.Epub.Schema;

#pragma warning disable IDE0022 // Use expression body for method
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

namespace KapitelShelf.Api.Tests.Logic.BookParser;

/// <summary>
/// Unit tests for the EPUBParser class.
/// </summary>
[TestFixture]
public class EPUBParserTests
{
    private EPUBParser testee;

    /// <summary>
    /// Sets up a new test instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testee = new EPUBParser();
    }

    /// <summary>
    /// Tests Parse throws ArgumentNullException when the file is null.
    /// </summary>
    [Test]
    public void Parse_ThrowsIfFileIsNull()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => testee.Parse(null!));
    }

    /// <summary>
    /// Tests Parse extracts title, author, description, release date, and cover.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Parse_ParsesAllMetadata()
    {
        // Arrange
        var file = Substitute.For<IFormFile>();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy data"));
        file.OpenReadStream().Returns(stream);

        var epubBook = CreateEpub(
            title: "The EPUB Title",
            metadataTitle: "Explicit Title",
            author: "First Last",
            description: "An EPUB book",
            metadataDescription: "Explicit Description",
            creationDate: "2023-05-01T00:00:00Z",
            addCover: true);

        var epubBookLoader = new EpubBookLoaderMock(epubBook);
        this.testee.BookLoader = epubBookLoader;

        // Act
        var result = await testee.Parse(file);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Book.Title, Is.EqualTo("The EPUB Title"));
            Assert.That(result.Book.Description, Is.EqualTo("An EPUB book"));
            Assert.That(result.Book.Author?.FirstName, Is.EqualTo("First"));
            Assert.That(result.Book.Author?.LastName, Is.EqualTo("Last"));
            Assert.That(result.Book.ReleaseDate?.Date, Is.EqualTo(new DateTime(2023, 5, 1).Date));
            Assert.That(result.CoverFile, Is.Not.Null);
        });
    }

    /// <summary>
    /// Tests Parse uses fallback fields for missing metadata.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Parse_UsesFallbacks_WhenFieldsMissing()
    {
        // Arrange
        var file = Substitute.For<IFormFile>();
        var stream = new MemoryStream();
        file.OpenReadStream().Returns(stream);

        var epubBook = CreateEpub(
            title: null,
            metadataTitle: "Fallback Title",
            author: null,
            description: null,
            metadataDescription: "Fallback Description",
            creationDate: null,
            addCover: false);

        var epubBookLoader = new EpubBookLoaderMock(epubBook);
        this.testee.BookLoader = epubBookLoader;

        // Act
        var result = await testee.Parse(file);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Book.Title, Is.EqualTo("Fallback Title"));
            Assert.That(result.Book.Description, Is.EqualTo("Fallback Description"));
            Assert.That(result.Book.Author?.FirstName, Is.EqualTo(string.Empty));
            Assert.That(result.Book.Author?.LastName, Is.EqualTo(string.Empty));
            Assert.That(result.Book.ReleaseDate, Is.Null);
            Assert.That(result.CoverFile, Is.Null);
        });
    }

    /// <summary>
    /// Tests Parse correctly extracts series info when present.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Parse_ParsesSeriesMetadata()
    {
        // Arrange
        var file = Substitute.For<IFormFile>();
        file.OpenReadStream().Returns(new MemoryStream());

        var epubBook = CreateEpub(
            title: "Book Title",
            metadataTitle: "Book Title",
            description: string.Empty,
            metadataDescription: null,
            author: "Test Author",
            creationDate: null,
            addCover: false,
            metaItems: new Dictionary<string, string>
            {
                { "calibre:series", "SeriesName" },
                { "calibre:series_index", "42" },
            });

        var epubBookLoader = new EpubBookLoaderMock(epubBook);
        this.testee.BookLoader = epubBookLoader;

        // Act
        var result = await testee.Parse(file);

        // Assert
        Assert.That(result.Book.Series, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Book.Series!.Name, Is.EqualTo("SeriesName"));
            Assert.That(result.Book.SeriesNumber, Is.EqualTo(42));
        });
    }

    /// <summary>
    /// Tests ParseReleaseDate parses from meta "calibre:timestamp".
    /// </summary>
    [Test]
    public void ParseReleaseDate_ParsesFromCalibreTimestamp()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-10);
        var metaItems = new List<EpubMetadataMeta>
        {
            new(
                name: "calibre:timestamp",
                content: date.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
            ),
        };
        var metadata = new EpubMetadata(
            dates: [],
            metaItems: metaItems);

        // Act
        var parsed = EPUBParser.ParseReleaseDate(metadata);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(parsed?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Is.EqualTo(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        });
    }

    /// <summary>
    /// Tests ParseSeries parses null and zero for missing series info.
    /// </summary>
    [Test]
    public void ParseSeries_ReturnsNullZero_WhenNoSeries()
    {
        var metadata = new EpubMetadata(metaItems: []);

        var (seriesName, seriesNumber) = EPUBParser.ParseSeries(metadata);

        Assert.Multiple(() =>
        {
            Assert.That(seriesName, Is.Null);
            Assert.That(seriesNumber, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Helper to create a EPUB.
    /// </summary>
    private static EpubBookRef CreateEpub(
        string? title = null,
        string? metadataTitle = null,
        string? author = null,
        string? description = null,
        string? metadataDescription = null,
        string? creationDate = null,
        bool addCover = false,
        Dictionary<string, string>? metaItems = null)
    {
        if (metaItems is null)
        {
            metaItems = [];
        }

        var file = Substitute.For<IZipFile>();

        var metadata = new EpubMetadata(
            titles: metadataTitle is not null ? [new EpubMetadataTitle(metadataTitle)] : [],
            descriptions: metadataDescription is not null ? [new EpubMetadataDescription(metadataDescription)] : [],
            dates: creationDate is not null ? [new EpubMetadataDate(creationDate)] : [],
            metaItems: metaItems.Select(x => new EpubMetadataMeta(name: x.Key, content: x.Value)).ToList());

        var manifest = new EpubManifest();
        var spine = new EpubSpine();

        var package = new EpubPackage(null, EpubVersion.EPUB_3_1, metadata, manifest, spine, null);

        var schema = new EpubSchema(package, null, null, null, string.Empty);

        EpubLocalByteContentFileRef? cover = null;
        if (addCover)
        {
            var coverMetadata = new EpubContentFileRefMetadata("cover.png", EpubContentType.IMAGE_PNG, "image/png");
            var contentLoader = new ContentLoaderMock(BookParserHelper.CoverBytes);
            cover = new EpubLocalByteContentFileRef(coverMetadata, string.Empty, contentLoader);
        }

        var content = new EpubContentRef(cover: cover);

        var epubBook = new EpubBookRef(
            file,
            null,
            title ?? string.Empty,
            author ?? string.Empty,
            author is not null ? [author] : [],
            description,
            schema,
            content);

        return epubBook;
    }

    private sealed class ContentLoaderMock(byte[] content) : IEpubContentLoader
    {
        private readonly byte[] content = content;

        public Stream GetContentStream(EpubContentFileRefMetadata contentFileRefMetadata) => throw new NotImplementedException();
        public Task<Stream> GetContentStreamAsync(EpubContentFileRefMetadata contentFileRefMetadata) => throw new NotImplementedException();
        public byte[] LoadContentAsBytes(EpubContentFileRefMetadata contentFileRefMetadata) => throw new NotImplementedException();
        public Task<byte[]> LoadContentAsBytesAsync(EpubContentFileRefMetadata contentFileRefMetadata)
        {
            return Task.FromResult(this.content);
        }

        public string LoadContentAsText(EpubContentFileRefMetadata contentFileRefMetadata) => throw new NotImplementedException();
        public Task<string> LoadContentAsTextAsync(EpubContentFileRefMetadata contentFileRefMetadata) => throw new NotImplementedException();
    }

    private sealed class EpubBookLoaderMock(EpubBookRef epubBook) : IEpubBookLoader
    {
        private readonly EpubBookRef epubBook = epubBook;

        public Task<EpubBookRef> OpenBookAsync(Stream stream) => Task.FromResult(this.epubBook);
    }
}
