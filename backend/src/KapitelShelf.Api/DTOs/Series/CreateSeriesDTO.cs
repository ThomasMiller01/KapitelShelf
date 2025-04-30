// <copyright file="CreateSeriesDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Series;

/// <summary>
/// The create dto for a series.
/// </summary>
public class CreateSeriesDTO
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;
}
