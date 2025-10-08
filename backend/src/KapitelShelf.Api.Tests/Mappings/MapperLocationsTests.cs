// <copyright file="MapperLocationsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the locations mapper.
/// </summary>
[TestFixture]
public class MapperLocationsTests
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
    /// Tests that CreateLocationDtoToLocationModel maps all fields correctly and ignores Id and FileInfo.
    /// </summary>
    [Test]
    public void CreateLocationDtoToLocationModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateLocationDTO
        {
            Type = LocationTypeDTO.KapitelShelf,
            Url = "https://kapitelshelf.local/books/123",
        };

        // execute
        var model = this.testee.CreateLocationDtoToLocationModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(Guid.Empty)); // ignored
            Assert.That(model.Type, Is.EqualTo(LocationType.KapitelShelf));
            Assert.That(model.Url, Is.EqualTo(dto.Url));
            Assert.That(model.FileInfo, Is.Null); // ignored
        });
    }

    /// <summary>
    /// Tests that LocationModelToLocationDto correctly maps all fields.
    /// </summary>
    [Test]
    public void LocationModelToLocationDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new LocationModel
        {
            Id = Guid.NewGuid(),
            Type = LocationType.Kindle,
            Url = "kindle://book/12345",
            FileInfo = new FileInfoModel
            {
                Id = Guid.NewGuid(),
                FilePath = "/files/kindle/book.azw3",
                FileSizeBytes = 1024,
                MimeType = "application/x-mobipocket-ebook",
                Sha256 = "sha256hash",
            },
        };

        // execute
        var dto = this.testee.LocationModelToLocationDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Type, Is.EqualTo(LocationTypeDTO.Kindle));
            Assert.That(dto.Url, Is.EqualTo(model.Url));
            Assert.That(dto.FileInfo, Is.Not.Null);
            Assert.That(dto.FileInfo!.FilePath, Is.EqualTo(model.FileInfo!.FilePath));
            Assert.That(dto.FileInfo.MimeType, Is.EqualTo(model.FileInfo.MimeType));
        });
    }

    /// <summary>
    /// Tests that LocationDtoToCreateLocationDto maps fields correctly and ignores Id and FileInfo.
    /// </summary>
    [Test]
    public void LocationDtoToCreateLocationDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new LocationDTO
        {
            Id = Guid.NewGuid(),
            Type = LocationTypeDTO.Onleihe,
            Url = "https://onleihe.library.de/item/555",
            FileInfo = new FileInfoDTO
            {
                Id = Guid.NewGuid(),
                FilePath = "/books/item.pdf",
                FileSizeBytes = 9999,
                MimeType = "application/pdf",
                Sha256 = "filehash",
            },
        };

        // execute
        var createDto = this.testee.LocationDtoToCreateLocationDto(dto);

        // assert
        Assert.That(createDto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createDto.Type, Is.EqualTo(dto.Type));
            Assert.That(createDto.Url, Is.EqualTo(dto.Url));
        });
    }

    /// <summary>
    /// Tests that LocationTypeToLocationTypeDto maps all enum values correctly.
    /// </summary>
    [Test]
    public void LocationTypeToLocationTypeDto_MapsAllEnumValuesCorrectly()
    {
        // execute & assert
        foreach (LocationType type in Enum.GetValues<LocationType>())
        {
            var result = this.testee.LocationTypeToLocationTypeDto(type);
            Assert.That(result.ToString(), Is.EqualTo(type.ToString()));
        }
    }

    /// <summary>
    /// Tests that LocationTypeDtoToLocationType maps all enum values correctly.
    /// </summary>
    [Test]
    public void LocationTypeDtoToLocationType_MapsAllEnumValuesCorrectly()
    {
        // execute & assert
        foreach (LocationTypeDTO dto in Enum.GetValues<LocationTypeDTO>())
        {
            var result = this.testee.LocationTypeDtoToLocationType(dto);
            Assert.That(result.ToString(), Is.EqualTo(dto.ToString()));
        }
    }
}
