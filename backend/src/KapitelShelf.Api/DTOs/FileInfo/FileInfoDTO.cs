﻿// <copyright file="FileInfoDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.FileInfo;

/// <summary>
/// Gets or sets the fileinfo dto.
/// </summary>
public class FileInfoDTO
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

    /// <summary>
    /// Gets the filename.
    /// </summary>
    /// <returns>The filename.</returns>
    public string FileName => Path.GetFileName(this.FilePath);
}
