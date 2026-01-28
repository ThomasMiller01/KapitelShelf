// <copyright file="IDynamicSettingsManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Settings;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for the dynamic settings manager.
/// </summary>
public interface IDynamicSettingsManager
{
    /// <summary>
    /// Initialize the settings on startup, if they dont exist yet.
    /// </summary>
    /// <returns>A task.</returns>
    Task InitializeOnStartup();

    /// <summary>
    /// Get the value for a setting by its key.
    /// </summary>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value.</returns>
    Task<SettingsDTO<T>> GetAsync<T>(string key);

    /// <summary>
    /// Get the value for a setting by its key.
    /// </summary>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value.</param>
    /// <returns>The new setting value.</returns>
    Task<SettingsDTO<T>> SetAsync<T>(string key, T value);
}
