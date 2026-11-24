// <copyright file="BookStorageTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Http;
using NSubstitute;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.Storage;

/// <summary>
/// Unit tests for the BookStorage class.
/// </summary>
[TestFixture]
public class BookStorageTests
{
    private string tempDataDir;
    private KapitelShelfSettings settings;
    private BookStorage testee;

    /// <summary>
    /// Creates a new temp directory before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        tempDataDir = Path.Combine(Path.GetTempPath(), "KapitelShelfTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDataDir);
        settings = new KapitelShelfSettings { DataDir = tempDataDir };
        testee = new BookStorage(settings);
    }

    /// <summary>
    /// Cleans up the temp directory after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDataDir))
        {
            Directory.Delete(tempDataDir, recursive: true);
        }
    }

    /// <summary>
    /// Tests Save creates a file and returns FileInfoDTO.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Save_CreatesFileAndReturnsFileInfoDTO()
    {
        // Setup
        var bookId = Guid.NewGuid();
        var fileName = "test.txt";
        var fileContent = "hello world";
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);

        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(fileName);
        formFile.Length.Returns(fileBytes.Length);
        formFile.OpenReadStream().Returns(new MemoryStream(fileBytes));
        formFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(
            call =>
            {
                var stream = call.ArgAt<Stream>(0);
                return new MemoryStream(fileBytes).CopyToAsync(stream, cancellationToken: call.ArgAt<CancellationToken>(1));
            });

        // Execute
        var result = await testee.Save(bookId, formFile);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(Path.GetDirectoryName(Path.GetDirectoryName(result.FilePath)), Is.EqualTo(bookId.ToString()));
            Assert.That(result.FileName, Is.EqualTo(fileName));
            Assert.That(result.FileSizeBytes, Is.EqualTo(fileBytes.Length));
            Assert.That(File.Exists(Path.Combine(tempDataDir, result.FilePath)), Is.True);
        });
    }

    /// <summary>
    /// Saving a replacement with the same filename keeps both stored checksums tied to their bytes.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Save_WithSameFileName_KeepsEachRevisionAndChecksum()
    {
        var bookId = Guid.NewGuid();
        const string fileName = "book.txt";
        using var originalSource = new MemoryStream(Encoding.UTF8.GetBytes("Original chapter"));
        using var replacementSource = new MemoryStream(Encoding.UTF8.GetBytes("A different chapter"));
        var original = await testee.Save(bookId, new FormFile(originalSource, 0, originalSource.Length, "file", fileName));
        var replacement = await testee.Save(bookId, new FormFile(replacementSource, 0, replacementSource.Length, "file", fileName));
        using var originalFile = testee.Stream(original)!;
        using var replacementFile = testee.Stream(replacement)!;

        Assert.Multiple(() =>
        {
            Assert.That(original.FilePath, Is.Not.EqualTo(replacement.FilePath));
            Assert.That(original.FileName, Is.EqualTo(fileName));
            Assert.That(replacement.FileName, Is.EqualTo(fileName));
            Assert.That(original.Sha256, Is.EqualTo(originalFile.Checksum()));
            Assert.That(replacement.Sha256, Is.EqualTo(replacementFile.Checksum()));
            Assert.That(replacement.Sha256, Is.Not.EqualTo(original.Sha256));
        });
    }

    /// <summary>
    /// Tests Save throws if file is null.
    /// </summary>
    [Test]
    public void Save_ThrowsOnNullFile()
    {
        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await testee.Save(Guid.NewGuid(), null!));
    }

    /// <summary>
    /// Tests DeleteDirectory removes the directory and all files.
    /// </summary>
    [Test]
    public void DeleteDirectory_RemovesDirectoryRecursively()
    {
        // Setup
        var bookId = Guid.NewGuid();
        var dir = Path.Combine(tempDataDir, bookId.ToString());
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "b.txt"), "b");

        // Precondition
        Assert.That(Directory.Exists(dir), Is.True);

        // Execute
        testee.DeleteDirectory(bookId);

        // Assert
        Assert.That(Directory.Exists(dir), Is.False);
    }

    /// <summary>
    /// Tests DeleteDirectory does nothing if directory doesn't exist.
    /// </summary>
    [Test]
    public void DeleteDirectory_DoesNothingIfDirectoryMissing()
    {
        // Setup
        var bookId = Guid.NewGuid();

        // Execute (should not throw)
        Assert.DoesNotThrow(() => testee.DeleteDirectory(bookId));
    }
}
