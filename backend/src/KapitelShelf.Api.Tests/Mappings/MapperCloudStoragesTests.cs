// <copyright file="MapperCloudStoragesTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.CloudStorage.RClone;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models.CloudStorage;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the cloud storages mapper.
/// </summary>
[TestFixture]
public class MapperCloudStoragesTests
{
    private Mapper testee;

    /// <summary>
    /// Sets up the mapper instance before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // setup
#pragma warning disable IDE0022 // Use expression body for method
        this.testee = new Mapper();
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <summary>
    /// Tests that CloudConfigurationModelToCloudConfigurationDto maps all fields correctly.
    /// </summary>
    [Test]
    public void CloudConfigurationModelToCloudConfigurationDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new CloudConfigurationModel
        {
            Id = Guid.NewGuid(),
            Type = CloudType.OneDrive,
            OAuthClientId = "client-123",
        };

        // execute
        var dto = this.testee.CloudConfigurationModelToCloudConfigurationDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Type, Is.EqualTo(CloudTypeDTO.OneDrive));
            Assert.That(dto.OAuthClientId, Is.EqualTo(model.OAuthClientId));
        });
    }

    /// <summary>
    /// Tests that CloudStorageModelToCloudStorageDto correctly maps fields and ignores RCloneConfig.
    /// </summary>
    [Test]
    public void CloudStorageModelToCloudStorageDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            Type = CloudType.OneDrive,
            NeedsReAuthentication = false,
            RCloneConfig = "/rclone/config/path",
            CloudDirectory = "/my/dir",
            CloudOwnerEmail = "owner@example.com",
            CloudOwnerName = "Owner Name",
            IsDownloaded = true,
        };

        // execute
        var dto = this.testee.CloudStorageModelToCloudStorageDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Type, Is.EqualTo(CloudTypeDTO.OneDrive));
            Assert.That(dto.NeedsReAuthentication, Is.EqualTo(model.NeedsReAuthentication));
            Assert.That(dto.CloudDirectory, Is.EqualTo(model.CloudDirectory));
            Assert.That(dto.CloudOwnerEmail, Is.EqualTo(model.CloudOwnerEmail));
            Assert.That(dto.CloudOwnerName, Is.EqualTo(model.CloudOwnerName));
            Assert.That(dto.IsDownloaded, Is.EqualTo(model.IsDownloaded));
        });
    }

    /// <summary>
    /// Tests that CloudStorageDtoToCloudStorageModel maps fields correctly and ignores RCloneConfig.
    /// </summary>
    [Test]
    public void CloudStorageDtoToCloudStorageModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            Type = CloudTypeDTO.OneDrive,
            NeedsReAuthentication = true,
            CloudDirectory = "/cloud/path",
            CloudOwnerEmail = "user@example.com",
            CloudOwnerName = "User Example",
            IsDownloaded = false,
        };

        // execute
        var model = this.testee.CloudStorageDtoToCloudStorageModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Type, Is.EqualTo(CloudType.OneDrive));
            Assert.That(model.NeedsReAuthentication, Is.EqualTo(dto.NeedsReAuthentication));
            Assert.That(model.CloudDirectory, Is.EqualTo(dto.CloudDirectory));
            Assert.That(model.CloudOwnerEmail, Is.EqualTo(dto.CloudOwnerEmail));
            Assert.That(model.CloudOwnerName, Is.EqualTo(dto.CloudOwnerName));
            Assert.That(model.IsDownloaded, Is.EqualTo(dto.IsDownloaded));
            Assert.That(model.RCloneConfig, Is.Null.Or.Empty); // ignored
        });
    }

    /// <summary>
    /// Tests that CloudTypeDtoToCloudType maps enum values correctly.
    /// </summary>
    [Test]
    public void CloudTypeDtoToCloudType_MapsCorrectly()
    {
        // execute
        var result = this.testee.CloudTypeDtoToCloudType(CloudTypeDTO.OneDrive);

        // assert
        Assert.That(result, Is.EqualTo(CloudType.OneDrive));
    }

    /// <summary>
    /// Tests that CloudTypeToCloudTypeDto maps enum values correctly.
    /// </summary>
    [Test]
    public void CloudTypeToCloudTypeDto_MapsCorrectly()
    {
        // execute
        var result = this.testee.CloudTypeToCloudTypeDto(CloudType.OneDrive);

        // assert
        Assert.That(result, Is.EqualTo(CloudTypeDTO.OneDrive));
    }

    /// <summary>
    /// Tests that RCloneListJsonDtoToCloudStorageDirectoryDtoCore maps all properties correctly.
    /// </summary>
    [Test]
    public void RCloneListJsonDtoToCloudStorageDirectoryDtoCore_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new RCloneListJsonDTO
        {
            ID = "abc123",
            Name = "Documents",
            Path = "/root/Documents",
            IsDir = true,
            MimeType = "inode/directory",
            Size = 0,
            ModTime = "2025-10-08T12:34:56Z",
        };

        // execute
        var result = this.testee.RCloneListJsonDtoToCloudStorageDirectoryDtoCore(dto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(dto.ID));
            Assert.That(result.Name, Is.EqualTo(dto.Name));
            Assert.That(result.Path, Is.EqualTo(dto.Path));
        });
    }

    /// <summary>
    /// Tests that RCloneListJsonDtoToCloudStorageDirectoryDto manually parses ModTime correctly.
    /// </summary>
    [Test]
    public void RCloneListJsonDtoToCloudStorageDirectoryDto_ParsesModifiedTimeCorrectly()
    {
        // setup
        var dto = new RCloneListJsonDTO
        {
            ID = "xyz987",
            Name = "Books",
            Path = "/root/Books",
            ModTime = "2025-10-08T18:30:00Z",
        };

        // execute
        var result = this.testee.RCloneListJsonDtoToCloudStorageDirectoryDto(dto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(dto.ID));
            Assert.That(result.Name, Is.EqualTo(dto.Name));
            Assert.That(result.Path, Is.EqualTo(dto.Path));
            Assert.That(result.ModifiedTime, Is.EqualTo(DateTime.Parse(dto.ModTime, CultureInfo.InvariantCulture).ToUniversalTime()));
        });
    }

    /// <summary>
    /// Tests that RCloneListJsonDtoToCloudStorageDirectoryDto sets ModifiedTime default when invalid.
    /// </summary>
    [Test]
    public void RCloneListJsonDtoToCloudStorageDirectoryDto_HandlesInvalidModifiedTime()
    {
        // setup
        var dto = new RCloneListJsonDTO
        {
            ID = "no-date",
            Name = "Temp",
            Path = "/tmp",
            ModTime = "not-a-date",
        };

        // execute
        var result = this.testee.RCloneListJsonDtoToCloudStorageDirectoryDto(dto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ModifiedTime, Is.EqualTo(default(DateTime)));
        });
    }
}
