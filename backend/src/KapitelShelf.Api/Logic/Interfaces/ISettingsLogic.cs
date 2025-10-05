// <copyright file="ISettingsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Settings;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The settings logic interface.
/// </summary>
public interface ISettingsLogic
{
    /// <summary>
    /// Get all settings.
    /// </summary>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    Task<List<SettingsDTO<object>>> GetSettingsAsync();

    /// <summary>
    /// Update a setting.
    /// </summary>
    /// <param name="settingId">The id of the setting to update.</param>
    /// <param name="value">The updated setting value.</param>
    /// <returns>A task.</returns>
    Task<SettingsDTO<object>?> UpdateSettingAsync(Guid settingId, object value);
}
