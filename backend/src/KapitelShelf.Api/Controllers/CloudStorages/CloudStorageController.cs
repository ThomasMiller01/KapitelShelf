// <copyright file="CloudStorageController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using KapitelShelf.Api.Settings;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers.CloudStorages;

/// <summary>
/// Initializes a new instance of the <see cref="CloudStorageController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The tasks logic.</param>
[ApiController]
[Route("cloudstorage")]
public class CloudStorageController(ILogger<CloudStorageController> logger, ICloudStoragesLogic logic) : ControllerBase
{
    private readonly ILogger<CloudStorageController> logger = logger;

    private readonly ICloudStoragesLogic logic = logic;

    /// <summary>
    /// Check if cloud storage is configured.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("isconfigured")]
    public async Task<ActionResult<bool>> IsConfigured(CloudTypeDTO cloudType)
    {
        try
        {
            return Ok(await this.logic.IsConfigured(cloudType));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting configured status of '{CloudType}' cloud", cloudType);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Configure the cloud storage.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <param name="configureCloudDto">The configure cloud dto.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("configure")]
    public async Task<IActionResult> Configure(CloudTypeDTO cloudType, ConfigureCloudDTO configureCloudDto)
    {
        try
        {
            await this.logic.Configure(cloudType, configureCloudDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error configuring '{CloudType}' cloud", cloudType);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// List all cloud storages.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("storages")]
    public async Task<ActionResult<List<CloudStorageDTO>>> ListCloudStorages(CloudTypeDTO cloudType)
    {
        try
        {
            return Ok(await this.logic.ListCloudStorages(cloudType));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting list of '{CloudType}' cloudstorages.", cloudType);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get a cloud storage.
    /// </summary>
    /// <param name="storageId">The cloud storage id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("storages/{storageId}")]
    public async Task<ActionResult<CloudStorageDTO>> GetCloudStorage(Guid storageId)
    {
        try
        {
            var storage = await this.logic.GetStorage(storageId);
            if (storage is null)
            {
                return NotFound();
            }

            return Ok(storage);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching cloud storage with id '{StorageId}'.", storageId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete a cloud storage.
    /// </summary>
    /// <param name="storageId">The cloud storage id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete("storages/{storageId}")]
    public async Task<IActionResult> DeleteCloudStorage(Guid storageId)
    {
        try
        {
            var storage = await this.logic.DeleteCloudStorage(storageId);
            if (storage is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting cloud storage with id '{StorageId}'.", storageId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Sync a cloud storage.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("storages/{storageId}/sync")]
    public async Task<IActionResult> Sync(Guid storageId)
    {
        try
        {
            await this.logic.SyncSingleStorageTask(storageId);
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error synching '{StorageId}' storage", storageId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Scan a cloud storage for new books to import.
    /// </summary>
    /// <param name="storageId">The storage id.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("storages/{storageId}/scan")]
    public async Task<IActionResult> Scan(Guid storageId)
    {
        try
        {
            await this.logic.ScanSingleStorageTask(storageId);
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error scanning '{StorageId}' storage", storageId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// List all directories.
    /// </summary>
    /// <param name="storageId">The cloud storage id.</param>
    /// <param name="path">The start path of the directories to list.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("storages/{storageId}/list/directories")]
    public async Task<ActionResult<List<CloudStorageDirectoryDTO>>> ListCloudStorageDirectories(Guid storageId, string path = "")
    {
        try
        {
            return Ok(await this.logic.ListCloudStorageDirectories(storageId, path));
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.CloudStorageDirectoryNotFoundExceptionKey)
        {
            return Conflict(new { error = "The directory was not found." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting list of directories from 'OneDrive' cloudstorages.");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Configure the cloud directory.
    /// </summary>
    /// <param name="storageId">The cloud storage id.</param>
    /// <param name="directory">The cloud directory.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("storages/{storageId}/configure/directory")]
    public async Task<IActionResult> ConfigureDirectory(Guid storageId, string directory)
    {
        try
        {
            await this.logic.ConfigureDirectory(storageId, directory);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == StaticConstants.CloudStorageStorageNotFoundExceptionKey)
        {
            return NotFound(new { error = "The storage was not found." });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error configuring '{StorageId}' storage directory", storageId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
