// <copyright file="IStorageBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;

namespace KapitelShelf.Api.Logic.Storage;

/// <summary>
/// BookStorage interface.
/// </summary>
public interface IStorageBase
{
    /// <summary>
    /// Get the stream for a file in the specified book directory.
    /// </summary>
    /// <param name="file">The file to stream.</param>
    /// <returns>The file stream.</returns>
    FileStream? Stream(FileInfoDTO file);

    /// <summary>
    /// Delete the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    void DeleteFile(string filePath);

    /// <summary>
    /// Save a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="file">The file.</param>
    /// <returns>The file info dto.</returns>
    Task<FileInfoDTO> Save(string filePath, IFormFile file);

    /// <summary>
    /// Stream the file content.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file stream.</returns>
    FileStream? Stream(string filePath);

    /// <summary>
    /// Delete a directory.
    /// </summary>
    /// <param name="directoryPath">The directory.</param>
    void DeleteDirectory(string directoryPath);

    /// <summary>
    /// Get the full path for a subpath.
    /// </summary>
    /// <param name="subPath">The subpath.</param>
    /// <returns>The full path.</returns>
    string FullPath(string subPath);
}
