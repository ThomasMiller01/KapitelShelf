// <copyright file="BookStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Settings;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Interface with the filesystem to retrieve and store book files.
/// </summary>
/// <param name="settings">The settings.</param>
public class BookStorage(KapitelShelfSettings settings)
{
    private readonly KapitelShelfSettings settings = settings;

    /// <summary>
    /// Save a file in the specific book directory.
    /// </summary>
    /// <param name="bookId">The id of the book directory.</param>
    /// <param name="file">The file to save.</param>
    /// <returns>A task.</returns>
    public async Task<FileInfoDTO> Save(Guid bookId, IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var filePath = Path.Combine(bookId.ToString(), file.FileName);
        return await Save(filePath, file);
    }

    /// <summary>
    /// Get the stream for a file in the specified book directory.
    /// </summary>
    /// <param name="file">The file to stream.</param>
    /// <returns>The file stream.</returns>
    public FileStream? Stream(FileInfoDTO file)
    {
        ArgumentNullException.ThrowIfNull(file);

        return this.Stream(file.FilePath);
    }

    private async Task<FileInfoDTO> Save(string filePath, IFormFile file)
    {
        var fullFilePath = this.FullFilePath(filePath);

        var directory = Path.GetDirectoryName(fullFilePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(fullFilePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return new FileInfoDTO
        {
            FilePath = filePath,
            FileSizeBytes = file.Length,
            MimeType = file.GetMimeType(),
            Sha256 = stream.Checksum(),
        };
    }

    private FileStream? Stream(string filePath)
    {
        var fullFilePath = this.FullFilePath(filePath);
        if (!File.Exists(fullFilePath))
        {
            return null;
        }

        return File.OpenRead(fullFilePath);
    }

    private string FullFilePath(string filePath)
    {
        var combinedPath = Path.Combine(this.settings.DataDir, filePath);
        var absPath = Path.GetFullPath(combinedPath);
        return absPath;
    }
}
