﻿// <copyright file="CloudStoragesLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Settings;
using KapitelShelf.Api.Utils;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Quartz;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic.CloudStorages;

/// <summary>
/// Unit tests for CloudStoragesLogic using a real PostgreSQL database.
/// </summary>
[TestFixture]
public class CloudStoragesLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private KapitelShelfSettings settings;
    private ISchedulerFactory schedulerFactory;
    private IProcessUtils processUtils;
    private CloudStoragesLogic testee;

    /// <summary>
    /// Setup database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        this.postgres = new PostgreSqlBuilder()
            .WithDatabase("kapitelshelf")
            .WithUsername("kapitelshelf")
            .WithPassword("kapitelshelf")
            .WithImage("postgres:16.8")
            .Build();

        await this.postgres.StartAsync();
    }

    /// <summary>
    /// Teardown database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeTearDown]
    public async Task Cleanup() => await this.postgres.DisposeAsync();

    /// <summary>
    /// Sets up the testee and fakes before each test.
    /// </summary>
    /// <returns>A task.</returns>
    [SetUp]
    public async Task SetUp()
    {
        this.dbOptions = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(this.postgres.GetConnectionString(), x => x.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            await context.Database.MigrateAsync();
            context.CloudStorages.RemoveRange(context.CloudStorages);
            context.CloudConfiguration.RemoveRange(context.CloudConfiguration);
            await context.SaveChangesAsync();
        }

        this.dbContextFactory = Substitute.For<IDbContextFactory<KapitelShelfDBContext>>();
        this.dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(new KapitelShelfDBContext(this.dbOptions)));

        this.mapper = Testhelper.CreateMapper();
        this.settings = new KapitelShelfSettings
        {
            CloudStorage = new CloudStorageSettings
            {
                RClone = "rclone",
            },
        };
        this.schedulerFactory = Substitute.For<ISchedulerFactory>();
        this.processUtils = Substitute.For<IProcessUtils>();
        this.testee = new CloudStoragesLogic(this.dbContextFactory, this.mapper, this.settings, this.schedulerFactory, this.processUtils);
    }

    /// <summary>
    /// Tests IsConfigured returns true if configuration exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task IsConfigured_ReturnsTrueIfConfigured()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;
        var modelType = CloudType.OneDrive;

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudConfiguration.Add(new CloudConfigurationModel
            {
                Type = modelType,
                OAuthClientId = "foo",
            });
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.IsConfigured(cloudType);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests IsConfigured returns false if configuration does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task IsConfigured_ReturnsFalseIfNotConfigured()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;

        // Execute
        var result = await this.testee.IsConfigured(cloudType);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests Configure creates a new configuration.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Configure_CreatesConfigurationIfNotExists()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;
        var modelType = CloudType.OneDrive;

        var dto = new ConfigureCloudDTO
        {
            OAuthClientId = "newclient",
        };

        // Execute
        await this.testee.Configure(cloudType, dto);

        // Assert
        using var context = new KapitelShelfDBContext(this.dbOptions);

        var config = context.CloudConfiguration.FirstOrDefault(x => x.Type == modelType);
        Assert.That(config, Is.Not.Null);
        Assert.That(config!.OAuthClientId, Is.EqualTo("newclient"));
    }

    /// <summary>
    /// Tests Configure updates existing configuration.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Configure_UpdatesConfigurationIfExists()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;
        var modelType = CloudType.OneDrive;

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudConfiguration.Add(new CloudConfigurationModel
            {
                Type = modelType,
                OAuthClientId = "old",
            });
            context.SaveChanges();
        }

        var dto = new ConfigureCloudDTO
        {
            OAuthClientId = "newid",
        };

        // Execute
        await this.testee.Configure(cloudType, dto);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var config = context.CloudConfiguration.FirstOrDefault(x => x.Type == modelType);
            Assert.That(config, Is.Not.Null);
            Assert.That(config!.OAuthClientId, Is.EqualTo("newid"));
        }
    }

    /// <summary>
    /// Tests Configure throws on null DTO.
    /// </summary>
    [Test]
    public void Configure_ThrowsIfDtoNull()
    {
        // Execute and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.testee.Configure(CloudTypeDTO.OneDrive, null!));
    }

    /// <summary>
    /// Tests Configure sets NeedsReAuthentication for storages of type.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Configure_SetsNeedsReAuthOnStorages()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;
        var modelType = CloudType.OneDrive;

        var storage = new CloudStorageModel
        {
            Type = modelType,
            NeedsReAuthentication = false,
            RCloneConfig = "rclone.conf",
            CloudOwnerName = "Name".Unique(),
            CloudOwnerEmail = "Email".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storage);
            context.SaveChanges();
        }

        var dto = new ConfigureCloudDTO
        {
            OAuthClientId = "z",
        };
        await this.testee.Configure(cloudType, dto);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var refreshed = context.CloudStorages.First(x => x.Id == storage.Id);
            Assert.That(refreshed.NeedsReAuthentication, Is.True);
        }
    }

    /// <summary>
    /// Tests ConfigureDirectory updates CloudDirectory if storage exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureDirectory_UpdatesIfStorageExists()
    {
        // Setup
        var id = Guid.NewGuid();
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(new CloudStorageModel
            {
                Id = id,
                RCloneConfig = "rclone.conf",
                CloudDirectory = "old",
                CloudOwnerName = "Name".Unique(),
                CloudOwnerEmail = "Email".Unique(),
            });
            context.SaveChanges();
        }

        // Execute
        await this.testee.ConfigureDirectory(id, "dir-new");

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var updated = context.CloudStorages.First(x => x.Id == id);
            Assert.That(updated.CloudDirectory, Is.EqualTo("dir-new"));
        }
    }

    /// <summary>
    /// Tests ConfigureDirectory throws if directory is null.
    /// </summary>
    [Test]
    public void ConfigureDirectory_ThrowsIfNullDirectory()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.testee.ConfigureDirectory(Guid.NewGuid(), null!));
    }

    /// <summary>
    /// Tests ConfigureDirectory throws if storage not found.
    /// </summary>
    [Test]
    public void ConfigureDirectory_ThrowsIfStorageNotFound()
    {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await this.testee.ConfigureDirectory(Guid.NewGuid(), "directory"));
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.CloudStorageStorageNotFoundExceptionKey));
    }

    /// <summary>
    /// Tests GetConfiguration returns mapped DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetConfiguration_ReturnsMappedConfig()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;
        var modelType = CloudType.OneDrive;
        var configModel = new CloudConfigurationModel
        {
            Type = modelType,
            OAuthClientId = "a",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudConfiguration.Add(configModel);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetConfiguration(cloudType);

        // Assert
        Assert.That(result.OAuthClientId, Is.EqualTo("a"));
    }

    /// <summary>
    /// Tests ListCloudStorages returns mapped DTOs for type.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ListCloudStorages_ReturnsMappedList()
    {
        // Setup
        var cloudType = CloudTypeDTO.OneDrive;
        var modelType = CloudType.OneDrive;
        var storage = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            Type = modelType,
            RCloneConfig = "rclone.conf",
            CloudOwnerName = "Name".Unique(),
            CloudOwnerEmail = "Email".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storage);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.ListCloudStorages(cloudType);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(storage.Id));
    }

    /// <summary>
    /// Tests ListCloudStorageDirectories returns empty if storage missing.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ListCloudStorageDirectories_ReturnsEmptyIfStorageMissing()
    {
        // Execute
        var result = await this.testee.ListCloudStorageDirectories(Guid.NewGuid(), "/");

        // Assert
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests DeleteCloudStorage deletes storage and calls fileStorage.DeleteDirectory.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteCloudStorage_DeletesStorageAndCallsFileStorage()
    {
        // Setup
        var id = Guid.NewGuid();
        var storage = new CloudStorageModel
        {
            Id = id,
            Type = CloudType.OneDrive,
            RCloneConfig = "rclone.conf",
            CloudOwnerName = "Name".Unique(),
            CloudOwnerEmail = "Email".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storage);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.DeleteCloudStorage(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.CloudStorages.Find(id), Is.Null);
        }
    }

    /// <summary>
    /// Tests DeleteCloudStorage returns null if storage does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteCloudStorage_ReturnsNullIfStorageNotFound()
    {
        // Execute
        var result = await this.testee.DeleteCloudStorage(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests GetStorageModel returns the correct storage by id.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStorageModel_ReturnsCorrectStorage()
    {
        // Setup
        var id = Guid.NewGuid();
        var storage = new CloudStorageModel
        {
            Id = id,
            RCloneConfig = "conf",
            CloudOwnerName = "A".Unique(),
            CloudOwnerEmail = "B".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storage);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetStorageModel(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(id));
    }

    /// <summary>
    /// Tests GetStorageModel returns null for non-existing id.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStorageModel_ReturnsNullIfNotFound()
    {
        // Execute
        var result = await this.testee.GetStorageModel(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests GetDownloadedStorageModels returns only downloaded storages.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetDownloadedStorageModels_ReturnsOnlyDownloaded()
    {
        // Setup
        var downloaded = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            IsDownloaded = true,
            CloudOwnerName = "X".Unique(),
            CloudOwnerEmail = "Y".Unique(),
            RCloneConfig = "rclone.conf",
        };
        var notDownloaded = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            IsDownloaded = false,
            CloudOwnerName = "P".Unique(),
            CloudOwnerEmail = "Q".Unique(),
            RCloneConfig = "rclone.conf",
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(downloaded);
            context.CloudStorages.Add(notDownloaded);
            context.SaveChanges();
        }

        // Execute
        var result = await this.testee.GetDownloadedStorageModels();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(downloaded.Id));
    }

    /// <summary>
    /// Tests GetStorage returns mapped DTO if storage exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStorage_ReturnsMappedDTOIfExists()
    {
        // Setup
        var id = Guid.NewGuid();
        var storage = new CloudStorageModel
        {
            Id = id,
            RCloneConfig = "rclone.conf",
            CloudOwnerName = "Map".Unique(),
            CloudOwnerEmail = "Test".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storage);
            context.SaveChanges();
        }

        // Execute
        var dto = await this.testee.GetStorage(id);

        // Assert
        Assert.That(dto, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dto!.Id, Is.EqualTo(id));
            Assert.That(dto.CloudOwnerName, Is.EqualTo(storage.CloudOwnerName));
        }
    }

    /// <summary>
    /// Tests GetStorage returns null if storage does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStorage_ReturnsNullIfStorageNotFound()
    {
        // Execute
        var dto = await this.testee.GetStorage(Guid.NewGuid());

        // Assert
        Assert.That(dto, Is.Null);
    }

    /// <summary>
    /// Tests MarkStorageAsDownloaded sets IsDownloaded to true.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MarkStorageAsDownloaded_SetsDownloadedTrue()
    {
        // Setup
        var id = Guid.NewGuid();
        var storage = new CloudStorageModel
        {
            Id = id,
            IsDownloaded = false,
            RCloneConfig = "mark.conf",
            CloudOwnerName = "Mark".Unique(),
            CloudOwnerEmail = "Download".Unique(),
        };
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storage);
            context.SaveChanges();
        }

        // Execute
        await this.testee.MarkStorageAsDownloaded(id);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var refreshed = context.CloudStorages.FirstOrDefault(x => x.Id == id);
            Assert.That(refreshed, Is.Not.Null);
            Assert.That(refreshed!.IsDownloaded, Is.True);
        }
    }

    /// <summary>
    /// Tests MarkStorageAsDownloaded does nothing if storage not found.
    /// </summary>
    [Test]
    public void MarkStorageAsDownloaded_NoExceptionIfStorageNotFound() =>
        Assert.DoesNotThrowAsync(async () => await this.testee.MarkStorageAsDownloaded(Guid.NewGuid()));

    /// <summary>
    /// Tests for AddCloudFileImportFail and CloudFileImportFailed.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddCloudFileImportFail_AddsEntry()
    {
        // Setup
        var storageModel = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            CloudOwnerName = "FailImport".Unique(),
            CloudOwnerEmail = "fail@test.com".Unique(),
            RCloneConfig = "test",
        };
        var fileInfoDto = new FileInfoDTO
        {
            FilePath = "/path/to/file.pdf",
            FileSizeBytes = 1234,
            MimeType = "application/pdf",
            Sha256 = "fakesha256".Unique(),
        };
        var errorMessage = "Test error message";

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storageModel);
            await context.SaveChangesAsync();
        }

        // Execute
        await this.testee.AddCloudFileImportFail(this.mapper.Map<CloudStorageDTO>(storageModel), fileInfoDto, errorMessage);

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var importFail = context.FailedCloudFileImports
                .Include(x => x.FileInfo)
                .FirstOrDefault(x => x.StorageId == storageModel.Id && x.FileInfo.Sha256 == fileInfoDto.Sha256);

            Assert.That(importFail, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(importFail!.ErrorMessaget, Is.EqualTo(errorMessage));
                Assert.That(importFail.FileInfo.FilePath, Is.EqualTo(fileInfoDto.FilePath));
            });
        }
    }

    /// <summary>
    /// Tests AddCloudFileImportFail throws ArgumentNullException for null storage.
    /// </summary>
    [Test]
    public void AddCloudFileImportFail_ThrowsIfStorageNull()
    {
        var fileInfoDto = new FileInfoDTO
        {
            FilePath = "/path",
            FileSizeBytes = 1,
            MimeType = "application/pdf",
            Sha256 = "sha",
        };

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.testee.AddCloudFileImportFail(null!, fileInfoDto, "error"));
    }

    /// <summary>
    /// Tests AddCloudFileImportFail throws ArgumentNullException for null fileInfo.
    /// </summary>
    [Test]
    public void AddCloudFileImportFail_ThrowsIfFileInfoNull()
    {
        var storageDto = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            CloudOwnerName = "fail".Unique(),
            CloudOwnerEmail = "fail".Unique(),
        };

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.testee.AddCloudFileImportFail(storageDto, null!, "error"));
    }

    /// <summary>
    /// Tests CloudFileImportFailed returns true if a failure exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CloudFileImportFailed_ReturnsTrueIfExists()
    {
        // Setup
        var storageModel = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            CloudOwnerName = "fail2".Unique(),
            CloudOwnerEmail = "fail2".Unique(),
            RCloneConfig = "test",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.CloudStorages.Add(storageModel);
            await context.SaveChangesAsync();
        }

        var file = BookParserHelper.BluePixelBytes.ToFile("TestFile");
        await this.testee.AddCloudFileImportFail(this.mapper.Map<CloudStorageDTO>(storageModel), file.ToFileInfo("Test"), "fail");

        // Execute
        var result = await this.testee.CloudFileImportFailed(this.mapper.Map<CloudStorageDTO>(storageModel), file);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests CloudFileImportFailed returns false if no fail exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CloudFileImportFailed_ReturnsFalseIfNotExists()
    {
        // Setup
        var storageDto = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            CloudOwnerName = "fail3".Unique(),
            CloudOwnerEmail = "fail3".Unique(),
        };

        var file = BookParserHelper.BluePixelBytes.ToFile("TestFile");

        // Execute
        var result = await this.testee.CloudFileImportFailed(storageDto, file);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests CloudFileImportFailed throws ArgumentNullException if storage is null.
    /// </summary>
    [Test]
    public void CloudFileImportFailed_ThrowsIfStorageNull()
    {
        var file = BookParserHelper.BluePixelBytes.ToFile("TestFile");

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.testee.CloudFileImportFailed(null!, file));
    }

    /// <summary>
    /// Tests CloudFileImportFailed throws ArgumentNullException if file is null.
    /// </summary>
    [Test]
    public void CloudFileImportFailed_ThrowsIfFileNull()
    {
        var storageDto = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            CloudOwnerName = "fail4".Unique(),
            CloudOwnerEmail = "fail4".Unique(),
        };

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.testee.CloudFileImportFailed(storageDto, null!));
    }
}
