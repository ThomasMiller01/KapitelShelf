// <copyright file="BookFileTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using KapitelShelf.Api.Controllers;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Controllers;

/// <summary>
/// Tests the checksum returned alongside book downloads.
/// </summary>
[TestFixture]
public class BookFileTests
{
    /// <summary>
    /// Checks that a checksum describes the streamed file and only opted-in legacy reads request a repair.
    /// </summary>
    /// <param name="isRevision">Whether the attachment uses a revision-specific path.</param>
    /// <param name="includeChecksum">Whether the client requests the checksum.</param>
    /// <returns>A task.</returns>
    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task GetBookFile_ReturnsChecksumAndRepairsStaleLegacyMetadata(bool isRevision, bool includeChecksum)
    {
        var bookId = Guid.NewGuid();
        const string fileName = "book.txt";
        var content = Encoding.UTF8.GetBytes("Chapter from the stored file.");
        var checksum = Convert.ToHexStringLower(SHA256.HashData(content));
        var file = new FileInfoDTO
        {
            Id = Guid.NewGuid(),
            FilePath = isRevision
                ? Path.Combine(bookId.ToString(), Guid.NewGuid().ToString("N"), fileName)
                : Path.Combine(bookId.ToString(), fileName),
            Sha256 = isRevision ? checksum : new string('0', 64),
            MimeType = "text/plain",
        };
        var book = new BookDTO
        {
            Id = bookId,
            Location = new LocationDTO { Type = LocationTypeDTO.KapitelShelf, FileInfo = file },
        };
        var logic = Substitute.For<IBooksLogic>();
        logic.GetBookByIdAsync(bookId).Returns(book);
        var storage = Substitute.For<IBookStorage>();
        var tempPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempPath, content);
            using var stream = File.OpenRead(tempPath);
            storage.Stream(file).Returns(stream);
            var controller = new BooksController(
                NullLogger<BooksController>.Instance,
                logic,
                storage,
                Substitute.For<ISeriesLogic>(),
                Substitute.For<IAuthorsLogic>(),
                Substitute.For<ICategoriesLogic>(),
                Substitute.For<ITagsLogic>(),
                Substitute.For<IUsersLogic>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            };

            var result = await controller.GetBookFile(bookId, includeChecksum) as FileStreamResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.FileDownloadName, Is.EqualTo(fileName));
                Assert.That(result.EnableRangeProcessing, Is.True);
                Assert.That(result.FileStream.Position, Is.Zero);
                Assert.That(controller.Response.Headers["X-Book-File-Sha256"].ToString(), Is.EqualTo(includeChecksum ? checksum : string.Empty));
            });
            await logic.Received(!isRevision && includeChecksum ? 1 : 0).RepairBookFileChecksumAsync(bookId, file, checksum);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
