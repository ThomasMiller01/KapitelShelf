// <copyright file="BookStorageTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Http;
using NSubstitute;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic;

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
        this.tempDataDir = Path.Combine(Path.GetTempPath(), "KapitelShelfTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDataDir);
        this.settings = new KapitelShelfSettings { DataDir = this.tempDataDir };
        this.testee = new BookStorage(this.settings);
    }

    /// <summary>
    /// Cleans up the temp directory after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.tempDataDir))
        {
            Directory.Delete(this.tempDataDir, recursive: true);
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
        var result = await this.testee.Save(bookId, formFile);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.FilePath, Is.EqualTo(Path.Combine(bookId.ToString(), fileName)));
            Assert.That(result.FileSizeBytes, Is.EqualTo(fileBytes.Length));
            Assert.That(File.Exists(Path.Combine(this.tempDataDir, result.FilePath)), Is.True);
        });
    }

    /// <summary>
    /// Tests Save throws if file is null.
    /// </summary>
    [Test]
    public void Save_ThrowsOnNullFile()
    {
        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await this.testee.Save(Guid.NewGuid(), null!));
    }

    /// <summary>
    /// Tests Stream returns stream for existing file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Stream_ReturnsStreamForExistingFile()
    {
        // Setup
        var bookId = Guid.NewGuid();
        var fileName = "file.txt";
        var fileContent = "data";
        var filePath = Path.Combine(bookId.ToString(), fileName);
        var fullPath = Path.Combine(this.tempDataDir, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, fileContent);

        var fileInfo = new FileInfoDTO { FilePath = filePath };

        // Execute
        using var stream = this.testee.Stream(fileInfo);

        // Assert
        Assert.That(stream, Is.Not.Null);
        using var reader = new StreamReader(stream!);
        Assert.That(reader.ReadToEnd(), Is.EqualTo(fileContent));
    }

    /// <summary>
    /// Tests Stream returns null for missing file.
    /// </summary>
    [Test]
    public void Stream_ReturnsNullForMissingFile()
    {
        // Setup
        var fileInfo = new FileInfoDTO { FilePath = "does/not/exist.pdf" };

        // Execute
        var result = this.testee.Stream(fileInfo);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests Stream throws on null FileInfoDTO.
    /// </summary>
    [Test]
    public void Stream_ThrowsOnNullFileInfo()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => this.testee.Stream(null!));
    }

    /// <summary>
    /// Tests DeleteFile removes existing file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DeleteFile_RemovesExistingFile()
    {
        // Setup
        var filePath = "to-delete.txt";
        var fullPath = Path.Combine(this.tempDataDir, filePath);
        await File.WriteAllTextAsync(fullPath, "remove me");

        // Precondition
        Assert.That(File.Exists(fullPath), Is.True);

        // Execute
        this.testee.DeleteFile(filePath);

        // Assert
        Assert.That(File.Exists(fullPath), Is.False);
    }

    /// <summary>
    /// Tests DeleteFile does nothing for non-existent file.
    /// </summary>
    [Test]
    public void DeleteFile_DoesNothingIfFileMissing()
    {
        // Execute (should not throw)
        Assert.DoesNotThrow(() => this.testee.DeleteFile("not-here.pdf"));
    }

    /// <summary>
    /// Tests DeleteDirectory removes the directory and all files.
    /// </summary>
    [Test]
    public void DeleteDirectory_RemovesDirectoryRecursively()
    {
        // Setup
        var bookId = Guid.NewGuid();
        var dir = Path.Combine(this.tempDataDir, bookId.ToString());
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "b.txt"), "b");

        // Precondition
        Assert.That(Directory.Exists(dir), Is.True);

        // Execute
        this.testee.DeleteDirectory(bookId);

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
        Assert.DoesNotThrow(() => this.testee.DeleteDirectory(bookId));
    }

    /// <summary>
    /// Tests FullPath creates correct absolute path.
    /// </summary>
    [Test]
    public void FullPath_CombinesDataDirAndRelPath()
    {
        // Use reflection to access private method
        var method = typeof(BookStorage).GetMethod("FullPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var relPath = "book/cover.jpg";
        var result = (string)method.Invoke(this.testee, [relPath])!;
        Assert.That(result, Is.EqualTo(Path.GetFullPath(Path.Combine(this.tempDataDir, relPath))));
    }
}
