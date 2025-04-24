// <copyright file="FileInfoModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The fileinfo model.
/// </summary>
public class FileInfoModel
{
    /// <summary>
    /// Gets or sets the fileinfo id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the mimetype.
    /// </summary>
    public string MimeType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sha256 checksum.
    /// </summary>
    public string Sha256 { get; set; } = null!;
}
