// <copyright file="SettingsController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Resources;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="SettingsController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The settings logic.</param>
[ApiController]
[Route("settings")]
public class SettingsController(ILogger<SettingsController> logger, ISettingsLogic logic) : ControllerBase
{
    private readonly ILogger<SettingsController> logger = logger;

    private readonly ISettingsLogic logic = logic;

    /// <summary>
    /// Fetch all settings.
    /// </summary>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<List<SettingsDTO<object>>>> GetSettings()
    {
        try
        {
            var settings = await this.logic.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching settings");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a setting.
    /// </summary>
    /// <param name="settingId">The id of the setting to update.</param>
    /// <param name="value">The updated setting value.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{settingId}")]
    public async Task<IActionResult> UpdateSetting(Guid settingId, object value)
    {
        try
        {
            var updatedSetting = await this.logic.UpdateSettingAsync(settingId, value);
            if (updatedSetting is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.InvalidSettingValueType)
        {
            return Conflict(new { error = "The value type does not match the setting type." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating setting with Id: {SettingId}", settingId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
