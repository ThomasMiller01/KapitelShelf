// <copyright file="OAuthTokensDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.CloudStorage;

/// <summary>
/// The OAuth tokens dto.
/// </summary>
public class OAuthTokensDTO
{
    /// <summary>
    /// Gets or sets the OAuth access token.
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Gets or sets the OAuth refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Gets or sets the expires in time.
    /// </summary>
    public int ExpiresIn { get; set; }
}
