// <copyright file="UserSettingDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.User;

/// <summary>
/// The user dto.
/// </summary>
public class UserSettingDTO
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the setting key.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the setting value.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets the setting value type.
    /// </summary>
    public UserSettingValueTypeDTO Type { get; set; }
}
