// <copyright file="CloudConfigurationDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.CloudStorage;

/// <summary>
/// The cloud configuration dto.
/// </summary>
public class CloudConfigurationDTO
{
    /// <summary>
    /// Gets or sets the author id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the cloud storage name.
    /// </summary>
    public CloudTypeDTO Type { get; set; }

    /// <summary>
    /// Gets or sets the OAuth clientId.
    /// </summary>
    public string OAuthClientId { get; set; } = null!;
}
