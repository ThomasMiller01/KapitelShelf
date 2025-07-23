// <copyright file="FailedCloudFileImportModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models.CloudStorage;

/// <summary>
/// The failed cloud file import model.
/// </summary>
public class FailedCloudFileImportModel
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the file info id..
    /// </summary>
    public Guid FileInfoId { get; set; }

    /// <summary>
    /// Gets or sets the file info.
    /// </summary>
    public FileInfoModel FileInfo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the storage id.
    /// </summary>
    public Guid StorageId { get; set; }

    /// <summary>
    /// Gets or sets the storage.
    /// </summary>
    public CloudStorageModel Storage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the import error message.
    /// </summary>
    public string ErrorMessaget { get; set; } = null!;
}
