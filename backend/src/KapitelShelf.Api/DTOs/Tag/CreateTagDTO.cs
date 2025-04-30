// <copyright file="CreateTagDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Tag;

/// <summary>
/// The create dto for a tag.
/// </summary>
public class CreateTagDTO
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = null!;
}
