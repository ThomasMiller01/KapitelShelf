// <copyright file="CloudStorageDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Data.Models.CloudStorage;

namespace KapitelShelf.Api.DTOs.CloudStorage;

/// <summary>
/// The cloud storage dto.
/// </summary>
public class CloudStorageDTO
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
    /// Gets or sets a value indicating whether this storage needs re-authentication.
    /// </summary>
    public bool NeedsReAuthentication { get; set; } = true;

    /// <summary>
    /// Gets or sets the cloud directory to mirror.
    /// </summary>
    public string? CloudDirectory { get; set; }

    /// <summary>
    /// Gets or sets the cloud owner email.
    /// </summary>
    public string CloudOwnerEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the cloud owner name.
    /// </summary>
    public string CloudOwnerName { get; set; } = null!;
}
