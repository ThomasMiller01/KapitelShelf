﻿// <copyright file="CreateLocationDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Location;

/// <summary>
/// The create model for a location.
/// </summary>
public class CreateLocationDTO
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public LocationTypeDTO Type { get; set; }

    /// <summary>
    /// Gets or sets the url.
    /// </summary>
    public string? Url { get; set; }
}
