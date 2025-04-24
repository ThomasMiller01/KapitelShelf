// <copyright file="LocationModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The location model.
/// </summary>
public class LocationModel
{
    /// <summary>
    /// Gets or sets the location id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public LocationType Type { get; set; }

    /// <summary>
    /// Gets or sets the url.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the fileinfo.
    /// </summary>
    public FileInfoModel? FileInfo { get; set; }
}
