// <copyright file="CloudStorageDirectoryDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.CloudStorage;

/// <summary>
/// The cloud storage directory dto.
/// </summary>
public class CloudStorageDirectoryDTO
{
    /// <summary>
    /// Gets or sets the directory id.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the path to the directory.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the directory name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last modified time.
    /// </summary>
    public DateTime ModifiedTime { get; set; }
}
