// <copyright file="MapperSettingsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the settings mapper.
/// </summary>
[TestFixture]
public class MapperSettingsTests
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
    /// Tests that SettingsModelToSettingsDto maps all properties and converts the value correctly.
    /// </summary>
    [Test]
    public void SettingsModelToSettingsDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new SettingsModel
        {
            Id = Guid.NewGuid(),
            Key = "cloudstorage.experimental",
            Value = "true",
            Type = SettingsValueType.TBoolean,
        };

        // execute
        var dto = this.testee.SettingsModelToSettingsDto<bool>(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Key, Is.EqualTo(model.Key));
            Assert.That(dto.Value, Is.True);
        });
    }

    /// <summary>
    /// Tests that SettingsDtoToSettingsModel maps all properties correctly when value is valid.
    /// </summary>
    [Test]
    public void SettingsDtoToSettingsModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new SettingsDTO<bool>
        {
            Id = Guid.NewGuid(),
            Key = "general.showHints",
            Value = true,
        };

        // execute
        var model = this.testee.SettingsDtoToSettingsModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(dto.Id));
            Assert.That(model.Key, Is.EqualTo(dto.Key));
            Assert.That(model.Value, Is.EqualTo(dto.Value.ToString()));
            Assert.That(model.Type, Is.EqualTo(SettingsValueType.TBoolean));
        });
    }

    /// <summary>
    /// Tests that SettingsDtoToSettingsModel throws InvalidCastException when Value is null.
    /// </summary>
    [Test]
    public void SettingsDtoToSettingsModel_Throws_WhenValueIsNull()
    {
        // setup
        var dto = new SettingsDTO<bool?>
        {
            Id = Guid.NewGuid(),
            Key = "invalid.setting",
            Value = null,
        };

        // execute & assert
        Assert.Throws<InvalidCastException>(() => this.testee.SettingsDtoToSettingsModel(dto));
    }

    /// <summary>
    /// Tests that SettingsValueTypeToSettingsValueTypeDto maps enums correctly.
    /// </summary>
    [Test]
    public void SettingsValueTypeToSettingsValueTypeDto_MapsEnumsCorrectly()
    {
        // execute
        var result = this.testee.SettingsValueTypeToSettingsValueTypeDto(SettingsValueType.TBoolean);

        // assert
        Assert.That(result.ToString(), Is.EqualTo(SettingsValueType.TBoolean.ToString()));
    }

    /// <summary>
    /// Tests that SettingsValueTypeDtoToSettingsValueType maps enums correctly.
    /// </summary>
    [Test]
    public void SettingsValueTypeDtoToSettingsValueType_MapsEnumsCorrectly()
    {
        // setup
        const SettingsValueTypeDTO dtoValue = SettingsValueTypeDTO.TBoolean;

        // execute
        var result = this.testee.SettingsValueTypeDtoToSettingsValueType(dtoValue);

        // assert
        Assert.That(result.ToString(), Is.EqualTo(dtoValue.ToString()));
    }
}
