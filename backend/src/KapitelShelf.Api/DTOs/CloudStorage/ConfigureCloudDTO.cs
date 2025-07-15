// <copyright file="ConfigureCloudDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.CloudStorage;

/// <summary>
/// The configure cloud dto.
/// </summary>
public class ConfigureCloudDTO
{
    /// <summary>
    /// Gets or sets the OAuth client id.
    /// </summary>
    public string OAuthClientId { get; set; } = null!;
}
