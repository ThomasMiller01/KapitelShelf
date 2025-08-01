﻿// <copyright file="ICloudStoragesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Data.Models.CloudStorage;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The interface for the cloud storages logic.
/// </summary>
public interface ICloudStoragesLogic
{
    /// <summary>
    /// Check if the cloud is configured.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>True if the cloud is configured, otherwise false.</returns>
    Task<bool> IsConfigured(CloudTypeDTO cloudType);

    /// <summary>
    /// Configure the the cloud.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <param name="configureCloudDto">The configure cloud dto.</param>
    /// <returns>A task.</returns>
    Task Configure(CloudTypeDTO cloudType, ConfigureCloudDTO configureCloudDto);

    /// <summary>
    /// Configure the the cloud directory.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <param name="directory">The cloud directory.</param>
    /// <returns>A task.</returns>
    Task ConfigureDirectory(Guid storageId, string directory);

    /// <summary>
    /// Get the cloud configuration.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>The cloud configuration.</returns>
    Task<CloudConfigurationDTO> GetConfiguration(CloudTypeDTO cloudType);

    /// <summary>
    /// Get the cloud storage.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <returns>The storage model.</returns>
    Task<CloudStorageModel?> GetStorageModel(Guid storageId);

    /// <summary>
    /// Get all storages that are downloaded.
    /// </summary>
    /// <returns>The storage models.</returns>
    Task<List<CloudStorageModel>> GetDownloadedStorageModels();

    /// <summary>
    /// Get the cloud storage.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <returns>The storage dto.</returns>
    Task<CloudStorageDTO?> GetStorage(Guid storageId);

    /// <summary>
    /// Mark the storage as downloaded.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <returns>A task.</returns>
    Task MarkStorageAsDownloaded(Guid storageId);

    /// <summary>
    /// List all the cloud storages.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>The cloud storages.</returns>
    Task<List<CloudStorageDTO>> ListCloudStorages(CloudTypeDTO cloudType);

    /// <summary>
    /// List all the cloud storages.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <param name="path">The start path of the directories to list.</param>
    /// <returns>The cloud storages.</returns>
    Task<List<CloudStorageDirectoryDTO>> ListCloudStorageDirectories(Guid storageId, string path);

    /// <summary>
    /// Delete a cloud storage.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <returns>The cloud storages.</returns>
    Task<CloudStorageDTO?> DeleteCloudStorage(Guid storageId);

    /// <summary>
    /// Add a fail to import a cloud file.
    /// </summary>
    /// <param name="storage">The cloud storage.</param>
    /// <param name="fileInfo">The file info.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A task.</returns>
    Task AddCloudFileImportFail(CloudStorageDTO storage, FileInfoDTO fileInfo, string errorMessage);

    /// <summary>
    /// Check if the import for this storage and file failed before.
    /// </summary>
    /// <param name="storage">The cloud storage.</param>
    /// <param name="file">The file.</param>
    /// <returns>True if the import failed before, otherwise false.</returns>
    Task<bool> CloudFileImportFailed(CloudStorageDTO storage, IFormFile file);
}
