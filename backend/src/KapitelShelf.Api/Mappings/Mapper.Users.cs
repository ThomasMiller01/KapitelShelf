// <copyright file="Mapper.Users.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.User;
using KapitelShelf.Data.Models.User;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The users mappers.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a profile image type to a profile image type dto.
    /// </summary>
    /// <param name="type">The profile image type.</param>
    /// <returns>The profile image type dto.</returns>
    public partial ProfileImageTypeDTO ProfileImageTypeToProfileImageTypeDto(ProfileImageType type);

    /// <summary>
    /// Map a profile image type dto to a profile image type.
    /// </summary>
    /// <param name="dto">The profile image type dto.</param>
    /// <returns>The profile image type.</returns>
    public partial ProfileImageType ProfileImageTypeDtoToProfileImageType(ProfileImageTypeDTO dto);

    /// <summary>
    /// Map a user model to a user dto.
    /// </summary>
    /// <param name="model">The user model.</param>
    /// <returns>The user dto.</returns>
    [MapperIgnoreSource(nameof(UserModel.Books))]
    [MapperIgnoreSource(nameof(UserModel.VisitedBooks))]
    public partial UserDTO UserModelToUserDto(UserModel model);

    /// <summary>
    /// Map a create user dto to a user model.
    /// </summary>
    /// <param name="dto">The create user dto.</param>
    /// <returns>The user model.</returns>
    [MapperIgnoreTarget(nameof(UserModel.Id))]
    [MapperIgnoreTarget(nameof(UserModel.Books))]
    [MapperIgnoreTarget(nameof(UserModel.VisitedBooks))]
    public partial UserModel CreateUserDtoToUserModel(CreateUserDTO dto);

    /// <summary>
    /// Map a user setting model to a user setting dto.
    /// </summary>
    /// <param name="model">The user setting model.</param>
    /// <returns>The user setting dto.</returns>
    [MapperIgnoreSource(nameof(UserSettingModel.UserId))]
    [MapperIgnoreSource(nameof(UserSettingModel.User))]
    public partial UserSettingDTO UserSettingModelToUserSettingDto(UserSettingModel model);

    /// <summary>
    /// Map a user setting dto to a user setting model.
    /// </summary>
    /// <param name="dto">The user setting dto.</param>
    /// <returns>The suer setting model.</returns>
    [MapperIgnoreTarget(nameof(UserSettingModel.UserId))]
    [MapperIgnoreTarget(nameof(UserSettingModel.User))]
    public partial UserSettingModel UserSettingDtoToUserSettingModel(UserSettingDTO dto);

    /// <summary>
    /// Map a user setting value type to a user setting value type dto.
    /// </summary>
    /// <param name="type">The user setting value type.</param>
    /// <returns>The user setting value type dto.</returns>
    public partial UserSettingValueTypeDTO UserSettingValueTypeToUserSettingValueTypeDto(UserSettingValueType type);

    /// <summary>
    /// Map a user setting value type dto to a user setting value type.
    /// </summary>
    /// <param name="dto">The user setting value type dto.</param>
    /// <returns>The user setting value type.</returns>
    public partial UserSettingValueType UserSettingValueTypeDtoToUserSettingValueType(UserSettingValueTypeDTO dto);

    /// <summary>
    /// Map a user book metadata model to a user book metadata dto.
    /// </summary>
    /// <param name="model">The user book metadata model.</param>
    /// <returns>The user book metadata dto.</returns>
    [MapperIgnoreSource(nameof(UserBookMetadataModel.BookId))]
    [MapperIgnoreSource(nameof(UserBookMetadataModel.Book))]
    [MapperIgnoreSource(nameof(UserBookMetadataModel.User))]
    public partial UserBookMetadataDTO UserBookMetadataModelToUserBookMetadataDto(UserBookMetadataModel model);
}
