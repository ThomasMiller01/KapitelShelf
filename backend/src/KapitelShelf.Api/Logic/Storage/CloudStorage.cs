// <copyright file="CloudStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Settings;

namespace KapitelShelf.Api.Logic.Storage;

/// <summary>
/// Interface with the filesystem to retrieve and store cloud files.
/// </summary>
/// <param name="settings">The settings.</param>
public class CloudStorage(KapitelShelfSettings settings) : StorageBase(settings)
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
    /// Delete the specific storage directory.
    /// </summary>
    /// <param name="storage">Delete all data for a storage.</param>
    public void DeleteDirectory(CloudStorageDTO storage)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var cloudTypeSubPath = CloudTypeToDirectory(storage.Type);
        var storageDataPath = Path.Combine(StaticConstants.CloudStorageConfigurationSubPath, cloudTypeSubPath, storage.CloudOwnerEmail);

        this.DeleteDirectory(storageDataPath);
    }

    /// <summary>
    /// Get the full data path to a cloud directory.
    /// </summary>
    /// <param name="cloudType">The cloud storage.</param>
    /// <param name="subPath">The subpath.</param>
    /// <returns>The full path.</returns>
    public string FullPath(CloudTypeDTO cloudType, string subPath)
    {
        var cloudTypeSubPath = CloudTypeToDirectory(cloudType);
        var cloudSubPath = Path.Combine(StaticConstants.CloudStorageConfigurationSubPath, cloudTypeSubPath, subPath);

        return this.FullPath(cloudSubPath);
    }

    private static string CloudTypeToDirectory(CloudTypeDTO cloudType)
    {
        return cloudType switch
        {
            CloudTypeDTO.OneDrive => "onedrive",
            _ => "unknown_type",
        };
    }
}
