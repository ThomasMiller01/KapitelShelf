// <copyright file="IBookStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;

namespace KapitelShelf.Api.Logic.Interfaces.Storage;

/// <summary>
/// BookStorage interface.
/// </summary>
public interface IBookStorage : IStorageBase
{
    /// <summary>
    /// Save a file in the specific book directory.
    /// </summary>
    /// <param name="bookId">The id of the book directory.</param>
    /// <param name="file">The file to save.</param>
    /// <returns>A task.</returns>
    Task<FileInfoDTO> Save(Guid bookId, IFormFile file);

    /// <summary>
    /// Delete the specific book directory.
    /// </summary>
    /// <param name="bookId">The id of the book directory.</param>
    void DeleteDirectory(Guid bookId);
}
