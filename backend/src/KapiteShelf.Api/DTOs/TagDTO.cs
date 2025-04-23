// <copyright file="TagDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs;

/// <summary>
/// The tag dto.
/// </summary>
public class TagDTO
{
    /// <summary>
    /// Gets or sets the tag id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;
}
