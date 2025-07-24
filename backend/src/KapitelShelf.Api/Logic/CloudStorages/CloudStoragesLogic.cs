// <copyright file="CloudStoragesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.CloudStorage.RClone;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Settings;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The cloud storages base logic.
/// </summary>
public class CloudStoragesLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, KapitelShelfSettings settings, ISchedulerFactory schedulerFactory) : ICloudStoragesLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    private readonly KapitelShelfSettings settings = settings;

    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    /// <inheritdoc/>
    public async Task<bool> IsConfigured(CloudTypeDTO cloudType)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.CloudConfiguration
            .Where(x => x.Type == this.mapper.Map<CloudType>(cloudType))
            .AnyAsync();
    }

    /// <inheritdoc/>
    public async Task Configure(CloudTypeDTO cloudType, ConfigureCloudDTO configureCloudDto)
    {
        ArgumentNullException.ThrowIfNull(configureCloudDto);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var isConfigured = await this.IsConfigured(cloudType);
        if (isConfigured)
        {
            // update the current configuration
            var configuration = await context.CloudConfiguration
                .FirstOrDefaultAsync(x => x.Type == this.mapper.Map<CloudType>(cloudType));

            ArgumentNullException.ThrowIfNull(configuration);

            configuration.OAuthClientId = configureCloudDto.OAuthClientId;
        }
        else
        {
            // create a new configuration
            var configuration = new CloudConfigurationModel
            {
                Type = this.mapper.Map<CloudType>(cloudType),
                OAuthClientId = configureCloudDto.OAuthClientId,
            };
            context.CloudConfiguration.Add(configuration);
        }

        // invalidate all cloud storages and require re-authentication
        await context.CloudStorages
            .Where(x => x.Type == this.mapper.Map<CloudType>(cloudType))
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.NeedsReAuthentication, true));

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task ConfigureDirectory(Guid storageId, string directory)
    {
        ArgumentNullException.ThrowIfNull(directory);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        // update the current configuration
        var storage = await context.CloudStorages
            .FirstOrDefaultAsync(x => x.Id == storageId);

        if (storage is null)
        {
            throw new InvalidOperationException(StaticConstants.CloudStorageStorageNotFoundExceptionKey);
        }

        storage.CloudDirectory = directory;
        storage.IsDownloaded = false;
        await context.SaveChangesAsync();

        // start initial download of the cloud directory
        var scheduler = await this.schedulerFactory.GetScheduler();

        // fire and forget task to get early response to frontend
        _ = InitialStorageDownload.Schedule(scheduler, mapper.Map<CloudStorageDTO>(storage), options: TaskScheduleOptionsDTO.RestartPreset);
    }

    /// <inheritdoc/>
    public async Task<CloudConfigurationDTO> GetConfiguration(CloudTypeDTO cloudType)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.CloudConfiguration
                .Where(x => x.Type == this.mapper.Map<CloudType>(cloudType))
                .Select(x => this.mapper.Map<CloudConfigurationDTO>(x))
                .FirstAsync();
    }

    /// <inheritdoc/>
    public async Task<CloudStorageModel?> GetStorageModel(Guid storageId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.CloudStorages
                .Where(x => x.Id == storageId)
                .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<List<CloudStorageModel>> GetDownloadedStorageModels()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.CloudStorages
                .Where(x => x.IsDownloaded)
                .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<CloudStorageDTO?> GetStorage(Guid storageId)
    {
        var storageModel = await this.GetStorageModel(storageId);
        return this.mapper.Map<CloudStorageDTO>(storageModel);
    }

    /// <inheritdoc/>
    public async Task MarkStorageAsDownloaded(Guid storageId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var storage = await context.CloudStorages
                .Where(x => x.Id == storageId)
                .FirstOrDefaultAsync();

        if (storage is null)
        {
            return;
        }

        storage.IsDownloaded = true;

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<List<CloudStorageDTO>> ListCloudStorages(CloudTypeDTO cloudType)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return context.CloudStorages
            .Where(x => x.Type == this.mapper.Map<CloudType>(cloudType))
            .Select(x => this.mapper.Map<CloudStorageDTO>(x))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<CloudStorageDirectoryDTO>> ListCloudStorageDirectories(Guid storageId, string path)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var cloudStorage = await context.CloudStorages
            .Where(x => x.Id == storageId)
            .FirstOrDefaultAsync();

        if (cloudStorage is null)
        {
            return [];
        }

        try
        {
            var output = await cloudStorage.ExecuteRCloneCommand(this.settings.CloudStorage.RClone, ["lsjson", $"\"{StaticConstants.CloudStorageRCloneConfigName}:{path}\"", "--dirs-only", "--fast-list"]);
            var entries = JsonSerializer.Deserialize<List<RCloneListJsonDTO>>(output);
            ArgumentNullException.ThrowIfNull(entries);

            var directories = entries
                .Where(x => x.IsDir)
                .Select(this.mapper.Map<CloudStorageDirectoryDTO>)
                .ToList();

            return directories;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("directory not found"))
        {
            throw new InvalidOperationException(StaticConstants.CloudStorageDirectoryNotFoundExceptionKey);
        }
    }

    /// <inheritdoc/>
    public async Task<CloudStorageDTO?> DeleteCloudStorage(Guid storageId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var storage = await context.CloudStorages.FindAsync(storageId);
        if (storage is null)
        {
            return null;
        }

        // delete cloud data
        var scheduler = await this.schedulerFactory.GetScheduler();
        await RemoveStorageData.Schedule(scheduler, this.mapper.Map<CloudStorageDTO>(storage));

        context.CloudStorages.Remove(storage);
        await context.SaveChangesAsync();

        return this.mapper.Map<CloudStorageDTO>(storage);
    }

    /// <inheritdoc/>
    public async Task AddCloudFileImportFail(CloudStorageDTO storage, FileInfoDTO fileInfo, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(fileInfo);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var failedCLoudFileImport = new FailedCloudFileImportModel
        {
            StorageId = storage.Id,
            FileInfo = this.mapper.Map<FileInfoModel>(fileInfo),
            ErrorMessaget = errorMessage,
        };

        context.FailedCloudFileImports.Add(failedCLoudFileImport);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> CloudFileImportFailed(CloudStorageDTO storage, IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(file);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.FailedCloudFileImports
            .AsNoTracking()
            .Include(x => x.FileInfo)
            .AnyAsync(x => x.StorageId == storage.Id && x.FileInfo.Sha256 == file.Checksum());
    }
}
