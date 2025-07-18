// <copyright file="CloudStorage.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Settings;

namespace KapitelShelf.Api.Logic.Storage;

/// <summary>
/// Interface with the filesystem to retrieve and store cloud files.
/// </summary>
/// <param name="settings">The settings.</param>
public class CloudStorage(KapitelShelfSettings settings) : StorageBase(settings), ICloudStorage
{
    /// <inheritdoc/>
    public async Task Save(CloudStorageDTO storage, string fileName, IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(file);

        var fullFilePath = this.FullPath(storage, fileName);

        var directory = Path.GetDirectoryName(fullFilePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(fullFilePath, FileMode.Create);
        await file.CopyToAsync(stream);
    }

    /// <inheritdoc/>
    public void DeleteDirectory(CloudStorageDTO storage)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var cloudTypeSubPath = CloudTypeToDirectory(storage.Type);
        var storageDataPath = Path.Combine(StaticConstants.CloudStorageConfigurationSubPath, cloudTypeSubPath, storage.CloudOwnerEmail);

        this.DeleteDirectory(storageDataPath);
    }

    /// <inheritdoc/>
    public string FullPath(CloudStorageDTO storage, string subPath = "")
    {
        ArgumentNullException.ThrowIfNull(storage);

        var cloudSubPath = Path.Combine(storage.CloudOwnerEmail, subPath);
        return this.FullPath(storage.Type, cloudSubPath);
    }

    /// <inheritdoc/>
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
