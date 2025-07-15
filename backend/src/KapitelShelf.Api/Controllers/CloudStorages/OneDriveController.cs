// <copyright file="OneDriveController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.CloudStorages;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers.CloudStorages;

/// <summary>
/// Initializes a new instance of the <see cref="OneDriveController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The tasks logic.</param>
[ApiController]
[Route("cloudstorage/onedrive")]
public partial class OneDriveController(ILogger<OneDriveController> logger, OneDriveLogic logic) : ControllerBase
{
    private readonly ILogger<OneDriveController> logger = logger;

    private readonly OneDriveLogic logic = logic;

    /// <summary>
    /// List all cloud storages.
    /// </summary>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<List<CloudStorageDTO>>> ListCloudStorages()
    {
        try
        {
            return Ok(await this.logic.ListCloudStorages());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting list of 'OneDrive' cloudstorages.");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Check if OneDrive cloud is configured.
    /// </summary>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("isconfigured")]
    public async Task<ActionResult<bool>> IsConfigured()
    {
        try
        {
            return Ok(await this.logic.IsConfigured());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting configured status of 'OneDrive' cloud");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Configure the OneDrive cloud.
    /// </summary>
    /// <param name="configureCloudDto">The configure cloud dto.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("configure")]
    public async Task<IActionResult> Configure(ConfigureCloudDTO configureCloudDto)
    {
        try
        {
            await this.logic.Configure(configureCloudDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error configuring 'OneDrive' cloud");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Redirect to the OneDrive OAuth url.
    /// </summary>
    /// <param name="redirectUrl">The url to redirect to after the OAuth flow finished.</param>
    /// <returns>An IActionResult.</returns>
    [HttpGet("oauth")]
    public async Task<ActionResult<string>> OAuth(string redirectUrl)
    {
        try
        {
            var isConfigured = await this.logic.IsConfigured();
            if (!isConfigured)
            {
                return StatusCode(403, "Cloud 'OneDrive' is not configured");
            }

            return await this.logic.GetOAuthUrl(redirectUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting OneDrive OAuth url");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Callback for the OneDrive OAuth flow.
    /// </summary>
    /// <param name="code">The OAuth callback code.</param>
    /// <param name="state">The OAuth callback state.</param>
    /// <returns>An IActionResult.</returns>
    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback(string code, string state)
    {
        try
        {
            var isConfigured = await this.logic.IsConfigured();
            if (!isConfigured)
            {
                return StatusCode(403, "Cloud 'OneDrive' is not configured");
            }

            var stateJson = Encoding.UTF8.GetString(Convert.FromBase64String(state));
            var stateInfo = JsonSerializer.Deserialize<OAuthStateInfoDTO>(stateJson);
            ArgumentNullException.ThrowIfNull(stateInfo);

            var tokens = await this.logic.GetOAuthTokensFromCode(code);
            await this.logic.GenerateRCloneConfig(tokens);

            // state contains the redirect url
            return Redirect(stateInfo.RedirectUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing OneDrive OAuth callback");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
