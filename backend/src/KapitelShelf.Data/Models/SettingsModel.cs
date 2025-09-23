// <copyright file="SettingsModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The global KapitelShelf settings.
/// </summary>
public class SettingsModel
{
    /// <summary>
    /// Gets or sets the id.
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
    public SettingsValueType Type { get; set; }
}
