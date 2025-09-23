// <copyright file="CloudStoragesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.CloudStorage.RClone;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Api.Utils;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The cloud storages base logic.
/// </summary>
public class CloudStoragesLogic(
    IDbContextFactory<KapitelShelfDBContext> dbContextFactory,
    IMapper mapper,
    KapitelShelfSettings settings,
    ISchedulerFactory schedulerFactory,
    IProcessUtils processUtils,
    ICloudStorage storage,
    ILogger<CloudStoragesLogic> logger,
    IBookParserManager bookParserManager,
    IBooksLogic booksLogic,
    IDynamicSettingsManager dynamicSettings) : ICloudStoragesLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    private readonly KapitelShelfSettings settings = settings;

    private readonly ISchedulerFactory schedulerFactory = schedulerFactory;

    private readonly IProcessUtils processUtils = processUtils;

    private readonly ICloudStorage storage = storage;

    private readonly ILogger<CloudStoragesLogic> logger = logger;

    private readonly IBookParserManager bookParserManager = bookParserManager;

    private readonly IBooksLogic booksLogic = booksLogic;

    private readonly IDynamicSettingsManager dynamicSettings = dynamicSettings;

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
            var output = await cloudStorage.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "lsjson",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{path}\"",
                    "--dirs-only",
                    "--fast-list"
                ],
                this.processUtils);

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

    /// <inheritdoc/>
    public async Task SyncStorage(CloudStorageDTO storage, Action<string>? onOutput = null, Action<Process>? onProcessStarted = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var storageModel = await this.GetStorageModel(storage.Id);
        ArgumentNullException.ThrowIfNull(storageModel);

        var localPath = this.storage.FullPath(storage, StaticConstants.CloudStorageCloudDataSubPath);

        var useExperimentalBisync = await this.dynamicSettings.GetAsync<bool>(StaticConstants.DynamicSettingCloudStorageExperimentalBisync);
        if (useExperimentalBisync.Value && StaticConstants.StoragesSupportRCloneBisync.Contains(storage.Type))
        {
            // use rclone bisync
            await storageModel.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "bisync",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--resilient",
                    "--recover",
                    "--conflict-resolve=newer",
                    "--max-lock=2m",
                    "--max-delete=100",
                    "--stats=1s",
                    "--stats-log-level=NOTICE",
                    "--use-json-log",
                ],
                this.processUtils,
                onStderr: onOutput,
                onProcessStarted: onProcessStarted,
                cancellationToken: cancellationToken);
        }
        else
        {
            // use rclone sync
            await storageModel.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "sync",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--stats=1s",
                    "--stats-log-level=NOTICE",
                    "--use-json-log",
                ],
                this.processUtils,
                onStderr: onOutput,
                onProcessStarted: onProcessStarted,
                cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc/>
    public void DeleteStorageData(CloudStorageDTO storage, bool removeOnlyCloudData = false, Action<string, int, int>? onFileDelete = null)
    {
        ArgumentNullException.ThrowIfNull(storage);

        // remove complete storage directory
        var subpath = string.Empty;
        if (removeOnlyCloudData)
        {
            // only remove synched cloud data, keep config
            subpath = StaticConstants.CloudStorageCloudDataSubPath;
        }

        var storagePath = this.storage.FullPath(storage, subpath);
        if (!Directory.Exists(storagePath))
        {
            // directory is already deleted
            return;
        }

        var files = Directory.EnumerateFiles(storagePath, "*", SearchOption.AllDirectories).ToList();
        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];

            try
            {
                // try to delete the file
                File.Delete(file);
            }
            catch (Exception ex)
            {
                // keep deleting other files
                this.logger.LogError(ex, "Could not delete file '{File}' in cloud storage", file);
            }

            onFileDelete?.Invoke(file, files.Count, i);
        }

        // try to delete the directory itself
        if (Directory.Exists(storagePath))
        {
            try
            {
                Directory.Delete(storagePath, recursive: true);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Could not delete directory '{Directory}' in cloud storage", storagePath);
            }
        }
    }

    /// <inheritdoc/>
    public async Task SyncSingleStorageTask(Guid storageId)
    {
        var storage = await this.GetStorage(storageId);
        if (storage is null)
        {
            throw new InvalidOperationException(StaticConstants.CloudStorageStorageNotFoundExceptionKey);
        }

        var scheduler = await this.schedulerFactory.GetScheduler();
        await SyncStorageData.Schedule(scheduler, forSingleStorage: storage);
    }

    /// <inheritdoc/>
    public async Task ScanStorageForBooks(CloudStorageDTO storage, Action<int, int>? onFileScanned = null)
    {
        // download new data
        var localPath = this.storage.FullPath(storage, StaticConstants.CloudStorageCloudDataSubPath);

        var files = new List<string>();
        foreach (var extension in this.bookParserManager.SupportedFileEndings())
        {
            files.AddRange(Directory.EnumerateFiles(localPath, $"*.{extension}", SearchOption.AllDirectories));
        }

        int j = 0;
        foreach (var filePath in files)
        {
            var file = filePath.ToFile();

            var fileImported = await this.booksLogic.BookFileExists(file);
            var fileImportFailedBefore = await this.CloudFileImportFailed(storage, file);
            if (fileImported || fileImportFailedBefore)
            {
                // ignore file
                continue;
            }

            try
            {
                // import book
                await this.booksLogic.ImportBookAsync(file);
            }
            catch (Exception ex)
            {
                await this.AddCloudFileImportFail(storage, file.ToFileInfo(filePath), ex.Message);
            }

            onFileScanned?.Invoke(files.Count, j);
            j++;
        }
    }

    /// <inheritdoc/>
    public async Task ScanSingleStorageTask(Guid storageId)
    {
        var storage = await this.GetStorage(storageId);
        if (storage is null)
        {
            throw new InvalidOperationException(StaticConstants.CloudStorageStorageNotFoundExceptionKey);
        }

        var scheduler = await this.schedulerFactory.GetScheduler();
        await ScanForBooks.Schedule(scheduler, forSingleStorage: storage);
    }

    /// <inheritdoc/>
    public async Task DownloadStorageInitially(CloudStorageDTO storage, Action<string>? onOutput = null, Action<Process>? onProcessStarted = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var storageModel = await this.GetStorageModel(storage.Id);
        ArgumentNullException.ThrowIfNull(storageModel);

        var localPath = this.storage.FullPath(storage, StaticConstants.CloudStorageCloudDataSubPath);
        if (!Directory.Exists(localPath))
        {
            Directory.CreateDirectory(localPath);
        }

        var useExperimentalBisync = await this.dynamicSettings.GetAsync<bool>(StaticConstants.DynamicSettingCloudStorageExperimentalBisync);
        if (useExperimentalBisync.Value && StaticConstants.StoragesSupportRCloneBisync.Contains(storage.Type))
        {
            // use rclone bisync
            await storageModel.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "bisync",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--resync",
                    "--max-lock=2m",
                    "--stats=1s",
                    "--stats-log-level=NOTICE",
                    "--use-json-log",
                ],
                this.processUtils,
                onStderr: onOutput,
                onProcessStarted: onProcessStarted,
                cancellationToken: cancellationToken);
        }
        else
        {
            // use rclone sync
            await storageModel.ExecuteRCloneCommand(
                this.settings.CloudStorage.RClone,
                [
                    "sync",
                    $"\"{StaticConstants.CloudStorageRCloneConfigName}:{storage.CloudDirectory}\"",
                    $"\"{localPath}\"",
                    "--stats=1s",
                    "--stats-log-level=NOTICE",
                    "--use-json-log",
                ],
                this.processUtils,
                onStderr: onOutput,
                onProcessStarted: onProcessStarted,
                cancellationToken: cancellationToken);
        }

        await this.MarkStorageAsDownloaded(storage.Id);
    }

    /// <inheritdoc/>
    public async Task CleanStorageDirectory(CloudStorageDTO storage)
    {
        var storagePath = this.storage.FullPath(storage);

        // check if the directory is empty
        var isEmpty = !Directory.EnumerateFileSystemEntries(storagePath).Any();
        if (isEmpty)
        {
            this.logger.LogDebug("Directory is empty, nothing to do.");
            return;
        }

        // schedule task for removing cloud data
        var scheduler = await this.schedulerFactory.GetScheduler();
        await RemoveStorageData.Schedule(scheduler, storage, removeOnlyCloudData: true, options: TaskScheduleOptionsDTO.WaitPreset);
    }
}
