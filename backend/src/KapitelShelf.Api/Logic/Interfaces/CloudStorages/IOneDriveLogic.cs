// <copyright file="IOneDriveLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;

namespace KapitelShelf.Api.Logic.Interfaces.CloudStorages;

/// <summary>
/// The onedrive logic interface.
/// </summary>
public interface IOneDriveLogic
{
    /// <summary>
    /// Get the url for the OAuth flow of OneDrive.
    /// </summary>
    /// <param name="redirectUrl">The url to redirect to after the OAuth flow finished.</param>
    /// <returns>The OAuth url.</returns>
    Task<string> GetOAuthUrl(string redirectUrl);

    /// <summary>
    /// Gets the OAuth token from the OAuth code.
    /// </summary>
    /// <param name="code">The OAuth code.</param>
    /// <returns>The OAuth tokens.</returns>
    Task<OAuthTokensDTO> GetOAuthTokensFromCode(string code);

    /// <summary>
    /// Generate the rclone config.
    /// </summary>
    /// <param name="tokens">The OAuth tokens.</param>
    /// <returns>A task.</returns>
    Task GenerateRCloneConfig(OAuthTokensDTO tokens);
}
