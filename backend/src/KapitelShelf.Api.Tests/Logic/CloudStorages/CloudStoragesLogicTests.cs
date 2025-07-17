// <copyright file="CloudStoragesLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
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
    private ICloudStorage fileStorage;
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
        this.fileStorage = Substitute.For<ICloudStorage>();
        this.testee = new CloudStoragesLogic(this.dbContextFactory, this.mapper, this.settings, this.fileStorage);
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

        this.fileStorage.Received().DeleteDirectory(Arg.Any<CloudStorageDTO>());
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
}
