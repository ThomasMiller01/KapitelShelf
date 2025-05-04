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

    private async Task<FileInfoDTO> Save(string filePath, IFormFile file)
    {
        var fullFilePath = Path.Combine(this.settings.DataDir, filePath);

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
}
