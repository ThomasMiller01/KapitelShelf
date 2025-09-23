// <copyright file="SettingsDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Settings;

/// <summary>
/// The settings dto.
/// </summary>
/// <typeparam name="T">The setting value type.</typeparam>
public class SettingsDTO<T>
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
    public T Value { get; set; } = default!;
}
