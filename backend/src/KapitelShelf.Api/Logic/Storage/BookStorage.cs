// <copyright file="BookStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Settings;

namespace KapitelShelf.Api.Logic.Storage;

/// <summary>
/// Interface with the filesystem to retrieve and store book files.
/// </summary>
/// <param name="settings">The settings.</param>
public class BookStorage(KapitelShelfSettings settings) : StorageBase(settings), IBookStorage
{
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
    /// Delete the specific book directory.
    /// </summary>
    /// <param name="bookId">The id of the book directory.</param>
    public void DeleteDirectory(Guid bookId) => DeleteDirectory(bookId.ToString());
}
