// <copyright file="CreateOrUpdateUserBookMetadataDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace KapitelShelf.Api.DTOs.User;

/// <summary>
/// The book dto.
/// </summary>
public class CreateOrUpdateUserBookMetadataDTO
{
    /// <summary>
    /// Gets or sets the rating.
    /// </summary>
    [Range(1, 10)]
    public int? Rating { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }
}
