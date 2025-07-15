// <copyright file="CloudConfigurationModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models.CloudStorage;

/// <summary>
/// The cloud configuration model.
/// </summary>
public class CloudConfigurationModel
{
    /// <summary>
    /// Gets or sets the author id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the cloud storage name.
    /// </summary>
    public CloudType Type { get; set; }

    /// <summary>
    /// Gets or sets the OAuth clientId.
    /// </summary>
    public string OAuthClientId { get; set; } = null!;
}
