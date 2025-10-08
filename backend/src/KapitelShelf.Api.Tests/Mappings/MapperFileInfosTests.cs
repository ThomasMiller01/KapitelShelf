// <copyright file="MapperFileInfosTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the file info mapper.
/// </summary>
[TestFixture]
public class MapperFileInfosTests
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
    /// Tests that FileInfoModelToFileInfoDto correctly maps all properties.
    /// </summary>
    [Test]
    public void FileInfoModelToFileInfoDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new FileInfoModel
        {
            Id = Guid.NewGuid(),
            FilePath = "/books/cover.jpg",
            FileSizeBytes = 123456,
            MimeType = "image/jpeg",
            Sha256 = "abc123hash",
        };

        // execute
        var dto = this.testee.FileInfoModelToFileInfoDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.FilePath, Is.EqualTo(model.FilePath));
            Assert.That(dto.FileSizeBytes, Is.EqualTo(model.FileSizeBytes));
            Assert.That(dto.MimeType, Is.EqualTo(model.MimeType));
            Assert.That(dto.Sha256, Is.EqualTo(model.Sha256));
            Assert.That(dto.FileName, Is.EqualTo("cover.jpg"));
        });
    }

    /// <summary>
    /// Tests that FileInfoDtoToFileInfoModel correctly maps all properties and ignores FileName (derived).
    /// </summary>
    [Test]
    public void FileInfoDtoToFileInfoModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new FileInfoDTO
        {
            Id = Guid.NewGuid(),
            FilePath = "/downloads/example.pdf",
            FileSizeBytes = 98765,
            MimeType = "application/pdf",
            Sha256 = "hash987xyz",
        };

        // execute
        var model = this.testee.FileInfoDtoToFileInfoModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(dto.Id));
            Assert.That(model.FilePath, Is.EqualTo(dto.FilePath));
            Assert.That(model.FileSizeBytes, Is.EqualTo(dto.FileSizeBytes));
            Assert.That(model.MimeType, Is.EqualTo(dto.MimeType));
            Assert.That(model.Sha256, Is.EqualTo(dto.Sha256));
        });
    }
}
