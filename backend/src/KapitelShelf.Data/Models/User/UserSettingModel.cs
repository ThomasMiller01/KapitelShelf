// <copyright file="UserSettingModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models.User;

/// <summary>
/// The user setting model.
/// </summary>
public class UserSettingModel
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
    public UserSettingValueType Type { get; set; }

    /// <summary>
    /// Gets or sets the id of the user this setting belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user this setting belongs to.
    /// </summary>
    public UserModel User { get; set; } = null!;
}
