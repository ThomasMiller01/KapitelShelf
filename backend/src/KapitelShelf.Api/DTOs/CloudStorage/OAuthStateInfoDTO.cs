// <copyright file="OAuthStateInfoDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.CloudStorage;

/// <summary>
/// The OAuth state info dto.
/// </summary>
public class OAuthStateInfoDTO
{
    /// <summary>
    /// Gets or sets the url to redirect to after the OAuth flow finished.
    /// </summary>
    public string RedirectUrl { get; set; } = null!;
}
