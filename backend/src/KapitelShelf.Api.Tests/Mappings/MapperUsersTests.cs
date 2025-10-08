// <copyright file="MapperUsersTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.User;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models.User;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the users mapper.
/// </summary>
[TestFixture]
public class MapperUsersTests
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
    /// Tests that UserModelToUserDto maps all properties correctly and ignores collections.
    /// </summary>
    [Test]
    public void UserModelToUserDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "Thomas",
            Image = ProfileImageType.CheeryChino,
            Color = "#3B5B92",
            Books = [],
            VisitedBooks = [],
        };

        // execute
        var dto = this.testee.UserModelToUserDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Username, Is.EqualTo(model.Username));
            Assert.That(dto.Image.ToString(), Is.EqualTo(model.Image.ToString()));
            Assert.That(dto.Color, Is.EqualTo(model.Color));
        });
    }

    /// <summary>
    /// Tests that CreateUserDtoToUserModel maps all properties correctly and ignores Id and collections.
    /// </summary>
    [Test]
    public void CreateUserDtoToUserModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new CreateUserDTO
        {
            Username = "NewUser",
            Image = ProfileImageTypeDTO.LittleStinky,
            Color = "#A45D5D",
        };

        // execute
        var model = this.testee.CreateUserDtoToUserModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(Guid.Empty)); // ignored
            Assert.That(model.Username, Is.EqualTo(dto.Username));
            Assert.That(model.Image.ToString(), Is.EqualTo(dto.Image.ToString()));
            Assert.That(model.Color, Is.EqualTo(dto.Color));
            Assert.That(model.Books, Is.Empty);
            Assert.That(model.VisitedBooks, Is.Empty);
        });
    }

    /// <summary>
    /// Tests that UserSettingModelToUserSettingDto maps all properties correctly and ignores UserId and User.
    /// </summary>
    [Test]
    public void UserSettingModelToUserSettingDto_MapsAllPropertiesCorrectly()
    {
        // setup
        var model = new UserSettingModel
        {
            Id = Guid.NewGuid(),
            Key = "theme.mode",
            Value = "dark",
            Type = UserSettingValueType.TString,
            UserId = Guid.NewGuid(),
            User = new UserModel(),
        };

        // execute
        var dto = this.testee.UserSettingModelToUserSettingDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Key, Is.EqualTo(model.Key));
            Assert.That(dto.Value, Is.EqualTo(model.Value));
            Assert.That(dto.Type.ToString(), Is.EqualTo(model.Type.ToString()));
        });
    }

    /// <summary>
    /// Tests that UserSettingDtoToUserSettingModel maps all properties correctly and ignores UserId and User.
    /// </summary>
    [Test]
    public void UserSettingDtoToUserSettingModel_MapsAllPropertiesCorrectly()
    {
        // setup
        var dto = new UserSettingDTO
        {
            Id = Guid.NewGuid(),
            Key = "notifications.enabled",
            Value = "true",
            Type = UserSettingValueTypeDTO.TBoolean,
        };

        // execute
        var model = this.testee.UserSettingDtoToUserSettingModel(dto);

        // assert
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(dto.Id));
            Assert.That(model.Key, Is.EqualTo(dto.Key));
            Assert.That(model.Value, Is.EqualTo(dto.Value));
            Assert.That(model.Type.ToString(), Is.EqualTo(dto.Type.ToString()));
            Assert.That(model.UserId, Is.EqualTo(Guid.Empty)); // ignored
            Assert.That(model.User, Is.Null); // ignored
        });
    }

    /// <summary>
    /// Tests that ProfileImageTypeToProfileImageTypeDto maps all enum values correctly.
    /// </summary>
    /// <param name="value">The value.</param>
    [TestCaseSource(nameof(GetProfileImageTypes))]
    public void ProfileImageTypeToProfileImageTypeDto_MapsEnumsCorrectly(ProfileImageType value)
    {
        // execute
        var dto = this.testee.ProfileImageTypeToProfileImageTypeDto(value);

        // assert
        Assert.That(dto.ToString(), Is.EqualTo(value.ToString()));
    }

    /// <summary>
    /// Tests that ProfileImageTypeDtoToProfileImageType maps all enum values correctly.
    /// </summary>
    /// <param name="dtoValue">The dto value.</param>
    [TestCaseSource(nameof(GetProfileImageTypeDtos))]
    public void ProfileImageTypeDtoToProfileImageType_MapsEnumsCorrectly(ProfileImageTypeDTO dtoValue)
    {
        // execute
        var modelEnum = this.testee.ProfileImageTypeDtoToProfileImageType(dtoValue);

        // assert
        Assert.That(modelEnum.ToString(), Is.EqualTo(dtoValue.ToString()));
    }

    /// <summary>
    /// Tests that UserSettingValueTypeToUserSettingValueTypeDto maps all enum values correctly.
    /// </summary>
    /// <param name="value">The value.</param>
    [TestCaseSource(nameof(GetUserSettingValueTypes))]
    public void UserSettingValueTypeToUserSettingValueTypeDto_MapsEnumsCorrectly(UserSettingValueType value)
    {
        // execute
        var dto = this.testee.UserSettingValueTypeToUserSettingValueTypeDto(value);

        // assert
        Assert.That(dto.ToString(), Is.EqualTo(value.ToString()));
    }

    /// <summary>
    /// Tests that UserSettingValueTypeDtoToUserSettingValueType maps all enum values correctly.
    /// </summary>
    /// <param name="dtoValue">The dto value.</param>
    [TestCaseSource(nameof(GetUserSettingValueTypeDtos))]
    public void UserSettingValueTypeDtoToUserSettingValueType_MapsEnumsCorrectly(UserSettingValueTypeDTO dtoValue)
    {
        // execute
        var modelEnum = this.testee.UserSettingValueTypeDtoToUserSettingValueType(dtoValue);

        // assert
        Assert.That(modelEnum.ToString(), Is.EqualTo(dtoValue.ToString()));
    }

    // helper sources
    private static IEnumerable<ProfileImageType> GetProfileImageTypes()
        => Enum.GetValues<ProfileImageType>().Cast<ProfileImageType>();

    private static IEnumerable<ProfileImageTypeDTO> GetProfileImageTypeDtos()
        => Enum.GetValues<ProfileImageTypeDTO>().Cast<ProfileImageTypeDTO>();

    private static IEnumerable<UserSettingValueType> GetUserSettingValueTypes()
        => Enum.GetValues<UserSettingValueType>().Cast<UserSettingValueType>();

    private static IEnumerable<UserSettingValueTypeDTO> GetUserSettingValueTypeDtos()
        => Enum.GetValues<UserSettingValueTypeDTO>().Cast<UserSettingValueTypeDTO>();
}
