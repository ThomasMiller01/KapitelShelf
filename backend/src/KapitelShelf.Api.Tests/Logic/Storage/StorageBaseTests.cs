// <copyright file="StorageBaseTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests.Logic.Storage;

/// <summary>
/// Unit tests for the BookStorage class.
/// </summary>
[TestFixture]
public class StorageBaseTests
{
    private string tempDataDir;
    private KapitelShelfSettings settings;
    private StorageBase testee;

    /// <summary>
    /// Creates a new temp directory before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.tempDataDir = Path.Combine(Path.GetTempPath(), "KapitelShelfTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDataDir);
        settings = new KapitelShelfSettings { DataDir = this.tempDataDir };
        testee = new StorageBase(settings);
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
    /// Tests save writes a file correctly.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Save_WritesFileCorrectly()
    {
        // Setup
        var filePath = "file/test.txt";
        var fileContent = "test content";
        var file = new BookParserHelper.InMemoryFormFile(Encoding.ASCII.GetBytes(fileContent), "test.txt");

        // Execute
        var result = await testee.Save(filePath, file);

        // Assert
        var fullPath = Path.Combine(this.tempDataDir, filePath);
        Assert.That(File.Exists(fullPath), Is.True);
        var savedContent = await File.ReadAllTextAsync(fullPath);
        Assert.Multiple(() =>
        {
            Assert.That(savedContent, Is.EqualTo(fileContent));
            Assert.That(result.FilePath, Is.EqualTo(filePath));
            Assert.That(result.FileSizeBytes, Is.EqualTo(file.Length));
            Assert.That(result.MimeType, Is.Not.Null.Or.Empty);
            Assert.That(result.Sha256, Is.Not.Null.Or.Empty);
        });
    }

    /// <summary>
    /// Tests save throws when file is null.
    /// </summary>
    [Test]
    public void Save_ThrowsOnNullFile()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await testee.Save("somefile.txt", null!);
        });
    }

    /// <summary>
    /// Tests save creates directory if it is missing.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Save_CreatesDirectoryIfMissing()
    {
        // Setup
        var filePath = Path.Combine("newfolder", "file.txt");
        var fileContent = "new dir";
        var file = new BookParserHelper.InMemoryFormFile(Encoding.ASCII.GetBytes(fileContent), "file.txt");

        // Execute
        await testee.Save(filePath, file);

        // Assert
        var dirPath = Path.Combine(this.tempDataDir, "newfolder");
        Assert.That(Directory.Exists(dirPath), Is.True);
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
        using var stream = testee.Stream(fileInfo);

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
        var result = testee.Stream(fileInfo);

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
        Assert.Throws<ArgumentNullException>(() => testee.Stream((FileInfoDTO)null!));
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
        testee.DeleteFile(filePath);

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
        Assert.DoesNotThrow(() => testee.DeleteFile("not-here.pdf"));
    }

    /// <summary>
    /// Tests DeleteDirectory removes directory recursively.
    /// </summary>
    [Test]
    public void DeleteDirectory_RemovesDirectoryRecursively()
    {
        // Setup
        var dir = Path.Combine(tempDataDir, "recursive");
        var file = Path.Combine(dir, "deep.txt");
        Directory.CreateDirectory(dir);
        File.WriteAllText(file, "data");
        Assert.That(Directory.Exists(dir), Is.True);

        // Execute
        testee.DeleteDirectory("recursive");

        // Assert
        Assert.That(Directory.Exists(dir), Is.False);
    }

    /// <summary>
    /// Tests DeleteDirectory does nothing if the directory is missing.
    /// </summary>
    [Test]
    public void DeleteDirectory_DoesNothingIfDirectoryMissing()
    {
        // Should not throw
        Assert.DoesNotThrow(() => testee.DeleteDirectory("notfound"));
    }

    /// <summary>
    /// Tests FullPath creates correct absolute path.
    /// </summary>
    [Test]
    public void FullPath_CombinesDataDirAndSubPath()
    {
        // Setup
        var subPath = "my/file.jpg";

        // Execute
        var result = testee.FullPath(subPath);

        // Assert
        Assert.That(result, Is.EqualTo(Path.GetFullPath(Path.Combine(this.tempDataDir, subPath))));
    }
}
