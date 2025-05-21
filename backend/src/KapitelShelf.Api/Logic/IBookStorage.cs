// <copyright file="IBookStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// BookStorage interface.
/// </summary>
public interface IBookStorage
{
    /// <summary>
    /// Save a file in the specific book directory.
    /// </summary>
    /// <param name="bookId">The id of the book directory.</param>
    /// <param name="file">The file to save.</param>
    /// <returns>A task.</returns>
    Task<FileInfoDTO> Save(Guid bookId, IFormFile file);

    /// <summary>
    /// Get the stream for a file in the specified book directory.
    /// </summary>
    /// <param name="file">The file to stream.</param>
    /// <returns>The file stream.</returns>
    FileStream? Stream(FileInfoDTO file);

    /// <summary>
    /// Delete the specific book directory.
    /// </summary>
    /// <param name="bookId">The id of the book directory.</param>
    void DeleteDirectory(Guid bookId);

    /// <summary>
    /// Delete the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    void DeleteFile(string filePath);
}
