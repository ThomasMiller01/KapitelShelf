// <copyright file="ICloudStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;

namespace KapitelShelf.Api.Logic.Storage;

/// <summary>
/// BookStorage interface.
/// </summary>
public interface ICloudStorage : IStorageBase
{
    /// <summary>
    /// Save a file to the storage.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="file">The file.</param>
    /// <returns>A task.</returns>
    Task Save(CloudStorageDTO storage, string fileName, IFormFile file);

    /// <summary>
    /// Delete the specific storage directory.
    /// </summary>
    /// <param name="storage">Delete all data for a storage.</param>
    void DeleteDirectory(CloudStorageDTO storage);

    /// <summary>
    /// Get the full data path to a cloud directory.
    /// </summary>
    /// <param name="storage">The cloud storage.</param>
    /// <param name="subPath">The subpath.</param>
    /// <returns>The full path.</returns>
    string FullPath(CloudStorageDTO storage, string subPath);

    /// <summary>
    /// Get the full data path to a cloud directory.
    /// </summary>
    /// <param name="cloudType">The cloud storage type.</param>
    /// <param name="subPath">The subpath.</param>
    /// <returns>The full path.</returns>
    string FullPath(CloudTypeDTO cloudType, string subPath);
}
