// <copyright file="RCloneListJsonDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.CloudStorage.RClone;

/// <summary>
/// The rclone list json dto.
/// </summary>
public class RCloneListJsonDTO
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public string ID { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether this is a directory.
    /// </summary>
    public bool IsDir { get; set; }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    public double Size { get; set; }

    /// <summary>
    /// Gets or sets the mimetype.
    /// </summary>
    public string MimeType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last modified time.
    /// </summary>
    public string ModTime { get; set; } = null!;
}
