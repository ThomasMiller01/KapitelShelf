// <copyright file="OneDriveController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers.CloudStorages;

/// <summary>
/// Initializes a new instance of the <see cref="OneDriveController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="baseLogic">The base logic.</param>
/// <param name="logic">The tasks logic.</param>
[ApiController]
[Route("cloudstorage/onedrive")]
public class OneDriveController(ILogger<OneDriveController> logger, ICloudStoragesLogic baseLogic, IOneDriveLogic logic) : ControllerBase
{
    private readonly ILogger<OneDriveController> logger = logger;

    private readonly ICloudStoragesLogic baseLogic = baseLogic;

    private readonly IOneDriveLogic logic = logic;

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
            var isConfigured = await this.baseLogic.IsConfigured(CloudTypeDTO.OneDrive);
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
            var isConfigured = await this.baseLogic.IsConfigured(CloudTypeDTO.OneDrive);
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
